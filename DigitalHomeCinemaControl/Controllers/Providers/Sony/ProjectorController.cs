/*
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
 * FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for
 * more details.
 *
 */

// TODO: need to decide to remove model # display in favor of color temp
// alternatively we could combine color space / temp into a single item
// ex. "BT2020 DCI-P3" "BT709 D65"
namespace DigitalHomeCinemaControl.Controllers.Providers.Sony
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Timers;
    using DigitalHomeCinemaControl.Components;
    using DigitalHomeCinemaControl.Controllers.Base;
    using DigitalHomeCinemaControl.Controllers.Providers.Sony.Sdcp;
    using DigitalHomeCinemaControl.Controllers.Routing;

    public sealed class ProjectorController : DisplayController, IRoutingDestination, ISupportCustomNames, IDisposable
    {

        #region Members

        private const string MODEL = "Model";
        private const string PRESET = "Calibration Preset";
        private const string COLORSPACE = "Color Space";
        private const string ASPECT = "Aspect Ratio";
        private const string GAMMA = "Gamma";
        private const string MOTIONFLOW = "Motion Flow";
        private const string REALITYCREATION = "Reality Creation";
        private const string COLORTEMP = "Color Temperature";

        private const int DEFAULT_PORT = 53484;
        private const int COMMAND_DELAY = 100;
        private const int IDLE_INTERVAL = 5000;
        private const int INTERVAL = 2000;

        // use lock(lockObject) to control access to the SdcpClient
        // aquire the lock before calling client.SendRequest()
        // and call client.Close() before releasing the lock
        private readonly object lockObject = new object();
        private IDictionary<string, Type> actions;
        private Timer timer;
        private bool running;
        private SdcpClient client;
        private bool disposed = false;

        #endregion

        #region Constructor

        public ProjectorController()
            : base()
        {
            this.Port = DEFAULT_PORT;
            this.CommandDelay = COMMAND_DELAY;

            this.CustomGamma = new NameValueCollection();
            this.CustomColorSpace = new NameValueCollection();
            this.CustomColorTemp = new NameValueCollection();

            this.DisplayType = DisplayType.Projector;
            this.LampStatus = LampStatus.Unknown;

            this.DataSource.Add(new BindingItem<string>(MODEL));
            this.DataSource.Add(new BindingItem<string>(PRESET));
            this.DataSource.Add(new BindingItem<string>(COLORSPACE));
            this.DataSource.Add(new BindingItem<string>(ASPECT));
            this.DataSource.Add(new BindingItem<string>(GAMMA));
            this.DataSource.Add(new BindingItem<string>(MOTIONFLOW));
            this.DataSource.Add(new BindingItem<string>(REALITYCREATION));

            this.actions = new Dictionary<string, Type> {
                { nameof(CalibrationPreset), typeof(CalibrationPreset) },
                { nameof(LampControl), typeof(LampControl) },
                { nameof(GammaCorrection), typeof(GammaCorrection) },
                { nameof(ColorSpace), typeof(ColorSpace) },
                { nameof(MotionFlow), typeof(MotionFlow) },
                { nameof(HDR), typeof(HDR) },
                { nameof(AspectRatio), typeof(AspectRatio) },
                { nameof(Input), typeof(Input) }
            };

            this.CustomNameTypes = new Dictionary<string, Type> {
                { COLORSPACE, typeof(ColorSpace) },
                { GAMMA, typeof(GammaCorrection) },
                { COLORTEMP, typeof(ColorTemp) }
            };

        }

        #endregion

        #region Methods

        public override void Connect()
        {
            this.disposed = false;
            this.running = true;
            this.timer = new Timer() {
                Interval = INTERVAL,
                AutoReset = false // important! each update could take longer than the interval and we don't want overlap
            };
            this.timer.Elapsed += TimerElapsed;

            bool connected;
            string model;

            lock (this.lockObject) {
                connected = TryGetItem(CommandItem.ModelName, out model);
                this.client?.Close();
            }

            if (connected) {
                this.ControllerStatus = ControllerStatus.Ok;
                UpdateDataSource<string>(MODEL, model);
                OnConnected();
            } else {
                this.ControllerStatus = ControllerStatus.Error;
                OnError(string.Format(CultureInfo.InvariantCulture, Properties.Resources.FMT_NETWORK_TIMEOUT, "projector"));
            }

            this.timer?.Start();
        }
    
        public override void Disconnect()
        {
            this.running = false;

            if ((this.timer != null) && this.timer.Enabled) {
                this.timer.Stop();
            }

            try {
                Dispose(true);
            } catch {
            } finally {
                OnDisconnected();
            }
        }

        public string RouteAction(string action, object args)
        {
            if (string.IsNullOrEmpty(action)) {
                return string.Format(CultureInfo.InvariantCulture, Properties.Resources.FMT_SDCP_ERROR, Properties.Resources.MSG_INVALID_ACTION);
            }

            if (Enum.TryParse(action, out CommandItem item)) {
                SdcpRequest request = new SdcpRequest("SONY", RequestType.Set) {
                    Item = item
                };
                request.SetData(args);

                SdcpResponse response;
                lock (this.lockObject) {
                    response = TrySendRequest(request);
                    this.client.Close();
                }

                if (response.Result == SdcpResult.OK) {
                    return Properties.Resources.MSG_SDCP_OK;
                } else {
                    return string.Format(CultureInfo.InvariantCulture, Properties.Resources.FMT_SDCP_ERROR, response.Error.ToString("G"));
                }
            } else {
                return string.Format(CultureInfo.InvariantCulture, Properties.Resources.FMT_SDCP_ERROR, Properties.Resources.MSG_INVALID_ACTION);
            }
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (!this.running) { return; }

            try {
                lock (this.lockObject) {
                    UpdateState();
                    this.client.Close();
                }
            } catch {
                this.ControllerStatus = ControllerStatus.Error;
                OnError(string.Format(CultureInfo.InvariantCulture, Properties.Resources.FMT_NETWORK_TIMEOUT, "projector"));
            } finally {
                if ((this.timer != null) && !this.timer.Enabled) {
                    if (this.running) {
                        this.timer.Start();
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private SdcpResponse TrySendRequest(SdcpRequest request)
        {
            try {
                return SendRequest(request);
            } catch {
                return new SdcpResponse(SdcpResult.ERROR, SdcpError.NetworkErrorTimeout);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private SdcpResponse SendRequest(SdcpRequest request)
        {
            // calling thread should have already aquired the lock
            // but grab it again to be safe
            lock (this.lockObject) {
                if ((this.client == null) || this.client.Closed) {
                    this.client = new SdcpClient(this.Host, this.Port) {
                        CommandDelay = this.CommandDelay,
                    };
                }
                if (!this.client.Connected) {
                    this.client.Connect();
                }

                var result = this.client.Send(request);

                return result;
            }
        }

        private void GetItemAndUpdate<T>(string name, CommandItem item, NameValueCollection customValues = null)
            where T : Enum
        {
            SdcpRequest request = new SdcpRequest("SONY", RequestType.Get) {
                Item = item,
            };
            SdcpResponse response = SendRequest(request);
            if (response.Result == SdcpResult.OK) {
                if (customValues == null) {
                    UpdateDataSource(name, ((T)(object)response.DataValue).GetDescription());
                } else {
                    UpdateDataSource(name, ((T)(object)response.DataValue).GetDescription(customValues));
                }
            } else if (response.Error == SdcpError.NotApplicableItem) {
                UpdateDataSource(name, "N/A");
            }
        }

        private T GetItem<T>(CommandItem item) 
            where T : Enum
        {
            SdcpRequest request = new SdcpRequest("SONY", RequestType.Get) {
                Item = item,
            };
            SdcpResponse response = SendRequest(request);
            if (response.Result == SdcpResult.OK) {
                return (T)(object)response.DataValue;
            }

            return default;
        }

        private bool TryGetItem<T>(CommandItem item, out T result)
            where T : Enum
        {
            try {
                SdcpRequest request = new SdcpRequest("SONY", RequestType.Get) {
                    Item = item,
                };
                SdcpResponse response = SendRequest(request);
                if (response.Result == SdcpResult.OK) {
                    result = (T)(object)response.DataValue;
                    return true;
                } else {
                    result = default;
                    return false;
                }
            } catch { }

            result = default;
            return false;
        }

        private bool TryGetItem(CommandItem item, out string result)
        {
            try {
                SdcpRequest request = new SdcpRequest("SONY", RequestType.Get) {
                    Item = item,
                };
                SdcpResponse response = SendRequest(request);
                if (response.Result == SdcpResult.OK) {
                    result = Encoding.ASCII.GetString(response.Data, 0, response.Data.Length).Replace("\0", "");
                    return true;
                } else {
                    result = null;
                    return false;
                }
            } catch { }

            result = null;
            return false;
        }

        private int GetItem(CommandItem item)
        {
            SdcpRequest request = new SdcpRequest("SONY", RequestType.Get) {
                Item = item,
            };
            SdcpResponse response = SendRequest(request);
            if (response.Result == SdcpResult.OK) {
                return response.DataValue;
            }

            return -2;
        }

        private void UpdateState()
        {
            if (TryGetItem<StatusPower>(CommandItem.StatusPower, out StatusPower status)) {
                this.ControllerStatus = status.ToControllerStatus();
            } else {
                this.ControllerStatus = ControllerStatus.Error;
            }

            // If projector is in standby mode, slow down polling inteval
            // and don't bother requesting any more items
            if ((this.ControllerStatus == ControllerStatus.Standby) ||
                (this.ControllerStatus == ControllerStatus.Error)) {

                this.timer.Interval = IDLE_INTERVAL;
                this.LampStatus = LampStatus.Off;

                UpdateDataSource<string>(PRESET, string.Empty);
                UpdateDataSource<string>(COLORSPACE, string.Empty);
                UpdateDataSource<string>(ASPECT, string.Empty);
                UpdateDataSource<string>(GAMMA, string.Empty);
                UpdateDataSource<string>(MOTIONFLOW, string.Empty);
                UpdateDataSource<string>(REALITYCREATION, string.Empty);

                this.timer.Start();
                return;
            }

            // projector not in standby mode
            // do a full update and reset the interval for more frequent updates
            this.timer.Interval = INTERVAL;

            // update Properties
            var lamp = GetItem<LampControl>(CommandItem.LampControl);
            this.LampStatus = lamp.ToLampStatus();

            int lampTimer = GetItem(CommandItem.LampTimer);
            if (lampTimer >= 0) {
                this.LampTimer = lampTimer;
            }

            // update DataSource
            GetItemAndUpdate<CalibrationPreset>(PRESET, CommandItem.CalibrationPreset);
            GetItemAndUpdate<ColorSpace>(COLORSPACE, CommandItem.ColorSpace, this.CustomColorSpace);
            GetItemAndUpdate<AspectRatio>(ASPECT, CommandItem.AspectRatio);
            GetItemAndUpdate<GammaCorrection>(GAMMA, CommandItem.GammaCorrection, this.CustomGamma);
            GetItemAndUpdate<MotionFlow>(MOTIONFLOW, CommandItem.MotionFlow);

            var rc = GetItem<RealityCreation>(CommandItem.RealityCreation);
            if (rc == RealityCreation.On) {
                var rcd = GetItem<RealityCreationDatabase>(CommandItem.RealityCreationDatabase);
                if (rcd == RealityCreationDatabase.Unknown) {
                    UpdateDataSource(REALITYCREATION, rc.GetDescription());
                } else {
                    UpdateDataSource(REALITYCREATION, string.Format(CultureInfo.InvariantCulture, "{0} - {1}", rc.GetDescription(), rcd.GetDescription()));
                }
            } else {
                UpdateDataSource(REALITYCREATION, rc.GetDescription());
            }

            this.timer.Start();
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed) {
                if (disposing) {
                    this.client?.Close();
                    this.timer?.Dispose();
                }

                this.disposed = true;

                this.client = null;
                this.timer = null;
            }
        }

        ~ProjectorController()
        {
            Dispose(false);
        }

#pragma warning disable CA1063 // Implement IDisposable Correctly
        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
#pragma warning restore CA1063

        #endregion

        #region Properties

        public string Name
        {
            get { return "Projector"; }
        }

        public IDictionary<string, Type> Actions
        {
            get { return this.actions; }
        }

        public int CommandDelay
        {
            get { return GetSetting<int>(); }
            set { Setting<int>(value); }
        }

        public NameValueCollection CustomGamma
        {
            get { return GetSetting<NameValueCollection>(); }
            private set { Setting<NameValueCollection>(value); }
        }

        public NameValueCollection CustomColorSpace
        {
            get { return GetSetting<NameValueCollection>(); }
            private set { Setting<NameValueCollection>(value); }
        }

        public NameValueCollection CustomColorTemp
        {
            get { return GetSetting<NameValueCollection>(); }
            private set { Setting<NameValueCollection>(value); }
        }

        public Dictionary<string, Type> CustomNameTypes { get; private set; }

        #endregion

    }

}

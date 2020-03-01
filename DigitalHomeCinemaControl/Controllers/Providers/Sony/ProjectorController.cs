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
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Timers;
    using DigitalHomeCinemaControl.Collections;
    using DigitalHomeCinemaControl.Controllers.Base;
    using DigitalHomeCinemaControl.Controllers.Providers.Sony.Sdcp;
    using DigitalHomeCinemaControl.Controllers.Routing;

    [SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "<Pending>")]
    public class ProjectorController : DisplayController, IRoutingDestination
    {

        #region Members

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

            this.DataSource.Add(new BindingItem<string>("Model"));
            this.DataSource.Add(new BindingItem<string>("Calibration Preset"));
            this.DataSource.Add(new BindingItem<string>("Color Space"));
            this.DataSource.Add(new BindingItem<string>("Aspect Ratio"));
            this.DataSource.Add(new BindingItem<string>("Gamma"));
            this.DataSource.Add(new BindingItem<string>("Motion Flow"));
            this.DataSource.Add(new BindingItem<string>("Reality Creation"));

            this.actions = new Dictionary<string, Type> {
                { "CalibrationPreset", typeof(CalibrationPreset) },
                { "LampControl", typeof(LampControl) },
                { "GammaCorrection", typeof(GammaCorrection) },
                { "ColorSpace", typeof(ColorSpace) },
                { "MotionFlow", typeof(MotionFlow) },
                { "HDR", typeof(HDR) },
                { "AspectRatio", typeof(AspectRatio) },
                { "Input", typeof(Input) }
            };

        }

        #endregion

        #region Methods

        public override void Connect()
        {
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
                this.client.Close();
            }

            if (connected) {
                this.ControllerStatus = ControllerStatus.Ok;
                UpdateDataSource<string>("Model", model);
                OnConnected();
            } else {
                this.ControllerStatus = ControllerStatus.Error;
                OnError(string.Format(CultureInfo.InvariantCulture, Properties.Resources.FMT_NETWORK_TIMEOUT, "projector"));
            }

            this.timer.Start();
        }
    
        public override void Disconnect()
        {
            this.running = false;

            if (this.client != null) {
                try {
                    this.client.Close();
                } catch { }
            }

            OnDisconnected();
        }

        public string RouteAction(string action, object args)
        {
            if (string.IsNullOrEmpty(action)) { return "SDCP ERROR: Invalid Action!"; }

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
                    return "SDCP OK.";
                } else {
                    return string.Format(CultureInfo.InvariantCulture, "SDCP ERROR: {0}", response.Error.ToString("G"));
                }
            } else {
                return "SDCP ERROR: Invalid Action!";
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
                OnError(string.Format(CultureInfo.InvariantCulture, Properties.Resources.FMT_NETWORK_TIMEOUT, "projector"));
            } finally {
                if (!this.timer.Enabled) {
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

                UpdateDataSource<string>("Calibration Preset", "");
                UpdateDataSource<string>("Color Space", "");
                UpdateDataSource<string>("Aspect Ratio", "");
                UpdateDataSource<string>("Gamma", "");
                UpdateDataSource<string>("Motion Flow", "");
                UpdateDataSource<string>("Reality Creation", "");

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
            GetItemAndUpdate<CalibrationPreset>("Calibration Preset", CommandItem.CalibrationPreset);
            GetItemAndUpdate<ColorSpace>("Color Space", CommandItem.ColorSpace, this.CustomColorSpace);
            GetItemAndUpdate<AspectRatio>("Aspect Ratio", CommandItem.AspectRatio);
            GetItemAndUpdate<GammaCorrection>("Gamma", CommandItem.GammaCorrection, this.CustomGamma);
            GetItemAndUpdate<MotionFlow>("Motion Flow", CommandItem.MotionFlow);

            var rc = GetItem<RealityCreation>(CommandItem.RealityCreation);
            if (rc == RealityCreation.On) {
                var rcd = GetItem<RealityCreationDatabase>(CommandItem.RealityCreationDatabase);
                if (rcd == RealityCreationDatabase.Unknown) {
                    UpdateDataSource("Reality Creation", rc.GetDescription());
                } else {
                    UpdateDataSource("Reality Creation", string.Format(CultureInfo.InvariantCulture, "{0} - {1}", rc.GetDescription(), rcd.GetDescription()));
                }
            } else {
                UpdateDataSource("Reality Creation", rc.GetDescription());
            }

            this.timer.Start();
        }

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

        #endregion

    }

}

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

namespace DigitalHomeCinemaControl.Controllers.Providers.HDFury
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;
    using System.Net.Sockets;
    using System.Runtime.CompilerServices;
    using System.Timers;
    using DigitalHomeCinemaControl.Components;
    using DigitalHomeCinemaControl.Controllers.Base;
    using DigitalHomeCinemaControl.Controllers.Routing;

    public sealed class DivaController : DeviceController, ISwitchController, IRoutingDestination, ISupportCustomNames, IDisposable
    {

        #region Members

        private const int DEFAULT_PORT = 2210;
        private const int UPDATE_INTERVAL = 1000;

        public const string TX0SINK = "Tx0 Sink";
        public const string TX0OUT = "Tx0 Output";
        public const string INPUT = "Input";

        private const string INPUTTX0 = "Input Tx 0";
        private const string INPUTTX1 = "Input Tx 1";

        private IDictionary<string, Type> actions;
        private TcpClient client;
        private NetworkStream networkStream;
        private StreamReader reader;
        private StreamWriter writer;
        private Timer timer;
        private volatile bool disposed = false;

        #endregion

        #region Constructor

        public DivaController()
            : base()
        {
            this.Host = string.Empty;
            this.Port = DEFAULT_PORT;

            this.CustomInputs = new NameValueCollection();

            this.DataSource.Add(new BindingItem<string>(TX0SINK));
            this.DataSource.Add(new BindingItem<string>(TX0OUT));
            this.DataSource.Add(new BindingItem<Rx>(INPUT, Rx.Unknown));

            this.actions = new Dictionary<string, Type> {
                { INPUT, typeof(Rx) },
                { INPUTTX0, typeof(Rx) },
                { INPUTTX1, typeof(Rx) },
            };

            this.CustomNameTypes = new Dictionary<string, Type> {
                { INPUT, typeof(Rx) },
            };

        }

        #endregion

        #region Methods

        public override void Connect()
        {
            if (string.IsNullOrEmpty(this.Host)) { throw new InvalidOperationException(Properties.Resources.MSG_INVALID_HOST); }
            this.disposed = false;

            this.client = new TcpClient() {
                NoDelay = true,
            };

            try {
                this.client.Connect(this.Host, this.Port);
                this.networkStream = this.client.GetStream();
                this.reader = new StreamReader(this.networkStream);
                this.writer = new StreamWriter(this.networkStream) {
                    AutoFlush = true,
                };
            } catch {
                this.IsConnected = false;
                this.ControllerStatus = ControllerStatus.Error;
                OnError(string.Format(CultureInfo.InvariantCulture, Properties.Resources.FMT_NETWORK_TIMEOUT, "HDFury"));
                return;
            }

            this.IsConnected = true;

            this.timer = new System.Timers.Timer() {
                Interval = UPDATE_INTERVAL,
                AutoReset = false,
            };
            this.timer.Elapsed += TimerElapsed;
            this.timer.Start();

            OnConnected();
        }

        public override void Disconnect()
        {
            this.IsConnected = false;

            if (this.timer != null && this.timer.Enabled) {
                this.timer.Stop();
            }

            try {
                Dispose(true);
            } catch {
            } finally {
                OnDisconnected();
            }
        }

        private void ClientDisconnected()
        {
            OnError(string.Format(CultureInfo.InvariantCulture, Properties.Resources.FMT_DISCONNECTED, "HDFury"));
            this.ControllerStatus = ControllerStatus.Error;

            try {
                this.client?.Close();
            } catch { }

            // TODO: reconnect?
        }

        public string RouteAction(string action, object args)
        {
            if (string.IsNullOrEmpty(action)) {
                return string.Format(CultureInfo.InvariantCulture, Properties.Resources.FMT_HDFURY_ERROR, Properties.Resources.MSG_INVALID_ACTION);
            }
            if (args == null) {
                return string.Format(CultureInfo.InvariantCulture, Properties.Resources.FMT_HDFURY_ERROR, Properties.Resources.MSG_INVALID_ARGS);
            }
            if (!this.IsConnected) {
                return string.Format(CultureInfo.InvariantCulture, Properties.Resources.FMT_HDFURY_ERROR, Properties.Resources.MSG_NOT_CONNECTED);
            }

            string command;

            switch (action) {
                case INPUT: command = string.Format(CultureInfo.InvariantCulture, "set insel {0} 4", (int)args); break;
                case INPUTTX0: command = string.Format(CultureInfo.InvariantCulture, "set inseltx0 {0}", (int)args); break;
                case INPUTTX1: command = string.Format(CultureInfo.InvariantCulture, "set inseltx1 {0}", (int)args); break;
                default: command = string.Empty; break;
            }

            if (string.IsNullOrEmpty(command)) {
                return string.Format(CultureInfo.InvariantCulture, Properties.Resources.FMT_HDFURY_ERROR, Properties.Resources.MSG_INVALID_ACTION);
            }

            if (this.writer != null) {
                try {
                    this.writer.WriteLine(command);
                    this.writer.Flush();
                } catch {
                    return string.Format(CultureInfo.InvariantCulture, Properties.Resources.FMT_HDFURY_ERROR, Properties.Resources.MSG_IO_ERROR);
                }
            }

            return string.Format(CultureInfo.InvariantCulture, "HDFURY {0}", Properties.Resources.MSG_OK);
        }

        internal bool SetInput(Rx input)
        {
            if (!this.IsConnected) { return false; }

            string command = string.Format(CultureInfo.InvariantCulture, "set insel {0} 4", (int)input);

            if (this.writer != null) {
                try {
                    this.writer.WriteLine(command);
                    this.writer.Flush();
                } catch {
                    return false;
                }
            }

            return true;
        }

        private void OnDataReceived(string data)
        {
            if (string.IsNullOrEmpty(data)) { return; }
            if (!data.Contains(" ")) { return; }

            string[] dataParts = data.Split(null, 2);

            switch (dataParts[0].ToUpperInvariant()) {
                case "TX0:":
                    UpdateDataSource<string>(TX0OUT, dataParts[1].Trim());
                    break;
                case "TX0SINK:": 
                    UpdateDataSource<string>(TX0SINK, dataParts[1].Trim());
                    break;
                case "INSELTX0": 
                    if (int.TryParse(dataParts[1], out int i)) {
                        UpdateDataSource<Rx>(INPUT, (Rx)i);
                    }
                    break;
            }

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateState()
        {
            string data;
            
            this.writer.WriteLine("get status tx0sink");
            data = this.reader.ReadLine();
            OnDataReceived(data);
            this.writer.WriteLine("get status tx0");
            data = this.reader.ReadLine();
            OnDataReceived(data);
            this.writer.WriteLine("get inseltx0");
            data = this.reader.ReadLine();
            OnDataReceived(data);

            this.timer.Start();
        }

        private void TimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if ((this.client == null) || !this.client.Connected) {
                ClientDisconnected();    
                return; 
            }

            if (!this.networkStream.CanRead || !this.networkStream.CanWrite) {
                ClientDisconnected();
                return;
            }

            try {
                UpdateState();
            } catch { 
            } finally {
                if (this.IsConnected && (this.timer != null) && !this.timer.Enabled) {
                    this.timer.Start();
                }
            }
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed) {
                if (disposing) {
                    this.client?.Close();
                    this.reader?.Dispose();
                    this.writer?.Dispose();
                    this.networkStream?.Dispose();
                    this.timer?.Dispose();
                }

                this.disposed = true;

                this.client = null;
                this.reader = null;
                this.writer = null;
                this.networkStream = null;
                this.timer = null;
            }
        }

        ~DivaController()
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

        public string Host
        {
            get { return GetSetting<string>(); }
            set { Setting<string>(value); }
        }

        public int Port
        {
            get { return GetSetting<int>(); }
            set { Setting<int>(value); }
        }

        public NameValueCollection CustomInputs
        {
            get { return GetSetting<NameValueCollection>(); }
            private set { Setting<NameValueCollection>(value); }
        }

        public string Name
        {
            get { return "HD Fury"; }
        }

        public IDictionary<string, Type> Actions
        {
            get { return this.actions; }
        }

        public bool IsConnected { get; private set; }

        public Dictionary<string, Type> CustomNameTypes { get; private set; }

        #endregion

    }

}

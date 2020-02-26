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
    using System.IO;
    using System.Net.Sockets;
    using System.Threading;
    using DigitalHomeCinemaControl.Collections;
    using DigitalHomeCinemaControl.Controllers.Base;
    using DigitalHomeCinemaControl.Controllers.Routing;

    public class DivaController : DeviceController, ISwitchController, IRoutingDestination
    {

        #region Members

        private const int DEFAULT_PORT = 2210;
        private const int UPDATE_INTERVAL = 2000;
        private const string INTERNAL_THREAD_NAME = "HDFURY_TCPREAD";

        private IDictionary<string, Type> actions;
        private TcpClient client;
        private NetworkStream networkStream;
        private StreamReader reader;
        private StreamWriter writer;
        private Thread readThread;
        private System.Timers.Timer timer;

        #endregion

        #region Constructor

        public DivaController()
            : base()
        {
            this.Host = string.Empty;
            this.Port = DEFAULT_PORT;

            this.CustomInputs = new NameValueCollection();

            this.DataSource.Add(new BindingItem<string>("Tx 0"));
            this.DataSource.Add(new BindingItem<string>("Tx 1"));
            this.DataSource.Add(new BindingItem<Rx>("Input"));

            this.actions = new Dictionary<string, Type> {
                { "Input", typeof(Rx) },
                { "Input Tx 0", typeof(Rx) },
                { "Input Tx 1", typeof(Rx) },
            };

        }

        #endregion

        #region Methods

        public override void Connect()
        {
            if (string.IsNullOrEmpty(this.Host)) { throw new InvalidOperationException("Invalid Host"); }

            this.client = new TcpClient() {
                NoDelay = true,
            };

            try {
                this.client.Connect(this.Host, this.Port);
                this.networkStream = this.client.GetStream();
                this.reader = new StreamReader(this.networkStream);
                this.writer = new StreamWriter(this.networkStream);
            } catch {
                this.IsConnected = false;
                this.ControllerStatus = ControllerStatus.Error;
                OnError("Network timeout connecting to HD Fury");
                return;
            }

            this.readThread = new Thread(this.ClientThread) {
                Name = INTERNAL_THREAD_NAME,
            };
            this.readThread.Start();

            this.timer = new System.Timers.Timer() {
                Interval = UPDATE_INTERVAL,
                AutoReset = true,
            };
            this.timer.Elapsed += TimerElapsed;
            this.timer.Start();

            this.IsConnected = true;
            OnConnected();
        }

        public override void Disconnect()
        {
            this.IsConnected = false;

            if (this.timer != null && this.timer.Enabled) {
                this.timer.Stop();
            }

            try {
                this.client.Close();
                this.readThread.Abort();
                this.readThread.Join();
            } catch {
            } finally {
                if (this.reader != null) {
                    this.reader.Close();
                }
                if (this.writer != null) {
                    this.writer.Close();
                }
                if (this.networkStream != null) {
                    this.networkStream.Close();
                }
            }

            OnDisconnected();
        }

        public string RouteAction(string action, object args)
        {
            if (!this.IsConnected) {
                return "HD FURY: Not Connected!";
            }

            string command;

            switch (action) {
                case "Input": command = string.Format("set insel {0} 4", (int)args); break;
                case "Input Tx 0": command = string.Format("set inseltx0 {0}", (int)args); break;
                case "Input Tx 1": command = string.Format("set inseltx1 {0}", (int)args); break;
                default: command = string.Empty; break;
            }

            if (string.IsNullOrEmpty(command)) {
                return "HD FURY: Invalid Action!";
            }

            if (this.writer != null) {
                try {
                    this.writer.WriteLine(command);
                    this.writer.Flush();
                } catch {
                    return "HD Fury: IO Error!";
                }
            }

            return "HD FURY: Ok.";
        }

        internal bool SetInput(Rx input)
        {
            if (!this.IsConnected) { return false; }

            string command = string.Format("set insel {0} 4", input);

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

        private void ClientThread()
        {
            string buffer = string.Empty;

            UpdateState();

            while (this.IsConnected) {
                if (!this.networkStream.CanRead || !this.networkStream.CanWrite) {
                    Disconnect();
                    return;
                }

                try {
                    buffer = this.reader.ReadLine();
                } catch {
                    if (!this.IsConnected) { return; }
                }

                OnDataReceived(buffer);
            }
        }

        private void OnDataReceived(string data)
        {
            if (string.IsNullOrEmpty(data)) { return; }

            // TODO: Parse data
        }

        /*        
        *      # diva get status x
        *      where x is [rx0, rx1, tx0, tx1, tx0sink, tx1sink, aud0, aud1, audout, spd0, spd1]
        *      rx0 and rx1 indicate the input stream received.Use rx0 in splitter mode and rx0 and rx1 along with 'insel' in matrix mode
        *      tx0 and tx1 indicate the outgoing stream
        *      tx0sink and tx1sink indicate EDID capabilities of the connected sink
        *      aud0 and aud1 and audout indicate the audio stream type going to the sink from the correspoding port
        *      spd0 and spd1 indicate the source name. Use spd0 for splitter mode and spd0 and spd1 along with 'insel' in matrix mode
        *      Ex. #diva get status rx0
        *      Gets the current incoming video format
        */

        private void UpdateState()
        {
            if (this.writer != null) {
                this.writer.WriteLine("get status tx0");
                this.writer.WriteLine("get status tx1");
                this.writer.WriteLine("get inseltx0");
                this.writer.Flush();
            }
        }

        private void TimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (this.IsConnected) {
                UpdateState();
            }
        }

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
            set { Setting<NameValueCollection>(value); }
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

        #endregion

    }

}

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

namespace DigitalHomeCinemaControl.Controllers.Providers.Serial
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO.Ports;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using DigitalHomeCinemaControl.Controllers.Base;
    using DigitalHomeCinemaControl.Controllers.Routing;


    /// <summary>
    /// Generic Serial Device Controller
    /// </summary>
    [SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "<Pending>")]
    public class SerialController : DeviceController, ISerialController, IRoutingSource
    {

        #region Members

        private const int READ_DELAY = 500;
        private const int BAUD_RATE = 19200;
        private const Parity PARITY = Parity.None;
        private const int DATA_BITS = 8;
        private const StopBits STOP_BITS = StopBits.One;

        private SerialPort serialPort;
        private AutoResetEvent waitHandle;

        #endregion

        #region Constructor

        public SerialController()
            : base()
        {
            this.waitHandle = new AutoResetEvent(false);

            this.Port = string.Empty;
            this.ReadDelay = READ_DELAY;
            this.BaudRate = BAUD_RATE;
            this.Parity = PARITY;
            this.DataBits = DATA_BITS;
            this.StopBits = STOP_BITS;
        }

        #endregion

        #region  Methods

        public override void Connect()
        {
            if (this.waitHandle == null) {
                this.waitHandle = new AutoResetEvent(false);
            }
            this.waitHandle.Reset();

            if (string.IsNullOrEmpty(this.Port)) {
                this.ControllerStatus = ControllerStatus.Error;
                return;
            }

            this.serialPort = new SerialPort(this.Port) {
                BaudRate = this.BaudRate,
                Parity = this.Parity,
                DataBits = this.DataBits,
                StopBits = this.StopBits
            };
            this.serialPort.DataReceived += SerialPort_DataReceived;
            this.serialPort.Disposed += SerialPort_Disposed;
            
            try {
                this.serialPort.Open();
                OnConnected();
            } catch {
                this.ControllerStatus = ControllerStatus.Error;
                OnError("Failed to open serial connection on " + this.Port);
            }
        }

        private void SerialPort_Disposed(object sender, EventArgs e)
        {
            this.serialPort = null;
            OnDisconnected();
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (!this.waitHandle.WaitOne(this.ReadDelay)) {
                string buffer = this.serialPort.ReadLine();
                string hexBuffer = buffer.AsciiToHexString();

                OnDataReceived(hexBuffer);
            }
        }

        public override void Disconnect()
        {
            if (this.waitHandle != null) {
                this.waitHandle.Set();
                this.waitHandle.Dispose();
            }

            if (this.serialPort != null) {
                this.serialPort.Close();
            }

            this.serialPort = null;
            this.waitHandle = null;

            OnDisconnected();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void OnDataReceived(string data)
        {
            RouteData?.Invoke(this, new RoutingItem(this.Name, typeof(string), data));
        }

        #endregion

        #region Events

        public event EventHandler<RoutingItem> RouteData;

        #endregion

        #region Properties

        public string CommPort
        {
            get { return this.Port; }
        }

        public string Name
        { 
            get { return "Serial"; }
        }

        public Type MatchType
        {
            get { return typeof(string); }
        }

        public string Port
        {
            get { return GetSetting<string>(); }
            set { Setting<string>(value); }
        }

        public int ReadDelay
        {
            get { return GetSetting<int>(); }
            set { Setting<int>(value); }
        }

        public int BaudRate
        {
            get { return GetSetting<int>(); }
            set { Setting<int>(value); }
        }

        public Parity Parity
        {
            get { return GetSetting<Parity>(); }
            set { Setting<Parity>(value); }
        }

        public int DataBits
        {
            get { return GetSetting<int>(); }
            set { Setting<int>(value); }
        }

        public StopBits StopBits
        {
            get { return GetSetting<StopBits>(); }
            set { Setting<StopBits>(value); }
        }

        public List<string> CommPorts
        {
            get {
                List<string> result = new List<string>();
                foreach (string port in SerialPort.GetPortNames()) {
                    result.Add(port);
                }
                return result;
            }

        }

        #endregion

    }

}

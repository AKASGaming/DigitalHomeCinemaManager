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
    using System.Net.Sockets;
    using DigitalHomeCinemaControl.Collections;
    using DigitalHomeCinemaControl.Controllers.Base;
    using DigitalHomeCinemaControl.Controllers.Routing;

    public class DivaController : DeviceController, ISwitchController, IRoutingDestination
    {

        #region Members

        private const int DEFAULT_PORT = 2210;

        private IDictionary<string, Type> actions;
        private TcpClient client;

        #endregion

        #region Constructor

        public DivaController()
            : base()
        {
            this.Host = string.Empty;
            this.Port = DEFAULT_PORT;

            this.CustomInputs = new NameValueCollection();

            this.DataSource.Add(new BindingItem<Rx>("Tx 0"));
            this.DataSource.Add(new BindingItem<Rx>("Tx 1"));

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
            throw new NotImplementedException();
        }

        public override void Disconnect()
        {
            throw new NotImplementedException();
        }

        public string RouteAction(string action, object args)
        {
            string command;

            switch (action) {
                case "Input": command = string.Format("set insel {0} {0}", (int)args); break;
                case "Input Tx 0": command = string.Format("set inseltx0 {0}", (int)args); break;
                case "Input Tx 1": command = string.Format("set inseltx1 {0}", (int)args); break;
                default: command = string.Empty; break;
            }
            
            if (string.IsNullOrEmpty(command)) {
                return "HD FURY: Invalid Action!";
            }


            return "HD FURY: Ok.";
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

        #endregion

    }

}

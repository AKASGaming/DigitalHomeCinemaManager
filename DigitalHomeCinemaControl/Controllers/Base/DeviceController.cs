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

namespace DigitalHomeCinemaControl.Controllers.Base
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Windows.Threading;
    using DigitalHomeCinemaControl.Collections;

    /// <summary>
    /// Abstract Controller class that all controllers should derive from.
    /// </summary>
    public abstract class DeviceController : IController
    {

        #region Members

        private Dispatcher dispatcher;
        private ControllerStatus controllerStatus;

        #endregion

        #region Contructor

        /// <summary>
        /// Create a new instance of the DeviceController class.
        /// </summary>
        protected DeviceController()
        {
            this.Settings = new Dictionary<string, SettingItem<object>>();
            this.DataSource = new DispatchedBindingList<IBindingItem> {
                RaiseListChangedEvents = true
            };
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Connect to device.
        /// </summary>
        public abstract void Connect();

        /// <summary>
        /// Disconnect from device.
        /// </summary>
        public abstract void Disconnect();

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets a controller setting.
        /// </summary>
        /// <typeparam name="T">The Type of the setting to set.</typeparam>
        /// <param name="value">The value of the setting.</param>
        /// <param name="name">The name of the setting.</param>
        public void Setting<T>(T value, [CallerMemberName] string name = null)
        {
            Debug.Assert(!string.IsNullOrEmpty(name));

            if (!this.Settings.ContainsKey(name)) {
                this.Settings.Add(name, new SettingItem<object>(typeof(T), value));
            } else {
                this.Settings[name].Value = value;
            }

            OnSettingChanged(name);
        }

        #endregion

        #region Protected Methods

        protected virtual void OnSettingChanged(string name)
        { }

        /// <summary>
        /// Gets a controller setting.
        /// </summary>
        /// <typeparam name="T">The Type of the setting to get.</typeparam>
        /// <param name="name">The name of the setting to get.</param>
        /// <returns>The settings value or default if not defined.</returns>
        protected T GetSetting<T>([CallerMemberName] string name = null)
        {
            Debug.Assert(!string.IsNullOrEmpty(name));
            if (!this.Settings.ContainsKey(name)) { return default; }

            Type type = this.Settings[name].Type;
            object result = this.Settings[name].Value;

            Debug.Assert(type == typeof(T));

            return (T)result;
        }

        /// <summary>
        /// Updates the specfied IBindingItem in the DataSource.
        /// </summary>
        /// <typeparam name="T">The type of the IBindingItem value.</typeparam>
        /// <param name="name">The name of the DataSource item to update.</param>
        /// <param name="value">The new value for the DataSource IBindingItem.</param>
        protected void UpdateDataSource<T>(string name, T value)
        {
            Debug.Assert(this.DataSource != null);

            IBindingItem item = this.DataSource[name];
            if ((item != null) && item.Name.Equals(name, StringComparison.Ordinal)) {
                item.Value = value;
            }
        }

        /// <summary>
        /// Raises the IController.Connected event.
        /// </summary>
        /// <remarks>
        /// This event is not marshalled to UI Thread.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void OnConnected()
        {
            this.ControllerStatus = ControllerStatus.Ok;

            Connected?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Raises the IController.Disconnected event.
        /// </summary>
        /// <remarks>
        /// This event is not marshalled to UI Thread.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void OnDisconnected()
        {
            this.ControllerStatus = ControllerStatus.Disconnected;

            Disconnected?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Raises the IController.Error event.
        /// </summary>
        /// <remarks>
        /// This event is not marshalled to the UI Thread.
        /// </remarks>
        /// <param name="message"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void OnError(string message)
        {
            Error?.Invoke(this, message);
        }

        /// <summary>
        /// Raises the INotifyPropertyChanged.PropertyChanged event.
        /// This event IS marshalled to the UI Thread if possible.
        /// </summary>
        /// <remarks>
        /// This event IS marshalled to the UI Thread if possible.
        /// </remarks>
        /// <param name="name"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            if ((this.Dispatcher != null) && !this.Dispatcher.CheckAccess()) {
                this.Dispatcher.BeginInvoke((Action)(() => {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
                }));
            } else {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Raised whenever the controller has connected to the device.
        /// </summary>
        public event EventHandler Connected;

        /// <summary>
        /// Raised whenever the controller has disconnected from the device.
        /// </summary>
        public event EventHandler Disconnected;

        /// <summary>
        /// Raised whenever the controller encouters an error with the device.
        /// </summary>
        public event EventHandler<string> Error;

        /// <summary>
        /// Raised whenever a controller property has changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current status of the controller.
        /// </summary>
        public ControllerStatus ControllerStatus
        {
            get { return this.controllerStatus; }
            protected set {
                if (value != this.controllerStatus) {
                    this.controllerStatus = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or Sets the Dispatcher used for marshalling updates to the UI thread.
        /// </summary>
        public Dispatcher Dispatcher
        {
            get { return this.dispatcher; }
            set {
                this.dispatcher = value;
                if (this.DataSource != null) {
                    this.DataSource.Dispatcher = this.dispatcher;
                }
            }
        }

        /// <summary>
        /// Gets a collection of settings defined by the controller.
        /// </summary>
        public Dictionary<string, SettingItem<object>> Settings { get; private set; }

        /// <summary>
        /// Gets a BindingList collection that can be bound to UI control elements.
        /// </summary>
        public IDispatchedBindingList<IBindingItem> DataSource { get; protected set; }

        #endregion

    }

}

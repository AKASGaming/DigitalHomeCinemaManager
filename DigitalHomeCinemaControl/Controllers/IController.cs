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

namespace DigitalHomeCinemaControl.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Threading;
    using DigitalHomeCinemaControl.Collections;

    /// <summary>
    /// Interface for all Device Controllers to implement.
    /// </summary>
    public interface IController : INotifyPropertyChanged
    {

        /// <summary>
        /// Connect to the device.
        /// </summary>
        void Connect();

        /// <summary>
        /// Disconnect from the device.
        /// </summary>
        void Disconnect();
 
        /// <summary>
        /// Sets a IController setting.
        /// </summary>
        /// <typeparam name="T">The Type of the setting.</typeparam>
        /// <param name="value">The setting value.</param>
        /// <param name="name">The setting name.</param>
        void Setting<T>(T value, [CallerMemberName] string name = null);

        /// <summary>
        /// Raised when the controller has connected to the device.
        /// </summary>
        event EventHandler Connected;

        /// <summary>
        /// Raised when the controller has disconnected from the device.
        /// </summary>
        event EventHandler Disconnected;

        /// <summary>
        /// Raised when the controller encounters an error.
        /// </summary>
#pragma warning disable CA1716 // Identifiers should not match keywords
        event EventHandler<ControllerErrorEventArgs> Error;
#pragma warning restore CA1716

        /// <summary>
        /// Gets the controller status.
        /// Must raise the PropertyChanged event.
        /// </summary>
        ControllerStatus ControllerStatus { get; }

        /// <summary>
        /// Gets or Sets the Dispatcher used to marshal events to the UI thread.
        /// </summary>
        Dispatcher Dispatcher { get; set; }

        /// <summary>
        /// Gets a collection of settings that are defined by the controller.
        /// </summary>
        Dictionary<string, SettingItem<object>> Settings { get; }

        /// <summary>
        /// Gets a collection of IBindingItems for data binding to controls.
        /// </summary>
        IDispatchedBindingList<IBindingItem> DataSource { get; }

    }

}

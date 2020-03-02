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

namespace DigitalHomeCinemaControl.Devices
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Serial device.
    /// </summary>
    public sealed class SerialDevice
        : Device
    {

        #region Registration

        public static SerialDevice Generic { get; } = new SerialDevice("Generic",
                typeof(Controllers.Providers.Serial.SerialController));

        static SerialDevice()
        {
            Items.Add(Generic.Name, Generic);
        }

        #endregion

        #region Members

        /// <summary>
        /// Lists all available SerialDevices.
        /// </summary>
        public static Dictionary<string, SerialDevice> Items { get; } = new Dictionary<string, SerialDevice>();

        #endregion

        #region Constructor

        private SerialDevice(string name, Type controllerType)
            : base(name, DeviceType.Serial, controllerType, null)
        { }

        #endregion

    }

}

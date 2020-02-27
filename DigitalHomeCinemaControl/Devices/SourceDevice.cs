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
    /// Source device.
    /// </summary>
    public sealed class SourceDevice : Device
    {

        #region Registration

        public static SourceDevice MpcHc { get; } = new SourceDevice("MPC Home Cinema",
                typeof(Controllers.Providers.MediaPlayerClassic.MpcController),
                typeof(Controls.Common.SourceInfoControl));

        static SourceDevice()
        {
            Items.Add(MpcHc.Name, MpcHc);
        }

        #endregion

        #region Members

        /// <summary>
        /// Lists all available SourceDevices.
        /// </summary>
        public static Dictionary<string, SourceDevice> Items = new Dictionary<string, SourceDevice>();

        #endregion

        #region Constructor

        private SourceDevice(string name, Type controllerType, Type uiElementType)
            : base(name, DeviceType.Source, controllerType, uiElementType)
        { }

        #endregion

    }

}

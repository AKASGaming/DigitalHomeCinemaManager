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
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Display device.
    /// </summary>
    public sealed class DisplayDevice : Device
    {

        #region Registration

        public static DisplayDevice SonyProjector { get; } = new DisplayDevice("Sony Projector",
                typeof(Controllers.Providers.Sony.ProjectorController),
                typeof(Controls.Common.ProjectorInfoPanelControl));



        static DisplayDevice()
        {
            Items.Add(SonyProjector.Name, SonyProjector);
        }

        #endregion

        #region Members

        /// <summary>
        /// Lists all available DisplayDevices.
        /// </summary>
        [SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible", Justification = "<Pending>")]
        public static Dictionary<string, DisplayDevice> Items = new Dictionary<string, DisplayDevice>();

        #endregion

        #region Constructor

        private DisplayDevice(string name, Type controllerType, Type uiElementType)
            : base(name, DeviceType.Display, controllerType, uiElementType)
        { }

        #endregion

    }

}

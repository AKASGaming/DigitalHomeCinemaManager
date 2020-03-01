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

    public sealed class SwitchDevice : Device
    {

        #region Registration

        public static SwitchDevice HDFuryDiva { get; } = new SwitchDevice("HDFury Diva",
                typeof(Controllers.Providers.HDFury.DivaController),
                typeof(Controls.HDFury.InputControl));



        static SwitchDevice()
        {
            Items.Add(HDFuryDiva.Name, HDFuryDiva);
        }

        #endregion

        #region Members

        /// <summary>
        /// Lists all available ProcessorDevices.
        /// </summary>
        [SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible", Justification = "<Pending>")]
        public static Dictionary<string, SwitchDevice> Items = new Dictionary<string, SwitchDevice>();

        #endregion

        #region Constructor

        private SwitchDevice(string name, Type controllerType, Type uiElementType)
            : base(name, DeviceType.InputSwitch, controllerType, uiElementType)
        { }

        #endregion

    }

}

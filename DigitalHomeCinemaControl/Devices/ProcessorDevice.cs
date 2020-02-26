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

    public sealed class ProcessorDevice : Device
    {

        #region Registration

        public static ProcessorDevice DenonAvr { get; } = new ProcessorDevice("Denon AVR",
                typeof(Controllers.Providers.Denon.AvrController),
                typeof(Controls.Denon.AvrInfoControl));



        static ProcessorDevice()
        {
            Items.Add(DenonAvr.Name, DenonAvr);
        }

        #endregion

        #region Members

        /// <summary>
        /// Lists all available ProcessorDevices.
        /// </summary>
        public static Dictionary<string, ProcessorDevice> Items = new Dictionary<string, ProcessorDevice>();

        #endregion

        #region Constructor

        private ProcessorDevice(string name, Type controllerType, Type uiElementType)
            : base(name, controllerType, uiElementType)
        { }

        #endregion

    }

}

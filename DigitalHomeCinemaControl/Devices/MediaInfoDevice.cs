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

    public sealed class MediaInfoDevice : Device
    {

        #region Regiatration

        public static MediaInfoDevice MovieDb { get; } = new MediaInfoDevice("TMDB",
                typeof(Controllers.Providers.MovieDb.MovieDbController),
                typeof(Controls.Common.MediaInfoControl));

        static MediaInfoDevice()
        {
            Items.Add(MovieDb.Name, MovieDb);
        }

        #endregion

        #region Members

        /// <summary>
        /// Lists all available MediaInfoDevices.
        /// </summary>
        public static Dictionary<string, MediaInfoDevice> Items = new Dictionary<string, MediaInfoDevice>();

        #endregion

        #region Constructor

        private MediaInfoDevice(string name, Type controllerType, Type uiElementType)
            : base(name, DeviceType.MediaInfo, controllerType, uiElementType)
        { }

        #endregion

    }

}

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

    /// <summary>
    /// Abstract DisplayController class.
    /// </summary>
    public abstract class DisplayController : DeviceController, IDisplayController
    {

        #region Members

        private int lampTimer = -1;
        private LampStatus lampStatus = LampStatus.Unknown;

        #endregion

        #region Contructor

        /// <summary>
        /// Creates a new instance of the DisplayController class.
        /// </summary>
        protected DisplayController()
            : base()
        {
            this.Host = string.Empty;
            this.Port = 0;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or Sets the IP Address of the display.
        /// </summary>
        public string Host
        {
            get { return GetSetting<string>(); }
            set { Setting<string>(value); }
        }

        /// <summary>
        /// Gets or Sets the IP Port of the display.
        /// </summary>
        public int Port
        {
            get { return GetSetting<int>(); }
            set { Setting<int>(value); }
        }

        /// <summary>
        /// Gets the DisplayType.
        /// </summary>
        public DisplayType DisplayType { get; protected set; }

        /// <summary>
        /// Gets the current Lamp hours for DisplayType.Projector,
        /// or operating hours for DisplayType.Lcd instances.
        /// </summary>
        public int LampTimer
        {
            get { return this.lampTimer; }
            protected set {
                if (value != this.lampTimer) {
                    this.lampTimer = value;
                    OnPropertyChanged();
                }
            }

        }

        /// <summary>
        /// Gets the current LampStatus for DisplayType.Projector instances.
        /// </summary>
        public LampStatus LampStatus
        {
            get { return this.lampStatus; }
            protected set {
                if (value != this.lampStatus) {
                    this.lampStatus = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

    }

}

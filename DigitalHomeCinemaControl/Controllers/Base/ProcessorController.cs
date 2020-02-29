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
    using System.Text;

    public abstract class ProcessorController : DeviceController, IProcessorController
    {

        #region Members

        private int delay;
        private decimal masterVolume;

        #endregion

        #region Constructor

        protected ProcessorController()
            : base()
        {
            this.Host = string.Empty;
            this.Port = 0;
        }

        #endregion

        #region Methods

        protected decimal RelativeToAbsoluteVolume(int scale, int relativeVolume)
        {
            if (relativeVolume == 0) {
                return (relativeVolume - scale);
            }

            StringBuilder sb = new StringBuilder(relativeVolume.ToString());
            if (sb.Length == 3) {
                sb.Insert(3, ".");
            }
            if (decimal.TryParse(sb.ToString(), out decimal d)) {
                d -= scale;
            }

            return d;
        }

        protected int AbsoluteToRelativeVolume(int scale, decimal absoluteVolume)
        {
            decimal d = absoluteVolume + scale;
            StringBuilder sb = new StringBuilder(d.ToString());
            sb.Replace(".", "");

            if (int.TryParse(sb.ToString(), out int i)) {
                return i;
            }

            return 0;
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

        public int Delay
        {
            get { return this.delay; }
            protected set {
                if (value != this.delay) {
                    this.delay = value;
                    OnPropertyChanged();
                }
            }

        }

        public decimal MasterVolume
        {
            get { return this.masterVolume; }
            protected set {
                if (value != this.masterVolume) {
                    this.masterVolume = value;
                    OnPropertyChanged();
                }
            }

        }

        #endregion

    }

}

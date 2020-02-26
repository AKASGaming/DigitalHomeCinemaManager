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

namespace DigitalHomeCinemaControl.Collections
{
    using System;
    using System.Collections.Generic;

    public class ChannelStatus
    {

        #region Constructor

        /// <summary>
        /// Creates a new ChannelStatus instance.
        /// </summary>
        public ChannelStatus()
        {
            this.AvailableChannels = new Dictionary<AudioChannel, bool>();
            this.ActiveChannels = new Dictionary<AudioChannel, bool>();

            foreach (AudioChannel channel in Enum.GetValues(typeof(AudioChannel))) {
                this.AvailableChannels.Add(channel, false);
                this.ActiveChannels.Add(channel, false);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Resets all available channels to a default value.
        /// </summary>
        /// <param name="value">Optional. Value to set, default is false.</param>
        public void ResetAvailableChannels(bool value = false)
        {
            foreach (AudioChannel channel in Enum.GetValues(typeof(AudioChannel))) {
                this.AvailableChannels[channel] = value;
            }
        }

        /// <summary>
        /// Resets all active channels to a default value.
        /// </summary>
        /// <param name="value">Optional. Value to set, default is false.</param>
        public void ResetActiveChannels(bool value = false)
        {
            foreach (AudioChannel channel in Enum.GetValues(typeof(AudioChannel))) {
                this.ActiveChannels[channel] = value;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a collection of available audio channels.
        /// </summary>
        public Dictionary<AudioChannel, bool> AvailableChannels { get; private set; }

        /// <summary>
        /// Gets a collection of active audio channels.
        /// </summary>
        public Dictionary<AudioChannel, bool> ActiveChannels { get; private set; }

        /// <summary>
        /// Gets or Sets a value indicating if unused channels should be hidden by the UI.
        /// </summary>
        public bool HideUnusedChannels { get; set; }

        #endregion

    }

}

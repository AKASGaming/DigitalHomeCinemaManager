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

namespace DigitalHomeCinemaControl.Components.Audio
{
    using System;
    using System.Collections.Generic;

    public class ChannelStatus
    {

        #region Members

        private static AudioChannel[] CHANNELS = (AudioChannel[])Enum.GetValues(typeof(AudioChannel));
        private static int CHANNEL_COUNT = CHANNELS.Length;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new ChannelStatus instance.
        /// </summary>
        public ChannelStatus()
        {
            this.AvailableChannels = new Dictionary<AudioChannel, bool>(CHANNEL_COUNT);
            this.ActiveChannels = new Dictionary<AudioChannel, bool>(CHANNEL_COUNT);

            for (int i = CHANNEL_COUNT - 1; i >= 0; i--) {
                this.AvailableChannels.Add(CHANNELS[i], false);
                this.ActiveChannels.Add(CHANNELS[i], false);
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
            if (CHANNEL_COUNT == 0) { return; }

            for (int i = CHANNEL_COUNT - 1; i >= 0; i--) {
                this.AvailableChannels[CHANNELS[i]] = value;
            }
        }

        /// <summary>
        /// Resets all active channels to a default value.
        /// </summary>
        /// <param name="value">Optional. Value to set, default is false.</param>
        public void ResetActiveChannels(bool value = false)
        {
            if (CHANNEL_COUNT == 0) { return; }

            for (int i = CHANNEL_COUNT - 1; i >=0; i--) {
                this.ActiveChannels[CHANNELS[i]] = value;
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

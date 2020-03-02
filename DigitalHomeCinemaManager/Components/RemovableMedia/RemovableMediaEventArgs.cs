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

namespace DigitalHomeCinemaManager.Components.RemovableMedia
{
    using System;

    /// <summary>
    /// Our class for passing in custom arguments to our event handlers 
    /// </summary>
    public class RemovableMediaEventArgs : EventArgs
    {

        public RemovableMediaEventArgs()
        {
            this.Cancel = false;
            this.Drive = "";
            this.HookQueryRemove = false;
        }

        /// <summary>
        /// Get/Set the value indicating that the event should be cancelled 
        /// Only in QueryRemove handler.
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// Drive letter for the device which caused this event 
        /// </summary>
        public string Drive { get; set; }

        /// <summary>
        /// Set to true in your DeviceArrived event handler if you wish to receive the 
        /// QueryRemove event for this drive. 
        /// </summary>
        public bool HookQueryRemove { get; set; }

    }

}

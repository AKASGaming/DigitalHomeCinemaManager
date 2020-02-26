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
    using System.Windows.Threading;

    /// <summary>
    /// Interface for classes which have a Dispatcher to marshall events to the UI thread.
    /// </summary>
    public interface IDispatchEvents
    {

        /// <summary>
        /// Gets or Sets the Dispatcher.
        /// </summary>
        Dispatcher Dispatcher { get; set; }

    }

}

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

namespace DigitalHomeCinemaControl.Controls
{
    using System.ComponentModel;
    using DigitalHomeCinemaControl.Collections;

    /// <summary>
    /// Provides a DataSource that controls can bind to.
    /// </summary>
    public interface IDeviceControl
    {

        /// <summary>
        /// Gets or Sets a generic collection that supports data binding.
        /// </summary>
        IDispatchedBindingList<IBindingItem> DataSource { get; set; }

    }

}

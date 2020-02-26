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
    using System.ComponentModel;
    using System.Windows.Threading;

    /// <summary>
    /// Notifies clients when a value has changed.
    /// </summary>
    public interface IBindingItem : INotifyPropertyChanged
    {

        /// <summary>
        /// Gets the Name of the IBindingItem.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets or Sets the Value of the IBindingItem
        /// </summary>
        object Value { get; set; }

    }

}

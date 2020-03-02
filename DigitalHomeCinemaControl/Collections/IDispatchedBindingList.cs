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

    /// <summary>
    /// Interface for IBindingList derived classes which also marshall events to the UI thread.
    /// </summary>
    /// <typeparam name="T"></typeparam>
#pragma warning disable CA1710 // Identifiers should have correct suffix
#pragma warning disable CA1010 // Collections should implement generic interface
    public interface IDispatchedBindingList<T> : IBindingList, IDispatchEvents
        where T : IBindingItem
    {
#pragma warning restore CA1010
#pragma warning restore CA1710

        /// <summary>
        /// Gets or Sets the item at the specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        new T this[int index] { get; set; }

        /// <summary>
        /// Gets the item with the specified name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        T this[string name] { get; }

    }

}

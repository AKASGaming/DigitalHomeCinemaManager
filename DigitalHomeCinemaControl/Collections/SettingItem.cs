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

    /// <summary>
    /// Implements a simple class for controller settings.
    /// </summary>
    /// <typeparam name="T">The type of the SettingItems value.</typeparam>
    public class SettingItem<T>
    {

        #region Contructor

        /// <summary>
        /// Create new SettingItem instance.
        /// </summary>
        /// <param name="t">The type of the value.</param>
        /// <param name="value">The value to set.</param>
        public SettingItem(Type t, T value)
        {
            this.Value = value;
            this.Type = t;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the type associated with this SettingItem.
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// Gets or Sets the value of this SettingItem.
        /// </summary>
        public T Value { get; set; }

        #endregion

    }

}

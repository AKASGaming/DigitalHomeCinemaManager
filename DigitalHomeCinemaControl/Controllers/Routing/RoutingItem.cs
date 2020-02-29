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

namespace DigitalHomeCinemaControl.Controllers.Routing
{
    using System;

    /// <summary>
    /// Object used by IRoutingSource devices to send data to the routing engine for processing.
    /// </summary>
    public struct RoutingItem
    {

        #region Contructor

        /// <summary>
        /// Creates a new instance of the RoutingItem class.
        /// </summary>
        /// <param name="source">The name of the source device for this item.</param>
        /// <param name="dataType">The Type of data being sent.</param>
        /// <param name="data">The data associated with this item.</param>
        public RoutingItem(string source, Type dataType, object data)
        {
            this.Source = source;
            this.DataType = dataType;
            this.Data = data;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the IRoutingSource that sent this item.
        /// </summary>
        public string Source { get; private set; }

        /// <summary>
        /// Gets the Type for the data that was sent.
        /// </summary>
        public Type DataType { get; private set; }

        /// <summary>
        /// Gets the data that was sent by the source.
        /// </summary>
        public object Data { get; private set; }

        #endregion

    }

}

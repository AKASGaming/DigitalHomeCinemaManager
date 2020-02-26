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
    /// Sends data to a routing engine for processing and distribution to IRoutingDestinations.
    /// </summary>
    public interface IRoutingSource
    {

        /// <summary>
        /// Notifies the routing engine of new data to process.
        /// </summary>
        event EventHandler<RoutingItem> RouteData;

        /// <summary>
        /// Gets the name of the IRoutingSource.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the Type of the object this source provides.
        /// </summary>
        Type MatchType { get; }

    }

}

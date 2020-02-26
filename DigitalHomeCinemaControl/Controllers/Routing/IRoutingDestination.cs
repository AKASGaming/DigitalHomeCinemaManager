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
    using System.Collections.Generic;

    /// <summary>
    /// Can act as a consumer of routed events from a IRoutingSource.
    /// </summary>
    public interface IRoutingDestination
    {

        /// <summary>
        /// Called by the RoutingEngine when a rule has been matched.
        /// </summary>
        /// <param name="action">The action that was defined in the rule.</param>
        /// <param name="args">The args that were defined in the rule.</param>
        /// <returns>A string result.</returns>
        string RouteAction(string action, object args);

        /// <summary>
        /// Gets the name of the IRoutingDestination.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets a collection of actions and their argument Type for this destination.
        /// </summary>
        IDictionary<string, Type> Actions { get; }

    }

}

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

namespace DigitalHomeCinemaControl.Controllers
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines the interface that serial data controllers implement.
    /// </summary>
    public interface ISerialController : IController
    { 

        /// <summary>
        /// Gets the Comm Port currently in use.
        /// </summary>
        string CommPort { get; }

        /// <summary>
        /// Gets a list of all available Comm Ports.
        /// </summary>
        List<string> CommPorts { get; }

    }

}

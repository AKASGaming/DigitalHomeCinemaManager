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

    /// <summary>
    /// Defines the interface that display controllers implement.
    /// </summary>
    public interface IDisplayController : IController
    {

        /// <summary>
        /// Gets the DisplayType for the controller.
        /// </summary>
        DisplayType DisplayType { get; }

        /// <summary>
        /// Gets the current Lamp hours for DisplayType.Projector,
        /// or operating hours for DisplayType.Lcd instances.
        /// Must raise the PropertyChanged event.
        /// </summary>
        int LampTimer { get; }

        /// <summary>
        /// Gets the current LampStatus for DisplayType.Projector instances.
        /// Must raise the PropertyChanged event.
        /// </summary>
        LampStatus LampStatus { get; }

    }

}

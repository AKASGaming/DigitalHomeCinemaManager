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
    /// Defines the interface that audio processor controllers implement.
    /// </summary>
    public interface IProcessorController : IController
    {

        /// <summary>
        /// Gets the current delay in ms to the audio stream.
        /// Must raise the PropertyChanged event.
        /// </summary>
        int Delay { get; }

        /// <summary>
        /// Gets the current master volume in Db.
        /// Must raise the PropertyChanged event.
        /// </summary>
        decimal MasterVolume { get; }

    }

}

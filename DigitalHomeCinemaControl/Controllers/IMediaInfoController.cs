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
    /// Defines the interface that media info controllers implement.
    /// </summary>
    public interface IMediaInfoController : IController
    {

        /// <summary>
        /// Gets the media info for the specified title.
        /// </summary>
        /// <param name="title">The name of the title to search for.</param>
        /// <param name="year">Optional year the title was releases.</param>
        void GetFeatureInfo(string title, string year = "");

    }

}

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

namespace DigitalHomeCinemaControl.Devices
{
    using DigitalHomeCinemaControl.Controllers;
    using DigitalHomeCinemaControl.Controls;

    /// <summary>
    /// Interface for all devices.
    /// </summary>
    public interface IDevice
    {

        /// <summary>
        /// Gets the Name of the Device.
        /// </summary>
        string Name { get; }

        DeviceType DeviceType { get; }

        /// <summary>
        /// Gets the device controller as T.
        /// </summary>
        /// <typeparam name="T">The Type of IController to return.</typeparam>
        /// <returns>The device controller as type T.</returns>
        T GetController<T>() where T : IController;

        /// <summary>
        /// Gets the device controller.
        /// </summary>
#pragma warning disable CA1721 // Property names should not match get methods
        IController Controller { get; }
#pragma warning restore CA1721

        /// <summary>
        /// Gets the device UI Element.
        /// </summary>
        DeviceControl  UIElement { get; }

    }

}

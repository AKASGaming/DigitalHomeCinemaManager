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
    using System;
    using System.Diagnostics.CodeAnalysis;
    using DigitalHomeCinemaControl.Controllers;
    using DigitalHomeCinemaControl.Controls;

    /// <summary>
    /// Abstract class for devices.
    /// </summary>
    public abstract class Device : IDevice
    {

        #region Members

        private Type controllerType;
        private Type uiElementType;
        private IController controller;
        private DeviceControl uiElement;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new instance of the Device class.
        /// </summary>
        /// <param name="name">Name of the device.</param>
        /// <param name="deviceType">The DeviceType for this device.</param>
        /// <param name="controllerType">The Type of the device controller.</param>
        /// <param name="uiElementType">The Type of the UI element for this device.</param>
        internal Device(string name, DeviceType deviceType, Type controllerType, Type uiElementType)
        {
            this.Name = name;
            this.DeviceType = deviceType;
            this.controllerType = controllerType;
            this.uiElementType = uiElementType;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the device controller as T.
        /// </summary>
        /// <typeparam name="T">The Type of IController to return.</typeparam>
        /// <returns>The device controller as type T.</returns>
        public T GetController<T>() where T : IController
        {
            return (T)this.Controller;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the Name of the Device.
        /// </summary>
        public string Name { get; protected set; }

        public DeviceType DeviceType { get; protected set; }


        /// <summary>
        /// Gets an instance of the device controller.
        /// </summary>
        [SuppressMessage("Naming", "CA1721:Property names should not match get methods", Justification = "<Pending>")]
        public IController Controller
        {
            get {
                if (this.controller == null) {
                    this.controller = (IController)Activator.CreateInstance(this.controllerType);
                }
                return this.controller;
            }
        }

        /// <summary>
        /// Gets an instance of the device UI element.
        /// </summary>
        public DeviceControl UIElement 
        { 
            get {
                if ((this.uiElement == null) && (this.uiElementType != null)) {
                    this.uiElement = (DeviceControl)Activator.CreateInstance(this.uiElementType);
                }
                return this.uiElement;
            } 
        
        }

        #endregion

    }

}

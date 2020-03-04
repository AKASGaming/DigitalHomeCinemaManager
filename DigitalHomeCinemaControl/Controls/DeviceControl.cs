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

namespace DigitalHomeCinemaControl.Controls
{
    using System.ComponentModel;
    using System.Windows.Controls;
    using DigitalHomeCinemaControl.Collections;
    using DigitalHomeCinemaControl.Components;

    /// <summary>
    /// Abstract UserControl that implements the IDeviceControl interface.
    /// All device UI elements should inherit from this class.
    /// </summary>
    public abstract class DeviceControl : UserControl, IDeviceControl
    {

        #region Members

        private IDispatchedBindingList<IBindingItem> dataSource;

        #endregion

        #region Methods

        /// <summary>
        /// Called when the DataSource has been modified.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected abstract void DataSourceListChanged(object sender, ListChangedEventArgs e);

        #endregion

        #region Properties

        /// <summary>
        /// Gets or Sets a generic collection that supports data binding.
        /// </summary>
#pragma warning disable CA2227 // Collection properties should be read only
        public IDispatchedBindingList<IBindingItem> DataSource
        {
            get { return this.dataSource; }
            set {
                this.dataSource = value;
                if (this.dataSource != null) {
                    this.dataSource.ListChanged += DataSourceListChanged;
                }
            }
        }
#pragma warning restore CA2227

        #endregion

    }

}

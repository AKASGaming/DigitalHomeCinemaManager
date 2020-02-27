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

namespace DigitalHomeCinemaManager.Controls.Settings
{
    using System;
    using System.Windows.Controls;
    using DigitalHomeCinemaControl;
    using DigitalHomeCinemaManager.Components;


    /// <summary>
    /// Interaction logic for SourceSettings.xaml
    /// </summary>
    public partial class SourceSettings : UserControl
    {
        public SourceSettings()
        {
            InitializeComponent();

            this.Provider.ItemsSource = DeviceManager.GetProviders(DeviceType.Source);
            this.Provider.SelectedValue = Properties.Settings.Default.SourceDevice;
        }

        protected void OnItemChanged()
        {
            ItemChanged?.Invoke(this, new EventArgs());
        }

        public event EventHandler ItemChanged;

    }

}

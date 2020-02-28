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
    using System.Windows.Controls;
    using DigitalHomeCinemaControl;
    using DigitalHomeCinemaManager.Components;

    /// <summary>
    /// Interaction logic for MediaInfoSettings.xaml
    /// </summary>
    public partial class MediaInfoSettings : SettingsControl
    {

        #region Constructor

        public MediaInfoSettings()
        {
            InitializeComponent();

            this.Provider.ItemsSource = DeviceManager.GetProviders(DeviceType.MediaInfo);
            if (string.IsNullOrEmpty(Properties.Settings.Default.MediaInfoDevice)) {
                this.Enabled.IsChecked = false;
            } else {
                this.Enabled.IsChecked = true;
                this.Provider.SelectedValue = Properties.Settings.Default.MediaInfoDevice;
            }

            this.ApiKey.Text = Properties.DeviceSettings.Default.MediaInfo_ApiKey;
        }

        #endregion

        #region Methods

        public override void SaveChanges()
        {
            if (this.Enabled.IsChecked == true) {
                Properties.Settings.Default.MediaInfoDevice = this.Provider.SelectedValue.ToString();
            } else {
                Properties.Settings.Default.MediaInfoDevice = string.Empty;
            }
            Properties.DeviceSettings.Default.MediaInfo_ApiKey = this.ApiKey.Text;
        }

        private void ProviderSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OnItemChanged();
        }

        private void ApiKeyTextChanged(object sender, TextChangedEventArgs e)
        {
            OnItemChanged();
        }

        private void EnabledChecked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.Enabled.IsChecked == true) {
                this.Provider.IsEnabled = true;
                this.ApiKey.IsEnabled = true;
            } else {
                this.Provider.IsEnabled = false;
                this.ApiKey.IsEnabled = false;
            }
        }

        #endregion

    }

}

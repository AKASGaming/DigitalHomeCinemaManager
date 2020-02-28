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
    public partial class SourceSettings : SettingsControl
    {

        #region Constructor

        public SourceSettings()
        {
            InitializeComponent();

            this.Provider.ItemsSource = DeviceManager.GetProviders(DeviceType.Source);
            if (string.IsNullOrEmpty(Properties.Settings.Default.SourceDevice)) {
                this.Enabled.IsChecked = false;
            } else {
                this.Enabled.IsChecked = true;
                this.Provider.SelectedValue = Properties.Settings.Default.SourceDevice;
            }

            this.Path.Text = Properties.DeviceSettings.Default.Source_Path;
            this.Display.Text = Properties.DeviceSettings.Default.Source_FullscreenDisplay.ToString();
        }

        #endregion

        #region Methods

        public override void SaveChanges()
        {
            if (this.Enabled.IsChecked == true) {
                Properties.Settings.Default.SourceDevice = this.Provider.SelectedValue.ToString();
            } else {
                Properties.Settings.Default.SourceDevice = string.Empty;
            }

            Properties.DeviceSettings.Default.Source_Path = this.Path.Text;
            if (int.TryParse(this.Display.Text, out int i)) {
                Properties.DeviceSettings.Default.Source_FullscreenDisplay = i;
            }
        }

        private void ProviderSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OnItemChanged();
        }

        private void EnabledChecked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.Enabled.IsChecked == true) {
                this.Provider.IsEnabled = true;
                this.Path.IsEnabled = true;
                this.Display.IsEnabled = true;
            } else {
                this.Provider.IsEnabled = false;
                this.Path.IsEnabled = false;
                this.Display.IsEnabled = false;
            }
        }

        private void PathTextChanged(object sender, TextChangedEventArgs e)
        {
            OnItemChanged();
        }

        private void ButtonPathClick(object sender, System.Windows.RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            //if (Directory.Exists(this.PrerollPath.Text)) {
            //    fbd.SelectedPath = this.PrerollPath.Text;
            //} else {
                fbd.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            //}

            System.Windows.Forms.DialogResult result = fbd.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrEmpty(fbd.SelectedPath)) {
                this.Path.Text = fbd.SelectedPath;
                OnItemChanged();
            }
        }

        private void DisplayTextChanged(object sender, TextChangedEventArgs e)
        {
            OnItemChanged();
        }

        #endregion

    }

}

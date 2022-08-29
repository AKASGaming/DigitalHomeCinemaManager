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
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows.Controls;
    using DigitalHomeCinemaControl;
    using DigitalHomeCinemaManager.Components;
    using Microsoft.Win32;

    /// <summary>
    /// Interaction logic for SourceSettings.xaml
    /// </summary>
    public partial class SourceSettings : SettingsControl
    {

        #region Members

        private bool initialized;

        #endregion

        #region Constructor

        public SourceSettings()
        {
            InitializeComponent();

             var pair = new Dictionary<string, string>()
                    {
                        {"MPC Home Cinema", "C:\\Program Files\\MPC-HC\\mpc-hc64.exe"},
                        {"MPC-HC K-Lite Codec", "C:\\Program Files (x86)\\K-Lite Codec Pack\\MPC-HC64\\mpc-hc64.exe"},
                        {"VLC", "C:\\Program Files (x86)\\VideoLAN\\VLC\\vlc.exe"}
                    };

            var sources = DeviceManager.GetProviders(DeviceType.Source);

            foreach (var devcie in sources)
            {
                this.Provider.Items.Add(devcie.ToString());
                Output.WriteLine(devcie);
            }

            if (Properties.Settings.Default.SourceDevice != null && !pair.ContainsKey(Properties.Settings.Default.SourceDevice))
            {
                Properties.Settings.Default.SourceDevice = "MPC Home Cinema";
            } else if (Properties.Settings.Default.SourceDevice != null) {                
                this.Enabled.IsChecked = false;
            } else {

                

                this.Enabled.IsChecked = true;
                this.Provider.SelectedValue = Properties.Settings.Default.SourceDevice.ToString();
            }

            this.Path.Text = pair.TryGetValue(Properties.Settings.Default.SourceDevice, out string value) ? value : "C:\\Program Files\\MPC-HC\\mpc-hc64.exe";
            this.Display.Text = Properties.DeviceSettings.Default.Source_FullscreenDisplay.ToString(CultureInfo.InvariantCulture);

            this.initialized = true;
        }

        #endregion

        #region Methods

        public override void SaveChanges()
        {
            if (this.Enabled.IsChecked == true) {
                Properties.Settings.Default.SourceDevice = Provider.SelectedValue.ToString();
            } else {
                Properties.Settings.Default.SourceDevice = null;
            }

            var pair = new Dictionary<string, string>()
                    {
                        {"MPC Home Cinema", "C:\\Program Files\\MPC-HC\\mpc-hc64.exe"},
                        {"MPC-HC K-Lite Codec", "C:\\Program Files (x86)\\K-Lite Codec Pack\\MPC-HC64\\mpc-hc64.exe"},
                        {"VLC", "C:\\Program Files (x86)\\VideoLAN\\VLC\\vlc.exe"}
                    };

            Properties.DeviceSettings.Default.Source_Path = pair[Provider.SelectedValue.ToString()];
            if (int.TryParse(this.Display.Text, out int i)) {
                Properties.DeviceSettings.Default.Source_FullscreenDisplay = i;
            }
        }

        private void ProviderSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var pair = new Dictionary<string, string>()
                {
                    {"MPC Home Cinema", "C:\\Program Files\\MPC-HC\\mpc-hc64.exe"},
                    {"MPC-HC K-Lite Codec", "C:\\Program Files (x86)\\K-Lite Codec Pack\\MPC-HC64\\mpc-hc64.exe"},
                    {"VLC", "C:\\Program Files (x86)\\VideoLAN\\VLC\\vlc.exe"}
                };

            Output.WriteLine(Provider.SelectedValue.ToString());

            Properties.Settings.Default.SourceDevice = Provider.SelectedValue.ToString();
            Path.Text = pair[Provider.SelectedValue.ToString()];
            Properties.DeviceSettings.Default.Source_Path = pair[Provider.SelectedValue.ToString()];

            if (this.initialized)
            {
                OnItemChanged();
            }
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

            if (this.initialized) {
                OnItemChanged();
            }
        }

        private void PathTextChanged(object sender, TextChangedEventArgs e)
        {
            OnItemChanged();
        }

        private void ButtonPathClick(object sender, System.Windows.RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog() {
                Filter = Properties.Resources.FILTER_PROGRAMS,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
            };

            if (ofd.ShowDialog() == true) {
                this.Path.Text = ofd.FileName;
                OnItemChanged();
            }
        }

        private void DisplayTextChanged(object sender, TextChangedEventArgs e)
        {
            OnItemChanged();
        }

        private void DisplayPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = !(e.Key.IsInteger() || e.Key == System.Windows.Input.Key.Subtract || e.Key == System.Windows.Input.Key.OemMinus);
        }

        #endregion

    }

}

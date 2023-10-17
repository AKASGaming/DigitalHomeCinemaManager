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
    using System.Web.UI;
    using System.Windows.Controls;
    using System.Windows.Forms;
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
        public List<string> DisplayIDs = new List<string>();

        #endregion

        #region Constructor

        public SourceSettings()
        {
            InitializeComponent();

             var pair = new Dictionary<string, string>()
                    {
                        {"Default", "C:\\Program Files\\MPC-HC\\mpc-hc64.exe"},
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

            if(Properties.Settings.Default.SourceDeviceEnabled == true)
            {
                this.Enabled.IsChecked = true;
                this.Provider.IsEnabled = true;
                this.Provider.SelectedValue = Properties.Settings.Default.SourceDevice.ToString();
                this.Path.IsEnabled = true;
                this.Path.Text = pair.TryGetValue(Properties.Settings.Default.SourceDevice, out string value) ? value : pair["Default"];
                this.Displays.IsEnabled = true;
                foreach (var screen in Screen.AllScreens)
                {
                    this.Displays.Items.Add(screen.DeviceFriendlyName());
                    DisplayIDs.Add(screen.DeviceName);
                }
                //DisplayIDs.ForEach(i => Console.Write("{0}\t", i));
                this.Displays.SelectedIndex = Properties.DeviceSettings.Default.Source_FullscreenDisplay;

                var item = (this.Displays.SelectedIndex == -1 ? null : DisplayIDs[Properties.DeviceSettings.Default.Source_FullscreenDisplay]);
                Console.WriteLine(item);
                Properties.DeviceSettings.Default.Source_FullscreenDisplayID = item;
                this.DisplayIDText.IsEnabled = true;
                this.DisplayIDText.Text = Properties.DeviceSettings.Default.Source_FullscreenDisplayID;

                this.Port.IsEnabled = true;
                this.PathButton.IsEnabled = true;

                this.PasswordText.IsEnabled = true;
                this.PasswordText.Text = Properties.DeviceSettings.Default.Source_VLCPassword;
                this.PasswordLabel.IsEnabled = true;
            }
            if(Properties.Settings.Default.SourceDeviceEnabled == false)
            {
                this.Enabled.IsChecked = false;
                this.Provider.IsEnabled = false;
                this.Path.IsEnabled = false;
                this.Path.Text = string.Empty;
                this.Displays.IsEnabled = false;
                this.PathButton.IsEnabled = false;
                this.Port.IsEnabled = false;
                this.DisplayIDText.IsEnabled = false;
                this.PasswordText.IsEnabled = false;
                this.PasswordLabel.IsEnabled = false;
            }

            this.initialized = true;
        }

        #endregion

        #region Methods

        public override void SaveChanges()
        {
            var pair = new Dictionary<string, string>()
                    {
                        {"MPC Home Cinema", "C:\\Program Files\\MPC-HC\\mpc-hc64.exe"},
                        {"MPC-HC K-Lite Codec", "C:\\Program Files (x86)\\K-Lite Codec Pack\\MPC-HC64\\mpc-hc64.exe"},
                        {"VLC", "C:\\Program Files (x86)\\VideoLAN\\VLC\\vlc.exe"}
                    };

            if (this.Enabled.IsChecked == true)
            {
                Properties.Settings.Default.SourceDevice = Provider.SelectedValue.ToString();
                Properties.Settings.Default.SourceDeviceEnabled = true;
                Properties.DeviceSettings.Default.Source_Path = pair[Provider.SelectedValue.ToString()];
                Properties.DeviceSettings.Default.Source_FullscreenDisplay = this.Displays.SelectedIndex;
                Properties.DeviceSettings.Default.Source_Port = int.Parse(this.Port.Text);
                if(Provider.SelectedValue.ToString() == "VLC")
                {
                    Properties.DeviceSettings.Default.Source_VLCPassword = this.PasswordText.Text;
                }
            }
            else
            {
                Properties.Settings.Default.SourceDevice = null;
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
            InitializeComponent();
        }

        private void EnabledChecked(object sender, System.Windows.RoutedEventArgs e)
        {
            Properties.Settings.Default.SourceDeviceEnabled = true;
            var pair = new Dictionary<string, string>()
                {
                    {"Default", "C:\\Program Files\\MPC-HC\\mpc-hc64.exe"},
                    {"MPC Home Cinema", "C:\\Program Files\\MPC-HC\\mpc-hc64.exe"},
                    {"MPC-HC K-Lite Codec", "C:\\Program Files (x86)\\K-Lite Codec Pack\\MPC-HC64\\mpc-hc64.exe"},
                    {"VLC", "C:\\Program Files (x86)\\VideoLAN\\VLC\\vlc.exe"}
                };

            this.Provider.IsEnabled = true;
            this.Provider.SelectedValue = Properties.Settings.Default.SourceDevice.ToString();

            if(Properties.Settings.Default.SourceDevice == "VLC")
            {
                this.PasswordLabel.IsEnabled = true;
                this.PasswordText.IsEnabled = true;
            } else
            {
                this.PasswordLabel.IsEnabled = false;
                this.PasswordText.IsEnabled = false;
            }

            this.Path.IsEnabled = true;
            this.Path.Text = pair.TryGetValue(Properties.Settings.Default.SourceDevice, out string value) ? value : pair["Default"];

            this.Displays.IsEnabled = true;
            this.Displays.SelectedIndex = Properties.DeviceSettings.Default.Source_FullscreenDisplay;

            this.Port.IsEnabled = true;
            this.Port.Text = Properties.DeviceSettings.Default.Source_Port.ToString();

            this.PathButton.IsEnabled = true;

            OnItemChanged();
            InitializeComponent();
        }

        private void EnabledUnchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            Properties.Settings.Default.SourceDeviceEnabled = false;
            this.Provider.IsEnabled = false;

            this.Path.IsEnabled = false;
            this.Path.Text = string.Empty;

            this.Displays.IsEnabled = false;

            this.Port.IsEnabled = false;
            this.Port.Text = string.Empty;

            this.PathButton.IsEnabled = false;

            OnItemChanged();
            InitializeComponent();
        }

        private void PathTextChanged(object sender, TextChangedEventArgs e)
        {
            OnItemChanged();
        }
        private void PortTextChanged(object sender, TextChangedEventArgs e)
        {
            OnItemChanged();
        }

        private void ButtonPathClick(object sender, System.Windows.RoutedEventArgs e)
        {
            var ofd = new Microsoft.Win32.OpenFileDialog() {
                Filter = Properties.Resources.FILTER_PROGRAMS,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
            };

            if (ofd.ShowDialog() == true) {
                this.Path.Text = ofd.FileName;
                OnItemChanged();
            }
        }

        #endregion

        private void Displays_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Properties.DeviceSettings.Default.Source_FullscreenDisplay = this.Displays.SelectedIndex;

            if (this.initialized == true)
            {
                var item = DisplayIDs[Properties.DeviceSettings.Default.Source_FullscreenDisplay];
                Properties.DeviceSettings.Default.Source_FullscreenDisplayID = item;
            }

            this.DisplayIDText.Text = Properties.DeviceSettings.Default.Source_FullscreenDisplayID;
            OnItemChanged();
            InitializeComponent();
        }

        private void PasswordText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.initialized == true)
            {
                Properties.DeviceSettings.Default.Source_VLCPassword = this.PasswordText.Text;
            }
            OnItemChanged();
            InitializeComponent();
        }
    }
}

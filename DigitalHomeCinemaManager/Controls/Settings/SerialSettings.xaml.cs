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
    using System.IO.Ports;
    using System.Windows.Controls;
    using DigitalHomeCinemaControl;
    using DigitalHomeCinemaManager.Components;

    /// <summary>
    /// Interaction logic for SerialSettings.xaml
    /// </summary>
    public partial class SerialSettings : SettingsControl
    {

        #region Constructor

        public SerialSettings()
        {
            InitializeComponent();

            this.Provider.ItemsSource = DeviceManager.GetProviders(DeviceType.Serial);
            this.Port.ItemsSource = GetAllPorts();
            this.Parity.ItemsSource = Enum.GetValues(typeof(Parity));
            this.StopBits.ItemsSource = Enum.GetValues(typeof(StopBits));

            if (string.IsNullOrEmpty(Properties.Settings.Default.SerialDevice)) {
                this.Enabled.IsChecked = false;
            } else {
                this.Enabled.IsChecked = true;
                this.Provider.SelectedValue = Properties.Settings.Default.SerialDevice;
            }
            this.Port.SelectedValue = Properties.DeviceSettings.Default.Serial_Port;
            this.ReadDelay.Text = Properties.DeviceSettings.Default.Serial_ReadDelay.ToString();
            this.BaudRate.Text = Properties.DeviceSettings.Default.Serial_BaudRate.ToString();
            this.DataBits.Text = Properties.DeviceSettings.Default.Serial_DataBits.ToString();
            this.Parity.SelectedValue = Properties.DeviceSettings.Default.Serial_Parity;
            this.StopBits.SelectedValue = Properties.DeviceSettings.Default.Serial_StopBits;
        }

        #endregion

        #region Methods

        public override void SaveChanges()
        {
            if (this.Enabled.IsChecked == true) {
                Properties.Settings.Default.SerialDevice = this.Provider.SelectedValue.ToString();
            } else {
                Properties.Settings.Default.SerialDevice = string.Empty;
            }
            Properties.DeviceSettings.Default.Serial_Port = this.Port.SelectedValue.ToString();
            if (int.TryParse(this.ReadDelay.Text, out int rd)) {
                Properties.DeviceSettings.Default.Serial_ReadDelay = rd;
            }
            if (int.TryParse(this.BaudRate.Text, out int br)) {
                Properties.DeviceSettings.Default.Serial_BaudRate = br;
            }
            if (int.TryParse(this.DataBits.Text, out int db)) {
                Properties.DeviceSettings.Default.Serial_DataBits = db;
            }
            if (Enum.TryParse<Parity>(this.Parity.SelectedValue.ToString(), out Parity p)) {
                Properties.DeviceSettings.Default.Serial_Parity = p;
            }
            if (Enum.TryParse<StopBits>(this.StopBits.SelectedValue.ToString(), out StopBits sb)) {
                Properties.DeviceSettings.Default.Serial_StopBits = sb;
            }
        }

        private List<string> GetAllPorts()
        {
            var result = new List<string>();
            foreach (string portName in SerialPort.GetPortNames()) {
                result.Add(portName);
            }

            return result;
        }
        private void ProviderSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OnItemChanged();
        }

        private void EnabledChecked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.Enabled.IsChecked == true) {
                this.Provider.IsEnabled = true;
                this.Port.IsEnabled = true;
                this.ReadDelay.IsEnabled = true;
                this.BaudRate.IsEnabled = true;
                this.DataBits.IsEnabled = true;
                this.Parity.IsEnabled = true;
                this.StopBits.IsEnabled = true;
            } else {
                this.Provider.IsEnabled = false;
                this.Port.IsEnabled = false;
                this.ReadDelay.IsEnabled = false;
                this.BaudRate.IsEnabled = false;
                this.DataBits.IsEnabled = false;
                this.Parity.IsEnabled = false;
                this.StopBits.IsEnabled = false;
            }
        }

        private void PortSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OnItemChanged();
        }

        private void ReadDelayTextChanged(object sender, TextChangedEventArgs e)
        {
            OnItemChanged();
        }

        private void BaudRateTextChanged(object sender, TextChangedEventArgs e)
        {
            OnItemChanged();
        }

        private void DataBitsTextChanged(object sender, TextChangedEventArgs e)
        {
            OnItemChanged();
        }

        private void ParitySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OnItemChanged();
        }

        private void StopBitsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OnItemChanged();
        }

        #endregion

    }

}

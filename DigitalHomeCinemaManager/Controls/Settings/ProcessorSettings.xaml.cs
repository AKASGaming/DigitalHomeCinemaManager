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
    using System.Globalization;
    using System.Windows.Controls;
    using DigitalHomeCinemaControl;
    using DigitalHomeCinemaManager.Components;

    /// <summary>
    /// Interaction logic for ProcessorSettings.xaml
    /// </summary>
    public partial class ProcessorSettings : SettingsControl
    {

        #region Members

        private bool initialized;

        #endregion

        #region Constructor

        public ProcessorSettings()
        {
            InitializeComponent();

            this.Provider.ItemsSource = DeviceManager.GetProviders(DeviceType.Processor);
            if (string.IsNullOrEmpty(Properties.Settings.Default.ProcessorDevice)) {
                this.Enabled.IsChecked = false;
            } else {
                this.Enabled.IsChecked = true;
                this.Provider.SelectedValue = Properties.Settings.Default.ProcessorDevice;
            }

            this.Host.Text = Properties.DeviceSettings.Default.Processor_Host;
            this.Port.Text = Properties.DeviceSettings.Default.Processor_Port.ToString(CultureInfo.InvariantCulture);
            if (Properties.DeviceSettings.Default.Processor_HideUnusedOutputs) {
                this.HideOutputs.SelectedIndex = 0;
            } else {
                this.HideOutputs.SelectedIndex = 1;
            }

            this.initialized = true;
        }

        #endregion

        #region Methods

        public override void SaveChanges()
        {
            if (this.Enabled.IsChecked == true) {
                Properties.Settings.Default.ProcessorDevice = this.Provider.SelectedValue.ToString();
            } else {
                Properties.Settings.Default.ProcessorDevice = string.Empty;
            }
            if (this.Host.Text.IsIpAddress()) {
                Properties.DeviceSettings.Default.Processor_Host = this.Host.Text;
            }
            if (int.TryParse(this.Port.Text, out int i)) {
                Properties.DeviceSettings.Default.Processor_Port = i;
            }
            if (bool.TryParse(this.HideOutputs.SelectedValue.ToString(), out bool b)) {
                Properties.DeviceSettings.Default.Processor_HideUnusedOutputs = b;
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
                this.Host.IsEnabled = true;
                this.Port.IsEnabled = true;
                this.HideOutputs.IsEnabled = true;
            } else {
                this.Provider.IsEnabled = false;
                this.Host.IsEnabled = false;
                this.Port.IsEnabled = false;
                this.HideOutputs.IsEnabled = false;
            }

            if (this.initialized) {
                OnItemChanged();
            }
        }

        private void HostTextChanged(object sender, TextChangedEventArgs e)
        {
            OnItemChanged();
        }

        private void PortTextChanged(object sender, TextChangedEventArgs e)
        {
            OnItemChanged();
        }

        private void HostPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = !e.Key.IsNumeric();
        }

        private void PortPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = !e.Key.IsInteger();
        }

        private void HideOutputsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OnItemChanged();
        }

        #endregion

    }

}

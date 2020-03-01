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
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using DigitalHomeCinemaControl;
    using DigitalHomeCinemaManager.Components;

    /// <summary>
    /// Interaction logic for InputSwitchSettings.xaml
    /// </summary>
    public partial class InputSwitchSettings : SettingsControl
    {

        #region Members

        private ObservableCollection<KeyValuePair<string, string>> customInputs;

        #endregion

        #region Constructor

        public InputSwitchSettings()
        {
            InitializeComponent();

            this.Provider.ItemsSource = DeviceManager.GetProviders(DeviceType.InputSwitch);
            if (string.IsNullOrEmpty(Properties.Settings.Default.InputSwitchDevice)) {
                this.Enabled.IsChecked = false;
            } else {
                this.Enabled.IsChecked = true;
                this.Provider.SelectedValue = Properties.Settings.Default.InputSwitchDevice;
            }
            this.Host.Text = Properties.DeviceSettings.Default.InputSwitch_Host;
            this.Port.Text = Properties.DeviceSettings.Default.InputSwitch_Port.ToString(CultureInfo.InvariantCulture);

            if (Properties.DeviceSettings.Default.InputSwitch_CustomInputs != null) {
                this.customInputs = Properties.DeviceSettings.Default.InputSwitch_CustomInputs.ToObservableCollection();
            } else {
                this.customInputs = new ObservableCollection<KeyValuePair<string, string>>();
            }
            this.Inputs.ItemsSource = this.customInputs;
        }

        #endregion

        #region Methods

        public override void SaveChanges()
        {
            if (this.Enabled.IsChecked == true) {
                Properties.Settings.Default.InputSwitchDevice = this.Provider.SelectedValue.ToString();
            } else {
                Properties.Settings.Default.InputSwitchDevice = string.Empty;
            }
            Properties.DeviceSettings.Default.InputSwitch_Host = this.Host.Text;
            if (int.TryParse(this.Port.Text, out int i)) {
                Properties.DeviceSettings.Default.InputSwitch_Port = i;
            }
            Properties.DeviceSettings.Default.InputSwitch_CustomInputs = this.customInputs.ToStringCollection();
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
                this.Inputs.IsEnabled = true;
            } else {
                this.Provider.IsEnabled = false;
                this.Host.IsEnabled = false;
                this.Port.IsEnabled = false;
                this.Inputs.IsEnabled = false;
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

        private void InputsMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (this.Inputs.SelectedValue == null) { return; }

            var kvp = (KeyValuePair<string, string>)this.Inputs.SelectedValue;
            KeyValueEditor editor = new KeyValueEditor(kvp.Key, kvp.Value) {
                Owner = Window.GetWindow(this),
                Title = "Edit Custom Input"
            };

            if (editor.ShowDialog() == true) {
                this.customInputs.RemoveAt(this.Inputs.SelectedIndex);
                this.customInputs.Add(new KeyValuePair<string, string>(editor.Key, editor.Value));
                OnItemChanged();
            }
        }

        private void InputsAddClick(object sender, System.Windows.RoutedEventArgs e)
        {
            KeyValueEditor editor = new KeyValueEditor(null, null) {
                Owner = Window.GetWindow(this),
                Title = "New Custom Input"
            };

            if (editor.ShowDialog() == true) {
                this.customInputs.Add(new KeyValuePair<string, string>(editor.Key, editor.Value));
                OnItemChanged();
            }
        }

        private void InputsDeleteClick(object sender, System.Windows.RoutedEventArgs e)
        {
            int selected = this.Inputs.SelectedIndex;
            if (selected < 0) { return; }

            this.customInputs.RemoveAt(selected);
            OnItemChanged();
        }

        #endregion

    }

}

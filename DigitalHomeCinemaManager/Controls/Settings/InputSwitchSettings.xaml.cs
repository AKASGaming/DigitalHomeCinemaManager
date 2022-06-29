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
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using DigitalHomeCinemaControl;
    using DigitalHomeCinemaControl.Controllers;
    using DigitalHomeCinemaControl.Devices;
    using DigitalHomeCinemaManager.Components;

    /// <summary>
    /// Interaction logic for InputSwitchSettings.xaml
    /// </summary>
    public partial class InputSwitchSettings : SettingsControl
    {

        #region Members

        private bool initialized;
        private Type customInputType;
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
                GetCustomTypes();
            }
            this.Host.Text = Properties.DeviceSettings.Default.InputSwitch_Host;
            this.Port.Text = Properties.DeviceSettings.Default.InputSwitch_Port.ToString(CultureInfo.InvariantCulture);

            if (Properties.DeviceSettings.Default.InputSwitch_CustomInputs != null) {
                this.customInputs = Properties.DeviceSettings.Default.InputSwitch_CustomInputs.ToObservableCollection();
            } else {
                this.customInputs = new ObservableCollection<KeyValuePair<string, string>>();
            }
            this.Inputs.ItemsSource = this.customInputs;
            this.initialized = true;
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
            if (this.Host.Text.IsIpAddress()) {
                Properties.DeviceSettings.Default.InputSwitch_Host = this.Host.Text;
            }
            if (int.TryParse(this.Port.Text, out int i)) {
                Properties.DeviceSettings.Default.InputSwitch_Port = i;
            }
            Properties.DeviceSettings.Default.InputSwitch_CustomInputs = this.customInputs.ToStringCollection();
        }

        private void GetCustomTypes()
        {
            string providerName = this.Provider.SelectedValue.ToString();
            if (string.IsNullOrEmpty(providerName)) { return; }

            Debug.Assert(SwitchDevice.Items.ContainsKey(providerName));

            var controller = SwitchDevice.Items[providerName].Controller;
            if (controller is ISupportCustomNames customNameController) {
                foreach (var customType in customNameController.CustomNameTypes) {
                    switch (customType.Key) {
                        case "Input": this.customInputType = customType.Value; break;
                    }
                } // foreach

                this.Inputs.IsEnabled = true;
            } else {
                this.Inputs.IsEnabled = false;
            }
        }

        private void ProviderSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.Provider.SelectedValue == null) { return; }

            OnItemChanged();
            GetCustomTypes();
        }

        private void EnabledChecked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.Enabled.IsChecked == true) {
                this.Provider.IsEnabled = true;
                this.Host.IsEnabled = true;
                this.Port.IsEnabled = true;
                if (this.customInputType != null) {
                    this.Inputs.IsEnabled = true;
                }
            } else {
                this.Provider.IsEnabled = false;
                this.Host.IsEnabled = false;
                this.Port.IsEnabled = false;
                this.Inputs.IsEnabled = false;
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

        private void InputsMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (this.Inputs.SelectedValue == null) { return; }

            var kvp = (KeyValuePair<string, string>)this.Inputs.SelectedValue;
            var editor = new KeyValueEditor(this.customInputType, kvp.Key, kvp.Value) {
                Owner = Window.GetWindow(this),
                Title = "Edit Custom Input"
            };

            if (editor.ShowDialog() == true) {
                if (!string.IsNullOrEmpty(editor.Key) && !string.IsNullOrEmpty(editor.Value)) {
                    var newKvp = new KeyValuePair<string, string>(editor.Key, editor.Value);
                    if (!this.customInputs.ContainsKey(newKvp.Key)) {
                        this.customInputs.RemoveAt(this.Inputs.SelectedIndex);
                        this.customInputs.Add(newKvp);
                        OnItemChanged();
                    }
                    
                }
            }
        }

        private void InputsAddClick(object sender, System.Windows.RoutedEventArgs e)
        {
            var editor = new KeyValueEditor(this.customInputType) {
                Owner = Window.GetWindow(this),
                Title = "New Custom Input"
            };

            if (editor.ShowDialog() == true) {
                if (!string.IsNullOrEmpty(editor.Key) && !string.IsNullOrEmpty(editor.Value)) {
                    var kvp = new KeyValuePair<string, string>(editor.Key, editor.Value);
                    if (!this.customInputs.ContainsKey(kvp.Key)) {
                        this.customInputs.Add(kvp);
                        OnItemChanged();
                    }
                }
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

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
    using System.Windows;
    using System.Windows.Controls;
    using DigitalHomeCinemaControl;
    using DigitalHomeCinemaManager.Components;

    /// <summary>
    /// Interaction logic for DisplaySettings.xaml
    /// </summary>
    public partial class DisplaySettings : SettingsControl
    {

        #region Members

        private ObservableCollection<KeyValuePair<string, string>> customColorSpace;
        private ObservableCollection<KeyValuePair<string, string>> customGamma;
        private ObservableCollection<KeyValuePair<string, string>> customColorTemp;

        #endregion

        #region Constructor

        public DisplaySettings()
        {
            InitializeComponent();

            this.Provider.ItemsSource = DeviceManager.GetProviders(DeviceType.Display);
            if (string.IsNullOrEmpty(Properties.Settings.Default.DisplayDevice)) {
                this.Enabled.IsChecked = false;
            } else {
                this.Enabled.IsChecked = true;
                this.Provider.SelectedValue = Properties.Settings.Default.DisplayDevice;
            }

            this.Host.Text = Properties.DeviceSettings.Default.Display_Host;
            this.Port.Text = Properties.DeviceSettings.Default.Display_Port.ToString();
            this.CommandDelay.Text = Properties.DeviceSettings.Default.Display_CommandDelay.ToString();

            if (Properties.DeviceSettings.Default.Display_CustomColorSpace != null) {
                this.customColorSpace = Properties.DeviceSettings.Default.Display_CustomColorSpace.ToObservableCollection();
            } else {
                this.customColorSpace = new ObservableCollection<KeyValuePair<string, string>>();
            }
            this.ColorSpace.ItemsSource = this.customColorSpace;

            if (Properties.DeviceSettings.Default.Display_CustomGamma != null) {
                this.customGamma = Properties.DeviceSettings.Default.Display_CustomGamma.ToObservableCollection();
            } else {
                this.customGamma = new ObservableCollection<KeyValuePair<string, string>>();
            }
            this.Gamma.ItemsSource = this.customGamma;

            if (Properties.DeviceSettings.Default.Display_CustomColorTemp != null) {
                this.customColorTemp = Properties.DeviceSettings.Default.Display_CustomColorTemp.ToObservableCollection();
            } else {
                this.customColorTemp = new ObservableCollection<KeyValuePair<string, string>>();
            }
            //this.ColorTemp.ItemsSource = this.customColorTemp;

        }

        #endregion

        #region Methods

        public override void SaveChanges()
        {
            if (this.Enabled.IsChecked == true) {
                Properties.Settings.Default.DisplayDevice = this.Provider.SelectedValue.ToString();
            } else {
                Properties.Settings.Default.DisplayDevice = string.Empty;
            }

            Properties.DeviceSettings.Default.Display_Host = this.Host.Text;
            if (int.TryParse(this.Port.Text, out int p)) {
                Properties.DeviceSettings.Default.Display_Port = p;
            }
            if (int.TryParse(this.CommandDelay.Text, out int d)) {
                Properties.DeviceSettings.Default.Display_CommandDelay = d;
            }

            Properties.DeviceSettings.Default.Display_CustomColorSpace = this.customColorSpace.ToStringCollection();
            Properties.DeviceSettings.Default.Display_CustomGamma = this.customGamma.ToStringCollection();
            Properties.DeviceSettings.Default.Display_CustomColorTemp = this.customColorTemp.ToStringCollection();
        }

        private void EnabledChecked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.Enabled.IsChecked == true) {
                this.Provider.IsEnabled = true;
                this.Host.IsEnabled = true;
                this.Port.IsEnabled = true;
                this.CommandDelay.IsEnabled = true;
                this.ColorSpace.IsEnabled = true;
                this.Gamma.IsEnabled = true;
            } else {
                this.Provider.IsEnabled = false;
                this.Host.IsEnabled = false;
                this.Port.IsEnabled = false;
                this.CommandDelay.IsEnabled = false;
                this.ColorSpace.IsEnabled = false;
                this.Gamma.IsEnabled = false;
            }
        }

        private void ProviderSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OnItemChanged();
        }

        private void HostTextChanged(object sender, TextChangedEventArgs e)
        {
            OnItemChanged();
        }

        private void PortTextChanged(object sender, TextChangedEventArgs e)
        {
            OnItemChanged();
        }

        private void CommandDelayTextChanged(object sender, TextChangedEventArgs e)
        {
            OnItemChanged();
        }

        private void ColorSpaceMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (this.ColorSpace.SelectedValue == null) { return; }

            var kvp = (KeyValuePair<string, string>)this.ColorSpace.SelectedValue;
            KeyValueEditor editor = new KeyValueEditor(kvp.Key, kvp.Value) {
                Owner = Window.GetWindow(this),
                Title = "Edit Custom ColorSpace"
            };

            if (editor.ShowDialog() == true) {
                this.customColorSpace.RemoveAt(this.ColorSpace.SelectedIndex);
                this.customColorSpace.Add(new KeyValuePair<string, string>(editor.Key, editor.Value));
                OnItemChanged();
            }
        }

        private void ColorSpaceAddClick(object sender, System.Windows.RoutedEventArgs e)
        {
            KeyValueEditor editor = new KeyValueEditor(null, null) {
                Owner = Window.GetWindow(this),
                Title = "New Custom ColorSpace"
            };

            if (editor.ShowDialog() == true) {
                this.customColorSpace.Add(new KeyValuePair<string, string>(editor.Key, editor.Value));
                OnItemChanged();
            }
        }

        private void ColorSpaceDeleteClick(object sender, System.Windows.RoutedEventArgs e)
        {
            var selected = this.ColorSpace.SelectedIndex;
            if (selected < 0) { return; }

            this.customColorSpace.RemoveAt(selected);
            OnItemChanged();
        }

        private void GammaMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (this.Gamma.SelectedValue == null) { return; }

            var kvp = (KeyValuePair<string, string>)this.Gamma.SelectedValue;
            KeyValueEditor editor = new KeyValueEditor(kvp.Key, kvp.Value) {
                Owner = Window.GetWindow(this),
                Title = "Edit Custom Gamma"
            };

            if (editor.ShowDialog() == true) {
                this.customGamma.RemoveAt(this.Gamma.SelectedIndex);
                this.customGamma.Add(new KeyValuePair<string, string>(editor.Key, editor.Value));
                OnItemChanged();
            }
        }

        private void GammaAddClick(object sender, System.Windows.RoutedEventArgs e)
        {
            KeyValueEditor editor = new KeyValueEditor(null, null) {
                Owner = Window.GetWindow(this),
                Title = "New Custom Gamma"
            };

            if (editor.ShowDialog() == true) {
                this.customGamma.Add(new KeyValuePair<string, string>(editor.Key, editor.Value));
                OnItemChanged();
            }

        }

        private void GammaDeleteClick(object sender, System.Windows.RoutedEventArgs e)
        {
            var selected = this.Gamma.SelectedIndex;
            if (selected < 0) { return; }

            this.customGamma.RemoveAt(selected);
            OnItemChanged();
        }

        #endregion

    }

}

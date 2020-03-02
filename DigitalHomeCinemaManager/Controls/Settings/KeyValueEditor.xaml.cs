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
    using System.Windows;

    /// <summary>
    /// Interaction logic for KeyValueEditor.xaml
    /// </summary>
    public partial class KeyValueEditor : Window
    {

        #region Constructor

        public KeyValueEditor(string name, string value)
        {
            InitializeComponent();

            if (!string.IsNullOrEmpty(name)) {
                this.key.Text = name;
            }
            if (!string.IsNullOrEmpty(value)) {
                this.value.Text = value;
            }

        }

        #endregion

        #region Methods

        private void ButtonOkClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            Close();
        }

        private void ButtonCancelClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }

        public string Key
        {
            get { return this.key.Text; }
        }

#pragma warning disable CA1721 // Property name confusing given GetValue method
        public string Value
        {
            get { return this.value.Text; }
        }
#pragma warning restore CA1721

        #endregion

    }

}

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
    using System.Diagnostics;
    using System.Windows;

    /// <summary>
    /// Interaction logic for KeyValueEditor.xaml
    /// </summary>
    public partial class KeyValueEditor : Window
    {

        #region Members

        private Type keyType;

        #endregion

        #region Constructor

        public KeyValueEditor(Type type, string name, string value)
        {
#pragma warning disable CA1062 // Validate arguments of public methods
            Debug.Assert(type != null);
            Debug.Assert(type.IsEnum);
#pragma warning restore CA1062

            InitializeComponent();

            this.keyType = type;

            this.key.ItemsSource = GetEnumValues(this.keyType);

            if (!string.IsNullOrEmpty(name)) {
                this.key.SelectedValue = name;
            }
            if (!string.IsNullOrEmpty(value)) {
                this.value.Text = value;
            }

        }

        public KeyValueEditor(Type type)
            : this(type, null, null)
        { }

        #endregion

        #region Methods

        private List<string> GetEnumValues(Type type)
        {
            var result = new List<string>();

            string[] names = Enum.GetNames(type);
            foreach (string s in names) {
                if (s.Equals("Unknown", StringComparison.OrdinalIgnoreCase) ||
                    s.Equals("NotApplicable", StringComparison.OrdinalIgnoreCase)) { continue; }

                result.Add(s);
            }

            return result;
        }

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
            get { return (this.key.SelectedValue != null)? this.key.SelectedValue.ToString() : string.Empty; }
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

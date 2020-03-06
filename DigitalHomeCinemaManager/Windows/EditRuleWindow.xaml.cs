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

namespace DigitalHomeCinemaManager.Windows
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using DigitalHomeCinemaControl;
    using DigitalHomeCinemaControl.Controllers.Routing;
    using DigitalHomeCinemaManager.Components;

    /// <summary>
    /// Interaction logic for EditRuleWindow.xaml
    /// </summary>
    internal partial class EditRuleWindow : Window
    {

        #region Members

        private RoutingEngine router;
        private MatchAction rule;
        private bool initializing = true;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new instance of the EditRuleWindow.
        /// </summary>
        /// <param name="router">A reference to the current RoutingEngine instance.</param>
        /// <param name="rule">Either a MatchAction to edit, or null to crate a new MatchAction.</param>
        public EditRuleWindow(RoutingEngine router, MatchAction rule)
        {
            this.router = router;
            InitializeComponent();

            this.cmbSource.ItemsSource = this.router.Sources;
            this.cmbDest.ItemsSource = this.router.Destinations;

            if (rule != null) {
                this.rule = rule;
                this.cmbSource.SelectedValue = rule.MatchSource;
                if (rule.GetMatchType().IsEnum) {
                    ((ComboBox)this.matchBorder.Child).SelectedValue = rule.Match;
                } else {
                    ((TextBox)this.matchBorder.Child).Text = rule.Match.ToString();
                }
                this.cmbDest.SelectedValue = rule.ActionDestination;
                this.cmbAction.SelectedValue = rule.Action.ToString(CultureInfo.InvariantCulture);
                if (rule.GetArgsType().IsEnum) {
                    ((ComboBox)this.argsBorder.Child).SelectedValue = rule.Args;
                } else {
                    ((TextBox)this.argsBorder.Child).Text = rule.Args.ToString();
                }
                this.CheckEnabled.IsChecked = rule.Enabled;
            } else {
                this.Title = Properties.Resources.NEW_RULE;
                this.rule = new MatchAction();
                this.matchBorder.IsEnabled = false;
                this.cmbAction.IsEnabled = false;
                this.argsBorder.IsEnabled = false;
            }

            this.initializing = false;
            CheckOK();
        }

        #endregion

        #region Methods

        private void CheckOK()
        {
            // Make sure MatchAction is fully defined before we
            // allow the user to save it.

            if ((this.rule != null) &&
                !string.IsNullOrEmpty(this.rule.MatchSource) &&
                (this.rule.Match != null) &&
                !string.IsNullOrEmpty(this.rule.MatchType) &&
                !string.IsNullOrEmpty(this.rule.ActionDestination) &&
                !string.IsNullOrEmpty(this.rule.Action) &&
                (this.rule.Args != null) &&
                !string.IsNullOrEmpty(this.rule.ArgsType)) {

                this.Ok.IsEnabled = true;
            } else {
                this.Ok.IsEnabled = false;
            }
        }

        private void SourceSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((this.cmbSource.SelectedItem != null) && 
                (this.cmbSource.SelectedItem is KeyValuePair<string, Type> kvp) &&
                kvp.Value.IsEnum) {

                // This source specifies an Enum for it's Match value
                // Create a ComboBox dynamically and add it to the window.
                ComboBox combo = new ComboBox() {
                    Name = "txtMatch",
                    ItemsSource = Enum.GetValues(kvp.Value), // bind the TextBox directly to the enum
                };
                combo.SelectionChanged += (s, ea) => {
                    this.rule.Match = ((ComboBox)s).SelectedItem; // store the selected value
                    CheckOK();
                };

                this.matchBorder.Child = combo;
            } else {
                // The source Match value is a simple type
                // Create a TextBox dynamically and add it to the window.
                TextBox text = new TextBox() {
                    Name = "txtMatch",
                    Height = 22,
                };
                text.LostFocus += (s, ea) => {
                    this.rule.Match = ((TextBox)s).Text;
                    CheckOK();
                };
                this.matchBorder.Child = text;
            }
            
            if (this.cmbSource.SelectedItem != null) {
                this.matchBorder.IsEnabled = true;
                if (!this.initializing) {
                    this.rule.Match = null;
                    this.rule.MatchSource = ((KeyValuePair<string, Type>)this.cmbSource.SelectedItem).Key;
                }
            } else {
                this.matchBorder.IsEnabled = false;
                if (!this.initializing) {
                    this.rule.MatchSource = string.Empty;
                }
            }
            
            CheckOK();
        }

        private void DestinationSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((this.cmbDest.SelectedItem != null) &&
                (this.cmbDest.SelectedItem is KeyValuePair<string, IDictionary<string,Type>> kvp)) {

                // New destination selected.
                // Repopulate the Action ComboBox with new values.
                this.cmbAction.ItemsSource = kvp.Value;
                this.cmbAction.IsEnabled = true;
                if (!this.initializing) {
                    this.rule.ActionDestination = kvp.Key;
                    this.rule.Action = string.Empty;
                    this.rule.Args = null;
                }
            } else {
                this.cmbAction.ItemsSource = null;
                this.cmbAction.IsEnabled = false;
                if (!this.initializing) {
                    this.rule.ActionDestination = string.Empty;
                    this.rule.Action = string.Empty;
                    this.rule.Args = null;
                }
            }
            CheckOK();
        }

        private void ActionSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.cmbAction.SelectedItem == null) {
                this.argsBorder.IsEnabled = false;
                if (!this.initializing) {
                    this.rule.Action = string.Empty;
                    this.rule.Args = null;
                }
                return; 
            }

            if (this.cmbAction.SelectedItem is KeyValuePair<string, Type> kvp) {
                if ((kvp.Value != null) && kvp.Value.IsEnum) {
                    // The destination has specified an Enum for this Action
                    // Create a ComboBox dynamically and add it to the window.
                    ComboBox combo = new ComboBox() {
                        Name = "txtArgs",
                        ItemsSource = Enum.GetValues(kvp.Value), // bind directly to the enum
                    };
                    combo.SelectionChanged += (s, ea) => {
                        this.rule.Args = ((ComboBox)s).SelectedItem; // store the selected value
                        CheckOK();
                    };
                    this.argsBorder.Child = combo;
                } else {
                    // The destination will accept a simple value
                    // Create a TextBox dynamically and add it to the window.
                    TextBox text = new TextBox() {
                        Name = "txtArgs",
                        Height = 22,
                    };
                    text.TextChanged += (s, ea) => {
                        this.rule.Args = ParseType(((TextBox)s).Text, kvp.Value);
                        CheckOK();
                    };
                    this.argsBorder.Child = text;
                }

            }

            this.argsBorder.IsEnabled = true;
            if (!this.initializing) {
                this.rule.Action = ((KeyValuePair<string, Type>)this.cmbAction.SelectedItem).Key;
                this.rule.Args = null;
            }

            CheckOK();
        }

        private void OkClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            Close();
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }

        private void CheckBoxChecked(object sender, RoutedEventArgs e)
        {
            this.rule.Enabled = (bool)this.CheckEnabled.IsChecked;
        }

        private static object ParseType(string data, Type type)
        {
            object result;

            if (type == null) { return data; }

            if (type == typeof(int) && int.TryParse(data, out int i)) {
                result = i;
            } else if (type == typeof(decimal) && decimal.TryParse(data, out decimal m)) {
                result = m;
            } else if (type == typeof(double) && double.TryParse(data, out double d)) {
                result = d;
            } else if (type == typeof(bool) && bool.TryParse(data, out bool b)) {
                result = b;
            } else {
                result = data;
            }

            return result;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the MatchAction edited or created by this instance.
        /// </summary>
        public MatchAction Rule
        {
            get { return this.rule; }
        }

        #endregion

    }

}

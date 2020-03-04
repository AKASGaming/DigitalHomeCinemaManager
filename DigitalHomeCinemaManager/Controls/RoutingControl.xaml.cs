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

namespace DigitalHomeCinemaManager.Controls
{
    using System;
    using System.Collections;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for RoutingControl.xaml
    /// </summary>
    public partial class RoutingControl : UserControl
    {

        #region Constructor

        public RoutingControl()
        {
            InitializeComponent();
        }

        #endregion

        #region Methods

        private void ListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.lstRules.SelectedIndex < 0) {
                this.MenuItemDelete.IsEnabled = false;
                this.MenuItemDown.IsEnabled = false;
                this.MenuItemUp.IsEnabled = false;
            } else {
                this.MenuItemDelete.IsEnabled = true;

                if (this.lstRules.Items.Count == 1) {
                    this.MenuItemDown.IsEnabled = false;
                    this.MenuItemUp.IsEnabled = false;
                } else {
                    this.MenuItemDown.IsEnabled = true;
                    this.MenuItemUp.IsEnabled = true;
                }

                if (this.lstRules.SelectedIndex == 0) {
                    this.MenuItemUp.IsEnabled = false;
                } else if (this.lstRules.SelectedIndex == (this.lstRules.Items.Count - 1)) {
                    this.MenuItemDown.IsEnabled = false;
                }
            }
        }

        private void ListMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (this.lstRules.SelectedIndex < 0) { return; }

            ListDoubleClick?.Invoke(sender, new SelectedItemChangedEventArgs(this.lstRules.SelectedItem));
        }

        private void RulesAddClick(object sender, System.Windows.RoutedEventArgs e)
        {
            ListAddClick?.Invoke(sender, new SelectedItemChangedEventArgs(null));
        }

        private void RulesDeleteClick(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.lstRules.SelectedIndex < 0) { return; }

            ListRemoveClick?.Invoke(sender, new SelectedItemChangedEventArgs(this.lstRules.SelectedItem));
        }

        private void RulesMoveUpClick(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.lstRules.SelectedIndex < 0) { return; }

            ListMoveItemClick?.Invoke(sender, new MoveSelectedItemEventArgs(this.lstRules.SelectedItem, -1));
        }

        private void RulesMoveDownClick(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.lstRules.SelectedIndex < 0) { return; }

            ListMoveItemClick?.Invoke(sender, new MoveSelectedItemEventArgs(this.lstRules.SelectedItem, 1));
        }

        #endregion

        #region Events

        public event EventHandler<SelectedItemChangedEventArgs> ListDoubleClick;

        public event EventHandler<SelectedItemChangedEventArgs> ListAddClick;

        public event EventHandler<SelectedItemChangedEventArgs> ListRemoveClick;

        public event EventHandler<MoveSelectedItemEventArgs> ListMoveItemClick;

        #endregion

        #region Properties

        public IEnumerable ItemsSource
        {
            get { return this.lstRules.ItemsSource; }
            set { this.lstRules.ItemsSource = value; }
        }


        #endregion

    }

}

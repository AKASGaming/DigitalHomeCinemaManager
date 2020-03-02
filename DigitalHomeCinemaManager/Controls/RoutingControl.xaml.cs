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

        private void ListMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            
            ListDoubleClick?.Invoke(sender, this.lstRules.SelectedItem);
        }

        private void RulesAddClick(object sender, System.Windows.RoutedEventArgs e)
        {
            ListAddClick?.Invoke(sender, null);
        }

        private void RulesDeleteClick(object sender, System.Windows.RoutedEventArgs e)
        {
            
            if (this.lstRules.SelectedIndex < 0) { return; }

            ListRemoveClick?.Invoke(sender, this.lstRules.SelectedItem);
        }

        #endregion

        #region Events
        
#pragma warning disable CA1009 // Declare second parameter of event as EventArgs
        public event EventHandler<object> ListDoubleClick;

        public event EventHandler<object> ListAddClick;

        public event EventHandler<object> ListRemoveClick;
#pragma warning restore CA1009

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

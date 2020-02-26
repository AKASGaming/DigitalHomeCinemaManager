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
    using System.Windows.Input;

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

        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            this.lstRules.SelectedValue = null;
            base.OnLostMouseCapture(e);
        }

        private void ListMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ListDoubleClick?.Invoke(sender, this.lstRules.SelectedItem);
        }

        #endregion

        #region Events

        public event EventHandler<object> ListDoubleClick;

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

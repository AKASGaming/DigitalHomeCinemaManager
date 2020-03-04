﻿/*
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

namespace DigitalHomeCinemaControl.Controls.Denon
{
    using System.ComponentModel;
    using System.Windows.Controls;
    using DigitalHomeCinemaControl.Collections;
    using DigitalHomeCinemaControl.Controllers.Providers.Denon;
    using DigitalHomeCinemaControl.Components;

    /// <summary>
    /// Interaction logic for AudysseyControl.xaml
    /// </summary>
    public partial class AudysseyControl : UserControl
    {

        #region Members

        private IDispatchedBindingList<IBindingItem> itemsSource;

        #endregion

        #region Constructor

        public AudysseyControl()
        {
            InitializeComponent();
        }

        #endregion

        #region Methods

        private void ItemsSourceListChanged(object sender, ListChangedEventArgs e)
        {
            var changedItem = this.ItemsSource[e.NewIndex];
            switch (changedItem.Name) {
                case AvrController.MULTEQ: this.MultEq.Text = changedItem.Value.ToString(); break;
                case AvrController.DYNEQ: this.DynEq.Text = changedItem.Value.ToString(); break;
                case AvrController.DYNVOL: this.DynVol.Text = changedItem.Value.ToString(); break;
            }
        }

        #endregion

        #region Properties

#pragma warning disable CA2227 // Collection properties should be read only
        public IDispatchedBindingList<IBindingItem> ItemsSource
        {
            get { return this.itemsSource; }
            set {
                this.itemsSource = value;
                if (this.itemsSource != null) {
                    this.itemsSource.ListChanged += ItemsSourceListChanged;
                }
            }
        }
#pragma warning restore CA2227

        #endregion

    }

}

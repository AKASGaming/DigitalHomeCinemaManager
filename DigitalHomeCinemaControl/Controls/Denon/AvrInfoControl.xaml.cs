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

namespace DigitalHomeCinemaControl.Controls.Denon
{
    using System.ComponentModel;
    using DigitalHomeCinemaControl.Collections.Specialized;
    using DigitalHomeCinemaControl.Components;
    using DigitalHomeCinemaControl.Components.Audio;
    using DigitalHomeCinemaControl.Controllers.Providers.Denon;

    /// <summary>
    /// Interaction logic for AvrInfoControl.xaml
    /// </summary>
    public partial class AvrInfoControl : DeviceControl
    {

        private string source;

        #region Constructor

        public AvrInfoControl()
        {
            InitializeComponent();
        }

        #endregion

        #region Methods

#pragma warning disable CA1062 // Validate arguments of public methods
        protected override void DataSourceListChanged(object sender, ListChangedEventArgs e)
        {
            // The controller for this UI Element uses a MultiplexedBindingList as it's 
            // DataSource. If our sub-controls aren't bound yet, do that now
            if (this.DataSource is MultiplexedBindingList<IBindingItem> mList) {
                if (this.StatusList.ItemsSource == null) {
                    this.StatusList.ItemsSource = mList.GetMultiplexedList(AvrController.STATUS);
                }
                if (this.Audyssey.ItemsSource == null) {
                    this.Audyssey.ItemsSource = mList.GetMultiplexedList(AvrController.AUDYSSEY);
                }
                if (this.SpeakerOutput.ItemsSource == null) {
                    var bindingList = mList.GetMultiplexedList(AvrController.OUTPUT);
                    if (bindingList[0] is ChannelBinding binding) {
                        this.SpeakerOutput.ItemsSource = binding;
                    }
                }
            }
            
            if (e.OldIndex == -2) { return; } // specifies that a multiplexed list has changed

            // handle local items
            var changedItem = (IBindingItem)this.DataSource[e.NewIndex];
            switch (changedItem.Name) {
                case AvrController.SURROUNDMODE: this.SurroundMode.Text = changedItem.Value.ToString(); break;
                case AvrController.INPUTSOURCE: 
                    this.source = changedItem.Value.ToString();
                    this.Source.Text = this.source; break;
                case AvrController.QUICKSELECT:
                    // Disabled for now. not happy with how this looks in UI
                    //string quick = changedItem.Value.ToString();
                    //if (string.IsNullOrEmpty(quick)) {
                    //    this.Source.Text = this.source;
                    //} else {
                    //    this.Source.Text = string.Format("{0} / {1}", this.source, quick);
                    //}
                    break;
            }
        }
#pragma warning restore CA1062

        #endregion

    }

}

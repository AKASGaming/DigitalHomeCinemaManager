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
    using System.Diagnostics.CodeAnalysis;
    using DigitalHomeCinemaControl.Collections;

    /// <summary>
    /// Interaction logic for AvrInfoControl.xaml
    /// </summary>
    public partial class AvrInfoControl : DeviceControl
    {

        #region Members

        //private readonly string[] statusIgnore = { "Output", "Surround Mode", "Input Source", "MultEQ", "Dyn EQ", "Dyn Vol" };
        //private readonly string[] audysseyInclude = { "MultEQ", "Dyn EQ", "Dyn Vol" };

        #endregion

        #region Constructor

        public AvrInfoControl()
        {
            InitializeComponent();
        }

        #endregion

        #region Methods

        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "<Pending>")]
        protected override void DataSourceListChanged(object sender, ListChangedEventArgs e)
        {
            // The controller for this UI Element uses a MultiplexedBindingList as it's 
            // DataSource. If our sub-controls aren't bound yet, do that now
            if (this.DataSource is MultiplexedBindingList<IBindingItem> mList) {
                if (this.StatusList.ItemsSource == null) {
                    this.StatusList.ItemsSource = mList.GetMultiplexedList("Status");
                }
                if (this.Audyssey.ItemsSource == null) {
                    this.Audyssey.ItemsSource = mList.GetMultiplexedList("Audyssey");
                }
                if (this.SpeakerOutput.ItemsSource == null) {
                    var bindingList = mList.GetMultiplexedList("Output");
                    if (bindingList[0] is ChannelBinding binding) {
                        this.SpeakerOutput.ItemsSource = binding;
                    }
                }
            }
            
            if (e.OldIndex == -2) { return; } // specifies that a multiplexed list has changed

            // handle local items
            var changedItem = (IBindingItem)this.DataSource[e.NewIndex];
            switch (changedItem.Name) {
                case "Surround Mode": this.SurroundMode.Text = changedItem.Value.ToString(); break;
                case "Input Source": this.Source.Text = changedItem.Value.ToString(); break;
            }
        }

        #endregion

    }

}

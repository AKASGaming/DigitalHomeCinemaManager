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

namespace DigitalHomeCinemaControl.Collections
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Threading;

    /// <summary>
    /// Implements a specialized IBindingItem class for the ChannelStatus type.
    /// </summary>
    public sealed class ChannelBinding : IBindingItem, IDispatchEvents
    {

        #region Constructor

        /// <summary>
        /// Creates a new ChannelBinding instance.
        /// </summary>
        /// <param name="name">The name of the ChannelBinding.</param>
        /// <param name="dispatcher">The Dispatcher to use to marshal events to the UI thread.</param>
        public ChannelBinding(string name, Dispatcher dispatcher)
        {
            this.Name = name;
            this.Dispatcher = dispatcher;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Raised the PropertyChanged event causing UI elements to refresh.
        /// </summary>
        /// <param name="name">Optional. The name of the Item that changed.</param>
        public void Reset(string name = null)
        {
            OnPropertyChanged(name);
        }

        /// <summary>
        /// Raises the INotifyPropertyChanged.PropertyChanged event.
        /// </summary>
        /// <param name="name"></param>
        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            if (this.Dispatcher == null) {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            } else {
                this.Dispatcher.BeginInvoke((Action)(() => {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
                }));
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Raised whenever the ChannelBinding has been reset.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the ChannelBinding Name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets or Sets the ChannelBinding value.
        /// </summary>
        /// <remarks>Also updates ChannelStatus.</remarks>
        object IBindingItem.Value
        {
            get { return this.ChannelStatus; }
            set {
                if (value is ChannelStatus channelStatus) {
                    this.ChannelStatus = channelStatus;
                }
            }
        }

        /// <summary>
        /// Gets or Sets the ChannelStatus.
        /// </summary>
        /// <remarks>Also updates IBindingItem.Value </remarks>
        public ChannelStatus ChannelStatus { get; set; }

        /// <summary>
        /// Gets or Sets the Dispatcher used for marshalling updates to the UI thread.
        /// </summary>
        public Dispatcher Dispatcher { get; set; }

        #endregion

    }

}

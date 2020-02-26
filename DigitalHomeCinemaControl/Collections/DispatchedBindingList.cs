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
    using System.Windows.Threading;

    /// <summary>
    /// The DispatchedBindingList is a specialized form of a BindingList which uses a
    /// Dispatcher to marshall ListChanged events to the UI thread.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DispatchedBindingList<T> : BindingList<T>, IDispatchedBindingList<T>
        where T : IBindingItem
    {

        #region Members

        private Dispatcher dispatcher;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new instance of the DispatchedBindingList class.
        /// </summary>
        public DispatchedBindingList()
            : base()
        {
        }

        #endregion

        #region Methods

        protected override void OnListChanged(ListChangedEventArgs e)
        {
            if ((this.Dispatcher != null) && !this.Dispatcher.CheckAccess()) {
                this.Dispatcher.BeginInvoke((Action)(() => {
                    base.OnListChanged(e);
                }));
            } else {
                base.OnListChanged(e);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or Sets the Dispatcher used to marshall events to the UI thread.
        /// </summary>
        public Dispatcher Dispatcher
        { 
            get { return this.dispatcher; }
            set {
                this.dispatcher = value;
                foreach (var item in this) {
                    if (item is IDispatchEvents dei) {
                        dei.Dispatcher = this.dispatcher;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the item in the list with the specified name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public T this[string name]
        {
            get {
                foreach (IBindingItem item in this) {
                    if (item.Name.Equals(name, StringComparison.Ordinal)) {
                        return (T)item;
                    }
                }

                return default;
            }
        }

        #endregion

    }

}

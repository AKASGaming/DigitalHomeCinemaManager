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
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Threading;

    /// <summary>
    /// The MultiplexedBindingList class is a special implementation of a DispatchedBindingList
    /// which is a Dictionary of DispatchedBindingList.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class MultiplexedBindingList<T> : BindingList<T>, IDispatchedBindingList<T>
        where T : IBindingItem
    {

        #region Members

        private Dictionary<string, DispatchedBindingList<T>> multiplexedLists;
        private Dictionary<string, string> hashTable;
        private Dispatcher dispatcher;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new MultiplexedBindingList instance.
        /// </summary>
        public MultiplexedBindingList()
            : base()
        {
            this.multiplexedLists = new Dictionary<string, DispatchedBindingList<T>>();
            this.hashTable = new Dictionary<string, string>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a new item to the specified multiplexed BindingList.
        /// </summary>
        /// <param name="key">The name of the multiplexed list to add this item to.</param>
        /// <param name="item">The item to add to the list.</param>
        public void Add(string key, T item)
        {
            if (string.IsNullOrEmpty(key)) {
                Add(item);
                return;
            }

            if (!this.multiplexedLists.ContainsKey(key)) {
                var list = new DispatchedBindingList<T>() {
                    RaiseListChangedEvents = this.RaiseListChangedEvents
                };
                list.ListChanged += MultiplexedListChanged;
                this.multiplexedLists.Add(key, list);
            }

            this.hashTable.Add(item.Name, key);
            this.multiplexedLists[key].Add(item);
        }

        /// <summary>
        /// Gets the specified multiplexed BindingList.
        /// </summary>
        /// <param name="key">The name of the BindingList to get.</param>
        /// <returns></returns>
        public IDispatchedBindingList<T> GetMultiplexedList(string key)
        {
            if (!this.multiplexedLists.ContainsKey(key)) { return null; }
                
            return this.multiplexedLists[key];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MultiplexedListChanged(object sender, ListChangedEventArgs e)
        {
            // Set the OldIndex to -2 so that consumers can identify the change was in a 
            // multiplexed list.
            OnListChanged(new ListChangedEventArgs(e.ListChangedType, e.NewIndex, -2));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        /// Gets an item contained in the collection by its name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public T this[string name]
        {
            get {
                if (this.hashTable.ContainsKey(name)) {
                    var key = this.hashTable[name];
                    foreach (IBindingItem item in this.multiplexedLists[key]) {
                        if (item.Name.Equals(name, StringComparison.Ordinal)) {
                            return (T)item;
                        }
                    }
                } else {
                    foreach (IBindingItem item in this) {
                        if (item.Name.Equals(name, StringComparison.Ordinal)) {
                            return (T)item;
                        }
                    }
                }

                return default;
            }
        }

        /// <summary>
        /// Gets an item in the specified BindingList at the provided index.
        /// </summary>
        /// <param name="key">The name of the BindingList to get the item from. If key is null or empty the base list will be used.</param>
        /// <param name="index"></param>
        /// <returns></returns>
        public T this[string key, int index]
        {
            get {
                if (string.IsNullOrEmpty(key)) {
                    return this[index];
                }

                if (!this.multiplexedLists.ContainsKey(key)) { return default; }

                return this.multiplexedLists[key][index];
            }
        }

        /// <summary>
        /// Gets or Sets the Dispatcher used to marshall events to the UI thread.
        /// </summary>
        public Dispatcher Dispatcher
        {
            get { return this.dispatcher; }
            set {
                this.dispatcher = value;
                foreach (var list in this.multiplexedLists.Values) {
                    list.Dispatcher = this.dispatcher;
                }
            }
        }

        #endregion

    }

}

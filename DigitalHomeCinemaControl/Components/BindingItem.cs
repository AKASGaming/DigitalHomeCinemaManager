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

namespace DigitalHomeCinemaControl.Components
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Implements a simple class used for data binding between controllers and UI elements.
    /// </summary>
    /// <typeparam name="T">The data type of the BindingItem</typeparam>
    public class BindingItem<T> : IBindingItem
    {

        #region Members

        private T value;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new instance of the BindingItem class.
        /// </summary>
        /// <param name="name">The name of the BindingItem.</param>
        public BindingItem(string name)
        {
            this.Name = name;
            this.Value = default;
        }

        /// <summary>
        /// Creates a new instance of the BindingItem class.
        /// </summary>
        /// <param name="name">The name of the BindingItem.</param>
        /// <param name="value">The value to assign to the BindingItem.</param>
        public BindingItem(string name, T value)
        {
            this.Name = name;
            this.Value = value;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Raises the INotifyPropertyChanged.PropertyChanged event.
        /// </summary>
        /// <param name="name"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion

        #region Events

        /// <summary>
        /// Raised whenever a BindingItem property has changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the BindingItem Name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets or Sets the BindingItem Value.
        /// </summary>
        public T Value
        {
            get { return this.value; }
            set {
                if (value != null && !value.Equals(this.value)) {
                    this.value = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or Sets the BindingItem Value.
        /// </summary>
        object IBindingItem.Value 
        {
            get { return this.Value; }
            set { this.Value = (T)value; }
        }

        #endregion

    }

}

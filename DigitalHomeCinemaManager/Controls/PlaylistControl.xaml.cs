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
    using System.Collections;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for PlaylistControl.xaml
    /// </summary>
    public partial class PlaylistControl : UserControl
    {

        public PlaylistControl()
        {
            InitializeComponent();
        }

        public IEnumerable ItemsSource
        {
            get { return this.lstPlaylist.ItemsSource; }
            set { this.lstPlaylist.ItemsSource = value; }
        }

    }

}

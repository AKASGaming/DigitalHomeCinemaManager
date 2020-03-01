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

namespace DigitalHomeCinemaManager.Windows
{
    using Microsoft.Win32;
    using System.Collections.Generic;
    using System.Windows;

    /// <summary>
    /// Interaction logic for PlaylistSelectionWindow.xaml
    /// </summary>
    public partial class PlaylistSelectionWindow : Window
    {

        #region Constructor

        public PlaylistSelectionWindow(List<string> playlist)
        {
            InitializeComponent();
            if (playlist == null) {
                this.Playlist = new List<string>();
            } else {
                this.Playlist = new List<string>(playlist);
            }
        }

        #endregion

        #region Methods

        public new bool? ShowDialog()
        {
            if (this.Playlist != null) {
                foreach (string s in this.Playlist) {
                    this.lstPlaylist.Items.Add(s);
                }
            }

            return base.ShowDialog();
        }

        private void AddClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog() {
                InitialDirectory = this.InitialDirectory,
                Filter = this.Filter
            };
            if (ofd.ShowDialog() == true) {
                this.lstPlaylist.Items.Add(ofd.FileName);
            }
        }

        private void RemoveClick(object sender, RoutedEventArgs e)
        {
            if (this.lstPlaylist.SelectedIndex < 0) { return; }
            this.lstPlaylist.Items.RemoveAt(this.lstPlaylist.SelectedIndex);
        }

        private void UpClick(object sender, RoutedEventArgs e)
        {
            MoveItem(-1);
        }

        private void DownClick(object sender, RoutedEventArgs e)
        {
            MoveItem(1);
        }

        private void MoveItem(int direction)
        {
            // Checking selected item
            if (this.lstPlaylist.SelectedItem == null || this.lstPlaylist.SelectedIndex < 0) {
                return; // No selected item - nothing to do
            }

            // Calculate new index using move direction
            int newIndex = this.lstPlaylist.SelectedIndex + direction;

            // Checking bounds of the range
            if (newIndex < 0 || newIndex >= this.lstPlaylist.Items.Count) {
                return; // Index out of range - nothing to do
            }

            object selected = this.lstPlaylist.SelectedItem;

            // Removing removable element
            this.lstPlaylist.Items.Remove(selected);
            // Insert it in new position
            this.lstPlaylist.Items.Insert(newIndex, selected);
            // Restore selection
            this.lstPlaylist.SelectedIndex = newIndex;
        }

        private void OkClick(object sender, RoutedEventArgs e)
        {
            this.Playlist.Clear();
            foreach (string item in this.lstPlaylist.Items) {
                this.Playlist.Add(item);
            }
            this.DialogResult = true;
            Close();
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }

        #endregion

        #region Properties

        public List<string> Playlist { get; private set; }

        public string InitialDirectory { get; set; }

        public string Filter { get; set; }

        #endregion

    }

}

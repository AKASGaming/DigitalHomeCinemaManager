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

namespace DigitalHomeCinemaManager.Controls.Settings
{
    using System;
    using System.IO;
    using System.Windows;
    using Microsoft.Win32;

    /// <summary>
    /// Interaction logic for GeneralSettings.xaml
    /// </summary>
    public partial class GeneralSettings : SettingsControl
    {

        #region Constructor

        public GeneralSettings()
        {
            InitializeComponent();

            this.MediaPath.Text = Properties.Settings.Default.MediaPath;
            this.PrerollPath.Text = Properties.Settings.Default.PrerollPath;
            this.TrailerPath.Text = Properties.Settings.Default.TrailerPath;
        }

        #endregion

        #region Methods

        public override void SaveChanges()
        {
             Properties.Settings.Default.MediaPath = this.MediaPath.Text;
             Properties.Settings.Default.PrerollPath = this.PrerollPath.Text;
             Properties.Settings.Default.TrailerPath = this.TrailerPath.Text;
        }

        private void ButtonMediaClick(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog() {
                Title = "Select Media Path",
                Filter = "Directory|*.this.directory",
                FileName = "select",
            };
            if (Directory.Exists(this.MediaPath.Text)) {
                dialog.InitialDirectory = this.MediaPath.Text;
            } else {
                dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            }
            if (dialog.ShowDialog() == true) {
                string path = dialog.FileName;
                path = path.Replace("\\select.this.directory", "");
                path = path.Replace(".this.directory", "");
                this.MediaPath.Text = path;
                OnItemChanged();
            }
        }

        private void ButtonPrerollClick(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog() {
                Title = "Select Preroll Path",
                Filter = "Directory|*.this.directory",
                FileName = "select",
            };
            if (Directory.Exists(this.PrerollPath.Text)) {
                dialog.InitialDirectory = this.PrerollPath.Text;
            } else {
                dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            }
            if (dialog.ShowDialog() == true) {
                string path = dialog.FileName;
                path = path.Replace("\\select.this.directory", "");
                path = path.Replace(".this.directory", "");
                this.PrerollPath.Text = path;
                OnItemChanged();
            }
        }

        private void ButtonTrailerClick(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog() {
                Title = "Select Trailer Path",
                Filter = "Directory|*.this.directory",
                FileName = "select",
            };
            if (Directory.Exists(this.TrailerPath.Text)) {
                dialog.InitialDirectory = this.TrailerPath.Text;
            } else {
                dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            }
            if (dialog.ShowDialog() == true) {
                string path = dialog.FileName;
                path = path.Replace("\\select.this.directory", "");
                path = path.Replace(".this.directory", "");
                this.TrailerPath.Text = path;
                OnItemChanged();
            }
        }

        private void MediaPathTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            OnItemChanged();
        }

        private void PrerollPathTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            OnItemChanged();
        }

        private void TrailerPathTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            OnItemChanged();
        }

        #endregion

    }

}

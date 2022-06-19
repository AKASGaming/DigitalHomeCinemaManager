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
    using System.Windows.Forms;
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
            this.RandTrailers.IsChecked = Properties.Settings.Default.RandTrailers;
        }

        #endregion

        #region Methods

        public override void SaveChanges()
        {
             Properties.Settings.Default.MediaPath = this.MediaPath.Text;
             Properties.Settings.Default.PrerollPath = this.PrerollPath.Text;
             Properties.Settings.Default.TrailerPath = this.TrailerPath.Text;
             Properties.Settings.Default.RandTrailers = (bool)this.RandTrailers.IsChecked;
        }

        private void ButtonMediaClick(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog()
            {
                ShowNewFolderButton = false
            };
            if (Directory.Exists(this.MediaPath.Text)) {
                dialog.SelectedPath = this.MediaPath.Text;
            } else {
                dialog.RootFolder = dialog.RootFolder;
            }
            if (dialog.ShowDialog() == DialogResult.OK) {
                string path = dialog.SelectedPath;
                this.MediaPath.Text = path;
                OnItemChanged();
            }
        }

        private void ButtonPrerollClick(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog()
            {
                ShowNewFolderButton = false
            };
            if (Directory.Exists(this.PrerollPath.Text))
            {
                dialog.SelectedPath = this.PrerollPath.Text;
            }
            else
            {
                dialog.RootFolder = dialog.RootFolder;
            }
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string path = dialog.SelectedPath;
                this.PrerollPath.Text = path;
                OnItemChanged();
            }
        }

        private void ButtonTrailerClick(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog()
            {
                ShowNewFolderButton = false
            };
            if (Directory.Exists(this.TrailerPath.Text))
            {
                dialog.SelectedPath = this.TrailerPath.Text;
            }
            else
            {
                dialog.RootFolder = dialog.RootFolder;
            }
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string path = dialog.SelectedPath;
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

        private void checkBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            OnItemChanged();
        }

        private void comboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            OnItemChanged();
        }

        private void RandTrailers_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.RandTrailers = true;
            OnItemChanged();
            Console.Write(Properties.Settings.Default.RandTrailers);
        }
        private void RandTrailers_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.RandTrailers = false;
            OnItemChanged();
            Console.Write(Properties.Settings.Default.RandTrailers);
        }
    }
}

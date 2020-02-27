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
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for GeneralSettings.xaml
    /// </summary>
    public partial class GeneralSettings : UserControl
    {
        public GeneralSettings()
        {
            InitializeComponent();

            this.MediaPath.Text = Properties.Settings.Default.MediaPath;
            this.PrerollPath.Text = Properties.Settings.Default.PrerollPath;
            this.TrailerPath.Text = Properties.Settings.Default.TrailerPath;

        }

        private void ButtonMediaClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            if (Directory.Exists(this.MediaPath.Text)) {
                fbd.SelectedPath = this.MediaPath.Text;
            } else {
                fbd.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            }

            System.Windows.Forms.DialogResult result = fbd.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrEmpty(fbd.SelectedPath)) {
                this.MediaPath.Text = fbd.SelectedPath;
                OnItemChanged();
            }
        }

        private void ButtonPrerollClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            if (Directory.Exists(this.PrerollPath.Text)) {
                fbd.SelectedPath = this.PrerollPath.Text;
            } else {
                fbd.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            }

            System.Windows.Forms.DialogResult result = fbd.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrEmpty(fbd.SelectedPath)) {
                this.PrerollPath.Text = fbd.SelectedPath;
                OnItemChanged();
            }
        }

        private void ButtonTrailerClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            if (Directory.Exists(this.TrailerPath.Text)) {
                fbd.SelectedPath = this.TrailerPath.Text;
            } else {
                fbd.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            }

            System.Windows.Forms.DialogResult result = fbd.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrEmpty(fbd.SelectedPath)) {
                this.TrailerPath.Text = fbd.SelectedPath;
                OnItemChanged();
            }
        }

        private void MediaPathLostFocus(object sender, RoutedEventArgs e)
        {
            OnItemChanged();
        }

        private void PrerollPathLostFocus(object sender, RoutedEventArgs e)
        {
            OnItemChanged();
        }

        private void TrailerPathLostFocus(object sender, RoutedEventArgs e)
        {
            OnItemChanged();
        }

        protected void OnItemChanged()
        {
            ItemChanged?.Invoke(this, new EventArgs());
        }

        public event EventHandler ItemChanged;

        
    }

}

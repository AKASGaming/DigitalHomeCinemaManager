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
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media.Imaging;
    using DigitalHomeCinemaControl.Controllers;

    /// <summary>
    /// Interaction logic for MediaControl.xaml
    /// </summary>
    public partial class MediaControl : UserControl
    {

        #region Members

        private bool subtitlesEnabled = false;
        private bool muted = false;

        #endregion

        #region Constructor

        public MediaControl()
        {
            InitializeComponent();
        }

        #endregion

        #region Methods

        private void ButtonPlayClick(object sender, RoutedEventArgs e)
        {
            this.Controller.Play();
        }

        private void ButtonPauseClick(object sender, RoutedEventArgs e)
        {
            this.Controller.Pause();
        }

        private void ButtonStopClick(object sender, RoutedEventArgs e)
        {
            this.Controller.Stop();
        }

        private void ButtonPrevClick(object sender, RoutedEventArgs e)
        {
            this.Controller.Previous();
        }

        private void ButtonSkipBackClick(object sender, RoutedEventArgs e)
        {
            this.Controller.Rewind();
        }

        private void ButtonSkipForwardClick(object sender, RoutedEventArgs e)
        {
            this.Controller.FastForward();
        }

        private void ButtonNextClick(object sender, RoutedEventArgs e)
        {
            this.Controller.Next();
        }

        private void ButtonSubtitleClick(object sender, RoutedEventArgs e)
        {
            this.Controller.Subtitles();
            if (this.subtitlesEnabled) {
                this.subtitlesEnabled = false;
                ((Image)this.ButtonSubtitle.Content).Source = new BitmapImage(new Uri("pack://application:,,/Resources/Icons/baseline_subtitles_black_36dp.png"));
            } else {
                this.subtitlesEnabled = true;
                ((Image)this.ButtonSubtitle.Content).Source = new BitmapImage(new Uri("pack://application:,,/Resources/Icons/baseline_subtitles_white_36dp.png"));
            }
        }

        private void ButtonMuteClick(object sender, RoutedEventArgs e)
        {
            this.Controller.Mute();
            if (this.muted) {
                this.muted = false;
                ((Image)this.ButtonMute.Content).Source = new BitmapImage(new Uri("pack://application:,,/Resources/Icons/baseline_volume_up_white_36dp.png"));
            } else {
                this.muted = true;
                ((Image)this.ButtonMute.Content).Source = new BitmapImage(new Uri("pack://application:,,/Resources/Icons/baseline_volume_off_white_36dp.png"));
            }
        }

        #endregion

        #region Properties

        public ISourceController Controller { get; set; }

        #endregion

    }

}

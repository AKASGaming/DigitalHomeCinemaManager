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

namespace DigitalHomeCinemaControl.Controls.Common
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using DigitalHomeCinemaControl.Collections;

    #region Extensions

    internal static class Extensions
    {
        internal static Visibility ToVisibility(this bool value)
        {
            if (value == true) {
                return Visibility.Visible;
            } else {
                return Visibility.Hidden;
            }
        }
    }

    #endregion

    /// <summary>
    /// Interaction logic for SpeakerOutputControl.xaml
    /// </summary>
    public partial class SpeakerOutputControl : UserControl
    {

        #region Members

        private readonly SolidColorBrush activeChannel = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF28B01A"));
        private readonly SolidColorBrush inactiveChannel = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF494949"));
        private ChannelBinding itemsSource;

        #endregion

        #region Constructor

        public SpeakerOutputControl()
        {
            InitializeComponent();
        }

        #endregion

        #region Members

        private void UpdateAvailableChannels()
        {
            if (this.ItemsSource.ChannelStatus.HideUnusedChannels) {
                foreach (var availableChannel in this.ItemsSource.ChannelStatus.AvailableChannels) {
                    switch (availableChannel.Key) {
                        case AudioChannel.Center: this.C.Visibility = availableChannel.Value.ToVisibility(); break;
                        case AudioChannel.FrontWideLeft: this.FWL.Visibility = availableChannel.Value.ToVisibility(); break;
                        case AudioChannel.FrontWideRight: this.FWR.Visibility = availableChannel.Value.ToVisibility(); break;
                        case AudioChannel.Left: this.FL.Visibility = availableChannel.Value.ToVisibility(); break;
                        case AudioChannel.Right: this.FR.Visibility = availableChannel.Value.ToVisibility(); break;
                        case AudioChannel.Subwoofer: this.SW.Visibility = availableChannel.Value.ToVisibility(); break;
                        case AudioChannel.SurroundBackLeft: this.SBL.Visibility = availableChannel.Value.ToVisibility(); break;
                        case AudioChannel.SurroundBackRight: this.SBR.Visibility = availableChannel.Value.ToVisibility(); break;
                        case AudioChannel.SurroundLeft: this.SL.Visibility = availableChannel.Value.ToVisibility(); break;
                        case AudioChannel.SurroundRight: this.SR.Visibility = availableChannel.Value.ToVisibility(); break;
                        case AudioChannel.TopBackLeft: this.TBL.Visibility = availableChannel.Value.ToVisibility(); break;
                        case AudioChannel.TopBackRight: this.TBR.Visibility = availableChannel.Value.ToVisibility(); break;
                        case AudioChannel.TopFrontLeft: this.TFL.Visibility = availableChannel.Value.ToVisibility(); break;
                        case AudioChannel.TopFrontRight: this.TFR.Visibility = availableChannel.Value.ToVisibility(); break;
                        case AudioChannel.TopMiddleLeft: this.TML.Visibility = availableChannel.Value.ToVisibility(); break;
                        case AudioChannel.TopMiddleRight: this.TMR.Visibility = availableChannel.Value.ToVisibility(); break;
                        case AudioChannel.VoiceOfGod: this.VOG.Visibility = availableChannel.Value.ToVisibility(); break;
                    }
                } // foreach
            } else {
                this.C.Visibility = Visibility.Visible;
                this.FWL.Visibility = Visibility.Visible;
                this.FWR.Visibility = Visibility.Visible;
                this.FL.Visibility = Visibility.Visible;
                this.FR.Visibility = Visibility.Visible;
                this.SW.Visibility = Visibility.Visible;
                this.SBL.Visibility = Visibility.Visible;
                this.SBR.Visibility = Visibility.Visible;
                this.SL.Visibility = Visibility.Visible;
                this.SR.Visibility = Visibility.Visible;
                this.TBL.Visibility = Visibility.Visible;
                this.TBR.Visibility = Visibility.Visible;
                this.TFL.Visibility = Visibility.Visible;
                this.TFR.Visibility = Visibility.Visible;
                this.TML.Visibility = Visibility.Visible;
                this.TMR.Visibility = Visibility.Visible;
                this.VOG.Visibility = Visibility.Visible;
            }
        }

        private void UpdateActiveChannels()
        {
            Border border;

            foreach (var activeChannel in this.ItemsSource.ChannelStatus.ActiveChannels) {
                switch (activeChannel.Key) {
                    case AudioChannel.Center: border = this.C; break;
                    case AudioChannel.FrontWideLeft: border = this.FWL; break;
                    case AudioChannel.FrontWideRight: border = this.FWR; break;
                    case AudioChannel.Left: border = this.FL; break;
                    case AudioChannel.Right: border = this.FR; break;
                    case AudioChannel.Subwoofer: border = this.SW; break;
                    case AudioChannel.SurroundBackLeft: border = this.SBL; break;
                    case AudioChannel.SurroundBackRight: border = this.SBR; break;
                    case AudioChannel.SurroundLeft: border = this.SL; break;
                    case AudioChannel.SurroundRight: border = this.SR; break;
                    case AudioChannel.TopBackLeft: border = this.TBL; break;
                    case AudioChannel.TopBackRight: border = this.TBR; break;
                    case AudioChannel.TopFrontLeft: border = this.TFL; break;
                    case AudioChannel.TopFrontRight: border = this.TFR; break;
                    case AudioChannel.TopMiddleLeft: border = this.TML; break;
                    case AudioChannel.TopMiddleRight: border = this.TMR; break;
                    case AudioChannel.VoiceOfGod:  border = this.VOG; break;
                    default: border = null; break;
                }

                if (border == null) { continue; }

                if (activeChannel.Value == true) {
                    border.Background = this.activeChannel;
                } else {
                    border.Background = this.inactiveChannel;
                }
            } // foreach
        }

        private void ItemsSourcePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName) {
                case "HideUnusedChannels":
                case "AvailableChannels":
                    UpdateAvailableChannels();
                    break;
                case "ActiveChannels":
                    UpdateActiveChannels();
                    break;
                default: // update everything
                    UpdateAvailableChannels();
                    UpdateActiveChannels();
                    break;
            }
        }

        #endregion

        #region Properties

        public ChannelBinding ItemsSource 
        { 
            get { return this.itemsSource; }
            set {
                this.itemsSource = value;
                if (this.itemsSource != null) {
                    this.itemsSource.PropertyChanged += ItemsSourcePropertyChanged;
                }
            }
        }

        #endregion

    }

}

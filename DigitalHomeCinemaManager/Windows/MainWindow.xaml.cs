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
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using DigitalHomeCinemaControl;
    using DigitalHomeCinemaControl.Collections;
    using DigitalHomeCinemaControl.Components;
    using DigitalHomeCinemaControl.Controllers;
    using DigitalHomeCinemaControl.Controllers.Base;
    using DigitalHomeCinemaControl.Devices;
    using DigitalHomeCinemaManager.Components;
    using DigitalHomeCinemaManager.Controls;
    using Microsoft.Win32;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    internal partial class MainWindow : Window, IDisposable
    {

        #region Members

        private PlaylistManager playlist;
        private bool playlistInitialized = false;
        private List<string> errorLog = new List<string>();
        private IDispatchedBindingList<IBindingItem> sourceData;
        private System.Timers.Timer clockTimer;
        private double playbackLength;
        private int playbackPosition;
        private bool disposed = false; 

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();

#pragma warning disable CA1308 // Normalize strings to uppercase
            this.txtDate.Text = DateTime.Now.Date.ToString(Properties.Resources.FMT_DATE, CultureInfo.InvariantCulture).ToLowerInvariant();
#pragma warning restore CA1308 // Normalize strings to uppercase
            this.lblTimeHour.Content = DateTime.Now.Hour.ToString("D2", CultureInfo.InvariantCulture);
            this.lblTimeMinute.Content = DateTime.Now.Minute.ToString("D2", CultureInfo.InvariantCulture);

            this.clockTimer = new System.Timers.Timer {
                Interval = 10000
            };
            this.clockTimer.Elapsed += OnTimerElapsed;
            this.clockTimer.AutoReset = true;
            this.clockTimer.Start();
        }

        #endregion

        #region Methods

        public void UpdateStatus(string message)
        {
            if (this.errorLog.Count >= 2) {
                this.errorLog.RemoveAt(0);
            }
            this.errorLog.Add(string.Format(CultureInfo.InvariantCulture, "{0} - {1}", DateTime.Now, message));
            StringBuilder result = new StringBuilder();
            foreach (string s in this.errorLog) {
                result.Append(string.Format(CultureInfo.InvariantCulture, "{0} \r\n", s));
            }
            this.txtLog.Text = result.ToString();
        }

        public void BindDevices(IEnumerable<IDevice> devices)
        {
            foreach (var device in devices) {
                if (device.UIElement == null) {
                    if (device is SerialDevice serial) {
                        device.Controller.PropertyChanged += SerialControllerPropertyChanged;
                    }
                    continue;
                }

                if (device is SourceDevice source) {
                    source.Controller.PropertyChanged += SourcePropertyChanged;
                    this.SourceData = source.Controller.DataSource;
                    MediaControl mediaControl = new MediaControl() {
                        Controller = source.GetController<ISourceController>(),
                    };
                    this.StatusBar.Children.Insert(0, mediaControl);
                    this.StatusBar.Children.Insert(1, device.UIElement);
                } else if (device is MediaInfoDevice) {
                    this.FeatureInfo.Child = device.UIElement;
                } else if (device is DisplayDevice) {
                    this.DisplayInfo.Child = device.UIElement;
                    device.Controller.PropertyChanged += DisplayControllerPropertyChanged;
                } else if (device is ProcessorDevice) {
                    this.ProcessorInfo.Child = device.UIElement;
                    device.Controller.PropertyChanged += ProcessorControllerPropertyChanged;
                } else if (device is SwitchDevice) {
                    this.InputSwitcher.Child = device.UIElement;
                }

            } // foreach

            if (this.InputSwitcher.Child == null) {
                this.InputSwitcher.Visibility = Visibility.Hidden;
                this.ShowPlaylist.Height = 442;
            }

            if (string.IsNullOrEmpty(Properties.Settings.Default.ProcessorDevice)) {
                this.StatusControl.ProcessorStatus.Content = "Disabled";
            }
            if (string.IsNullOrEmpty(Properties.Settings.Default.DisplayDevice)) {
                this.StatusControl.ProjectorStatus.Content = "Disabled";
            }
            if (string.IsNullOrEmpty(Properties.Settings.Default.SerialDevice)) {
                this.StatusControl.SerialStatus.Content = "Disabled";
            }
        }

        private void ProcessorControllerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ((sender is IProcessorController processor) &&
                ((e.PropertyName == nameof(processor.ControllerStatus)) || 
                 (e.PropertyName == nameof(processor.Delay)) ||
                 (e.PropertyName == nameof(processor.MasterVolume)))) {

                this.StatusControl.ProcessorStatus.Content = processor.ControllerStatus.ToString();
                this.StatusControl.Delay.Content = string.Format(CultureInfo.InvariantCulture, Properties.Resources.FMT_DELAY, processor.Delay);
                this.MasterVolume.Text = (processor.MasterVolume <= -80m)? Properties.Resources.DB_MUTE :
                    (processor.MasterVolume > 0)? string.Format(CultureInfo.InvariantCulture, Properties.Resources.FMT_DB_PLUS, processor.MasterVolume) : 
                     string.Format(CultureInfo.InvariantCulture, Properties.Resources.FMT_DB, processor.MasterVolume);

                if (processor.MasterVolume <= -80) {
                    this.MasterVolume.Foreground = new SolidColorBrush(Colors.Aqua);
                } else if (processor.MasterVolume < -10) {
                    this.MasterVolume.Foreground = new SolidColorBrush(Colors.Gold);
                } else if (processor.MasterVolume > 0) {
                    this.MasterVolume.Foreground = new SolidColorBrush(Colors.OrangeRed);
                } else {
                    this.MasterVolume.Foreground = new SolidColorBrush(Colors.Chartreuse);
                }
            }
        }

        private void DisplayControllerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ((sender is IDisplayController display) &&
                ((e.PropertyName == nameof(display.ControllerStatus)) ||
                 (e.PropertyName  == nameof(display.LampStatus)) ||
                  (e.PropertyName  == nameof(display.LampTimer)))) { 

                this.StatusControl.ProjectorStatus.Content = display.ControllerStatus.ToString();
                this.StatusControl.Lamp.Content = display.LampStatus.GetDescription();
                this.StatusControl.LampTime.Content = (display.LampTimer >= 0)? 
                    string.Format(CultureInfo.InvariantCulture, Properties.Resources.FMT_LAMP_HOUS, display.LampTimer) : string.Empty;
            }
        }

        private void SerialControllerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ((sender is ISerialController serial) && (e.PropertyName == nameof(serial.ControllerStatus))) {
                this.StatusControl.SerialStatus.Content = serial.ControllerStatus.ToString();
                this.StatusControl.SerialPort.Content = serial.CommPort;
            }
        }

        public void InsertControl(object control)
        {
            if (control is RoutingControl routingControl) {
                this.RoutingInfo.Child = routingControl;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetImageSource(Image image, string source)
        {
            if (string.IsNullOrEmpty(source)) {
                image.Source = new BitmapImage();
            } else {
                image.Source = new BitmapImage(new Uri(source));
            }
        }

        private void PlaylistChanged(object sender, string e)
        {
            switch (this.Playlist.FeatureVideoFormat) {
                case VideoFormat.SD: SetImageSource(this.imgResolution, "pack://application:,,/Resources/Labels/dvd.jpg"); break;
                case VideoFormat.HD: SetImageSource(this.imgResolution, "pack://application:,,/Resources/Labels/hd.jpg"); break;
                case VideoFormat.UHD: SetImageSource(this.imgResolution, "pack://application:,,/Resources/Labels/uhd.jpg"); break;
                case VideoFormat.Unknown: SetImageSource(this.imgResolution, string.Empty); break;
            }
            switch (this.Playlist.FeatureAudioFormat) {
                case AudioFormat.Atmos: SetImageSource(this.imgAudio, "pack://application:,,/Resources/Labels/atmos.png"); break;
                case AudioFormat.DTS: SetImageSource(this.imgAudio, "pack://application:,,/Resources/Labels/dts.jpg"); break;
                case AudioFormat.Dolby: SetImageSource(this.imgAudio, "pack://application:,,/Resources/Labels/dd.png"); break;
                case AudioFormat.Unknown: SetImageSource(this.imgAudio, string.Empty); break;
            }
        }

        private void PlaybackStateChanged(PlaybackState state)
        {
            switch (state) {
                case PlaybackState.Paused:
                    this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Paused;
                    break;
                case PlaybackState.Playing:
                case PlaybackState.PlayingFeature:
                    this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
                    break;
                default:
                    this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                    break;
            }
        }

        private void SourcePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ((sender is ISourceController source) && (e.PropertyName == nameof(source.State))) {
                PlaybackStateChanged(source.State);
            }
        }

        private void OnTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.Dispatcher.BeginInvoke((Action)(() => {
#pragma warning disable CA1308 // Normalize strings to uppercase
                this.txtDate.Text = DateTime.Now.Date.ToString(Properties.Resources.FMT_DATE, CultureInfo.InvariantCulture).ToLowerInvariant();
#pragma warning restore CA1308 // Normalize strings to uppercase
                this.lblTimeHour.Content = DateTime.Now.Hour.ToString("D2", CultureInfo.InvariantCulture);
                this.lblTimeMinute.Content = DateTime.Now.Minute.ToString("D2", CultureInfo.InvariantCulture);
            }));
        }

        private void SourceDataListChanged(object sender, System.ComponentModel.ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemChanged) {
                IBindingItem item = this.SourceData[e.NewIndex];
                switch (item.Name) {
                    case nameof(SourceController.LENGTH):
                        int i = (int)item.Value;
                        if (i > 0) {
                            this.playbackLength = 1d / (int)item.Value;
                        } else {
                            this.playbackLength = 1d;
                        }
                        this.TaskbarItemInfo.ProgressValue = 0;
                        break;
                    case nameof(SourceController.CURRENTPOSITION):
                        this.playbackPosition = (int)item.Value;
                        this.TaskbarItemInfo.ProgressValue = this.playbackPosition * this.playbackLength;
                        break;
                }
            }
        }

        private void ButtonSettingsClick(object sender, RoutedEventArgs e)
        {
            OpenSettings?.Invoke(sender, e);
        }

        private void ButtonScheduleClick(object sender, RoutedEventArgs e)
        {
            OpenScheduler?.Invoke(sender, e);
        }

        private void ButtonPrerollClick(object sender, RoutedEventArgs e)
        {
            var window = new PlaylistSelectionWindow(this.Playlist.PrerollPlaylist) {
                Owner = this,
                Filter = Properties.Resources.FILTER_VIDEOS
            };

            if (Directory.Exists(Properties.Settings.Default.PrerollPath)) {
                window.InitialDirectory = Properties.Settings.Default.PrerollPath;
            } else {
                window.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            }

            if (window.ShowDialog() == true) {
                this.Playlist.PrerollPlaylist = new List<string>(window.Playlist);
                this.Playlist.CreatePlaylist();
            }
        }

        private void ButtonTrailersClick(object sender, RoutedEventArgs e)
        {
            var window = new PlaylistSelectionWindow(this.Playlist.TrailerPlaylist) {
                Owner = this,
                Filter = Properties.Resources.FILTER_VIDEOS
            };

            if (Directory.Exists(Properties.Settings.Default.TrailerPath)) {
                window.InitialDirectory = Properties.Settings.Default.TrailerPath;
            } else {
                window.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            }

            if (window.ShowDialog() == true) {
                this.Playlist.TrailerPlaylist = new List<string>(window.Playlist);
                this.Playlist.CreatePlaylist();
            }
        }

        private void ButtonCommercialClick(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog() {
                Filter = Properties.Resources.FILTER_VIDEOS
            };

            if (Directory.Exists(Properties.Settings.Default.PrerollPath)) {
                ofd.InitialDirectory = Properties.Settings.Default.PrerollPath;
            } else {
                ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            }

            if (ofd.ShowDialog() == true) {
                this.Playlist.Commercial = ofd.FileName;
                this.Playlist.CreatePlaylist();
            }
        }

        private void ButtonFeatureClick(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog() {
                Filter = Properties.Resources.FILTER_MOVIES
            };

            if (Directory.Exists(Properties.Settings.Default.MediaPath)) {
                ofd.InitialDirectory = Properties.Settings.Default.MediaPath;
            } else {
                ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            }

            if (ofd.ShowDialog() == true) {
                this.Playlist.Feature = ofd.FileName;
                this.Playlist.CreatePlaylist();
            }
        }

        private void PrerollEnabledChecked(object sender, RoutedEventArgs e)
        {
            if (!this.PlaylistInitialized) { return; }

            this.Playlist.PrerollEnabled = (this.PrerollEnabled.IsChecked == true) ? true : false;
            this.Playlist.CreatePlaylist();
        }

        private void TrailersEnabledChecked(object sender, RoutedEventArgs e)
        {
            if (!this.PlaylistInitialized) { return; }

            this.Playlist.TrailersEnabled = (this.TrailersEnabled.IsChecked == true) ? true : false;
            this.Playlist.CreatePlaylist();
        }

        private void CommercialEnabledChecked(object sender, RoutedEventArgs e)
        {
            if (!this.PlaylistInitialized) { return; }

            this.Playlist.CommercialEnabled = (this.CommercialEnabled.IsChecked == true) ? true : false;
            this.Playlist.CreatePlaylist();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed) {
                if (disposing) {
                    if (this.clockTimer != null) {
                        this.clockTimer.Dispose();
                    }
                }

                this.clockTimer = null;

                this.disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        #region Events

        public event EventHandler OpenSettings;

        public event EventHandler OpenScheduler;

        #endregion

        #region Properties

        public bool PlaylistInitialized
        {
            get { return this.playlistInitialized; }
            set {
                if ((this.playlistInitialized == false) && (value == true)) {
                    this.PrerollEnabled.IsChecked = this.Playlist.PrerollEnabled;
                    this.TrailersEnabled.IsChecked = this.Playlist.TrailersEnabled;
                    this.CommercialEnabled.IsChecked = this.Playlist.CommercialEnabled;
                }
                this.playlistInitialized = value;
            }
        }

        public PlaylistManager Playlist
        {
            get { return this.playlist; }
            set {
                this.playlist = value;
                if (this.playlist != null) {
                    this.playlist.PlaylistChanged += PlaylistChanged;
                    PlaylistControl pl = new PlaylistControl() {
                        ItemsSource = this.playlist.Playlist
                    };
                    this.ShowPlaylist.Child = pl;
                }
            }
        }

        public IDispatchedBindingList<IBindingItem> SourceData
        { 
            get { return this.sourceData; }
            set {
                this.sourceData = value;
                if (this.sourceData != null) {
                    this.sourceData.ListChanged += SourceDataListChanged;
                }
            }
        }

        #endregion

    }

}


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
    using System.Linq;
    using NAudio.CoreAudioApi;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    internal partial class MainWindow : Window, IDisposable
    {

        #region Members

        private static BitmapSource EMPTY_IMAGE = BitmapImage.Create(2, 2, 96, 96, PixelFormats.Indexed1, new BitmapPalette(new List<Color> { Colors.Transparent }), new byte[] { 0, 0, 0, 0 }, 1);
        
        private PlaylistManager playlist;
        private bool playlistInitialized;
        private List<string> errorLog = new List<string>();
        private IDispatchedBindingList<IBindingItem> sourceData;
        private System.Timers.Timer clockTimer;
        private System.Timers.Timer CT;
        private MMDevice device;
        private double playbackLength;
        private int playbackPosition;
        private bool disposed; 

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();

            this.txtDate.Text = DateTime.Now.Date.ToString(Properties.Resources.FMT_DATE, CultureInfo.InvariantCulture).ToUpperInvariant();
            this.lblTime.Content = DateTime.Now.ToString("hh:mm:ss tt", CultureInfo.InvariantCulture).ToUpperInvariant();

            this.clockTimer = new System.Timers.Timer {
                Interval = 1000
            };
            this.clockTimer.Elapsed += OnTimerElapsed;
            this.clockTimer.AutoReset = true;
            this.clockTimer.Start();

            this.CT = new System.Timers.Timer
            {
                Interval = 200
            };
            this.CT.Elapsed += OnTimerElapsed2;
            this.CT.AutoReset = true;
            this.CT.Start();
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
            ImageSource imageSource;
            
            if (!string.IsNullOrEmpty(source)) {
                var bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = new Uri(source);
                bi.EndInit();
                imageSource = bi;
            } else {
                imageSource = EMPTY_IMAGE;
            }
            
            image.Source = imageSource;
        }

        private void PlaylistChanged(object sender, string e)
        {
            switch (this.Playlist.FeatureVideoFormat) {
                case VideoFormat.HD:
                    SetImageSource(this.imgResolution, "pack://application:,,/Resources/Labels/Video/HD-Gold.png");
                    this.imgResolution.ToolTip = "HD - 720p";
                    this.imgResolution.Visibility = Visibility.Visible;
                    break;
                case VideoFormat.FHD:
                    SetImageSource(this.imgResolution, "pack://application:,,/Resources/Labels/Video/Full_HD-Gold.png");
                    this.imgResolution.ToolTip = "Full HD - 1080p";
                    this.imgResolution.Visibility = Visibility.Visible;
                    break;
                case VideoFormat.QHD:
                    SetImageSource(this.imgResolution, "pack://application:,,/Resources/Labels/Video/QHD-Gold.png");
                    this.imgResolution.ToolTip = "QHD - 1440p";
                    this.imgResolution.Visibility = Visibility.Visible;
                    break;
                case VideoFormat.UHD:
                    SetImageSource(this.imgResolution, "pack://application:,,/Resources/Labels/Video/4K-Gold.png");
                    this.imgResolution.ToolTip = "4K";
                    this.imgResolution.Visibility = Visibility.Visible;
                    break;
                case VideoFormat.EightK:
                    SetImageSource(this.imgResolution, "pack://application:,,/Resources/Labels/Video/8K-Gold.png");
                    this.imgResolution.ToolTip = "8K";
                    this.imgResolution.Visibility = Visibility.Visible;
                    break;
                case VideoFormat.DVD:
                    SetImageSource(this.imgResolution, "pack://application:,,/Resources/Labels/Video/dvd.png");
                    this.imgResolution.ToolTip = "DVD Quality";
                    this.imgResolution.Visibility = Visibility.Visible;
                    break;
                case VideoFormat.Unknown:
                    SetImageSource(this.imgResolution, string.Empty);
                    this.imgResolution.Visibility = Visibility.Hidden;
                    this.imgResolution.ToolTip = null;
                    break;
            }
            switch (this.Playlist.FeatureAudioFormat) {
                case AudioFormat.DolbyAtmos: 
                    SetImageSource(this.imgAudio, "pack://application:,,/Resources/Labels/Audio/atmos.png");
                    this.imgAudio.ToolTip = "Dolby Atmos";
                    this.imgAudio.Visibility = Visibility.Visible;
                    break;
                case AudioFormat.DTS:
                    SetImageSource(this.imgAudio, "pack://application:,,/Resources/Labels/Audio/dts.png");
                    this.imgAudio.ToolTip = "DTS";
                    this.imgAudio.Visibility = Visibility.Visible;
                    break;
                case AudioFormat.DolbyDigital: 
                    SetImageSource(this.imgAudio, "pack://application:,,/Resources/Labels/Audio/dd.png");
                    this.imgAudio.ToolTip = "Dolby Digital";
                    this.imgAudio.Visibility = Visibility.Visible;
                    break;
                case AudioFormat.TrueHD: 
                    SetImageSource(this.imgAudio, "pack://application:,,/Resources/Labels/Audio/dolby-truehd.png");
                    this.imgAudio.ToolTip = "Dolby TrueHD";
                    this.imgAudio.Visibility = Visibility.Visible;
                    break;
                case AudioFormat.TrueHDAtmos:
                    SetImageSource(this.imgAudio, "pack://application:,,/Resources/Labels/Audio/dolby-truehd.png");
                    this.imgAudio.ToolTip = "Dolby TrueHD with Atmos";
                    this.imgAudio.Visibility = Visibility.Visible;
                    break;
                case AudioFormat.DTSX: 
                    SetImageSource(this.imgAudio, "pack://application:,,/Resources/Labels/Audio/dts-x.png"); 
                    this.imgAudio.ToolTip = "DTS X";
                    this.imgAudio.Visibility = Visibility.Visible;
                    break;
                case AudioFormat.DTSHDMA: 
                    SetImageSource(this.imgAudio, "pack://application:,,/Resources/Labels/Audio/dts-master-audio.png");
                    this.imgAudio.ToolTip = "DTS Master Audio";
                    this.imgAudio.Visibility = Visibility.Visible;
                    break;
                case AudioFormat.DTSHD:
                    SetImageSource(this.imgAudio, "pack://application:,,/Resources/Labels/Audio/dts-master-audio.png");
                    this.imgAudio.ToolTip = "DTS HD";
                    this.imgAudio.Visibility = Visibility.Visible;
                    break;
                case AudioFormat.DolbyDigitalPlus:
                    SetImageSource(this.imgAudio, "pack://application:,,/Resources/Labels/Audio/dolby-digital-plus.png");
                    this.imgAudio.ToolTip = "Dolby Digital Plus";
                    this.imgAudio.Visibility = Visibility.Visible;
                    break;
                case AudioFormat.DolbyDigitalPlusAtmos:
                    SetImageSource(this.imgAudio, "pack://application:,,/Resources/Labels/Audio/dolby-digital-plus.png.png");
                    this.imgAudio.ToolTip = "Dolby Digital Plus with Atmos";
                    this.imgAudio.Visibility = Visibility.Visible;
                    break;
                case AudioFormat.Unknown:
                    SetImageSource(this.imgAudio, string.Empty);
                    this.imgAudio.Visibility = Visibility.Hidden;
                    this.imgAudio.ToolTip = null;
                    break;
            }
            switch (this.playlist.FeatureHDRFormat)
            {
                case HDR.HDR10:
                    SetImageSource(this.imgHDR, "pack://application:,,/Resources/Labels/HDR/hdr10.png");
                    this.imgHDR.ToolTip = "HDR10";
                    this.imgHDR.Visibility = Visibility.Visible;
                    break;
                case HDR.HDR10Plus:
                    SetImageSource(this.imgHDR, "pack://application:,,/Resources/Labels/HDR/hdr10plus.png");
                    this.imgHDR.ToolTip = "HDR10+";
                    this.imgHDR.Visibility = Visibility.Visible;
                    break;
                case HDR.DolbyVision:
                    SetImageSource(this.imgHDR, "pack://application:,,/Resources/Labels/HDR/dolby-vision.png");
                    this.imgHDR.ToolTip = "Dolby Vision";
                    this.imgHDR.Visibility = Visibility.Visible;
                    break;
                case HDR.HLG:
                    SetImageSource(this.imgHDR, "pack://application:,,/Resources/Labels/HDR/HLG.png");
                    this.imgHDR.ToolTip = "HLG";
                    this.imgHDR.Visibility = Visibility.Visible;
                    break;
                case HDR.SLHDR1:
                    SetImageSource(this.imgHDR, "pack://application:,,/Resources/Labels/HDR/technicolor.png");
                    this.imgHDR.ToolTip = "Technicolor";
                    this.imgHDR.Visibility = Visibility.Visible;
                    break;
                case HDR.SLHDR2:
                    SetImageSource(this.imgHDR, "pack://application:,,/Resources/Labels/HDR/technicolor.png");
                    this.imgHDR.ToolTip = "Technicolor";
                    this.imgHDR.Visibility = Visibility.Visible;
                    break;
                case HDR.SLHDR3:
                    SetImageSource(this.imgHDR, "pack://application:,,/Resources/Labels/HDR/technicolor.png");
                    this.imgHDR.ToolTip = "Technicolor";
                    this.imgHDR.Visibility = Visibility.Visible;
                    break;
                case HDR.Unknown:
                    SetImageSource(this.imgHDR, string.Empty);
                    this.imgHDR.Visibility = Visibility.Hidden;
                    this.imgHDR.ToolTip = null;
                    break;
            }

            switch (this.playlist.FeatureChannelFormat)
            {
                case "Mono":
                    this.txtAudioChannels.ToolTip = "Mono";
                    this.txtAudioChannels.Text = "Mono";
                    break;
                case "Stereo":
                    this.txtAudioChannels.ToolTip = "Stereo";
                    this.txtAudioChannels.Text = "Stereo";
                    break;
                case "2.1":
                    this.txtAudioChannels.ToolTip = "2.1";
                    this.txtAudioChannels.Text = "2.1";
                    break;
                case "4.0":
                    this.txtAudioChannels.ToolTip = "4.0";
                    this.txtAudioChannels.Text = "4.0";
                    break;
                case "5.0":
                    this.txtAudioChannels.ToolTip = "5.0";
                    this.txtAudioChannels.Text = "5.0";
                    break;
                case "5.1":
                    this.txtAudioChannels.ToolTip = "5.1";
                    this.txtAudioChannels.Text = "5.1";
                    break;
                case "6.1":
                    this.txtAudioChannels.ToolTip = "6.1";
                    this.txtAudioChannels.Text = "6.1";
                    break;
                case "7.1":
                    this.txtAudioChannels.ToolTip = "7.1";
                    this.txtAudioChannels.Text = "7.1";
                    break;
                case "7.2":
                    this.txtAudioChannels.ToolTip = "7.2";
                    this.txtAudioChannels.Text = "7.2";
                    break;
                case "7.2.1":
                    this.txtAudioChannels.ToolTip = "7.2.1";
                    this.txtAudioChannels.Text = "7.2.1";
                    break;
                case "":
                    this.txtAudioChannels.Text = "";
                    this.txtAudioChannels.ToolTip = null;
                    break;
            }

            switch (this.playlist.FeatureIs3D)
            {
                case true:
                    SetImageSource(this.img3D, "pack://application:,,/Resources/Labels/Video/3D.png");
                    this.img3D.Visibility = Visibility.Visible;
                    break;
                case false:
                    SetImageSource(this.img3D, string.Empty);
                    this.img3D.Visibility = Visibility.Hidden;
                    break;

            }
        }

        private void PlaybackStateChanged(PlaybackState state)
        {
            switch (state) {
                case PlaybackState.Paused:
                    this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Paused;
                    UpdateStatus("PAUSED");
                    break;
                case PlaybackState.Playing:
                    UpdateStatus("PLAYING");
                    break;
                case PlaybackState.PlayingTrailer:
                    this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
                    UpdateStatus("PLAYING_TRAILER");
                    break;
                case PlaybackState.PlayingPreroll:
                    this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
                    UpdateStatus("PLAYING_PREROLL");
                    break;
                case PlaybackState.PlayingFeature:
                    this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
                    UpdateStatus("PLAYING_MOVIE");
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

        private void OnTimerElapsed(object sender, System.Timers.ElapsedEventArgs e) => this.Dispatcher.BeginInvoke((Action)(() =>
        {
            this.txtDate.Text = DateTime.Now.Date.ToString(Properties.Resources.FMT_DATE, CultureInfo.InvariantCulture).ToUpperInvariant();
            this.lblTime.Content = DateTime.Now.ToString("hh:mm:ss tt", CultureInfo.InvariantCulture).ToUpperInvariant();
        }));

        private void OnTimerElapsed2(object sender, System.Timers.ElapsedEventArgs e)
        {
            InitializeComponent();

            this.Dispatcher.Invoke(() =>
            {
                //Console.WriteLine(devices.Where(x => x.DeviceFriendlyName.Equals(Properties.Settings.Default.VUDevice)).ToList());

                MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
                var devices = enumerator.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active);

                device = devices.FirstOrDefault();
                //devices.FirstOrDefault(x => x.FriendlyName.Equals(Properties.Settings.Default.VUDevice.ToString()));
                var MasterVolume = (int)Math.Round(device.AudioMeterInformation.MasterPeakValue * 100);

                //Console.WriteLine(MasterVolume);

                this.PCVolume.Text = (MasterVolume <= -80m) ? Properties.Resources.DB_MUTE :
                    (MasterVolume > 0) ? ("+" + MasterVolume + "db") :
                    (MasterVolume + "db");

                if (MasterVolume <= 20)
                {
                    this.PCVolume.Foreground = new SolidColorBrush(Colors.Aqua);
                }
                else if (MasterVolume < 60)
                {
                    this.PCVolume.Foreground = new SolidColorBrush(Colors.Gold);
                }
                else if (MasterVolume > 61)
                {
                    this.PCVolume.Foreground = new SolidColorBrush(Colors.OrangeRed);
                }
                else
                {
                    this.PCVolume.Foreground = new SolidColorBrush(Colors.Chartreuse);
                }
            });
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
                Filter = Properties.Resources.FILTER_VIDEOS,
                Title = "Select Preroll",
                Multiselect = false
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
                Filter = Properties.Resources.FILTER_VIDEOS,
                Title = "Select Trailers",
                Multiselect = true
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
                Filter = Properties.Resources.FILTER_VIDEOS,
                Title = "Select Commercial"
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
                Filter = Properties.Resources.FILTER_MOVIES,
                Title = "Select Feature"
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

                    if (this.CT != null)
                    {
                        this.CT.Dispose();
                    }

                }

                this.clockTimer = null;
                this.CT = null;

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


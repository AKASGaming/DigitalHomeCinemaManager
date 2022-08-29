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

using MediaInfo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Data;
using System.Windows.Threading;

namespace DigitalHomeCinemaManager.Components
{
    internal sealed class PlaylistManager
    {

        #region Members

        private static string PLAYLIST = "\\Show_Playlist.m3u";
        private static string VIDEO_PATH = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);

        private const string MPCPLAYLIST = "#EXTM3U";
        //private const string MPC_FORMAT_TYPE = "{0},type,0";
        private const string MPC_FORMAT_FILE = "{1}";
        
        //Video Formats
        private const string HD = "HD";
        private const string FHD = "FHD";
        private const string QHD = "QHD";
        private const string UHD = "UHD";
        private const string EightK = "EightK";
        private const string DVD = "DVD";

        //HDR Formats
        private const string HDR10 = "HDR10"; 
        private const string HDR10Plus = "HDR10Plus"; 
        private const string DolbyVision = "DolbyVision";
        private const string HLG = "HLG";
        private const string SLHDR1 = "SLHDR1";
        private const string SLHDR2 = "SLHDR2";
        private const string SLHDR3 = "SLHDR3";

        //Audio Formats
        private const string DolbyAtmos = "DolbyAtmos";
        private const string DTS = "DTS";
        private const string DolbyDigital = "DolbyDigital";
        private const string TrueHD = "TrueHD";
        private const string TrueHDAtmos = "TrueHDAtmos";
        private const string DTSX = "DTSX";
        private const string DTSHDMA = "DTSHDMA";
        private const string DTSHD = "DTSHD";
        private const string DolbyDigitalPlus = "DolbyDigitalPlus";
        private const string DolbyDigitalPlusAtmos = "DolbyDigitalPlusAtmos";


        private string prerollPath = Properties.Settings.Default.PrerollPath;
        private string trailerPath = Properties.Settings.Default.TrailerPath;
        private string mediaPath = Properties.Settings.Default.MediaPath;
        private Dispatcher dispatcher;
        private ObservableCollection<PlaylistEntry> playlist = new ObservableCollection<PlaylistEntry>();
        private object playlistLock = new object();
        private string feature;
        

        #endregion

        #region Constructor

        internal PlaylistManager(Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher;

            BindingOperations.EnableCollectionSynchronization(this.Playlist, this.playlistLock);

            this.PrerollPlaylist = new List<string>(10);
            this.TrailerPlaylist = new List<string>(5);
        }

        #endregion

        #region Methods

        public void LoadPlaylist()
        {
            string path = VIDEO_PATH + PLAYLIST;

            if (!File.Exists(path)) { return; }

            using (var reader = new StreamReader(path)) {
                string line;
                bool hasTrailer = false;
                lock (this.playlistLock) {
                    while ((line = reader.ReadLine()) != null) {
                        if (line.Contains(this.trailerPath))
                        {
                            hasTrailer = true;
                            this.TrailerPlaylist.Add(line.Substring(line.LastIndexOf(",", StringComparison.Ordinal) + 1));
                            this.playlist.Add(new PlaylistEntry(PlaylistType.Trailer, line.Substring(line.LastIndexOf(",", StringComparison.Ordinal) + 1)));
                            this.TrailersEnabled = true;
                        }
                        else if (line.Contains(this.mediaPath))
                        {
                            this.Feature = line.Substring(line.LastIndexOf(",", StringComparison.Ordinal) + 1);
                            if (!File.Exists(this.Feature))
                            {
                                this.Feature = string.Empty;
                            }
                            else
                            {
                                this.playlist.Add(new PlaylistEntry(PlaylistType.Feature, this.Feature));
                            }
                        }
                        else if (line.Contains(this.prerollPath))
                        {
                            if (hasTrailer)
                            {
                                this.Commercial = line.Substring(line.LastIndexOf(",", StringComparison.Ordinal) + 1);
                                this.playlist.Add(new PlaylistEntry(PlaylistType.Commercial, this.Commercial));
                                this.CommercialEnabled = true;
                            }
                            else
                            {
                                this.PrerollPlaylist.Add(line.Substring(line.LastIndexOf(",", StringComparison.Ordinal) + 1));
                                this.playlist.Add(new PlaylistEntry(PlaylistType.Preroll, line.Substring(line.LastIndexOf(",", StringComparison.Ordinal) + 1)));
                                this.PrerollEnabled = true;
                            }
                        }
                    }
                }
            }

            OnPlaylistChanged();
        }

        public void CreatePlaylist()
        {
            string path = VIDEO_PATH + PLAYLIST;

            using (var writer = new StreamWriter(path, false)) {
                lock (this.playlistLock) {
                    this.playlist.Clear();
                    writer.WriteLine(MPCPLAYLIST);
                    int i = 1;
                    if (this.PrerollEnabled == true) {
                        foreach (string s in this.PrerollPlaylist) {
                            this.playlist.Add(new PlaylistEntry(PlaylistType.Preroll, s));
                            //writer.WriteLine(string.Format(CultureInfo.InvariantCulture, MPC_FORMAT_TYPE, i));
                            writer.WriteLine(string.Format(CultureInfo.InvariantCulture, MPC_FORMAT_FILE, i, s));
                            i++;
                        }
                    }
                    if (this.TrailersEnabled == true) {
                        foreach (string s in this.TrailerPlaylist) {
                            this.playlist.Add(new PlaylistEntry(PlaylistType.Trailer, s));
                            //writer.WriteLine(string.Format(CultureInfo.InvariantCulture, MPC_FORMAT_TYPE, i));
                            writer.WriteLine(string.Format(CultureInfo.InvariantCulture, MPC_FORMAT_FILE, i, s));
                            i++;
                        }
                    }
                    if (this.CommercialEnabled == true && !string.IsNullOrEmpty(this.Commercial)) {
                        this.playlist.Add(new PlaylistEntry(PlaylistType.Commercial, this.Commercial));
                        //writer.WriteLine(string.Format(CultureInfo.InvariantCulture, MPC_FORMAT_TYPE, i));
                        writer.WriteLine(string.Format(CultureInfo.InvariantCulture, MPC_FORMAT_FILE, i, this.Commercial));
                        i++;
                    }
                    if (!string.IsNullOrEmpty(this.Feature)) {
                        this.playlist.Add(new PlaylistEntry(PlaylistType.Feature, this.Feature));
                        //writer.WriteLine(string.Format(CultureInfo.InvariantCulture, MPC_FORMAT_TYPE, i));
                        writer.WriteLine(string.Format(CultureInfo.InvariantCulture, MPC_FORMAT_FILE, i, this.Feature));
                    }
                    writer.Flush();
                }
            }

            OnPlaylistChanged();
        }

        /// <summary>
        /// Raised when playlist has been changed.
        /// Not marshalled to UI thread.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnPlaylistChanged()
        {
            if (this.dispatcher.CheckAccess()) {
                PlaylistChanged?.Invoke(this, VIDEO_PATH + PLAYLIST);
            } else {
                this.dispatcher.BeginInvoke((Action)(() => {
                    PlaylistChanged?.Invoke(this, VIDEO_PATH + PLAYLIST);
                }));
            }
        }

        private void ParseFeature()
        {
            this.FeatureTitle = string.Empty;
            this.FeatureYear = string.Empty;
            this.FeatureExtendedInfo = string.Empty;

            if (string.IsNullOrEmpty(this.Feature)) { return; }

            bool isYearPart = false;
            bool isExInfoPart = false;
            int len = this.Feature.LastIndexOf("_", StringComparison.Ordinal);
            int start = this.Feature.LastIndexOf("\\", StringComparison.Ordinal) + 1;
            int lastSpace = 0;

            if (len < 0) { len = this.Feature.LastIndexOf(".", StringComparison.Ordinal); }

            var title = new StringBuilder((len - start < 0)? 0 : len - start);
            var year = new StringBuilder(4);
            var exInfo = new StringBuilder(20);
            
            for (int i = start; i < len; i++) {
                char c = this.Feature[i];

                if (isYearPart) {
                    if (c == ')') {
                        isYearPart = false;
                    } else {
                        year.Append(c);
                    }
                } else if (isExInfoPart) {
                    if (c == ']') {
                        isExInfoPart = false;
                    } else {
                        if ((c == '_') || (c == '-')) {
                            exInfo.Append(' ');
                        } else {
                            exInfo.Append(c);
                        }
                    }
                } else {
                    switch (c) {
                        case '(': isYearPart = true; break;
                        case '[': isExInfoPart = true; break;
                        case '-':
                        case '_': title.Append(' '); lastSpace = title.Length; break;
                        default:
                            title.Append(c);
                            break;
                    }
                }
            }

            if ((title.Length > 0) && (title.Length == lastSpace)) {
                title.Remove(lastSpace - 1, 1);
            }
            
            this.FeatureTitle = title.ToString();
            this.FeatureYear = year.ToString();
            this.FeatureExtendedInfo = exInfo.ToString();
        }

        private void ParseFormat()
        {
            this.FeatureVideoFormat = VideoFormat.Unknown;
            this.FeatureAudioFormat = AudioFormat.Unknown;
            this.FeatureHDRFormat = HDR.Unknown;
            this.FeatureIs3D = false;
            this.FeatureChannelFormat = "Mono";


            var MIW = new MediaInfoWrapper(this.feature);
            var resolution = MIW.VideoStreams[0].Width;
            var SurroundFormat = MIW.AudioCodec;
            var audioChannels = MIW.AudioChannels;
            var hdr = MIW.VideoStreams[0].Hdr;
            var StereoMode = MIW.VideoStreams[0].Stereoscopic;
            //Output.WriteLine(hdr.ToString());

            if (string.IsNullOrEmpty(this.Feature)) { return; }

            var resolutions = new Dictionary<int, string>()
            {
                {1280, "HD"},
                {1920, "FHD"},
                {2560, "QHD"},
                {3840, "UHD"},
                {3996, "UHD"},
                {4096, "UHD"},
                {7680, "EightK"},
            };

            var aFormats = new Dictionary<string, string>()
            {
                {"Ac3Atmos", "DolbyAtmos"},
                {"Dts", "DTS"},
                {"DTS", "DTS"},
                {"Ac3", "DolbyDigital"},
                {"Truehd", "TrueHD"},
                {"TruehdAtmos", "TrueHDAtmos"},
                {"DtsX", "DTSX"},
                {"DtsHdMa", "DTSHDMA"},
                {"DtsHd", "DTSHD"},
                {"Eac3", "DolbyDigitalPlus"},
                {"E-AC-3", "DolbyDigitalPlus"},
                {"Eac3Atmos", "DolbyDigitalPlusAtmos"},
            };

            var Channels = new Dictionary<int, string>()
            {
                { 1, "Mono" },
                { 2, "Stereo" },
                { 3, "2.1" },
                { 4, "4.0" },
                { 5, "5.0" },
                { 6, "5.1" },
                { 7, "6.1" },
                { 8, "7.1" },
                { 9, "7.2" },
                { 10, "7.2.1" }
            };

            var StereoModeFormat = new Dictionary<MediaInfo.Model.StereoMode, bool>()
            {
                { MediaInfo.Model.StereoMode.Mono, false },
                { MediaInfo.Model.StereoMode.SideBySideLeft, true },
                { MediaInfo.Model.StereoMode.TopBottomRight, true },
                { MediaInfo.Model.StereoMode.TopBottomLeft, true },
                { MediaInfo.Model.StereoMode.CheckerboardRight, true },
                { MediaInfo.Model.StereoMode.CheckerboardLeft, true },
                { MediaInfo.Model.StereoMode.RowInterleavedRight, true },
                { MediaInfo.Model.StereoMode.RowInterleavedLeft, true },
                { MediaInfo.Model.StereoMode.ColumnInterleavedRight, true },
                { MediaInfo.Model.StereoMode.ColumnInterleavedLeft, true },
                { MediaInfo.Model.StereoMode.AnaglyphCyanRed, true },
                { MediaInfo.Model.StereoMode.SideBySideRight, true },
                { MediaInfo.Model.StereoMode.AnaglyphGreenMagenta, true },
                { MediaInfo.Model.StereoMode.BothEyesLacedLeft, true },
                { MediaInfo.Model.StereoMode.BothEyesLacedRight, true }
            };

            var videoFormat = resolutions.ContainsKey(resolution) ? resolutions[resolution] : "DVD";
            var audioFormat = aFormats.ContainsKey(SurroundFormat) ? aFormats[SurroundFormat] : "DolbyDigital";
            var channelFormat = Channels[audioChannels];
            var StereoFormatted = StereoModeFormat[StereoMode];
            Output.WriteLine(channelFormat);
            Output.WriteLine(SurroundFormat);
            Output.WriteLine(audioFormat);
            Output.WriteLine(StereoMode.ToString() + " - " + StereoFormatted.ToString());

            switch (videoFormat.ToString()) {
                case HD: this.FeatureVideoFormat = VideoFormat.HD; break;
                case FHD: this.FeatureVideoFormat = VideoFormat.FHD; break;
                case QHD: this.FeatureVideoFormat = VideoFormat.QHD; break;
                case UHD: this.FeatureVideoFormat = VideoFormat.UHD; break;
                case EightK: this.FeatureVideoFormat = VideoFormat.EightK; break;
                case DVD: this.FeatureVideoFormat = VideoFormat.DVD; break;
                default: this.FeatureVideoFormat = VideoFormat.Unknown; break;
            }

            switch (audioFormat.ToString()) {
                case DolbyAtmos: this.FeatureAudioFormat = AudioFormat.DolbyAtmos; break;
                case DTS: this.FeatureAudioFormat = AudioFormat.DTS; break;
                case DolbyDigital: this.FeatureAudioFormat = AudioFormat.DolbyDigital; break;
                case TrueHD: this.FeatureAudioFormat = AudioFormat.TrueHD; break;
                case DTSX: this.FeatureAudioFormat = AudioFormat.DTSX; break;
                case DTSHDMA: this.FeatureAudioFormat = AudioFormat.DTSHDMA; break;
                case DTSHD: this.FeatureAudioFormat = AudioFormat.DTSHD; break;
                case DolbyDigitalPlus: this.FeatureAudioFormat = AudioFormat.DolbyDigitalPlus; break;
                default: this.FeatureAudioFormat = AudioFormat.Unknown; break;
            }

            switch (hdr.ToString())
            {
                case HDR10: this.FeatureHDRFormat = HDR.HDR10; break;
                case HDR10Plus: this.FeatureHDRFormat = HDR.HDR10Plus; break;
                case DolbyVision: this.FeatureHDRFormat = HDR.DolbyVision; break;
                case HLG: this.FeatureHDRFormat = HDR.HLG; break;
                case SLHDR1: this.FeatureHDRFormat = HDR.SLHDR1; break;
                case SLHDR2: this.FeatureHDRFormat = HDR.SLHDR2; break;
                case SLHDR3: this.FeatureHDRFormat = HDR.SLHDR3; break;
                default: this.FeatureHDRFormat = HDR.Unknown; break;
            }

            switch (channelFormat.ToString())
            {
                case "Mono":
                    this.FeatureChannelFormat = "Mono";
                    break;
                case "Stereo":
                    this.FeatureChannelFormat = "Stereo";
                    break;
                case "2.1":
                    this.FeatureChannelFormat = "2.1";
                    break;
                case "4.0": 
                    this.FeatureChannelFormat = "4.0";
                    break;
                case "5.0":
                    this.FeatureChannelFormat = "5.0";
                    break;
                case "5.1":
                    this.FeatureChannelFormat = "5.1";
                    break;
                case "6.1":
                    this.FeatureChannelFormat = "6.1";
                    break;
                case "7.1":
                    this.FeatureChannelFormat = "7.1";
                    break;
                case "7.2":
                    this.FeatureChannelFormat = "7.2";
                    break;
                case "7.2.1":
                    this.FeatureChannelFormat = "7.2.1";
                    break;
                default:
                    this.FeatureChannelFormat = "";
                    break;
            }

            switch (StereoFormatted)
            {
                case true:
                    this.FeatureIs3D = true;
                    break;
                case false:
                    this.FeatureIs3D = false;
                    break;
                default:
                    this.FeatureIs3D = false;
                    break;
            }

            Output.WriteLine(this.FeatureVideoFormat.ToString());
        }

        #endregion

        #region Events

        public event EventHandler<string> PlaylistChanged;

        #endregion

        #region Properties

        public bool PrerollEnabled { get; set; }

        public bool TrailersEnabled { get; set; }

        public bool CommercialEnabled { get; set; }

        public List<string> PrerollPlaylist { get; set; }

        public List<string> TrailerPlaylist { get; set; }

        public string Commercial { get; set; }

        public string Feature
        {
            get { return this.feature; }
            set {
                this.feature = value;
                ParseFeature();
                ParseFormat();
            }
        }

        public string FeatureTitle { get; private set; }

        public string FeatureYear { get; private set; }

        public string FeatureExtendedInfo { get; private set; }

        public VideoFormat FeatureVideoFormat { get; private set; }

        public AudioFormat FeatureAudioFormat { get; private set; }

        public string FeatureChannelFormat { get; private set; }

        public bool FeatureIs3D { get; private set; }

        public HDR FeatureHDRFormat { get; private set; }

        public ObservableCollection<PlaylistEntry> Playlist
        {
            get { return this.playlist; }
        }

        #endregion

    }

}

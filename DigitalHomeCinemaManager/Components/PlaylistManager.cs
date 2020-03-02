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

namespace DigitalHomeCinemaManager.Components
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text;

    internal sealed class PlaylistManager
    {

        #region Members

        private static string PLAYLIST = "\\Show_Playlist.mpcpl";
        private static string VIDEO_PATH = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);

        private const string MPCPLAYLIST = "MPCPLAYLIST";
        private const string MPC_FORMAT_TYPE = "{0},type,0";
        private const string MPC_FORMAT_FILE = "{0},filename,{1}";
        private const string SD = "SD";
        private const string HD = "HD";
        private const string UHD = "UHD";
        private const string ATMOS = "ATMOS";
        private const string DTS = "DTS";

        private string prerollPath = Properties.Settings.Default.PrerollPath;
        private string trailerPath = Properties.Settings.Default.TrailerPath;
        private string mediaPath = Properties.Settings.Default.MediaPath;
        private ObservableCollection<PlaylistEntry> playlist = new ObservableCollection<PlaylistEntry>();
        private string feature;
        

        #endregion

        #region Constructor

        internal PlaylistManager()
        {
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
                while ((line = reader.ReadLine()) != null) {
                    if (line.Contains(this.trailerPath)) {
                        hasTrailer = true;
                        this.TrailerPlaylist.Add(line.Substring(line.LastIndexOf(",", StringComparison.Ordinal) + 1));
                        this.playlist.Add(new PlaylistEntry(PlaylistType.Trailer, line.Substring(line.LastIndexOf(",", StringComparison.Ordinal) + 1)));
                        this.TrailersEnabled = true;
                    } else if (line.Contains(this.mediaPath)) {
                        this.Feature = line.Substring(line.LastIndexOf(",", StringComparison.Ordinal) + 1);
                        if (!File.Exists(this.Feature)) {
                            this.Feature = string.Empty;
                        } else {
                            this.playlist.Add(new PlaylistEntry(PlaylistType.Feature, this.Feature));
                        }
                    } else if (line.Contains(this.prerollPath)) {
                        if (hasTrailer) {
                            this.Commercial = line.Substring(line.LastIndexOf(",", StringComparison.Ordinal) + 1);
                            this.playlist.Add(new PlaylistEntry(PlaylistType.Commercial, this.Commercial));
                            this.CommercialEnabled = true;
                        } else {
                            this.PrerollPlaylist.Add(line.Substring(line.LastIndexOf(",", StringComparison.Ordinal) + 1));
                            this.playlist.Add(new PlaylistEntry(PlaylistType.Preroll, line.Substring(line.LastIndexOf(",", StringComparison.Ordinal) + 1)));
                            this.PrerollEnabled = true;
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
                this.playlist.Clear();
                writer.WriteLine(MPCPLAYLIST);
                int i = 1;
                if (this.PrerollEnabled == true) {
                    foreach (string s in this.PrerollPlaylist) {
                        this.playlist.Add(new PlaylistEntry(PlaylistType.Preroll, s));
                        writer.WriteLine(string.Format(CultureInfo.InvariantCulture, MPC_FORMAT_TYPE, i));
                        writer.WriteLine(string.Format(CultureInfo.InvariantCulture, MPC_FORMAT_FILE, i, s));
                        i++;
                    }
                }
                if (this.TrailersEnabled == true) {
                    foreach (string s in this.TrailerPlaylist) {
                        this.playlist.Add(new PlaylistEntry(PlaylistType.Trailer, s));
                        writer.WriteLine(string.Format(CultureInfo.InvariantCulture, MPC_FORMAT_TYPE,  i));
                        writer.WriteLine(string.Format(CultureInfo.InvariantCulture, MPC_FORMAT_FILE, i, s));
                        i++;
                    }
                }
                if (this.CommercialEnabled == true && !string.IsNullOrEmpty(this.Commercial)) {
                    this.playlist.Add(new PlaylistEntry(PlaylistType.Commercial, this.Commercial));
                    writer.WriteLine(string.Format(CultureInfo.InvariantCulture, MPC_FORMAT_TYPE, i));
                    writer.WriteLine(string.Format(CultureInfo.InvariantCulture, MPC_FORMAT_FILE, i, this.Commercial));
                    i++;
                }
                if (!string.IsNullOrEmpty(this.Feature)) {
                    this.playlist.Add(new PlaylistEntry(PlaylistType.Feature, this.Feature));
                    writer.WriteLine(string.Format(CultureInfo.InvariantCulture, MPC_FORMAT_TYPE, i));
                    writer.WriteLine(string.Format(CultureInfo.InvariantCulture, MPC_FORMAT_FILE, i, this.Feature));
                }
                writer.Flush();
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
            PlaylistChanged?.Invoke(this, VIDEO_PATH + PLAYLIST);
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
            var title = new StringBuilder(len - start);
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

            if (title.Length == lastSpace) {
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

            if (string.IsNullOrEmpty(this.Feature)) { return; }

            int start = this.Feature.LastIndexOf("_", StringComparison.Ordinal) + 1;
            int len = this.Feature.LastIndexOf(".", StringComparison.Ordinal);
            bool isAudioFormat = false;

            var videoFormat = new StringBuilder(3);
            var audioFormat = new StringBuilder(5);

            for (int i = start; i < len; i++) {
                if (isAudioFormat) {
                    if (this.Feature[i] == '.') {
                        isAudioFormat = false;
                    } else {
                        audioFormat.Append((char.IsUpper(this.Feature[i])) ? this.Feature[i] : char.ToUpper(this.Feature[i], CultureInfo.InvariantCulture));
                    }
                } else {
                    switch (this.Feature[i]) {
                        case '-': isAudioFormat = true; break;
                        default:
                            videoFormat.Append((char.IsUpper(this.Feature[i])) ? this.Feature[i] : char.ToUpper(this.Feature[i], CultureInfo.InvariantCulture));
                            break;
                    }
                }
            }

            switch (videoFormat.ToString()) {
                case SD: this.FeatureVideoFormat = VideoFormat.SD; break;
                case HD: this.FeatureVideoFormat = VideoFormat.HD; break;
                case UHD: this.FeatureVideoFormat = VideoFormat.UHD; break;
                default: this.FeatureVideoFormat = VideoFormat.Unknown; break;
            }

            switch (audioFormat.ToString()) {
                case ATMOS: this.FeatureAudioFormat = AudioFormat.Atmos; break;
                case DTS: this.FeatureAudioFormat = AudioFormat.DTS; break;
                default: this.FeatureAudioFormat = AudioFormat.Dolby; break;
            }
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

        public ObservableCollection<PlaylistEntry> Playlist
        {
            get { return this.playlist; }
        }

        #endregion

    }

}

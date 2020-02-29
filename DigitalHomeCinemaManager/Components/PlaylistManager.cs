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
    using System.IO;
    using System.Runtime.CompilerServices;

    #region PlaylistType

    public enum PlaylistType
    {
        Preroll,
        Trailer,
        Commercial,
        Feature
    }

    #endregion

    #region PlaylistEntry

    public struct PlaylistEntry
    {
        public PlaylistEntry(PlaylistType playlistType, string filename)
        {
            this.PlaylistType = playlistType;
            this.FileName = filename;
        }

        public PlaylistType PlaylistType { get; private set; }
        public string FileName { get; private set; }
    }

    #endregion

    #region VideoFormat

    public enum VideoFormat
    {
        Unknown,
        SD,
        HD,
        UHD,
    }

    #endregion

    #region AudioFormat

    public enum AudioFormat
    {
        Unknown,
        Atmos,
        DTS,
        Dolby,
    }

    #endregion

    public class PlaylistManager
    {

        #region Members

        private static string PLAYLIST = "\\Show_Playlist.mpcpl";
        private static string VIDEO_PATH = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);

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
                        this.TrailerPlaylist.Add(line.Substring(line.LastIndexOf(",") + 1));
                        this.playlist.Add(new PlaylistEntry(PlaylistType.Trailer, line.Substring(line.LastIndexOf(",") + 1)));
                        this.TrailersEnabled = true;
                    } else if (line.Contains(this.mediaPath)) {
                        this.Feature = line.Substring(line.LastIndexOf(",") + 1);
                        if (!File.Exists(this.Feature)) {
                            this.Feature = string.Empty;
                        } else {
                            this.playlist.Add(new PlaylistEntry(PlaylistType.Feature, this.Feature));
                        }
                    } else if (line.Contains(this.prerollPath)) {
                        if (hasTrailer) {
                            this.Commercial = line.Substring(line.LastIndexOf(",") + 1);
                            this.playlist.Add(new PlaylistEntry(PlaylistType.Commercial, this.Commercial));
                            this.CommercialEnabled = true;
                        } else {
                            this.PrerollPlaylist.Add(line.Substring(line.LastIndexOf(",") + 1));
                            this.playlist.Add(new PlaylistEntry(PlaylistType.Preroll, line.Substring(line.LastIndexOf(",") + 1)));
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
                writer.WriteLine("MPCPLAYLIST");
                int i = 1;
                if (this.PrerollEnabled == true) {
                    foreach (string s in this.PrerollPlaylist) {
                        this.playlist.Add(new PlaylistEntry(PlaylistType.Preroll, s));
                        writer.WriteLine(i.ToString() + ",type,0");
                        writer.WriteLine(i.ToString() + ",filename," + s);
                        i++;
                    }
                }
                if (this.TrailersEnabled == true) {
                    foreach (string s in this.TrailerPlaylist) {
                        this.playlist.Add(new PlaylistEntry(PlaylistType.Trailer, s));
                        writer.WriteLine(i.ToString() + ",type,0");
                        writer.WriteLine(i.ToString() + ",filename," + s);
                        i++;
                    }
                }
                if (this.CommercialEnabled == true && !string.IsNullOrEmpty(this.Commercial)) {
                    this.playlist.Add(new PlaylistEntry(PlaylistType.Commercial, this.Commercial));
                    writer.WriteLine(i.ToString() + ",type,0");
                    writer.WriteLine(i.ToString() + ",filename," + this.Commercial);
                    i++;
                }
                if (!string.IsNullOrEmpty(this.Feature)) {
                    this.playlist.Add(new PlaylistEntry(PlaylistType.Feature, this.Feature));
                    writer.WriteLine(i.ToString() + ",type,0");
                    writer.WriteLine(i.ToString() + ",filename," + this.Feature);
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
        protected void OnPlaylistChanged()
        {
            PlaylistChanged?.Invoke(this, VIDEO_PATH + PLAYLIST);
        }

        private void ParseFeature()
        {
            this.FeatureTitle = string.Empty;
            this.FeatureYear = string.Empty;
            this.FeatureExtendedInfo = string.Empty;
            this.FeatureVideoFormat = VideoFormat.Unknown;
            this.FeatureAudioFormat = AudioFormat.Unknown;

            if (string.IsNullOrEmpty(this.Feature)) {
                return;
            }

            string title = this.Feature.Substring(this.Feature.LastIndexOf("\\") + 1);
            string year = string.Empty;
            string exInfo = string.Empty;

            if (title.Contains(".")) { // strip file extension
                title = title.Substring(0, title.IndexOf(".")).Trim();
            }
            if (title.Contains("[")) { // strip brackets
                // consider any text in brackets to be purely informational and not part of title
                // ex: The_Martian_[Extended_Edition]_UHD-Atmos.mkv
                exInfo = title.Substring(title.IndexOf("[") + 1);
                exInfo = exInfo.Substring(0, exInfo.IndexOf("]")).Trim();
                title = title.Substring(0, title.IndexOf("[")).Trim();
            }
            if (title.Contains("_")) { // replace underscores with spaces
                title = title.Substring(0, title.LastIndexOf("_")).Replace("_", " ").Trim();
            }
            if (title.Contains("(") && title.Contains(")")) { // process year filter
                // consider any text in parenthesis to be year of release to aid searching
                // ex: Ghost_in_the_Shell_(2017)_UHD-Atmos.mkv
                year = title.Substring(title.IndexOf("(") + 1);
                year = year.Substring(0, year.IndexOf(")")).Trim();
                title = title.Substring(0, title.IndexOf("(")).Trim();
            }

            this.FeatureTitle = title;
            this.FeatureYear = year;
            this.FeatureExtendedInfo = exInfo;

            string[] format = this.Feature.Substring(this.Feature.LastIndexOf("_") + 1).Split('-');
            if (format != null && format.Length > 0) {
                switch (format[0].ToUpper()) {
                    case "SD": this.FeatureVideoFormat = VideoFormat.SD; break;
                    case "HD": this.FeatureVideoFormat = VideoFormat.HD; break;
                    case "UHD": this.FeatureVideoFormat = VideoFormat.UHD; break;
                    default: this.FeatureVideoFormat = VideoFormat.Unknown; break;
                }
            }

            if (format != null && format.Length == 2) {
                string s = format[1].Substring(0, format[1].IndexOf("."));
                switch (s.ToUpper()) {
                    case "ATMOS": this.FeatureAudioFormat = AudioFormat.Atmos; break;
                    case "DTS": this.FeatureAudioFormat = AudioFormat.DTS; break;
                    default: this.FeatureAudioFormat = AudioFormat.Dolby; break;
                }
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

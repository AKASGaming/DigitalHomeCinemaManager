﻿/*
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

namespace DigitalHomeCinemaControl.Controllers.Base
{
    using DigitalHomeCinemaControl.Components;

    /// <summary>
    /// Abstract SourceController class.
    /// </summary>
    public abstract class SourceController : DeviceController, ISourceController
    {

        #region Members

        public const string CURRENTFILE = "CurrentFile";
        public const string FILESIZE = "FileSize";
        public const string POSITION = "Position";
        public const string DURATION = "Duration";
        public const string CURRENTPOSITION = "CurrentPosition";
        public const string LENGTH = "Length";

        private PlaybackState state;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new instance of the SourceController class.
        /// </summary>
        protected SourceController()
            : base()
        {
            this.Path = string.Empty;
            this.FullscreenDisplay = -1;
            this.FullscreenDisplayID = string.Empty;
            this.Port = 13579;
            this.VLCPassword = string.Empty;

            this.DataSource.Add(new BindingItem<string>(CURRENTFILE));
            this.DataSource.Add(new BindingItem<string>(FILESIZE));
            this.DataSource.Add(new BindingItem<string>(POSITION));
            this.DataSource.Add(new BindingItem<string>(DURATION));
            this.DataSource.Add(new BindingItem<int>(CURRENTPOSITION));
            this.DataSource.Add(new BindingItem<int>(LENGTH));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Opens the specified playlist file in the source player.
        /// </summary>
        /// <param name="playlist">The playlist file to open.</param>
        public abstract void OpenPlaylist(string playlist, string feature = "");

        /// <summary>
        /// Start or resume playback.
        /// </summary>
        public abstract void Play();

        /// <summary>
        /// Pause playback.
        /// </summary>
        public abstract void Pause();

        /// <summary>
        /// Stop playback.
        /// </summary>
#pragma warning disable  CA1716// Identifiers should not match keywords
        public abstract void Stop();
#pragma warning restore CA1716

        /// <summary>
        /// Skip backwards in the current track.
        /// </summary>
        public abstract void Rewind();

        /// <summary>
        /// Skip forwards in the current track.
        /// </summary>
        public abstract void FastForward();

        /// <summary>
        /// Jump to the previous track.
        /// </summary>
        public abstract void Previous();

        /// <summary>
        /// Jump to the next track.
        /// </summary>
#pragma warning disable CA1716 // Identifiers should not match keywords
        public abstract void Next();
#pragma warning restore CA1716

        /// <summary>
        /// Toggle subtitles on/off.
        /// </summary>
        public abstract void Subtitles();

        /// <summary>
        /// Toggle mute on/off.
        /// </summary>
        public abstract void Mute();

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current PlaybackState.
        /// </summary>
        public PlaybackState State
        {
            get { return this.state; }
            protected set {
                if (value != this.state) {
                    this.state = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or Sets the path to the player executable.
        /// </summary>
        public string Path
        {
            get { return GetSetting<string>(); }
            set { Setting(value); }
        }

        /// <summary>
        /// Gets or Sets the display number to open player in fullscreen mode. 
        /// </summary>
        public int FullscreenDisplay
        {
            get { return GetSetting<int>(); }
            set { Setting(value); }
        }

        /// <summary>
        /// Gets or Sets the display ID for VLC in fullscreen mode. 
        /// </summary>
        public string FullscreenDisplayID
        {
            get { return GetSetting<string>(); }
            set { Setting(value); }
        }

        /// <summary>
        /// Gets or Sets the port for your player. 
        /// </summary>
        public int Port
        {
            get { return GetSetting<int>(); }
            set { Setting(value); }
        }

        /// <summary>
        /// Gets or Sets the password for VLC. 
        /// </summary>
        public string VLCPassword
        {
            get { return GetSetting<string>(); }
            set { Setting(value); }
        }

        #endregion

    }

}

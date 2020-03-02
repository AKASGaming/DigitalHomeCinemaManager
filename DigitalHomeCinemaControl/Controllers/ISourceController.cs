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

namespace DigitalHomeCinemaControl.Controllers
{
    
    /// <summary>
    /// Defines the interface that source controllers implement.
    /// </summary>
    public interface ISourceController : IController
    {

        /// <summary>
        /// Opens the specified playlist file in the source player.
        /// Note: This method may be called before Connect().
        /// </summary>
        /// <param name="playlist">The playlist file to open.</param>
        /// <param name="feature">Optional. The filename of the feature in the playlist.</param>
        void OpenPlaylist(string playlist, string feature = "");
       
        /// <summary>
        /// Start or resume playback.
        /// </summary>
        void Play();

        /// <summary>
        /// Pause playback.
        /// </summary>
        void Pause();

        /// <summary>
        /// Stop playback.
        /// </summary>
#pragma warning disable CA1716 // Identifiers should not match keywords
        void Stop();
#pragma warning restore CA1716

        /// <summary>
        /// Skip backwards in the current track.
        /// </summary>
        void Rewind();

        /// <summary>
        /// Skip forwards in the current track.
        /// </summary>
        void FastForward();

        /// <summary>
        /// Jump to the previous track.
        /// </summary>
        void Previous();

        /// <summary>
        /// Jump to the next track.
        /// </summary>
#pragma warning disable CA1716 // Identifiers should not match keywords
        void Next();
#pragma warning restore CA1716

        /// <summary>
        /// Toggle subtitles on/off.
        /// </summary>
        void Subtitles();

        /// <summary>
        /// Toggle mute on/off.
        /// </summary>
        void Mute();

        /// <summary>
        /// Gets the current PlaybackState.
        /// Must raise the PropertyChanged event.
        /// </summary>
        PlaybackState State { get; }

    }

}

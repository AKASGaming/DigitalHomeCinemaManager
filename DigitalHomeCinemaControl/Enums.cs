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

namespace DigitalHomeCinemaControl
{
    using System.ComponentModel;

    /// <summary>
    /// Enumerates values used to specify a controllers status.
    /// </summary>
    public enum ControllerStatus
    {
        Disconnected,
        Ok,
        On,
        Standby,
        Error,
        Startup,
        Cooling,
    }

    /// <summary>
    /// Enumerates values used to specify a projectors lamp status.
    /// </summary>
    public enum LampStatus
    {
        [Description("---")]
        Unknown,
        Off,
        Low,
        High,
        [Description("N/A")]
        NotApplicable,
    }

    /// <summary>
    /// Enumerates values used to define a display type.
    /// </summary>
    public enum DisplayType
    {
        Projector,
        LCD,
    }

    /// <summary>
    /// Enumerates values used to specify the playback state of a source device.
    /// </summary>
    public enum PlaybackState
    {
        Unknown,
        Playing,
        PlayingFeature,
        Paused,
        Stopped,
    }

    /// <summary>
    /// Enumerates values used to specify an audio channel.
    /// </summary>
    public enum AudioChannel
    {
        Left,
        Right,
        Center,
        Subwoofer,
        SurroundLeft,
        SurroundRight,
        SurroundBackLeft,
        SurroundBackRight,
        TopFrontLeft,
        TopFrontRight,
        TopBackLeft,
        TopBackRight,
        FrontWideLeft,
        FrontWideRight,
        TopMiddleLeft,
        TopMiddleRight,
        VoiceOfGod
    }

}

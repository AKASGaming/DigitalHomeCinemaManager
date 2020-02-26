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

namespace DigitalHomeCinemaControl.Controllers.Providers.Denon
{
    using DigitalHomeCinemaControl.Controllers.Providers.Denon.Avr;

    internal static class Extensions
    {

        #region Methods

        public static ControllerStatus ToControllerStatus(this PowerStatus status)
        {
            ControllerStatus result;

            switch (status) {
                case PowerStatus.On:
                    result = ControllerStatus.On;
                    break;
                case PowerStatus.Standby:
                    result = ControllerStatus.Standby;
                    break;
                default:
                    result = ControllerStatus.Ok;
                    break;
            }

            return result;
        }

        /// <summary>
        /// Converts a Denon.Avr.Channel to a DigitalHomeCinemaControl.AudioChannel
        /// </summary>
        /// <remarks>
        /// In the future it might be worth optimizing this, by doing either an Enum Name
        /// mapping or with a custom attribute (or both).
        /// </remarks>
        /// <param name="channel"></param>
        /// <returns></returns>
        public static AudioChannel ToAudioChannel(this Channel channel)
        {
            AudioChannel result;

            switch (channel) {
                case Channel.Center: result = AudioChannel.Center; break;
                case Channel.FrontWideLeft: result = AudioChannel.FrontWideLeft; break;
                case Channel.FrontWideRight: result = AudioChannel.FrontWideRight; break;
                case Channel.Left: result = AudioChannel.Left; break;
                case Channel.Right: result = AudioChannel.Right; break;
                case Channel.Subwoofer: result = AudioChannel.Subwoofer; break;
                case Channel.SurroundBackLeft: result = AudioChannel.SurroundBackLeft; break;
                case Channel.SurroundBackRight: result = AudioChannel.SurroundBackRight; break;
                case Channel.SurroundLeft: result = AudioChannel.SurroundLeft; break;
                case Channel.SurroundRight: result = AudioChannel.SurroundRight; break;
                case Channel.TopBackLeft: result = AudioChannel.TopBackLeft; break;
                case Channel.TopBackRight: result = AudioChannel.TopBackRight; break;
                case Channel.TopFrontLeft: result = AudioChannel.TopFrontLeft; break;
                case Channel.TopFrontRight: result = AudioChannel.TopFrontRight; break;
                case Channel.TopMiddleLeft: result = AudioChannel.TopMiddleLeft; break;
                case Channel.TopMiddleRight: result = AudioChannel.TopMiddleRight; break;
                case Channel.VoiceOfGod: result = AudioChannel.VoiceOfGod; break;
                default: result = default; break;
            }

            return result;
        }

        #endregion

    }

}

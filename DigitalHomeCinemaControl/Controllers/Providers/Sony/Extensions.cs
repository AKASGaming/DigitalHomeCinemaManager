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

namespace DigitalHomeCinemaControl.Controllers.Providers.Sony
{
    using DigitalHomeCinemaControl.Controllers.Providers.Sony.Sdcp;

    internal static class Extensions
    {

        #region Methods

        public static ControllerStatus ToControllerStatus(this StatusPower status)
        {
            ControllerStatus result;

            switch (status) {
                case StatusPower.Cooling1:
                case StatusPower.Cooling2:
                    result = ControllerStatus.Cooling;
                    break;
                case StatusPower.Standby:
                    result = ControllerStatus.Standby;
                    break;
                case StatusPower.PowerOn:
                    result = ControllerStatus.On;
                    break;
                case StatusPower.Startup:
                case StatusPower.StartupLamp:
                    result = ControllerStatus.Startup;
                    break;
                default:
                    result = ControllerStatus.Ok;
                    break;
            }

            return result;
        }

        public static LampStatus ToLampStatus(this LampControl lamp)
        {
            LampStatus result;

            switch (lamp) {
                case LampControl.High: result = LampStatus.High; break;
                case LampControl.Low: result = LampStatus.Low; break;
                default: result = LampStatus.Unknown; break;
            }

            return result;
        }

        #endregion

    }

}

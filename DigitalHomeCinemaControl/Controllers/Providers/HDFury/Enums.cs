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

namespace DigitalHomeCinemaControl.Controllers.Providers.HDFury
{
    using System.ComponentModel;

    public enum Rx
    {
        [Description("Rx 0")]
        Input1 = 0,
        [Description("Rx 1")]
        Input2 = 1,
        [Description("Rx 2")]
        Input3 = 2,
        [Description("Rx 3")]
        Input4 = 3,
        [Description("Follow Tx0")]
        Follow = 4,
    }

}

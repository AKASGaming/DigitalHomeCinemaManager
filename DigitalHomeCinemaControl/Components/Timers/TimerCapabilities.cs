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

namespace DigitalHomeCinemaControl.Components.Timers
{
    using System.Runtime.InteropServices;

#pragma warning disable CA1815 // Override equals and operator equals on value types

    [StructLayout(LayoutKind.Sequential)]
    internal struct TimerCapabilities
    {
        public int periodMin;
        public int periodMax;
    }

#pragma warning restore CA1815

}

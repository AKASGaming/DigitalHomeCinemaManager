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
    using System;
    using System.Runtime.InteropServices;

    internal delegate void TimeProc(int id, int msg, int user, int param1, int param2);

    internal class NativeMethods
    {
#pragma warning disable IDE1006 // Naming Styles

        [DllImport("winmm.dll")]
        internal static extern int timeGetDevCaps(ref TimerCapabilities caps, int sizeOfTimerCaps);

        [DllImport("winmm.dll", SetLastError = true)]
        internal static extern int timeSetEvent(int delay, int resolution, TimeProc proc, ref UIntPtr user, int mode);

        [DllImport("winmm.dll")]
        internal static extern int timeKillEvent(int id);

#pragma warning restore IDE1006
    }

}

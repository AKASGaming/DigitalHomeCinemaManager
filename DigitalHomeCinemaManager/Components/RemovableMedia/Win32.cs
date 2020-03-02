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

namespace DigitalHomeCinemaManager.Components.RemovableMedia
{
    using System;
    using System.Runtime.InteropServices;

#pragma warning disable CA1051 // Do not declare visible instance fields
#pragma warning disable CA1707 // Identifiers should not contain underscores
#pragma warning disable CA1815 // Override equals and operator equals on value types

    // Structure with information for RegisterDeviceNotification.
    [StructLayout(LayoutKind.Sequential)]
    internal struct DEV_BROADCAST_HANDLE
    {
        public int dbch_size;
        public int dbch_devicetype;
        public int dbch_reserved;
        public IntPtr dbch_handle;
        public IntPtr dbch_hdevnotify;
        public Guid dbch_eventguid;
        public long dbch_nameoffset;
        public byte dbch_data;
        public byte dbch_data1;
    }

    // Struct for parameters of the WM_DEVICECHANGE message
    [StructLayout(LayoutKind.Sequential)]
    internal struct DEV_BROADCAST_VOLUME
    {
        public int dbcv_size;
        public int dbcv_devicetype;
        public int dbcv_reserved;
        public int dbcv_unitmask;
    }

#pragma warning restore CA1815 // Override equals and operator equals on value types
#pragma warning restore CA1707 // Identifiers should not contain underscores
#pragma warning restore CA1051 // Do not declare visible instance fields

}

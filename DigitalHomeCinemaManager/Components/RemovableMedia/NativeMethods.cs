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

    /// <summary>
    /// WinAPI functions
    /// </summary>        
    internal class NativeMethods
    {

        //
        // CreateFile  - MSDN
        const uint GENERIC_READ = 0x80000000;
        const uint OPEN_EXISTING = 3;
        const uint FILE_SHARE_READ = 0x00000001;
        const uint FILE_SHARE_WRITE = 0x00000002;
        const uint FILE_ATTRIBUTE_NORMAL = 128;
        const uint FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;
        static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        public static bool CloseDirectoryHandle(IntPtr handle)
        {
            return CloseHandle(handle);
        }

        /// <summary>
        /// Opens a directory, returns it's handle or zero.
        /// </summary>
        /// <param name="dirPath">path to the directory, e.g. "C:\\dir"</param>
        /// <returns>handle to the directory. Close it with CloseHandle().</returns>
        public static IntPtr OpenDirectory(string dirPath)
        {
            // open the existing file for reading          
            IntPtr handle = CreateFile(
                  dirPath,
                  GENERIC_READ,
                  FILE_SHARE_READ | FILE_SHARE_WRITE,
                  IntPtr.Zero,
                  OPEN_EXISTING,
                  FILE_FLAG_BACKUP_SEMANTICS | FILE_ATTRIBUTE_NORMAL,
                  IntPtr.Zero);

            if (handle == INVALID_HANDLE_VALUE) {
                return IntPtr.Zero;
            } else {
                return handle;
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr RegisterDeviceNotification(IntPtr hRecipient, IntPtr NotificationFilter, uint Flags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern uint UnregisterDeviceNotification(IntPtr hHandle);

        // should be "static extern unsafe"
        [DllImport("kernel32", CharSet = CharSet.Unicode, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        static extern IntPtr CreateFile(
              [MarshalAs(UnmanagedType.LPTStr)]
              string FileName,                    // file name
              uint DesiredAccess,                 // access mode
              uint ShareMode,                     // share mode
              IntPtr SecurityAttributes,          // Security Attributes
              uint CreationDisposition,           // how to create
              uint FlagsAndAttributes,            // file attributes
              IntPtr hTemplateFile                // handle to template file
        );

        [DllImport("kernel32", SetLastError = true)]
        static extern bool CloseHandle(IntPtr hObject);

    }

}

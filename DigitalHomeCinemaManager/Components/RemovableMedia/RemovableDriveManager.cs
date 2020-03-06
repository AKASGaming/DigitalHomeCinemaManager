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

/*
 * 
 * Original source from:  https://www.codeproject.com/articles/18062/detecting-usb-drive-removal-in-a-c-program
 * Refactored on 3/2020 by Bill Mandra
 * 
 */

namespace DigitalHomeCinemaManager.Components.RemovableMedia
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;   // required for Marshal
    using System.Windows.Forms;             // required for Message
    using Microsoft.Win32.SafeHandles;

    /// <summary>
    /// Detects insertion or removal of removable drives.
    /// Use it in 1 or 2 steps:
    /// 1) Create instance of this class in your project and add handlers for the
    /// DeviceArrived, DeviceRemoved and QueryRemove events.
    /// AND (if you do not want drive detector to creaate a hidden form))
    /// 2) Override WndProc in your form and call DriveDetector's WndProc from there. 
    /// If you do not want to do step 2, just use the DriveDetector constructor without arguments and
    /// it will create its own invisible form to receive messages from Windows.
    /// </summary>
    internal class RemovableDriveManager : IDisposable
    {

        #region Members

        private static string DRIVES = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        // Win32 constants
        private const int DBT_DEVTYP_VOLUME = 0x00000002; // drive type is logical volume
        private const int DBT_DEVTYP_HANDLE = 6;
        private const int BROADCAST_QUERY_DENY = 0x424D5144;
        private const int WM_DEVICECHANGE = 0x0219;
        private const int DBT_DEVICEARRIVAL = 0x8000; // system detected a new device
        private const int DBT_DEVICEQUERYREMOVE = 0x8001;   // Preparing to remove (any program can disable the removal)
        private const int DBT_DEVICEREMOVECOMPLETE = 0x8004; // removed 

        private IntPtr dirHandle = IntPtr.Zero; // New: 28.10.2007 - handle to root directory of flash drive which is opened for device notification
        private FileStream fileOnFlash = null; // Class which contains also handle to the file opened on the flash drive
        private string fileToOpen; // Name of the file to try to open on the removable drive for query remove registration
        private IntPtr deviceNotifyHandle; // Handle to file which we keep opened on the drive if query remove message is required by the client
        private IntPtr recipientHandle; // Handle of the window which receives messages from Windows. This will be a form.
        private string currentDrive; // Drive which is currently hooked for query remove
        private DetectorForm form;
        private volatile bool disposed = false;

        #endregion

        #region Constructor

        /// <summary>
        /// The easiest way to use DriveDetector. 
        /// It will create hidden form for processing Windows messages about USB drives
        /// You do not need to override WndProc in your form.
        /// </summary>
        public RemovableDriveManager()
        {
            this.form = new DetectorForm(this);
            this.form.Show(); // will be hidden immediatelly

            Initialize(this.form, null);
        }

        #endregion

        #region Methods

        /// <summary>
        /// init the DriveDetector object
        /// </summary>
        /// <param name="intPtr"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Initialize(Control control, string fileToOpen)
        {
            this.fileToOpen = fileToOpen;
            this.fileOnFlash = null;
            this.deviceNotifyHandle = IntPtr.Zero;
            this.recipientHandle = control.Handle;
            this.dirHandle = IntPtr.Zero;   // handle to the root directory of the flash drive which we open 
            this.currentDrive = string.Empty;
        }

        /// <summary>
        /// Hooks specified drive to receive a message when it is being removed.  
        /// This can be achieved also by setting e.HookQueryRemove to true in your 
        /// DeviceArrived event handler. 
        /// By default DriveDetector will open the root directory of the flash drive to obtain notification handle
        /// from Windows (to learn when the drive is about to be removed). 
        /// </summary>
        /// <param name="fileOnDrive">Drive letter or relative path to a file on the drive which should be 
        /// used to get a handle - required for registering to receive query remove messages.
        /// If only drive letter is specified (e.g. "D:\\", root directory of the drive will be opened.</param>
        /// <returns>true if hooked ok, false otherwise</returns>
        public bool EnableQueryRemove(string fileOnDrive)
        {
            if ((fileOnDrive == null) || (fileOnDrive.Length == 0)) {
                throw new ArgumentException(Properties.Resources.MSG_INVALID_PATH);
            }

            if ((fileOnDrive.Length == 2) && (fileOnDrive[1] == ':')) {
                fileOnDrive += '\\';
            }

            if (this.deviceNotifyHandle != IntPtr.Zero) {
                RegisterForDeviceChange(false, null); // Unregister first...
            }

            if ((Path.GetFileName(fileOnDrive).Length == 0) || !File.Exists(fileOnDrive)) {
                this.fileToOpen = null;     // use root directory...
            } else {
                this.fileToOpen = fileOnDrive;
            }

            RegisterQuery(Path.GetPathRoot(fileOnDrive));
            if (this.deviceNotifyHandle == IntPtr.Zero) {
                return false;   // failed to register
            }

            return true;
        }

        /// <summary>
        /// Unhooks any currently hooked drive so that the query remove 
        /// message is not generated for it.
        /// </summary>
        public void DisableQueryRemove()
        {
            if (this.deviceNotifyHandle != IntPtr.Zero) {
                RegisterForDeviceChange(false, null);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void OnDeviceArrived(DEV_BROADCAST_VOLUME volume)
        {
            // Get the drive letter 
            char c = DriveMaskToLetter(volume.dbcv_unitmask);
            var e = new RemovableMediaEventArgs {
                Drive = c + ":\\"
            };

            DeviceArrived?.Invoke(this, e);

            // Register for query remove if requested
            if (e.HookQueryRemove) {
                // If something is already hooked, unhook it now
                if (this.deviceNotifyHandle != IntPtr.Zero) {
                    RegisterForDeviceChange(false, null);
                }

                RegisterQuery(c + ":\\");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool OnQueryRemove()
        {
            var e = new RemovableMediaEventArgs() {
                Drive = this.currentDrive, // drive which is hooked
            };
            QueryRemove?.Invoke(this, e);

            return e.Cancel;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void OnDeviceRemoved(DEV_BROADCAST_VOLUME volume)
        {
            char c = DriveMaskToLetter(volume.dbcv_unitmask);
            var e = new RemovableMediaEventArgs() {
                Drive = c + ":\\",
            };
            DeviceRemoved?.BeginInvoke(this, e, null, null);
        }

        /// <summary>
        /// Message handler which must be called from client form.
        /// Processes Windows messages and calls event handlers. 
        /// </summary>
        /// <param name="m"></param>
        public void WndProc(ref Message m)
        {
            int devType;

            if (m.Msg == WM_DEVICECHANGE) {
                switch (m.WParam.ToInt32()) { // WM_DEVICECHANGE can have several meanings depending on the WParam value...
                    case DBT_DEVICEARRIVAL: // New device has just arrived
                        devType = Marshal.ReadInt32(m.LParam, 4);
                        if (devType == DBT_DEVTYP_VOLUME) {
                            var vol = (DEV_BROADCAST_VOLUME)Marshal.PtrToStructure(m.LParam, typeof(DEV_BROADCAST_VOLUME));
                            OnDeviceArrived(vol);
                        }
                        break;
                    case DBT_DEVICEQUERYREMOVE: // Device is about to be removed
                        devType = Marshal.ReadInt32(m.LParam, 4);
                        if (devType == DBT_DEVTYP_HANDLE) {
                            bool cancel = OnQueryRemove();
                            if (cancel) {
                                // If the client wants to cancel, let Windows know
                                m.Result = (IntPtr)BROADCAST_QUERY_DENY;
                            } else {
                                // Change 28.10.2007: Unregister the notification, this will
                                // close the handle to file or root directory also. 
                                RegisterForDeviceChange(false, null);
                            }
                        }
                        break;
                    case DBT_DEVICEREMOVECOMPLETE: // Device has been removed
                        devType = Marshal.ReadInt32(m.LParam, 4);
                        if (devType == DBT_DEVTYP_VOLUME) {
                            var vol = (DEV_BROADCAST_VOLUME)Marshal.PtrToStructure(m.LParam, typeof(DEV_BROADCAST_VOLUME));
                            OnDeviceRemoved(vol);
                        }
                        break;
                } // switch
            } // WM_DEVICECHANGE
        }

        /// <summary>
        /// Registers for receiving the query remove message for a given drive.
        /// We need to open a handle on that drive and register with this handle. 
        /// Client can specify this file in mFileToOpen or we will open root directory of the drive
        /// </summary>
        /// <param name="drive">drive for which to register. </param>
        private void RegisterQuery(string drive)
        {
            if (this.fileToOpen != null) {
                // Make sure the path in mFileToOpen contains valid drive
                // If there is a drive letter in the path, it may be different from the actual
                // letter assigned to the drive now. We will cut it off and merge the actual drive 
                // with the rest of the path.
                if (this.fileToOpen.Contains(":")) {
                    string tmp = this.fileToOpen.Substring(3);
                    string root = Path.GetPathRoot(drive);
                    this.fileToOpen = Path.Combine(root, tmp);
                } else {
                    this.fileToOpen = Path.Combine(drive, this.fileToOpen);
                }
            }

            if (this.fileToOpen == null) { // open root directory
                this.fileOnFlash = null;
                RegisterForDeviceChange(drive);
                this.currentDrive = drive;
            } else {
                try {
                    this.fileOnFlash = new FileStream(this.fileToOpen, FileMode.Open);
                    RegisterForDeviceChange(true, this.fileOnFlash.SafeFileHandle);
                    this.currentDrive = drive;
                } catch { }
            }
        }

        /// <summary>
        /// New version which gets the handle automatically for specified directory
        /// Only for registering! Unregister with the old version of this function...
        /// </summary>
        /// <param name="register"></param>
        /// <param name="dirPath">e.g. C:\\dir</param>
        private void RegisterForDeviceChange(string dirPath)
        {
            IntPtr handle = NativeMethods.OpenDirectory(dirPath);
            if (handle == IntPtr.Zero) {
                this.deviceNotifyHandle = IntPtr.Zero;
                return;
            } else {
                this.dirHandle = handle;    // save handle for closing it when unregistering
            }

            // Register for handle
            DEV_BROADCAST_HANDLE data = new DEV_BROADCAST_HANDLE {
                dbch_devicetype = DBT_DEVTYP_HANDLE,
                dbch_reserved = 0,
                dbch_nameoffset = 0,
                dbch_handle = handle,
                dbch_hdevnotify = (IntPtr)0,
            };
            int size = Marshal.SizeOf(data);
            data.dbch_size = size;
            IntPtr buffer = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(data, buffer, true);

            this.deviceNotifyHandle = NativeMethods.RegisterDeviceNotification(this.recipientHandle, buffer, 0);
        }

        /// <summary>
        /// Registers to be notified when the volume is about to be removed
        /// This is requierd if you want to get the QUERY REMOVE messages
        /// </summary>
        /// <param name="register">true to register, false to unregister</param>
        /// <param name="fileHandle">handle of a file opened on the removable drive</param>
        private void RegisterForDeviceChange(bool register, SafeFileHandle fileHandle)
        {
            if (register) {
                // Register for handle
                DEV_BROADCAST_HANDLE data = new DEV_BROADCAST_HANDLE {
                    dbch_devicetype = DBT_DEVTYP_HANDLE,
                    dbch_reserved = 0,
                    dbch_nameoffset = 0,
                    dbch_handle = fileHandle.DangerousGetHandle(),
                    dbch_hdevnotify = (IntPtr)0,
                };
                int size = Marshal.SizeOf(data);
                data.dbch_size = size;
                IntPtr buffer = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(data, buffer, true);

                this.deviceNotifyHandle = NativeMethods.RegisterDeviceNotification(this.recipientHandle, buffer, 0);
            } else {
                // close the directory handle
                if (this.dirHandle != IntPtr.Zero) {
                    NativeMethods.CloseDirectoryHandle(this.dirHandle);
                }

                // unregister
                if (this.deviceNotifyHandle != IntPtr.Zero) {
                    _ = NativeMethods.UnregisterDeviceNotification(this.deviceNotifyHandle);
                }

                this.deviceNotifyHandle = IntPtr.Zero;
                this.dirHandle = IntPtr.Zero;

                this.currentDrive = string.Empty;
                this.fileOnFlash?.Close();
                this.fileOnFlash = null;
            }
        }

        /// <summary>
        /// Gets drive letter from a bit mask where bit 0 = A, bit 1 = B etc.
        /// There can actually be more than one drive in the mask but we 
        /// just use the last one in this case.
        /// </summary>
        /// <param name="mask"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static char DriveMaskToLetter(int mask)
        {
            char letter;
            
            int cnt = 0;
            int pom = mask / 2;
            while (pom != 0) {
                // while there is any bit set in the mask
                // shift it to the right...                
                pom /= 2;
                cnt++;
            }

            if (cnt < DRIVES.Length) {
                letter = DRIVES[cnt];
            } else { 
                letter = '?';
            }

            return letter;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed) {
                try {
                    RegisterForDeviceChange(false, null);
                } catch { }

                if (disposing) {
                    this.fileOnFlash?.Close();
                    if (this.form != null) {
                        this.form.Invoke((Action)(() => {
                            this.form.Close();
                            this.form.Dispose();
                        }));
                    } 
                }

                this.disposed = true;
                this.fileOnFlash = null;
                this.form = null;
            }
        }

        ~RemovableDriveManager()
        {
            Dispose(false);
        }

        /// <summary>
        /// Unregister and close the file we may have opened on the removable drive. 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Events

        /// <summary>
        /// Events signalized to the client app.
        /// Add handlers for these events in your form to be notified of removable device events 
        /// </summary>
        public event EventHandler<RemovableMediaEventArgs> DeviceArrived;
        public event EventHandler<RemovableMediaEventArgs> DeviceRemoved;
        public event EventHandler<RemovableMediaEventArgs> QueryRemove;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the value indicating whether the query remove event will be fired.
        /// </summary>
        public bool IsQueryHooked
        {
            get { return (this.deviceNotifyHandle == IntPtr.Zero) ? false : true; }
        }

        /// <summary>
        /// Gets letter of drive which is currently hooked. Empty string if none.
        /// See also IsQueryHooked.
        /// </summary>
        public string HookedDrive
        {
            get { return this.currentDrive; }
        }

        /// <summary>
        /// Gets the file stream for file which this class opened on a drive to be notified
        /// about it's removal. 
        /// This will be null unless you specified a file to open (DriveDetector opens root directory of the flash drive) 
        /// </summary>
        public FileStream OpenedFile
        {
            get { return this.fileOnFlash; }
        }

        #endregion

   }

}

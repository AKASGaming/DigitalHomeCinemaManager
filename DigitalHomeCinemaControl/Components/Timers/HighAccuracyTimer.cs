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
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Windows.Threading;

    public sealed class HighAccuracyTimer : MarshalByRefObject, IDisposable
    {

        #region Members

        private static TimerCapabilities capabilities;
        private volatile bool disposed = false;
        private volatile TimerMode mode;
        private volatile int interval;
        private volatile int resolution;
        private int timerId;
        private UIntPtr zero = UIntPtr.Zero;

        #endregion

        #region Constructor

        static HighAccuracyTimer()
        {
            _ = NativeMethods.timeGetDevCaps(ref capabilities, Marshal.SizeOf(capabilities));
        }

        public HighAccuracyTimer()
        {
            this.Mode = TimerMode.Periodic;
            this.Interval = capabilities.periodMin;
            this.Resolution = 1;

            this.Enabled = false;
        }

        #endregion

        #region Methods

        public void Start()
        {
            if (this.disposed) { throw new ObjectDisposedException(GetType().Name); }
            if (this.Enabled) { return; }

            if (this.Mode == TimerMode.Periodic) {
                this.timerId = NativeMethods.timeSetEvent(this.Interval, this.Resolution, PeriodicTimerCallback, ref this.zero, (int)this.Mode);
            } else {
                this.timerId = NativeMethods.timeSetEvent(this.Interval, this.Resolution, OneShotTimerCallback, ref this.zero, (int)this.Mode);
            }

            if (this.timerId != 0) {
                this.Enabled = true;
            } else {
                throw new Win32Exception(this.timerId);
            }
        }

        public void Stop()
        {
            if (this.disposed) { throw new ObjectDisposedException(GetType().Name); }
            if (!this.Enabled) { return; }

            int result = NativeMethods.timeKillEvent(this.timerId);

            Debug.Assert(result == 0);

            this.Enabled = false;

        }

        private void PeriodicTimerCallback(int id, int msg, int user, int param1, int param2)
        {
            OnElapsed();
        }

        // Callback method called by the Win32 multimedia timer when a timer
        // one shot event occurs.
        private void OneShotTimerCallback(int id, int msg, int user, int param1, int param2)
        {
            OnElapsed();
            Stop();
        }

        private void OnElapsed()
        {
            if ((this.Dispatcher != null) && !this.Dispatcher.CheckAccess()) {
                this.Dispatcher.BeginInvoke((Action)(() => {
                    Elapsed?.Invoke(this, EventArgs.Empty);
                }));
            } else {
                Elapsed?.Invoke(this, EventArgs.Empty);
            }
        }

        ~HighAccuracyTimer()
        {
            if (this.Enabled) {
                _ = NativeMethods.timeKillEvent(this.timerId);
            }
        }

        public void Dispose()
        {
            if (!this.disposed) {
                if (this.Enabled) {
                    Stop();
                }
                this.disposed = true;
            }

            GC.SuppressFinalize(this);
        }

        #endregion

        #region Events

        public event EventHandler Elapsed;

        #endregion

        #region Properties

        public Dispatcher Dispatcher { get; set; }

        public int Interval
        {
            get {
                if (this.disposed) { throw new ObjectDisposedException(GetType().Name); }
                return this.interval;
            }
            set {
                if (this.disposed) { throw new ObjectDisposedException(GetType().Name); }
                if ((value < capabilities.periodMin) || (value > capabilities.periodMax)) {
                    throw new ArgumentOutOfRangeException(nameof(this.Interval), value, Properties.Resources.MSG_TIMER_INTERVAL_ERROR);
                }

                if (value != this.interval) {
                    this.interval = value;
                    if (this.Enabled) {
                        Stop();
                        Start();
                    }
                }
            }
        }

        public int Resolution
        {
            get {
                if (this.disposed) { throw new ObjectDisposedException(GetType().Name); }
                return this.resolution;
            }
            set {
                if (this.disposed) { throw new ObjectDisposedException(GetType().Name); }
                if (value < 0) {
                    throw new ArgumentOutOfRangeException(nameof(this.Resolution), value, Properties.Resources.MSG_TIMER_RESOLUTION_ERROR);
                }

                if (value != this.resolution) {
                    this.resolution = value;
                    if (this.Enabled) {
                        Stop();
                        Start();
                    }
                }
            }
        }

        public TimerMode Mode
        {
            get {
                if (this.disposed) { throw new ObjectDisposedException(GetType().Name); }
                return this.mode;
            }
            set {
                if (this.disposed) { throw new ObjectDisposedException(GetType().Name); }

                if (value != this.mode) {
                    this.mode = value;
                    if (this.Enabled) {
                        Stop();
                        Start();
                    }
                }
            }
        }

        public bool Enabled { get; private set; }

        #endregion

    }

}

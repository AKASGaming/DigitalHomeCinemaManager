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

    /// <summary>
    /// Implements a high accuracy timer using the Windows Multimedia API.
    /// </summary>
    /// <remarks>
    /// The HighAccuracy timer will typically raise the Elapsed event within
    /// +/- 1 millisecond of the specified interval.
    /// </remarks>
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
        private TimeProc timeProc;

        #endregion

        #region Constructor

        static HighAccuracyTimer()
        {
            _ = NativeMethods.timeGetDevCaps(ref capabilities, Marshal.SizeOf(capabilities));
        }

        /// <summary>
        /// Create a new instance of the HighAccuracyTimer class.
        /// </summary>
        public HighAccuracyTimer()
        {
            this.timeProc = new TimeProc(this.PeriodicTimerCallback);
            this.Mode = TimerMode.Periodic;
            this.Interval = capabilities.periodMin;
            this.Resolution = 1;

            this.Enabled = false;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Starts the timer.
        /// </summary>
        public void Start()
        {
            if (this.disposed) { throw new ObjectDisposedException(GetType().Name); }
            if (this.Enabled) { return; }

            this.timerId = NativeMethods.timeSetEvent(this.Interval, this.Resolution, this.timeProc, ref this.zero, (int)this.Mode);

            if (this.timerId != 0) {
                this.Enabled = true;
            } else {
                throw new Win32Exception(this.timerId);
            }
        }

        /// <summary>
        /// Stops the timer.
        /// </summary>
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

            if (this.Mode == TimerMode.OneShot) {
                Stop();
            }
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

        /// <summary>
        /// Releases all resources used by the current HighAccuracyTimer.
        /// </summary>
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

        /// <summary>
        /// Occurs when the interval elapses.
        /// </summary>
        public event EventHandler Elapsed;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or Sets the Dispatcher used to marshal event handler calls to the UI thread.
        /// </summary>
        public Dispatcher Dispatcher { get; set; }

        /// <summary>
        /// Gets or Sets the interval, in milliseconds, at which to raise the Elapsed event.
        /// </summary>
        /// <remarks>
        /// Changing this value while the timer is running will reset the timer with the new
        /// interval.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The specified interval is outside the capabilities of the multimedia timer.
        /// </exception>
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

        /// <summary>
        /// Gets or Sets the timer resolution in milliseconds.
        /// </summary>
        /// <remarks>
        /// Changing this value while the timer is running will reset the timer with the new
        /// interval.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">The Resolution is less than 0.</exception>
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

        /// <summary>
        /// Gets or Sets the timer mode.
        /// </summary>
        /// <remarks>
        /// Changing this value while the timer is running will reset the timer with the new
        /// interval.
        /// </remarks>
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

        /// <summary>
        /// Gets a value indicating whether the Timer is enabled.
        /// </summary>
        public bool Enabled { get; private set; }

        #endregion

    }

}

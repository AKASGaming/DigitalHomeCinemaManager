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
    using System.Threading;

    /// <summary>
    /// Implements a highly accurate wait timer.
    /// </summary>
    /// <remarks>
    /// This WaitTimer is the most accurate way to cause a thread to wait
    /// for a desired interval. The timeout will occur within +/- 1 
    /// millisecond of the specified value.
    /// 
    /// The worst performing method is calling Thread.Sleep() which almost
    /// always causes the thread to block for much longer than the time 
    /// specified. Slightly better is using either an AutoResetEvent or
    /// ManualResetEvent and providing a timeout when calling WaitOne(),
    /// but even this has tested to be very innacurate on some systems.
    /// </remarks>
    public sealed class WaitTimer : WaitHandle
    {

        #region Members

        private AutoResetEvent waitHandle;
        private HighAccuracyTimer timer;
        private volatile bool signaled;
        private volatile bool disposed = false;
        private bool autoReset;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new instance of the WaitTimer class.
        /// </summary>
        /// <param name="initialState">true to set the initial state to signaled; false to set the initial state to non-signaled.</param>
        /// <param name="autoReset">true for the wait handle to auto reset, false for manual reset.</param>
        public WaitTimer(bool initialState, bool autoReset)
            : base()
        {
            this.autoReset = autoReset;
            this.signaled = initialState;
            this.waitHandle = new AutoResetEvent(initialState);
            this.timer = new HighAccuracyTimer() {
                Resolution = 1,
                Mode = TimerMode.OneShot,
            };
            this.timer.Elapsed += TimerElapsed;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the state of the WaitTimer to non-signaled, which causes threads to block.
        /// </summary>
        /// <returns></returns>
        public bool Reset()
        {
            if (this.disposed) { throw new ObjectDisposedException(GetType().Name); }

            this.signaled = false;

            return this.waitHandle.Reset();
        }

        /// <summary>
        /// Sets the state of the WaitTimer to signaled, which allows at most one waiting thread
        /// to proceed.
        /// </summary>
        /// <returns></returns>
        public bool Set()
        {
            if (this.disposed) { throw new ObjectDisposedException(GetType().Name); }

            this.signaled = true;

            return this.waitHandle.Set();
        }

        public override bool WaitOne()
        {
            if (this.disposed) { throw new ObjectDisposedException(GetType().Name); }
            
            this.waitHandle.WaitOne();

            bool result = this.signaled;

            if (this.autoReset) {
                Reset();
            }

            return result; 
        }

        public override bool WaitOne(int millisecondsTimeout)
        {
            this.timer.Interval = millisecondsTimeout;
            this.timer.Start();
            return WaitOne();
        }

        public override bool WaitOne(int millisecondsTimeout, bool exitContext)
        {
            return WaitOne(millisecondsTimeout);
        }

        public override bool WaitOne(TimeSpan timeout)
        {
            return WaitOne((int)timeout.TotalMilliseconds);
        }

        public override bool WaitOne(TimeSpan timeout, bool exitContext)
        {
            return WaitOne((int)timeout.TotalMilliseconds);
        }

        private void TimerElapsed(object sender, EventArgs e)
        {
            if (this.disposed) { return; }

            this.waitHandle.Set();
        }

        protected override void Dispose(bool explicitDisposing)
        {
            if (!this.disposed) {
                if (explicitDisposing) {
                    this.waitHandle.Dispose();
                    this.timer.Dispose();
                }

                this.waitHandle = null;
                this.timer = null;
                this.disposed = true;
            }

            base.Dispose(explicitDisposing);
        }

        #endregion

    }

}

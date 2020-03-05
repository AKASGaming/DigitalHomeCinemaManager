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

    public sealed class AutoResetTimer : WaitHandle
    {

        #region Members

        private AutoResetEvent waitHandle;
        private HighAccuracyTimer timer;
        private volatile bool disposed = false;

        #endregion

        #region Constructor

        public AutoResetTimer(bool initialState)
            : base()
        {
            this.waitHandle = new AutoResetEvent(initialState);
            this.timer = new HighAccuracyTimer() {
                Resolution = 1,
                Mode = TimerMode.OneShot,
            };
            this.timer.Elapsed += TimerElapsed;
        }

        #endregion

        #region Methods

        public bool Reset()
        {
            if (this.disposed) { throw new ObjectDisposedException(GetType().Name); }

            return this.waitHandle.Reset();
        }

        public bool Set()
        {
            if (this.disposed) { throw new ObjectDisposedException(GetType().Name); }

            return this.waitHandle.Set();
        }

        public override bool WaitOne()
        {
            if (this.disposed) { throw new ObjectDisposedException(GetType().Name); }

            return this.waitHandle.WaitOne();
        }

        public override bool WaitOne(int millisecondsTimeout)
        {
            this.timer.Interval = millisecondsTimeout;
            this.timer.Start();
            return this.waitHandle.WaitOne();
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

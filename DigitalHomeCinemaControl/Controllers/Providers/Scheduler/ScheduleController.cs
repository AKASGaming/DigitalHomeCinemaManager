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

namespace DigitalHomeCinemaControl.Controllers.Providers.Scheduler
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using DigitalHomeCinemaControl.Components.Timers;
    using DigitalHomeCinemaControl.Controllers.Base;
    using DigitalHomeCinemaControl.Controllers.Routing;

    public sealed class ScheduleController : DeviceController, IScheduleController
    {

        #region Members

        private const string DELAY = "Delay Seconds";
        private const int INTERVAL = 1000;

        private HighAccuracyTimer timer;
        private IDictionary<string, Type> actions;
        private volatile ScheduleState state = ScheduleState.None;
        private volatile bool disposed = false;
        private bool enabled;

        #endregion

        #region Constructor

        public ScheduleController()
            : base()
        {
            this.actions = new Dictionary<string, Type> {
                { DELAY, typeof(int) },
            };
        }

        #endregion

        #region Methods

        public override void Connect()
        {
            this.disposed = false;

            this.timer = new HighAccuracyTimer() {
                Interval = INTERVAL,
                Resolution = 0,
                Mode = TimerMode.Periodic,
            };
            this.timer.Elapsed += TimerElapsed;

            OnConnected();
        }

        public override void Disconnect()
        {
            this.Enabled = false;

            try {
                Dispose(true);
            } catch {
            } finally {
                OnDisconnected();
            }
        }

        public bool SetSchedule(ScheduleItem start)
        {
            if (this.disposed) { return false; }
            if (start == null) { return false; }
            
            this.Schedule = start;
            this.Enabled = true;
            this.State = ScheduleState.Scheduled;
            this.timer.Start();

            return true;
        }

        public void ClearSchedule()
        {
            this.timer.Stop();
            this.Enabled = false;
            this.Schedule = null;
        }

        public string RouteAction(string action, object args)
        {
            if (string.IsNullOrEmpty(action)) {
                return string.Format(CultureInfo.InvariantCulture, Properties.Resources.FMT_SCH_ERROR,
                    Properties.Resources.MSG_INVALID_ACTION);
            }
            if (args == null) {
                return string.Format(CultureInfo.InvariantCulture, Properties.Resources.FMT_SCH_ERROR,
                    Properties.Resources.MSG_INVALID_ARGS);
            }

            switch (action) {
                case DELAY:
                    int seconds = (int)args;
                    using (var delay = new WaitTimer(false, false)) {
                        delay.WaitOne(seconds * 1000);
                    }
                    break;
                default:
                    return "SCHEDULE Unknown Action!";
            }

            return string.Format(CultureInfo.InvariantCulture, "SCHEDULE {0}", Properties.Resources.MSG_OK);
        }

        private void TimerElapsed(object sender, EventArgs e)
        {
            if (!this.Enabled) { return; }
            if (this.Schedule == null) { return; }

            var now = DateTime.Now;

            if ((this.Schedule.Interval == ScheduleInterval.Exact) && (DateTime.Compare(now, this.Schedule.Date) >= 0)) {
                this.timer.Stop();
                this.State = ScheduleState.Start;
                this.Enabled = false;
            } else if ((this.Schedule.Interval == ScheduleInterval.Once) && this.Schedule.Date.IsTimeGreaterThan(now)) {
                this.timer.Stop();
                this.State = ScheduleState.Start;
                this.Enabled = false;
            } else if ((this.Schedule.Interval == ScheduleInterval.Daily) && (DateTime.Compare(now, this.Schedule.Date) >= 0)) {
                this.Schedule.Date = this.Schedule.Date.AddDays(1);
                this.State = ScheduleState.Start;
                this.State = ScheduleState.Scheduled;
            }
            
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnStateChanged()
        {
            var item = new RoutingItem(this.Name, typeof(ScheduleState), this.State);
            RouteData?.Invoke(this, new RoutingDataEventArgs(item));
        }

        void Dispose(bool disposing)
        {
            if (!this.disposed) {
                if (disposing) {
                    this.timer?.Dispose();
                }

                this.timer = null;

                this.disposed = true;
            }
        }

        ~ScheduleController()
        {
            Dispose(false);
        }

#pragma warning disable CA1063 // Implement IDisposable Correctly
        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
#pragma warning restore CA1063

        #endregion

        #region Events

        public event EventHandler<RoutingDataEventArgs> RouteData;

        #endregion

        #region Properties

        public ScheduleState State
        {
            get { return this.state; }
            private set {
                if (value != this.state) {
                    this.state = value;
                    OnStateChanged();
                }
            }
        }
 
        public bool Enabled
        {
            get { return this.enabled; }
            set {
                if (value != this.enabled) {
                    this.enabled = value;
                    if (!this.enabled) {
                        this.State = ScheduleState.None;
                    }
                }
            }
        }

        public ScheduleItem Schedule { get; private set; }

        public string Name
        {
            get { return "Scheduler"; }
        }

        public Type MatchType
        {
            get { return typeof(ScheduleState); }
        }

        public IDictionary<string, Type> Actions
        {
            get { return this.actions; }
        }

        #endregion

    }

}

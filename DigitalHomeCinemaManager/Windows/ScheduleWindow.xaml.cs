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

namespace DigitalHomeCinemaManager.Windows
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using DigitalHomeCinemaControl.Controllers.Providers.Scheduler;


    /// <summary>
    /// Interaction logic for ScheduleWindow.xaml
    /// </summary>
    public partial class ScheduleWindow : Window
    {

        #region Members

        private ScheduleItem schedule;

        #endregion

        #region Constructor

        public ScheduleWindow(ScheduleItem schedule)
        {
            InitializeComponent();

            if (schedule == null) {
                this.schedule = new ScheduleItem() {
                    Interval = ScheduleInterval.Exact,
                    Date = DateTime.Now,
                };
            } else {
                this.schedule = schedule;
                this.Enabled.IsChecked = true;
            }

            this.Time.TimeInterval = new TimeSpan(0, 30, 0);
            this.Interval.ItemsSource = Enum.GetValues(typeof(ScheduleInterval));
            this.Interval.SelectedItem = this.schedule.Interval;
            this.Date.SelectedDate = this.schedule.Date;
            this.Time.Value = this.schedule.Date;
        }

        #endregion

        #region Methods

        private void IntervalSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var interval = (ScheduleInterval)this.Interval.SelectedItem;

            switch (interval) {
                case ScheduleInterval.Exact:
                    this.DateStack.Visibility = Visibility.Visible;
                    this.TimeStack.Visibility = Visibility.Visible;
                    break;
                case ScheduleInterval.Once:
                case ScheduleInterval.Daily:
                    this.DateStack.Visibility = Visibility.Collapsed;
                    this.TimeStack.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void OkClick(object sender, RoutedEventArgs e)
        {
            this.schedule = new ScheduleItem() {
                Interval = (ScheduleInterval)this.Interval.SelectedItem,
            };

            if (this.schedule.Interval == ScheduleInterval.Exact) {
                var date = (DateTime)this.Date.SelectedDate;
                var time = (DateTime)this.Time.Value;
                var dateTime = new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);
                this.schedule.Date = dateTime;
            } else {
                this.schedule.Date = (DateTime)this.Time.Value;
            }

            this.DialogResult = true;
            Close();
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }

        #endregion

        #region Properties

        public ScheduleItem Schedule { get { return this.schedule; } }

        #endregion

    }

}

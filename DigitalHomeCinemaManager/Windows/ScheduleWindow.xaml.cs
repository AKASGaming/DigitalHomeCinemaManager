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
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Shapes;
    using DigitalHomeCinemaControl.Controllers.Providers.Scheduler;


    /// <summary>
    /// Interaction logic for ScheduleWindow.xaml
    /// </summary>
    public partial class ScheduleWindow : Window
    {

        private ScheduleItem schedule;

        public ScheduleWindow(ScheduleItem schedule)
        {
            InitializeComponent();

            if (schedule == null) {
                this.schedule = new ScheduleItem() {
                    Interval = ScheduleInterval.Exact,
                    Date = new DateTime(),
                };
            } else {
                this.schedule = schedule;
            }

            this.Time.TimeInterval = new TimeSpan(0, 30, 0);
            this.Interval.ItemsSource = Enum.GetValues(typeof(ScheduleInterval));
            this.Interval.SelectedItem = this.schedule.Interval;
            this.Date.SelectedDate = this.schedule.Date;
            this.Time.Value = this.schedule.Date;
        }

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

        public ScheduleItem Schedule { get { return this.schedule; } }
    }

}

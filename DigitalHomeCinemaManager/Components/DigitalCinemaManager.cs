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

namespace DigitalHomeCinemaManager.Components
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Threading;
    using DigitalHomeCinemaControl;
    using DigitalHomeCinemaControl.Controllers;
    using DigitalHomeCinemaControl.Controllers.Routing;
    using DigitalHomeCinemaManager.Components.RemovableMedia;
    using DigitalHomeCinemaManager.Controls;
    using DigitalHomeCinemaManager.Windows;

    internal sealed class DigitalCinemaManager : IDisposable
    {

        #region Members

        private volatile bool disposed = false;
        private MainWindow mainWindow;
        private Dispatcher dispatcher;
        private DeviceManager deviceManager;
        private RoutingEngine router;
        private PlaylistManager playlist;
        private RemovableDriveManager diskManager;

        #endregion

        #region Constructor

        internal DigitalCinemaManager()
        { 
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads all components and sets up bindings for main window.
        /// Components will be started later in separate threads once
        /// the main window is rendered.
        /// </summary>
        public void Run()
        {
            // initialize main window and get the dispatcher
            this.mainWindow = new MainWindow();
            this.dispatcher = this.mainWindow.Dispatcher;

            // create components
            this.deviceManager = new DeviceManager(this.dispatcher);
            this.router = new RoutingEngine(this.dispatcher);
            this.playlist = new PlaylistManager(this.dispatcher);
            this.diskManager = new RemovableDriveManager();

            // main window needs the PlaylistManager
            this.mainWindow.Playlist = this.playlist;

            // subscribe to component events
            this.mainWindow.Closed += MainWindowClosed;
            this.mainWindow.ContentRendered += MainWindowContentRendered;
            this.mainWindow.OpenSettings += MainWindowOpenSettings;
            this.mainWindow.OpenScheduler += MainWindowOpenScheduler;
            this.router.RuleProcessed += RouterRuleProcessed;
            this.playlist.PlaylistChanged += PlaylistChanged;
            this.deviceManager.ControllerError += ControllerError;
            this.diskManager.DeviceArrived += DiskManagerDeviceArrived;
            this.diskManager.DeviceRemoved += DiskManagerDeviceRemoved;
            this.diskManager.QueryRemove += DiskManagerQueryRemove;

            // initialize devices
            this.deviceManager.ControllersInit();
            this.router.LoadRules();
            this.router.BindControllers(this.deviceManager.Controllers);
            this.router.Start();

            // add all UI elements to the main window
            this.mainWindow.BindDevices(this.deviceManager.Devices);

            // create a control for the routing engine rules
            var routingControl = new Controls.RoutingControl() {
                ItemsSource = this.router.Rules,
            };
            routingControl.ListDoubleClick += RoutingControlListDoubleClick;
            routingControl.ListAddClick += RoutingControlListAddClick;
            routingControl.ListRemoveClick += RoutingControlListRemoveClick;
            routingControl.ListMoveItemClick += RoutingControlListMoveItemClick;
            this.mainWindow.InsertControl(routingControl);

            // show the window
            this.mainWindow.Show();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SendStatusUpdate(string message)
        { 
            // if we're already on the UI thread just call UpdateStatus directly
            // otherwise marshall to UI thread with BeginInvoke
            if (this.dispatcher.CheckAccess()) {
                this.mainWindow?.UpdateStatus(message);
            } else {
                this.dispatcher.BeginInvoke((Action)(() => {
                    this.mainWindow?.UpdateStatus(message);
                }));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ControllerError(object sender, string e)
        {
            SendStatusUpdate(e);
        }

        #region Main Window

        /// <summary>
        /// Once main window is displayed, load the playlist then have the DeviceManager
        /// start up all the controllers on a separate thread to avoid risk of UI 
        /// becoming unresponsive.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindowContentRendered(object sender, System.EventArgs e)
        {
            this.playlist.LoadPlaylist();
            this.mainWindow.PlaylistInitialized = true;

            new Task(async () => {
                await Task.Delay(1000).ConfigureAwait(false);
                this.deviceManager.ControllersStart();
            }).Start();
            
            SendStatusUpdate(Properties.Resources.MSG_INIT_COMPLETE);
        }

        /// <summary>
        /// Application exit. Stop all routing and dispose of all controllers.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindowClosed(object sender, System.EventArgs e)
        {
            Closing?.BeginInvoke(this, new EventArgs(), EndInvokeClosing, null);
        }

        private void EndInvokeClosing(IAsyncResult iar)
        {
            var ar = (System.Runtime.Remoting.Messaging.AsyncResult)iar;
            var invokedMethod = (EventHandler)ar.AsyncDelegate;

            try {
                invokedMethod.EndInvoke(iar);
            } catch { }
        }

        /// <summary>
        /// Handler for DoubleClick event on RoutingControl
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RoutingControlListDoubleClick(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem is MatchAction rule) {
                var window = new EditRuleWindow(this.router, rule) {
                    Owner = this.mainWindow,
                };
                if (window.ShowDialog() == true) {
                    this.router.SaveRules();
                } 
            }
            if (sender is ListBox list) {
                list.Items.Refresh();
            }
        }

        /// <summary>
        /// Handler for RoutingControl Remove ContextMenu.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RoutingControlListRemoveClick(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem is MatchAction rule) {
                this.router.RemoveRule(rule);
                this.router.SaveRules();
            }
            if (sender is ListBox list) {
                list.Items.Refresh();
            }
        }

        /// <summary>
        /// Handler for RoutingControl Add ContextMenu.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RoutingControlListAddClick(object sender, SelectedItemChangedEventArgs e)
        {
            var window = new EditRuleWindow(this.router, null) {
                Owner = this.mainWindow,
            };
            if (window.ShowDialog() == true) {
                this.router.AddRule(window.Rule);
                this.router.SaveRules();
            }
            if (sender is ListBox list) {
                list.Items.Refresh();
            }
        }

        /// <summary>
        /// Handler for RoutingControl Move ContextMenu.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RoutingControlListMoveItemClick(object sender, MoveSelectedItemEventArgs e)
        {
            if (e.SelectedItem is MatchAction rule) {
                int i = this.router.MoveRule(rule, e.Direction);
                this.router.SaveRules();
                if ((i >= 0) && (sender is ListBox list)) {
                    list.SelectedIndex = i;
                    list.Items.Refresh();
                }
            }
        }

        /// <summary>
        /// Handler for Settings button click event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindowOpenSettings(object sender, EventArgs e)
        {
            var window = new SettingsWindow() {
                Owner = this.mainWindow
            };
            if (window.ShowDialog() == true) {
                Properties.Settings.Default.Save();
                Properties.DeviceSettings.Default.Save();

                var result = MessageBox.Show(Properties.Resources.SETTINGS_CHANGED, 
                    Properties.Resources.APP_RESTART, MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes) { 
                    Process.Start(Application.ResourceAssembly.Location);
                    Application.Current.Shutdown();
                }
            }
        }

        /// <summary>
        /// Handler for Scheduler button click event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindowOpenScheduler(object sender, EventArgs e)
        {
            var window = new ScheduleWindow(this.deviceManager.Scheduler.Schedule) {
                Owner = this.mainWindow,
                SchedulerEnable = this.deviceManager.Scheduler.Enabled,
            };

            if (window.ShowDialog() == true) {
                // TODO: Figure out how to display this in UI
                if (window.SchedulerEnable) {
                    if (this.deviceManager.Scheduler.SetSchedule(window.Schedule)) {
                        SendStatusUpdate(Properties.Resources.MSG_SCHEDULE_SET);
                    }
                } else {
                    this.deviceManager.Scheduler.ClearSchedule();
                    SendStatusUpdate(Properties.Resources.MSG_SCHEDULE_CLEAR);
                }
            }
        }

        #endregion

        #region Playlist

        private void PlaylistChanged(object sender, string e)
        {
            Debug.Assert(!string.IsNullOrEmpty(e));

            var mediaInfo = this.deviceManager.MediaInfoDevice.GetController<IMediaInfoController>();
            var source = this.deviceManager.SourceDevice.GetController<ISourceController>();

            source.OpenPlaylist(e, this.playlist.Feature);
            mediaInfo.GetFeatureInfo(this.playlist.FeatureTitle, this.playlist.FeatureYear);
        }

        #endregion

        #region Routing

        private void RouterRuleProcessed(object sender, string e)
        {
            SendStatusUpdate(e);
        }

        #endregion

        #region Removable Disk Management

        private void DiskManagerDeviceArrived(object sender, RemovableMediaEventArgs e)
        {
            if (Properties.Settings.Default.MediaPath.Contains(e.Drive)) {
                e.HookQueryRemove = true;
                SendStatusUpdate(Properties.Resources.MSG_DRIVE_INSERTED);
                this.playlist.Feature = string.Empty;
                this.playlist.CreatePlaylist();
            }
        }

        private void DiskManagerQueryRemove(object sender, RemovableMediaEventArgs e)
        {
            var source = this.deviceManager.SourceDevice.GetController<ISourceController>();

            // we can try and cancel, but if user just pulled the disk playback will be interrupted
            if (source.State >= PlaybackState.Playing) {
                e.Cancel = true;
            } else {
                e.Cancel = false;
            }
        }

        private void DiskManagerDeviceRemoved(object sender, RemovableMediaEventArgs e)
        {
            if (Properties.Settings.Default.MediaPath.Contains(e.Drive)) {
                SendStatusUpdate(Properties.Resources.MSG_DRIVE_EJECTED);
                this.playlist.Feature = string.Empty;
                this.playlist.CreatePlaylist();
            }
        }

        #endregion

        #region IDisposable

        private void Dispose(bool disposing)
        {
            if (!this.disposed) {
                if (disposing) {
                    this.router?.Stop();
                    this.deviceManager.Dispose();
                    this.router.Dispose();
                    this.mainWindow.Dispose();
                }

                this.disposed = true;

                try {
                    this.diskManager.Dispose();
                } catch {
                } finally { 
                    this.deviceManager = null;
                    this.router = null;
                    this.diskManager = null;
                    this.mainWindow = null;
                }
            }
        }

        ~DigitalCinemaManager()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #endregion

        #region Events

        public event EventHandler Closing;

        #endregion

    }

}

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
    using System.Threading.Tasks;
    using System.Windows.Controls;
    using System.Windows.Threading;
    using DigitalHomeCinemaControl;
    using DigitalHomeCinemaControl.Controllers;
    using DigitalHomeCinemaControl.Controllers.Routing;
    using DigitalHomeCinemaManager.Components.Include;
    using DigitalHomeCinemaManager.Windows;

    internal class DigitalCinemaManager : IDisposable
    {

        #region Members

        private bool disposed = false;
        private MainWindow mainWindow;
        private Dispatcher dispatcher;
        private DeviceManager deviceManager;
        private RoutingEngine router;
        private PlaylistManager playlist;
        private DriveDetector driveDetector;

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
            this.playlist = new PlaylistManager();
            this.driveDetector = new DriveDetector();

            // main window needs the PlaylistManager
            this.mainWindow.Playlist = this.playlist;

            // subscribe to component events
            this.mainWindow.Closed += MainWindowClosed;
            this.mainWindow.ContentRendered += MainWindowContentRendered;
            this.mainWindow.OpenSettings += MainWindowOpenSettings;
            this.router.RuleProcessed += RouterRuleProcessed;
            this.playlist.PlaylistChanged += PlaylistChanged;
            this.deviceManager.ControllerError += ControllerError;
            this.driveDetector.DeviceArrived += DriveDetectorDeviceArrived;
            this.driveDetector.DeviceRemoved += DriveDetectorDeviceRemoved;
            this.driveDetector.QueryRemove += DriveDetectorQueryRemove;

            // initialize devices
            this.deviceManager.ControllersInit();
            this.router.LoadRules();
            this.router.BindControllers(this.deviceManager.Controllers);

            // add all UI elements to the main window
            this.mainWindow.BindDevices(this.deviceManager.Devices);

            // create a control for the routing engine rules
            var routingControl = new Controls.RoutingControl() {
                ItemsSource = this.router.Rules,
            };
            routingControl.ListDoubleClick += RoutingControlListDoubleClick;
            routingControl.ListAddClick += RoutingControlListAddClick;
            routingControl.ListRemoveClick += RoutingControlListRemoveClick;
            this.mainWindow.InsertControl(routingControl);

            // show the window
            this.mainWindow.Show();
        }

        private void SendStatusUpdate(string message)
        {
            // if we're already on the UI thread just call UpdateStatus directly
            // otherwise marshall to UI thread with BeginInvoke
            if (this.dispatcher.CheckAccess()) {
                this.mainWindow.UpdateStatus(message);
            } else {
                this.dispatcher.BeginInvoke((Action)(() => {
                    this.mainWindow.UpdateStatus(message);
                }));
            }
        }

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
                await Task.Delay(1000);
                this.deviceManager.ControllersStart();
            }).Start();

            SendStatusUpdate("Initialization complete.");
        }

        /// <summary>
        /// Application exit. Stop all routing and dispose of all controllers.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindowClosed(object sender, System.EventArgs e)
        {
            this.router.Stop();
            this.router.Dispose();
            this.deviceManager.Dispose();

            Environment.Exit(0);
        }

        /// <summary>
        /// Handler for DoubleClick event on RoutingControl
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RoutingControlListDoubleClick(object sender, object e)
        {
            if (e is MatchAction rule) {
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
        private void RoutingControlListRemoveClick(object sender, object e)
        {
            if (e is MatchAction rule) {
                this.router.Rules.Remove(rule);
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
        private void RoutingControlListAddClick(object sender, object e)
        {
            var window = new EditRuleWindow(this.router, null) {
                Owner = this.mainWindow,
            };
            if (window.ShowDialog() == true) {
                this.router.Rules.Add(window.Rule);
                this.router.SaveRules();
            }
            if (sender is ListBox list) {
                list.Items.Refresh();
            }
        }

        /// <summary>
        /// Handler for Settings button click event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindowOpenSettings(object sender, EventArgs e)
        {
            SettingsWindow window = new SettingsWindow() {
                Owner = this.mainWindow
            };
            if (window.ShowDialog() == true) {
                Properties.Settings.Default.Save();
                Properties.DeviceSettings.Default.Save();
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

        #region DriveDetector

        private void DriveDetectorDeviceArrived(object sender, DriveDetectorEventArgs e)
        {
            if (Properties.Settings.Default.MediaPath.Contains(e.Drive)) {
                e.HookQueryRemove = true;
                SendStatusUpdate("Media Drive Inserted.");
                this.playlist.Feature = string.Empty;
                this.playlist.CreatePlaylist();
            }
        }

        private void DriveDetectorQueryRemove(object sender, DriveDetectorEventArgs e)
        {
            var source = this.deviceManager.SourceDevice.GetController<ISourceController>();

            // we can try and cancel, but if user just pulled disk playback will be interrupted
            if ((source.State == PlaybackState.Playing) || (source.State == PlaybackState.PlayingFeature)) {
                e.Cancel = true;
            } else {
                e.Cancel = false;
            }
        }

        private void DriveDetectorDeviceRemoved(object sender, DriveDetectorEventArgs e)
        {

            if (Properties.Settings.Default.MediaPath.Contains(e.Drive)) {
                SendStatusUpdate("Media Drive Ejected!");
                this.playlist.Feature = string.Empty;
                this.playlist.CreatePlaylist();
            }
        }

        #endregion

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed) {
                if (disposing) {
                    this.deviceManager.Dispose();
                    this.router.Dispose();
                }

                this.driveDetector.Dispose();

                this.deviceManager = null;
                this.router = null;
                this.driveDetector = null;

                this.disposed = true;
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

    }

}

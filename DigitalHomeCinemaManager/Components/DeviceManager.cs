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
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using System.Windows.Threading;
    using DigitalHomeCinemaControl.Controllers;
    using DigitalHomeCinemaControl.Devices;

    public class DeviceManager : IDisposable
    {

        #region Members

        private Dispatcher dispatcher;
        private bool disposed = false;

        #endregion

        #region Constructor

        internal DeviceManager(Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
            this.Devices = new List<IDevice>();
            this.Controllers = new List<IController>();
        }

        #endregion

        #region Methods

        public void ControllersInit()
        {
            this.Devices.Clear();
            this.Controllers.Clear();

            if (!string.IsNullOrEmpty(Properties.Settings.Default.SerialDevice)) {
                this.SerialDevice = SerialDevice.Items[Properties.Settings.Default.SerialDevice];
                this.SerialDevice.Controller.Dispatcher = this.dispatcher;
                this.SerialDevice.Controller.Error += OnControllerError;
                LoadDeviceSettings(this.SerialDevice);
                this.Controllers.Add(this.SerialDevice.Controller);
                this.Devices.Add(this.SerialDevice);
            }

            if (!string.IsNullOrEmpty(Properties.Settings.Default.SourceDevice)) {
                this.SourceDevice = SourceDevice.Items[Properties.Settings.Default.SourceDevice];
                this.SourceDevice.Controller.Dispatcher = this.dispatcher;
                this.SourceDevice.Controller.Error += OnControllerError;
                LoadDeviceSettings(this.SourceDevice);
                if (this.SourceDevice.UIElement != null) {
                    this.SourceDevice.UIElement.DataSource = this.SourceDevice.Controller.DataSource;
                }
                this.Controllers.Add(this.SourceDevice.Controller);
                this.Devices.Add(this.SourceDevice);
            }

            if (!string.IsNullOrEmpty(Properties.Settings.Default.DisplayDevice)) {
                this.DisplayDevice = DisplayDevice.Items[Properties.Settings.Default.DisplayDevice];
                this.DisplayDevice.Controller.Dispatcher = this.dispatcher;
                this.DisplayDevice.Controller.Error += OnControllerError;
                LoadDeviceSettings(this.DisplayDevice);
                if (this.DisplayDevice.UIElement != null) {
                    this.DisplayDevice.UIElement.DataSource = this.DisplayDevice.Controller.DataSource;
                }
                this.Controllers.Add(this.DisplayDevice.Controller);
                this.Devices.Add(this.DisplayDevice);
            }

            if (!string.IsNullOrEmpty(Properties.Settings.Default.MediaInfoDevice)) {
                this.MediaInfoDevice = MediaInfoDevice.Items[Properties.Settings.Default.MediaInfoDevice];
                this.MediaInfoDevice.Controller.Dispatcher = this.dispatcher;
                this.MediaInfoDevice.Controller.Error += OnControllerError;
                LoadDeviceSettings(this.MediaInfoDevice);
                if (this.MediaInfoDevice.UIElement != null) {
                    this.MediaInfoDevice.UIElement.DataSource = this.MediaInfoDevice.Controller.DataSource;
                }
                this.Controllers.Add(this.MediaInfoDevice.Controller);
                this.Devices.Add(this.MediaInfoDevice);
                // we have to call connect here to ensure that the controller is initialized
                // for the initial playlist load
                this.MediaInfoDevice.Controller.Connect();
            }

            if (!string.IsNullOrEmpty(Properties.Settings.Default.ProcessorDevice)) {
                this.ProcessorDevice = ProcessorDevice.Items[Properties.Settings.Default.ProcessorDevice];
                this.ProcessorDevice.Controller.Dispatcher = this.dispatcher;
                this.ProcessorDevice.Controller.Error += OnControllerError;
                LoadDeviceSettings(this.ProcessorDevice);
                if (this.ProcessorDevice.UIElement != null) {
                    this.ProcessorDevice.UIElement.DataSource = this.ProcessorDevice.Controller.DataSource;
                }
                this.Controllers.Add(this.ProcessorDevice.Controller);
                this.Devices.Add(this.ProcessorDevice);
            }

            // TODO: Initialize other controllers!

        }

        public void ControllersStart()
        {
            foreach (var controller in this.Controllers) {
                new Task(async () => {
                    await Task.Delay(100);
                    controller.Connect();
                }).Start();
            }
        }

        public void LoadDeviceSettings(IDevice device)
        {
            Debug.Assert(device != null);

            string deviceType = GetDeviceType(device);

            Debug.Assert(!string.IsNullOrEmpty(deviceType));

            foreach (SettingsProperty setting in Properties.DeviceSettings.Default.Properties) {
                if (setting.Name.StartsWith(deviceType)) {
                    string[] settingName = setting.Name.Split('_');
                    if (settingName.Length == 2) {
                        if (setting.PropertyType.IsEnum) {
                            var value = Enum.Parse(setting.PropertyType, setting.DefaultValue.ToString());
                            device.Controller.Setting(value, settingName[1]);
                        } else {
                            var value = Convert.ChangeType(setting.DefaultValue, setting.PropertyType);
                            device.Controller.Setting(value, settingName[1]);
                        }
                    }
                }
            } // foreach 
        }

        public void SaveDeviceSettings(IDevice device)
        {
            Debug.Assert(device != null);

            string deviceType = GetDeviceType(device);

            Debug.Assert(!string.IsNullOrEmpty(deviceType));

            foreach (var setting in device.Controller.Settings) {
                string settingName = deviceType + "_" + setting.Key;

                var settingsProperty = Properties.DeviceSettings.Default.Properties[settingName];
                if (settingsProperty == null) { continue; }

                settingsProperty.DefaultValue = setting.Value;
            } // foreach

            Properties.DeviceSettings.Default.Save();
        }

        private string GetDeviceType(IDevice device)
        {
            Debug.Assert(device != null);

            if (device is SourceDevice) {
                return "Source";
            } else if (device is SerialDevice) {
                return "Serial";
            } else if (device is DisplayDevice) {
                return "Display";
            } else if (device is MediaInfoDevice) {
                return "MediaInfo";
            } else if (device is ProcessorDevice) {
                return "Processor";
            } else {
                return string.Empty;
            }
        }

        private void OnControllerError(object sender, string message)
        {
            this.ControllerError?.Invoke(sender, message);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed) {
                if (disposing) {
                    foreach (var controller in this.Controllers) {
                        controller.Disconnect();
                    }
                }

                this.disposed = true;
            }
        }

         ~DeviceManager()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Events

        public event EventHandler<string> ControllerError;

        #endregion

        #region Properties

        public SerialDevice SerialDevice { get; private set; }

        public SourceDevice SourceDevice { get; private set; }

        public DisplayDevice DisplayDevice { get; private set; }

        public MediaInfoDevice MediaInfoDevice { get; private set; }

        public ProcessorDevice ProcessorDevice { get; private set; }

        public List<IDevice> Devices { get; private set; }

        public List<IController> Controllers { get; private set; }

        #endregion

    }

}

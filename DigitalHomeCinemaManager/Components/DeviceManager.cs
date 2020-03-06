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
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows.Threading;
    using DigitalHomeCinemaControl;
    using DigitalHomeCinemaControl.Controllers;
    using DigitalHomeCinemaControl.Controls;
    using DigitalHomeCinemaControl.Devices;

    internal sealed class DeviceManager : IDisposable
    {

        #region Members

        private Dispatcher dispatcher;
        private volatile bool disposed = false;
        private volatile bool disposing = false;

        #endregion

        #region Constructor

        internal DeviceManager(Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
            this.Devices = new List<IDevice>(10);
            this.Controllers = new List<IController>(10);
        }

        #endregion

        #region Methods

        public void ControllersInit()
        {
            this.Devices.Clear();
            this.Controllers.Clear();

            // Special case for Scheduler since it's not an actual device. Just create an instance and add
            // it to the collection so that the RoutingEngine can bind to it.
            this.Scheduler = new DigitalHomeCinemaControl.Controllers.Providers.Scheduler.ScheduleController();
            this.Controllers.Add(this.Scheduler);

            if (!string.IsNullOrEmpty(Properties.Settings.Default.SerialDevice)) {
                InitializeDevice<SerialDevice>(SerialDevice.Items[Properties.Settings.Default.SerialDevice]);
            }

            if (!string.IsNullOrEmpty(Properties.Settings.Default.SourceDevice)) {
                this.SourceDevice = InitializeDevice<SourceDevice>(SourceDevice.Items[Properties.Settings.Default.SourceDevice]);
            }

            if (!string.IsNullOrEmpty(Properties.Settings.Default.DisplayDevice)) {
                InitializeDevice<DisplayDevice>(DisplayDevice.Items[Properties.Settings.Default.DisplayDevice]);
            }

            if (!string.IsNullOrEmpty(Properties.Settings.Default.MediaInfoDevice)) {
                this.MediaInfoDevice = InitializeDevice<MediaInfoDevice>(MediaInfoDevice.Items[Properties.Settings.Default.MediaInfoDevice]);
                // we have to call connect here to ensure that the controller is initialized
                // for the initial playlist load
                this.MediaInfoDevice.Controller.Connect();
            }

            if (!string.IsNullOrEmpty(Properties.Settings.Default.ProcessorDevice)) {
                InitializeDevice<ProcessorDevice>(ProcessorDevice.Items[Properties.Settings.Default.ProcessorDevice]);
            }

            if (!string.IsNullOrEmpty(Properties.Settings.Default.InputSwitchDevice)) {
                InitializeDevice<SwitchDevice>(SwitchDevice.Items[Properties.Settings.Default.InputSwitchDevice]);
            }

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ControllersStart()
        {
            foreach (var controller in this.Controllers) {
                new Task(async () => {
                    await Task.Delay(100).ConfigureAwait(false);
                    controller.Connect();
                }).Start();
            }
        }

        public static void LoadDeviceSettings(IDevice device)
        {
            Debug.Assert(device != null);

#pragma warning disable CA1062 // Validate arguments of public methods
            string deviceType = GetDeviceType(device);
#pragma warning restore CA1062 // Validate arguments of public methods

            Debug.Assert(!string.IsNullOrEmpty(deviceType));

            foreach (SettingsProperty setting in Properties.DeviceSettings.Default.Properties) {
                if (!setting.Name.StartsWith(deviceType, StringComparison.Ordinal)) { continue; }

                string[] settingName = setting.Name.Split('_');
                if (settingName.Length != 2) { continue; }

                object valueObject = Properties.DeviceSettings.Default[setting.Name];
                if (setting.PropertyType.IsEnum) {

#pragma warning disable IDE0008 // Use explicit type
                    var value = Enum.Parse(setting.PropertyType, valueObject.ToString());
#pragma warning restore IDE0008

                    device.Controller.Setting(value, settingName[1]);
                } else if (setting.PropertyType == typeof(StringCollection)) {
                    var value = (StringCollection)valueObject;
                    device.Controller.Setting(value.ToNameValueCollection(), settingName[1]);
                } else {

#pragma warning disable IDE0008 // Use explicit type
                    var value = Convert.ChangeType(valueObject, setting.PropertyType, CultureInfo.InvariantCulture);
#pragma warning restore IDE0008 
                    
                    device.Controller.Setting(value, settingName[1]);
                }
            } // foreach 
        }

        public static IEnumerable<string> GetProviders(DeviceType deviceType)
        {
            List<string> result = null;

            switch (deviceType) {
                case DeviceType.Source:
                    result = new List<string>(SourceDevice.Items.Keys);
                    break;
                case DeviceType.Serial:
                    result = new List<string>(SerialDevice.Items.Keys);
                    break;
                case DeviceType.Display:
                    result = new List<string>(DisplayDevice.Items.Keys);
                    break;
                case DeviceType.MediaInfo:
                    result = new List<string>(MediaInfoDevice.Items.Keys);
                    break;
                case DeviceType.Processor:
                    result = new List<string>(ProcessorDevice.Items.Keys);
                    break;
                case DeviceType.InputSwitch:
                    result = new List<string>(SwitchDevice.Items.Keys);
                    break;
            }

            return result;
        }

        private T InitializeDevice<T>(T device)
            where T : IDevice
        {
            Debug.Assert(device != null);
  
            device.Controller.Dispatcher = this.dispatcher;
            device.Controller.Error += OnControllerError;
            LoadDeviceSettings(device);
            if (device.UIElement != null) {
                device.UIElement.DataSource = device.Controller.DataSource;
                if (device.UIElement is IRequireController ircDevice) {
                    ircDevice.Controller = device.Controller;
                }
            }

            this.Controllers.Add(device.Controller);
            this.Devices.Add(device);

            return device;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string GetDeviceType(IDevice device)
        {
            Debug.Assert(device != null);

            return device.DeviceType.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnControllerError(object sender, ControllerErrorEventArgs e)
        {
            if (this.disposed || this.disposing) { return; }

            ControllerError?.Invoke(sender, e.Message);
        }

        private void Dispose(bool disposing)
        {
            this.disposing = true;

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

        public IScheduleController Scheduler { get; private set; }

        public SourceDevice SourceDevice { get; private set; }

        public MediaInfoDevice MediaInfoDevice { get; private set; }

        public List<IDevice> Devices { get; private set; }

        public List<IController> Controllers { get; private set; }

        #endregion

    }

}

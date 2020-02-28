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
    using DigitalHomeCinemaControl;
    using DigitalHomeCinemaControl.Collections;
    using DigitalHomeCinemaControl.Controllers;
    using DigitalHomeCinemaControl.Controls;
    using DigitalHomeCinemaControl.Devices;
    using System.Linq;
    using System.Collections.Specialized;
    using System.Xml.Serialization;
    using System.IO;

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
                        } else if (setting.PropertyType == typeof(StringCollection)) {
                            XmlSerializer serializer = new XmlSerializer(typeof(StringCollection));
                            using (TextReader reader = new StringReader(setting.DefaultValue.ToString())) {
                                var value = (StringCollection)serializer.Deserialize(reader);
                                NameValueCollection collection = value.ToNameValueCollection();
                                device.Controller.Setting(collection, settingName[1]);
                            }
                        } else {
                            var value = Convert.ChangeType(setting.DefaultValue, setting.PropertyType);
                            device.Controller.Setting(value, settingName[1]);
                        }
                    }
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

        private string GetDeviceType(IDevice device)
        {
            Debug.Assert(device != null);

            return device.DeviceType.ToString();
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

        public SourceDevice SourceDevice { get; private set; }

        public MediaInfoDevice MediaInfoDevice { get; private set; }

        public List<IDevice> Devices { get; private set; }

        public List<IController> Controllers { get; private set; }

        #endregion

    }

}

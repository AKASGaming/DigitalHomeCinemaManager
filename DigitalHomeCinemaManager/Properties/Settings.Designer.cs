﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DigitalHomeCinemaManager.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.3.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Generic")]
        public string SerialDevice {
            get {
                return ((string)(this["SerialDevice"]));
            }
            set {
                this["SerialDevice"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Sony Projector")]
        public string DisplayDevice {
            get {
                return ((string)(this["DisplayDevice"]));
            }
            set {
                this["DisplayDevice"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("TMDB")]
        public string MediaInfoDevice {
            get {
                return ((string)(this["MediaInfoDevice"]));
            }
            set {
                this["MediaInfoDevice"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("C:\\Users\\Theater\\Videos\\Preroll")]
        public string PrerollPath {
            get {
                return ((string)(this["PrerollPath"]));
            }
            set {
                this["PrerollPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("C:\\Users\\Theater\\Videos\\Movies")]
        public string MediaPath {
            get {
                return ((string)(this["MediaPath"]));
            }
            set {
                this["MediaPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("C:\\Users\\Theater\\Videos\\Trailers")]
        public string TrailerPath {
            get {
                return ((string)(this["TrailerPath"]));
            }
            set {
                this["TrailerPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Denon AVR")]
        public string ProcessorDevice {
            get {
                return ((string)(this["ProcessorDevice"]));
            }
            set {
                this["ProcessorDevice"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("HDFury Diva")]
        public string InputSwitchDevice {
            get {
                return ((string)(this["InputSwitchDevice"]));
            }
            set {
                this["InputSwitchDevice"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::NAudio.CoreAudioApi.MMDeviceEnumerator VUDevice {
            get {
                return ((global::NAudio.CoreAudioApi.MMDeviceEnumerator)(this["VUDevice"]));
            }
            set {
                this["VUDevice"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool VUDeviceEnabled {
            get {
                return ((bool)(this["VUDeviceEnabled"]));
            }
            set {
                this["VUDeviceEnabled"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool RandTrailers {
            get {
                return ((bool)(this["RandTrailers"]));
            }
            set {
                this["RandTrailers"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string TrailerLimit {
            get {
                return ((string)(this["TrailerLimit"]));
            }
            set {
                this["TrailerLimit"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool TrailerLimitEnabled {
            get {
                return ((bool)(this["TrailerLimitEnabled"]));
            }
            set {
                this["TrailerLimitEnabled"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool EnableLogs {
            get {
                return ((bool)(this["EnableLogs"]));
            }
            set {
                this["EnableLogs"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("MPC Home Cinema")]
        public string SourceDevice {
            get {
                return ((string)(this["SourceDevice"]));
            }
            set {
                this["SourceDevice"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool SourceDeviceEnabled {
            get {
                return ((bool)(this["SourceDeviceEnabled"]));
            }
            set {
                this["SourceDeviceEnabled"] = value;
            }
        }
    }
}

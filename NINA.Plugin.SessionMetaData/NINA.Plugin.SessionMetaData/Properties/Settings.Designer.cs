﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SessionMetaData.NINAPlugin.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.2.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool UpdateSettings {
            get {
                return ((bool)(this["UpdateSettings"]));
            }
            set {
                this["UpdateSettings"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool SessionMetaDataEnabled {
            get {
                return ((bool)(this["SessionMetaDataEnabled"]));
            }
            set {
                this["SessionMetaDataEnabled"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool CSVEnabled {
            get {
                return ((bool)(this["CSVEnabled"]));
            }
            set {
                this["CSVEnabled"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool JSONEnabled {
            get {
                return ((bool)(this["JSONEnabled"]));
            }
            set {
                this["JSONEnabled"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("AcquisitionDetails")]
        public string AcquisitionDetailsFileName {
            get {
                return ((string)(this["AcquisitionDetailsFileName"]));
            }
            set {
                this["AcquisitionDetailsFileName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("ImageMetaData")]
        public string ImageMetaDataFileName {
            get {
                return ((string)(this["ImageMetaDataFileName"]));
            }
            set {
                this["ImageMetaDataFileName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("AutoFocusRuns")]
        public string AutoFocusRunsFileName {
            get {
                return ((string)(this["AutoFocusRunsFileName"]));
            }
            set {
                this["AutoFocusRunsFileName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool WeatherEnabled {
            get {
                return ((bool)(this["WeatherEnabled"]));
            }
            set {
                this["WeatherEnabled"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("WeatherData")]
        public string WeatherMetaDataFileName {
            get {
                return ((string)(this["WeatherMetaDataFileName"]));
            }
            set {
                this["WeatherMetaDataFileName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string MetaDataOutputDirectory {
            get {
                return ((string)(this["MetaDataOutputDirectory"]));
            }
            set {
                this["MetaDataOutputDirectory"] = value;
            }
        }
    }
}

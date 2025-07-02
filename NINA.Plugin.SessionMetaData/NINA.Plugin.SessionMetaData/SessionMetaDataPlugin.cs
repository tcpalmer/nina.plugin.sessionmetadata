using Microsoft.WindowsAPICodePack.Dialogs;
using NINA.Core.Utility;
using NINA.Plugin;
using NINA.Plugin.Interfaces;
using NINA.Profile;
using NINA.Profile.Interfaces;
using NINA.WPF.Base.Interfaces.Mediator;
using NINA.WPF.Base.Interfaces.ViewModel;
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using RelayCommand = CommunityToolkit.Mvvm.Input.RelayCommand;

namespace SessionMetaData.NINAPlugin {

    [Export(typeof(IPluginManifest))]
    public class SessionMetaDataPlugin : PluginBase, INotifyPropertyChanged {
        public ICommand MetadataOutputDirectoryDialogCommand { get; private set; }
        private IPluginOptionsAccessor pluginSettings;

        [ImportingConstructor]
        public SessionMetaDataPlugin(IProfileService profileService, IImageSaveMediator imageSaveMediator, IImageHistoryVM imageHistory) {
            MetadataOutputDirectoryDialogCommand = new RelayCommand(OpenMetadataOutputDirectoryDialog);

            if (Properties.Settings.Default.UpdateSettings) {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpdateSettings = false;
                CoreUtil.SaveSettings(Properties.Settings.Default);
            }

            pluginSettings = new PluginOptionsAccessor(profileService, Guid.Parse(this.Identifier));
            MigrateSettings();
            profileService.ProfileChanged += ProfileService_ProfileChanged;

            new SessionMetaDataWatcher(imageSaveMediator, this);

            // Putting this on hold, hopefully plugins will get more access via:
            // https://bitbucket.org/Isbeorn/nina/issues/1005/request-to-provide-plugin-access-to
            // new SessionAutoFocusWatcher(profileService, this, imageHistory);
        }

        public bool SessionMetaDataEnabled {
            get => pluginSettings.GetValueBoolean(nameof(SessionMetaDataEnabled), Properties.Settings.Default.SessionMetaDataEnabled);
            set {
                pluginSettings.SetValueBoolean(nameof(SessionMetaDataEnabled), value);
                RaisePropertyChanged();
            }
        }

        public bool CSVEnabled {
            get => pluginSettings.GetValueBoolean(nameof(CSVEnabled), Properties.Settings.Default.CSVEnabled);
            set {
                pluginSettings.SetValueBoolean(nameof(CSVEnabled), value);
                RaisePropertyChanged();
            }
        }

        public bool JSONEnabled {
            get => pluginSettings.GetValueBoolean(nameof(JSONEnabled), Properties.Settings.Default.JSONEnabled);
            set {
                pluginSettings.SetValueBoolean(nameof(JSONEnabled), value);
                RaisePropertyChanged();
            }
        }

        public bool NonLightsEnabled {
            get => pluginSettings.GetValueBoolean(nameof(NonLightsEnabled), Properties.Settings.Default.NonLightsEnabled);
            set {
                pluginSettings.SetValueBoolean(nameof(NonLightsEnabled), value);
                RaisePropertyChanged();
            }
        }

        public bool WeatherEnabled {
            get => pluginSettings.GetValueBoolean(nameof(WeatherEnabled), Properties.Settings.Default.WeatherEnabled);
            set {
                pluginSettings.SetValueBoolean(nameof(WeatherEnabled), value);
                RaisePropertyChanged();
            }
        }

        public string AcquisitionDetailsFileName {
            get => pluginSettings.GetValueString(nameof(AcquisitionDetailsFileName), Properties.Settings.Default.AcquisitionDetailsFileName);
            set {
                pluginSettings.SetValueString(nameof(AcquisitionDetailsFileName), value);
                RaisePropertyChanged();
            }
        }

        public string ImageMetaDataFileName {
            get => pluginSettings.GetValueString(nameof(ImageMetaDataFileName), Properties.Settings.Default.ImageMetaDataFileName);
            set {
                pluginSettings.SetValueString(nameof(ImageMetaDataFileName), value);
                RaisePropertyChanged();
            }
        }

        public string WeatherMetaDataFileName {
            get => pluginSettings.GetValueString(nameof(WeatherMetaDataFileName), Properties.Settings.Default.WeatherMetaDataFileName);
            set {
                pluginSettings.SetValueString(nameof(WeatherMetaDataFileName), value);
                RaisePropertyChanged();
            }
        }

        public string AutoFocusRunsFileName {
            get => pluginSettings.GetValueString(nameof(AutoFocusRunsFileName), Properties.Settings.Default.AutoFocusRunsFileName);
            set {
                pluginSettings.SetValueString(nameof(AutoFocusRunsFileName), value);
                RaisePropertyChanged();
            }
        }

        public string MetaDataOutputDirectory {
            get => pluginSettings.GetValueString(nameof(MetaDataOutputDirectory), Properties.Settings.Default.MetaDataOutputDirectory);
            set {
                pluginSettings.SetValueString(nameof(MetaDataOutputDirectory), value);
                RaisePropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ProfileService_ProfileChanged(object sender, EventArgs e) {
            MigrateSettings();
            RaisePropertyChanged(nameof(SessionMetaDataEnabled));
            RaisePropertyChanged(nameof(CSVEnabled));
            RaisePropertyChanged(nameof(JSONEnabled));
            RaisePropertyChanged(nameof(NonLightsEnabled));
            RaisePropertyChanged(nameof(WeatherEnabled));
            RaisePropertyChanged(nameof(AcquisitionDetailsFileName));
            RaisePropertyChanged(nameof(ImageMetaDataFileName));
            RaisePropertyChanged(nameof(WeatherMetaDataFileName));
            RaisePropertyChanged(nameof(MetaDataOutputDirectory));
        }

        private void MigrateSettings() {
            bool hasMigratedProperties = pluginSettings.GetValueBoolean("HasMigratedProperties", false);
            if (hasMigratedProperties) { return; }

            Logger.Debug("performing onetime migration of Session Metadata plugin configuration for this profile");
            pluginSettings.SetValueBoolean(nameof(SessionMetaDataEnabled), Properties.Settings.Default.SessionMetaDataEnabled);
            pluginSettings.SetValueBoolean(nameof(CSVEnabled), Properties.Settings.Default.CSVEnabled);
            pluginSettings.SetValueBoolean(nameof(JSONEnabled), Properties.Settings.Default.JSONEnabled);
            pluginSettings.SetValueBoolean(nameof(NonLightsEnabled), Properties.Settings.Default.NonLightsEnabled);
            pluginSettings.SetValueBoolean(nameof(WeatherEnabled), Properties.Settings.Default.WeatherEnabled);
            pluginSettings.SetValueString(nameof(AcquisitionDetailsFileName), Properties.Settings.Default.AcquisitionDetailsFileName);
            pluginSettings.SetValueString(nameof(ImageMetaDataFileName), Properties.Settings.Default.ImageMetaDataFileName);
            pluginSettings.SetValueString(nameof(WeatherMetaDataFileName), Properties.Settings.Default.WeatherMetaDataFileName);
            pluginSettings.SetValueString(nameof(MetaDataOutputDirectory), Properties.Settings.Default.MetaDataOutputDirectory);

            pluginSettings.SetValueBoolean("HasMigratedProperties", true);
        }

        private void OpenMetadataOutputDirectoryDialog() {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.Title = "Metadata Output Directory";
            dialog.IsFolderPicker = true;
            dialog.InitialDirectory = MetaDataOutputDirectory;

            CommonFileDialogResult result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok) {
                MetaDataOutputDirectory = dialog.FileName;
            }
        }
    }
}
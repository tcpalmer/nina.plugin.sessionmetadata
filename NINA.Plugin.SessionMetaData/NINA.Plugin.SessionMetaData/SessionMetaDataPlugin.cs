using Microsoft.WindowsAPICodePack.Dialogs;
using NINA.Core.Utility;
using NINA.Plugin;
using NINA.Plugin.Interfaces;
using NINA.Profile.Interfaces;
using NINA.WPF.Base.Interfaces.Mediator;
using NINA.WPF.Base.Interfaces.ViewModel;
using SessionMetaData.NINAPlugin.Properties;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace SessionMetaData.NINAPlugin {

    [Export(typeof(IPluginManifest))]
    public class SessionMetaDataPlugin : PluginBase, INotifyPropertyChanged {
        public ICommand MetadataOutputDirectoryDialogCommand { get; private set; }

        [ImportingConstructor]
        public SessionMetaDataPlugin(IProfileService profileService, IImageSaveMediator imageSaveMediator, IImageHistoryVM imageHistory) {
            MetadataOutputDirectoryDialogCommand = new RelayCommand(OpenMetadataOutputDirectoryDialog);

            if (Settings.Default.UpdateSettings) {
                Settings.Default.Upgrade();
                Settings.Default.UpdateSettings = false;
                CoreUtil.SaveSettings(Settings.Default);
            }

            new SessionMetaDataWatcher(imageSaveMediator);

            // Putting this on hold, hopefully plugins will get more access via:
            // https://bitbucket.org/Isbeorn/nina/issues/1005/request-to-provide-plugin-access-to
            // new SessionAutoFocusWatcher(profileService, imageHistory);
        }

        public bool SessionMetaDataEnabled {
            get => Settings.Default.SessionMetaDataEnabled;
            set {
                Settings.Default.SessionMetaDataEnabled = value;
                Settings.Default.Save();
                RaisePropertyChanged();
            }
        }

        public bool CSVEnabled {
            get => Settings.Default.CSVEnabled;
            set {
                Settings.Default.CSVEnabled = value;
                Settings.Default.Save();
                RaisePropertyChanged();
            }
        }

        public bool JSONEnabled {
            get => Settings.Default.JSONEnabled;
            set {
                Settings.Default.JSONEnabled = value;
                Settings.Default.Save();
                RaisePropertyChanged();
            }
        }

        public bool NonLightsEnabled {
            get => Settings.Default.NonLightsEnabled;
            set {
                Settings.Default.NonLightsEnabled = value;
                Settings.Default.Save();
                RaisePropertyChanged();
            }
        }

        public bool WeatherEnabled {
            get => Settings.Default.WeatherEnabled;
            set {
                Settings.Default.WeatherEnabled = value;
                Settings.Default.Save();
                RaisePropertyChanged();
            }
        }

        public string AcquisitionDetailsFileName {
            get => Settings.Default.AcquisitionDetailsFileName;
            set {
                Settings.Default.AcquisitionDetailsFileName = value;
                Settings.Default.Save();
                RaisePropertyChanged();
            }
        }

        public string ImageMetaDataFileName {
            get => Settings.Default.ImageMetaDataFileName;
            set {
                Settings.Default.ImageMetaDataFileName = value;
                Settings.Default.Save();
                RaisePropertyChanged();
            }
        }

        public string WeatherMetaDataFileName {
            get => Settings.Default.WeatherMetaDataFileName;
            set {
                Settings.Default.WeatherMetaDataFileName = value;
                Settings.Default.Save();
                RaisePropertyChanged();
            }
        }

        public string MetaDataOutputDirectory {
            get => Settings.Default.MetaDataOutputDirectory;
            set {
                Settings.Default.MetaDataOutputDirectory = value;
                Settings.Default.Save();
                RaisePropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OpenMetadataOutputDirectoryDialog(object obj) {
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
using NINA.Core.Utility;
using NINA.Plugin;
using NINA.Plugin.Interfaces;
using NINA.WPF.Base.Interfaces.Mediator;
using SessionMetaData.NINAPlugin.Properties;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Runtime.CompilerServices;

namespace SessionMetaData.NINAPlugin {

    [Export(typeof(IPluginManifest))]
    public class SessionMetaDataPlugin : PluginBase, INotifyPropertyChanged {

        [ImportingConstructor]
        public SessionMetaDataPlugin(IImageSaveMediator imageSaveMediator) {
            if (Settings.Default.UpdateSettings) {
                Settings.Default.Upgrade();
                Settings.Default.UpdateSettings = false;
                CoreUtil.SaveSettings(Settings.Default);
            }

            new SessionMetaDataWatcher(imageSaveMediator);
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

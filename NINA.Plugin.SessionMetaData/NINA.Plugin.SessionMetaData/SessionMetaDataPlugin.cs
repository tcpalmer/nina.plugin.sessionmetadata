using NINA.Core.Utility;
using NINA.Plugin;
using NINA.Plugin.Interfaces;
using NINA.WPF.Base.Interfaces.Mediator;
using SessionMetaData.NINAPlugin.Properties;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Runtime.CompilerServices;

namespace SessionMetaData.NINAPlugin {
    /// <summary>
    /// This class exports the IPluginManifest interface and will be used for the general plugin information and options
    /// The base class "PluginBase" will populate all the necessary Manifest Meta Data out of the AssemblyInfo attributes. Please fill these accoringly
    /// 
    /// An instance of this class will be created and set as datacontext on the plugin options tab in N.I.N.A. to be able to configure global plugin settings
    /// The user interface for the settings will be defined by a DataTemplate with the key having the naming convention "<MyPlugin.Name>_Options" where MyPlugin.Name corresponds to the AssemblyTitle - In this template example it is found in the Options.xaml
    /// </summary>
    [Export(typeof(IPluginManifest))]
    public class SessionMetaDataPlugin : PluginBase, INotifyPropertyChanged {

        SessionMetaDataWatcher watcher;

        [ImportingConstructor]
        public SessionMetaDataPlugin(IImageSaveMediator imageSaveMediator) {
            if (Settings.Default.UpdateSettings) {
                Settings.Default.Upgrade();
                Settings.Default.UpdateSettings = false;
                CoreUtil.SaveSettings(Settings.Default);
            }

            this.watcher = new SessionMetaDataWatcher(imageSaveMediator);
        }

        public bool SessionMetaDataEnabled {
            get => Settings.Default.SessionMetaDataEnabled;
            set {
                Settings.Default.SessionMetaDataEnabled = value;
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

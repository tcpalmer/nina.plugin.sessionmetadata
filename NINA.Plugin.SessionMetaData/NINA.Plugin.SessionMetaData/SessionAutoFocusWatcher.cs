using CsvHelper;
using Newtonsoft.Json;
using NINA.Core.Model;
using NINA.Core.Utility;
using NINA.Profile.Interfaces;
using NINA.WPF.Base.Interfaces.ViewModel;
using NINA.WPF.Base.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;

namespace SessionMetaData.NINAPlugin {

    public class SessionAutoFocusWatcher {

        private bool SessionMetaDataEnabled;
        private bool CSVEnabled;
        private bool JSONEnabled;
        private string AutoFocusRunsFileName;
        private string AutoFocusRunsFile;

        // Putting this on hold, hopefully plugins will get more access via:
        // https://bitbucket.org/Isbeorn/nina/issues/1005/request-to-provide-plugin-access-to

        public SessionAutoFocusWatcher(IProfileService profileService, IImageHistoryVM imageHistory) {
            SessionMetaDataEnabled = Properties.Settings.Default.SessionMetaDataEnabled;
            AutoFocusRunsFileName = Properties.Settings.Default.AutoFocusRunsFileName; // IGNORED FOR NOW
            AutoFocusRunsFile = Path.Combine(profileService.ActiveProfile.ImageFileSettings.FilePath, $"autofocus-{DateTime.Now.ToString("yyyyMMdd-HHmmss")}");

            Properties.Settings.Default.PropertyChanged += SettingsChanged;
            imageHistory.AutoFocusPoints.CollectionChanged += AutoFocusPoints_CollectionChanged;
        }

        private void AutoFocusPoints_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args) {

            if (!SessionMetaDataEnabled) {
                Logger.Debug("SessionMetaData not enabled");
                return;
            }

            try {
                IList newItems = args.NewItems;
                foreach (ImageHistoryPoint item in newItems) {
                    if (item.AutoFocusPoint != null) {
                        WriteAutoFocusRunRecord(item.AutoFocusPoint);
                    }
                }
            }
            catch (Exception e) {
                Logger.Warning($"autofocus data save failed: {e.Message}");
            }
        }

        private void WriteAutoFocusRunRecord(AutoFocusPoint autoFocusPoint) {

            AutoFocusRunRecord record = new AutoFocusRunRecord(autoFocusPoint);
            //AutoFocusRunsFile

            if (CSVEnabled) {
                string fileName = $"{AutoFocusRunsFile}.csv";

                bool exists = File.Exists(fileName);

                using (var writer = File.AppendText(fileName))
                using (var csv = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture)) {
                    if (!exists) {
                        csv.WriteHeader<AutoFocusRunRecord>();
                        csv.NextRecord();
                    }
                    csv.WriteRecord(record);
                    csv.NextRecord();
                }
            }

            if (JSONEnabled) {
                string fileName = $"{AutoFocusRunsFile}.json";

                JsonSerializer serializer = new JsonSerializer();
                serializer.NullValueHandling = NullValueHandling.Include;
                serializer.Formatting = Formatting.Indented;

                List<AutoFocusRunRecord> Records;

                bool exists = File.Exists(fileName);

                if (exists) {
                    string json = File.ReadAllText(fileName);
                    Records = JsonConvert.DeserializeObject<List<AutoFocusRunRecord>>(json);
                    Records.Add(record);

                    using (StreamWriter sw = new StreamWriter(fileName))
                    using (JsonWriter writer = new JsonTextWriter(sw)) {
                        serializer.Serialize(writer, Records);
                    }
                }
                else {
                    Records = new List<AutoFocusRunRecord>();
                    Records.Add(record);

                    using (StreamWriter sw = new StreamWriter(fileName))
                    using (JsonWriter writer = new JsonTextWriter(sw)) {
                        serializer.Serialize(writer, Records);
                    }
                }
            }
        }

        public class AutoFocusRunRecord {
            DateTime Time { get; set; }
            string Filter { get; set; }
            double Temperature { get; set; }
            double OldPos { get; set; }
            double NewPos { get; set; }
            string Description { get; set; }

            public AutoFocusRunRecord() {
            }

            public AutoFocusRunRecord(AutoFocusPoint autoFocusPoint) {
                Time = autoFocusPoint.Time;
                Filter = autoFocusPoint.Filter;
                Temperature = autoFocusPoint.Temperature;
                OldPos = autoFocusPoint.OldPosition;
                NewPos = autoFocusPoint.NewPosition;
                Description = autoFocusPoint.GetDescription();
            }

        }

        void SettingsChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case "SessionMetaDataEnabled":
                    SessionMetaDataEnabled = Properties.Settings.Default.SessionMetaDataEnabled;
                    break;
                case "AutoFocusRunsFileName":
                    AutoFocusRunsFileName = Properties.Settings.Default.AutoFocusRunsFileName;
                    break;
                case "CSVEnabled":
                    CSVEnabled = Properties.Settings.Default.CSVEnabled;
                    break;
                case "JSONEnabled":
                    JSONEnabled = Properties.Settings.Default.JSONEnabled;
                    break;
            }
        }
    }
}

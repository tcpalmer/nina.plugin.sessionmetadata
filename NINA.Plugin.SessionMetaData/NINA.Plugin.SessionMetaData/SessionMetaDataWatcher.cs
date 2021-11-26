using CsvHelper;
using Newtonsoft.Json;
using NINA.Core.Enum;
using NINA.Core.Utility;
using NINA.Image.ImageData;
using NINA.WPF.Base.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace SessionMetaData.NINAPlugin {

    public class SessionMetaDataWatcher {

        private static string ACQUISITION_FILE_NAME = "AcquisitionDetails";
        private static string IMAGE_METADATA_FILE_NAME = "ImageMetaData";

        private bool SessionMetaDataEnabled;
        private bool CSVEnabled;
        private bool JSONEnabled;

        public SessionMetaDataWatcher(IImageSaveMediator imageSaveMediator) {
            SessionMetaDataEnabled = Properties.Settings.Default.SessionMetaDataEnabled;
            CSVEnabled = Properties.Settings.Default.CSVEnabled;
            JSONEnabled = Properties.Settings.Default.JSONEnabled;

            Properties.Settings.Default.PropertyChanged += SettingsChanged;
            imageSaveMediator.ImageSaved += ImageSaveMeditator_ImageSaved;
        }

        private void ImageSaveMeditator_ImageSaved(object sender, ImageSavedEventArgs msg) {

            if (!SessionMetaDataEnabled) {
                Logger.Debug("SessionMetaData not enabled");
                return;
            }

            if (msg.MetaData.Image.ImageType != "LIGHT") {
                Logger.Debug("image is not a light, skipping");
                return;
            }

            try {
                WriteAcquisitionMetaData(msg);
                WriteImageMetaData(msg);
            }
            catch (Exception e) {
                Logger.Warning($"session metadata save failed: {e.Message}");
            }
        }

        private void WriteAcquisitionMetaData(ImageSavedEventArgs msg) {
            string ImageFilePath = msg.PathToImage.ToString();
            Logger.Trace($"ImageFilePath: {ImageFilePath}");
            string ImageDirectory = GetImageDirectory(ImageFilePath);
            Logger.Trace($"ImageDirectory: {ImageDirectory}");

            AcquisitionMetaDataRecord Record = new AcquisitionMetaDataRecord(msg);

            if (CSVEnabled) {
                string AcquisitionFileName = Path.Combine(ImageDirectory, $"{ACQUISITION_FILE_NAME}.csv");

                // Only write this once per image output directory
                if (!File.Exists(AcquisitionFileName)) {
                    Logger.Info($"Writing CSV acquisition summary: {AcquisitionFileName}");

                    using (var writer = File.AppendText(AcquisitionFileName))
                    using (var csv = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture)) {
                        csv.WriteHeader<AcquisitionMetaDataRecord>();
                        csv.NextRecord();
                        csv.WriteRecord(Record);
                        csv.NextRecord();
                    }
                }
            }

            if (JSONEnabled) {
                string AcquisitionFileName = Path.Combine(ImageDirectory, $"{ACQUISITION_FILE_NAME}.json");

                // Only write this once per image output directory
                if (!File.Exists(AcquisitionFileName)) {
                    Logger.Info($"Writing JSON acquisition summary: {AcquisitionFileName}");

                    JsonSerializer serializer = new JsonSerializer();
                    serializer.NullValueHandling = NullValueHandling.Include;
                    serializer.Formatting = Formatting.Indented;

                    using (StreamWriter sw = new StreamWriter(AcquisitionFileName))
                    using (JsonWriter writer = new JsonTextWriter(sw)) {
                        serializer.Serialize(writer, Record);
                    }
                }
            }
        }

        private void WriteImageMetaData(ImageSavedEventArgs msg) {

            string ImageFilePath = msg.PathToImage.ToString();
            Logger.Trace($"ImageFilePath: {ImageFilePath}");
            string ImageDirectory = GetImageDirectory(ImageFilePath);
            Logger.Trace($"ImageDir: {ImageDirectory}");

            ImageMetaDataRecord Record = new ImageMetaDataRecord(msg, ImageFilePath);

            if (CSVEnabled) {
                string ImageMetaDataFileName = Path.Combine(ImageDirectory, $"{IMAGE_METADATA_FILE_NAME}.csv");
                Logger.Info($"Writing CSV image metadata: {ImageMetaDataFileName}");

                bool exists = File.Exists(ImageMetaDataFileName);

                using (var writer = File.AppendText(ImageMetaDataFileName))
                using (var csv = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture)) {
                    if (!exists) {
                        csv.WriteHeader<ImageMetaDataRecord>();
                        csv.NextRecord();
                    }
                    csv.WriteRecord(Record);
                    csv.NextRecord();
                }
            }

            if (JSONEnabled) {
                string ImageMetaDataFileName = Path.Combine(ImageDirectory, $"{IMAGE_METADATA_FILE_NAME}.json");
                Logger.Info($"Writing JSON image metadata: {ImageMetaDataFileName}");

                JsonSerializer serializer = new JsonSerializer();
                serializer.NullValueHandling = NullValueHandling.Include;
                serializer.Formatting = Formatting.Indented;

                List<ImageMetaDataRecord> Records;

                bool exists = File.Exists(ImageMetaDataFileName);

                if (exists) {
                    string json = File.ReadAllText(ImageMetaDataFileName);
                    Records = JsonConvert.DeserializeObject<List<ImageMetaDataRecord>>(json);
                    Records.Add(Record);

                    using (StreamWriter sw = new StreamWriter(ImageMetaDataFileName))
                    using (JsonWriter writer = new JsonTextWriter(sw)) {
                        serializer.Serialize(writer, Records);
                    }
                }
                else {
                    Records = new List<ImageMetaDataRecord>();
                    Records.Add(Record);

                    using (StreamWriter sw = new StreamWriter(ImageMetaDataFileName))
                    using (JsonWriter writer = new JsonTextWriter(sw)) {
                        serializer.Serialize(writer, Records);
                    }
                }
            }
        }

        void SettingsChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case "SessionMetaDataEnabled":
                    SessionMetaDataEnabled = Properties.Settings.Default.SessionMetaDataEnabled;
                    break;
                case "CSVEnabled":
                    CSVEnabled = Properties.Settings.Default.CSVEnabled;
                    break;
                case "JSONEnabled":
                    JSONEnabled = Properties.Settings.Default.JSONEnabled;
                    break;
            }
        }

        public class ImageMetaDataRecord {
            public int ExposureNumber { get; set; }
            public string FilePath { get; set; }
            public string FilterName { get; set; }
            public DateTime ExposureStart { get; set; }
            public double Duration { get; set; }
            public string Binning { get; set; }
            public double CameraTemperature { get; set; }
            public int Gain { get; set; }
            public int Offset { get; set; }
            public double ADUStDev { get; set; }
            public double ADUMean { get; set; }
            public double ADUMedian { get; set; }
            public int ADUMin { get; set; }
            public int ADUMax { get; set; }
            public int DetectedStars { get; set; }
            public double HFR { get; set; }
            public double HFRStDev { get; set; }
            public string GuidingRMS { get; set; }
            public string GuidingRMSArcSec { get; set; }
            public int? FocuserPosition { get; set; }
            public string PierSide { get; set; }

            public ImageMetaDataRecord() {
            }

            public ImageMetaDataRecord(ImageSavedEventArgs msg, string ImageFilePath) {
                ExposureNumber = msg.MetaData.Image.Id;
                FilePath = ImageFilePath;
                FilterName = msg.Filter;
                ExposureStart = msg.MetaData.Image.ExposureStart;
                Duration = msg.Duration;
                Binning = msg.MetaData.Image.Binning?.ToString();
                CameraTemperature = msg.MetaData.Camera.Temperature;
                Gain = msg.MetaData.Camera.Gain;
                Offset = msg.MetaData.Camera.Offset;
                ADUStDev = msg.Statistics.StDev;
                ADUMean = msg.Statistics.Mean;
                ADUMedian = msg.Statistics.Median;
                ADUMin = msg.Statistics.Min;
                ADUMax = msg.Statistics.Max;
                DetectedStars = msg.StarDetectionAnalysis.DetectedStars;
                HFR = msg.StarDetectionAnalysis.HFR;
                HFRStDev = msg.StarDetectionAnalysis.HFRStDev;
                GuidingRMS = GetGuidingRMS(msg.MetaData.Image);
                GuidingRMSArcSec = GetGuidingRMSArcSec(msg.MetaData.Image);
                FocuserPosition = msg.MetaData.Focuser.Position;
                PierSide = GetPierSide(msg.MetaData.Telescope.SideOfPier);
            }

            private string GetGuidingRMS(ImageParameter image) {
                return image.RecordedRMS != null ? image.RecordedRMS.Total.ToString() : "n/a";
            }

            private string GetGuidingRMSArcSec(ImageParameter image) {
                return image.RecordedRMS != null ? (image.RecordedRMS.Total * image.RecordedRMS.Scale).ToString() : "n/a";
            }

            private string GetPierSide(PierSide sideOfPier) {
                switch (sideOfPier) {
                    case NINA.Core.Enum.PierSide.pierEast: return "East";
                    case NINA.Core.Enum.PierSide.pierWest: return "West";
                    default: return "n/a";
                }
            }
        }

        private class AcquisitionMetaDataRecord {
            public string TargetName { get; }
            public string RACoordinates { get; }
            public string DECCoordinates { get; }
            public string TelescopeName { get; }
            public double FocalLength { get; }
            public double FocalRatio { get; }
            public string CameraName { get; }
            public double PixelSize { get; }
            public int BitDepth { get; }
            public double ObserverLatitude { get; }
            public double ObserverLongitude { get; }
            public double ObserverElevation { get; }

            public AcquisitionMetaDataRecord(ImageSavedEventArgs msg) {
                TargetName = msg.MetaData.Target.Name;
                RACoordinates = msg.MetaData.Target.Coordinates?.RAString;
                DECCoordinates = msg.MetaData.Target.Coordinates?.DecString;
                TelescopeName = msg.MetaData.Telescope.Name;
                FocalLength = msg.MetaData.Telescope.FocalLength;
                FocalRatio = msg.MetaData.Telescope.FocalRatio;
                CameraName = msg.MetaData.Camera.Name;
                PixelSize = msg.MetaData.Camera.PixelSize;
                BitDepth = msg.Statistics.BitDepth;
                ObserverLatitude = msg.MetaData.Observer.Latitude;
                ObserverLongitude = msg.MetaData.Observer.Longitude;
                ObserverElevation = msg.MetaData.Observer.Elevation;
            }
        }

        private string GetImageDirectory(string ImageFilePath) {
            return Path.GetDirectoryName(ImageFilePath.Substring(8)); // skip 'file:///'
        }
    }
}

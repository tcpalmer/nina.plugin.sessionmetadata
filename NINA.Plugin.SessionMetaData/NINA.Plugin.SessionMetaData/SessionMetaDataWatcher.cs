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
using System.Text.RegularExpressions;
using System.Web;

namespace SessionMetaData.NINAPlugin {

    public class SessionMetaDataWatcher {

        private bool SessionMetaDataEnabled;
        private bool CSVEnabled;
        private bool JSONEnabled;
        private bool WeatherEnabled;
        private string AcquisitionDetailsFileName;
        private string ImageMetaDataFileName;
        private string WeatherMetaDataFileName;
        private string AutoFocusRunsFileName;

        public SessionMetaDataWatcher(IImageSaveMediator imageSaveMediator) {
            SessionMetaDataEnabled = Properties.Settings.Default.SessionMetaDataEnabled;
            CSVEnabled = Properties.Settings.Default.CSVEnabled;
            JSONEnabled = Properties.Settings.Default.JSONEnabled;
            WeatherEnabled = Properties.Settings.Default.WeatherEnabled;
            AcquisitionDetailsFileName = Properties.Settings.Default.AcquisitionDetailsFileName;
            ImageMetaDataFileName = Properties.Settings.Default.ImageMetaDataFileName;
            WeatherMetaDataFileName = Properties.Settings.Default.WeatherMetaDataFileName;

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
                WriteWeatherMetaData(msg);
            }
            catch (Exception e) {
                Logger.Warning($"session metadata save failed: {e.Message}");
            }
        }

        private void WriteAcquisitionMetaData(ImageSavedEventArgs msg) {
            string ImageDirectory = GetImageDirectory(msg.PathToImage);
            Logger.Debug($"ImageDirectory: {ImageDirectory}");

            /* TODO: to support a relative path prepended to the file names:
             *   - Pull off any path before the last \ and run that through token substitution
             *   - Put the remaining string through token substitution
             *   - Add any path prefix to the image dir
             *   - Compose the full path name
             */

            AcquisitionMetaDataRecord Record = new AcquisitionMetaDataRecord(msg);
            string acquisitionFileNameSub = Utility.Utility.FileNameTokenSubstitution(AcquisitionDetailsFileName, msg);
            Logger.Debug($"AcquisitionDetails file name: {AcquisitionDetailsFileName} -> {acquisitionFileNameSub}");

            if (CSVEnabled) {
                string AcquisitionFileName = Path.Combine(ImageDirectory, $"{acquisitionFileNameSub}.csv");

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
                string AcquisitionFileName = Path.Combine(ImageDirectory, $"{acquisitionFileNameSub}.json");

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
            string ImageDirectory = GetImageDirectory(msg.PathToImage);
            Logger.Debug($"ImageDirectory: {ImageDirectory}");

            ImageMetaDataRecord Record = new ImageMetaDataRecord(msg, GetImageFilePath(msg.PathToImage));
            string imageMetaDataFileNameSub = Utility.Utility.FileNameTokenSubstitution(ImageMetaDataFileName, msg);
            Logger.Debug($"ImageMetaData file name: {ImageMetaDataFileName} -> {imageMetaDataFileNameSub}");

            if (CSVEnabled) {
                string ImageMetaDataFileName = Path.Combine(ImageDirectory, $"{imageMetaDataFileNameSub}.csv");
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
                string ImageMetaDataFileName = Path.Combine(ImageDirectory, $"{imageMetaDataFileNameSub}.json");
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

        private void WriteWeatherMetaData(ImageSavedEventArgs msg) {

            if (!WeatherEnabled) {
                return;
            }

            if (msg.MetaData.WeatherData == null) {
                Logger.Warning("weather data output is enabled but no weather metadata is present");
                return;
            }

            string ImageDirectory = GetImageDirectory(msg.PathToImage);
            Logger.Debug($"ImageDirectory: {ImageDirectory}");

            WeatherMetaDataRecord Record = new WeatherMetaDataRecord(msg);
            string weatherMetaDataFileNameSub = Utility.Utility.FileNameTokenSubstitution(WeatherMetaDataFileName, msg);
            Logger.Debug($"WeatherMetaData file name: {WeatherMetaDataFileName} -> {weatherMetaDataFileNameSub}");

            if (CSVEnabled) {
                string WeatherMetaDataFileName = Path.Combine(ImageDirectory, $"{weatherMetaDataFileNameSub}.csv");
                Logger.Info($"Writing CSV weather metadata: {WeatherMetaDataFileName}");

                bool exists = File.Exists(WeatherMetaDataFileName);

                using (var writer = File.AppendText(WeatherMetaDataFileName))
                using (var csv = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture)) {
                    if (!exists) {
                        csv.WriteHeader<WeatherMetaDataRecord>();
                        csv.NextRecord();
                    }
                    csv.WriteRecord(Record);
                    csv.NextRecord();
                }
            }

            if (JSONEnabled) {
                string WeatherMetaDataFileName = Path.Combine(ImageDirectory, $"{weatherMetaDataFileNameSub}.json");
                Logger.Info($"Writing JSON weather metadata: {WeatherMetaDataFileName}");

                JsonSerializer serializer = new JsonSerializer();
                serializer.NullValueHandling = NullValueHandling.Include;
                serializer.Formatting = Formatting.Indented;

                List<WeatherMetaDataRecord> Records;

                bool exists = File.Exists(WeatherMetaDataFileName);

                if (exists) {
                    string json = File.ReadAllText(WeatherMetaDataFileName);
                    Records = JsonConvert.DeserializeObject<List<WeatherMetaDataRecord>>(json);
                    Records.Add(Record);

                    using (StreamWriter sw = new StreamWriter(WeatherMetaDataFileName))
                    using (JsonWriter writer = new JsonTextWriter(sw)) {
                        serializer.Serialize(writer, Records);
                    }
                }
                else {
                    Records = new List<WeatherMetaDataRecord>();
                    Records.Add(Record);

                    using (StreamWriter sw = new StreamWriter(WeatherMetaDataFileName))
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
                case "WeatherEnabled":
                    WeatherEnabled = Properties.Settings.Default.WeatherEnabled;
                    break;
                case "AcquisitionDetailsFileName":
                    AcquisitionDetailsFileName = Properties.Settings.Default.AcquisitionDetailsFileName;
                    break;
                case "ImageMetaDataFileName":
                    ImageMetaDataFileName = Properties.Settings.Default.ImageMetaDataFileName;
                    break;
                case "AutoFocusRunsFileName":
                    AutoFocusRunsFileName = Properties.Settings.Default.AutoFocusRunsFileName;
                    break;
                case "WeatherMetaDataFileName":
                    WeatherMetaDataFileName = Properties.Settings.Default.WeatherMetaDataFileName;
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
            public double CameraTemp { get; set; }
            public double CameraTargetTemp { get; set; }
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
            public double GuidingRMS { get; set; }
            public double GuidingRMSArcSec { get; set; }
            public double GuidingRMSRA { get; set; }
            public double GuidingRMSRAArcSec { get; set; }
            public double GuidingRMSDEC { get; set; }
            public double GuidingRMSDECArcSec { get; set; }
            public int? FocuserPosition { get; set; }
            public double FocuserTemp { get; set; }
            public double RotatorPosition { get; set; }
            public string PierSide { get; set; }

            public ImageMetaDataRecord() {
            }

            public ImageMetaDataRecord(ImageSavedEventArgs msg, string ImageFilePath) {
                ExposureNumber = msg.MetaData.Image.ExposureNumber;
                FilePath = ImageFilePath;
                FilterName = msg.Filter;
                ExposureStart = msg.MetaData.Image.ExposureStart;
                Duration = Utility.Utility.ReformatDouble(msg.Duration);
                Binning = msg.MetaData.Image.Binning?.ToString();

                CameraTemp = Utility.Utility.ReformatDouble(msg.MetaData.Camera.Temperature);
                CameraTargetTemp = Utility.Utility.ReformatDouble(msg.MetaData.Camera.SetPoint);

                Gain = msg.MetaData.Camera.Gain;
                Offset = msg.MetaData.Camera.Offset;

                ADUStDev = Utility.Utility.ReformatDouble(msg.Statistics.StDev);
                ADUMean = Utility.Utility.ReformatDouble(msg.Statistics.Mean);
                ADUMedian = Utility.Utility.ReformatDouble(msg.Statistics.Median);
                ADUMin = msg.Statistics.Min;
                ADUMax = msg.Statistics.Max;

                DetectedStars = msg.StarDetectionAnalysis.DetectedStars;
                HFR = Utility.Utility.ReformatDouble(msg.StarDetectionAnalysis.HFR);
                HFRStDev = Utility.Utility.ReformatDouble(msg.StarDetectionAnalysis.HFRStDev);

                GuidingRMS = GetGuidingMetric(msg.MetaData.Image, msg.MetaData.Image?.RecordedRMS?.Total);
                GuidingRMSArcSec = GetGuidingMetricArcSec(msg.MetaData.Image, msg.MetaData.Image?.RecordedRMS?.Total);
                GuidingRMSRA = GetGuidingMetric(msg.MetaData.Image, msg.MetaData.Image?.RecordedRMS?.RA);
                GuidingRMSRAArcSec = GetGuidingMetricArcSec(msg.MetaData.Image, msg.MetaData.Image?.RecordedRMS?.RA);
                GuidingRMSDEC = GetGuidingMetric(msg.MetaData.Image, msg.MetaData.Image?.RecordedRMS?.Dec);
                GuidingRMSDECArcSec = GetGuidingMetricArcSec(msg.MetaData.Image, msg.MetaData.Image?.RecordedRMS?.Dec);

                FocuserPosition = msg.MetaData.Focuser.Position;
                FocuserTemp = Utility.Utility.ReformatDouble(msg.MetaData.Focuser.Temperature);
                RotatorPosition = Utility.Utility.ReformatDouble(msg.MetaData.Rotator.Position);
                PierSide = GetPierSide(msg.MetaData.Telescope.SideOfPier);
            }

            private double GetGuidingMetric(ImageParameter image, double? metric) {
                return (image.RecordedRMS != null && metric != null) ? Utility.Utility.ReformatDouble((double)metric) : 0.0;
            }

            private double GetGuidingMetricArcSec(ImageParameter image, double? metric) {
                return (image.RecordedRMS != null && metric != null) ? Utility.Utility.ReformatDouble((double)(metric * image.RecordedRMS.Scale)) : 0.0;
            }

            private string GetPierSide(PierSide sideOfPier) {
                switch (sideOfPier) {
                    case NINA.Core.Enum.PierSide.pierEast: return "East";
                    case NINA.Core.Enum.PierSide.pierWest: return "West";
                    default: return "n/a";
                }
            }
        }

        private class WeatherMetaDataRecord {
            public int ExposureNumber { get; set; }
            public DateTime ExposureStart { get; set; }
            public double Temperature { get; set; }
            public double DewPoint { get; set; }
            public double Humidity { get; set; }
            public double Pressure { get; set; }
            public double WindSpeed { get; set; }
            public double WindDirection { get; set; }
            public double WindGust { get; set; }
            public double CloudCover { get; set; }
            public double SkyTemperature { get; set; }
            public double SkyBrightness { get; set; }
            public double SkyQuality { get; set; }

            public WeatherMetaDataRecord() {
            }

            public WeatherMetaDataRecord(ImageSavedEventArgs msg) {
                ExposureNumber = msg.MetaData.Image.ExposureNumber;
                ExposureStart = msg.MetaData.Image.ExposureStart;
                WeatherDataParameter weatherData = msg.MetaData.WeatherData;
                Temperature = SafeRound(weatherData.Temperature, 1);
                DewPoint = SafeRound(weatherData.DewPoint, 1);
                Humidity = weatherData.Humidity;
                Pressure = weatherData.Pressure;
                WindSpeed = weatherData.WindSpeed;
                WindDirection = weatherData.WindDirection;
                WindGust = weatherData.WindGust;
                CloudCover = weatherData.CloudCover;
                SkyTemperature = SafeRound(weatherData.SkyTemperature, 1);
                SkyBrightness = weatherData.SkyBrightness;
                SkyQuality = weatherData.SkyQuality;
            }

            private double SafeRound(double value, int digits) {
                return (Double.IsNaN(value)) ? value : Math.Round(value, digits);
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
                RACoordinates = ReformatRA(msg.MetaData.Target.Coordinates?.RAString);
                DECCoordinates = ReformatDEC(msg.MetaData.Target.Coordinates?.DecString);
                TelescopeName = msg.MetaData.Telescope.Name;
                FocalLength = Utility.Utility.ReformatDouble(msg.MetaData.Telescope.FocalLength);
                FocalRatio = Utility.Utility.ReformatDouble(msg.MetaData.Telescope.FocalRatio);
                CameraName = msg.MetaData.Camera.Name;
                PixelSize = Utility.Utility.ReformatDouble(msg.MetaData.Camera.PixelSize);
                BitDepth = msg.Statistics.BitDepth;
                ObserverLatitude = Utility.Utility.ReformatDouble(msg.MetaData.Observer.Latitude);
                ObserverLongitude = Utility.Utility.ReformatDouble(msg.MetaData.Observer.Longitude);
                ObserverElevation = Utility.Utility.ReformatDouble(msg.MetaData.Observer.Elevation);
            }

            public string ReformatRA(string RAString) {
                try {
                    string pattern = @"(\d+):(\d+):(\d+)";
                    if (Regex.IsMatch(RAString, pattern)) {
                        Match match = Regex.Match(RAString, pattern);
                        return $"{Zeros(match.Groups[1].Value)}h {Zeros(match.Groups[2].Value)}m {Zeros(match.Groups[3].Value)}s";
                    }
                    else {
                        return RAString;
                    }
                }
                catch (Exception) {
                    return "";
                }
            }

            private string Zeros(string value) {
                value = value.TrimStart('0');
                return (value == "") ? "0" : value;
            }

            public string ReformatDEC(string DECString) {
                return DECString != null ? DECString : "";
            }
        }

        private string GetImageDirectory(Uri imageUri) {
            return Path.GetDirectoryName(HttpUtility.UrlDecode(imageUri.AbsolutePath));
        }

        private string GetImageFilePath(Uri imageUri) {
            return HttpUtility.UrlDecode(imageUri.AbsolutePath);
        }

    }
}

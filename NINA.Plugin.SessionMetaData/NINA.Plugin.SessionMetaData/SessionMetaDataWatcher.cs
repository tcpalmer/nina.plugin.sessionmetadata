using CsvHelper;
using Namotion.Reflection;
using Newtonsoft.Json;
using NINA.Core.Enum;
using NINA.Core.Utility;
using NINA.Image.ImageData;
using NINA.Image.Interfaces;
using NINA.WPF.Base.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Text.RegularExpressions;
using System.Web;
using static NINA.Equipment.Model.CaptureSequence;

namespace SessionMetaData.NINAPlugin {

    public class SessionMetaDataWatcher {
        private SessionMetaDataPlugin plugin;

        public SessionMetaDataWatcher(IImageSaveMediator imageSaveMediator, SessionMetaDataPlugin plugin) {
            this.plugin = plugin;
            imageSaveMediator.ImageSaved += ImageSaveMeditator_ImageSaved;
        }

        private void ImageSaveMeditator_ImageSaved(object sender, ImageSavedEventArgs msg) {
            if (!plugin.SessionMetaDataEnabled) {
                Logger.Debug("SessionMetaData not enabled");
                return;
            }

            if (!plugin.NonLightsEnabled && msg.MetaData.Image.ImageType != ImageTypes.LIGHT) {
                Logger.Debug("image is not a light, skipping");
                return;
            }

            if (plugin.NonLightsEnabled && msg.MetaData.Image.ImageType == ImageTypes.SNAPSHOT) {
                Logger.Debug("image is snapshot, skipping");
                return;
            }

            try {
                string outputDirectory = GetOutputDirectory(msg);
                WriteAcquisitionMetaData(msg, outputDirectory);
                WriteImageMetaData(msg, outputDirectory);
                WriteWeatherMetaData(msg, outputDirectory);
            } catch (Exception e) {
                Logger.Warning($"session metadata save failed: {e.Message}\n{e.StackTrace}");
            }
        }

        private void WriteAcquisitionMetaData(ImageSavedEventArgs msg, string outputDirectory) {
            AcquisitionMetaDataRecord Record = new AcquisitionMetaDataRecord(msg);
            string finalFileName = GetFinalOutputFileName(outputDirectory, plugin.AcquisitionDetailsFileName, msg);
            Logger.Debug($"AcquisitionDetails file name: {plugin.AcquisitionDetailsFileName} -> {finalFileName}");

            if (plugin.CSVEnabled) {
                string AcquisitionFileName = $"{finalFileName}.csv";

                // Only write this once per output directory
                if (!File.Exists(AcquisitionFileName)) {
                    Logger.Info($"Writing CSV acquisition summary: {AcquisitionFileName}");

                    using (var writer = File.AppendText(AcquisitionFileName))
                    using (var csv = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture)) {
                        csv.WriteHeader<AcquisitionMetaDataRecord>();
                        csv.NextRecord();
                        csv.WriteRecord(Record);
                        csv.NextRecord();
                    }
                } else {
                    Logger.Warning($"acquisition file already exists, will not overwrite: {AcquisitionFileName}");
                }
            }

            if (plugin.JSONEnabled) {
                string AcquisitionFileName = $"{finalFileName}.json";

                // Only write this once per output directory
                if (!File.Exists(AcquisitionFileName)) {
                    Logger.Info($"Writing JSON acquisition summary: {AcquisitionFileName}");

                    JsonSerializer serializer = new JsonSerializer();
                    serializer.NullValueHandling = NullValueHandling.Include;
                    serializer.Formatting = Formatting.Indented;

                    using (StreamWriter sw = new StreamWriter(AcquisitionFileName))
                    using (JsonWriter writer = new JsonTextWriter(sw)) {
                        serializer.Serialize(writer, Record);
                    }
                } else {
                    Logger.Warning($"acquisition file already exists, will not overwrite: {AcquisitionFileName}");
                }
            }
        }

        private void WriteImageMetaData(ImageSavedEventArgs msg, string outputDirectory) {
            ImageMetaDataRecord Record = new ImageMetaDataRecord(msg, GetImageFilePath(msg.PathToImage));
            string finalFileName = GetFinalOutputFileName(outputDirectory, plugin.ImageMetaDataFileName, msg);
            Logger.Debug($"ImageMetaData file name: {plugin.ImageMetaDataFileName} -> {finalFileName}");

            if (plugin.CSVEnabled) {
                string ImageMetaDataFileName = $"{finalFileName}.csv";
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

            if (plugin.JSONEnabled) {
                string ImageMetaDataFileName = $"{finalFileName}.json";
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
                } else {
                    Records = new List<ImageMetaDataRecord>();
                    Records.Add(Record);

                    using (StreamWriter sw = new StreamWriter(ImageMetaDataFileName))
                    using (JsonWriter writer = new JsonTextWriter(sw)) {
                        serializer.Serialize(writer, Records);
                    }
                }
            }
        }

        private void WriteWeatherMetaData(ImageSavedEventArgs msg, string outputDirectory) {
            if (!plugin.WeatherEnabled) {
                return;
            }

            if (msg.MetaData.WeatherData == null) {
                Logger.Warning("weather data output is enabled but no weather metadata is present");
                return;
            }

            WeatherMetaDataRecord Record = new WeatherMetaDataRecord(msg);
            string finalFileName = GetFinalOutputFileName(outputDirectory, plugin.WeatherMetaDataFileName, msg);
            Logger.Debug($"WeatherMetaData file name: {plugin.WeatherMetaDataFileName} -> {finalFileName}");

            if (plugin.CSVEnabled) {
                string WeatherMetaDataFileName = $"{finalFileName}.csv";
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

            if (plugin.JSONEnabled) {
                string WeatherMetaDataFileName = $"{finalFileName}.json";
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
                } else {
                    Records = new List<WeatherMetaDataRecord>();
                    Records.Add(Record);

                    using (StreamWriter sw = new StreamWriter(WeatherMetaDataFileName))
                    using (JsonWriter writer = new JsonTextWriter(sw)) {
                        serializer.Serialize(writer, Records);
                    }
                }
            }
        }

        public class ImageMetaDataRecord {
            public int ExposureNumber { get; set; }
            public string FilePath { get; set; }
            public string FilterName { get; set; }
            public string ExposureStart { get; set; }
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
            public double FWHM { get; set; }
            public double Eccentricity { get; set; }
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
            public double Airmass { get; set; }
            public string ExposureStartUTC { get; set; }
            public double MountRA { get; set; }
            public double MountDec { get; set; }
            public string ImageType { get; set; }
            public ImageMetaDataRecord() {
            }

            public ImageMetaDataRecord(ImageSavedEventArgs msg, string ImageFilePath) {
                ExposureNumber = msg.MetaData.Image.ExposureNumber;
                FilePath = ImageFilePath;
                FilterName = msg.Filter;
                ExposureStart = Utility.Utility.FormatDateTime(msg.MetaData.Image.ExposureStart);
                ExposureStartUTC = Utility.Utility.FormatDateTimeISO8601(msg.MetaData.Image.ExposureStart);
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

                FWHM = GetHocusFocusMetric(msg.StarDetectionAnalysis, "FWHM");
                Eccentricity = GetHocusFocusMetric(msg.StarDetectionAnalysis, "Eccentricity");

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

                Airmass = Utility.Utility.ReformatDouble(msg.MetaData.Telescope.Airmass);

                if (msg.MetaData.Telescope.Coordinates != null) {
                    MountRA = msg.MetaData.Telescope.Coordinates.RADegrees;
                    MountDec = msg.MetaData.Telescope.Coordinates.Dec;
                } else {
                    MountRA = 0;
                    MountDec = 0;
                }

                ImageType = msg.MetaData.Image.ImageType;
            }

            private double GetHocusFocusMetric(IStarDetectionAnalysis starDetectionAnalysis, string propertyName) {
                return starDetectionAnalysis.HasProperty(propertyName) ?
                    (Double)starDetectionAnalysis.GetType().GetProperty(propertyName).GetValue(starDetectionAnalysis) :
                    Double.NaN;
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
            public string ExposureStart { get; set; }
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
            public string ExposureStartUTC { get; set; }

            public WeatherMetaDataRecord() {
            }

            public WeatherMetaDataRecord(ImageSavedEventArgs msg) {
                ExposureNumber = msg.MetaData.Image.ExposureNumber;
                ExposureStart = Utility.Utility.FormatDateTime(msg.MetaData.Image.ExposureStart);
                ExposureStartUTC = Utility.Utility.FormatDateTimeISO8601(msg.MetaData.Image.ExposureStart);
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
                    } else {
                        return RAString;
                    }
                } catch (Exception) {
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

        private string GetOutputDirectory(ImageSavedEventArgs msg) {
            if (String.IsNullOrEmpty(plugin.MetaDataOutputDirectory)) {
                Logger.Debug("MetaDataOutputDirectory is empty, defaulting to image save directory");
                return GetImageDirectory(msg.PathToImage);
            }

            if (!Directory.Exists(plugin.MetaDataOutputDirectory)) {
                Logger.Warning($"MetaDataOutputDirectory does not exist ({plugin.MetaDataOutputDirectory}), defaulting to image save directory");
                return GetImageDirectory(msg.PathToImage);
            }

            if (!IsDirectoryWriteable(plugin.MetaDataOutputDirectory)) {
                Logger.Warning($"MetaDataOutputDirectory is not writable ({plugin.MetaDataOutputDirectory}), defaulting to image save directory");
                return GetImageDirectory(msg.PathToImage);
            }

            return plugin.MetaDataOutputDirectory;
        }

        private bool IsDirectoryWriteable(string path) {
            try {
                DirectorySecurity ds = new DirectoryInfo(path).GetAccessControl();
                return true;
            } catch (Exception e) {
                Logger.Trace($"exception checking access to directory ({plugin.MetaDataOutputDirectory}): {e.Message}");
                return false;
            }
        }

        private string GetFinalOutputFileName(string outputDirectory, string metadataFileName, ImageSavedEventArgs msg) {
            List<string> pathComponents = new List<string>();
            pathComponents.Add(outputDirectory);

            // Extract any relative path prefix from the base metadata file name
            int pos = metadataFileName.LastIndexOf(@"\");
            string pathPrefix = (pos != -1) ? metadataFileName.Substring(0, pos) : "";

            // Run each directory of the path prefix through token substitution
            if (pathPrefix.Length > 0) {
                string[] dirs = pathPrefix.Split('\\');
                foreach (string dir in dirs) {
                    pathComponents.Add(Utility.Utility.FileNameTokenSubstitution(dir, msg));
                }
            }

            // Create any new sub-directories
            Directory.CreateDirectory(String.Join("\\", pathComponents.ToArray()));

            // Run the remaining metadata file name through token substitution
            string strippedMetadataFileName = (pos != -1) ? metadataFileName.Substring(pos) : metadataFileName;
            pathComponents.Add(Utility.Utility.FileNameTokenSubstitution(strippedMetadataFileName, msg));

            // Combine all components into final path
            return Path.Combine(pathComponents.ToArray());
        }
    }
}
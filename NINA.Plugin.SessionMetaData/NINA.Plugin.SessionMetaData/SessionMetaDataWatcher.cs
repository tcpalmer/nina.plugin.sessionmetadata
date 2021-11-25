using CsvHelper;
using NINA.Core.Enum;
using NINA.Core.Utility;
using NINA.Image.ImageData;
using NINA.WPF.Base.Interfaces.Mediator;
using System;
using System.IO;

namespace SessionMetaData.NINAPlugin {

    public class SessionMetaDataWatcher {

        private static string ACQUISITION_FILE_NAME = "AcquisitionDetails.csv";
        private static string IMAGE_METADATA_FILE_NAME = "ImageMetaData.csv";
        private bool SessionMetaDataEnabled;

        public SessionMetaDataWatcher(IImageSaveMediator imageSaveMediator) {
            SessionMetaDataEnabled = Properties.Settings.Default.SessionMetaDataEnabled;
            imageSaveMediator.ImageSaved += ImageSaveMeditator_ImageSaved;
        }

        // TODO: we're not picking up a change in enabled w/in the NINA session - you have to stop/start for the change to take effect.
        // TODO: do we care that the acquisition details might change in the same dir but will only be written once?

        // TODO: we could allow JSON output.  Even the image metadata could be a JSON list and we read/write it each time.

        private void ImageSaveMeditator_ImageSaved(object sender, ImageSavedEventArgs msg) {

            if (!SessionMetaDataEnabled) {
                Logger.Trace("SessionMetaData not enabled");
                return;
            }

            if (msg.MetaData.Image.ImageType != "LIGHT") {
                Logger.Trace("image is not a light, skipping");
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
            string AcquisitionFileName = Path.Combine(ImageDirectory, ACQUISITION_FILE_NAME);
            Logger.Trace($"AcquisitionFileName: {AcquisitionFileName}");

            // Only write this once per image output directory
            if (File.Exists(AcquisitionFileName)) {
                return;
            }

            AcquisitionMetaDataRecord Record = new AcquisitionMetaDataRecord {
                TargetName = msg.MetaData.Target.Name,
                RACoordinates = msg.MetaData.Target.Coordinates?.RAString,
                DECCoordinates = msg.MetaData.Target.Coordinates?.DecString,
                TelescopeName = msg.MetaData.Telescope.Name,
                FocalLength = msg.MetaData.Telescope.FocalLength,
                FocalRatio = msg.MetaData.Telescope.FocalRatio,
                CameraName = msg.MetaData.Camera.Name,
                PixelSize = msg.MetaData.Camera.PixelSize,
                BitDepth = msg.Statistics.BitDepth,
                ObserverLatitude = msg.MetaData.Observer.Latitude,
                ObserverLongitude = msg.MetaData.Observer.Longitude,
                ObserverElevation = msg.MetaData.Observer.Elevation,
            };

            using (var writer = File.AppendText(AcquisitionFileName))
            using (var csv = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture)) {
                csv.WriteHeader<AcquisitionMetaDataRecord>();
                csv.NextRecord();
                csv.WriteRecord(Record);
                csv.NextRecord();
            }
        }

        private void WriteImageMetaData(ImageSavedEventArgs msg) {

            string ImageFilePath = msg.PathToImage.ToString();
            Logger.Trace($"ImageFilePath: {ImageFilePath}");
            string ImageDirectory = GetImageDirectory(ImageFilePath);
            Logger.Trace($"ImageDir: {ImageDirectory}");
            string ImageMetaDataFileName = Path.Combine(ImageDirectory, IMAGE_METADATA_FILE_NAME);
            Logger.Trace($"ImageMetaDataFileName: {ImageMetaDataFileName}");

            ImageMetaDataRecord Record = new ImageMetaDataRecord {
                ExposureNumber = msg.MetaData.Image.Id,
                FilePath = ImageFilePath,
                FilterName = msg.Filter,
                ExposureStart = msg.MetaData.Image.ExposureStart,
                Duration = msg.Duration,
                Binning = msg.MetaData.Image.Binning?.ToString(),
                CameraTemperature = msg.MetaData.Camera.Temperature,
                Gain = msg.MetaData.Camera.Gain,
                Offset = msg.MetaData.Camera.Offset,
                ADUStDev = msg.Statistics.StDev,
                ADUMean = msg.Statistics.Mean,
                ADUMedian = msg.Statistics.Median,
                ADUMin = msg.Statistics.Min,
                ADUMax = msg.Statistics.Max,
                DetectedStars = msg.StarDetectionAnalysis.DetectedStars,
                HFR = msg.StarDetectionAnalysis.HFR,
                HFRStDev = msg.StarDetectionAnalysis.HFRStDev,
                GuidingRMS = GetGuidingRMS(msg.MetaData.Image),
                GuidingRMSArcSec = GetGuidingRMSArcSec(msg.MetaData.Image),
                FocuserPosition = msg.MetaData.Focuser.Position,
                PierSide = GetPierSide(msg.MetaData.Telescope.SideOfPier)
            };

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

        private class ImageMetaDataRecord {
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
        }

        private class AcquisitionMetaDataRecord {
            public string TargetName { get; set; }
            public string RACoordinates { get; set; }
            public string DECCoordinates { get; set; }
            public string TelescopeName { get; set; }
            public double FocalLength { get; set; }
            public double FocalRatio { get; set; }
            public string CameraName { get; set; }
            public double PixelSize { get; set; }
            public int BitDepth { get; set; }
            public double ObserverLatitude { get; set; }
            public double ObserverLongitude { get; set; }
            public double ObserverElevation { get; set; }
        }

        private string GetImageDirectory(string ImageFilePath) {
            return Path.GetDirectoryName(ImageFilePath.Substring(8)); // skip 'file:///'
        }

        private string GetGuidingRMS(ImageParameter image) {
            return image.RecordedRMS != null ? image.RecordedRMS.Total.ToString() : "n/a";
        }

        private string GetGuidingRMSArcSec(ImageParameter image) {
            return image.RecordedRMS != null ? (image.RecordedRMS.Total * image.RecordedRMS.Scale).ToString() : "n/a";
        }

        private string GetPierSide(PierSide sideOfPier) {
            switch (sideOfPier) {
                case PierSide.pierEast: return "East";
                case PierSide.pierWest: return "West";
                default: return "unknown";
            }
        }

    }
}

using NINA.Core.Model;
using NINA.Image.ImageData;
using NINA.WPF.Base.Interfaces.Mediator;
using System;
using System.IO;

namespace SessionMetaData.NINAPlugin.Utility {

    public static class Utility {

        /// <summary>
        /// Uses the core NINA image file pattern tokens to support dynamic file names.  The following tokens are supported:
        /// $$DATE$$, $$DATEMINUS12$$, $$DATETIME$$, $$TARGETNAME$$, $$FILTER$$
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="eventArgs"></param>
        /// <returns>file name with tokens substituted</returns>
        public static string FileNameTokenSubstitution(string fileName, ImageSavedEventArgs eventArgs) {
            if (String.IsNullOrEmpty(fileName)) {
                return fileName;
            }

            ImageMetaData metadata = eventArgs.MetaData;
            ImagePatterns p = new ImagePatterns();

            p.Set(ImagePatternKeys.DateTime, metadata.Image.ExposureStart.ToString("yyyy-MM-dd_HH-mm-ss"));
            p.Set(ImagePatternKeys.TargetName, metadata.Target.Name);
            p.Set(ImagePatternKeys.Filter, eventArgs.Filter);
            p.Set(ImagePatternKeys.Date, metadata.Image.ExposureStart.ToString("yyyy-MM-dd"));
            if (metadata.Image.ExposureStart > DateTime.MinValue.AddHours(12)) {
                p.Set(ImagePatternKeys.DateMinus12, metadata.Image.ExposureStart.AddHours(-12).ToString("yyyy-MM-dd"));
            }

            // Strip invalid characters before core method (which would replace them with '_')
            fileName = string.Join("", fileName.Split(Path.GetInvalidFileNameChars()));

            // Perform substitutions and replace any remaining spaces
            return p.GetImageFileString(fileName).Replace(' ', '_');
        }

        /// <summary>
        /// Format a date/time to yyyy-MM-dd HH:mm
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string FormatDateTime(DateTime dateTime) {
            return dateTime.ToLocalTime().ToString("yyyy-MM-dd HH\\:mm");
        }

        /// <summary>
        /// Convert a date/time to UTC and output in ISO 8601 format
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string FormatDateTimeISO8601(DateTime dateTime) {
            return dateTime.ToUniversalTime().ToString("O");
        }

        /// <summary>
        /// Format a double, convert back to double
        /// </summary>
        /// <param name="value"></param>
        /// <returns>double reformatted</returns>
        public static Double ReformatDouble(Double value) {
            return Double.Parse(String.Format("{0:0.####}", value));
        }
    }
}
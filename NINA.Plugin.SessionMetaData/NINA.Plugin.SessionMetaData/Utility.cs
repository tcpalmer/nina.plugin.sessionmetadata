﻿using NINA.Core.Model;
using NINA.Image.ImageData;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace SessionMetaData.NINAPlugin.Utility {

    public static class Utility {

        /// <summary>
        /// Uses the core NINA image file pattern tokens to support dynamic file names.  The following tokens are supported:
        /// $$DATE$$, $$DATEMINUS12$$, $$DATETIME$$, $$TARGETNAME$$
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="metadata"></param>
        /// <returns>file name with tokens substituted</returns>
        public static string FileNameTokenSubstitution(string fileName, ImageMetaData metadata) {
            if (String.IsNullOrEmpty(fileName)) {
                return fileName;
            }

            ImagePatterns p = new ImagePatterns();
            p.Set(ImagePatternKeys.DateTime, metadata.Image.ExposureStart.ToString("yyyy-MM-dd_HH-mm-ss"));
            p.Set(ImagePatternKeys.TargetName, metadata.Target.Name);
            p.Set(ImagePatternKeys.Date, metadata.Image.ExposureStart.ToString("yyyy-MM-dd"));
            if (metadata.Image.ExposureStart > DateTime.MinValue.AddHours(12)) {
                p.Set(ImagePatternKeys.DateMinus12, metadata.Image.ExposureStart.AddHours(-12).ToString("yyyy-MM-dd"));
            }

            // Strip invalid characters before core method (which would replace them with '_')
            fileName = string.Join("", fileName.Split(Path.GetInvalidFileNameChars()));

            // Perform substitutions and replace any remaining spaces
            return p.GetImageFileString(fileName).Replace(' ', '_');
        }
    }
}
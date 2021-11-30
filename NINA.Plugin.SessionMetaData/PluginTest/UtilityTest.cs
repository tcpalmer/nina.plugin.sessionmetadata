using FluentAssertions;
using NINA.Image.ImageData;
using NUnit.Framework;
using System;

namespace SessionMetaData.NINAPlugin.Test {

    public class UtilityTest {

        [Test]
        public void TestIt() {

            string result = Utility.Utility.FileNameTokenSubstitution(null, GenImageMetaData("My Target Name"));
            result.Should().BeNull();

            result = Utility.Utility.FileNameTokenSubstitution("", GenImageMetaData("My Target Name"));
            result.Should().BeEmpty();

            result = Utility.Utility.FileNameTokenSubstitution("NoTokens", GenImageMetaData("My Target Name"));
            result.Should().Be("NoTokens");

            result = Utility.Utility.FileNameTokenSubstitution("foo-$$NADA$$-bar", GenImageMetaData("My Target Name"));
            result.Should().Be("foo-$$NADA$$-bar");

            result = Utility.Utility.FileNameTokenSubstitution("foo-$$DATE$$-bar", GenImageMetaData(new DateTime(2021, 6, 1, 12, 32, 30), "My Target Name"));
            result.Should().Be("foo-2021-06-01-bar");

            result = Utility.Utility.FileNameTokenSubstitution("foo-$$DATETIME$$-bar", GenImageMetaData(new DateTime(2021, 6, 1, 12, 32, 30), "My Target Name"));
            result.Should().Be("foo-2021-06-01_12-32-30-bar");

            result = Utility.Utility.FileNameTokenSubstitution("foo-$$DATEMINUS12$$-bar", GenImageMetaData(new DateTime(2021, 6, 1, 9, 32, 30), "My Target Name"));
            result.Should().Be("foo-2021-05-31-bar");

            result = Utility.Utility.FileNameTokenSubstitution("foo-$$TARGETNAME$$_$$DATE$$-bar", GenImageMetaData(new DateTime(2021, 6, 1, 12, 32, 30), "My Target Name"));
            result.Should().Be("foo-My_Target_Name_2021-06-01-bar");

            result = Utility.Utility.FileNameTokenSubstitution("foo/$$TARGETNAME$$/bar", GenImageMetaData("My Target Name"));
            result.Should().Be("fooMy_Target_Namebar");

            result = Utility.Utility.FileNameTokenSubstitution("foo\\$$TARGETNAME$$\\bar", GenImageMetaData("My Target Name"));
            result.Should().Be("fooMy_Target_Namebar");

            result = Utility.Utility.FileNameTokenSubstitution("Acquisition/Details-$$DATEMINUS12$$", GenImageMetaData(new DateTime(2021, 6, 1, 12, 32, 30), "My Target Name"));
            result.Should().Be("AcquisitionDetails-2021-06-01");

            result = Utility.Utility.FileNameTokenSubstitution("AcquisitionDetails:|<>\"/\\?*-$$TARGETNAME$$", GenImageMetaData(new DateTime(2021, 6, 1, 12, 32, 30), "My Target Name"));
            result.Should().Be("AcquisitionDetails-My_Target_Name");
        }

        private ImageMetaData GenImageMetaData(string targetName) {
            return GenImageMetaData(new DateTime(), targetName);
        }

        private ImageMetaData GenImageMetaData(DateTime dateTime, string targetName) {
            ImageMetaData metadata = new ImageMetaData();
            metadata.Image.ExposureStart = dateTime;
            TargetParameter tp = new TargetParameter();
            tp.Name = targetName;
            metadata.Target = tp;
            return metadata;
        }
    }
}
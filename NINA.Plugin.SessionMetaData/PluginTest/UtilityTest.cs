using FluentAssertions;
using NINA.Image.ImageData;
using NINA.WPF.Base.Interfaces.Mediator;
using NUnit.Framework;
using System;

namespace SessionMetaData.NINAPlugin.Test {

    public class UtilityTest {

        [Test]
        public void TestFileNameTokenSubstitution() {
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

            result = Utility.Utility.FileNameTokenSubstitution("foo\\$$TARGETNAME$$-$$FILTER$$\\bar", GenImageMetaData("My Target Name"));
            result.Should().Be("fooMy_Target_Name-Foobar");

            result = Utility.Utility.FileNameTokenSubstitution("Acquisition/Details-$$DATEMINUS12$$", GenImageMetaData(new DateTime(2021, 6, 1, 12, 32, 30), "My Target Name"));
            result.Should().Be("AcquisitionDetails-2021-06-01");

            result = Utility.Utility.FileNameTokenSubstitution("AcquisitionDetails:|<>\"/\\?*-$$TARGETNAME$$", GenImageMetaData(new DateTime(2021, 6, 1, 12, 32, 30), "My Target Name"));
            result.Should().Be("AcquisitionDetails-My_Target_Name");
        }

        [Test]
        public void TestFormatDateTime() {
            Utility.Utility.FormatDateTime(new DateTime(2024, 1, 2, 3, 4, 5)).Should().Be("2024-01-01 22:04");
            Utility.Utility.FormatDateTime(new DateTime(2024, 12, 31, 17, 18, 19)).Should().Be("2024-12-31 12:18");
        }

        [Test]
        public void TestReformatDouble() {
            Utility.Utility.ReformatDouble(1.2345678).Should().Be(1.2346);
            Utility.Utility.ReformatDouble(1.23).Should().Be(1.23);
            Utility.Utility.ReformatDouble(31867.87451480006246).Should().Be(31867.8745);
            Utility.Utility.ReformatDouble(0.4510157391181826).Should().Be(0.451);
        }

        private ImageSavedEventArgs GenImageMetaData(string targetName) {
            return GenImageMetaData(new DateTime(), targetName);
        }

        private ImageSavedEventArgs GenImageMetaData(DateTime dateTime, string targetName) {
            ImageMetaData metadata = new ImageMetaData();
            metadata.Image.ExposureStart = dateTime;
            TargetParameter tp = new TargetParameter();
            tp.Name = targetName;
            metadata.Target = tp;

            ImageSavedEventArgs args = new ImageSavedEventArgs();
            args.MetaData = metadata;
            args.Filter = "Foo";

            return args;
        }
    }
}
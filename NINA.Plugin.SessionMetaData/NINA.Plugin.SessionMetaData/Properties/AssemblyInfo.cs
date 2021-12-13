using System.Reflection;
using System.Runtime.InteropServices;

[assembly: Guid("DCB1D37B-F121-4966-99EC-D11410C562B6")]

[assembly: AssemblyTitle("Session Metadata")]
[assembly: AssemblyDescription("Write additional metadata for an imaging session")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Tom Palmer @tcpalmer")]
[assembly: AssemblyProduct("SessionMetaData.NINAPlugin")]
[assembly: AssemblyCopyright("Copyright © 2021")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: AssemblyVersion("1.2.0.0")]
[assembly: AssemblyFileVersion("1.2.0.0")]

// The minimum Version of N.I.N.A. that this plugin is compatible with
[assembly: AssemblyMetadata("MinimumApplicationVersion", "2.0.0.2001")]

[assembly: AssemblyMetadata("License", "MPL-2.0")]
[assembly: AssemblyMetadata("LicenseURL", "https://www.mozilla.org/en-US/MPL/2.0/")]
[assembly: AssemblyMetadata("Repository", "https://github.com/tcpalmer/nina.plugin.sessionmetadata/")]
[assembly: AssemblyMetadata("FeaturedImageURL", "https://raw.githubusercontent.com/tcpalmer/nina.plugin.sessionmetadata/main/NINA.Plugin.SessionMetaData/assets/metadata-tag-icon.png?raw=true")]

[assembly: AssemblyMetadata("LongDescription", @"Session Metadata will output additional acquisition and per-image information associated with an imaging session.  The files will be written to the folder containing your images. If Session Metadata is enabled, two types of metadata will be written:

* Acquisition Details: Additional information assciated with the session, including target, telescope, camera, and observing location.  This file will be written once for each new imaging folder.
* Image Metadata: Additional information assciated with each captured image, including exposure details, ADU statistics, HFR, detected stars, guiding RMS and more. An aggregate file will be updated for each image written to the folder.

There is certainly overlap between this plugin and other parts of NINA.  For example, NINA will write metadata to the FITS or XISF image headers - but it's not really convenient to access for a set of images.  You can also click save in the HFR History panel in the Imaging tab to save a CSV file of image metadata - but you have to remember to do it.  This plugin automates the metadata capture process and makes it easy to view and compare data.

# Output Formats #
Either or both output types can be enabled.
* If CSV output is enabled, CSV files will be written: AcquisitionDetails.csv and ImageMetaData.csv (containing one row per image).
* If JSON output is enabled, JSON files will be written: AcquisitionDetails.json and ImageMetaData.json (containing one list element per image).

The output file names can be customized - see above.

# Getting Help #
* Ask for help in the #plugin-discussions channel on the NINA project [Discord server](https://discord.com/invite/rWRbVbw).
* [Source code](https://github.com/tcpalmer/nina.plugin.sessionmetadata)
* [Change log](https://github.com/tcpalmer/nina.plugin.sessionmetadata/blob/main/CHANGELOG.md)

Session Metadata is provided 'as is' under the terms of the [Mozilla Public License 2.0](https://github.com/tcpalmer/nina.plugin.sessionmetadata/blob/main/LICENSE.txt)
")]

[assembly: ComVisible(false)]

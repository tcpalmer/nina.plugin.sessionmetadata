# Session Metadata NINA Plugin

Session Metadata will output additional acquisition and per-image information associated with an imaging session.  The files will be written to the folder containing your images (see options for overriding this). If Session Metadata is enabled, three types of metadata will be written:

* Acquisition Details: Additional information associated with the session, including target, telescope, camera, and observing location.  This file will be written once for each new imaging folder.
* Image Metadata: Additional information associated with each captured image, including exposure details, ADU statistics, HFR, detected stars, guiding RMS and more. An aggregate file will be updated for each image written to the folder.
* Weather Metrics: If enabled, weather metrics as reported by the connected observing conditions data source.

There is certainly overlap between this plugin and other parts of NINA.  For example, NINA will write metadata to the FITS or XISF image headers - but it's not really convenient to access for a set of images.  You can also click save in the HFR History panel in the Imaging tab to save a CSV file of image metadata - but you have to remember to do it.  This plugin automates the metadata capture process and makes it easy to view and compare data.

## Output Formats
Either or both output types can be enabled.
* If CSV output is enabled, CSV files will be written: AcquisitionDetails.csv and ImageMetaData.csv (containing one row per image).
* If JSON output is enabled, JSON files will be written: AcquisitionDetails.json and ImageMetaData.json (containing one list element per image).

The output file names can be customized to whatever you prefer - including inserting dynamic token values like \$\$DATE\$\$, \$\$DATEMINUS12$$, \$\$DATETIME\$\$, \$\$TARGETNAME\$\$, and \$\$FILTER\$\$.  See the options for details.

## Getting Help
Ask for help in the #plugin-discussions channel on the NINA project [Discord server](https://discord.com/invite/rWRbVbw).

## Acknowledgements

Thanks to all the NINA contributors and plugin writers for providing a well-blazed trail to follow.  Thanks especially to Stephen Eckenrode and his Lightbucket plugin which provided direct code examples.

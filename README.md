# Session Metadata NINA Plugin

Session Metadata will output additional acquisition and per-image information associated with an imaging session.  The files will be written to the folder containing your images. If Session Metadata is enabled, two types of metadata will be written:

* AcquisitionDetails: Additional information assciated with the session, including target, telescope, camera, and observing location.  This file will be written once for each new imaging folder.
* ImageMetaData: Additional information assciated with each captured image, including exposure details, ADU statistics, HFR, detected stars, guiding RMS and more. An aggregate file will be updated for each image written to the folder.

## Output Formats
Either or both output types can be enabled.
* If CSV output is enabled, CSV files will be written: AcquisitionDetails.csv and ImageMetaData.csv (containing one row per image).
* If JSON output is enabled, JSON files will be written: AcquisitionDetails.json and ImageMetaData.json (containing one list element per image).

The output file names can be customized to whatever you prefer - including inserting dynamic token values like $$DATE$$, $$DATEMINUS12$$, $$DATETIME$$, and $$TARGETNAME$$.  See the options for details.

## Getting Help
Ask for help in the #plugin-discussions channel on the NINA project [Discord server](https://discord.com/invite/rWRbVbw).

## Acknowledgements

Thanks to all the NINA contributors and plugin writers for providing a well-blazed trail to follow.  Thanks especially to Stephen Eckenrode and his Lightbucket plugin which provided direct code examples.

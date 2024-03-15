# Session Metadata

## 2.4.0.0 - 2024-03-15
* Can now write metadata for flats, darks, and bias frames (off by default)
* Output date format for exposure start times is now yyyy-MM-dd HH:mm

## 2.3.4.0 - 2023-10-27
* Added FWHM and eccentricity (calculated by Hocus Focus) to the output metrics.

## 2.3.0.0 - 2022-12-22
* Migrated to .NET 7 for NINA 3

## 1.3.0.0 - 2002-08-XX
* Added much better support for specifying the location of metadata files
* Added output of weather metrics (separate file)
* Added output of airmass
* Updated to latest NINA stable dependencies

## 1.2.0.0 - 2021-12-13
* Added additional per-image fields: CameraTargetTemp, FocuserTemp, RotatorPosition, guiding RMS in RA and DEC
* Added support for \$\$FILTER\$\$ in file names
* Fixed a bug in directory path encoding
* Added a plugin logo

## 1.1.0.0 - 2021-12-01
* Added support for dynamic output file names using a subset of the image file pattern tokens
* Restricted the output of numerics to four decimal places in the output files

## 1.0.0.0 - 2021-11-27
* Initial release

## 0.0.0.1 - 2021-11-26 (beta)
* Initial test release

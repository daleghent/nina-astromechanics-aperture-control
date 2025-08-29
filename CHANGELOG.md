# Astromechanics Aperture Control

## 2.1.0.0 - 2025-08-28
* Overhaul of internal logic to improve reliability under contempoary NINA use-cases. This corrects issues with sequences where the focuser is not already connected at the start of the sequence.
* The attached lens name is now displayed in the **Lens Focal Ratio** instruction.
* Updated to .NET 8.
* **NOTE:** These changes are limited to the Astromechanics Aperture Control plugin for NINA. There are no changes to the Enhanced ASCOM Lens Driver.

## 2.0.0.0 - 2022-11-12
* Updated plugin to Microsoft .NET 7 for compatibility with NINA 3.0. The version of Astromechanics Aperture Control that is compatible with NINA 2.x will remain under the 1.x versioning scheme, and Astromechanics Aperture Control 2.x and later is relvant only to NINA 3.x and later.

## 1.0.0.1 - 2022-04-13
* Fixed driver check logic that lead to NINA crash

## 1.0.0.0 - 2022-04-11
* Initial Release

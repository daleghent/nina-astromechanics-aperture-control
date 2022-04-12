using System.Reflection;
using System.Runtime.InteropServices;

// [MANDATORY] The following GUID is used as a unique identifier of the plugin
[assembly: Guid("CD21B234-F4A2-47B0-89B9-D2E77B97FE4A")]

// [MANDATORY] The assembly versioning
//Should be incremented for each new release build of a plugin
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

// [MANDATORY] The name of your plugin
[assembly: AssemblyTitle("Astromechanics Aperture Control")]
// [MANDATORY] A short description of your plugin
[assembly: AssemblyDescription("Controls the focal ratio of a Canon lens when using an Astromechanics Canon Lens Controller and a custom ASCOM driver")]

// The following attributes are not required for the plugin per se, but are required by the official manifest meta data

// Your name
[assembly: AssemblyCompany("Dale Ghent")]
// The product name that this plugin is part of
[assembly: AssemblyProduct("Astromechanics Aperture Control")]
[assembly: AssemblyCopyright("Copyright © 2022 Dale Ghent")]

// The minimum Version of N.I.N.A. that this plugin is compatible with
[assembly: AssemblyMetadata("MinimumApplicationVersion", "2.0.0.2056")]

// The license your plugin code is using
[assembly: AssemblyMetadata("License", "MPL-2.0")]
// The url to the license
[assembly: AssemblyMetadata("LicenseURL", "https://www.mozilla.org/en-US/MPL/2.0/")]
// The repository where your pluggin is hosted
[assembly: AssemblyMetadata("Repository", "https://github.com/daleghent/nina-astromechanics-aperture-control")]

// The following attributes are optional for the official manifest meta data

//[Optional] Your plugin homepage - omit if not applicaple
[assembly: AssemblyMetadata("Homepage", "https://daleghent.com/astromechanics-aperture-control")]

//[Optional] Common tags that quickly describe your plugin
[assembly: AssemblyMetadata("Tags", "ASCOM, action, commandblind, commandbool, commandstring")]

//[Optional] A link that will show a log of all changes in between your plugin's versions
[assembly: AssemblyMetadata("ChangelogURL", "https://github.com/daleghent/nina-astromechanics-aperture-control/blob/main/CHANGELOG.md")]

//[Optional] The url to a featured logo that will be displayed in the plugin list next to the name
[assembly: AssemblyMetadata("FeaturedImageURL", "https://daleghent.github.io/nina-plugins/assets/images/aac-logo.png")]
//[Optional] A url to an example screenshot of your plugin in action
//[assembly: AssemblyMetadata("ScreenshotURL", "https://daleghent.github.io/nina-plugins/assets/images/daac-screen1.png")]
//[Optional] An additional url to an example example screenshot of your plugin in action
//[assembly: AssemblyMetadata("AltScreenshotURL", "")]
//[Optional] An in-depth description of your plugin
[assembly: AssemblyMetadata("LongDescription", @"# Astromechanics Aperture Control

**NOTE:** This plugin is useless without the **Enhanced ASCOM Lens Driver**. Please refer to [this plugin's website](https://daleghent.com/astromechanics-aperture-control) for full details on installation and use.

## Getting help

Help for this plugin may be found in the **#plugin-discussions** channel on the NINA project [Discord chat server](https://discord.gg/nighttime-imaging) or by filing an issue report at this plugin's [Github repository](https://github.com/daleghent/nina-astromechanics-aperture-control/issues)")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]
// [Unused]
[assembly: AssemblyConfiguration("")]
// [Unused]
[assembly: AssemblyTrademark("")]
// [Unused]
[assembly: AssemblyCulture("")]

#region "copyright"

/*
    Copyright Dale Ghent <daleg@elemental.org>

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/
*/

#endregion "copyright"

using NINA.Equipment.Equipment.MyFocuser;
using System;
using System.Collections.Generic;

namespace DaleGhent.NINA.AstromechApertureControl {

    public class Utility {

        public static List<string> WantedDrivers() {
            return new List<string>() {
                "ASCOM.EnhancedCanonEF.Focuser",
                "ASCOM.EnhancedCanonEF2.Focuser",
            };
        }

        public static List<string> WantedActions() {
            return new List<string>() {
                "GetApertureIndex",
                "SetApertureIndex",
                "GetFocalRatioList",
                "GetLensModel",
            };
        }

        public static string GetVersion() {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            return fvi.FileVersion;
        }
    }
}

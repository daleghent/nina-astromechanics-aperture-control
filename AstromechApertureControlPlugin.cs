#region "copyright"

/*
    Copyright Dale Ghent <daleg@elemental.org>

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/
*/

#endregion "copyright"

using NINA.Plugin;
using NINA.Plugin.Interfaces;
using System.ComponentModel.Composition;

namespace DaleGhent.NINA.AstromechApertureControl {

    [Export(typeof(IPluginManifest))]
    public class AstromechApertureControlPlugin : PluginBase {

        [ImportingConstructor]
        public AstromechApertureControlPlugin() {
        }
    }
}

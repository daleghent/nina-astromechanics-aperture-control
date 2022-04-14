#region "copyright"

/*
    Copyright Dale Ghent <daleg@elemental.org>
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/
*/

#endregion "copyright"

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NINA.Core.Enum;
using NINA.Core.Locale;
using NINA.Core.Model;
using NINA.Core.Utility;
using NINA.Equipment.Equipment.MyFocuser;
using NINA.Equipment.Interfaces.Mediator;
using NINA.Sequencer.SequenceItem;
using NINA.Sequencer.Validations;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DaleGhent.NINA.AstromechApertureControl {

    [ExportMetadata("Name", "Lens Focal Ratio")]
    [ExportMetadata("Description", "Changes the focal ratio of a lens attached to an Astromechanics Canon Lens Controller")]
    [ExportMetadata("Icon", "CameraSVG")]
    [ExportMetadata("Category", "Astromechanics Lens Controller")]
    [Export(typeof(ISequenceItem))]
    [JsonObject(MemberSerialization.OptIn)]
    public class AstromechApertureInstruction : SequenceItem, IValidatable, IFocuserConsumer {
        private readonly IFocuserMediator focuserMediator;
        private readonly List<string> wantedDrivers;
        private readonly List<string> wantedActions;
        private bool seen = true;

        [ImportingConstructor]
        public AstromechApertureInstruction(IFocuserMediator focuserMediator) {
            this.focuserMediator = focuserMediator;

            wantedDrivers = Utility.WantedDrivers();
            wantedActions = Utility.WantedActions();

            this.focuserMediator = focuserMediator;
            this.focuserMediator.RegisterConsumer(this);
            FocuserInfo = this.focuserMediator.GetInfo();

            UpdateDeviceInfo(FocuserInfo);

            if (DriverAvailable) {
                int.TryParse(focuserMediator.Action("GetApertureIndex", string.Empty), out apertureIndex);
                RaisePropertyChanged("ApertureIndex");
            }
        }

        private AstromechApertureInstruction(AstromechApertureInstruction cloneMe) : this(focuserMediator: cloneMe.focuserMediator) {
            CopyMetaData(cloneMe);
        }

        public override object Clone() {
            return new AstromechApertureInstruction(this) {
                ApertureIndex = ApertureIndex,
            };
        }

        public FocuserInfo FocuserInfo { get; private set; }

        private bool driverAvailable = false;

        public bool DriverAvailable {
            get => driverAvailable;
            private set {
                driverAvailable = value;
                RaisePropertyChanged();
            }
        }

        private List<string> focalRatios = new List<string>();

        public List<string> FocalRatios {
            get => focalRatios;
            private set {
                focalRatios = value;
                RaisePropertyChanged();
            }
        }

        private string lensModel = string.Empty;

        public string LensModel {
            get => lensModel;
            private set {
                if (lensModel != value) {
                    lensModel = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int apertureIndex = 0;

        [JsonProperty]
        public int ApertureIndex {
            get => apertureIndex;
            set {
                apertureIndex = value;
                RaisePropertyChanged();
            }
        }

        public override async Task Execute(IProgress<ApplicationStatus> progress, CancellationToken token) {
            try {
                focuserMediator.Action("SetApertureIndex", apertureIndex.ToString());
                await Task.Delay(TimeSpan.FromSeconds(2.5));
            } catch (Exception ex) {
                Logger.Error($"{ex.Message}");
                throw new SequenceEntityFailedException(ex.Message);
            }
        }

        private IList<string> issues = new List<string>();

        public IList<string> Issues {
            get => issues;
            set {
                issues = value;
                RaisePropertyChanged();
            }
        }

        public bool Validate() {
            var i = new List<string>();

            if (!focuserMediator.GetInfo().Connected) {
                i.Add(Loc.Instance["LblFocuserNotConnected"]);
            } else if (!DriverAvailable) {
                i.Add("Unsupported focuser driver");
            }

            Issues = i;
            return i.Count == 0;
        }

        public override void AfterParentChanged() {
            Validate();
        }

        public void UpdateDeviceInfo(FocuserInfo focuserInfo) {
            FocuserInfo = focuserInfo;

            if (FocuserInfo.Connected) {
                if (!wantedDrivers.Contains(FocuserInfo.DeviceId)) {
                    if (!seen) Logger.Error($"{FocuserInfo.Name} is not a supported driver");
                    DriverAvailable = false;
                    seen = true;
                    return;
                }

                foreach (var action in wantedActions) {
                    if (!FocuserInfo.SupportedActions.Contains(action)) {
                        if (!seen) Logger.Error($"{FocuserInfo.DeviceId} ({FocuserInfo.DeviceId}) does not contain the required actions");
                        DriverAvailable = false;
                        seen = true;
                        return;
                    }
                }

                var driverVersion = new Version(FocuserInfo.DriverVersion);
                var pluginVersion = new Version(Utility.GetVersion());

                if (driverVersion.Major != pluginVersion.Major) {
                    if (!seen) Logger.Error($"{FocuserInfo.DeviceId} ({FocuserInfo.DriverVersion}) major version mismatch");
                    DriverAvailable = false;
                    seen = true;
                    return;
                } else if (driverVersion.Minor != pluginVersion.Minor) {
                    if (!seen) Logger.Error($"{FocuserInfo.DeviceId} ({FocuserInfo.DriverVersion}) minor version mismatch");
                    DriverAvailable = false;
                    seen = true;
                    return;
                }

                seen = true;

                try {
                    string lensModel = focuserMediator.Action("GetLensModel", string.Empty);

                    if (!string.IsNullOrEmpty(lensModel) && !LensModel.Equals(lensModel)) {
                        LensModel = lensModel;
                        FocalRatios = focuserMediator.Action("GetFocalRatioList", string.Empty).Split(':').ToList();

                        int.TryParse(focuserMediator.Action("GetApertureIndex", string.Empty), out apertureIndex);
                        RaisePropertyChanged("ApertureIndex");
                    }
                } catch (Exception ex) {
                    Logger.Error($"Exception occurred: {ex.Message}");
                }

                DriverAvailable = true;
                Validate();
            } else {
                DriverAvailable = false;
                LensModel = string.Empty;
                focalRatios.Clear();
                RaisePropertyChanged("FocalRatios");
            }
        }

        public void Dispose() {
            this.focuserMediator.RemoveConsumer(this);
        }

        public void UpdateEndAutoFocusRun(AutoFocusInfo info) {
        }

        public void UpdateUserFocused(FocuserInfo info) {
        }

        public override string ToString() {
            return $"Category: {Category}, Item: {nameof(AstromechApertureInstruction)}, ApertureIndex: {apertureIndex} ({LensModel}, {FocalRatios[apertureIndex]})";
        }
    }
}

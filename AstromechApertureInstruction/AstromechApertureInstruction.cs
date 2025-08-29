#region "copyright"

/*
    Copyright 2022-2025 Dale Ghent <daleg@elemental.org>
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/
*/

#endregion "copyright"

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NINA.Core.Locale;
using NINA.Core.Model;
using NINA.Core.Utility;
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
    public class AstromechApertureInstruction : SequenceItem, IValidatable, IDisposable {
        private readonly IFocuserMediator focuserMediator;

        [ImportingConstructor]
        public AstromechApertureInstruction(IFocuserMediator focuserMediator) {
            this.focuserMediator = focuserMediator;

            this.focuserMediator.Connected += OnFocuserConnected;
            this.focuserMediator.Disconnected += OnFocuserDisconnected;

            UpdateDeviceInfo();

            if (DriverAvailable) {
                _ = int.TryParse(focuserMediator.Action("GetApertureIndex", string.Empty), out apertureIndex);
                RaisePropertyChanged(nameof(ApertureIndex));
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

        public void Dispose() {
            focuserMediator.Connected -= OnFocuserConnected;
            focuserMediator.Disconnected -= OnFocuserDisconnected;

            GC.SuppressFinalize(this);
        }

        private bool DriverAvailable { get; set; } = false;

        private List<string> focalRatios = [];

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

        private int apertureIndex = -1;

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
                await Task.Delay(TimeSpan.FromSeconds(2.5), token);
            } catch (Exception ex) {
                Logger.Error($"{ex.Message}");
                throw new SequenceEntityFailedException(ex.Message);
            }
        }

        private IList<string> issues = [];

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

        public Task UpdateDeviceInfo() {
            var focuserInfo = focuserMediator.GetInfo();

            if (focuserInfo.Connected) {
                if (!Utility.WantedDrivers().Contains(focuserInfo.DeviceId)) {
                    Logger.Error($"{focuserInfo.Name} is not a supported driver");
                    DriverAvailable = false;
                    return Task.CompletedTask;
                }

                foreach (var action in Utility.WantedActions()) {
                    if (!focuserInfo.SupportedActions.Contains(action)) {
                        Logger.Error($"{focuserInfo.DeviceId} ({focuserInfo.DeviceId}) does not contain the required actions (wanted: {action}, has: {string.Join(',', focuserInfo.SupportedActions)}");
                        DriverAvailable = false;
                        return Task.CompletedTask;
                    }
                }

                try {
                    string lensModel = focuserMediator.Action("GetLensModel", string.Empty);

                    if (!string.IsNullOrEmpty(lensModel) && !LensModel.Equals(lensModel)) {
                        LensModel = lensModel;
                        FocalRatios = focuserMediator.Action("GetFocalRatioList", string.Empty).Split(':').ToList();

                        _ = int.TryParse(focuserMediator.Action("GetApertureIndex", string.Empty), out int defaultApertureIndex);

                        if (ApertureIndex == -1) {
                            apertureIndex = defaultApertureIndex;
                        }

                        RaisePropertyChanged(nameof(FocalRatios));
                        RaisePropertyChanged(nameof(ApertureIndex));
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
                RaisePropertyChanged(nameof(FocalRatios));
            }

            return Task.CompletedTask;
        }

        private async Task OnFocuserConnected(object arg1, EventArgs args) {
            await UpdateDeviceInfo();
        }

        private async Task OnFocuserDisconnected(object arg1, EventArgs args) {
            await UpdateDeviceInfo();
        }

        public override string ToString() {
            return $"Category: {Category}, Item: {Name}, ApertureIndex: {apertureIndex} ({LensModel}, {FocalRatios[apertureIndex]})";
        }
    }
}

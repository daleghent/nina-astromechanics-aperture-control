#region "copyright"

/*
    Copyright 2022-2025 Dale Ghent <daleg@elemental.org>
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/
*/

#endregion "copyright"

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using NINA.Core.Utility;
using NINA.Equipment.Interfaces.Mediator;
using NINA.Equipment.Interfaces.ViewModel;
using NINA.Profile.Interfaces;
using NINA.WPF.Base.ViewModel;

namespace DaleGhent.NINA.AstromechApertureControl {

    [Export(typeof(IDockableVM))]
    public class AstromechApertureDock : DockableVM {
        private readonly IFocuserMediator focuserMediator;

        [ImportingConstructor]
        public AstromechApertureDock(IProfileService profileService, IFocuserMediator focuserMediator) : base(profileService) {
            Title = "Astromechanics Aperture Control";

            ImageGeometry = (GeometryGroup)System.Windows.Application.Current.Resources["CameraSVG"];
            ImageGeometry.Freeze();

            this.focuserMediator = focuserMediator;

            this.focuserMediator.Connected += OnFocuserConnected;
            this.focuserMediator.Disconnected += OnFocuserDisconnected;
            UpdateDeviceInfo();

            if (DriverAvailable) {
                _ = int.TryParse(focuserMediator.Action("GetApertureIndex", string.Empty), out apertureIndex);
                RaisePropertyChanged(nameof(ApertureIndex));
            }
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

        public int ApertureIndex {
            get => apertureIndex;
            set {
                Logger.Debug($"Setting aperture index to {value} ({lensModel}, {focalRatios[value]})");
                focuserMediator.Action("SetApertureIndex", apertureIndex.ToString());
                apertureIndex = value;
                RaisePropertyChanged();
            }
        }

        public override bool IsTool => true;

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
    }
}

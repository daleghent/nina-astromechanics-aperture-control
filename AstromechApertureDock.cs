#region "copyright"

/*
    Copyright Dale Ghent <daleg@elemental.org>
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/
*/

#endregion "copyright"

using NINA.Core.Utility;
using NINA.Equipment.Equipment.MyFocuser;
using NINA.Equipment.Interfaces.Mediator;
using NINA.Equipment.Interfaces.ViewModel;
using NINA.Profile.Interfaces;
using NINA.WPF.Base.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Media;

namespace DaleGhent.NINA.AstromechApertureControl {

    [Export(typeof(IDockableVM))]
    public class AstromechApertureDock : DockableVM, IFocuserConsumer {
        private readonly IFocuserMediator focuserMediator;
        private readonly List<string> wantedDrivers;
        private readonly List<string> wantedActions;
        private bool seen = true;

        [ImportingConstructor]
        public AstromechApertureDock(IProfileService profileService, IFocuserMediator focuserMediator) : base(profileService) {
            Title = "Astromechanics Aperture Control";

            ImageGeometry = (GeometryGroup)System.Windows.Application.Current.Resources["CameraSVG"];
            ImageGeometry.Freeze();

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
    }
}

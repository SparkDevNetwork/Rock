// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

using System.Collections.Generic;

using Rock.ViewModels.CheckIn;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.CheckIn.MobileCheckInLauncher
{
    /// <summary>
    /// The options available to the custom settings panel for the Mobile Check-in Launcher block.
    /// </summary>
    public class CustomSettingsOptionsBag
    {
        /// <summary>
        /// Gets or sets the check-in areas available for the selected devices.
        /// </summary>
        public List<ListItemBag> AreaItems { get; set; }

        /// <summary>
        /// Gets or sets the check-in configuration templates available for selection.
        /// </summary>
        public List<ConfigurationTemplateBag> CheckInConfigurationOptions { get; set; }

        /// <summary>
        /// Gets or sets the active check-in kiosk devices available for selection.
        /// </summary>
        public List<ListItemBag> DeviceItems { get; set; }

        /// <summary>
        /// Gets or sets the check-in themes available for selection. Only themes whose purpose is check-in are
        /// offered, since any other theme lacks the layout this page is bound to.
        /// </summary>
        public List<ListItemBag> ThemeItems { get; set; }
    }
}

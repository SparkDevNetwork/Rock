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

namespace Rock.ViewModels.Blocks.CheckIn.MobileCheckInLauncher
{
    /// <summary>
    /// The settings that can be edited in the custom settings panel for the Mobile Check-in Launcher block.
    /// </summary>
    public class CustomSettingsBag
    {
        /// <summary>
        /// Gets or sets the hashed identifiers of the check-in area group types offered during check-in.
        /// </summary>
        public List<string> CheckInAreas { get; set; }

        /// <summary>
        /// Gets or sets the hashed identifier of the check-in configuration template group type.
        /// </summary>
        public string CheckInConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the hashed identifiers of the kiosk devices this block can check in through.
        /// </summary>
        public List<string> Devices { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the device's location services are bypassed, in which case a
        /// campus is selected from a list instead.
        /// </summary>
        public bool IsLocationServicesDisabled { get; set; }

        /// <summary>
        /// Gets or sets the name of the check-in theme this page renders in, which is also its folder name. An empty
        /// value follows the theme configured on the site.
        /// </summary>
        public string Theme { get; set; }
    }
}

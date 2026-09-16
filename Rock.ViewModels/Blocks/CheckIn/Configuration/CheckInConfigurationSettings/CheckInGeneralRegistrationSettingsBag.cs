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

using Rock.Enums.CheckIn;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.CheckIn.Configuration.CheckInConfigurationSettings
{
    /// <summary>
    /// The general registration settings for a check-in configuration. Configures general features and
    /// default settings applied when new individuals are added during check-in registration.
    /// </summary>
    public class CheckInGeneralRegistrationSettingsBag
    {
        /// <summary>
        /// Gets or sets the default connection status applied to a person created during check-in
        /// registration.
        /// </summary>
        public ListItemBag DefaultPersonConnectionStatus { get; set; }

        /// <summary>
        /// Gets or sets the default record source applied to a person created during check-in registration.
        /// </summary>
        public ListItemBag DefaultPersonRecordSource { get; set; }

        /// <summary>
        /// Gets or sets whether the family continues along the check-in path after registration. When
        /// disabled, the family is returned to the search step (useful when registration is handled at a
        /// different kiosk than check-in).
        /// </summary>
        public bool EnableCheckInAfterRegistration { get; set; }

        /// <summary>
        /// Gets or sets whether an "SMS Enabled" option is shown alongside the phone number field during
        /// registration.
        /// </summary>
        public bool DisplaySmsEnabled { get; set; }

        /// <summary>
        /// Gets or sets whether the "SMS Enabled" option is checked by default on the phone number field
        /// during registration.
        /// </summary>
        public bool SmsEnabledByDefault { get; set; }

        /// <summary>
        /// Gets or sets for which people (adults, children, both, or neither) the name suffix field is
        /// displayed when adding a new individual.
        /// </summary>
        public AdultsOrChildrenSelectionMode DisplaySuffix { get; set; }
    }
}

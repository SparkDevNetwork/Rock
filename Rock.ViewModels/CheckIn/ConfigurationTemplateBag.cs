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

namespace Rock.ViewModels.CheckIn
{
    /// <summary>
    /// The summary information about a single check-in configuration template.
    /// </summary>
    public class ConfigurationTemplateBag
    {
        /// <summary>
        /// Gets or sets the identifier of this check-in configuration
        /// template.
        /// </summary>
        /// <value>The identifier.</value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the name of this check-in configuration.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class defined on the check-in configuration.
        /// </summary>
        /// <value>The icon CSS class.</value>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets a value that determines how check-in should gather the
        /// ability level of the current individual.
        /// </summary>
        /// <value>How check-in should gather the ability level.</value>
        public AbilityLevelDeterminationMode AbilityLevelDetermination { get; set; }

        /// <summary>
        /// Gets the type of check-in experience to use. Family check-in allows
        /// more than one person in the family to be checked in at a time.
        /// </summary>
        /// <value>The type of check-in experience to use.</value>
        public KioskCheckInMode KioskCheckInType { get; set; }

        /// <summary>
        /// Gets or sets the type of the family search configured for
        /// the configuration.
        /// </summary>
        /// <value>The type of the family search.</value>
        public FamilySearchMode FamilySearchType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether auto-select mode is enabled
        /// for this template. In auto-select mode all the selected information
        /// is displayed on the person select screen with an optional button to
        /// change those selections.
        /// </summary>
        /// <value><c>true</c> if this auto-select mode is enabled; otherwise, <c>false</c>.</value>
        public bool IsAutoSelect { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to allow self-checkout on
        /// public kiosks for this configuration. When enabled, if an individual
        /// in the family is already checked in then the kiosk will prompt if
        /// they want to check-in another family member of checkout existing
        /// individuals.
        /// </summary>
        /// <value><c>true</c> if self check-out mode is enabled; otherwise, <c>false</c>.</value>
        public bool IsCheckoutAtKioskAllowed { get; set; }

        /// <summary>
        /// Gets a value indicating whether the current location occupancy
        /// counts should be displayed when selecting a location.
        /// </summary>
        /// <value><c>true</c> if current location occupancy counts should be displayed; otherwise, <c>false</c>.</value>
        public bool IsLocationCountDisplayed { get; set; }

        /// <summary>
        /// Gets a value indicating whether an override option is available in
        /// the kiosk supervisor screen. This allows an authorized support
        /// person to bypass check-in requirements.
        /// </summary>
        /// <value><c>true</c> if the kiosk override option is available; otherwise, <c>false</c>.</value>
        public bool IsOverrideAvailable { get; set; }

        /// <summary>
        /// Gets a value indicating whether individual photos should be hidden
        /// on public kiosks.
        /// </summary>
        /// <value><c>true</c> if photos should be hidden; otherwise, <c>false</c>.</value>
        public bool IsPhotoHidden { get; set; }

        /// <summary>
        /// Gets a value indicating whether removing people with a "can check-in"
        /// relationship from the family is allowed. This does not allow
        /// full family members to be removed.
        /// </summary>
        /// <value><c>true</c> if can check-in relationship can be removed; otherwise, <c>false</c>.</value>
        public bool IsRemoveFromFamilyAtKioskAllowed { get; set; }

        /// <summary>
        /// Gets a value indicating whether the supervisor screen is available
        /// to kiosks after entering a pin number.
        /// </summary>
        /// <value><c>true</c> if the supervisor screen is available; otherwise, <c>false</c>.</value>
        public bool IsSupervisorEnabled { get; set; }

        /// <summary>
        /// Gets a value indicating whether to attempt to use the same options
        /// from the first service when a person is checking into more than one
        /// service schedule.
        /// </summary>
        /// <value><c>true</c> if the same options from the first service will be used; otherwise, <c>false</c>.</value>
        public virtual bool IsSameOptionUsed { get; set; }

        /// <summary>
        /// Gets or sets the maximum length of the phone number during
        /// family search.
        /// </summary>
        /// <value>The maximum length of the phone number.</value>
        public int? MaximumPhoneNumberLength { get; set; }

        /// <summary>
        /// Gets or sets the minimum length of the phone number during
        /// family search.
        /// </summary>
        /// <value>The minimum length of the phone number.</value>
        public int? MinimumPhoneNumberLength { get; set; }

        /// <summary>
        /// Gets or sets the type of the phone search used in family search.
        /// </summary>
        /// <value>The type of the phone search used in family search.</value>
        public PhoneSearchMode PhoneSearchType { get; set; }
    }
}

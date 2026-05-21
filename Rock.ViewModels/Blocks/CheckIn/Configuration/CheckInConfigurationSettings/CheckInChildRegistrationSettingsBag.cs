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

using Rock.Enums.Controls;

namespace Rock.ViewModels.Blocks.CheckIn.Configuration.CheckInConfigurationSettings
{
    /// <summary>
    /// The child registration settings for a check-in configuration. Configures the types of information to
    /// collect from families and children during check-in registration.
    /// </summary>
    public class CheckInChildRegistrationSettingsBag
    {
        /// <summary>
        /// Gets or sets the person attributes that are required to be filled in when registering a child.
        /// </summary>
        public List<string> RequiredAttributesForChildren { get; set; }

        /// <summary>
        /// Gets or sets the person attributes that are optionally displayed when registering a child.
        /// </summary>
        public List<string> OptionalAttributesForChildren { get; set; }

        /// <summary>
        /// Gets or sets how the birthdate field is displayed for children (Hide, Optional, or Required).
        /// </summary>
        public string DisplayBirthdateForChildren { get; set; }

        /// <summary>
        /// Gets or sets how the grade field is displayed for children (Hide, Optional, or Required).
        /// </summary>
        public string DisplayGradeForChildren { get; set; }

        /// <summary>
        /// Gets or sets how the mobile phone field is displayed for children (Hide, Optional, or Required).
        /// </summary>
        public RequirementLevel DisplayMobilePhoneForChildren { get; set; }

        /// <summary>
        /// Gets or sets how the race field is displayed for children (Hide, Optional, or Required).
        /// </summary>
        public string DisplayRaceForChildren { get; set; }

        /// <summary>
        /// Gets or sets how the ethnicity field is displayed for children (Hide, Optional, or Required).
        /// </summary>
        public string DisplayEthnicityForChildren { get; set; }

        /// <summary>
        /// Gets or sets whether the Alternate ID field is displayed when registering a child.
        /// </summary>
        public bool DisplayAlternateIdForChildren { get; set; }

        /// <summary>
        /// Gets or sets whether a known relationship type (e.g., child, grandchild, can-check-in) must be
        /// selected when registering a child.
        /// </summary>
        public bool RequireRelationshipTypeSelectionForChildren { get; set; }

        /// <summary>
        /// Gets or sets the minimum age at which a child is prompted to confirm their grade during
        /// registration.
        /// </summary>
        public decimal? GradeConfirmationAge { get; set; }
    }
}

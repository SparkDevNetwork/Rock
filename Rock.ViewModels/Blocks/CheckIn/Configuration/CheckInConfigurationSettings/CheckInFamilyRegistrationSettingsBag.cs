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
    /// The family registration settings for a check-in configuration. Configures the types of information
    /// to collect from families during registration.
    /// </summary>
    public class CheckInFamilyRegistrationSettingsBag
    {
        /// <summary>
        /// Gets or sets how the address field is displayed for families (Hide, Optional, or Required).
        /// </summary>
        public RequirementLevel DisplayAddressForFamilies { get; set; }

        /// <summary>
        /// Gets or sets the person attributes that are required to be filled in when registering a family.
        /// </summary>
        public List<string> RequiredAttributesForFamilies { get; set; }

        /// <summary>
        /// Gets or sets the person attributes that are optionally displayed when registering a family.
        /// </summary>
        public List<string> OptionalAttributesForFamilies { get; set; }
    }
}

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

namespace Rock.ViewModels.Blocks.CheckIn.Configuration.CheckInConfigurationSettings
{
    /// <summary>
    /// The adult registration settings for a check-in configuration. Configures the types of information to
    /// collect from adults during check-in registration.
    /// </summary>
    public class CheckInAdultRegistrationSettingsBag
    {
        /// <summary>
        /// Gets or sets the person attributes that are required to be filled in when registering an adult.
        /// </summary>
        public List<string> RequiredAttributesForAdults { get; set; }

        /// <summary>
        /// Gets or sets the person attributes that are optionally displayed when registering an adult.
        /// </summary>
        public List<string> OptionalAttributesForAdults { get; set; }

        /// <summary>
        /// Gets or sets how the birthdate field is displayed for adults (Hide, Optional, or Required).
        /// </summary>
        public string DisplayBirthdateForAdults { get; set; }

        /// <summary>
        /// Gets or sets how the race field is displayed for adults (Hide, Optional, or Required).
        /// </summary>
        public string DisplayRaceForAdults { get; set; }

        /// <summary>
        /// Gets or sets how the ethnicity field is displayed for adults (Hide, Optional, or Required).
        /// </summary>
        public string DisplayEthnicityForAdults { get; set; }

        /// <summary>
        /// Gets or sets whether the Alternate ID field is displayed when registering an adult.
        /// </summary>
        public bool DisplayAlternateIdForAdults { get; set; }
    }
}

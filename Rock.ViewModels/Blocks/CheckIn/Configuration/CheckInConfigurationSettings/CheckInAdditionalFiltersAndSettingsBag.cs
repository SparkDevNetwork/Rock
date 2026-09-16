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

namespace Rock.ViewModels.Blocks.CheckIn.Configuration.CheckInConfigurationSettings
{
    /// <summary>
    /// The additional filters and settings for a check-in configuration. Configures requirements like age,
    /// grade, or ability level, enables proximity check-in, and filters who can check in.
    /// </summary>
    public class CheckInAdditionalFiltersAndSettingsBag
    {
        /// <summary>
        /// Gets or sets how and when the individual is asked for an ability level during check-in.
        /// </summary>
        public AbilityLevelDeterminationMode? AbilityLevelDetermination { get; set; }

        /// <summary>
        /// Gets or sets how grade and age ranges are matched against group criteria when determining the
        /// groups an individual is eligible to check into.
        /// </summary>
        public GradeAndAgeMatchingMode? GradeAndAgeMatchingBehavior { get; set; }

        /// <summary>
        /// Gets or sets whether adults, children, or all ages are shown on the family member selection
        /// screen.
        /// </summary>
        public AgeRestrictionMode? AgeRestriction { get; set; }

        /// <summary>
        /// Gets or sets whether this configuration (and all of its areas and groups) is available for
        /// proximity check-in via the Rock Mobile app.
        /// </summary>
        public bool EnableProximityCheckIn { get; set; }

        /// <summary>
        /// Gets or sets the Lava template used for the notification shown when an individual is detected
        /// via proximity check-in.
        /// </summary>
        public string ProximityAttendanceNotificationTemplate { get; set; }

        /// <summary>
        /// Gets or sets whether people with an inactive record status are excluded from check-in.
        /// </summary>
        public bool PreventInactivePeople { get; set; }

        /// <summary>
        /// Gets or sets whether an age value is required for an individual to be considered for check-in.
        /// </summary>
        public bool AgeRequired { get; set; }

        /// <summary>
        /// Gets or sets whether a grade value is required for an individual to be considered for check-in.
        /// </summary>
        public bool GradeRequired { get; set; }
    }
}

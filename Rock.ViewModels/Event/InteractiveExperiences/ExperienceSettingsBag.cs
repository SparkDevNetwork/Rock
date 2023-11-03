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

using Rock.Enums.Event;

namespace Rock.ViewModels.Event.InteractiveExperiences
{
    /// <summary>
    /// Contents of the ExperienceSettingsJson property on an InteractiveExperience.
    /// </summary>
    public class ExperienceSettingsBag
    {
        /// <summary>
        /// The behavior of campus choices for this experience. See the description
        /// of the individual enum values for specific functionality.
        /// </summary>
        public InteractiveExperienceCampusBehavior CampusBehavior { get; set; }

        /// <summary>
        /// The default campus to use when recording an Interaction if no other
        /// campus could be determined.
        /// </summary>
        public int? DefaultCampusId { get; set; }

        /// <summary>
        /// The lava template to use when the experience has ended.
        /// </summary>
        public string ExperienceEndedTemplate { get; set; }
    }
}

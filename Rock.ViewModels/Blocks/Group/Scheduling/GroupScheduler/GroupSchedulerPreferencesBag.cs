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
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Group.Scheduling.GroupScheduler
{
    /// <summary>
    /// The information describing a group member's existing scheduling preferences for the group scheduler.
    /// </summary>
    public class GroupSchedulerPreferencesBag
    {
        /// <summary>
        /// Gets or sets the existing preference that matches the current group and schedule, if any.
        /// </summary>
        /// <value>
        /// The existing preference that matches the current group and schedule, if any.
        /// </value>
        public GroupSchedulerPreferenceBag SchedulePreference { get; set; }

        /// <summary>
        /// Gets or sets friendly descriptions of existing preferences that match the current group, but not the current schedule, if any.
        /// </summary>
        /// <value>
        /// Friendly descriptions of existing preferences that match the current group, but not the current schedule, if any.
        /// </value>
        public List<string> OtherPreferencesForGroup { get; set; }

        /// <summary>
        /// Gets or sets the available schedule templates.
        /// </summary>
        /// <value>
        /// The available schedule templates.
        /// </value>
        public List<ListItemBag> AvailableScheduleTemplates { get; set; }

        /// <summary>
        /// Gets or sets the warning message, if any.
        /// </summary>
        /// <value>
        /// The warning message, if any.
        /// </value>
        public string WarningMessage { get; set; }

        /// <summary>
        /// Gets or sets the error message, if any.
        /// </summary>
        /// <value>
        /// The error message, if any.
        /// </value>
        public string ErrorMessage { get; set; }
    }
}

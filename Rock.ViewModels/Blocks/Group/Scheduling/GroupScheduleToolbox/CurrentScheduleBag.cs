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

using Rock.ViewModels.Utility;

using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Group.Scheduling.GroupScheduleToolbox
{
    /// <summary>
    /// A bag that contains information about the selected person's current schedule for the group schedule toolbox block.
    /// </summary>
    public class CurrentScheduleBag
    {
        /// <summary>
        /// Gets or sets the selected person's current schedule rows, representing a combination of attendance
        /// and person schedule exclusion records.
        /// </summary>
        public List<ScheduleRowBag> ScheduleRows { get; set; }

        /// <summary>
        /// Gets or sets the selected person's group schedule feed URL.
        /// </summary>
        public string PersonGroupScheduleFeedUrl { get; set; }

        /// <summary>
        /// Gets or sets the selected person's family members, regardless of whether they're schedulable.
        /// Note that the selected person themself will always be the first person in this list.
        /// </summary>
        public List<ListItemBag> FamilyMembers { get; set; }

        /// <summary>
        /// Gets or sets the selected person's schedulable groups for which unavailability may be added.
        /// </summary>
        public List<GroupBag> SchedulableGroups { get; set; }
    }
}

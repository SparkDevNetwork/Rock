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

namespace Rock.ViewModels.Blocks.Group.GroupMemberDetail
{
    /// <summary>
    /// The response returned for the Group Member Detail block's Assignment
    /// Preference modal.
    /// </summary>
    public class ScheduleAssignmentOptionsBag
    {
        /// <summary>
        /// Gets or sets the schedules available for assignment, carrying the
        /// formatted name and sort metadata the client grid needs. Schedules
        /// already assigned are filtered client-side, where that state lives.
        /// </summary>
        public List<GroupScheduleAssignmentBag> Schedules { get; set; }

        /// <summary>
        /// Gets or sets the locations available for the selected schedule.
        /// </summary>
        public List<ListItemBag> LocationItems { get; set; }
    }
}

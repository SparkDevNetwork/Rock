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

using System;
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Group.Scheduling.GroupScheduleToolbox
{
    /// <summary>
    /// A bag that contains schedule preference assignment options for the group schedule toolbox block.
    /// </summary>
    public class AssignmentOptionsBag
    {
        /// <summary>
        /// Gets or sets the schedule options.
        /// </summary>
        public List<ListItemBag> Schedules { get; set; }

        /// <summary>
        /// Gets or sets the location options, grouped by schedule.
        /// </summary>
        public Dictionary<string, List<ListItemBag>> LocationsBySchedule { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of an existing assignment that is being edited.
        /// If this value is null, the request represents an attempt to add a new assignment.
        /// </summary>
        public Guid? EditAssignmentGuid { get; set; }

        /// <summary>
        /// Gets or sets the existing assignment's selected schedule unique identifier, if any.
        /// </summary>
        public Guid? SelectedScheduleGuid { get; set; }

        /// <summary>
        /// Gets or sets the existing assignment's selected location unique identifier, if any.
        /// </summary>
        public Guid? SelectedLocationGuid { get; set; }

        /// <summary>
        /// Gets or sets the current list of assignments.
        /// </summary>
        public List<SchedulePreferenceAssignmentBag> Assignments { get; set; }
    }
}

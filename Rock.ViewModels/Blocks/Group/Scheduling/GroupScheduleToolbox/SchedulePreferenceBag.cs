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
    /// A bag that contains information about a schedule preference for the group schedule toolbox block.
    /// </summary>
    public class SchedulePreferenceBag
    {
        /// <summary>
        /// Gets or sets the schedule reminder email offset days.
        /// </summary>
        public string ScheduleReminderEmailOffsetDays { get; set; }

        /// <summary>
        /// Gets or sets schedule templates that may be selected for a schedule preference.
        /// </summary>
        public List<ListItemBag> ScheduleTemplates { get; set; }

        /// <summary>
        /// Gets or sets the selected schedule template unique identifier.
        /// </summary>
        public Guid? SelectedScheduleTemplateGuid { get; set; }

        /// <summary>
        /// Gets or sets the schedule start date.
        /// </summary>
        public DateTimeOffset? ScheduleStartDate { get; set; }

        /// <summary>
        /// Gets or sets the assignments.
        /// </summary>
        public List<SchedulePreferenceAssignmentBag> Assignments { get; set; }
    }
}

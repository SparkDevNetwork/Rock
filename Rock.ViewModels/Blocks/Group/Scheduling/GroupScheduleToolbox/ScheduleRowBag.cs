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

using Rock.Enums.Blocks.Group.Scheduling;

using System;

namespace Rock.ViewModels.Blocks.Group.Scheduling.GroupScheduleToolbox
{
    /// <summary>
    /// A bag that contains information about a current attendance or person schedule exclusion for the group schedule toolbox block.
    /// </summary>
    public class ScheduleRowBag
    {
        /// <summary>
        /// Gets or sets the attendance or person schedule exclusion unique identifier.
        /// <para>
        /// If confirmation status is "Unavailable," this value represents a person schedule exclusion entity.
        /// Otherwise, this value represents an attendance entity.
        /// </para>
        /// </summary>
        public Guid EntityGuid { get; set; }

        /// <summary>
        /// Gets or sets the name of the family member to whom this row belongs, if it doesn't belong to the current person.
        /// </summary>
        public string FamilyMemberName { get; set; }

        /// <summary>
        /// Gets or sets the name of the group.
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Gets or sets the name of the location.
        /// </summary>
        public string LocationName { get; set; }

        /// <summary>
        /// Gets or sets the occurrence start date.
        /// </summary>
        public DateTimeOffset OccurrenceStartDate { get; set; }

        /// <summary>
        /// Gets or sets the occurrence end date.
        /// </summary>
        public DateTimeOffset? OccurrenceEndDate { get; set; }

        /// <summary>
        /// Gets or sets the friendly schedule name.
        /// </summary>
        public string ScheduleName { get; set; }

        /// <summary>
        /// Gets or sets the confirmation status.
        /// <para>
        /// If "Unavailable,", this value represents a person schedule exclusion entity.
        /// Otherwise, this value represents an attendance entity.
        /// </para>
        /// </summary>
        public ToolboxScheduleRowConfirmationStatus ConfirmationStatus { get; set; }
    }
}

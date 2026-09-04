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

using System;

namespace Rock.ViewModels.Blocks.Group.GroupMemberDetail
{
    /// <summary>
    /// One schedule and location assignment preference row in the Group
    /// Member Detail block. Rows are edited client-side and reconciled
    /// against GroupMemberAssignment records on save.
    /// </summary>
    public class GroupScheduleAssignmentBag
    {
        /// <summary>
        /// Gets or sets the assignment's unique identifier. New rows get a
        /// client-generated guid.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the schedule identifier.
        /// </summary>
        public int ScheduleId { get; set; }

        /// <summary>
        /// Gets or sets the location identifier. Null means no location
        /// preference.
        /// </summary>
        public int? LocationId { get; set; }

        /// <summary>
        /// Gets or sets the schedule display text formatted per the
        /// Schedule List Format block setting.
        /// </summary>
        public string FormattedScheduleName { get; set; }

        /// <summary>
        /// Gets or sets the location's name, or the no-preference text when
        /// no location is selected.
        /// </summary>
        public string LocationName { get; set; }

        /// <summary>
        /// Gets or sets the schedule's sort order, used for client-side row
        /// ordering.
        /// </summary>
        public int ScheduleOrder { get; set; }

        /// <summary>
        /// Gets or sets the schedule's next start date and time, used as
        /// the secondary client-side sort.
        /// </summary>
        public DateTimeOffset? ScheduleNextStartDateTime { get; set; }
    }
}

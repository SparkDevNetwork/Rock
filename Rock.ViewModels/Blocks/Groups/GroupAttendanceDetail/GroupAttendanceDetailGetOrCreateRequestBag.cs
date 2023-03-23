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
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Groups.GroupAttendanceDetail
{
    /// <summary>
    /// A bag that contains the get or create request information.
    /// </summary>
    public class GroupAttendanceDetailGetOrCreateRequestBag
    {
        /// <summary>
        /// Gets or sets the attendance occurrence date (time ignored).
        /// </summary>
        public DateTimeOffset? AttendanceOccurrenceDate { get; set; }

        /// <summary>
        /// Gets or sets the attendance occurrence unique identifier.
        /// </summary>
        public Guid? AttendanceOccurrenceGuid { get; set; }

        /// <summary>
        /// Gets or sets the location unique identifier.
        /// </summary>
        public Guid? LocationGuid { get; set; }

        /// <summary>
        /// Gets or sets the schedule unique identifier.
        /// </summary>
        public Guid? ScheduleGuid { get; set; }

        /// <summary>
        /// Gets or sets the notes.
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the attendance occurrence occurred.
        /// </summary>
        public bool DidNotOccur { get; set; }

        /// <summary>
        /// Gets or sets the attendance type unique identifier.
        /// </summary>
        public Guid? AttendanceTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the campus unique identifier.
        /// </summary>
        public Guid? CampusGuid { get; set; }

        /// <summary>
        /// Gets or sets the attendees.
        /// </summary>
        public List<GroupAttendanceDetailAttendanceBag> Attendees { get; set; }
    }
}

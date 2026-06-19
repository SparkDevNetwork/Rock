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

namespace Rock.ViewModels.Blocks.CheckIn.RapidAttendanceEntry
{
    /// <summary>
    /// The validated attendance session the entry screen operates under: the group, location, schedule, and date
    /// attendance is recorded against.
    /// </summary>
    public class RapidAttendanceEntrySessionBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the session's group.
        /// </summary>
        public Guid GroupGuid { get; set; }

        /// <summary>
        /// Gets or sets the name of the session's group.
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the session's location.
        /// </summary>
        public Guid LocationGuid { get; set; }

        /// <summary>
        /// Gets or sets the name of the session's location.
        /// </summary>
        public string LocationName { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the session's schedule.
        /// </summary>
        public Guid ScheduleGuid { get; set; }

        /// <summary>
        /// Gets or sets the name of the session's schedule.
        /// </summary>
        public string ScheduleName { get; set; }

        /// <summary>
        /// Gets or sets the session's attendance date.
        /// </summary>
        public DateTime AttendanceDate { get; set; }

        /// <summary>
        /// Gets or sets the number of people attended for the session's occurrence.
        /// </summary>
        public int AttendanceCount { get; set; }

        /// <summary>
        /// Gets or sets the URL of the Attendance List page for the session's occurrence. Null when no page is
        /// configured.
        /// </summary>
        public string AttendanceListUrl { get; set; }
    }
}

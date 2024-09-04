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

namespace Rock.CheckIn.v2
{
    /// <summary>
    /// A representation of a recent attendance record for a person.
    /// </summary>
    internal class RecentAttendance
    {
        /// <summary>
        /// Gets or sets the Attendance unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.</value>
        public Guid AttendanceGuid { get; set; }

        /// <summary>
        /// Gets or sets the Attendance identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public string AttendanceId { get; set; }

        /// <summary>
        /// Gets or sets the status of the attendance record.
        /// </summary>
        /// <value>The status of the attendance record.</value>
        public Rock.Enums.Event.CheckInStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the start date time.
        /// </summary>
        /// <value>
        /// The start date time.
        /// </value>
        public DateTime StartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the end date time.
        /// </summary>
        /// <value>
        /// The end date time.
        /// </value>
        public DateTime? EndDateTime { get; set; }

        /// <summary>
        /// Gets or sets the person identifier.
        /// </summary>
        /// <value>
        /// The person identifier.
        /// </value>
        public string PersonId { get; set; }

        /// <summary>
        /// Gets or sets the group type identifier.
        /// </summary>
        /// <value>
        /// The group type identifier.
        /// </value>
        public string GroupTypeId { get; set; }

        /// <summary>
        /// Gets or sets the group identifier.
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        public string GroupId { get; set; }

        /// <summary>
        /// Gets or sets the location identifier.
        /// </summary>
        /// <value>
        /// The location identifier.
        /// </value>
        public string LocationId { get; set; }

        /// <summary>
        /// Gets or sets the schedule identifier.
        /// </summary>
        /// <value>
        /// The schedule identifier.
        /// </value>
        public string ScheduleId { get; set; }

        /// <summary>
        /// Gets or sets the campus identifier.
        /// </summary>
        /// <value>The campus identifier.</value>
        public string CampusId { get; set; }
    }
}

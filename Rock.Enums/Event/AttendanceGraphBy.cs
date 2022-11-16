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

namespace Rock.Model
{
    /// <summary>
    /// For Attendance Reporting, graph into series partitioned by Total, Group, Campus, or Schedule
    /// </summary>
    [Enums.EnumDomain( "Event" )]
    public enum AttendanceGraphBy
    {
        /// <summary>
        /// Total (one series)
        /// </summary>
        Total = 0,

        /// <summary>
        /// Each selected Check-in Group (which is actually a [Group] under the covers) is a series
        /// </summary>
        Group = 1,

        /// <summary>
        /// Each campus (from Attendance.CampusId) is its own series
        /// </summary>
        Campus = 2,

        /// <summary>
        /// Each schedule (from Attendance.ScheduleId) is its own series
        /// </summary>
        Schedule = 3,

        /// <summary>
        /// Each Location (from Attendance.LocationId) is its own series
        /// </summary>
        Location = 4
    }
}

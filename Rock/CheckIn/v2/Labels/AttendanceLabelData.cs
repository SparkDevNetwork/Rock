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

using Rock.Model;
using Rock.Web.Cache;

namespace Rock.CheckIn.v2.Labels
{
    /// <summary>
    /// <para>
    /// Attendance labels are printed for every single attendance record.
    /// </para>
    /// <para>
    /// Meaning if Noah is checked in to the Bunnies room at the 9am service
    /// and also the Bunnies room at the 11am service, he will get two
    /// attendance labels.
    /// </para>
    /// </summary>
    internal class AttendanceLabelData
    {
        /// <summary>
        /// The data that describes the attendance record this label is being
        /// printed for.
        /// </summary>
        public AttendanceLabel Attendance { get; set; }

        /// <summary>
        /// The person that was checked in that this label is being printed for.
        /// </summary>
        public Person Person => Attendance.Person;

        /// <summary>
        /// The location that represents the room this attendance label is being
        /// printed for.
        /// </summary>
        public NamedLocationCache Location => Attendance.Location;

        /// <summary>
        /// The attendance records for the <see cref="Person"/> during this
        /// check-in session. This includes the <see cref="Attendance"/> record.
        /// </summary>
        public List<AttendanceLabel> PersonAttendance { get; set; }

        /// <summary>
        /// All attendance records for this session, including those from
        /// other people being checked in as well as those from
        /// <see cref="PersonAttendance"/>.
        /// </summary>
        public List<AttendanceLabel> AllAttendance { get; set; }

        /// <summary>
        /// The family object that was determined via either kiosk search
        /// or <see cref="Person.PrimaryFamily"/> if checking in a single
        /// person by API call.
        /// </summary>
        public Group Family { get; set; }
    }
}

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

using Rock.Model;
using Rock.Web.Cache;

namespace Rock.CheckIn.v2.Labels
{
    /// <summary>
    /// <para>
    /// A check-out label is printed for every attendance record during the
    /// check-out process.
    /// </para>
    /// <para>
    /// Meaning if Noah is checked in to the 9am service and the 11am service
    /// and is checked out from both at the same time, he will get two check-out
    /// labels.
    /// </para>
    /// </summary>
    internal class CheckOutLabelData
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
        /// The family object that was determined via either kiosk search
        /// or <see cref="Person.PrimaryFamily"/> if checking in a single
        /// person by API call.
        /// </summary>
        public Group Family { get; set; }
    }
}

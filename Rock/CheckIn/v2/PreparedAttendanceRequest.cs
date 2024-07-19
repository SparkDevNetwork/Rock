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

using Rock.Enums.CheckIn;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.CheckIn.v2
{
    /// <summary>
    /// This contains all the information about an attendance record that
    /// should be created. As much data is provided as possible so that
    /// information can be loaded in fewer queries when processing multiple
    /// attendance requests at once.
    /// </summary>
    internal class PreparedAttendanceRequest
    {
        /// <summary>
        /// Gets or sets the check-in session.
        /// </summary>
        /// <value>The check-in session.</value>
        public AttendanceCheckInSession Session { get; set; }

        /// <summary>
        /// Gets or sets the attendance code for this attendance record.
        /// </summary>
        /// <value>The attendance code for this attendance record.</value>
        public AttendanceCode AttendanceCode { get; set; }

        /// <summary>
        /// Gets or sets the person to be checked in.
        /// </summary>
        /// <value>The person to be checked in.</value>
        public Person Person { get; set; }

        /// <summary>
        /// Gets or sets the ability level selected for this attendee.
        /// </summary>
        /// <value>The ability level or <c>null</c>.</value>
        public DefinedValueCache AbilityLevel { get; set; }

        /// <summary>
        /// Gets or sets the area to be used for this attendance record.
        /// </summary>
        /// <value>The area to be used for this attendance record.</value>
        public GroupTypeCache Area { get; set; }

        /// <summary>
        /// Gets or sets the group to be used for this attendance record.
        /// </summary>
        /// <value>The group to be used for this attendance record.</value>
        public GroupCache Group { get; set; }

        /// <summary>
        /// Gets or sets the location to be used for this attendance record.
        /// </summary>
        /// <value>The location to be used for this attendance record.</value>
        public NamedLocationCache Location { get; set; }

        /// <summary>
        /// Gets or sets the schedule to be used for this attendance record.
        /// </summary>
        /// <value>The schedule to be used for this attendance record.</value>
        public NamedScheduleCache Schedule { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this record is pending.
        /// </summary>
        /// <value><c>true</c> if this record is pending; otherwise, <c>false</c>.</value>
        public bool IsPending { get; set; }

        /// <summary>
        /// Gets or sets the start date time for the attendance record.
        /// </summary>
        /// <value>The start date time for the attendance record.</value>
        public DateTime StartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the family selected during check-in.
        /// </summary>
        /// <value>The identifier of the family selected during check-in.</value>
        public int? FamilyId { get; set; }

        /// <summary>
        /// Gets or sets the kiosk device used for this check-in request.
        /// </summary>
        /// <value>The kiosk device or <c>null</c>.</value>
        public DeviceCache Kiosk { get; set; }

        /// <summary>
        /// Gets or sets the client IP address.
        /// </summary>
        /// <value>The client IP address.</value>
        public string ClientIpAddress { get; set; }

        /// <summary>
        /// Gets or sets the person alias identifier to use as the person
        /// checking in this attendee.
        /// </summary>
        /// <value>The person alias identifier.</value>
        public int? CheckedInByPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the search mode used when searching for families.
        /// </summary>
        /// <value>The search mode used when searching for families.</value>
        public FamilySearchMode SearchMode { get; set; }

        /// <summary>
        /// Gets or sets the search term used to find the family of this attendee.
        /// </summary>
        /// <value>The search term used to find the family of this attendee.</value>
        public string SearchTerm { get; set; }

        /// <summary>
        /// Gets or sets the note to put on the attendance record. If an existing
        /// attendance record is updated then this will replace the note value.
        /// </summary>
        /// <value>The note text.</value>
        public string Note { get; set; }
    }
}

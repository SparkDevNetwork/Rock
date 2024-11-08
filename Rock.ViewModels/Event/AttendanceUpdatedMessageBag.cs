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

using Rock.Enums.Event;
using Rock.Model;

namespace Rock.ViewModels.Event
{
    /// <summary>
    /// Details about an attendance record that is transmitted over
    /// the RealTime engine.
    /// </summary>
    public class AttendanceUpdatedMessageBag
    {
        /// <summary>
        /// Gets or sets the attendance unique identifier.
        /// </summary>
        /// <value>The attendance unique identifier.</value>
        public Guid AttendanceGuid { get; set; }

        /// <summary>
        /// Gets or sets the attendance encrypted identifier.
        /// </summary>
        /// <value>The attendance encrypted identifier.</value>
        public string AttendanceIdKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if the attendance record was added
        /// or if this is an update to an existing record.
        /// </summary>
        public bool IsNew { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the person that attended.
        /// </summary>
        /// <value>The unique identifier of the person that attended.</value>
        public Guid PersonGuid { get; set; }

        /// <summary>
        /// Gets or sets the occurrence unique identifier.
        /// </summary>
        /// <value>The occurrence unique identifier.</value>
        public Guid OccurrenceGuid { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the location that was attended.
        /// </summary>
        /// <value>The unique identifier of the location that was attended.</value>
        public Guid? LocationGuid { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the group the person attended.
        /// </summary>
        /// <value>The unique identifier of the group the person attended.</value>
        public Guid? GroupGuid { get; set; }

        /// <summary>
        /// Gets or sets the group type unique identifier of the group the
        /// person attended.
        /// </summary>
        /// <value>The group type unique identifier of the group the person attended.</value>
        public Guid? GroupTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the attendance status which indicates if the persent
        /// attended or not.
        /// </summary>
        /// <value>The attendance status.</value>
        public AttendanceStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the check in status of the attendance record.
        /// </summary>
        /// <value>The status of the attendance record.</value>
        public CheckInStatus CheckInStatus { get; set; }

        /// <summary>
        /// Gets or sets the RSVP state of this attendance record.
        /// </summary>
        /// <value>The RSVP state of this attendance record.</value>
        public RSVP RSVP { get; set; }

        /// <summary>
        /// Gets or sets the full name of the person this attendance record is for.
        /// </summary>
        /// <value>The full name of the person this attendance record is for.</value>
        public string PersonFullName { get; set; }

        /// <summary>
        /// Gets or sets the photo URL of the person this attendance record is for.
        /// </summary>
        /// <value>The photo URL of the person this attendance record is for.</value>
        public string PersonPhotoUrl { get; set; }
    }
}

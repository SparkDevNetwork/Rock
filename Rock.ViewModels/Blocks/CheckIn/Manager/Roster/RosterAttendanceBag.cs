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
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.CheckIn.Manager.Roster
{
    /// <summary>
    /// Represents a single attendance record on the roster.
    /// </summary>
    public class RosterAttendanceBag
    {
        /// <summary>
        /// The encoded identifier for the attendance record.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// The attendee associated with this attendance record.
        /// </summary>
        public RosterAttendeeBag Attendee { get; set; }

        /// <summary>
        /// The security code included on the individual's nametag.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// The date and time the individual checked in.
        /// </summary>
        public DateTimeOffset CheckInTime { get; set; }

        /// <summary>
        /// The date and time the individual was marked as present in the room.
        /// </summary>
        public DateTimeOffset? PresentTime { get; set; }

        /// <summary>
        /// The date and time the individual was checked out.
        /// </summary>
        public DateTimeOffset? CheckoutTime { get; set; }

        /// <summary>
        /// The name and identifier of the schedule the individual checked in to.
        /// </summary>
        public ListItemBag Schedule { get; set; }

        /// <summary>
        /// The name and identifier of the group the individual checked in to.
        /// </summary>
        public ListItemBag Group { get; set; }

        /// <summary>
        /// The name and identifier of the area the individual checked in to.
        /// </summary>
        public ListItemBag Area { get; set; }

        /// <summary>
        /// The current status of the attendance record.
        /// </summary>
        public CheckInStatus Status { get; set; }

        /// <summary>
        /// Determines if this attendance record represents the individual's first
        /// ever check-in record.
        /// </summary>
        public bool IsFirstTime { get; set; }

        /// <summary>
        /// Determines if checkout is supported for this attendance record.
        /// </summary>
        public bool IsCheckoutSupported { get; set; }

        /// <summary>
        /// Determines if presence tracking is supported for this attendance
        /// record.
        /// </summary>
        public bool IsPresenceSupported { get; set; }
    }
}

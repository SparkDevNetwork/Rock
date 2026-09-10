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

namespace Rock.ViewModels.Blocks.CheckIn.Manager.EnRoute
{
    /// <summary>
    /// Represents a single attendee row in the En Route grid. Each row is
    /// one person who may be checked into one or more services.
    /// </summary>
    public class EnRouteAttendeeBag
    {
        /// <summary>
        /// Gets or sets the person's unique identifier, used as the grid
        /// row key.
        /// </summary>
        public Guid PersonGuid { get; set; }

        /// <summary>
        /// Gets or sets the attendance record identifiers for this person.
        /// A person checked into multiple services will have multiple ids.
        /// Used by the Move Person modal to determine which attendance to
        /// move.
        /// </summary>
        public List<int> AttendanceIds { get; set; }

        /// <summary>
        /// Gets or sets the HTML img tag for the person's photo avatar,
        /// including the appropriate no-photo silhouette when the person
        /// has no photo on file.
        /// </summary>
        public string PhotoImageTag { get; set; }

        /// <summary>
        /// Gets or sets the person's nick (preferred) name.
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// Gets or sets the person's last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the person's full display name.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the parent names for child attendees. Null for
        /// adults.
        /// </summary>
        public string ParentNames { get; set; }

        /// <summary>
        /// Gets or sets the group name to display. When the "Show Only
        /// Parent Group" setting is enabled, this contains the parent group
        /// name; otherwise it contains the actual check-in group name.
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Gets or sets the group type path displayed as a subtitle under
        /// the group name. When "Show Only Parent Group" is enabled, this
        /// is the parent group's type path.
        /// </summary>
        public string GroupPath { get; set; }

        /// <summary>
        /// Gets or sets the comma-separated service times the person is
        /// checked into.
        /// </summary>
        public string ServiceTimes { get; set; }

        /// <summary>
        /// Gets or sets the room (location) name for the attendance.
        /// </summary>
        public string RoomName { get; set; }
    }
}

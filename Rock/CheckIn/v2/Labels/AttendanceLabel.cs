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

using Rock.Model;
using Rock.ViewModels.CheckIn;
using Rock.Web.Cache;

namespace Rock.CheckIn.v2.Labels
{
    /// <summary>
    /// Details about a single attendance record that will be made available
    /// during printing of check-in labels.
    /// </summary>
    internal class AttendanceLabel
    {
        /// <summary>
        /// The person this attendance record is for.
        /// </summary>
        public Person Person { get; set; }

        /// <summary>
        /// The date and time the individual was checked in.
        /// </summary>
        public DateTime StartDateTime { get; set; }

        /// <summary>
        /// The date and time the individual was checked out or <c>null</c>
        /// if they have not been checked out yet.
        /// </summary>
        public DateTime? EndDateTime { get; set; }

        /// <summary>
        /// Indicates if attendance record was from a check-in session where
        /// no other attendance records previously existed for the same person.
        /// </summary>
        public bool IsFirstTime { get; set; }

        /// <summary>
        /// The area used during the check-in for this attendance.
        /// </summary>
        public GroupTypeCache Area { get; set; }

        /// <summary>
        /// The group used during the check-in for this attendance.
        /// </summary>
        public GroupCache Group { get; set; }

        /// <summary>
        /// The location used during the check-in for this attendance.
        /// </summary>
        public NamedLocationCache Location { get; set; }

        /// <summary>
        /// The schedule used during the check-in for this attendance.
        /// </summary>
        public NamedScheduleCache Schedule { get; set; }

        /// <summary>
        /// All <see cref="GroupMember"/> records this <see cref="Person"/>
        /// has in <see cref="Group"/>.
        /// </summary>
        public List<GroupMember> GroupMembers { get; set; }

        /// <summary>
        /// The security code that was assigned to this person for check-in.
        /// </summary>
        public string SecurityCode { get; set; }

        /// <summary>
        /// The details of any achievements that were completed during this
        /// check-in session.
        /// </summary>
        public List<AchievementBag> JustCompletedAchievements { get; set; }

        /// <summary>
        /// The details of any achievements that are still in progress after
        /// the check-in session was completed.
        /// </summary>
        public List<AchievementBag> InProgressAchievements { get; set; }

        /// <summary>
        /// The details of any achievements that were previously completed
        /// before the check-in session was started.
        /// </summary>
        public List<AchievementBag> PreviouslyCompletedAchievements { get; set; }
    }
}

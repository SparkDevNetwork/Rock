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
using System;

using Rock.Model;
using Rock.Web.Cache;
using Rock.Data;
using System.Linq;

namespace Rock.CheckIn.v2.Labels
{
    /// <summary>
    /// <para>
    /// A checkout label is printed for every attendance record during the
    /// checkout process.
    /// </para>
    /// <para>
    /// Meaning if Noah is checked in to the 9am service and the 11am service
    /// and is checked out from both at the same time, he will get two check-out
    /// labels.
    /// </para>
    /// </summary>
    internal class CheckoutLabelData : ILabelDataHasPerson, ILabelDataHasAttendance
    {
        /// <inheritdoc/>
        public LabelAttendanceDetail Attendance { get; set; }

        /// <inheritdoc/>
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

        /// <summary>
        /// The date and time this person was checked in for this label.
        /// </summary>
        public DateTime CheckInDateTime { get; }

        /// <summary>
        /// The date and time this person was checked out for this label.
        /// </summary>
        public DateTime CheckoutDateTime { get; }

        /// <summary>
        /// The current date and time the label is being printed at.
        /// </summary>
        public DateTime CurrentDateTime { get; }

        /// <summary>
        /// The names of any group roles for any group membership records
        /// the person has in the group they were checked into.
        /// </summary>
        public List<string> GroupRoleNames { get; }

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckoutLabelData"/> class.
        /// </summary>
        /// <param name="attendance">The attendance data for the label that is being generated.</param>
        /// <param name="family">The family group used during the check-in process.</param>
        /// <param name="rockContext">The <see cref="RockContext"/> for data operations.</param>
        public CheckoutLabelData( LabelAttendanceDetail attendance, Group family, RockContext rockContext )
        {
            Attendance = attendance;
            Family = family ?? attendance.Person?.PrimaryFamily;

            CheckInDateTime = Attendance.StartDateTime;
            CheckoutDateTime = Attendance.EndDateTime ?? RockDateTime.Now;
            CurrentDateTime = RockDateTime.Now;

            GroupRoleNames = Attendance.GroupMembers
                ?.Select( gm => GroupTypeRoleCache.Get( gm.GroupRoleId, rockContext )?.Name )
                .Where( n => n != null )
                .ToList();
        }

        #endregion
    }
}

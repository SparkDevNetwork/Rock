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
using System.Linq;

using Rock.Model;

namespace Rock.CheckIn
{
    /// <summary>
    /// Attendance Information needed for the Roster CheckinManager UIs
    /// </summary>
    public class RosterAttendeeAttendance
    {
        /// <summary>
        /// Gets the group type identifier.
        /// </summary>
        /// <value>
        /// The group type identifier.
        /// </value>
        public int GroupTypeId { get; internal set; }

        /// <summary>
        /// Gets the person identifier.
        /// </summary>
        /// <value>
        /// The person identifier.
        /// </value>
        public int PersonId { get; internal set; }

        /// <summary>
        /// Gets the person.
        /// </summary>
        /// <value>
        /// The person.
        /// </value>
        public Person Person { get; internal set; }

        /// <summary>
        /// Gets the attendance code.
        /// </summary>
        /// <value>
        /// The attendance code.
        /// </value>
        public string AttendanceCode { get; internal set; }

        /// <summary>
        /// Gets the start date time.
        /// </summary>
        /// <value>
        /// The start date time.
        /// </value>
        public DateTime StartDateTime { get; internal set; }

        /// <summary>
        /// Gets the present date time.
        /// </summary>
        /// <value>
        /// The present date time.
        /// </value>
        public DateTime? PresentDateTime { get; internal set; }

        /// <summary>
        /// Gets the end date time.
        /// </summary>
        /// <value>
        /// The end date time.
        /// </value>
        public DateTime? EndDateTime { get; internal set; }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id { get; internal set; }

        /// <summary>
        /// Gets the is first time.
        /// </summary>
        /// <value>
        /// The is first time.
        /// </value>
        public bool? IsFirstTime { get; internal set; }

        /// <summary>
        /// Gets the schedule.
        /// </summary>
        /// <value>
        /// The schedule.
        /// </value>
        public Schedule Schedule { get; internal set; }

        /// <summary>
        /// Gets the name of the group.
        /// </summary>
        /// <value>
        /// The name of the group.
        /// </value>
        public string GroupName { get; internal set; }

        /// <summary>
        /// Gets the group identifier.
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        public int? GroupId { get; internal set; }

        /// <summary>
        /// Gets the parent group identifier.
        /// </summary>
        /// <value>
        /// The parent group identifier.
        /// </value>
        public int? ParentGroupId { get; internal set; }

        /// <summary>
        /// Gets the name of the parent group.
        /// </summary>
        /// <value>
        /// The name of the parent group.
        /// </value>
        public string ParentGroupName { get; internal set; }

        /// <summary>
        /// Gets the parent group's group type identifier.
        /// </summary>
        /// <value>
        /// The parent group group's type identifier.
        /// </value>
        public int? ParentGroupGroupTypeId { get; internal set; }

        /// <summary>
        /// Gets the location identifier.
        /// </summary>
        /// <value>
        /// The location identifier.
        /// </value>
        public int? LocationId { get; internal set; }

        /// <summary>
        /// Gets the schedule identifier.
        /// </summary>
        /// <value>
        /// The schedule identifier.
        /// </value>
        public int? ScheduleId { get; internal set; }

        /// <summary>
        /// Selects a RosterAttendeeAttendance from specified attendance query.
        /// </summary>
        /// <param name="attendanceQuery">The attendance query.</param>
        /// <returns></returns>
        public static IQueryable<RosterAttendeeAttendance> Select( IQueryable<Attendance> attendanceQuery )
        {
            var rosterAttendeeAttendanceQry = attendanceQuery.Select( a => new RosterAttendeeAttendance
            {
                Id = a.Id,
                Schedule = a.Occurrence.Schedule,
                ScheduleId = a.Occurrence.ScheduleId,
                IsFirstTime = a.IsFirstTime,
                StartDateTime = a.StartDateTime,
                PresentDateTime = a.PresentDateTime,
                EndDateTime = a.EndDateTime,
                AttendanceCode = a.AttendanceCode.Code,
                PersonId = a.PersonAlias.PersonId,
                Person = a.PersonAlias.Person,
                LocationId = a.Occurrence.LocationId,

                GroupId = a.Occurrence.GroupId,
                GroupTypeId = a.Occurrence.Group.GroupTypeId,
                GroupName = a.Occurrence.Group.Name,

                ParentGroupId = a.Occurrence.Group.ParentGroupId,
                ParentGroupGroupTypeId = a.Occurrence.Group.ParentGroupId.HasValue ? a.Occurrence.Group.ParentGroup.GroupTypeId : ( int? ) null,
                ParentGroupName = a.Occurrence.Group.ParentGroupId.HasValue ? a.Occurrence.Group.ParentGroup.Name : null
            } );

            return rosterAttendeeAttendanceQry;
        }
    }
}
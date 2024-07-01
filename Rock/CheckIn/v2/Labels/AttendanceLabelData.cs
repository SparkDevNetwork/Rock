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
using System.Linq;

using Rock.Data;
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
    internal class AttendanceLabelData : ILabelDataHasPerson
    {
        /// <summary>
        /// The data that describes the attendance record this label is being
        /// printed for.
        /// </summary>
        public AttendanceLabel Attendance { get; set; }

        /// <inheritdoc/>
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

        /// <summary>
        /// The achievement type names that were completed by the person this
        /// label is being printed for during this check-in session.
        /// </summary>
        public List<string> JustCompletedAchievements { get; set; }

        /// <summary>
        /// The achievement type identifiers that were completed by the person
        /// this label is being printed for during this check-in session.
        /// </summary>
        public List<int> JustCompletedAchievementIds { get; set; }

        /// <summary>
        /// The achievement type names that are currently in progress for
        /// the person this label is being printed for during this check-in
        /// session.
        /// </summary>
        public List<string> InProgressAchievements { get; set; }

        /// <summary>
        /// The achievement type identifiers that are currently in progress for
        /// the person this label is being printed for during this check-in
        /// session.
        /// </summary>
        public List<int> InProgressAchievementIds { get; set; }

        /// <summary>
        /// The achievement type names that were previously completed for
        /// the person this label is being printed for during this check-in
        /// session.
        /// </summary>
        public List<string> PreviouslyCompletedAchievements { get; set; }

        /// <summary>
        /// The achievement type identifiers that were previously completed for
        /// the person this label is being printed for during this check-in
        /// session.
        /// </summary>
        public List<int> PreviouslyCompletedAchievementIds { get; set; }

        /// <summary>
        /// The date and time this person was checked in for this label.
        /// </summary>
        public DateTime CheckInTime { get; }

        /// <summary>
        /// The current date and time the label is being printed at.
        /// </summary>
        public DateTime CurrentTime { get; }

        /// <summary>
        /// The names of any group roles for any group membership records
        /// the person has in the group they were checked into.
        /// </summary>
        public List<string> GroupRoleNames { get; }

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AttendanceLabelData"/> class.
        /// </summary>
        /// <param name="attendance">The attendance data for the label that is being generated.</param>
        /// <param name="family">The family group used during the check-in process.</param>
        /// <param name="allAttendance">The list of all attendance labels.</param>
        /// <param name="rockContext">The <see cref="RockContext"/> for data operations.</param>
        public AttendanceLabelData( AttendanceLabel attendance, Group family, List<AttendanceLabel> allAttendance, RockContext rockContext )
        {
            Attendance = attendance;
            Family = family ?? attendance.Person.PrimaryFamily;
            AllAttendance = allAttendance;

            PersonAttendance = allAttendance
                .Where( a => a.Person.Id == attendance.Person.Id )
                .ToList();

            CheckInTime = PersonAttendance.Count > 0
                ? PersonAttendance.Min( a => a.StartDateTime )
                : RockDateTime.Now;
            CurrentTime = RockDateTime.Now;

            GroupRoleNames = PersonAttendance
                .SelectMany( a => a.GroupMembers )
                .Select( gm => GroupTypeCache.Get( gm.GroupTypeId, rockContext )
                    ?.Roles
                    .FirstOrDefault( r => r.Id == gm.GroupRoleId )
                    ?.Name )
                .Where( n => n != null )
                .ToList();

            // Just completed achievements.
            var justCompletedAchievements = PersonAttendance
                .SelectMany( a => a.JustCompletedAchievements.Select( ach => ach.AchievementTypeId ) )
                .Distinct()
                .Select( id => AchievementTypeCache.GetByIdKey( id, rockContext ) )
                .Where( a => a != null )
                .ToList();

            JustCompletedAchievementIds = justCompletedAchievements
                .Select( a => a.Id )
                .ToList();
            JustCompletedAchievements = justCompletedAchievements
                .Select( a => a.Name )
                .ToList();

            // In progress achievements.
            var inProgressAchievements = PersonAttendance
                .SelectMany( a => a.InProgressAchievements.Select( ach => ach.AchievementTypeId ) )
                .Distinct()
                .Select( id => AchievementTypeCache.GetByIdKey( id, rockContext ) )
                .Where( a => a != null )
                .ToList();

            InProgressAchievementIds = inProgressAchievements
                .Select( a => a.Id )
                .ToList();
            InProgressAchievements = inProgressAchievements
                .Select( a => a.Name )
                .ToList();

            // Just completed achievements.
            var previouslyCompletedAchievements = PersonAttendance
                .SelectMany( a => a.PreviouslyCompletedAchievements.Select( ach => ach.AchievementTypeId ) )
                .Distinct()
                .Select( id => AchievementTypeCache.GetByIdKey( id, rockContext ) )
                .Where( a => a != null )
                .ToList();

            PreviouslyCompletedAchievementIds = previouslyCompletedAchievements
                .Select( a => a.Id )
                .ToList();
            PreviouslyCompletedAchievements = previouslyCompletedAchievements
                .Select( a => a.Name )
                .ToList();
        }

        #endregion
    }
}

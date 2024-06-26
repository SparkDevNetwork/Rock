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
    /// The label data for a person label to be printed during the check-in
    /// session. A person label is printed once per person per session.
    /// </para>
    /// <para>
    /// Meaning, if Noah is checked in to the 9am service and the 11am service
    /// in the same session, he gets one Person label.
    /// </para>
    /// <para>
    /// However, if Noah is checked in to the 9am service and then a new
    /// check-in session is done to check him in to the 11am service, he will
    /// get 2 Person labels (one for each session).
    /// </para>
    /// </summary>
    internal class PersonLabelData : ILabelDataHasPerson
    {
        /// <inheritdoc/>
        public Person Person { get; }

        /// <summary>
        /// The attendance records for the <see cref="Person"/> during this
        /// check-in session.
        /// </summary>
        public List<AttendanceLabel> PersonAttendance { get; }

        /// <summary>
        /// All attendance records for this session, including those from
        /// other people being checked in as well as those from
        /// <see cref="PersonAttendance"/>.
        /// </summary>
        public List<AttendanceLabel> AllAttendance { get; }

        /// <summary>
        /// The family object that was determined via either kiosk search
        /// or <see cref="Person.PrimaryFamily"/> if checking in a single
        /// person by API call.
        /// </summary>
        public Group Family { get; }

        /// <summary>
        /// The achievement type names that were completed by the person this
        /// label is being printed for during this check-in session.
        /// </summary>
        public List<string> JustCompletedAchievements { get; }

        /// <summary>
        /// The achievement type identifiers that were completed by the person
        /// this label is being printed for during this check-in session.
        /// </summary>
        public List<int> JustCompletedAchievementIds { get; }

        /// <summary>
        /// The achievement type names that are currently in progress for
        /// the person this label is being printed for during this check-in
        /// session.
        /// </summary>
        public List<string> InProgressAchievements { get; }

        /// <summary>
        /// The achievement type identifiers that are currently in progress for
        /// the person this label is being printed for during this check-in
        /// session.
        /// </summary>
        public List<int> InProgressAchievementIds { get; }

        /// <summary>
        /// The achievement type names that were previously completed for
        /// the person this label is being printed for during this check-in
        /// session.
        /// </summary>
        public List<string> PreviouslyCompletedAchievements { get; }

        /// <summary>
        /// The achievement type identifiers that were previously completed for
        /// the person this label is being printed for during this check-in
        /// session.
        /// </summary>
        public List<int> PreviouslyCompletedAchievementIds { get; }

        /// <summary>
        /// Indicates if this is the first time this person has attended
        /// anything at the organization. Uses the <see cref="Attendance.IsFirstTime"/>
        /// property of <see cref="PersonAttendance"/> to make the determination.
        /// </summary>
        public bool IsFirstTime { get; }

        /// <summary>
        /// The names of the check-in areas that the person was checked into.
        /// </summary>
        public List<string> AreaNames { get; }

        /// <summary>
        /// The date and time this person was checked in for this label.
        /// </summary>
        public DateTime CheckInTime { get; }

        /// <summary>
        /// The current date and time the label is being printed at.
        /// </summary>
        public DateTime CurrentTime { get; }

        /// <summary>
        /// The names of the check-in groups that the person was checked into.
        /// </summary>
        public List<string> GroupNames { get; }

        /// <summary>
        /// The names of any group roles for any group membership records
        /// the person has in any groups they were checked into.
        /// </summary>
        public List<string> GroupRoleNames { get; }

        /// <summary>
        /// The names of the locations that the person was checked into.
        /// </summary>
        public List<string> LocationNames { get; }

        /// <summary>
        /// The names of the schedules that the person was checked into.
        /// </summary>
        public List<string> ScheduleNames { get; }

        /// <summary>
        /// The security code for the person during this check-in session.
        /// </summary>
        public string SecurityCode { get; }

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonLabelData"/> class.
        /// </summary>
        /// <param name="person">The person for whom the label data is being generated.</param>
        /// <param name="family">The family group used during the check-in process.</param>
        /// <param name="allAttendance">The list of all attendance labels.</param>
        /// <param name="rockContext">The <see cref="RockContext"/> for data operations.</param>
        public PersonLabelData( Person person, Group family, List<AttendanceLabel> allAttendance, RockContext rockContext )
        {
            Person = person;
            Family = family ?? person.PrimaryFamily;
            AllAttendance = allAttendance;

            PersonAttendance = allAttendance
                .Where( a => a.Person.Id == person.Id )
                .ToList();

            IsFirstTime = PersonAttendance.Any( a => a.IsFirstTime );
            AreaNames = PersonAttendance.Select( a => a.Area.Name ).ToList();
            CheckInTime = PersonAttendance.Count > 0
                ? PersonAttendance.Min( a => a.StartDateTime )
                : RockDateTime.Now;
            CurrentTime = RockDateTime.Now;
            GroupNames = PersonAttendance.Select( a => a.Group.Name ).ToList();
            LocationNames = PersonAttendance.Select( a => a.Location.Name ).ToList();
            ScheduleNames = PersonAttendance.Select( a => a.Schedule.Name ).ToList();
            SecurityCode = PersonAttendance.Select( a => a.SecurityCode ).FirstOrDefault() ?? string.Empty;

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

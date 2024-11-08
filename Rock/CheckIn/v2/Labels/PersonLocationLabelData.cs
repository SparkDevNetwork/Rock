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
    /// A person location label is printed once per person and location.
    /// </para>
    /// <para>
    /// Meaning, if Jordan Greggs is checked into the Bears room at both the
    /// 9:00am service and the 11:00am service, then only one person location
    /// label will print.
    /// </para>
    /// <para>
    /// If Jordan Greggs is checked into the Bears room and Billy Greggs is
    /// also checked into the Bears room, then two labels will print.
    /// </para>
    /// </summary>
    internal class PersonLocationLabelData : ILabelDataHasPerson
    {
        /// <inheritdoc/>
        public Person Person { get; }

        /// <summary>
        /// The location that represents the room this attendance label is being
        /// printed for.
        /// </summary>
        public NamedLocationCache Location { get; }

        /// <summary>
        /// The attendance records for the <see cref="Person"/> during this
        /// check-in session.
        /// </summary>
        public List<LabelAttendanceDetail> PersonAttendance { get; }

        /// <summary>
        /// The attendance records for the <see cref="Person"/> and location
        /// during this check-in session.
        /// </summary>
        public List<LabelAttendanceDetail> LocationAttendance { get; }

        /// <summary>
        /// All attendance records for this session.
        /// </summary>
        public List<LabelAttendanceDetail> AllAttendance { get; }

        /// <summary>
        /// The achievement type names that were completed by any person during
        /// this check-in session.
        /// </summary>
        public List<string> JustCompletedAchievements { get; }

        /// <summary>
        /// The achievement type identifiers that were completed by any person
        /// during this check-in session.
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
        /// The names of the check-in areas that the person was checked into.
        /// </summary>
        public List<string> AreaNames { get; }

        /// <summary>
        /// The date and time this person was checked in for this label.
        /// </summary>
        public DateTime CheckInDateTime { get; }

        /// <summary>
        /// The current date and time the label is being printed at.
        /// </summary>
        public DateTime CurrentDateTime { get; }

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
        /// Indicates if this is the first time this person has attended
        /// anything at the organization. Uses the <see cref="Attendance.IsFirstTime"/>
        /// property of <see cref="PersonAttendance"/> to make the determination.
        /// </summary>
        public bool IsFirstTime { get; }

        /// <summary>
        /// The names of the schedules that any person was checked into.
        /// </summary>
        public List<string> ScheduleNames { get; }

        /// <summary>
        /// The security code for the person during this check-in session.
        /// </summary>
        public string SecurityCode { get; }

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FamilyLabelData"/> class.
        /// </summary>
        /// <param name="person">The person for whom the label data is being generated.</param>
        /// <param name="location">The location the person was checked into.</param>
        /// <param name="allAttendance">The list of all attendance labels.</param>
        /// <param name="rockContext">The <see cref="RockContext"/> for data operations.</param>
        public PersonLocationLabelData( Person person, NamedLocationCache location, List<LabelAttendanceDetail> allAttendance, RockContext rockContext )
        {
            Person = person;
            Location = location;
            AllAttendance = allAttendance;

            PersonAttendance = allAttendance
                .Where( a => a.Person != null && a.Person.Id == person.Id )
                .ToList();
            LocationAttendance = allAttendance
                .Where( a => a.Person != null && a.Person.Id == person.Id
                    && a.Location != null && a.Location.Id == location.Id )
                .ToList();

            AreaNames = LocationAttendance.Select( a => a.Area.Name ).ToList();
            CheckInDateTime = AllAttendance.Count > 0
                ? AllAttendance.Min( a => a.StartDateTime )
                : RockDateTime.Now;
            CurrentDateTime = RockDateTime.Now;
            GroupNames = LocationAttendance.Select( a => a.Group.Name ).ToList();
            GroupRoleNames = PersonAttendance
                .SelectMany( a => a.GroupMembers )
                .Select( gm => GroupTypeRoleCache.Get( gm.GroupRoleId, rockContext )?.Name )
                .Where( n => n != null )
                .ToList();
            IsFirstTime = PersonAttendance.Any( a => a.IsFirstTime );
            ScheduleNames = LocationAttendance.Select( a => a.Schedule.Name ).ToList();
            SecurityCode = PersonAttendance.Select( a => a.SecurityCode ).FirstOrDefault() ?? string.Empty;

            // Just completed achievements.
            var justCompletedAchievements = AllAttendance
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

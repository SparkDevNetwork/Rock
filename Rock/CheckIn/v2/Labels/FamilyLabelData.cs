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
    /// A family label is printed once per session.
    /// </para>
    /// <para>
    /// Meaning, if Jordan Greggs has a "can check-in" relationship to Noah
    /// then Jordan will show up when checking in the Decker family. If both
    /// Noah and Jordan are checked in at the same time, only one family
    /// label will print even though Jordan is technically in a different
    /// family.
    /// </para>
    /// </summary>
    internal class FamilyLabelData
    {
        /// <summary>
        /// All attendance records for this session.
        /// </summary>
        public List<LabelAttendanceDetail> AllAttendance { get; }

        /// <summary>
        /// The family object that was determined via either kiosk search
        /// or <see cref="Person.PrimaryFamily"/> if checking in a single
        /// person by API call.
        /// </summary>
        public Group Family { get; }

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
        /// The date and time these people were checked in for this label.
        /// </summary>
        public DateTime CheckInDateTime { get; }

        /// <summary>
        /// The current date and time the label is being printed at.
        /// </summary>
        public DateTime CurrentDateTime { get; }

        /// <summary>
        /// The nick name of each person that was checked in.
        /// </summary>
        public List<string> NickNames { get; set; }

        /// <summary>
        /// The first name of each person that was checked in.
        /// </summary>
        public List<string> FirstNames { get; set; }

        /// <summary>
        /// The last name of each person that was checked in.
        /// </summary>
        public List<string> LastNames { get; set; }

        /// <summary>
        /// The names of the check-in areas that any person was checked into.
        /// </summary>
        public List<string> AreaNames { get; }

        /// <summary>
        /// The names of the check-in groups that any person was checked into.
        /// </summary>
        public List<string> GroupNames { get; }

        /// <summary>
        /// The names of the locations that any person was checked into.
        /// </summary>
        public List<string> LocationNames { get; }

        /// <summary>
        /// The names of the schedules that any person was checked into.
        /// </summary>
        public List<string> ScheduleNames { get; }

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FamilyLabelData"/> class.
        /// </summary>
        /// <param name="family">The family group used during the check-in process.</param>
        /// <param name="allAttendance">The list of all attendance labels.</param>
        /// <param name="rockContext">The <see cref="RockContext"/> for data operations.</param>
        public FamilyLabelData( Group family, List<LabelAttendanceDetail> allAttendance, RockContext rockContext )
        {
            Family = family;
            AllAttendance = allAttendance;

            CheckInDateTime = AllAttendance.Count > 0
                ? AllAttendance.Min( a => a.StartDateTime )
                : RockDateTime.Now;
            CurrentDateTime = RockDateTime.Now;

            NickNames = AllAttendance.Where(a => a.Person != null )
                .Select( a => a.Person )
                .DistinctBy( p => p.Id )
                .Select( p => p.NickName )
                .ToList();
            FirstNames = AllAttendance.Where( a => a.Person != null )
                .Select( a => a.Person )
                .DistinctBy( p => p.Id )
                .Select( p => p.FirstName )
                .ToList();
            LastNames = AllAttendance.Where( a => a.Person != null )
                .Select( a => a.Person )
                .DistinctBy( p => p.Id )
                .Select( p => p.LastName )
                .ToList();

            AreaNames = AllAttendance.Select( a => a.Area?.Name )
                .Distinct()
                .ToList();

            GroupNames = AllAttendance.Select( a => a.Group?.Name )
                .Distinct()
                .ToList();

            LocationNames = AllAttendance.Select( a => a.Location?.Name )
                .Distinct()
                .ToList();

            ScheduleNames = AllAttendance.Select( a => a.Schedule?.Name )
                .Distinct()
                .ToList();

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
        }

        #endregion
    }
}

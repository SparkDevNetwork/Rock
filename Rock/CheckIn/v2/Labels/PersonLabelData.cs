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

using Rock.Model;
using Rock.ViewModels.CheckIn;

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
        public Person Person { get; set; }

        /// <summary>
        /// The attendance records for the <see cref="Person"/> during this
        /// check-in session.
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
        public List<Guid> JustCompletedAchievemenIds { get; set; }

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
        public List<string> InProgressAchievementIds { get; set; }

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
        public List<string> PreviouslyCompletedAchievementIds { get; set; }
    }
}

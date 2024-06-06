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
        public List<AttendanceLabel> AllAttendance { get; set; }

        /// <summary>
        /// The family object that was determined via either kiosk search
        /// or <see cref="Person.PrimaryFamily"/> if checking in a single
        /// person by API call.
        /// </summary>
        public Group Family { get; set; }

        /// <summary>
        /// The achievement type names that were completed by any person during
        /// this check-in session.
        /// </summary>
        public List<string> JustCompletedAchievements { get; set; }

        /// <summary>
        /// The achievement type identifiers that were completed by any person
        /// during this check-in session.
        /// </summary>
        public List<int> JustCompletedAchievemenIds { get; set; }
    }
}

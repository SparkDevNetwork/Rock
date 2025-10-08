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

namespace Rock.AI.Agent.Classes.Skills.PersonSkill
{
    /// <summary>
    /// Represents a summarized view of family service attendance.
    /// </summary>
    /// <remarks>
    /// This class contains details about family attendance, including check-ins, summaries, and attendance statistics.
    /// </remarks>
    internal class SummarizedFamilyServiceAttendanceResult
    {
        /// <summary>
        /// Gets or sets the Sunday date for the attendance summary.
        /// </summary>
        /// <value>
        /// The Sunday date for the attendance summary.
        /// </value>
        public DateTime? SundayDate { get; set; }

        /// <summary>
        /// Gets or sets the list of family member check-ins.
        /// </summary>
        /// <value>
        /// The list of family member check-ins.
        /// </value>
        public List<FamilyMemberCheckInResult> CheckIns { get; set; }

        /// <summary>
        /// Gets or sets the check-in summary for the family.
        /// </summary>
        /// <value>
        /// The check-in summary for the family.
        /// </value>
        public List<CheckInSummaryMonthResult> CheckInSummary { get; set; }

        /// <summary>
        /// Gets or sets the date of the first-time check-in.
        /// </summary>
        /// <value>
        /// The date of the first-time check-in, or <c>null</c> if not applicable.
        /// </value>
        public DateTime? FirstTimeCheckIn { get; set; }

        /// <summary>
        /// Gets or sets the number of weeks attended in the last 16 weeks.
        /// </summary>
        /// <value>
        /// The number of weeks attended in the last 16 weeks.
        /// </value>
        public int WeeksAttendedLast16Weeks { get; set; }
    }
}

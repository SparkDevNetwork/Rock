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

using Rock.ViewModels.CheckIn;

namespace Rock.ViewModels.Rest.CheckIn
{
    /// <summary>
    /// The response that will be returned by the list family members check-in
    /// REST endpoint.
    /// </summary>
    public class FamilyMembersResponseBag
    {
        /// <summary>
        /// Gets or sets the family identifier.
        /// </summary>
        /// <value>The family identifier.</value>
        public string FamilyId { get; set; }

        /// <summary>
        /// Gets or sets all possible schedules that are available across all
        /// of the people in this family. If schedule A is available for the
        /// first person and schedule B is available for the second person then
        /// both schedule A and B will be included.
        /// </summary>
        /// <value>The collection of possible schedules.</value>
        public List<ScheduleOpportunityBag> PossibleSchedules { get; set; }

        /// <summary>
        /// Gets or sets the people that can be potentially checked in for
        /// the family.
        /// </summary>
        /// <value>The people.</value>
        public List<AttendeeBag> People { get; set; }

        /// <summary>
        /// Gets or sets the current attendance records that can be checked out.
        /// </summary>
        /// <value>The current attendance.</value>
        public List<AttendanceBag> CurrentlyCheckedInAttendances { get; set; }
    }
}

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

namespace Rock.ViewModels.CheckIn
{
    /// <summary>
    /// Details about a single attendee being considered for check-in.
    /// </summary>
    public class AttendeeBag
    {
        /// <summary>
        /// Gets or sets the person represented by this item.
        /// </summary>
        /// <value>The person.</value>
        public PersonBag Person { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this attendee is unavailable.
        /// </summary>
        /// <value><c>true</c> if this attendee is unavailable; otherwise, <c>false</c>.</value>
        public bool IsUnavailable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this attendee is already selected.
        /// </summary>
        /// <value><c>true</c> if this attendee is already selected; otherwise, <c>false</c>.</value>
        public bool IsPreSelected { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this attendee has multiple selections available.
        /// </summary>
        /// <value><c>true</c> if this attendee has multiple selections available; otherwise, <c>false</c>.</value>
        public bool IsMultipleSelectionsAvailable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this attendee is in the
        /// primary family matched during the search operation.
        /// </summary>
        public bool IsInPrimaryFamily { get; set; }

        /// <summary>
        /// Gets or sets the message describing why this attendee is not available.
        /// </summary>
        /// <value>The unavailable reason message.</value>
        public string UnavailableMessage { get; set; }

        /// <summary>
        /// Gets or sets the selected opportunities for this attendee.
        /// </summary>
        /// <value>The selected opportunities.</value>
        public List<OpportunitySelectionBag> SelectedOpportunities { get; set; }

        /// <summary>
        /// Gets all possible schedules that are available to this attendee.
        /// </summary>
        public List<ScheduleOpportunityBag> PossibleSchedules { get; set; }

        /// <summary>
        /// <para>
        /// All potential schedules that are valid for this attendee. A
        /// schedule is considered a potential if at least one group was valid
        /// for the attendee. The group may no longer exist in the
        /// opportunities if no locations were available, but the schedule was
        /// still considered a potential match.
        /// </para>
        /// <para>
        /// The primary use of potential schedules is to determine if the skip
        /// screen should be shown during family check-in when multiple schedules
        /// are selected and one or more attendees have no way they could have
        /// checked into one of the schedules.
        /// </para>
        /// </summary>
        public List<string> PotentialScheduleIds { get; set; }
    }
}

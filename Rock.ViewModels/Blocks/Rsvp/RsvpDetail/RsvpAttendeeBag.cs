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

using Rock.ViewModels.Core.Grid;

namespace Rock.ViewModels.Blocks.Rsvp.RsvpDetail
{
    /// <summary>
    /// Represents a single invitee row in the attendee RSVP grid.
    /// </summary>
    public class RsvpAttendeeBag
    {
        /// <summary>
        /// Gets or sets the IdKey of the invitee Person. Used as the row key and to upsert the Attendance record.
        /// </summary>
        public string PersonIdKey { get; set; }

        /// <summary>
        /// Gets or sets the invitee's person field used by the PersonColumn (avatar, name, profile link).
        /// </summary>
        public PersonFieldBag Person { get; set; }

        /// <summary>
        /// Gets or sets the friendly RSVP status used for grid display, filtering, and the per-row save payload
        /// ("Accept", "Decline", or "No Response").
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the Guid of the selected decline reason (DefinedValue), or null when none was provided.
        /// </summary>
        public string DeclineReasonValueGuid { get; set; }

        /// <summary>
        /// Gets or sets the free-text decline note left by (or for) the invitee.
        /// </summary>
        public string DeclineNote { get; set; }
    }
}

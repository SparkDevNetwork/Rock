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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.CheckIn.Manager.AttendanceDetail
{
    /// <summary>
    /// The lookup data the Move Person modal needs to populate its cascading
    /// Schedule / Location / Group dropdowns and pre-select the current
    /// occurrence. Returned by the GetMovePersonOptions block action.
    /// </summary>
    public class AttendanceDetailMovePersonOptionsBag
    {
        /// <summary>
        /// Gets or sets the full list of schedules the person may be moved
        /// to, sorted by CheckinManagerHelper.GroupAndSortMovePersonOptions.
        /// Value is the schedule integer id (as a string).
        /// </summary>
        public List<ListItemBag> Schedules { get; set; }

        /// <summary>
        /// Gets or sets the list of eligible locations keyed by the selected
        /// schedule's id-as-string. Consumed client-side to cascade-filter
        /// the Location dropdown without a server round-trip.
        /// </summary>
        public Dictionary<string, List<ListItemBag>> LocationsBySchedule { get; set; }

        /// <summary>
        /// Gets or sets the list of eligible groups keyed by
        /// "{scheduleId}{Delimiter}{locationId}". Consumed client-side to
        /// cascade-filter the Group dropdown after Schedule + Location are
        /// picked.
        /// </summary>
        public Dictionary<string, List<ListItemBag>> GroupsByScheduleAndLocation { get; set; }

        /// <summary>
        /// Gets or sets the delimiter used to compose the composite key in
        /// GroupsByScheduleAndLocation. Passed through so the client uses the
        /// exact same delimiter as the server.
        /// </summary>
        public string GroupListItemKeyDelimiter { get; set; }

        /// <summary>
        /// Gets or sets the schedule id currently on the attendance's
        /// occurrence. Used to pre-select the Schedule dropdown when the
        /// modal opens.
        /// </summary>
        public int? CurrentScheduleId { get; set; }

        /// <summary>
        /// Gets or sets the location id currently on the attendance's
        /// occurrence. Used to pre-select the Location dropdown.
        /// </summary>
        public int? CurrentLocationId { get; set; }

        /// <summary>
        /// Gets or sets the group id currently on the attendance's
        /// occurrence. Used to pre-select the Group dropdown.
        /// </summary>
        public int? CurrentGroupId { get; set; }

        /// <summary>
        /// Gets or sets the current attendance start date/time as an ISO 8601
        /// string, used to pre-populate the check-in date-time picker.
        /// </summary>
        public string StartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the current attendance end date/time as an ISO 8601
        /// string, used to pre-populate the check-out date-time picker.
        /// </summary>
        public string EndDateTime { get; set; }

        /// <summary>
        /// Gets or sets an error message when the options could not be
        /// computed (e.g. the attendance no longer exists). Non-empty message
        /// tells the client to keep the modal closed and surface the text.
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}

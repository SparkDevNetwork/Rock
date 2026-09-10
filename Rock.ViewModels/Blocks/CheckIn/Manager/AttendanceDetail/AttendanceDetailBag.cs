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

namespace Rock.ViewModels.Blocks.CheckIn.Manager.AttendanceDetail
{
    /// <summary>
    /// Read-only display payload for the Attendance Detail block. Populated
    /// on initial load and re-returned by the MovePerson block action so the
    /// view panel can rebind without a second round-trip.
    /// </summary>
    public class AttendanceDetailBag
    {
        /// <summary>
        /// Gets or sets the person's full name (used in the delete-confirmation
        /// dialog and as convenience context on the view panel). Empty when the
        /// person alias is missing.
        /// </summary>
        public string PersonName { get; set; }

        /// <summary>
        /// Gets or sets the composed group display text, formatted as
        /// "{Check-in Area Path} &gt; {Group Name}".
        /// </summary>
        public string GroupText { get; set; }

        /// <summary>
        /// Gets or sets the location name for the attendance occurrence. Empty
        /// when the occurrence has no location.
        /// </summary>
        public string LocationText { get; set; }

        /// <summary>
        /// Gets or sets the attendance-code text (e.g. "AB12"). Empty when the
        /// attendance has no code.
        /// </summary>
        public string TagText { get; set; }

        /// <summary>
        /// Gets or sets the schedule name for the attendance occurrence. Empty
        /// when the occurrence has no schedule.
        /// </summary>
        public string ScheduleText { get; set; }

        /// <summary>
        /// Gets or sets the pre-formatted check-in label, e.g.
        /// "10/24/2026 9:15 AM by Jane Doe 555-123-4567". Null hides the row.
        /// </summary>
        public string CheckInLabel { get; set; }

        /// <summary>
        /// Gets or sets the pre-formatted "present" label, using the same
        /// shape as CheckInLabel. Null hides the row.
        /// </summary>
        public string PresentLabel { get; set; }

        /// <summary>
        /// Gets or sets the pre-formatted checked-out label, using the same
        /// shape as CheckInLabel. Null hides the row.
        /// </summary>
        public string CheckedOutLabel { get; set; }

        /// <summary>
        /// Gets or sets the check-in start date/time as an ISO 8601 string.
        /// Used to pre-populate the Move Person modal's start-time picker.
        /// </summary>
        public string StartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the check-out end date/time as an ISO 8601 string.
        /// Used to pre-populate the Move Person modal's end-time picker.
        /// </summary>
        public string EndDateTime { get; set; }

        /// <summary>
        /// Gets or sets the ordered list of change-history rows displayed on
        /// the view panel.
        /// </summary>
        public List<AttendanceDetailChangeHistoryBag> ChangeHistory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entire "search context"
        /// section (search group, search type, search value) should render.
        /// False when none of the underlying fields have data.
        /// </summary>
        public bool IsSearchSectionVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the "Search Group Name"
        /// sub-section (name label + adult members list) should render.
        /// </summary>
        public bool IsSearchResultGroupVisible { get; set; }

        /// <summary>
        /// Gets or sets the search-result group's display name. Empty when
        /// there is no search-result group.
        /// </summary>
        public string SearchResultGroupName { get; set; }

        /// <summary>
        /// Gets or sets the adult members of the search-result group. Empty
        /// when there is no search-result group.
        /// </summary>
        public List<AttendanceDetailSearchGroupAdultBag> SearchGroupAdults { get; set; }

        /// <summary>
        /// Gets or sets the search type (e.g. "Phone Number", "Name"). Null
        /// hides the row.
        /// </summary>
        public string SearchTypeText { get; set; }

        /// <summary>
        /// Gets or sets the raw search value entered at check-in. Null hides
        /// the row.
        /// </summary>
        public string SearchValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current user is
        /// authorized to open the Move Person modal.
        /// </summary>
        public bool CanEdit { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current user is
        /// authorized to delete this attendance.
        /// </summary>
        public bool CanDelete { get; set; }
    }
}

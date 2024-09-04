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
using Rock.Enums.Blocks.Group.GroupAttendanceDetail;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Group.GroupAttendanceDetail
{
    /// <summary>
    /// A bag that contains the information required to render the Obsidian Group Attendance Detail block.
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Blocks.BlockBox" />
    public class GroupAttendanceDetailInitializationBox : BlockBox
    {
        /// <summary>
        /// Gets or sets the group unique identifier.
        /// </summary>
        /// <value>
        /// The group unique identifier.
        /// </value>
        public Guid GroupGuid { get; set; }

        /// <summary>
        /// Gets or sets the name of the group.
        /// </summary>
        /// <value>
        /// The name of the group.
        /// </value>
        public string GroupName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is new attendance date addition restricted.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is new attendance date addition restricted; otherwise, <c>false</c>.
        /// </value>
        public bool IsNewAttendanceDateAdditionRestricted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an authorized group was not found.
        /// </summary>
        public bool IsAuthorizedGroupNotFoundError { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is future occurrence date selection restricted.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is future occurrence date selection restricted; otherwise, <c>false</c>.
        /// </value>
        public bool IsFutureOccurrenceDateSelectionRestricted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is campus filtering allowed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is campus filtering allowed; otherwise, <c>false</c>.
        /// </value>
        public bool IsCampusFilteringAllowed { get; set; }

        /// <summary>
        /// Gets or sets the notes section label.
        /// </summary>
        /// <value>
        /// The notes section label.
        /// </value>
        public string NotesSectionLabel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is notes section hidden.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is notes section hidden; otherwise, <c>false</c>.
        /// </value>
        public bool IsNotesSectionHidden { get; set; }

        /// <summary>
        /// Gets or sets the attendance occurrence date selection mode.
        /// </summary>
        /// <value>
        /// The attendance occurrence date selection mode.
        /// </value>
        public GroupAttendanceDetailDateSelectionMode AttendanceOccurrenceDateSelectionMode { get; set; }

        /// <summary>
        /// Gets or sets the location label.
        /// </summary>
        /// <value>
        /// The location label.
        /// </value>
        public string LocationLabel { get; set; }

        /// <summary>
        /// Gets or sets the location selection mode.
        /// </summary>
        /// <value>
        /// The location selection mode.
        /// </value>
        public GroupAttendanceDetailLocationSelectionMode LocationSelectionMode { get; set; }

        /// <summary>
        /// Gets or sets the schedule selection mode.
        /// </summary>
        /// <value>
        /// The schedule selection mode.
        /// </value>
        public GroupAttendanceDetailScheduleSelectionMode ScheduleSelectionMode { get; set; }

        /// <summary>
        /// Gets or sets the schedule label.
        /// </summary>
        /// <value>
        /// The schedule label.
        /// </value>
        public string ScheduleLabel { get; set; }

        /// <summary>
        /// Gets or sets the attendance occurrence date (time ignored).
        /// </summary>
        /// <value>
        /// The attendance occurrence date.
        /// </value>
        public DateTimeOffset? AttendanceOccurrenceDate { get; set; }

        /// <summary>
        /// Gets or sets the location unique identifier.
        /// </summary>
        /// <value>
        /// The location unique identifier.
        /// </value>
        public Guid? LocationGuid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is attendance occurrence types section shown.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is attendance occurrence types section shown; otherwise, <c>false</c>.
        /// </value>
        public bool IsAttendanceOccurrenceTypesSectionShown { get; set; }

        /// <summary>
        /// Gets or sets the attendance occurrence types section label.
        /// </summary>
        /// <value>
        /// The attendance occurrence types section label.
        /// </value>
        public string AttendanceOccurrenceTypesSectionLabel { get; set; }

        /// <summary>
        /// Gets or sets the attendance occurrence types.
        /// </summary>
        /// <value>
        /// The attendance occurrence types.
        /// </value>
        public List<ListItemBag> AttendanceOccurrenceTypes { get; set; }

        /// <summary>
        /// Gets or sets the selected attendance occurrence type value.
        /// </summary>
        /// <value>
        /// The selected attendance occurrence type value.
        /// </value>
        public string SelectedAttendanceOccurrenceTypeValue { get; set; }

        /// <summary>
        /// Gets or sets the group members section label.
        /// </summary>
        /// <value>
        /// The group members section label.
        /// </value>
        public string GroupMembersSectionLabel { get; set; }

        /// <summary>
        /// Gets or sets the notes.
        /// </summary>
        /// <value>
        /// The notes.
        /// </value>
        public string Notes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is did not meet checked.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is did not meet checked; otherwise, <c>false</c>.
        /// </value>
        public bool IsDidNotMeetChecked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is new attendee addition allowed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is new attendee addition allowed; otherwise, <c>false</c>.
        /// </value>
        public bool IsNewAttendeeAdditionAllowed { get; set; }

        /// <summary>
        /// Gets or sets the add person as.
        /// </summary>
        /// <value>
        /// The add person as.
        /// </value>
        public string AddPersonAs { get; set; }

        /// <summary>
        /// Gets or sets the add group member page URL.
        /// </summary>
        /// <value>
        /// The add group member page URL.
        /// </value>
        public string AddGroupMemberPageUrl { get; set; }

        /// <summary>
        /// Gets or sets the attendances.
        /// </summary>
        /// <value>
        /// The attendances.
        /// </value>
        public List<GroupAttendanceDetailAttendanceBag> Attendances { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is no attendance occurrences error.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is no attendance occurrences error; otherwise, <c>false</c>.
        /// </value>
        public bool IsNoAttendanceOccurrencesError { get; set; }

        /// <summary>
        /// Gets a value indicating whether there is a configuration error.
        /// </summary>
        public bool IsConfigError => !string.IsNullOrWhiteSpace( ErrorMessage )
            || IsAuthorizedGroupNotFoundError
            || IsNoAttendanceOccurrencesError;

        /// <summary>
        /// Gets or sets the schedule unique identifier.
        /// </summary>
        /// <value>
        /// The schedule unique identifier.
        /// </value>
        public Guid? ScheduleGuid { get; set; }

        /// <summary>
        /// Gets or sets the attendance occurrence unique identifier.
        /// </summary>
        /// <value>
        /// The attendance occurrence unique identifier.
        /// </value>
        public Guid? AttendanceOccurrenceGuid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is long list disabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is long list disabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsLongListDisabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is did not meet disabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is did not meet disabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsDidNotMeetDisabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is back button hidden.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is back button hidden; otherwise, <c>false</c>.
        /// </value>
        public bool IsBackButtonHidden { get; set; }

        /// <summary>
        /// Gets or sets the back page URL.
        /// </summary>
        /// <value>
        /// The back page URL.
        /// </value>
        public string BackPageUrl { get; set; }

        /// <summary>
        /// Gets or sets the attendance occurrence identifier.
        /// </summary>
        /// <value>
        /// The attendance occurrence identifier.
        /// </value>
        public int? AttendanceOccurrenceId { get; set; }

        /// <summary>
        /// The number of days back appear in the schedule drop down list to choose from.
        /// </summary>
        public int NumberOfPreviousDaysToShow { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether location is required.
        /// </summary>
        public bool IsLocationRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether schedule is required.
        /// </summary>
        public bool IsScheduleRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether roster download is shown.
        /// </summary>
        public bool IsRosterDownloadShown { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if date is included in the pick from schedule picker.
        /// </summary>
        public bool IsDateIncludedInPickFromSchedule { get; set; }


        /// <summary>
        /// Campus status defined value guids that limit which campuses are included in the list of available campuses in the campus picker.
        /// </summary>
        /// <value>
        /// The campus status filter.
        /// </value>
        public List<Guid> CampusStatusFilter { get; set; }

        /// <summary>
        /// Campus type defined value guids that limit which campuses are included in the list of available campuses in the campus picker.
        /// </summary>
        /// <value>
        /// The campus type filter.
        /// </value>
        public List<Guid> CampusTypeFilter { get; set; }
    }
}

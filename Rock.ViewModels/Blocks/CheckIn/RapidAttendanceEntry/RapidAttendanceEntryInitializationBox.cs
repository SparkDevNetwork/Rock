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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.CheckIn.RapidAttendanceEntry
{
    /// <summary>
    /// The box that contains all the initialization information for the Rapid Attendance Entry block.
    /// </summary>
    public class RapidAttendanceEntryInitializationBox : BlockBox
    {
        /// <summary>
        /// Gets or sets a value indicating whether the attendance setup screen is shown at the start of each session
        /// so attendance can be taken.
        /// </summary>
        public bool IsAttendanceEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the campus picker is shown on the attendance setup screen. The
        /// selected campus limits the available group locations.
        /// </summary>
        public bool IsCampusPickerVisible { get; set; }

        /// <summary>
        /// Gets or sets the campus type defined value unique identifiers that limit which campuses the campus picker
        /// offers.
        /// </summary>
        public List<Guid> CampusTypeFilter { get; set; }

        /// <summary>
        /// Gets or sets the campus status defined value unique identifiers that limit which campuses the campus
        /// picker offers.
        /// </summary>
        public List<Guid> CampusStatusFilter { get; set; }

        /// <summary>
        /// Gets or sets the group the block is locked to by the Attendance Group block setting. When set, no group
        /// selection is offered at session start.
        /// </summary>
        public ListItemBag AttendanceGroup { get; set; }

        /// <summary>
        /// Gets or sets the selectable groups, the active children of the configured Parent Group. Null when the
        /// block is not constrained to a parent group, in which case the full group picker is shown instead.
        /// </summary>
        public List<ListItemBag> GroupItems { get; set; }

        /// <summary>
        /// Gets or sets the session resumed from the individual's last visit. When set, the block bypasses the setup
        /// screen and starts on the entry screen. Null when setup must be shown first, or when attendance is
        /// disabled.
        /// </summary>
        public RapidAttendanceEntrySessionBag ActiveSession { get; set; }

        /// <summary>
        /// Gets or sets the age below which a family member cannot be marked as attended, from the Minimum
        /// Attendance Age setting. Zero when no minimum applies.
        /// </summary>
        public int MinimumAttendanceAge { get; set; }

        /// <summary>
        /// Gets or sets the label displayed above the workflow checkbox list.
        /// </summary>
        public string WorkflowListTitle { get; set; }

        /// <summary>
        /// Gets or sets the workflows offered as checkboxes on each individual's entry panel. Null when none are
        /// configured, in which case the section is hidden.
        /// </summary>
        public List<ListItemBag> WorkflowItems { get; set; }

        /// <summary>
        /// Gets or sets the note types available in the Note section. Null when none are configured, in which case
        /// the section is hidden.
        /// </summary>
        public List<ListItemBag> NoteTypeItems { get; set; }

        /// <summary>
        /// Gets or sets the label displayed above the Connection Opportunities checkbox list.
        /// </summary>
        public string ConnectionOpportunitiesListTitle { get; set; }

        /// <summary>
        /// Gets or sets the prayer request entry options. Null when prayer request entry is disabled, in which case
        /// the section is hidden.
        /// </summary>
        public RapidAttendanceEntryPrayerOptionsBag PrayerOptions { get; set; }
    }
}

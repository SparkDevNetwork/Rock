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

using Rock.ViewModels.Core.Grid;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Rsvp.RsvpDetail
{
    /// <summary>
    /// Holds the initialization data for the RSVP Detail block.
    /// </summary>
    public class RsvpDetailBag
    {
        /// <summary>
        /// Gets or sets the name of the parent group; used for the panel heading.
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this represents a new (not yet persisted) occurrence.
        /// When true, the block opens directly into edit mode and the attendee grid is hidden.
        /// </summary>
        public bool IsNewOccurrence { get; set; }

        /// <summary>
        /// Gets or sets the IdKey of the AttendanceOccurrence being viewed or edited.
        /// Empty when <see cref="IsNewOccurrence"/> is true.
        /// </summary>
        public string OccurrenceIdKey { get; set; }

        /// <summary>
        /// Gets or sets the optional name describing the occurrence.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the occurrence date.
        /// </summary>
        public DateTime? OccurrenceDate { get; set; }

        /// <summary>
        /// Gets or sets the friendly label of the occurrence date (used in the read-only view).
        /// </summary>
        public string OccurrenceDateText { get; set; }

        /// <summary>
        /// Gets or sets the location associated with the occurrence (Value = Location.Guid, Text = friendly name).
        /// </summary>
        public ListItemBag Location { get; set; }

        /// <summary>
        /// Gets or sets the friendly description of the occurrence's schedule (used in the read-only view).
        /// </summary>
        public string ScheduleText { get; set; }

        /// <summary>
        /// Gets or sets the schedule associated with the occurrence (Value = Schedule.Guid, Text = friendly schedule text).
        /// </summary>
        public ListItemBag Schedule { get; set; }

        /// <summary>
        /// Gets or sets the custom message shown when invitees accept the RSVP.
        /// </summary>
        public string AcceptMessage { get; set; }

        /// <summary>
        /// Gets or sets the custom message shown when invitees decline the RSVP.
        /// </summary>
        public string DeclineMessage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether decline reasons should be collected.
        /// </summary>
        public bool ShowDeclineReasons { get; set; }

        /// <summary>
        /// Gets or sets the decline reasons selected as available for this specific occurrence.
        /// Each value is a DefinedValue Guid.
        /// </summary>
        public List<string> AvailableDeclineReasonGuids { get; set; }

        /// <summary>
        /// Gets or sets the full list of active decline reason DefinedValues from the configured DefinedType.
        /// Used to populate the "Available Decline Reasons" check box list on the edit form.
        /// </summary>
        public List<ListItemBag> AllDeclineReasons { get; set; }

        /// <summary>
        /// Gets or sets the decline reasons that are actually selectable in the attendee grid.
        /// Filtered to <see cref="AvailableDeclineReasonGuids"/> when set; otherwise contains all active decline reasons.
        /// </summary>
        public List<ListItemBag> AttendeeDeclineReasons { get; set; }

        /// <summary>
        /// Gets or sets the attendee rows displayed in the editable grid.
        /// </summary>
        public List<RsvpAttendeeBag> Attendees { get; set; }

        /// <summary>
        /// Gets or sets the count of attendees who have accepted the RSVP.
        /// </summary>
        public int AcceptCount { get; set; }

        /// <summary>
        /// Gets or sets the count of attendees who have declined the RSVP.
        /// </summary>
        public int DeclineCount { get; set; }

        /// <summary>
        /// Gets or sets the count of attendees who have not yet responded.
        /// </summary>
        public int NoResponseCount { get; set; }

        /// <summary>
        /// Gets or sets the grid definition for the attendees grid. Built server-side so
        /// the grid's filter row, action toolbar (Communicate, Bulk Update, Merge Person,
        /// Merge Template, Export), and field metadata are available to the Obsidian Grid.
        /// </summary>
        public GridDefinitionBag AttendeesGridDefinition { get; set; }
    }
}

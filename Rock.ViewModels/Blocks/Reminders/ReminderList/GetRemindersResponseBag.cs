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

namespace Rock.ViewModels.Blocks.Reminders.ReminderList
{
    /// <summary>
    /// Response payload from <c>GetReminders</c> and every mutating block action
    /// (Mark Complete, Reschedule, Reassign, Delete) so the client can re-render
    /// from a single source of truth without an extra fetch.
    /// </summary>
    public class GetRemindersResponseBag
    {
        /// <summary>
        /// Gets or sets the filtered, sorted reminder cards.
        /// </summary>
        public List<ReminderListBag> Reminders { get; set; }

        /// <summary>
        /// Gets or sets the option lists. Refreshed on every response because
        /// the available reminder types depend on the entity-type filter, which
        /// can change between requests.
        /// </summary>
        public ReminderListOptionsBag Options { get; set; }

        /// <summary>
        /// Gets or sets the resolved entity selection for the View Options
        /// picker. Server-resolved from the stored EntityGuid preference so the
        /// client can rehydrate the picker with the correct text without an
        /// additional round trip. Null when no entity drilldown is active.
        /// </summary>
        public ListItemBag SelectedEntity { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current person has zero
        /// reminders for any entity type, before any filter is applied. When
        /// true the client collapses the toolbar / filter chrome and renders a
        /// single "You do not have any reminders." notification, mirroring the
        /// WebForms <c>pnlNoReminders</c> behavior.
        /// </summary>
        public bool HasNoReminders { get; set; }
    }
}

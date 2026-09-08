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

namespace Rock.ViewModels.Blocks.CheckIn.Manager.RoomList
{
    /// <summary>
    /// A single row of the Room List grid. The same shape covers both display
    /// modes: one row per Location+Group by default, or one row per Location
    /// when Show Only Parent Group is enabled.
    /// </summary>
    public class RoomInfoBag
    {
        /// <summary>
        /// Gets or sets the stable key for this row. When Show Only Parent
        /// Group is off it is a composite "{LocationIdKey}|{GroupIdKey}";
        /// when Show Only Parent Group is on it is simply the LocationIdKey.
        /// The row select handler ships the row's LocationIdKey (not this
        /// value) to the Roster page.
        /// </summary>
        public string RowKey { get; set; }

        /// <summary>
        /// Gets or sets the Location's IdKey. Used to build the roster page
        /// URL when the row is clicked.
        /// </summary>
        public string LocationIdKey { get; set; }

        /// <summary>
        /// Gets or sets the Location's display name. Rendered in the Room
        /// column when it is visible.
        /// </summary>
        public string LocationName { get; set; }

        /// <summary>
        /// Gets or sets the group name for the Group column. When Show Only
        /// Parent Group is on this holds the comma-joined, alphabetically
        /// ordered parent group names for the location; otherwise it is the
        /// actual group's name.
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Gets or sets the group-type path shown as the secondary line under
        /// the group name (rendered by HighlightDetailColumn). Empty when
        /// Show Only Parent Group is on.
        /// </summary>
        public string GroupTypePath { get; set; }

        /// <summary>
        /// Gets or sets the count of attendees currently in the Checked-in
        /// state for this row. Deduplicated by person.
        /// </summary>
        public int CheckedInCount { get; set; }

        /// <summary>
        /// Gets or sets the count of attendees currently in the Present state
        /// for this row. Deduplicated by person.
        /// </summary>
        public int PresentCount { get; set; }

        /// <summary>
        /// Gets or sets the count of attendees currently in the Checked-out
        /// state for this row. Deduplicated by person.
        /// </summary>
        public int CheckedOutCount { get; set; }
    }
}

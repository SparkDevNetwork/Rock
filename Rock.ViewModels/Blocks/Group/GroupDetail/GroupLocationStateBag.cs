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

using Rock.Model;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Group.GroupDetail
{
    /// <summary>
    /// Editable Group Location row used by the Location modal. <see cref="SelectedLocation"/>'s
    /// runtime shape is driven by <see cref="SelectedLocationMode"/>, which the server-side
    /// resolver branches on to materialize a <c>Location</c>. Active schedules only;
    /// inactive schedules are reconciled server-side at save time.
    /// </summary>
    public class GroupLocationStateBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the group location. New rows arrive with
        /// <see cref="Guid.Empty"/>; the save flow assigns a fresh Guid before persisting.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the friendly text used to render the Location column in the grid.
        /// </summary>
        public string LocationName { get; set; }

        /// <summary>
        /// Gets or sets the discriminator describing how <see cref="SelectedLocation"/> was
        /// emitted by the <c>&lt;LocationPicker&gt;</c> or the Member-tab dropdown.
        /// </summary>
        public GroupLocationPickerMode SelectedLocationMode { get; set; }

        /// <summary>
        /// Gets or sets the raw picker emit. The runtime shape is determined by
        /// <see cref="SelectedLocationMode"/>: <see cref="ListItemBag"/> when Named or
        /// GroupMember; <see cref="Rock.ViewModels.Controls.AddressControlBag"/> when Address;
        /// a Well-Known Text string when Point or Polygon. Typed as <see cref="object"/> so
        /// System.Text.Json can round-trip the heterogeneous shape.
        /// </summary>
        public object SelectedLocation { get; set; }

        /// <summary>
        /// Gets or sets the Location Type DefinedValue Guid scoped to <c>GroupType.LocationTypeValues</c>.
        /// </summary>
        public Guid? GroupLocationTypeValueGuid { get; set; }

        /// <summary>
        /// Gets or sets the friendly name of the selected location type.
        /// </summary>
        public string GroupLocationTypeValueName { get; set; }

        /// <summary>
        /// Gets or sets the active schedules attached to this <c>GroupLocation</c>.
        /// </summary>
        public List<ListItemBag> Schedules { get; set; } = new List<ListItemBag>();

        /// <summary>
        /// Gets or sets the PersonAlias Guid for the family member who owns this location.
        /// Non-null only when the row was added via the Member tab.
        /// </summary>
        public Guid? GroupMemberPersonAliasGuid { get; set; }

        /// <summary>
        /// Gets or sets the per-schedule capacity matrix entries (one row per selected schedule).
        /// </summary>
        public List<GroupLocationScheduleConfigBag> ScheduleConfigs { get; set; } = new List<GroupLocationScheduleConfigBag>();
    }
}

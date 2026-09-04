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

namespace Rock.ViewModels.Blocks.CheckIn.Manager.EnRoute
{
    /// <summary>
    /// The options bag that seeds the En Route block's initial render, including
    /// the filter state restored from the shared CheckinManager cookie and
    /// block-level settings.
    /// </summary>
    public class EnRouteOptionsBag
    {
        /// <summary>
        /// Gets or sets the list of active check-in schedules available for
        /// filtering. Each item's Value is the schedule integer id (as a
        /// string) and Text is the schedule display name.
        /// </summary>
        public List<ListItemBag> AvailableSchedules { get; set; }

        /// <summary>
        /// Gets or sets the schedule integer ids that were previously selected
        /// in the cookie-based filter. Used to pre-select items in the schedule
        /// list box on the initial render.
        /// </summary>
        public List<int> SelectedScheduleIds { get; set; }

        /// <summary>
        /// Gets or sets the groups that were previously selected in the
        /// cookie-based filter, hydrated into ListItemBag format for the
        /// GroupPicker. Each item's Value is the group Guid (as a string).
        /// </summary>
        public List<ListItemBag> SelectedGroups { get; set; }

        /// <summary>
        /// Gets or sets the check-in area group type Guids that the
        /// GroupPicker should be limited to. Computed from the resolved
        /// check-in area filter.
        /// </summary>
        public List<Guid> CheckinAreaGroupTypeGuids { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the "Include Child Groups"
        /// checkbox was checked in the cookie-based filter.
        /// </summary>
        public bool IsIncludeChildGroups { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the "Always Show Child
        /// Groups" block setting is enabled. When true, child groups are
        /// always included and the checkbox is hidden.
        /// </summary>
        public bool IsAlwaysShowChildGroups { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the "Show Only Parent
        /// Group" block setting is enabled. When true, the Group column
        /// displays the parent group name and path instead of the actual
        /// group.
        /// </summary>
        public bool IsShowOnlyParentGroup { get; set; }
    }
}

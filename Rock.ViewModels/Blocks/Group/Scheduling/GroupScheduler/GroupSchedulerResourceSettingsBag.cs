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
using Rock.Enums.Blocks.Group.Scheduling;
using Rock.Model;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Group.Scheduling.GroupScheduler
{
    /// <summary>
    /// The resource settings to indicate how individuals should be selected for assignment within the group scheduler.
    /// </summary>
    public class GroupSchedulerResourceSettingsBag
    {
        /// <summary>
        /// Gets or sets the enabled resource list source types, from which individuals may be scheduled.
        /// </summary>
        /// <value>
        /// The enabled resource list source types, from which individuals may be scheduled.
        /// </value>
        public List<ResourceListSourceType> EnabledResourceListSourceTypes { get; set; }

        /// <summary>
        /// Gets or sets the selected resource list source type.
        /// </summary>
        /// <value>
        /// The selected resource list source type.
        /// </value>
        public ResourceListSourceType ResourceListSourceType { get; set; }

        /// <summary>
        /// Gets or sets the selected resource group member filter type.
        /// </summary>
        /// <value>
        /// The selected resource group member filter type.
        /// </value>
        public SchedulerResourceGroupMemberFilterType ResourceGroupMemberFilterType { get; set; }

        /// <summary>
        /// Gets or sets the resource alternate group (if any).
        /// </summary>
        /// <value>
        /// The alternate resource alternate group (if any).
        /// </value>
        public ListItemBag ResourceAlternateGroup { get; set; }

        /// <summary>
        /// Gets or sets the resource data view (if any).
        /// </summary>
        /// <value>
        /// The resource data view (if any).
        /// </value>
        public ListItemBag ResourceDataView { get; set; }
    }
}

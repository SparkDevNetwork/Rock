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

namespace Rock.ViewModels.Blocks.Group.Scheduling.GroupScheduler
{
    /// <summary>
    /// The available and selected locations for the Group Scheduler.
    /// </summary>
    public class GroupSchedulerLocationsBag
    {
        /// <summary>
        /// Gets or sets the available locations.
        /// </summary>
        /// <value>
        /// The available locations.
        /// </value>
        public List<ListItemBag> AvailableLocations { get; set; }

        /// <summary>
        /// Gets or sets the selected locations.
        /// </summary>
        /// <value>
        /// The selected locations.
        /// </value>
        public List<ListItemBag> SelectedLocations { get; set; }
    }
}

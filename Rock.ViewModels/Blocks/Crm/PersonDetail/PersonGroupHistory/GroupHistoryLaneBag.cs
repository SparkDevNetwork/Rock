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

namespace Rock.ViewModels.Blocks.Crm.PersonDetail.PersonGroupHistory
{
    /// <summary>
    /// A single group's membership history, rendered as one set of bars on the timeline.
    /// </summary>
    public class GroupHistoryLaneBag
    {
        /// <summary>
        /// Gets or sets the group's IdKey. Used to build the group link shown in the timeline popup.
        /// </summary>
        public string GroupIdKey { get; set; }

        /// <summary>
        /// Gets or sets the group type identifier.
        /// </summary>
        public int GroupTypeId { get; set; }

        /// <summary>
        /// Gets or sets the group type color used to fill the timeline bars. May be null when the group type has no color.
        /// </summary>
        public string GroupTypeColor { get; set; }

        /// <summary>
        /// Gets or sets the group type name.
        /// </summary>
        public string GroupTypeName { get; set; }

        /// <summary>
        /// Gets or sets the start/stop membership periods within this group.
        /// </summary>
        public List<GroupHistoryLaneItemBag> StartStopHistory { get; set; }
    }
}

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

namespace Rock.ViewModels.Blocks.Event.EventCalendarItemList
{
    /// <summary>
    /// The additional configuration options for the Event Calendar Item List block.
    /// </summary>
    public class EventCalendarItemListOptionsBag
    {
        /// <summary>
        /// Gets or sets available campus items for the Campus filter.
        /// </summary>
        /// <value>
        /// The campus items.
        /// </value>
        public List<ListItemBag> CampusItems { get; set; }

        /// <summary>
        /// Gets or sets the event calendar identifier key.
        /// </summary>
        /// <value>
        /// The event calendar identifier key.
        /// </value>
        public string EventCalendarIdKey { get; set; }
    }
}

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

namespace Rock.ViewModels.Blocks.Event.CalendarContentChannelItemList
{
    /// <summary>
    /// The additional configuration options for the Calendar Content Channel Item List block.
    /// </summary>
    public class CalendarContentChannelItemListOptionsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the block should be displayed.
        /// </summary>
        /// <value>
        /// <c>true</c> if the block should be visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsBlockVisible { get; set; }

        /// <summary>
        /// Gets or sets the content channel sections to render as collapsible grids.
        /// </summary>
        /// <value>
        /// The content channel sections.
        /// </value>
        public List<CalendarContentChannelItemListContentChannelBag> ContentChannels { get; set; }
    }
}

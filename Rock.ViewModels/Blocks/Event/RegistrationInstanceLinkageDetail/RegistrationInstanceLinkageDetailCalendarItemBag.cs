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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Event.RegistrationInstanceLinkageDetail
{
    /// <summary>
    /// Represents the details of a calendar item used in the Registration Instance Linkage Detail block.
    /// </summary>
    public class RegistrationInstanceLinkageDetailCalendarItemBag
    {
        /// <summary>
        /// Gets or sets the selected calendar.
        /// </summary>
        public ListItemBag SelectedCalendar { get; set; }

        /// <summary>
        /// Gets or sets the start date of the date range.
        /// </summary>
        public DateTime DateRangeStart { get; set; }

        /// <summary>
        /// Gets or sets the end date of the date range.
        /// </summary>
        public DateTime DateRangeEnd { get; set; }

        /// <summary>
        /// Gets or sets the selected calendar item.
        /// </summary>
        public ListItemBag SelectedCalendarItem { get; set; }

        /// <summary>
        /// Gets or sets the selected occurrence.
        /// </summary>
        public ListItemBag SelectedOccurrence { get; set; }
    }
}

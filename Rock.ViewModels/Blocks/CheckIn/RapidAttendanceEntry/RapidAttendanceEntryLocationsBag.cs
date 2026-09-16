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

namespace Rock.ViewModels.Blocks.CheckIn.RapidAttendanceEntry
{
    /// <summary>
    /// The locations available for a selected group on the attendance setup screen.
    /// </summary>
    public class RapidAttendanceEntryLocationsBag
    {
        /// <summary>
        /// Gets or sets the offered locations, limited to the selected campus when one is selected.
        /// </summary>
        public List<ListItemBag> Items { get; set; }

        /// <summary>
        /// Gets or sets the group's location count before the campus filter. A count of one means no choice exists,
        /// so the lone location is auto-selected and its dropdown hidden. A positive count alongside an empty offered
        /// list means the campus filter excluded every location.
        /// </summary>
        public int TotalLocationCount { get; set; }
    }
}

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

namespace Rock.ViewModels.Blocks.CheckIn.AttendanceHistoryList
{
    /// <summary>
    /// The additional configuration options for the Attendance List block.
    /// </summary>
    public class AttendanceHistoryListOptionsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the block has a valid context entity.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the block's Context Entity is valid; otherwise, <c>false</c>.
        /// </value>
        public bool IsValidContextEntity { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the grid's person column should be visible.
        /// The person column is hidden if the block is configured with a person as its context entity.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the grid's person column should be visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsPersonColumnVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the grid's group column should be visible.
        /// The group column is hidden if the block is configured with a group as its context entity.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the grid's group column should be visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsGroupColumnVisible { get; set; }

        /// <summary>
        /// Gets or sets the group items for the group filter picker.
        /// </summary>
        /// <value>
        /// The group items.
        /// </value>
        public List<ListItemBag> GroupItems { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the DidAttend filter should default to true.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [filter attendance by default]; otherwise, <c>false</c>.
        /// </value>
        public bool FilterAttendanceByDefault { get; set; }
    }
}

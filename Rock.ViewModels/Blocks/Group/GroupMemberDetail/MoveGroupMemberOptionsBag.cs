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

namespace Rock.ViewModels.Blocks.Group.GroupMemberDetail
{
    /// <summary>
    /// The response returned when a destination group is selected in the
    /// Group Member Detail block's move modal.
    /// </summary>
    public class MoveGroupMemberOptionsBag
    {
        /// <summary>
        /// Gets or sets the roles available in the destination group.
        /// </summary>
        public List<ListItemBag> RoleItems { get; set; }

        /// <summary>
        /// Gets or sets the destination group's default role identifier.
        /// </summary>
        public int? DefaultRoleId { get; set; }

        /// <summary>
        /// Gets or sets the warning message shown below the fields
        /// (attribute loss, same group, or already a member). Null when
        /// there is nothing to warn about.
        /// </summary>
        public string WarningMessage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Move Fundraising
        /// Transactions checkbox is shown. True only for fundraising group
        /// types with transactions to move.
        /// </summary>
        public bool IsFundraisingOptionShown { get; set; }
    }
}

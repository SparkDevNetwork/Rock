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

namespace Rock.ViewModels.Blocks.Group.GroupMemberDetail
{
    /// <summary>
    /// The request sent to the Group Member Detail block's MoveGroupMember
    /// action.
    /// </summary>
    public class MoveGroupMemberRequestBag
    {
        /// <summary>
        /// Gets or sets the IdKey of the group member to move.
        /// </summary>
        public string GroupMemberIdKey { get; set; }

        /// <summary>
        /// Gets or sets the IdKey of the destination group.
        /// </summary>
        public string DestinationGroupIdKey { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the role the member will hold in
        /// the destination group.
        /// </summary>
        public int? DestinationGroupRoleId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the member's notes move
        /// to the new group.
        /// </summary>
        public bool IsMoveNotesChecked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the member's fundraising
        /// financial transactions move to the new group.
        /// </summary>
        public bool IsMoveFundraisingTransactionsChecked { get; set; }
    }
}

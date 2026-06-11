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

using Rock.Enums.Communication.Chat;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Group.GroupTypeDetail
{
    /// <summary>
    /// Represents a group type role, including its edit-able settings and optional attributes.
    /// </summary>
    public class GroupTypeRoleBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the unique identifier of the role.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this role is a system role.
        /// </summary>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the name of the role.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the role.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the order of the role.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of members allowed for this role.
        /// </summary>
        public int? MaxCount { get; set; }

        /// <summary>
        /// Gets or sets the minimum number of members required for this role.
        /// </summary>
        public int? MinCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this role is a leader role.
        /// </summary>
        public bool IsLeader { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether members in this role can view the group.
        /// </summary>
        public bool CanView { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether members in this role can edit the group.
        /// </summary>
        public bool CanEdit { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether members in this role receive requirements notifications.
        /// </summary>
        public bool ReceiveRequirementsNotifications { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether members in this role can manage group members.
        /// </summary>
        public bool CanManageMembers { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether members in this role are excluded from the peer network.
        /// </summary>
        public bool IsExcludedFromPeerNetwork { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether members in this role are allowed for check-in.
        /// </summary>
        public bool IsCheckInAllowed { get; set; }

        /// <summary>
        /// Gets or sets the chat role for members in this role.
        /// </summary>
        public ChatRole ChatRole { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether members in this role can take attendance.
        /// </summary>
        public bool CanTakeAttendance { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this role is public.
        /// </summary>
        public bool IsPublic { get; set; }
    }
}



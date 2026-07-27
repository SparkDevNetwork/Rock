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

using Rock.Utility.Enums;

namespace Rock.ViewModels.Blocks.Group.GroupList
{
    /// <summary>
    /// Represents a single row in the Group List grid. Used for both GroupList mode
    /// (listing groups) and GroupsPersonMemberOf mode (listing a person's group memberships).
    /// Mode-specific fields are null or zero when not applicable to the current mode.
    /// </summary>
    public class GroupListRowBag
    {
        /// <summary>
        /// Gets or sets the integer Id of the group. Exposed as <c>Id</c> to match the
        /// field name used in WebForms custom Lava column templates (e.g. <c>{{Row.Id}}</c>).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the IdKey of the group. Used as the primary grid row key
        /// and for Detail Page navigation in both modes.
        /// </summary>
        public string GroupIdKey { get; set; }

        /// <summary>
        /// Gets or sets the integer Id of the group member record.
        /// Only populated in GroupsPersonMemberOf mode; null in GroupList mode.
        /// </summary>
        public int? GroupMemberId { get; set; }

        /// <summary>
        /// Gets or sets the IdKey of the group member record.
        /// Only populated in GroupsPersonMemberOf mode; null in GroupList mode.
        /// Passed to the DeleteMember block action when removing a membership.
        /// </summary>
        public string GroupMemberIdKey { get; set; }

        /// <summary>
        /// Gets or sets the display name of the group. In GroupList mode this may be
        /// prefixed with "GROUP - " when security-role groups are shown alongside other types.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the full ancestor path of the group (e.g. "Parent > Child > Group").
        /// Only populated when the DisplayGroupPath block attribute is enabled.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the integer Id of the group's GroupType.
        /// Used internally for ordering; not sent to the grid as a visible column.
        /// </summary>
        public int GroupTypeId { get; set; }

        /// <summary>
        /// Gets or sets the name of the group's GroupType, shown in the Group Type column.
        /// </summary>
        public string GroupTypeName { get; set; }

        /// <summary>
        /// Gets or sets the display order of the group's GroupType. Used for default grid sorting.
        /// </summary>
        public int GroupTypeOrder { get; set; }

        /// <summary>
        /// Gets or sets the display order of the group within its type. Used for default grid sorting.
        /// </summary>
        public int GroupOrder { get; set; }

        /// <summary>
        /// Gets or sets the group's description. Shown in the Description column or as a
        /// tooltip when the column is hidden.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is a system-defined group that
        /// cannot be deleted.
        /// </summary>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the group (and group membership in
        /// GroupsPersonMemberOf mode) is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the group or group membership is archived.
        /// Archived rows are rendered in a lighter style.
        /// </summary>
        public bool IsArchived { get; set; }

        /// <summary>
        /// Gets or sets the name of the person's role within the group.
        /// Only populated in GroupsPersonMemberOf mode; empty string in GroupList mode.
        /// </summary>
        public string GroupRole { get; set; }

        /// <summary>
        /// Gets or sets the elevated security level of the group. Used to show a
        /// High/Extreme security badge in GroupList mode for security-role groups.
        /// </summary>
        public ElevatedSecurityLevel ElevatedSecurityLevel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this group is a security role group.
        /// </summary>
        public bool IsSecurityRole { get; set; }

        /// <summary>
        /// Gets or sets the date the person was added to the group.
        /// Only populated in GroupsPersonMemberOf mode; null in GroupList mode.
        /// </summary>
        public DateTime? DateAdded { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this group (or the person's specific role
        /// in GroupsPersonMemberOf mode) is managed by a GroupSync. In GroupList mode this
        /// drives the sync icon in the separate sync column. In GroupsPersonMemberOf mode
        /// this disables the delete button for that membership row.
        /// </summary>
        public bool IsSynced { get; set; }

        /// <summary>
        /// Gets or sets the total number of group members. Only populated in GroupList mode;
        /// zero in GroupsPersonMemberOf mode.
        /// </summary>
        public int MemberCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the group has a linked chat channel.
        /// Only relevant in GroupList mode and used to include a warning in the delete confirmation.
        /// </summary>
        public bool HasChatChannel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current user is permitted to delete
        /// this row. In GroupList mode this requires EDIT authorization on the group and
        /// <see cref="IsSystem"/> being false. In GroupsPersonMemberOf mode this requires
        /// EDIT or MANAGE_MEMBERS authorization and the membership not being synced.
        /// </summary>
        public bool CanDelete { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the group (or membership in person mode)
        /// should be archived rather than hard-deleted. True when the GroupType has
        /// EnableGroupHistory set and history snapshot records exist.
        /// </summary>
        public bool NeedsArchive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the group's GroupType has group history
        /// enabled. Used in combination with history snapshot existence to compute
        /// <see cref="NeedsArchive"/>.
        /// </summary>
        public bool EnableGroupHistory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the "GROUP - " prefix should be prepended
        /// to <see cref="Name"/> when rendering. True when security-role groups appear
        /// alongside other group types in the same list.
        /// </summary>
        public bool UseRolePrefix { get; set; }
    }
}

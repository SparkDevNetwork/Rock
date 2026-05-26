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
using System.Collections.Generic;

using Rock.Enums.Group;
using Rock.Model;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Group.GroupDetail
{
    /// <summary>
    /// Per-GroupType options consumed by the edit panel. Returned both alongside the initial
    /// edit bag and from the <c>GetGroupTypeOptions</c> block action when the GroupType changes
    /// mid-edit. Carries every flag, dropdown source, and pin-by-group-type value the edit
    /// panel needs to reshape itself.
    /// </summary>
    public class GroupTypeOptionsBag
    {
        #region Section Visibility

        /// <summary>
        /// Gets or sets a value indicating whether the RSVP section renders.
        /// </summary>
        public bool IsRsvpSectionVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Chat section renders.
        /// </summary>
        public bool IsChatSectionVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the "Overall Group Schedule" stack inside
        /// the Meeting &amp; Scheduling section renders. Distinct from
        /// <see cref="IsSchedulingEnabled"/>, which drives the Member Scheduling stack.
        /// </summary>
        public bool IsOverallScheduleStackVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Peer Network override stack renders.
        /// </summary>
        public bool IsPeerNetworkSectionVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Group Administrator picker renders.
        /// </summary>
        public bool IsAdministratorVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Member Record Source picker renders.
        /// </summary>
        public bool IsGroupSpecificRecordSourceVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Who Can Check-in radio renders.
        /// </summary>
        public bool IsCheckInRequirementsVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Group Capacity field renders.
        /// </summary>
        public bool IsGroupCapacityVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Group Capacity field is required.
        /// </summary>
        public bool IsGroupCapacityRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Inactive Reason dropdown renders.
        /// </summary>
        public bool IsInactiveReasonVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Inactive Reason dropdown is required.
        /// </summary>
        public bool IsInactiveReasonRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Status defined-value picker renders.
        /// </summary>
        public bool IsStatusVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether selecting a Campus is required when saving
        /// a group of this type.
        /// </summary>
        public bool RequiresCampus { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the group type permits per-group attribute
        /// definitions for members.
        /// </summary>
        public bool AllowSpecificGroupMemberAttributes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the group type permits per-group group
        /// requirements.
        /// </summary>
        public bool EnableSpecificGroupRequirements { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the group type permits group sync rules.
        /// </summary>
        public bool AllowGroupSync { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the group type permits per-group member
        /// workflow triggers.
        /// </summary>
        public bool AllowSpecificGroupMemberWorkflows { get; set; }

        #endregion

        #region Allowed Flags

        /// <summary>
        /// Gets or sets the bitmask of <c>ScheduleType</c> values the group type permits.
        /// </summary>
        public ScheduleType AllowedScheduleTypes { get; set; }

        /// <summary>
        /// Gets or sets the location-selection mode for the group type. Drives the
        /// <c>&lt;LocationPicker&gt;</c>'s <c>allowedPickerModes</c> inside the Location modal.
        /// </summary>
        public GroupLocationPickerMode LocationSelectionMode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether per-location schedule configuration is enabled.
        /// </summary>
        public bool EnableLocationSchedules { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether scheduling is enabled at the group-type level.
        /// </summary>
        public bool IsSchedulingEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether more than one <c>GroupLocation</c> may be
        /// added to groups of this type. The Add button is shown only when
        /// <c>allowMultipleLocations || locations.length == 0</c>.
        /// </summary>
        public bool AllowMultipleLocations { get; set; }

        #endregion

        #region Localization

        /// <summary>
        /// Gets or sets the localized term for the group administrator (default: "Administrator").
        /// </summary>
        public string AdministratorTerm { get; set; }

        #endregion

        #region Peer-Network Defaults

        /// <summary>
        /// Gets or sets the group-type's relationship strength default, shown when the group
        /// has no override.
        /// </summary>
        public RelationshipStrength RelationshipStrengthDefault { get; set; }

        /// <summary>
        /// Gets or sets the group-type's relationship-growth-enabled default, used when the
        /// group has no override.
        /// </summary>
        public bool RelationshipGrowthEnabledDefault { get; set; }

        /// <summary>
        /// Gets or sets the group-type's leader-to-leader multiplier as a 0-1 decimal.
        /// </summary>
        public decimal LeaderToLeaderMultiplierDefault { get; set; }

        /// <summary>
        /// Gets or sets the group-type's leader-to-non-leader multiplier as a 0-1 decimal.
        /// </summary>
        public decimal LeaderToNonLeaderMultiplierDefault { get; set; }

        /// <summary>
        /// Gets or sets the group-type's non-leader-to-leader multiplier as a 0-1 decimal.
        /// </summary>
        public decimal NonLeaderToLeaderMultiplierDefault { get; set; }

        /// <summary>
        /// Gets or sets the group-type's non-leader-to-non-leader multiplier as a 0-1 decimal.
        /// </summary>
        public decimal NonLeaderToNonLeaderMultiplierDefault { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the group type has any non-100% relationship
        /// multipliers configured. Drives the initial "Show Advanced Relationship Settings"
        /// toggle so the multiplier matrix auto-opens when inherited values are non-trivial.
        /// </summary>
        public bool AreAnyRelationshipMultipliersCustomized { get; set; }

        #endregion

        #region Pinned Per-Group Overrides

        /// <summary>
        /// Gets or sets the group-type's pinned RSVP reminder offset days. Non-null means the
        /// group-type pins the value and the per-group override is read-only.
        /// </summary>
        public int? RsvpReminderOffsetDays { get; set; }

        /// <summary>
        /// Gets or sets the group-type's pinned RSVP reminder system communication. Non-null
        /// means the group-type pins the value and the per-group override is read-only.
        /// </summary>
        public ListItemBag RsvpReminderSystemCommunication { get; set; }

        #endregion

        #region Dropdown Sources

        /// <summary>
        /// Gets or sets the list of <c>DefinedValue</c> rows under
        /// <c>GroupType.GroupStatusDefinedType</c>, used to populate the Status dropdown.
        /// </summary>
        public List<ListItemBag> StatusValues { get; set; }

        /// <summary>
        /// Gets or sets the list of allowed inactive reason <c>DefinedValue</c> rows for this
        /// group type.
        /// </summary>
        public List<ListItemBag> InactiveReasons { get; set; }

        /// <summary>
        /// Gets or sets the group role dropdown options used by the Group Requirement,
        /// Group Sync, and Member Workflow modals.
        /// </summary>
        public List<ListItemBag> GroupRoleOptions { get; set; }

        /// <summary>
        /// Gets or sets the Location Type DefinedValue dropdown options scoped to
        /// <c>GroupType.LocationTypeValues</c>.
        /// </summary>
        public List<ListItemBag> LocationTypeValueOptions { get; set; }

        #endregion

        #region Inherited Grids

        /// <summary>
        /// Gets or sets the inherited group-member attribute definitions for the active group
        /// type. Each entry carries the immediate ancestor's name + URL for the "Inherited
        /// Attributes" grid.
        /// </summary>
        public List<GroupMemberInheritedAttributeBag> InheritedMemberAttributes { get; set; }

        /// <summary>
        /// Gets or sets the read-only inherited group-requirement rows for the current group
        /// type. Each row carries its own <c>InheritedFromGroupTypeName</c> /
        /// <c>InheritedFromGroupTypeUrl</c> so the grid can render per-row inheritance links.
        /// </summary>
        public List<InheritedGroupRequirementBag> InheritedGroupRequirements { get; set; }

        #endregion
    }
}

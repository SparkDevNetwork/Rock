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

using Rock.Enums.Communication.Chat;
using Rock.Enums.Group;
using Rock.Model;
using Rock.Utility.Enums;
using Rock.ViewModels.Utility;

using RelationshipStrength = Rock.Enums.Group.RelationshipStrength;

namespace Rock.ViewModels.Blocks.Group.GroupDetail
{
    /// <summary>
    /// The bag returned by the Group Detail block.
    /// </summary>
    public class GroupBag : EntityBagBase
    {
        #region State

        /// <summary>
        /// Gets or sets a value indicating whether the group is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the group is archived.
        /// </summary>
        public bool IsArchived { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the group is a system
        /// group.
        /// </summary>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current user has
        /// EDIT authorization on the group.
        /// </summary>
        public bool CanEdit { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current user has
        /// ADMINISTRATE authorization on the group.
        /// </summary>
        public bool CanAdministrate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the group has any
        /// child groups. Drives the archive-with-children prompt.
        /// </summary>
        public bool HasChildGroups { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether any active descendants
        /// exist. Drives visibility of the "Also Inactivate Child Groups"
        /// checkbox.
        /// </summary>
        public bool HasActiveDescendants { get; set; }

        /// <summary>
        /// Gets or sets the count of currently-active members of the
        /// group. Drives the capacity-below-members warning.
        /// </summary>
        public int MemberCount { get; set; }

        #endregion

        #region Top Fields

        /// <summary>
        /// Gets or sets the friendly name of the group.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the inactive-reason DefinedValue Id. Null when
        /// the group is active or no reason was chosen.
        /// </summary>
        public int? InactiveReasonValueId { get; set; }

        /// <summary>
        /// Gets or sets the free-text inactive note.
        /// </summary>
        public string InactiveReasonNote { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the inactive flag
        /// should cascade to all active descendants on save.
        /// </summary>
        public bool InactivateChildGroups { get; set; }

        /// <summary>
        /// Gets or sets the group's description text.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets how this group meets (in person, online, or hybrid), or null when unspecified.
        /// Only editable when the group's type has meeting style enabled.
        /// </summary>
        public MeetingStyle? MeetingStyle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the group is publicly
        /// visible.
        /// </summary>
        public bool IsPublic { get; set; }

        /// <summary>
        /// Gets or sets the BinaryFile reference for the group's hero
        /// image, or null when the group has no photo set.
        /// </summary>
        public ListItemBag PhotoBinaryFile { get; set; }

        #endregion

        #region Overview

        /// <summary>
        /// Gets or sets the group type Id. Required on Add; read-only on
        /// existing groups.
        /// </summary>
        public int? GroupTypeId { get; set; }

        /// <summary>
        /// Gets or sets the parent group reference, or null when the
        /// group has no parent.
        /// </summary>
        public ParentGroupBag ParentGroup { get; set; }

        /// <summary>
        /// Gets or sets the campus selection, or null when no campus is
        /// selected.
        /// </summary>
        public ListItemBag Campus { get; set; }

        /// <summary>
        /// Gets or sets the status DefinedValue Id, or null when no
        /// status is set.
        /// </summary>
        public int? StatusValueId { get; set; }

        #endregion

        #region Administration and Security

        /// <summary>
        /// Gets or sets the group administrator reference. Null when no
        /// administrator is set or when the group type hides the row.
        /// </summary>
        public GroupAdministratorBag Administrator { get; set; }

        /// <summary>
        /// Gets or sets the administrator field label, composed from the group type's
        /// group term and administrator term (e.g. "Group Administrator", "Group Director").
        /// </summary>
        public string AdministratorLabel { get; set; }

        /// <summary>
        /// Gets or sets the group's capacity, or null when not configured.
        /// </summary>
        public int? GroupCapacity { get; set; }

        /// <summary>
        /// Gets or sets the Required Signature Document Template Id, or
        /// null when no template is required.
        /// </summary>
        public int? RequiredSignatureDocumentTemplateId { get; set; }

        /// <summary>
        /// Gets or sets the member record source DefinedValue.
        /// </summary>
        public ListItemBag GroupMemberRecordSource { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the group is enabled
        /// as a security role. Editable only by members of the
        /// GROUP_ADMINISTRATORS system group.
        /// </summary>
        public bool IsSecurityRole { get; set; }

        /// <summary>
        /// Gets or sets the elevated-security level applied when the
        /// group is acting as a security role.
        /// </summary>
        public ElevatedSecurityLevel ElevatedSecurityLevel { get; set; }

        #endregion

        #region Relationships

        /// <summary>
        /// Gets or sets a value indicating whether the user has chosen
        /// to override the group type's peer-network configuration.
        /// </summary>
        public bool OverrideRelationshipStrength { get; set; }

        /// <summary>
        /// Gets or sets the relationship-strength override, or null to
        /// inherit from the group type.
        /// </summary>
        public RelationshipStrength? RelationshipStrengthOverride { get; set; }

        /// <summary>
        /// Gets or sets the relationship-growth-enabled override, or
        /// null to inherit from the group type.
        /// </summary>
        public bool? RelationshipGrowthEnabledOverride { get; set; }

        /// <summary>
        /// Gets or sets the leader-to-leader relationship multiplier
        /// override, or null to inherit from the group type.
        /// </summary>
        public decimal? LeaderToLeaderRelationshipMultiplierOverride { get; set; }

        /// <summary>
        /// Gets or sets the leader-to-non-leader relationship multiplier
        /// override, or null to inherit from the group type.
        /// </summary>
        public decimal? LeaderToNonLeaderRelationshipMultiplierOverride { get; set; }

        /// <summary>
        /// Gets or sets the non-leader-to-leader relationship multiplier
        /// override, or null to inherit from the group type.
        /// </summary>
        public decimal? NonLeaderToLeaderRelationshipMultiplierOverride { get; set; }

        /// <summary>
        /// Gets or sets the non-leader-to-non-leader relationship
        /// multiplier override, or null to inherit from the group type.
        /// </summary>
        public decimal? NonLeaderToNonLeaderRelationshipMultiplierOverride { get; set; }

        #endregion

        #region RSVP

        /// <summary>
        /// Gets or sets the RSVP reminder system communication override,
        /// or null to inherit from the group type.
        /// </summary>
        public ListItemBag RsvpReminderSystemCommunication { get; set; }

        /// <summary>
        /// Gets or sets the RSVP reminder offset days override, or null
        /// to inherit from the group type.
        /// </summary>
        public int? RsvpReminderOffsetDays { get; set; }

        #endregion

        #region Overall Group Schedule

        /// <summary>
        /// Gets or sets the schedule type selection (None, Weekly,
        /// Custom, or Named).
        /// </summary>
        public ScheduleType ScheduleType { get; set; }

        /// <summary>
        /// Gets or sets the day-of-week selected for a Weekly schedule.
        /// Stored as <see cref="int"/> because the Obsidian code-gen
        /// tool has no TypeScript mapping for <see cref="DayOfWeek"/>;
        /// values align with the enum members (Sunday = 0).
        /// </summary>
        public int? WeeklyDayOfWeek { get; set; }

        /// <summary>
        /// Gets or sets the time-of-day for a Weekly schedule, as an
        /// ISO-8601 time string ("HH:mm:ss").
        /// </summary>
        public string WeeklyTimeOfDay { get; set; }

        /// <summary>
        /// Gets or sets the iCalendar content for a Custom schedule.
        /// </summary>
        public string ICalendarContent { get; set; }

        /// <summary>
        /// Gets or sets the selected named Schedule.
        /// </summary>
        public ListItemBag NamedSchedule { get; set; }

        #endregion

        #region Group Locations

        /// <summary>
        /// Gets or sets the editable per-group meeting locations.
        /// </summary>
        public List<GroupLocationStateBag> GroupLocations { get; set; }

        #endregion

        #region Member Scheduling and Check-In

        /// <summary>
        /// Gets or sets a value indicating whether group-member
        /// scheduling is disabled at the group level.
        /// </summary>
        public bool DisableScheduling { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the group is hidden
        /// from the Schedule Toolbox.
        /// </summary>
        public bool DisableScheduleToolboxAccess { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether members must meet
        /// requirements to be schedulable.
        /// </summary>
        public bool SchedulingMustMeetRequirements { get; set; }

        /// <summary>
        /// Gets or sets the schedule confirmation logic, or null to
        /// inherit from the group type.
        /// </summary>
        public ScheduleConfirmationLogic? ScheduleConfirmationLogic { get; set; }

        /// <summary>
        /// Gets or sets the schedule coordinator person.
        /// </summary>
        public ListItemBag ScheduleCoordinatorPerson { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the group explicitly
        /// overrides the group type's coordinator notification settings.
        /// When false, <see cref="ScheduleCoordinatorNotificationTypes"/>
        /// is ignored and the entity inherits from the group type.
        /// </summary>
        public bool HasCoordinatorNotificationOverride { get; set; }

        /// <summary>
        /// Gets or sets the bitmask of coordinator notification types.
        /// Persisted only when
        /// <see cref="HasCoordinatorNotificationOverride"/> is true.
        /// </summary>
        public ScheduleCoordinatorNotificationType ScheduleCoordinatorNotificationTypes { get; set; }

        /// <summary>
        /// Gets or sets the attendance-record-required-for-check-in
        /// behavior.
        /// </summary>
        public AttendanceRecordRequiredForCheckIn AttendanceRecordRequiredForCheckIn { get; set; }

        #endregion

        #region Member Attributes

        /// <summary>
        /// Gets or sets the editable per-group member attribute
        /// definitions.
        /// </summary>
        public List<PublicEditableAttributeBag> GroupMemberAttributes { get; set; }

        #endregion

        #region Group Requirements

        /// <summary>
        /// Gets or sets the per-group requirements.
        /// </summary>
        public List<GroupRequirementBag> GroupRequirements { get; set; }

        #endregion

        #region Chat

        /// <summary>
        /// Gets or sets the chat-enabled override (null = inherit).
        /// </summary>
        public bool? IsChatEnabledOverride { get; set; }

        /// <summary>
        /// Gets or sets the leaving-chat-channel-allowed override (null
        /// = inherit).
        /// </summary>
        public bool? IsLeavingChatChannelAllowedOverride { get; set; }

        /// <summary>
        /// Gets or sets the public-channel override (null = inherit).
        /// </summary>
        public bool? IsChatChannelPublicOverride { get; set; }

        /// <summary>
        /// Gets or sets the always-show override (null = inherit).
        /// </summary>
        public bool? IsChatChannelAlwaysShownOverride { get; set; }

        /// <summary>
        /// Gets or sets the chat push-notification-mode override (null
        /// = inherit).
        /// </summary>
        public ChatNotificationMode? ChatPushNotificationModeOverride { get; set; }

        /// <summary>
        /// Gets or sets the BinaryFile reference for the chat-channel
        /// avatar.
        /// </summary>
        public ListItemBag ChatChannelAvatarBinaryFile { get; set; }

        /// <summary>
        /// Gets or sets the effective chat-enabled state for view mode.
        /// </summary>
        public bool IsChatEnabled { get; set; }

        #endregion

        #region Group Sync

        /// <summary>
        /// Gets or sets the per-group sync rules.
        /// </summary>
        public List<GroupSyncBag> GroupSyncs { get; set; }

        #endregion

        #region Group Member Workflows

        /// <summary>
        /// Gets or sets the per-group member workflow triggers.
        /// </summary>
        public List<GroupMemberWorkflowTriggerBag> GroupMemberWorkflowTriggers { get; set; }

        #endregion

        #region View-Mode Display

        /// <summary>
        /// Gets or sets the icon CSS class sourced from the group type.
        /// </summary>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the group type reference shown as a chip in the
        /// panel header.
        /// </summary>
        public GroupDetailGroupTypeBag GroupType { get; set; }

        /// <summary>
        /// Gets or sets the campus name, or null when the group has no
        /// campus assigned.
        /// </summary>
        public string CampusName { get; set; }

        /// <summary>
        /// Gets or sets the URL of the hero image rendered at the top of
        /// the Overview card. Null when the group has no photo set.
        /// </summary>
        public string PhotoUrl { get; set; }

        /// <summary>
        /// Gets or sets the effective relationship strength shown in the
        /// subheader chip, or null when the group type lacks peer-network
        /// support.
        /// </summary>
        public RelationshipStrength? RelationshipStrength { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the group overrides
        /// any of its group type's peer-network configuration. Drives
        /// the asterisk indicator on the Relationship Strength chip.
        /// </summary>
        public bool IsOverridingPeerNetwork { get; set; }

        /// <summary>
        /// Gets or sets the friendly schedule text for the group's
        /// primary schedule.
        /// </summary>
        public string ScheduleFriendlyText { get; set; }

        /// <summary>
        /// Gets or sets the linkages section content (registrations,
        /// event item occurrences, content items), or null when the
        /// group has no linkages.
        /// </summary>
        public GroupLinkagesBag Linkages { get; set; }

        /// <summary>
        /// Gets or sets the meeting-location cards rendered on the View
        /// panel.
        /// </summary>
        public List<GroupMeetingLocationBag> MeetingLocations { get; set; }

        /// <summary>
        /// Gets or sets the role-limit warning HTML rendered in the view
        /// panel when one or more roles violate their min/max counts.
        /// </summary>
        public string RoleLimitWarning { get; set; }

        #endregion
    }
}

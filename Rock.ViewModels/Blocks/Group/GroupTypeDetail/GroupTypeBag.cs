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

using Rock.ViewModels.Utility;
using Rock.Enums.CheckIn;
using Rock.Model;
using Rock.Enums.Communication.Chat;
using Rock.Enums.Group;

namespace Rock.ViewModels.Blocks.Group.GroupTypeDetail
{
    /// <summary>
    /// The item details for the Group Type Detail block.
    /// </summary>
    public class GroupTypeBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the Guid of the GroupType.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the administrator term for the group of this GroupType.
        /// </summary>
        public string AdministratorTerm { get; set; }

        /// <summary>
        ///  Gets or sets the allowed schedule types.
        /// </summary>
        public ScheduleType AllowedScheduleTypes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if group type allows any child group type.
        /// </summary>
        public bool AllowAnyChildGroupType { get; set; }

        /// <summary>
        /// Gets or sets whether Rock.Model.Groups of this type can override Rock.Model.GroupType.GroupMemberRecordSourceValueId.
        /// </summary>
        public bool AllowGroupSpecificRecordSource { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if groups of this type are allowed to be sync'ed.
        /// </summary>
        public bool AllowGroupSync { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if Groups of this type are allowed to have multiple locations.
        /// </summary>
        public bool AllowMultipleLocations { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if specific groups are allowed to have their own member attributes.
        /// </summary>
        public bool AllowSpecificGroupMemberAttributes { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if groups of this type should be allowed to have Group Member Workflows.
        /// </summary>
        public bool AllowSpecificGroupMemberWorkflows { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [attendance counts as weekend service].
        /// </summary>
        public bool AttendanceCountsAsWeekendService { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.PrintTo indicating the type of  location of where attendee labels for Groups of this GroupType should print.
        /// </summary>
        public PrintTo AttendancePrintTo { get; set; }

        /// <summary>
        /// Gets or sets the attendance reminder followup days list.
        /// </summary>
        public List<int> AttendanceReminderFollowupDays { get; set; }

        /// <summary>
        /// Gets or sets the attendance reminder send start offset minutes.
        /// </summary>
        public int? AttendanceReminderSendStartOffsetMinutes { get; set; }

        /// <summary>
        /// Gets or sets the attendance reminder system communication.
        /// </summary>
        public ListItemBag AttendanceReminderSystemCommunication { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.AttendanceRule that indicates how attendance is managed a Rock.Model.Group of this GroupType
        /// </summary>
        public AttendanceRule AttendanceRule { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Enums.Communication.Chat.ChatNotificationMode to control how push notifications are sent for chat
        /// channels of this type. This can be overridden by the value of Rock.Model.Group.ChatPushNotificationModeOverride.
        /// </summary>
        public ChatNotificationMode ChatPushNotificationMode { get; set; }

        /// <summary>
        /// Gets or sets the collection of GroupTypes that inherit from this GroupType.
        /// </summary>
        public List<ListItemBag> ChildGroupTypes { get; set; }

        /// <summary>
        /// Gets or sets the default Rock.Model.GroupTypeRole for GroupMembers who belong to a
        /// Rock.Model.Group of this GroupType.
        /// </summary>
        public ListItemBag DefaultGroupRole { get; set; }

        /// <summary>
        /// Gets or sets the Description of the GroupType.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether group history should be enabled for groups of this type
        /// </summary>
        public bool EnableGroupHistory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether group tag should be enabled for groups of this type
        /// </summary>
        public bool EnableGroupTag { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable inactive reason].
        /// </summary>
        public bool EnableInactiveReason { get; set; }

        /// <summary>
        /// Gets or sets the enable location schedules.
        /// </summary>
        public bool? EnableLocationSchedules { get; set; }

        /// <summary>
        /// Indicates whether RSVP functionality should be enabled for this group.
        /// </summary>
        public bool EnableRSVP { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if group requirements section is enabled for group of this type.
        /// </summary>
        public bool EnableSpecificGroupRequirements { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [group attendance requires location].
        /// </summary>
        public bool GroupAttendanceRequiresLocation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [group attendance requires schedule].
        /// </summary>
        public bool GroupAttendanceRequiresSchedule { get; set; }

        /// <summary>
        /// Gets or sets the group capacity rule.
        /// </summary>
        public GroupCapacityRule GroupCapacityRule { get; set; }

        /// <summary>
        /// Gets or sets the group member workflow triggers for this group type.
        /// </summary>
        public List<GroupTypeGroupMemberWorkflowTriggerBag> GroupMemberWorkflowTriggers { get; set; }

        /// <summary>
        /// Gets or sets the default Record Source Type Rock.Model.DefinedValue, representing the source of
        /// Rock.Model.GroupMembers added to Rock.Model.Groups of this type. This can be overridden by
        /// Rock.Model.Group.GroupMemberRecordSourceValue if Rock.Model.GroupType.AllowGroupSpecificRecordSource is
        /// .
        /// </summary>
        public ListItemBag GroupMemberRecordSourceValue { get; set; }

        /// <summary>
        /// Gets or sets the term that a Rock.Model.GroupMember of a Rock.Model.Group that belongs to this GroupType is called.
        /// </summary>
        public string GroupMemberTerm { get; set; }

        /// <summary>
        /// Gets or sets the group requirements for groups of this Group Type (NOTE: Groups also can have additional GroupRequirements )
        /// </summary>
        public List<GroupTypeGroupRequirementBag> GroupRequirements { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [groups require campus].
        /// </summary>
        public bool GroupsRequireCampus { get; set; }

        /// <summary>
        /// Gets or sets the DefinedType that Groups of this type will use for the Group.StatusValue
        /// </summary>
        public ListItemBag GroupStatusDefinedType { get; set; }

        /// <summary>
        /// Gets or sets the term that a Rock.Model.Group belonging to this Rock.Model.GroupType is called.
        /// </summary>
        public string GroupTerm { get; set; }

        /// <summary>
        /// The color used to visually distinguish groups on lists.
        /// </summary>
        public string GroupTypeColor { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.DefinedValue that represents the purpose of the GroupType.
        /// </summary>
        public ListItemBag GroupTypePurposeValue { get; set; }

        /// <summary>
        /// Gets or sets a lava template that can be used for generating  view details for Group.
        /// </summary>
        public string GroupViewLavaTemplate { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class name for a font vector based icon.
        /// </summary>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to ignore person inactivated.
        /// By default group members are inactivated in their group whenever the person
        /// is inactivated. If this value is set to true, members in groups of this type
        /// will not be marked inactive when the person is inactivated
        /// </summary>
        public bool IgnorePersonInactivated { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.GroupType that this GroupType is inheriting settings and properties from.
        /// This is similar to a parent or a template GroupType.
        /// </summary>
        public ListItemBag InheritedGroupType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is capacity required.
        /// </summary>
        public bool IsCapacityRequired { get; set; }

        /// <summary>
        /// Gets or sets whether groups of this type are allowed to participate in the chat system as a chat channel.
        /// </summary>
        public bool IsChatAllowed { get; set; }

        /// <summary>
        /// Gets or sets whether chat channels of this type are always shown in the channel list even if the person has
        /// not joined the channel. This also implies that the channel may be joined by any person via the chat
        /// application. This can be overridden by the value of Rock.Model.Group.IsChatChannelAlwaysShownOverride.
        /// </summary>
        public bool IsChatChannelAlwaysShown { get; set; }

        /// <summary>
        /// Gets or sets whether chat channels of this type are public. A public channel is visible to everyone when
        /// performing a search. This also implies that the channel may be joined by any person via the chat application.
        /// This can be overridden by the value of Rock.Model.Group.IsChatChannelPublicOverride.
        /// </summary>
        public bool IsChatChannelPublic { get; set; }

        /// <summary>
        /// Gets or sets whether all groups of this type have the chat feature enabled by default. This can be overridden
        /// by the value of Rock.Model.Group.IsChatEnabledOverride.
        /// </summary>
        public bool IsChatEnabledForAllGroups { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is index enabled.
        /// </summary>
        public bool IsIndexEnabled { get; set; }

        /// <summary>
        /// Gets or sets whether individuals are allowed to leave chat channels of this type. If set to
        /// , then they will only be allowed to mute the channel. This can be overridden by the
        /// value of Rock.Model.Group.IsLeavingChatChannelAllowedOverride.
        /// </summary>
        public bool IsLeavingChatChannelAllowed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Group Type has Peer Network enabled.
        /// </summary>
        public bool IsPeerNetworkEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether scheduling is enabled for groups of this type
        /// </summary>
        public bool IsSchedulingEnabled { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this GroupType is part of the Rock core system/framework.  This property is required.
        /// </summary>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the leader to leader relationship multiplier.
        /// </summary>
        public decimal LeaderToLeaderRelationshipMultiplier { get; set; }

        /// <summary>
        /// Gets or sets the leader to non leader relationship multiplier.
        /// </summary>
        public decimal LeaderToNonLeaderRelationshipMultiplier { get; set; }

        /// <summary>
        /// Gets or sets the Location Selection Mode.
        /// </summary>
        public GroupLocationPickerMode LocationSelectionMode { get; set; }

        /// <summary>
        /// Gets or sets a collection of the GroupTypeLocationTypes that are associated with this GroupType.
        /// </summary>
        public List<ListItemBag> LocationTypes { get; set; }

        /// <summary>
        /// Gets or sets the Name of the GroupType. This property is required.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the non leader to leader relationship multiplier.
        /// </summary>
        public decimal NonLeaderToLeaderRelationshipMultiplier { get; set; }

        /// <summary>
        /// Gets or sets the non leader to non leader relationship multiplier.
        /// </summary>
        public decimal NonLeaderToNonLeaderRelationshipMultiplier { get; set; }

        /// <summary>
        /// Gets or sets the order for this GroupType. This is used for display and priority purposes, the lower the number the higher the priority, or the higher the GroupType is displayed. This property is required.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether relationship growth is enabled.
        /// </summary>
        public bool RelationshipGrowthEnabled { get; set; }

        /// <summary>
        /// Gets or sets the relationship strength.
        /// </summary>
        public int RelationshipStrength { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [requires inactive reason].
        /// </summary>
        public bool RequiresInactiveReason { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a person must specify a reason when declining/cancelling.
        /// </summary>
        public bool RequiresReasonIfDeclineSchedule { get; set; }

        /// <summary>
        /// Gets or sets the group type roles as fully-described bags.
        /// </summary>
        public List<GroupTypeRoleBag> Roles { get; set; }

        /// <summary>
        /// Gets or sets the number of days prior to the RSVP date that a reminder should be sent.
        /// </summary>
        public int? RSVPReminderOffsetDays { get; set; }

        /// <summary>
        /// Gets or sets the system communication to use for sending an RSVP reminder.
        /// </summary>
        public ListItemBag RSVPReminderSystemCommunication { get; set; }

        /// <summary>
        /// Gets or sets the WorkflowType to execute when a person indicates they won't be able to attend at their scheduled time
        /// </summary>
        public ListItemBag ScheduleCancellationWorkflowType { get; set; }

        /// <summary>
        /// Gets or sets the number of days prior to the schedule to send a confirmation email.
        /// </summary>
        public int? ScheduleConfirmationEmailOffsetDays { get; set; }

        /// <summary>
        /// Gets or sets the schedule confirmation logic.
        /// </summary>
        public ScheduleConfirmationLogic ScheduleConfirmationLogic { get; set; }

        /// <summary>
        /// Gets or sets the system communication to use when a person is scheduled or when the schedule has been updated
        /// </summary>
        public ListItemBag ScheduleConfirmationSystemCommunication { get; set; }

        /// <summary>
        /// Gets or sets the types of notifications the coordinator receives about scheduled individuals.
        /// </summary>
        public ScheduleCoordinatorNotificationType? ScheduleCoordinatorNotificationTypes { get; set; }

        /// <summary>
        /// Gets or sets the number of days prior to the schedule to send a reminder email. See also Rock.Model.GroupMember.ScheduleReminderEmailOffsetDays.
        /// </summary>
        public int? ScheduleReminderEmailOffsetDays { get; set; }

        /// <summary>
        /// Gets or sets the system communication to use when sending a Schedule Reminder
        /// </summary>
        public ListItemBag ScheduleReminderSystemCommunication { get; set; }

        /// <summary>
        /// Gets or sets the schedule exclusion date ranges for this group type.
        /// </summary>
        public List<GroupTypeGroupScheduleExclusionBag> ScheduleExclusions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if an attendance reminder should be sent to group leaders.
        /// </summary>
        public bool SendAttendanceReminder { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether administrator for the group of this GroupType will be shown.
        /// </summary>
        public bool ShowAdministrator { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show the Person's connection status as a column in the Group Member Grid
        /// </summary>
        public bool ShowConnectionStatus { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if a Rock.Model.Group of this GroupType will be shown in the group list.
        /// </summary>
        public bool ShowInGroupList { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this GroupType and its Groups are shown in Navigation.
        /// If false, this GroupType will be hidden navigation controls, such as TreeViews and Menus
        /// </summary>
        public bool ShowInNavigation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show the Person's marital status as a column in the Group Member Grid
        /// </summary>
        public bool ShowMaritalStatus { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if a Rock.Model.Group of this GroupType supports taking attendance.
        /// </summary>
        public bool TakesAttendance { get; set; }

        /// <summary>
        /// Gets or sets the group attributes.
        /// </summary>
        public List<PublicEditableAttributeBag> GroupAttributes { get; set; }

        /// <summary>
        /// Gets or sets the group member attributes.
        /// </summary>
        public List<PublicEditableAttributeBag> GroupMemberAttributes { get; set; }

        /// <summary>
        /// Gets or sets the group type attributes.
        /// </summary>
        public List<PublicEditableAttributeBag> GroupTypeAttributes { get; set; }
    }
}

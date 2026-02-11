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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Communication.Chat;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.UniversalSearch;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Group.GroupTypeDetail;
using Rock.ViewModels.Utility;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.Group
{
    /// <summary>
    /// Displays the details of a particular group type.
    /// </summary>

    [DisplayName( "Group Type Detail" )]
    [Category( "Groups" )]
    [Description( "Displays the details of a particular group type." )]
    [IconCssClass( "ti ti-question-mark" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [BooleanField(
        "Enable Group View Lava Template",
        DefaultBooleanValue = false,
        Description = "This Lava template will be used by the Group Details block when viewing a group. This allows you to customize the layout of a group base on its type.",
        IsRequired = false,
        Key = AttributeKey.EnableGroupViewLavaTemplate,
        Order = 0 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "B207EC12-AF99-49C1-89FA-9E47556163A7" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "033A25B1-02ED-4AEE-8A73-33D7DDC9CBB5" )]
    [Rock.SystemGuid.BlockTypeGuid( "78B8EE69-71A7-43C1-B00B-ED13828FE104" )]
    public class GroupTypeDetail : RockEntityDetailBlockType<GroupType, GroupTypeBag>, IBreadCrumbBlock
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string GroupTypeId = "GroupTypeId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        private static class AttributeKey
        {
            public const string EnableGroupViewLavaTemplate = "EnableGroupViewLavaTemplate";
        }

        private static class EntityKey
        {
            public const string GroupTypeRole = "GroupTypeRole";
            public const string ScheduleExclusion = "ScheduleExclusion";
            public const string GroupRequirement = "GroupRequirement";
            public const string GroupMemberWorkflowTrigger = "GroupMemberWorkflowTrigger";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<GroupTypeBag, GroupTypeDetailOptionsBag>();

            SetBoxInitialEntityState( box );

            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        private GroupTypeDetailOptionsBag GetBoxOptions()
        {
            var systemCommunicationOptions = GetSystemCommunicationOptions( out var rsvpSystemCommunicationOptions );

            var options = new GroupTypeDetailOptionsBag()
            {
                EnableGroupViewLavaTemplate = GetAttributeValue( AttributeKey.EnableGroupViewLavaTemplate ).AsBoolean(),
                IsChatEnabledSystem = ChatHelper.IsChatEnabled,
                GroupRequirementTypeOptions = new GroupRequirementTypeService( RockContext ).Queryable()
                    .OrderBy( req => req.Name )
                    .Select( req => new GroupRequirementTypeBag
                    {
                        Text = req.Name,
                        Value = req.Guid.ToString(),
                        DueDateType = req.DueDateType
                    } )
                    .ToList(),
                DefinedTypeOptions = GetDefinedTypeOptions(),
                SystemCommunicationOptions = systemCommunicationOptions,
                RSVPSystemCommunicationOptions = rsvpSystemCommunicationOptions
            };

            options.IsIndexingOptionAvailable = IndexContainer.IndexingEnabled;

            var key = PageParameter( PageParameterKey.GroupTypeId );
            var groupTypeId = new GroupTypeService( RockContext ).GetSelect( key, gt => ( int? ) gt.Id );

            if ( groupTypeId.HasValue && groupTypeId.Value > 0 )
            {
                options.HasHistoricalRecords = new GroupHistoricalService( RockContext ).Queryable().Any( a => a.GroupTypeId == groupTypeId.Value ) ||
                        new GroupMemberHistoricalService( RockContext ).Queryable().Any( a => a.Group.GroupTypeId == groupTypeId.Value );
            }

            return options;
        }

        /// <summary>
        /// Validates the GroupType for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="groupType">The GroupType to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the GroupType is valid, <c>false</c> otherwise.</returns>
        private bool ValidateGroupType( GroupType groupType, GroupTypeBag bag, out string errorMessage )
        {
            errorMessage = null;

            if ( bag != null )
            {
                /*
                    1/23/2026 - MSE

                    Prevent a GroupTypeRole from being deleted if it is still referenced by
                    Group Requirements or Group Member Workflow Triggers. Deleting a role
                    that is still in use would leave orphaned configuration and cause
                    unexpected behavior.

                    The UI already includes logic to prevent this, but this provides an additional safeguard.
                */
                var roleGuids = new HashSet<Guid>(
                    ( bag.Roles ?? new List<GroupTypeRoleBag>() ).Select( r => r.Guid )
                );

                // Prevent duplicate Group Requirements (same requirement type + same role).
                var requirementKeys = new HashSet<(Guid RequirementTypeGuid, Guid? RoleGuid)>();

                foreach ( var req in bag.GroupRequirements ?? new List<GroupTypeGroupRequirementBag>() )
                {
                    var requirementTypeGuid = req.GroupRequirementType?.Value.AsGuidOrNull();
                    if ( !requirementTypeGuid.HasValue )
                    {
                        continue;
                    }

                    var roleGuid = req.Role?.Value.AsGuidOrNull();

                    if ( !requirementKeys.Add( (requirementTypeGuid.Value, roleGuid) ) )
                    {
                        errorMessage = $"The Group Requirement '{req.GroupRequirementType?.Text}' has been configured more than once for the same role.";
                        return false;
                    }

                    if ( roleGuid.HasValue && !roleGuids.Contains( roleGuid.Value ) )
                    {
                        errorMessage = $"The role '{req.Role.Text}' cannot be deleted because it is used by a Group Requirement.";
                        return false;
                    }
                }

                // Check Workflow Triggers
                if ( bag.GroupMemberWorkflowTriggers != null )
                {
                    foreach ( var trigger in bag.GroupMemberWorkflowTriggers )
                    {
                        if ( trigger.ToRoleGuid.HasValue && !roleGuids.Contains( trigger.ToRoleGuid.Value ) )
                        {
                            errorMessage = $"The role associated with the '{trigger.Name}' trigger cannot be deleted because it is in use.";
                            return false;
                        }

                        if ( trigger.FromRoleGuid.HasValue && !roleGuids.Contains( trigger.FromRoleGuid.Value ) )
                        {
                            errorMessage = $"The role associated with the '{trigger.Name}' trigger cannot be deleted because it is in use.";
                            return false;
                        }
                    }
                }
            }

            if ( groupType.IsSchedulingEnabled && groupType.AllowedScheduleTypes == ScheduleType.None )
            {
                errorMessage = "A 'Group Schedule Option' must be selected under 'Attendance / Check-In' when Scheduling is enabled.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<GroupTypeBag, GroupTypeDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {GroupType.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = entity.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            if ( entity.Id != 0 )
            {
                // Existing entity was found, prepare for view mode by default.
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( GroupType.FriendlyTypeName );
                }
            }
            else
            {
                // New entity is being created, prepare for edit mode by default.
                if ( box.IsEditable )
                {
                    box.Entity = GetEntityBagForEdit( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( GroupType.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="GroupTypeBag"/> that represents the entity.</returns>
        private GroupTypeBag GetCommonEntityBag( GroupType entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var roleBags = GetGroupTypeRoleBags( entity.Id );

            // If this is a new group type being created, we set the default role to
            // the default "member" role that was seeded within GetGroupTypeRoleBags().
            var defaultGroupRole = entity.DefaultGroupRole.ToListItemBag()
                ?? new ListItemBag
                {
                    Value = roleBags[0].Guid.ToString(),
                    Text = roleBags[0].Name
                };

            return new GroupTypeBag
            {
                IdKey = entity.IdKey,
                Guid = entity.Guid,
                AdministratorTerm = entity.AdministratorTerm,
                AllowedScheduleTypes = entity.AllowedScheduleTypes,
                AllowAnyChildGroupType = entity.AllowAnyChildGroupType,
                AllowGroupSpecificRecordSource = entity.AllowGroupSpecificRecordSource,
                AllowGroupSync = entity.AllowGroupSync,
                AllowMultipleLocations = entity.AllowMultipleLocations,
                AllowSpecificGroupMemberAttributes = entity.AllowSpecificGroupMemberAttributes,
                AllowSpecificGroupMemberWorkflows = entity.AllowSpecificGroupMemberWorkflows,
                AttendanceCountsAsWeekendService = entity.AttendanceCountsAsWeekendService,
                AttendancePrintTo = entity.AttendancePrintTo,
                AttendanceReminderFollowupDays = entity.AttendanceReminderFollowupDaysList,
                AttendanceReminderSendStartOffsetMinutes = entity.AttendanceReminderSendStartOffsetMinutes,
                AttendanceReminderSystemCommunication = GetSystemCommunicationListItemBag( entity.AttendanceReminderSystemCommunication, entity.AttendanceReminderSystemCommunicationId ),
                AttendanceRule = entity.AttendanceRule,
                ChatPushNotificationMode = entity.ChatPushNotificationMode,
                ChildGroupTypes = entity.ChildGroupTypes.ToListItemBagList(),
                DefaultGroupRole = defaultGroupRole,
                Description = entity.Description,
                EnableGroupHistory = entity.EnableGroupHistory,
                EnableGroupTag = entity.EnableGroupTag,
                EnableInactiveReason = entity.EnableInactiveReason,
                EnableLocationSchedules = entity.EnableLocationSchedules,
                EnableRSVP = entity.EnableRSVP,
                EnableSpecificGroupRequirements = entity.EnableSpecificGroupRequirements,
                GroupAttendanceRequiresLocation = entity.GroupAttendanceRequiresLocation,
                GroupAttendanceRequiresSchedule = entity.GroupAttendanceRequiresSchedule,
                GroupCapacityRule = entity.GroupCapacityRule,
                GroupMemberWorkflowTriggers = GetGroupTypeGroupMemberWorkflowTriggerBags( entity.Id ),
                GroupMemberRecordSourceValue = entity.GroupMemberRecordSourceValue.ToListItemBag(),
                GroupMemberTerm = entity.GroupMemberTerm,
                GroupRequirements = GetGroupTypeGroupRequirementBags( entity.Id ),
                GroupsRequireCampus = entity.GroupsRequireCampus,
                GroupStatusDefinedType = entity.GroupStatusDefinedType.ToListItemBag(),
                GroupTerm = entity.GroupTerm,
                GroupTypeColor = entity.GroupTypeColor,
                GroupTypePurposeValue = entity.GroupTypePurposeValue.ToListItemBag(),
                IconCssClass = entity.IconCssClass,
                IgnorePersonInactivated = entity.IgnorePersonInactivated,
                InheritedGroupType = entity.InheritedGroupType.ToListItemBag(),
                IsCapacityRequired = entity.IsCapacityRequired,
                IsChatAllowed = entity.IsChatAllowed,
                IsChatChannelAlwaysShown = entity.IsChatChannelAlwaysShown,
                IsChatChannelPublic = entity.IsChatChannelPublic,
                IsChatEnabledForAllGroups = entity.IsChatEnabledForAllGroups,
                IsIndexEnabled = entity.IsIndexEnabled,
                IsLeavingChatChannelAllowed = entity.IsLeavingChatChannelAllowed,
                IsPeerNetworkEnabled = entity.IsPeerNetworkEnabled,
                IsSchedulingEnabled = entity.IsSchedulingEnabled,
                IsSystem = entity.IsSystem,
                LeaderToLeaderRelationshipMultiplier = entity.LeaderToLeaderRelationshipMultiplier,
                LeaderToNonLeaderRelationshipMultiplier = entity.LeaderToNonLeaderRelationshipMultiplier,
                LocationSelectionMode = entity.LocationSelectionMode,
                LocationTypes = entity.LocationTypes.Select( lt => lt.LocationTypeValue ).Where( dv => dv != null ).ToListItemBagList(),
                Name = entity.Name,
                NonLeaderToLeaderRelationshipMultiplier = entity.NonLeaderToLeaderRelationshipMultiplier,
                NonLeaderToNonLeaderRelationshipMultiplier = entity.NonLeaderToNonLeaderRelationshipMultiplier,
                Order = entity.Order,
                RelationshipGrowthEnabled = entity.RelationshipGrowthEnabled,
                RelationshipStrength = entity.RelationshipStrength,
                RequiresInactiveReason = entity.RequiresInactiveReason,
                RequiresReasonIfDeclineSchedule = entity.RequiresReasonIfDeclineSchedule,
                Roles = roleBags,
                RSVPReminderOffsetDays = entity.RSVPReminderOffsetDays,
                RSVPReminderSystemCommunication = GetSystemCommunicationListItemBag( entity.RSVPReminderSystemCommunication, entity.RSVPReminderSystemCommunicationId ),
                ScheduleCancellationWorkflowType = entity.ScheduleCancellationWorkflowType.ToListItemBag(),
                ScheduleConfirmationEmailOffsetDays = entity.ScheduleConfirmationEmailOffsetDays,
                ScheduleConfirmationLogic = entity.ScheduleConfirmationLogic,
                ScheduleConfirmationSystemCommunication = GetSystemCommunicationListItemBag( entity.ScheduleConfirmationSystemCommunication, entity.ScheduleConfirmationSystemCommunicationId ),
                ScheduleCoordinatorNotificationTypes = entity.ScheduleCoordinatorNotificationTypes,
                ScheduleReminderEmailOffsetDays = entity.ScheduleReminderEmailOffsetDays,
                ScheduleReminderSystemCommunication = GetSystemCommunicationListItemBag( entity.ScheduleReminderSystemCommunication, entity.ScheduleReminderSystemCommunicationId ),
                ScheduleExclusions = GetGroupTypeScheduleExclusionBags( entity.Id ),
                SendAttendanceReminder = entity.SendAttendanceReminder,
                ShowAdministrator = entity.ShowAdministrator,
                ShowConnectionStatus = entity.ShowConnectionStatus,
                ShowInGroupList = entity.ShowInGroupList,
                ShowInNavigation = entity.ShowInNavigation,
                ShowMaritalStatus = entity.ShowMaritalStatus,
                TakesAttendance = entity.TakesAttendance
            };
        }

        /// <inheritdoc/>
        protected override GroupTypeBag GetEntityBagForView( GroupType entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            return bag;
        }

        /// <inheritdoc/>
        protected override GroupTypeBag GetEntityBagForEdit( GroupType entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.GroupViewLavaTemplate = entity.GroupViewLavaTemplate;

            if ( entity.Attributes == null )
            {
                entity.LoadAttributes( RockContext );
            }

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson, enforceSecurity: true );

            // Get the Group, GroupMember, and GroupType attribute definitions for edit.
            LoadAttributesForLocalEntities( entity.Id, bag );

            return bag;
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( GroupType entity, ValidPropertiesBox<GroupTypeBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.AdministratorTerm ),
                () => entity.AdministratorTerm = box.Bag.AdministratorTerm );

            box.IfValidProperty( nameof( box.Bag.AllowAnyChildGroupType ),
                () => entity.AllowAnyChildGroupType = box.Bag.AllowAnyChildGroupType );

            box.IfValidProperty( nameof( box.Bag.AllowedScheduleTypes ),
                () => entity.AllowedScheduleTypes = box.Bag.AllowedScheduleTypes );

            box.IfValidProperty( nameof( box.Bag.AllowGroupSpecificRecordSource ),
                () => entity.AllowGroupSpecificRecordSource = box.Bag.AllowGroupSpecificRecordSource );

            box.IfValidProperty( nameof( box.Bag.AllowGroupSync ),
                () => entity.AllowGroupSync = box.Bag.AllowGroupSync );

            box.IfValidProperty( nameof( box.Bag.AllowMultipleLocations ),
                () => entity.AllowMultipleLocations = box.Bag.AllowMultipleLocations );

            box.IfValidProperty( nameof( box.Bag.AllowSpecificGroupMemberAttributes ),
                () => entity.AllowSpecificGroupMemberAttributes = box.Bag.AllowSpecificGroupMemberAttributes );

            box.IfValidProperty( nameof( box.Bag.AllowSpecificGroupMemberWorkflows ),
                () => entity.AllowSpecificGroupMemberWorkflows = box.Bag.AllowSpecificGroupMemberWorkflows );

            box.IfValidProperty( nameof( box.Bag.AttendanceCountsAsWeekendService ),
                () => entity.AttendanceCountsAsWeekendService = box.Bag.AttendanceCountsAsWeekendService );

            box.IfValidProperty( nameof( box.Bag.AttendancePrintTo ),
                () => entity.AttendancePrintTo = box.Bag.AttendancePrintTo );

            box.IfValidProperty( nameof( box.Bag.AttendanceReminderFollowupDays ),
                () => entity.AttendanceReminderFollowupDaysList = box.Bag.AttendanceReminderFollowupDays ?? new List<int>() );

            box.IfValidProperty( nameof( box.Bag.AttendanceReminderSendStartOffsetMinutes ),
                () => entity.AttendanceReminderSendStartOffsetMinutes = box.Bag.AttendanceReminderSendStartOffsetMinutes );

            box.IfValidProperty( nameof( box.Bag.AttendanceReminderSystemCommunication ),
                () => entity.AttendanceReminderSystemCommunicationId = box.Bag.AttendanceReminderSystemCommunication.GetEntityId<SystemCommunication>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.AttendanceRule ),
                () => entity.AttendanceRule = box.Bag.AttendanceRule );

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.Description = box.Bag.Description );

            box.IfValidProperty( nameof( box.Bag.EnableGroupHistory ),
                () => entity.EnableGroupHistory = box.Bag.EnableGroupHistory );

            box.IfValidProperty( nameof( box.Bag.EnableGroupTag ),
                () => entity.EnableGroupTag = box.Bag.EnableGroupTag );

            box.IfValidProperty( nameof( box.Bag.EnableInactiveReason ),
                () => entity.EnableInactiveReason = box.Bag.EnableInactiveReason );

            box.IfValidProperty( nameof( box.Bag.EnableLocationSchedules ),
                () => entity.EnableLocationSchedules = box.Bag.EnableLocationSchedules );

            box.IfValidProperty( nameof( box.Bag.EnableRSVP ),
                () => entity.EnableRSVP = box.Bag.EnableRSVP );

            box.IfValidProperty( nameof( box.Bag.EnableSpecificGroupRequirements ),
                () => entity.EnableSpecificGroupRequirements = box.Bag.EnableSpecificGroupRequirements );

            box.IfValidProperty( nameof( box.Bag.GroupAttendanceRequiresLocation ),
                () => entity.GroupAttendanceRequiresLocation = box.Bag.GroupAttendanceRequiresLocation );

            box.IfValidProperty( nameof( box.Bag.GroupAttendanceRequiresSchedule ),
                () => entity.GroupAttendanceRequiresSchedule = box.Bag.GroupAttendanceRequiresSchedule );

            box.IfValidProperty( nameof( box.Bag.GroupCapacityRule ),
                () => entity.GroupCapacityRule = box.Bag.GroupCapacityRule );

            box.IfValidProperty( nameof( box.Bag.GroupMemberRecordSourceValue ),
                () => entity.GroupMemberRecordSourceValueId = box.Bag.GroupMemberRecordSourceValue.GetEntityId<DefinedValue>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.GroupMemberTerm ),
                () => entity.GroupMemberTerm = box.Bag.GroupMemberTerm );

            box.IfValidProperty( nameof( box.Bag.GroupsRequireCampus ),
                () => entity.GroupsRequireCampus = box.Bag.GroupsRequireCampus );

            box.IfValidProperty( nameof( box.Bag.GroupStatusDefinedType ),
                () => entity.GroupStatusDefinedTypeId = box.Bag.GroupStatusDefinedType.GetEntityId<DefinedType>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.GroupTerm ),
                () => entity.GroupTerm = box.Bag.GroupTerm );

            box.IfValidProperty( nameof( box.Bag.GroupTypeColor ),
                () => entity.GroupTypeColor = box.Bag.GroupTypeColor );

            box.IfValidProperty( nameof( box.Bag.GroupTypePurposeValue ),
                () => entity.GroupTypePurposeValueId = box.Bag.GroupTypePurposeValue.GetEntityId<DefinedValue>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.GroupViewLavaTemplate ),
                () => entity.GroupViewLavaTemplate = box.Bag.GroupViewLavaTemplate );

            box.IfValidProperty( nameof( box.Bag.IconCssClass ),
                () => entity.IconCssClass = box.Bag.IconCssClass );

            box.IfValidProperty( nameof( box.Bag.IgnorePersonInactivated ),
                () => entity.IgnorePersonInactivated = box.Bag.IgnorePersonInactivated );

            box.IfValidProperty( nameof( box.Bag.InheritedGroupType ),
                () => entity.InheritedGroupTypeId = box.Bag.InheritedGroupType.GetEntityId<GroupType>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.IsCapacityRequired ),
                () => entity.IsCapacityRequired = box.Bag.IsCapacityRequired );

            box.IfValidProperty( nameof( box.Bag.IsIndexEnabled ),
                () => entity.IsIndexEnabled = box.Bag.IsIndexEnabled );

            box.IfValidProperty( nameof( box.Bag.IsPeerNetworkEnabled ),
                () => entity.IsPeerNetworkEnabled = box.Bag.IsPeerNetworkEnabled );

            box.IfValidProperty( nameof( box.Bag.IsSchedulingEnabled ),
                () => entity.IsSchedulingEnabled = box.Bag.IsSchedulingEnabled );

            box.IfValidProperty( nameof( box.Bag.LeaderToLeaderRelationshipMultiplier ),
                () => entity.LeaderToLeaderRelationshipMultiplier = box.Bag.LeaderToLeaderRelationshipMultiplier );

            box.IfValidProperty( nameof( box.Bag.LeaderToNonLeaderRelationshipMultiplier ),
                () => entity.LeaderToNonLeaderRelationshipMultiplier = box.Bag.LeaderToNonLeaderRelationshipMultiplier );

            box.IfValidProperty( nameof( box.Bag.LocationSelectionMode ),
                () => entity.LocationSelectionMode = box.Bag.LocationSelectionMode );

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.NonLeaderToLeaderRelationshipMultiplier ),
                () => entity.NonLeaderToLeaderRelationshipMultiplier = box.Bag.NonLeaderToLeaderRelationshipMultiplier );

            box.IfValidProperty( nameof( box.Bag.NonLeaderToNonLeaderRelationshipMultiplier ),
                () => entity.NonLeaderToNonLeaderRelationshipMultiplier = box.Bag.NonLeaderToNonLeaderRelationshipMultiplier );

            box.IfValidProperty( nameof( box.Bag.Order ),
                () => entity.Order = box.Bag.Order );

            box.IfValidProperty( nameof( box.Bag.RelationshipGrowthEnabled ),
                () => entity.RelationshipGrowthEnabled = box.Bag.RelationshipGrowthEnabled );

            box.IfValidProperty( nameof( box.Bag.RelationshipStrength ),
                () => entity.RelationshipStrength = box.Bag.RelationshipStrength );

            box.IfValidProperty( nameof( box.Bag.RequiresInactiveReason ),
                () => entity.RequiresInactiveReason = box.Bag.RequiresInactiveReason );

            box.IfValidProperty( nameof( box.Bag.RequiresReasonIfDeclineSchedule ),
                () => entity.RequiresReasonIfDeclineSchedule = box.Bag.RequiresReasonIfDeclineSchedule );

            box.IfValidProperty( nameof( box.Bag.RSVPReminderOffsetDays ), () =>
            {
                entity.RSVPReminderOffsetDays = box.Bag.RSVPReminderOffsetDays == 0
                    ? ( int? ) null
                    : box.Bag.RSVPReminderOffsetDays;
            } );

            box.IfValidProperty( nameof( box.Bag.RSVPReminderSystemCommunication ),
                () => entity.RSVPReminderSystemCommunicationId = box.Bag.RSVPReminderSystemCommunication.GetEntityId<SystemCommunication>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.ScheduleCancellationWorkflowType ),
                () => entity.ScheduleCancellationWorkflowTypeId = box.Bag.ScheduleCancellationWorkflowType.GetEntityId<WorkflowType>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.ScheduleConfirmationEmailOffsetDays ),
                () => entity.ScheduleConfirmationEmailOffsetDays = box.Bag.ScheduleConfirmationEmailOffsetDays );

            box.IfValidProperty( nameof( box.Bag.ScheduleConfirmationLogic ),
                () => entity.ScheduleConfirmationLogic = box.Bag.ScheduleConfirmationLogic );

            box.IfValidProperty( nameof( box.Bag.ScheduleConfirmationSystemCommunication ),
                () => entity.ScheduleConfirmationSystemCommunicationId = box.Bag.ScheduleConfirmationSystemCommunication.GetEntityId<SystemCommunication>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.ScheduleCoordinatorNotificationTypes ),
                () => entity.ScheduleCoordinatorNotificationTypes = box.Bag.ScheduleCoordinatorNotificationTypes );

            box.IfValidProperty( nameof( box.Bag.ScheduleReminderEmailOffsetDays ),
                () => entity.ScheduleReminderEmailOffsetDays = box.Bag.ScheduleReminderEmailOffsetDays );

            box.IfValidProperty( nameof( box.Bag.ScheduleReminderSystemCommunication ),
                () => entity.ScheduleReminderSystemCommunicationId = box.Bag.ScheduleReminderSystemCommunication.GetEntityId<SystemCommunication>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.SendAttendanceReminder ),
                () => entity.SendAttendanceReminder = box.Bag.SendAttendanceReminder );

            box.IfValidProperty( nameof( box.Bag.ShowAdministrator ),
                () => entity.ShowAdministrator = box.Bag.ShowAdministrator );

            box.IfValidProperty( nameof( box.Bag.ShowConnectionStatus ),
                () => entity.ShowConnectionStatus = box.Bag.ShowConnectionStatus );

            box.IfValidProperty( nameof( box.Bag.ShowInGroupList ),
                () => entity.ShowInGroupList = box.Bag.ShowInGroupList );

            box.IfValidProperty( nameof( box.Bag.ShowInNavigation ),
                () => entity.ShowInNavigation = box.Bag.ShowInNavigation );

            box.IfValidProperty( nameof( box.Bag.ShowMaritalStatus ),
                () => entity.ShowMaritalStatus = box.Bag.ShowMaritalStatus );

            box.IfValidProperty( nameof( box.Bag.TakesAttendance ),
                () => entity.TakesAttendance = box.Bag.TakesAttendance );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: true );
                } );

            if ( ChatHelper.IsChatEnabled )
            {
                box.IfValidProperty( nameof( box.Bag.IsChatAllowed ),
                () => entity.IsChatAllowed = box.Bag.IsChatAllowed );

                box.IfValidProperty( nameof( box.Bag.IsChatEnabledForAllGroups ),
                    () => entity.IsChatEnabledForAllGroups = box.Bag.IsChatEnabledForAllGroups );

                box.IfValidProperty( nameof( box.Bag.IsLeavingChatChannelAllowed ),
                    () => entity.IsLeavingChatChannelAllowed = box.Bag.IsLeavingChatChannelAllowed );

                box.IfValidProperty( nameof( box.Bag.IsChatChannelPublic ),
                    () => entity.IsChatChannelPublic = box.Bag.IsChatChannelPublic );

                box.IfValidProperty( nameof( box.Bag.IsChatChannelAlwaysShown ),
                    () => entity.IsChatChannelAlwaysShown = box.Bag.IsChatChannelAlwaysShown );

                box.IfValidProperty( nameof( box.Bag.ChatPushNotificationMode ),
                    () => entity.ChatPushNotificationMode = box.Bag.ChatPushNotificationMode );
            }

            return true;
        }

        /// <inheritdoc/>
        protected override GroupType GetInitialEntity()
        {
            var entity = GetInitialEntity<GroupType, GroupTypeService>( RockContext, PageParameterKey.GroupTypeId );

            ApplyNewGroupTypeDefaultValues( entity );

            return entity;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl()
            };
        }

        /// <inheritdoc/>
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            var key = pageReference.GetPageParameter( PageParameterKey.GroupTypeId );
            var pageParameters = new Dictionary<string, string>();

            var name = new GroupTypeService( RockContext )
                .GetSelect( key, mf => mf.Name );

            if ( name != null )
            {
                pageParameters.Add( PageParameterKey.GroupTypeId, key );
            }

            var breadCrumbPageRef = new PageReference( pageReference.PageId, 0, pageParameters );
            var breadCrumb = new BreadCrumbLink( name ?? "New Group Type", breadCrumbPageRef );

            return new BreadCrumbResult
            {
                BreadCrumbs = new List<IBreadCrumb> { breadCrumb }
            };
        }

        /// <inheritdoc/>
        protected override bool TryGetEntityForEditAction( string idKey, out GroupType entity, out BlockActionResult error )
        {
            var entityService = new GroupTypeService( RockContext );
            error = null;

            // Determine if we are editing an existing entity or creating a new one.
            if ( idKey.IsNotNullOrWhiteSpace() )
            {
                // If editing an existing entity then load it and make sure it
                // was found and can still be edited.
                entity = entityService.Get( idKey, !PageCache.Layout.Site.DisablePredictableIds );
            }
            else
            {
                // Create a new entity.
                entity = new GroupType();
                entityService.Add( entity );

                ApplyNewGroupTypeDefaultValues( entity, entityService );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{GroupType.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit {GroupType.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Synchronizes related entities by comparing existing entities with incoming data, deleting removed items,
        /// and adding or updating entities as needed.
        /// </summary>
        private void SyncRelatedEntities<TEntity, TBag, TKey>(
            Service<TEntity> service,
            IQueryable<TEntity> existingEntitiesQuery,
            IEnumerable<TBag> incomingBags,
            Func<TEntity, TKey> existingKeySelector,
            Func<TBag, TKey> incomingKeySelector,
            Func<TBag, TEntity> createNew,
            Action<TEntity, TBag> updateEntity )
            where TEntity : Entity<TEntity>, new()
        {
            // Load existing entities from database
            var existingEntities = existingEntitiesQuery.ToList();

            var existingByKey = existingEntities.ToDictionary( existingKeySelector );

            var incomingList = ( incomingBags ?? Enumerable.Empty<TBag>() ).ToList();
            var incomingKeys = incomingList.Select( incomingKeySelector ).ToHashSet();

            // Delete entities that are no longer in the incoming set
            foreach ( var entity in existingEntities.Where( e => !incomingKeys.Contains( existingKeySelector( e ) ) ).ToList() )
            {
                service.Delete( entity );
            }

            // Add or update entities based on incoming data
            foreach ( var bag in incomingList )
            {
                var key = incomingKeySelector( bag );

                if ( !existingByKey.TryGetValue( key, out var entity ) )
                {
                    entity = createNew( bag );
                    service.Add( entity );
                }

                updateEntity( entity, bag );
            }
        }

        #endregion Methods

        #region Helper Methods

        /// <summary>
        /// Applies default values to a new <see cref="GroupType"/>.
        /// </summary>
        /// <param name="entity">The group type entity.</param>
        /// <param name="groupTypeService">An optional service instance to use for queries.</param>
        private void ApplyNewGroupTypeDefaultValues( GroupType entity, GroupTypeService groupTypeService = null )
        {
            if ( entity == null || entity.Id != 0 )
            {
                return;
            }

            groupTypeService = groupTypeService ?? new GroupTypeService( RockContext );

            entity.ShowInGroupList = true;
            entity.GroupTerm = entity.GroupTerm.IsNullOrWhiteSpace() ? "Group" : entity.GroupTerm;
            entity.GroupMemberTerm = entity.GroupMemberTerm.IsNullOrWhiteSpace() ? "Member" : entity.GroupMemberTerm;

            var maxOrder = groupTypeService.Queryable()
                .Select( t => ( int? ) t.Order )
                .Max();
            entity.Order = maxOrder.HasValue ? maxOrder.Value + 1 : 0;

            // Default system communications.
            var systemCommunicationGuids = new List<Guid>
            {
                Rock.SystemGuid.SystemCommunication.SCHEDULING_CONFIRMATION.AsGuid(),
                Rock.SystemGuid.SystemCommunication.SCHEDULING_REMINDER.AsGuid(),
                Rock.SystemGuid.SystemCommunication.GROUP_ATTENDANCE_REMINDER.AsGuid()
            };

            var systemCommunicationIdLookup = new SystemCommunicationService( RockContext ).Queryable()
                .Where( c => systemCommunicationGuids.Contains( c.Guid ) )
                .Select( c => new { c.Guid, c.Id } )
                .ToList()
                .ToDictionary( k => k.Guid, v => v.Id );

            entity.ScheduleConfirmationSystemCommunicationId =
                systemCommunicationIdLookup.GetValueOrNull( Rock.SystemGuid.SystemCommunication.SCHEDULING_CONFIRMATION.AsGuid() );

            entity.ScheduleReminderSystemCommunicationId =
                systemCommunicationIdLookup.GetValueOrNull( Rock.SystemGuid.SystemCommunication.SCHEDULING_REMINDER.AsGuid() );

            entity.AttendanceReminderSystemCommunicationId =
                systemCommunicationIdLookup.GetValueOrNull( Rock.SystemGuid.SystemCommunication.GROUP_ATTENDANCE_REMINDER.AsGuid() );

            if ( entity.GroupViewLavaTemplate.IsNullOrWhiteSpace() )
            {
                entity.GroupViewLavaTemplate = Rock.Web.SystemSettings.GetValue( "core_templates_GroupViewTemplate" );
            }
        }

        /// <summary>
        /// Gets a <see cref="ListItemBag"/> for a system communication, using the navigation property if available,
        /// otherwise falling back to fetching by Id.
        /// </summary>
        private ListItemBag GetSystemCommunicationListItemBag( SystemCommunication systemCommunication, int? systemCommunicationId )
        {
            if ( systemCommunication != null )
            {
                return systemCommunication.ToListItemBag( systemCommunication.Title );
            }

            if ( !systemCommunicationId.HasValue )
            {
                return null;
            }

            var communication = new SystemCommunicationService( RockContext ).Get( systemCommunicationId.Value );
            return communication?.ToListItemBag( communication.Title );
        }

        /// <summary>
        /// Gets a list of defined types the current person is authorized to view,
        /// formatted as <see cref="ListItemBag"/> options sorted by name.
        /// </summary>
        /// <returns>
        /// A list of defined type options available to the current user.
        /// </returns>
        private List<ListItemBag> GetDefinedTypeOptions()
        {
            return DefinedTypeCache.All()
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToListItemBagList();
        }

        /// <summary>
        /// Gets system communication options and also returns the RSVP subset.
        /// </summary>
        /// <param name="rsvpSystemCommunicationOptions">The RSVP subset of system communication options.</param>
        /// <returns>A list of <see cref="ListItemBag"/> objects representing all system communications.</returns>
        private List<ListItemBag> GetSystemCommunicationOptions( out List<ListItemBag> rsvpSystemCommunicationOptions )
        {
            var rsvpCategoryGuid = Rock.SystemGuid.Category.SYSTEM_COMMUNICATION_RSVP_CONFIRMATION.AsGuid();

            var systemCommunications = new SystemCommunicationService( RockContext ).Queryable()
                .Select( c => new
                {
                    c.Guid,
                    c.Title,
                    CategoryGuid = ( Guid? ) c.Category.Guid
                } )
                .OrderBy( c => c.Title )
                .ToList();

            var allOptions = systemCommunications
                .Select( c => new ListItemBag
                {
                    Value = c.Guid.ToString(),
                    Text = c.Title
                } )
                .ToList();

            rsvpSystemCommunicationOptions = systemCommunications
                .Where( c => c.CategoryGuid.HasValue && c.CategoryGuid.Value == rsvpCategoryGuid )
                .Select( c => new ListItemBag
                {
                    Value = c.Guid.ToString(),
                    Text = c.Title
                } )
                .ToList();

            return allOptions;
        }

        /// <summary>
        /// Gets the group requirement bags for the specified group type.
        /// </summary>
        private List<GroupTypeGroupRequirementBag> GetGroupTypeGroupRequirementBags( int groupTypeId )
        {
            if ( groupTypeId == 0 )
            {
                return new List<GroupTypeGroupRequirementBag>();
            }

            var groupRequirements = new GroupRequirementService( RockContext ).Queryable()
                .AsNoTracking()
                .Include( r => r.GroupRequirementType )
                .Include( r => r.GroupRole )
                .Include( r => r.AppliesToDataView )
                .Include( r => r.DueDateAttribute )
                .Where( r => r.GroupTypeId.HasValue && r.GroupTypeId.Value == groupTypeId )
                .ToList()
                .Select( r => new GroupTypeGroupRequirementBag
                {
                    Guid = r.Guid,
                    GroupRequirementType = r.GroupRequirementType.ToListItemBag(),
                    Role = r.GroupRole != null ? new ListItemBag { Value = r.GroupRole.Guid.ToString(), Text = r.GroupRole.Name } : null,
                    MustMeetRequirementToAddMember = r.MustMeetRequirementToAddMember,
                    AppliesToAgeClassification = r.AppliesToAgeClassification,
                    AppliesToDataView = r.AppliesToDataView.ToListItemBag(),
                    AllowLeadersToOverride = r.AllowLeadersToOverride,
                    DueDateType = r.GroupRequirementType?.DueDateType ?? DueDateType.Immediate,
                    DueDateStaticDate = r.DueDateStaticDate?.ToRockDateTimeOffset(),
                    DueDateAttribute = r.DueDateAttribute != null ? new ListItemBag { Value = r.DueDateAttribute.Guid.ToString(), Text = r.DueDateAttribute.Name } : null
                } )
                .OrderBy( r => r.GroupRequirementType.Text )
                .ToList();

            return groupRequirements;
        }

        /// <summary>
        /// Gets the schedule exclusion bags for the specified group type.
        /// </summary>
        private List<GroupTypeGroupScheduleExclusionBag> GetGroupTypeScheduleExclusionBags( int groupTypeId )
        {
            if ( groupTypeId == 0 )
            {
                return new List<GroupTypeGroupScheduleExclusionBag>();
            }

            var scheduleExclusions = new GroupScheduleExclusionService( RockContext ).Queryable()
                .Where( s => s.GroupTypeId == groupTypeId )
                .OrderBy( s => s.StartDate )
                .Select( s => new
                {
                    s.Guid,
                    s.StartDate,
                    s.EndDate
                } )
                .ToList()
                .Select( s => new GroupTypeGroupScheduleExclusionBag
                {
                    Guid = s.Guid,
                    StartDate = s.StartDate?.ToRockDateTimeOffset(),
                    EndDate = s.EndDate?.ToRockDateTimeOffset()
                } )
                .ToList();

            return scheduleExclusions;
        }

        /// <summary>
        /// Gets the group type role bags for the specified group type.
        /// </summary>
        private List<GroupTypeRoleBag> GetGroupTypeRoleBags( int groupTypeId )
        {
            var roles = new List<GroupTypeRole>();

            // New group types get a default "Member" role.
            if ( groupTypeId == 0 )
            {
                var memberRole = new GroupTypeRole
                {
                    GroupTypeId = 0,
                    Name = "Member",
                    Guid = Guid.NewGuid()
                };

                roles.Add( memberRole );
            }
            else
            {
                roles = new GroupTypeRoleService( RockContext ).Queryable()
                .AsNoTracking()
                .Where( r => r.GroupTypeId == groupTypeId )
                .OrderBy( r => r.Order )
                .ToList();
            }

            var bags = new List<GroupTypeRoleBag>();

            foreach ( var role in roles )
            {
                role.LoadAttributes( RockContext );

                var bag = new GroupTypeRoleBag
                {
                    Guid = role.Guid,
                    IsSystem = role.IsSystem,
                    Name = role.Name,
                    Description = role.Description,
                    Order = role.Order,
                    MaxCount = role.MaxCount,
                    MinCount = role.MinCount,
                    IsLeader = role.IsLeader,
                    CanView = role.CanView,
                    CanEdit = role.CanEdit,
                    ReceiveRequirementsNotifications = role.ReceiveRequirementsNotifications,
                    CanManageMembers = role.CanManageMembers,
                    IsExcludedFromPeerNetwork = role.IsExcludedFromPeerNetwork,
                    IsCheckInAllowed = role.IsCheckInAllowed,
                    ChatRole = role.ChatRole,
                    CanTakeAttendance = role.CanTakeAttendance,
                    IsPublic = role.IsPublic
                };

                bag.LoadAttributesAndValuesForPublicEdit( role, RequestContext.CurrentPerson, enforceSecurity: true );

                bags.Add( bag );
            }

            return bags;
        }

        /// <summary>
        /// Gets the group member workflow trigger bags for the specified group type.
        /// </summary>
        private List<GroupTypeGroupMemberWorkflowTriggerBag> GetGroupTypeGroupMemberWorkflowTriggerBags( int groupTypeId )
        {
            if ( groupTypeId <= 0 )
            {
                return new List<GroupTypeGroupMemberWorkflowTriggerBag>();
            }

            var workflowTriggers = new GroupMemberWorkflowTriggerService( RockContext ).Queryable()
                .AsNoTracking()
                .Include( t => t.WorkflowType )
                .Where( t => t.GroupTypeId == groupTypeId )
                .OrderBy( t => t.Order )
                .ToList();

            var bags = new List<GroupTypeGroupMemberWorkflowTriggerBag>( workflowTriggers.Count );

            foreach ( var t in workflowTriggers )
            {
                // {ToStatus}|{ToRoleGuid}|{FromStatus}|{FromRoleGuid}|{TriggerOnFirstAttendance}|{ShowNoteOnPlacement}|{RequireNoteOnPlacement}
                var parts = ( t.TypeQualifier ?? string.Empty ).Split( '|' );

                var bag = new GroupTypeGroupMemberWorkflowTriggerBag
                {
                    Guid = t.Guid,
                    Order = t.Order,
                    Name = t.Name,
                    IsActive = t.IsActive,
                    WorkflowType = t.WorkflowType?.ToListItemBag(),
                    TriggerType = t.TriggerType
                };

                GroupMemberStatus? toStatus = parts.Length > 0
                    ? ( GroupMemberStatus? ) parts[0].AsIntegerOrNull()
                    : null;

                Guid? toRoleGuid = parts.Length > 1
                    ? parts[1].AsGuidOrNull()
                    : null;

                GroupMemberStatus? fromStatus = parts.Length > 2
                    ? ( GroupMemberStatus? ) parts[2].AsIntegerOrNull()
                    : null;

                Guid? fromRoleGuid = parts.Length > 3
                    ? parts[3].AsGuidOrNull()
                    : null;

                var triggerOnFirstAttendance = parts.Length > 4 && parts[4].AsBoolean();
                var showNoteOnPlacement = parts.Length > 5 && parts[5].AsBoolean();
                var requireNoteOnPlacement = parts.Length > 6 && parts[6].AsBoolean();

                switch ( t.TriggerType )
                {
                    case GroupMemberWorkflowTriggerType.MemberAddedToGroup:
                    case GroupMemberWorkflowTriggerType.MemberRemovedFromGroup:
                        /*
                             1/23/2026 - MSE

                             For these trigger types, the UI displays these qualifiers using the label "With Status/ With Role",
                             However, the persisted qualifier format actually stores these values in the "To" slots (part[0] and part[1]).
                        */
                        bag.ToStatus = toStatus;
                        bag.ToRoleGuid = toRoleGuid;
                        break;

                    case GroupMemberWorkflowTriggerType.MemberStatusChanged:
                        bag.FromStatus = fromStatus;
                        bag.ToStatus = toStatus;
                        break;

                    case GroupMemberWorkflowTriggerType.MemberRoleChanged:
                        bag.FromRoleGuid = fromRoleGuid;
                        bag.ToRoleGuid = toRoleGuid;
                        break;

                    case GroupMemberWorkflowTriggerType.MemberAttendedGroup:
                        bag.TriggerOnFirstAttendance = triggerOnFirstAttendance;
                        break;

                    case GroupMemberWorkflowTriggerType.MemberPlacedElsewhere:
                        bag.ShowNoteOnPlacement = showNoteOnPlacement;
                        bag.RequireNoteOnPlacement = requireNoteOnPlacement;
                        break;
                }

                bags.Add( bag );
            }

            return bags;
        }

        /// <summary>
        /// Builds the type qualifier string for a group member workflow trigger.
        /// </summary>
        private static string BuildGroupMemberWorkflowTriggerTypeQualifier( GroupTypeGroupMemberWorkflowTriggerBag bag )
        {
            // Format:
            // {ToStatus}|{ToRoleGuid}|{FromStatus}|{FromRoleGuid}|{TriggerOnFirstAttendance}|{ShowNoteOnPlacement}|{RequireNoteOnPlacement}
            // Even though the UI renders some trigger types as "With Status/Role of", the persisted qualifier format
            // stores values in the "to" slots (part[0] and part[1]).

            string toStatus = string.Empty;
            string toRoleGuid = string.Empty;
            string fromStatus = string.Empty;
            string fromRoleGuid = string.Empty;
            bool triggerOnFirstAttendance = false;
            bool showNoteOnPlacement = false;
            bool requireNoteOnPlacement = false;

            if ( bag != null )
            {
                switch ( bag.TriggerType )
                {
                    case GroupMemberWorkflowTriggerType.MemberAddedToGroup:
                    case GroupMemberWorkflowTriggerType.MemberRemovedFromGroup:
                        toStatus = bag.ToStatus.HasValue ? ( ( int ) bag.ToStatus.Value ).ToString() : string.Empty;
                        toRoleGuid = bag.ToRoleGuid?.ToString() ?? string.Empty;
                        break;

                    case GroupMemberWorkflowTriggerType.MemberStatusChanged:
                        toStatus = bag.ToStatus.HasValue ? ( ( int ) bag.ToStatus.Value ).ToString() : string.Empty;
                        fromStatus = bag.FromStatus.HasValue ? ( ( int ) bag.FromStatus.Value ).ToString() : string.Empty;
                        break;

                    case GroupMemberWorkflowTriggerType.MemberRoleChanged:
                        toRoleGuid = bag.ToRoleGuid?.ToString() ?? string.Empty;
                        fromRoleGuid = bag.FromRoleGuid?.ToString() ?? string.Empty;
                        break;

                    case GroupMemberWorkflowTriggerType.MemberAttendedGroup:
                        triggerOnFirstAttendance = bag.TriggerOnFirstAttendance;
                        break;

                    case GroupMemberWorkflowTriggerType.MemberPlacedElsewhere:
                        showNoteOnPlacement = bag.ShowNoteOnPlacement;
                        requireNoteOnPlacement = bag.RequireNoteOnPlacement;
                        break;
                }
            }

            return string.Format(
                "{0}|{1}|{2}|{3}|{4}|{5}|{6}",
                toStatus,
                toRoleGuid,
                fromStatus,
                fromRoleGuid,
                triggerOnFirstAttendance,
                showNoteOnPlacement,
                requireNoteOnPlacement );
        }

        /// <summary>
        /// Checks if the group member workflow triggers have changed.
        /// </summary>
        /// <param name="groupType">The group type.</param>
        /// <param name="incomingBags">The incoming bags.</param>
        /// <returns></returns>
        private bool HaveGroupMemberWorkflowTriggersChanged( GroupType groupType, List<GroupTypeGroupMemberWorkflowTriggerBag> incomingBags )
        {
            var existingTriggers = new GroupMemberWorkflowTriggerService( RockContext ).Queryable()
                .AsNoTracking()
                .Where( t => t.GroupTypeId == groupType.Id )
                .ToList();

            // Check for deletions
            if ( existingTriggers.Any( t => !incomingBags.Any( b => b.Guid == t.Guid ) ) )
            {
                return true;
            }

            // Check for additions or modifications
            foreach ( var bag in incomingBags )
            {
                var existing = existingTriggers.FirstOrDefault( t => t.Guid == bag.Guid );
                if ( existing == null )
                {
                    return true;
                }

                if ( existing.Name != bag.Name ||
                     existing.IsActive != bag.IsActive ||
                     existing.Order != bag.Order ||
                     existing.WorkflowTypeId != ( bag.WorkflowType?.GetEntityId<WorkflowType>( RockContext ) ?? 0 ) ||
                     existing.TriggerType != bag.TriggerType ||
                     existing.TypeQualifier != BuildGroupMemberWorkflowTriggerTypeQualifier( bag ) )
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Loads the non-inherited attributes for the group type.
        /// </summary>
        /// <param name="entity">The group type entity.</param>
        /// <param name="bag">The bag to populate.</param>
        private void LoadAttributesForLocalEntities( int entityId, GroupTypeBag bag )
        {
            if ( entityId == 0 )
            {
                bag.GroupAttributes = new List<PublicEditableAttributeBag>();
                bag.GroupMemberAttributes = new List<PublicEditableAttributeBag>();
                bag.GroupTypeAttributes = new List<PublicEditableAttributeBag>();
                return;
            }

            var attributeService = new AttributeService( RockContext );
            var qualifierValue = entityId.ToString();

            // Group attributes
            bag.GroupAttributes = attributeService.GetByEntityTypeId( new Rock.Model.Group().TypeId, true )
                .AsNoTracking()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "GroupTypeId", StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( qualifierValue ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList()
                .ConvertAll( a => PublicAttributeHelper.GetPublicEditableAttribute( a ) );

            // GroupMember attributes
            bag.GroupMemberAttributes = attributeService.GetByEntityTypeId( new GroupMember().TypeId, true )
                .AsNoTracking()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "GroupTypeId", StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( qualifierValue ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList()
                .ConvertAll( a => PublicAttributeHelper.GetPublicEditableAttribute( a ) );

            // GroupType attributes
            bag.GroupTypeAttributes = attributeService.GetByEntityTypeId( new GroupType().TypeId, true )
                .AsNoTracking()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "Id", StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( qualifierValue ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList()
                .ConvertAll( a => PublicAttributeHelper.GetPublicEditableAttribute( a ) );
        }

        /// <summary>
        /// Saves the attributes for the specified entity type and qualifier.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier whose attributes are being edited.</param>
        /// <param name="qualifierColumn">The attribute qualifier column.</param>
        /// <param name="qualifierValue">The qualifier value.</param>
        /// <param name="attributes">The attributes as edited in the UI.</param>
        /// <param name="addAuthorizationsFromGroupType">if set to <c>true</c> for new attributes this will copy the explicit permissions for the group type into the attribute.</param>
        /// <param name="groupType">The group type.</param>
        private void SaveAttributes( int entityTypeId, string qualifierColumn, string qualifierValue, List<PublicEditableAttributeBag> attributes, bool addAuthorizationsFromGroupType = false, GroupType groupType = null )
        {
            if ( attributes == null )
            {
                return;
            }

            // Get the existing attributes for this entity type and qualifier value
            var attributeService = new AttributeService( RockContext );
            var existingAttributes = attributeService.GetByEntityTypeQualifier( entityTypeId, qualifierColumn, qualifierValue, true ).ToList();

            // Delete any of those attributes that were removed in the UI
            var remainingAttributeGuids = attributes.Select( a => a.Guid );
            foreach ( var attr in existingAttributes.Where( a => !remainingAttributeGuids.Contains( a.Guid ) ) )
            {
                attributeService.Delete( attr );
                RockContext.SaveChanges();
            }

            // The attributes are coming from the frontend already sorted in the correct order.
            int attributeOrder = 0;
            foreach ( var attrBag in attributes )
            {
                var isNew = !attrBag.Guid.HasValue || !existingAttributes.Any( a => a.Guid == attrBag.Guid.Value );

                var attr = Helper.SaveAttributeEdits( attrBag, entityTypeId, qualifierColumn, qualifierValue, RockContext );
                if ( attr != null )
                {
                    attr.Order = attributeOrder++;

                    if ( isNew && addAuthorizationsFromGroupType && groupType != null )
                    {
                        Authorization.CopyAuthorization( groupType, attr );
                    }
                }
            }

            RockContext.SaveChanges();
        }

        /// <summary>
        /// Saves the public attribute values for the specified <see cref="GroupTypeRole"/>.
        /// </summary>
        /// <param name="role">The role whose attribute values should be saved.</param>
        /// <param name="attributeValues">The public attribute values coming from the UI.</param>
        private void SaveGroupTypeRoleAttributeValues( GroupTypeRole role, Dictionary<string, string> attributeValues )
        {
            if ( role == null || attributeValues == null )
            {
                return;
            }

            role.LoadAttributes( RockContext );

            role.SetPublicAttributeValues( attributeValues, RequestContext.CurrentPerson, enforceSecurity: true );

            role.SaveAttributeValues( RockContext );
        }

        #endregion Helper Methods

        #region Block Actions

        /// <summary>
        /// Gets the box that will contain all the information needed to begin
        /// the edit operation.
        /// </summary>
        /// <param name="key">The identifier of the entity to be edited.</param>
        /// <returns>A box that contains the entity and any other information required.</returns>
        [BlockAction]
        public BlockActionResult Edit( string key )
        {
            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<GroupTypeBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( ValidPropertiesBox<GroupTypeBag> box )
        {
            var entityService = new GroupTypeService( RockContext );

            if ( !TryGetEntityForEditAction( box.Bag.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            var originalEnableGroupHistory = entity.EnableGroupHistory;

            // Update the entity instance from the information in the bag.
            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            // Ensure everything is valid before saving.
            if ( !ValidateGroupType( entity, box.Bag, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            var isNew = entity.Id == 0;
            var triggersUpdated = false;
            Dictionary<Guid, GroupTypeRole> rolesByGuid = null;

            RockContext.WrapTransaction( () =>
            {
                // Save the group type first to ensure it has an Id ( if it's a new group type )
                // before saving the related entities.
                RockContext.SaveChanges();

                // Child Group Types
                box.IfValidProperty( nameof( box.Bag.ChildGroupTypes ), () =>
                {
                    entity.ChildGroupTypes.Clear();

                    var guids = ( box.Bag.ChildGroupTypes ?? new List<ListItemBag>() )
                        .Select( li => li?.Value.AsGuidOrNull() )
                        .Where( g => g.HasValue )
                        .Select( g => g.Value )
                        .Distinct()
                        .ToList();

                    if ( guids.Any() )
                    {
                        var childGroupTypes = new GroupTypeService( RockContext )
                            .Queryable()
                            .Where( gt => guids.Contains( gt.Guid ) )
                            .ToList();

                        foreach ( var childGroupType in childGroupTypes )
                        {
                            entity.ChildGroupTypes.Add( childGroupType );
                        }
                    }
                } );

                // Location Types
                box.IfValidProperty( nameof( box.Bag.LocationTypes ), () =>
                {
                    entity.LocationTypes.Clear();

                    var locationTypeValueIds = ( box.Bag.LocationTypes ?? new List<ListItemBag>() )
                        .Select( a => a.GetEntityId<DefinedValue>( RockContext ) )
                        .Where( a => a.HasValue && a.Value > 0 )
                        .Select( a => a.Value )
                        .Distinct()
                        .ToList();

                    foreach ( var locationTypeValueId in locationTypeValueIds )
                    {
                        entity.LocationTypes.Add( new GroupTypeLocationType
                        {
                            LocationTypeValueId = locationTypeValueId
                        } );
                    }
                } );

                // GroupTypeRoles
                box.IfValidProperty( nameof( box.Bag.Roles ), () =>
                {
                    var roleService = new GroupTypeRoleService( RockContext );
                    var roleBags = ( box.Bag.Roles ?? new List<GroupTypeRoleBag>() ).Where( b => b != null ).ToList();

                    // The roles are coming from the frontend already sorted in the correct order.
                    // Since we're potentially creating a new group type or roles, we cannot implement the ReorderItem() block action pattern.
                    // We set the order properly here.
                    for ( var i = 0; i < roleBags.Count; i++ )
                    {
                        var bag = roleBags[i];

                        if ( bag.Guid == Guid.Empty )
                        {
                            bag.Guid = Guid.NewGuid();
                        }

                        bag.Order = i;
                    }

                    SyncRelatedEntities(
                        roleService,
                        roleService.Queryable().Where( r => r.GroupTypeId == entity.Id ),
                        roleBags,
                        existingKeySelector: r => r.Guid,
                        incomingKeySelector: b => b.Guid,
                        createNew: b => new GroupTypeRole { Guid = b.Guid },
                        updateEntity: ( role, bag ) =>
                        {
                            role.GroupType = entity;
                            role.Name = bag.Name;
                            role.Description = bag.Description;
                            role.Order = bag.Order;
                            role.MaxCount = bag.MaxCount;
                            role.MinCount = bag.MinCount;
                            role.IsLeader = bag.IsLeader;
                            role.ReceiveRequirementsNotifications = bag.ReceiveRequirementsNotifications;
                            role.CanView = bag.CanView;
                            role.CanEdit = bag.CanEdit;
                            role.CanManageMembers = bag.CanManageMembers;
                            role.CanTakeAttendance = bag.CanTakeAttendance;
                            role.IsExcludedFromPeerNetwork = bag.IsExcludedFromPeerNetwork;
                            role.IsCheckInAllowed = bag.IsCheckInAllowed;
                            role.ChatRole = bag.ChatRole;
                            role.IsPublic = bag.IsPublic;
                        } );

                    // Persist role changes so Role.Id is available when saving other related entities.
                    RockContext.SaveChanges();

                    rolesByGuid = roleService.Queryable()
                        .Where( r => r.GroupTypeId == entity.Id )
                        .ToList()
                        .ToDictionary( r => r.Guid );

                    foreach ( var roleBag in roleBags )
                    {
                        if ( rolesByGuid.TryGetValue( roleBag.Guid, out var role ) )
                        {
                            SaveGroupTypeRoleAttributeValues( role, roleBag.AttributeValues );
                        }
                    }
                } );

                // Default Group Role
                box.IfValidProperty( nameof( box.Bag.DefaultGroupRole ), () =>
                {
                    var defaultRoleGuid = box.Bag.DefaultGroupRole?.Value.AsGuidOrNull();
                    if ( defaultRoleGuid.HasValue )
                    {
                        if ( rolesByGuid == null )
                        {
                            rolesByGuid = new GroupTypeRoleService( RockContext ).Queryable()
                                .Where( r => r.GroupTypeId == entity.Id )
                                .ToList()
                                .ToDictionary( r => r.Guid );
                        }

                        rolesByGuid.TryGetValue( defaultRoleGuid.Value, out var defaultRole );

                        if ( defaultRole != null )
                        {
                            entity.DefaultGroupRole = defaultRole;
                        }
                    }
                    else
                    {
                        entity.DefaultGroupRole = null;
                    }
                } );

                // Group Requirements
                box.IfValidProperty( nameof( box.Bag.GroupRequirements ), () =>
                {
                    var requirementService = new GroupRequirementService( RockContext );
                    var requirementBags = ( box.Bag.GroupRequirements ?? new List<GroupTypeGroupRequirementBag>() ).Where( b => b != null ).ToList();
                    foreach ( var b in requirementBags.Where( b => b.Guid == Guid.Empty ) )
                    {
                        b.Guid = Guid.NewGuid();
                    }

                    SyncRelatedEntities(
                        requirementService,
                        requirementService.Queryable().Where( r => r.GroupTypeId.HasValue && r.GroupTypeId.Value == entity.Id ),
                        requirementBags,
                        existingKeySelector: r => r.Guid,
                        incomingKeySelector: b => b.Guid,
                        createNew: b => new GroupRequirement { Guid = b.Guid },
                        updateEntity: ( requirement, bag ) =>
                        {
                            requirement.GroupType = entity;
                            requirement.GroupRequirementTypeId = bag.GroupRequirementType?.GetEntityId<GroupRequirementType>( RockContext ) ?? 0;
                            requirement.GroupRoleId = bag.Role?.GetEntityId<GroupTypeRole>( RockContext );
                            requirement.MustMeetRequirementToAddMember = bag.MustMeetRequirementToAddMember;
                            requirement.AppliesToAgeClassification = bag.AppliesToAgeClassification;
                            requirement.AppliesToDataViewId = bag.AppliesToDataView?.GetEntityId<DataView>( RockContext );
                            requirement.AllowLeadersToOverride = bag.AllowLeadersToOverride;

                            requirement.DueDateStaticDate = null;
                            requirement.DueDateAttributeId = null;

                            if ( bag.DueDateType == DueDateType.ConfiguredDate )
                            {
                                requirement.DueDateStaticDate = bag.DueDateStaticDate?.DateTime;
                            }
                            else if ( bag.DueDateType == DueDateType.GroupAttribute )
                            {
                                requirement.DueDateAttributeId = bag.DueDateAttribute?.GetEntityId<Rock.Model.Attribute>( RockContext );
                            }
                        } );
                } );

                // Schedule Exclusions
                box.IfValidProperty( nameof( box.Bag.ScheduleExclusions ), () =>
                {
                    var exclusionService = new GroupScheduleExclusionService( RockContext );
                    var exclusionBags = ( box.Bag.ScheduleExclusions ?? new List<GroupTypeGroupScheduleExclusionBag>() ).Where( b => b != null ).ToList();
                    foreach ( var b in exclusionBags.Where( b => b.Guid == Guid.Empty ) )
                    {
                        b.Guid = Guid.NewGuid();
                    }

                    SyncRelatedEntities(
                        exclusionService,
                        exclusionService.Queryable().Where( se => se.GroupTypeId == entity.Id ),
                        exclusionBags,
                        existingKeySelector: se => se.Guid,
                        incomingKeySelector: b => b.Guid,
                        createNew: b => new GroupScheduleExclusion { Guid = b.Guid },
                        updateEntity: ( exclusion, bag ) =>
                        {
                            exclusion.GroupType = entity;
                            exclusion.StartDate = bag.StartDate?.DateTime;
                            exclusion.EndDate = bag.EndDate?.DateTime;
                        } );
                } );

                // Group Member Workflow Triggers
                box.IfValidProperty( nameof( box.Bag.GroupMemberWorkflowTriggers ), () =>
                {
                    var triggerService = new GroupMemberWorkflowTriggerService( RockContext );
                    var triggerBags = ( box.Bag.GroupMemberWorkflowTriggers ?? new List<GroupTypeGroupMemberWorkflowTriggerBag>() ).Where( b => b != null ).ToList();
                    foreach ( var b in triggerBags.Where( b => b.Guid == Guid.Empty ) )
                    {
                        b.Guid = Guid.NewGuid();
                    }

                    if ( HaveGroupMemberWorkflowTriggersChanged( entity, triggerBags ) )
                    {
                        triggersUpdated = true;
                        SyncRelatedEntities(
                            triggerService,
                            triggerService.Queryable().Where( t => t.GroupTypeId == entity.Id ),
                            triggerBags,
                            existingKeySelector: t => t.Guid,
                            incomingKeySelector: b => b.Guid,
                            createNew: b => new GroupMemberWorkflowTrigger { Guid = b.Guid },
                            updateEntity: ( trigger, bag ) =>
                            {
                                trigger.GroupType = entity;
                                trigger.Name = bag.Name;
                                trigger.IsActive = bag.IsActive;
                                trigger.Order = bag.Order;
                                trigger.WorkflowTypeId = bag.WorkflowType?.GetEntityId<WorkflowType>( RockContext ) ?? 0;
                                trigger.TriggerType = bag.TriggerType;
                                trigger.TypeQualifier = BuildGroupMemberWorkflowTriggerTypeQualifier( bag );
                            } );
                    }
                } );

                // Set the ModifiedDateTime field to ensure that at least one property of the primary entity is updated before saving changes.
                // This forces the Rock.Data.DbContext.RockPostSave method to be triggered even in cases where only related entities have been modified by the user.
                entity.ModifiedDateTime = RockDateTime.Now;

                RockContext.SaveChanges();

                var qualifierValue = entity.Id.ToString();
                SaveAttributes( new Rock.Model.Group().TypeId, "GroupTypeId", qualifierValue, box.Bag.GroupAttributes, true, entity );
                SaveAttributes( new GroupMember().TypeId, "GroupTypeId", qualifierValue, box.Bag.GroupMemberAttributes );
                SaveAttributes( new GroupType().TypeId, "Id", qualifierValue, box.Bag.GroupTypeAttributes );

                entity.SaveAttributeValues( RockContext );
            } );

            // If EnableGroupHistory was turned from ON to OFF, delete any existing history records.
            // UI has a warning message.
            if ( originalEnableGroupHistory && !entity.EnableGroupHistory )
            {
                new GroupTypeService( RockContext ).BulkDeleteGroupHistory( entity.Id );
            }

            if ( triggersUpdated )
            {
                GroupMemberWorkflowTriggerService.RemoveCachedTriggers();
            }

            if ( isNew )
            {
                return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.GroupTypeId] = entity.IdKey
                } ) );
            }

            // Ensure navigation properties will work now.
            entity = entityService.Get( entity.Id );
            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<GroupTypeBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new GroupTypeService( RockContext );

            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk( this.GetParentPageUrl() );
        }

        /// <summary>
        /// Gets the public editable attribute definitions for a <see cref="GroupTypeRole"/>
        /// scoped to the current <see cref="GroupType"/>.
        /// </summary>
        [BlockAction]
        public BlockActionResult GetGroupTypeRoleDefaultAttributes()
        {
            /*
                1/20/2026 - MSE

                In GetGroupTypeRoleBags(), LoadAttributesAndValuesForPublicEdit() is called for each
                GroupTypeRole associated with the current GroupType.

                This method provides attribute data so that the attribute value container
                can initialize correctly when creating a new role, without requiring an initial
                database save and a second pass into the edit panel in order to edit the
                new role's attribute values.
            */
            var currentGroupType = GetInitialEntity();

            if ( currentGroupType == null )
            {
                return ActionBadRequest( $"{GroupType.FriendlyTypeName} not found." );
            }

            var currentPerson = RequestContext.CurrentPerson;

            /*
                 If the current GroupType is new, it does not yet have an Id, so there wouldn't be
                 any GroupTypeRole attributes qualified to it...

                 aka:
                 ( Attribute.EntityTypeQualifierValue == currentGroupType.Id )

                 We still need to query for GroupTypeRole attributes with no qualifier value
                 ( not scoped to any GroupType ). Calling LoadAttributes() on a GroupTypeRole object
                 with GroupTypeId == 0 will still accomplish this.
            */
            var tempRole = new GroupTypeRole
            {
                GroupTypeId = currentGroupType.Id
            };

            tempRole.LoadAttributes( RockContext );

            var response = new GroupTypeRoleDefaultAttributesResponseBag();
            response.LoadAttributesAndValuesForPublicEdit( tempRole, currentPerson, enforceSecurity: true );

            return ActionOk( response );
        }

        /// <summary>
        /// Gets the count of groups of this type that have chat enabled.
        /// </summary>
        /// <returns>The number of groups that have chat enabled.</returns>
        [BlockAction]
        public BlockActionResult GetChatEnabledGroupCount()
        {
            var key = PageParameter( PageParameterKey.GroupTypeId );
            var groupTypeId = new GroupTypeService( RockContext ).GetSelect( key, gt => ( int? ) gt.Id );

            if ( !groupTypeId.HasValue || groupTypeId.Value == 0 )
            {
                return ActionOk( 0 );
            }

            var count = new GroupTypeService( RockContext )
                .GetChatEnabledGroupCount( groupTypeId.Value );

            return ActionOk( count );
        }

        /// <summary>
        /// Checks if the specified entity can be deleted.
        /// </summary>
        /// <param name="entityKey">The key identifying the type of entity to check.</param>
        /// <param name="request">The request that identifies the entity to check.</param>
        /// <returns>A response indicating if the entity can be deleted.</returns>
        [BlockAction]
        public BlockActionResult CanDeleteEntity( CanDeleteRequestBag request )
        {
            if ( request == null || request.EntityGuid == Guid.Empty || request.EntityKey.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Invalid entity." );
            }

            var entityKey = request.EntityKey;
            string errorMessage;
            bool canDelete;

            if ( entityKey == EntityKey.GroupTypeRole )
            {
                var service = new GroupTypeRoleService( RockContext );
                var entity = service.Get( request.EntityGuid );
                if ( entity == null )
                {
                    return ActionOk( new CanDeleteResponseBag { CanDelete = true } );
                }

                canDelete = service.CanDelete( entity, out errorMessage );
            }
            else if ( entityKey == EntityKey.ScheduleExclusion )
            {
                var service = new GroupScheduleExclusionService( RockContext );
                var entity = service.Get( request.EntityGuid );
                if ( entity == null )
                {
                    return ActionOk( new CanDeleteResponseBag { CanDelete = true } );
                }

                canDelete = service.CanDelete( entity, out errorMessage );
            }
            else if ( entityKey == EntityKey.GroupRequirement )
            {
                var service = new GroupRequirementService( RockContext );
                var entity = service.Get( request.EntityGuid );
                if ( entity == null )
                {
                    return ActionOk( new CanDeleteResponseBag { CanDelete = true } );
                }

                canDelete = service.CanDelete( entity, out errorMessage );
            }
            else if ( entityKey == EntityKey.GroupMemberWorkflowTrigger )
            {
                var service = new GroupMemberWorkflowTriggerService( RockContext );
                var entity = service.Get( request.EntityGuid );
                if ( entity == null )
                {
                    return ActionOk( new CanDeleteResponseBag { CanDelete = true } );
                }

                canDelete = service.CanDelete( entity, out errorMessage );
            }
            else
            {
                return ActionBadRequest( $"Unknown entity: {entityKey}" );
            }

            return ActionOk( new CanDeleteResponseBag { CanDelete = canDelete, ErrorMessage = errorMessage } );
        }

        /// <summary>
        /// Gets group, group member, and group type attributes for the specified inherited group type, walking the inheritance hierarchy.
        /// </summary>
        /// <param name="inheritedGroupTypeGuid">The inherited group type Guid selected in the UI.</param>
        [BlockAction]
        public BlockActionResult GetInheritedAttributes( Guid inheritedGroupTypeGuid )
        {
            var responseBag = new GetInheritedAttributesResponseBag
            {
                InheritedGroupAttributes = new List<GroupTypeInheritedAttributeBag>(),
                InheritedGroupMemberAttributes = new List<GroupTypeInheritedAttributeBag>(),
                InheritedGroupTypeAttributes = new List<GroupTypeInheritedAttributeBag>(),
            };

            if ( inheritedGroupTypeGuid != Guid.Empty )
            {
                var attributeService = new AttributeService( RockContext );
                var groupEntityTypeId = new Rock.Model.Group().TypeId;
                var groupMemberEntityTypeId = new GroupMember().TypeId;
                var groupTypeEntityTypeId = new GroupType().TypeId;

                var groupTypeService = new GroupTypeService( RockContext );
                var inheritedGroupType = groupTypeService.Get( inheritedGroupTypeGuid );

                if ( inheritedGroupType != null )
                {
                    /*
                         2/3/2026 - MSE

                         Added a safeguard to prevent infinite loops when resolving inherited Group Types
                         in case there is a circular inheritance chain (A -> B -> C -> A).
                    */
                    var visitedGroupTypeIds = new HashSet<int>();

                    do
                    {
                        if ( !visitedGroupTypeIds.Add( inheritedGroupType.Id ) )
                        {
                            return ActionBadRequest( "A circular Group Type inheritance relationship was detected. Please examine the 'Inherited Group Type' setting." );
                        }

                        var qualifierValue = inheritedGroupType.Id.ToString();

                        var inheritedFromName = inheritedGroupType.Name;
                        string inheritedFromUrl = null;
                        var urlTemplate = EntityTypeCache.Get( typeof( GroupType ) ).LinkUrlLavaTemplate;
                        if ( !string.IsNullOrWhiteSpace( urlTemplate ) )
                        {
                            inheritedFromUrl = urlTemplate.ResolveMergeFields( new Dictionary<string, object>
                            {
                                { "Entity", inheritedGroupType }
                            } );

                            inheritedFromUrl = this.RequestContext.ResolveRockUrl( inheritedFromUrl );
                        }

                        // Inherited Group attributes
                        responseBag.InheritedGroupAttributes.AddRange(
                            attributeService.GetByEntityTypeId( groupEntityTypeId, true )
                                .Where( a =>
                                    a.EntityTypeQualifierColumn.Equals( "GroupTypeId", StringComparison.OrdinalIgnoreCase ) &&
                                    a.EntityTypeQualifierValue.Equals( qualifierValue ) )
                                .OrderBy( a => a.Order )
                                .ThenBy( a => a.Name )
                                .Select( a => new GroupTypeInheritedAttributeBag
                                {
                                    Name = a.Name,
                                    Description = a.Description,
                                    Key = a.Key,
                                    Guid = a.Guid,
                                    InheritedFromGroupTypeName = inheritedFromName,
                                    InheritedFromGroupTypeUrl = inheritedFromUrl
                                } )
                                .ToList() );

                        // Inherited GroupMember attributes
                        responseBag.InheritedGroupMemberAttributes.AddRange(
                            attributeService.GetByEntityTypeId( groupMemberEntityTypeId, true )
                                .Where( a =>
                                    a.EntityTypeQualifierColumn.Equals( "GroupTypeId", StringComparison.OrdinalIgnoreCase ) &&
                                    a.EntityTypeQualifierValue.Equals( qualifierValue ) )
                                .OrderBy( a => a.Order )
                                .ThenBy( a => a.Name )
                                .Select( a => new GroupTypeInheritedAttributeBag
                                {
                                    Name = a.Name,
                                    Description = a.Description,
                                    Key = a.Key,
                                    Guid = a.Guid,
                                    InheritedFromGroupTypeName = inheritedFromName,
                                    InheritedFromGroupTypeUrl = inheritedFromUrl
                                } )
                                .ToList() );

                        // Inherited GroupType attributes
                        responseBag.InheritedGroupTypeAttributes.AddRange(
                            attributeService.GetByEntityTypeId( groupTypeEntityTypeId, true )
                                .Where( a =>
                                    a.EntityTypeQualifierColumn.Equals( "Id", StringComparison.OrdinalIgnoreCase ) &&
                                    a.EntityTypeQualifierValue.Equals( qualifierValue ) )
                                .OrderBy( a => a.Order )
                                .ThenBy( a => a.Name )
                                .Select( a => new GroupTypeInheritedAttributeBag
                                {
                                    Name = a.Name,
                                    Description = a.Description,
                                    Key = a.Key,
                                    Guid = a.Guid,
                                    InheritedFromGroupTypeName = inheritedFromName,
                                    InheritedFromGroupTypeUrl = inheritedFromUrl
                                } )
                                .ToList() );

                        // Continue to walk the hierarchy chain
                        inheritedGroupType = inheritedGroupType.InheritedGroupTypeId.HasValue
                            ? groupTypeService.Get( inheritedGroupType.InheritedGroupTypeId.Value )
                            : null;

                    } while ( inheritedGroupType != null );
                }
            }

            return ActionOk( responseBag );
        }

        #endregion Block Actions
    }
}

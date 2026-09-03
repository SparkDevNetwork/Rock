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

using Rock.Attribute;
using Rock.Communication;
using Rock.Communication.Chat;
using Rock.Constants;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Group.GroupMemberDetail;
using Rock.ViewModels.Utility;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.Group
{
    /// <summary>
    /// Displays the details of a particular group member.
    /// </summary>
    [DisplayName( "Group Member Detail" )]
    [Category( "Groups" )]
    [Description( "Displays the details of the given group member for editing role, status, etc." )]
    [IconCssClass( "ti ti-user" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [LinkedPage(
        "Registration Page",
        Description = "The page used to view the registration(s) linked to this group member.",
        IsRequired = false,
        Order = 0,
        Key = AttributeKey.RegistrationPage )]

    [BooleanField(
        "Show Move To Group Button",
        Description = "Whether the button for moving this member to another group is shown.",
        DefaultBooleanValue = true,
        Order = 1,
        Key = AttributeKey.ShowMoveToOtherGroup )]

    [BooleanField(
        "Hide Requirements",
        Description = "Whether this member's requirement status alerts, and the option to refresh them, are hidden.",
        DefaultBooleanValue = false,
        Order = 2,
        Key = AttributeKey.AreRequirementsPubliclyHidden )]

    [BooleanField(
        "Hide Requirement Type Summary",
        Description = "Whether each requirement type's summary text is hidden, when requirements are shown.",
        DefaultBooleanValue = false,
        Order = 3,
        Key = AttributeKey.IsSummaryHidden )]

    [BooleanField(
        "Refresh Requirements on Load",
        Description = "Whether group requirements are recalculated every time the block loads, instead of using cached results.",
        DefaultBooleanValue = false,
        Order = 4,
        Key = AttributeKey.AreRequirementsRefreshedOnLoad )]

    [LinkedPage(
        "Workflow Entry Page",
        Description = "The page used to launch a new workflow of the selected type.",
        DefaultValue = Rock.SystemGuid.Page.WORKFLOW_ENTRY,
        Order = 5,
        Key = AttributeKey.WorkflowEntryPage )]

    [BooleanField(
        "Enable Communications",
        Description = "Whether quick communications can be sent from this block.",
        DefaultBooleanValue = true,
        Order = 6,
        Key = AttributeKey.EnableCommunications )]

    [BooleanField(
        "Enable SMS",
        Description = "Whether SMS is offered as a communication option, when the recipient has a messaging-enabled number. Email is the only option otherwise.",
        DefaultBooleanValue = true,
        Order = 7,
        Key = AttributeKey.EnableSMS )]

    [BooleanField(
        "Append Email Header/Footer",
        Description = "Whether the organization's email header and footer are appended to the message.",
        DefaultBooleanValue = true,
        Order = 8,
        Key = AttributeKey.AppendHeaderFooter )]

    [BooleanField(
        "Allow Sending From Other Email Addresses",
        Description = "Whether the email 'From' address can be changed. When disabled, messages are sent from the logged-in person's email address.",
        DefaultBooleanValue = true,
        Order = 9,
        Key = AttributeKey.AllowSelectingFrom )]

    [SystemPhoneNumberField(
        "Allowed SMS Numbers",
        Description = "The system phone numbers offered as the SMS sender. All authorized numbers are offered if none are selected.",
        IsRequired = false,
        AllowMultiple = true,
        Order = 10,
        Key = AttributeKey.AllowedSMSNumbers )]

    [CustomDropdownListField(
        "Schedule List Format",
        Description = "How each schedule is displayed in the preference list and picker. By time, by name, or both.",
        ListSource = "1^Schedule Time,2^Schedule Name,3^Schedule Time and Name",
        IsRequired = false,
        DefaultValue = "1",
        Order = 11,
        Key = AttributeKey.ScheduleListFormat )]

    [BooleanField(
        "Include Group Name in Breadcrumb",
        Description = "Whether the group's name is included in the breadcrumb trail ahead of the member's name.",
        DefaultBooleanValue = true,
        Order = 12,
        Key = AttributeKey.IncludeGroupNameInBreadcrumb )]

    #endregion Block Attributes

    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Primary )]
    [Rock.SystemGuid.EntityTypeGuid( "443841E5-6D0E-4CF4-83D0-CE8083FF10EA" )]
    [Rock.SystemGuid.BlockTypeGuid( "BB6FB9A3-4177-4702-BC8B-1B254137732F" )]
    public class GroupMemberDetail : RockEntityDetailBlockType<GroupMember, GroupMemberBag>, IBreadCrumbBlock
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string CampusId = "CampusId";
            public const string GroupId = "GroupId";
            public const string GroupMemberId = "GroupMemberId";
            public const string LocationId = "LocationId";
            public const string RegistrationId = "RegistrationId";
            public const string ScheduleId = "ScheduleId";
            public const string ReturnUrl = "returnUrl";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        private static class AttributeKey
        {
            public const string RegistrationPage = "RegistrationPage";
            public const string ShowMoveToOtherGroup = "ShowMoveToOtherGroup";
            public const string AreRequirementsPubliclyHidden = "AreRequirementsPubliclyHidden";
            public const string IsSummaryHidden = "IsSummaryHidden";
            public const string AreRequirementsRefreshedOnLoad = "AreRequirementsRefreshedOnLoad";
            public const string WorkflowEntryPage = "WorkflowEntryPage";
            public const string EnableCommunications = "EnableCommunications";
            public const string EnableSMS = "EnableSMS";
            public const string AppendHeaderFooter = "AppendHeaderFooter";
            public const string AllowSelectingFrom = "AllowSelectingFrom";
            public const string AllowedSMSNumbers = "AllowedSMSNumbers";
            public const string ScheduleListFormat = "ScheduleListFormat";
            public const string IncludeGroupNameInBreadcrumb = "IncludeGroupNameInBreadcrumb";
        }

        #endregion Keys

        #region Fields

        private const string NoLocationPreference = "No Location Preference";

        /// <summary>
        /// The constant value used for naming new fundraising transfer batches.
        /// </summary>
        private const string FundraisingBatchName = "Fundraising Transaction Transfer";

        /// <summary>
        /// The constant Note field of batches used for fundraising transfer transactions.
        /// </summary>
        private const string FundraisingBatchNote = "Fundraising Transfer";

        #endregion Fields

        #region Properties

        /// <summary>
        /// The campus identifier page parameter, carried through to
        /// navigation URLs.
        /// </summary>
        private int? CampusId => PageParameter( PageParameterKey.CampusId ).AsIntegerOrNull();

        /// <summary>
        /// The sign-up project location identifier page parameter.
        /// </summary>
        private int? LocationId => PageParameter( PageParameterKey.LocationId ).AsIntegerOrNull();

        /// <summary>
        /// The sign-up project schedule identifier page parameter.
        /// </summary>
        private int? ScheduleId => PageParameter( PageParameterKey.ScheduleId ).AsIntegerOrNull()
            ?? Rock.Utility.IdHasher.Instance.GetId( PageParameter( PageParameterKey.ScheduleId ) );

        /// <summary>
        /// Sign-up mode is active when both a location and a schedule are
        /// supplied, as when reached from a Sign-Up project's attendee list.
        /// </summary>
        private bool IsSignUpMode => LocationId.ToIntSafe() > 0 && ScheduleId.ToIntSafe() > 0;

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                var box = new DetailBlockBox<GroupMemberBag, GroupMemberDetailOptionsBag>();

                // A lookup was attempted when a non-zero GroupMemberId parameter was supplied, even as an IdKey or Guid.
                var groupMemberKey = PageParameter( PageParameterKey.GroupMemberId );
                var isLookupAttempt = groupMemberKey.IsNotNullOrWhiteSpace() && groupMemberKey.AsIntegerOrNull() != 0;

                box.ErrorMessage = isLookupAttempt
                    ? "Group Member not found. Group Member may have been moved to another group or deleted."
                    : "An incorrect querystring parameter was used. A valid GroupMemberId or GroupId parameter is required.";

                PrepareDetailBox( box, entity );

                return box;
            }

            return GetDetailBox( entity );
        }

        /// <summary>
        /// Builds the fully populated detail box for a group member. Used
        /// both for the initial render and when the sign-up flow swaps in an
        /// existing member.
        /// </summary>
        /// <param name="entity">The group member to describe.</param>
        /// <returns>The detail box.</returns>
        private DetailBlockBox<GroupMemberBag, GroupMemberDetailOptionsBag> GetDetailBox( GroupMember entity )
        {
            var box = new DetailBlockBox<GroupMemberBag, GroupMemberDetailOptionsBag>();
            var isReadOnly = GetIsReadOnly( entity );
            string editModeMessage;

            // System members are always read-only, regardless of authorization.
            if ( entity.IsSystem )
            {
                editModeMessage = EditModeMessage.ReadOnlySystem( Model.Group.FriendlyTypeName );
            }
            else
            {
                editModeMessage = isReadOnly
                    ? EditModeMessage.ReadOnlyEditActionNotAllowed( Model.Group.FriendlyTypeName )
                    : string.Empty;
            }

            box.IsEditable = !isReadOnly;

            box.Entity = GetEntityBagForEdit( entity );
            box.Options = GetBoxOptions( entity, isReadOnly, editModeMessage );
            box.NavigationUrls = GetBoxNavigationUrls( entity );

            if ( IsSignUpMode )
            {
                SetSignUpAssignmentAttributes( box.Entity, box.Options, entity, isReadOnly );
            }

            PrepareDetailBox( box, entity );

            return box;
        }

        /// <summary>
        /// Determines whether the form is read-only for the current person,
        /// either from failing the edit authorization or the member being a
        /// system record.
        /// </summary>
        /// <param name="entity">The group member being viewed or edited.</param>
        /// <returns><c>true</c> if the form is read-only.</returns>
        private bool GetIsReadOnly( GroupMember entity )
        {
            return entity.IsSystem || !IsAuthorizedToEdit( entity.Group );
        }

        /// <inheritdoc/>
        protected override GroupMember GetInitialEntity()
        {
            var entity = GetInitialEntity<GroupMember, GroupMemberService>( RockContext, PageParameterKey.GroupMemberId );

            if ( entity != null && entity.Id == 0 )
            {
                entity = ApplyNewGroupMemberDefaultValues( entity );
            }

            return entity;
        }

        /// <summary>
        /// Applies default values to a new <see cref="GroupMember"/> from
        /// the GroupId page parameter. Returns null when no valid group was
        /// supplied, since a member cannot be added without one.
        /// </summary>
        /// <param name="entity">The new group member entity.</param>
        /// <returns>The defaulted entity, or null when the group is missing.</returns>
        private GroupMember ApplyNewGroupMemberDefaultValues( GroupMember entity )
        {
            var groupKey = PageParameter( PageParameterKey.GroupId );
            var group = groupKey.IsNotNullOrWhiteSpace()
                ? new GroupService( RockContext ).Get( groupKey, !PageCache.Layout.Site.DisablePredictableIds )
                : null;

            if ( group == null )
            {
                return null;
            }

            entity.GroupId = group.Id;
            entity.Group = group;
            entity.GroupRoleId = GroupTypeCache.Get( group.GroupTypeId )?.DefaultGroupRoleId ?? 0;
            entity.GroupMemberStatus = GroupMemberStatus.Active;
            entity.DateTimeAdded = RockDateTime.Now;

            return entity;
        }

        /// <summary>
        /// Determines whether the current person is authorized to edit the
        /// group member. Edit rights come from block EDIT, group EDIT,
        /// group MANAGE_MEMBERS, or, in sign-up mode only, group SCHEDULE.
        /// </summary>
        /// <param name="group">The group the member belongs to.</param>
        /// <returns><c>true</c> if the current person may edit the member.</returns>
        private bool IsAuthorizedToEdit( Model.Group group )
        {
            if ( BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return true;
            }

            if ( group == null )
            {
                return false;
            }

            return group.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson )
                || group.IsAuthorized( Authorization.MANAGE_MEMBERS, RequestContext.CurrentPerson )
                || ( IsSignUpMode && group.IsAuthorized( Authorization.SCHEDULE, RequestContext.CurrentPerson ) );
        }

        /// <summary>
        /// Gets the options bag that describes everything the client needs
        /// to render the block.
        /// </summary>
        /// <param name="entity">The group member being viewed or edited.</param>
        /// <param name="isReadOnly">Whether the form is read-only for the current person.</param>
        /// <param name="editModeMessage">The message explaining why the form is read-only.</param>
        /// <returns>The options bag.</returns>
        private GroupMemberDetailOptionsBag GetBoxOptions( GroupMember entity, bool isReadOnly, string editModeMessage )
        {
            var group = entity.Group;
            var groupType = GroupTypeCache.Get( group.GroupTypeId );
            var isNewMember = entity.Id == 0;

            var options = new GroupMemberDetailOptionsBag
            {
                EditModeMessage = editModeMessage,
                IsSignUpMode = IsSignUpMode,

                // Header state.
                GroupTerm = groupType.GroupTerm,
                GroupMemberTerm = groupType.GroupMemberTerm,
                GroupIconCssClass = groupType.IconCssClass.IsNotNullOrWhiteSpace() ? groupType.IconCssClass : "ti ti-user",
                AddedDateText = entity.DateTimeAdded.HasValue ? $"Added: {entity.DateTimeAdded.Value.ToShortDateString()}" : string.Empty,
                IsArchived = entity.IsArchived,
                IsSaveThenAddShown = isNewMember && !isReadOnly,
                IsMoveButtonShown = !isNewMember && !isReadOnly && GetAttributeValue( AttributeKey.ShowMoveToOtherGroup ).AsBoolean( true ),
                IsCommunicationButtonShown = !isNewMember && GetAttributeValue( AttributeKey.EnableCommunications ).AsBoolean( true ),
                IsNotifiedShown = BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ),

                // Form options.
                StatusItems = typeof( GroupMemberStatus ).ToEnumListItemBag(),
                CommunicationPreferenceItems = GetCommunicationPreferenceItems(),
                AreChatPreferencesShown = ChatHelper.IsChatEnabled && group.GetIsChatEnabled(),

                // Scheduling.
                IsSchedulingEnabled = groupType.IsSchedulingEnabled && !IsSignUpMode,
                ScheduleTemplateItems = GetScheduleTemplateItems( groupType.Id ),

                // Person display for edit mode, where the person can no longer be changed.
                PersonIdKey = entity.Person?.IdKey,
                PersonPhotoUrl = entity.Person != null ? Person.GetPersonPhotoUrl( entity.Person ) : null,

                LinkedRegistrations = GetLinkedRegistrations( entity ),
                SignatureDocument = GetSignatureDocumentStatus( entity )
            };

            SetRoleOptions( options, entity, groupType );
            SetViewableAttributes( options, entity, isReadOnly );
            SetRequirementOptions( options, entity, groupType );

            return options;
        }

        /// <summary>
        /// Sets the group requirement options: the requirement block
        /// settings, the identifiers used by requirement plumbing, and the
        /// inline alert bags for the member's current requirement statuses.
        /// </summary>
        /// <param name="options">The options bag to populate.</param>
        /// <param name="entity">The group member being viewed or edited.</param>
        /// <param name="groupType">The group's group type cache.</param>
        private void SetRequirementOptions( GroupMemberDetailOptionsBag options, GroupMember entity, GroupTypeCache groupType )
        {
            var group = entity.Group;

            options.AreRequirementsHidden = GetAttributeValue( AttributeKey.AreRequirementsPubliclyHidden ).AsBoolean();
            options.IsRequirementSummaryHidden = GetAttributeValue( AttributeKey.IsSummaryHidden ).AsBoolean();
            options.WorkflowEntryPageValue = GetAttributeValue( AttributeKey.WorkflowEntryPage );
            options.HasGroupRequirements = group.GetGroupRequirements( RockContext ).Any();
            options.RequirementAlerts = new List<GroupMemberRequirementAlertBag>();

            if ( !options.HasGroupRequirements )
            {
                return;
            }

            options.GroupGuid = group.Guid;
            options.GroupRoleGuid = groupType.Roles.FirstOrDefault( r => r.Id == entity.GroupRoleId )?.Guid;
            options.GroupMemberGuid = entity.Id != 0 ? entity.Guid : ( Guid? ) null;
            options.PersonGuid = entity.Person?.Guid;

            // Workflow links write immediately, so they are only offered on a saved member; a pending role change does not disable them (WebForms parity).
            options.IsRequirementInteractionDisabled = entity.Id == 0;

            // Don't check or show requirements until a person is selected.
            if ( entity.PersonId == 0 )
            {
                return;
            }

            // Recalculating here also saves the results, so only do it for an existing, unchanged member.
            if ( GetAttributeValue( AttributeKey.AreRequirementsRefreshedOnLoad ).AsBoolean()
                && entity.Id != 0
                && !entity.IsNewOrChangedGroupMember( RockContext ) )
            {
                entity.CalculateRequirements( RockContext, true );
            }

            // Hidden requirements are enforced here so their data is never serialized to the client, matching WebForms never rendering it.
            if ( options.AreRequirementsHidden )
            {
                return;
            }

            options.RequirementAlerts = GetRequirementAlerts( entity, entity.GroupRoleId, out var calculationErrors );
            options.RequirementCalculationErrors = calculationErrors;
        }

        /// <summary>
        /// Gets the inline requirement alerts for the member's requirement
        /// statuses against the selected role. Only statuses the current
        /// person may view are included; Not Applicable and Error statuses
        /// never render (errors surface through the calculation errors
        /// text instead).
        /// </summary>
        /// <param name="entity">The group member being viewed or edited.</param>
        /// <param name="selectedRoleId">The currently selected role identifier.</param>
        /// <param name="calculationErrors">The calculation error details, or null when every calculation succeeded.</param>
        /// <returns>The requirement alert bags.</returns>
        private List<GroupMemberRequirementAlertBag> GetRequirementAlerts( GroupMember entity, int selectedRoleId, out string calculationErrors )
        {
            calculationErrors = null;

            List<GroupRequirementStatus> statusList = null;

            if ( entity.Id != 0 && !entity.IsNewOrChangedGroupMember( RockContext ) )
            {
                statusList = entity.GetGroupRequirementsStatuses( RockContext )?.ToList();
            }

            if ( statusList?.Any() != true && entity.PersonId != 0 )
            {
                statusList = entity.Group.PersonMeetsGroupRequirements( RockContext, entity.PersonId, selectedRoleId )?.ToList<GroupRequirementStatus>();
            }

            if ( statusList == null || !statusList.Any() )
            {
                return new List<GroupMemberRequirementAlertBag>();
            }

            var requirementsWithErrors = statusList
                .Where( s => s.MeetsGroupRequirement == MeetsGroupRequirement.Error )
                .ToList();

            if ( requirementsWithErrors.Any() )
            {
                calculationErrors = requirementsWithErrors
                    .Select( s => $"{s.GroupRequirement.GroupRequirementType.Name}: {s.CalculationException?.Message}" )
                    .ToList()
                    .AsDelimited( Environment.NewLine );
            }

            var isSummaryHidden = GetAttributeValue( AttributeKey.IsSummaryHidden ).AsBoolean();
            var isLeader = IsCurrentPersonLeaderOfGroup( entity.GroupId );

            // Order matches the WebForms container: uncategorized types first, then by category name, then by type name.
            var visibleStatuses = statusList
                .Where( s =>
                    s.MeetsGroupRequirement != MeetsGroupRequirement.NotApplicable
                    && s.MeetsGroupRequirement != MeetsGroupRequirement.Error
                    && s.GroupRequirement.GroupRequirementType.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                .OrderBy( s => s.GroupRequirement.GroupRequirementType.CategoryId.HasValue )
                .ThenBy( s => s.GroupRequirement.GroupRequirementType.CategoryId.HasValue ? s.GroupRequirement.GroupRequirementType.Category.Name : string.Empty )
                .ThenBy( s => s.GroupRequirement.GroupRequirementType.Name )
                .ToList();

            var memberRequirementIds = visibleStatuses
                .Where( s => s.GroupMemberRequirementId.HasValue )
                .Select( s => s.GroupMemberRequirementId.Value )
                .ToList();

            var memberRequirementsById = memberRequirementIds.Any()
                ? new GroupMemberRequirementService( RockContext ).Queryable().AsNoTracking()
                    .Where( r => memberRequirementIds.Contains( r.Id ) )
                    .Select( r => new
                    {
                        r.Id,
                        r.Guid,
                        r.WasManuallyCompleted,
                        r.WasOverridden,
                        r.OverriddenDateTime,
                        OverriddenByPersonName = r.OverriddenByPersonAlias.Person.NickName + " " + r.OverriddenByPersonAlias.Person.LastName
                    } )
                    .ToList()
                    .ToDictionary( r => r.Id, r => r )
                : null;

            // Workflow links only render when the Workflow Entry page setting resolves.
            var hasWorkflowEntryPage = this.GetLinkedPageUrl( AttributeKey.WorkflowEntryPage ).IsNotNullOrWhiteSpace();

            var alerts = new List<GroupMemberRequirementAlertBag>();

            foreach ( var status in visibleStatuses )
            {
                var requirementType = status.GroupRequirement.GroupRequirementType;

                var memberRequirement = status.GroupMemberRequirementId.HasValue && memberRequirementsById?.TryGetValue( status.GroupMemberRequirementId.Value, out var foundRequirement ) == true
                    ? foundRequirement
                    : null;

                // A stored resolution presents as met with no checkbox, matching the WebForms
                // card; only an unresolved requirement offers a checkbox, held client-side until save.
                var isOverridden = memberRequirement?.WasOverridden == true;
                var isResolved = memberRequirement?.WasManuallyCompleted == true || isOverridden;
                var effectiveStatus = isResolved ? MeetsGroupRequirement.Meets : status.MeetsGroupRequirement;
                var isMet = effectiveStatus == MeetsGroupRequirement.Meets;

                var hasDoesNotMeetWorkflow = hasWorkflowEntryPage
                    && requirementType.DoesNotMeetWorkflowTypeId.HasValue
                    && !requirementType.ShouldAutoInitiateDoesNotMeetWorkflow
                    && effectiveStatus == MeetsGroupRequirement.NotMet;

                var hasWarningWorkflow = hasWorkflowEntryPage
                    && requirementType.WarningWorkflowTypeId.HasValue
                    && !requirementType.ShouldAutoInitiateWarningWorkflow
                    && effectiveStatus == MeetsGroupRequirement.MeetsWithWarning;

                alerts.Add( new GroupMemberRequirementAlertBag
                {
                    Title = requirementType.Name,
                    Summary = isSummaryHidden ? string.Empty : requirementType.Summary,
                    StatusText = GetRequirementStatusText( requirementType, effectiveStatus ),
                    MetStatusText = GetRequirementStatusText( requirementType, MeetsGroupRequirement.Meets ),
                    MeetsGroupRequirement = effectiveStatus,
                    TypeIconCssClass = requirementType.IconCssClass,
                    CanOverride = !isMet
                        && ( ( status.GroupRequirement.AllowLeadersToOverride && isLeader )
                            || requirementType.IsAuthorized( Authorization.OVERRIDE, RequestContext.CurrentPerson ) ),
                    IsManualRequirement = requirementType.RequirementCheckType == RequirementCheckType.Manual,
                    ManualCheckboxLabel = requirementType.CheckboxLabel.IsNotNullOrWhiteSpace()
                        ? requirementType.CheckboxLabel
                        : requirementType.Name,
                    DoesNotMeetWorkflowLinkText = hasDoesNotMeetWorkflow
                        ? ( requirementType.DoesNotMeetWorkflowLinkText.IsNotNullOrWhiteSpace() ? requirementType.DoesNotMeetWorkflowLinkText : "Requirement Not Met" )
                        : null,
                    WarningWorkflowLinkText = hasWarningWorkflow
                        ? ( requirementType.WarningWorkflowLinkText.IsNotNullOrWhiteSpace() ? requirementType.WarningWorkflowLinkText : "Requirement Met With Warning" )
                        : null,
                    DueDateText = !isMet && status.RequirementDueDate.HasValue
                        ? $"Due: {status.RequirementDueDate.Value.ToShortDateString()}"
                        : null,
                    OverriddenText = isOverridden
                        ? $"Requirement Marked Met by {memberRequirement.OverriddenByPersonName} on {memberRequirement.OverriddenDateTime?.ToShortDateString()}"
                        : null,
                    GroupRequirementGuid = status.GroupRequirement.Guid,
                    GroupMemberRequirementGuid = memberRequirement?.Guid
                } );
            }

            return alerts;
        }

        /// <summary>
        /// Gets the status line for a requirement alert from the
        /// requirement type's positive, negative, or warning label, with
        /// the same defaults the WebForms requirement card used.
        /// </summary>
        /// <param name="requirementType">The group requirement type.</param>
        /// <param name="meetsGroupRequirement">The met state to describe.</param>
        /// <returns>The status text.</returns>
        private string GetRequirementStatusText( GroupRequirementType requirementType, MeetsGroupRequirement meetsGroupRequirement )
        {
            switch ( meetsGroupRequirement )
            {
                case MeetsGroupRequirement.Meets:
                    return requirementType.PositiveLabel.IsNotNullOrWhiteSpace() ? requirementType.PositiveLabel : "Requirement Met";

                case MeetsGroupRequirement.NotMet:
                    return requirementType.NegativeLabel.IsNotNullOrWhiteSpace() ? requirementType.NegativeLabel : "Requirement Not Met";

                case MeetsGroupRequirement.MeetsWithWarning:
                    return requirementType.WarningLabel.IsNotNullOrWhiteSpace() ? requirementType.WarningLabel : "Requirement Met With Warning";

                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Determines whether the current person holds a leader role in the
        /// group, which can grant requirement override rights.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <returns><c>true</c> if the current person is a leader of the group.</returns>
        private bool IsCurrentPersonLeaderOfGroup( int groupId )
        {
            if ( RequestContext.CurrentPerson == null )
            {
                return false;
            }

            return new GroupMemberService( RockContext ).GetByGroupId( groupId )
                .Where( m => m.GroupRole.IsLeader )
                .Select( m => m.PersonId )
                .Contains( RequestContext.CurrentPerson.Id );
        }

        /// <summary>
        /// Validates that every must-meet requirement is satisfied for a member being added or
        /// changed, treating the requirements resolved client-side (manual mark or leader
        /// override) as met. This closes the manual-requirement enforcement gap, where the
        /// entity's own check excludes manual types because the WebForms card wrote resolutions
        /// straight to the database and had nothing to write to while adding.
        /// </summary>
        /// <returns><c>true</c> when all must-meet requirements are satisfied or resolved.</returns>
        private bool TryValidateMustMeetRequirements( GroupMember entity, GroupMemberBag bag, out string errorMessage )
        {
            errorMessage = null;

            // Businesses and REST users are exempt, matching the entity's own IsValidGroupMember check.
            var restUserRecordTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_RESTUSER.AsGuid() ).Id;
            if ( entity.Person != null && ( entity.Person.IsBusiness() || entity.Person.RecordTypeValueId == restUserRecordTypeId ) )
            {
                return true;
            }

            // Nothing to enforce when the group has no must-meet requirements, matching the entity's own guard.
            if ( !entity.Group.GetGroupRequirements( RockContext ).Any( r => r.MustMeetRequirementToAddMember ) )
            {
                return true;
            }

            var authorizedResolutions = GetAuthorizedRequirementResolutions( entity, bag );
            var resolvedRequirementGuids = new HashSet<Guid>( authorizedResolutions.ManualGuids );
            resolvedRequirementGuids.UnionWith( authorizedResolutions.OverrideGuids );

            // A manual requirement is only enforceable when its checkbox was sent to the client, otherwise WebForms behavior applies.
            var areRequirementsHidden = GetAttributeValue( AttributeKey.AreRequirementsPubliclyHidden ).AsBoolean();

            var unmetRequirementNames = entity.Group
                .PersonMeetsGroupRequirements( RockContext, entity.PersonId, entity.GroupRoleId )
                .Where( s => s.MeetsGroupRequirement == MeetsGroupRequirement.NotMet
                    && s.GroupRequirement.MustMeetRequirementToAddMember
                    && !resolvedRequirementGuids.Contains( s.GroupRequirement.Guid )
                    && IsMustMeetRequirementEnforceable( s.GroupRequirement.GroupRequirementType, areRequirementsHidden ) )
                .Select( s => s.GroupRequirement.GroupRequirementType.Name )
                .ToList();

            if ( unmetRequirementNames.Any() )
            {
                errorMessage = $"{entity.Person.FullName} must meet the following requirements before being added or made an active member in group '{entity.Group.Name}': {unmetRequirementNames.AsDelimited( ", " )}";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether an unmet must-meet requirement may block the save. Non-manual types
        /// always block, as they did in WebForms. A manual type can only be satisfied through the
        /// client-side checkbox, so it blocks only when that checkbox could have rendered: the
        /// requirements are not hidden and the current person can view the requirement type.
        /// Otherwise the manual type is skipped, matching the WebForms check that never enforced it.
        /// </summary>
        /// <param name="requirementType">The requirement type being evaluated.</param>
        /// <param name="areRequirementsHidden">Whether the block hides the requirements section.</param>
        /// <returns><c>true</c> if the unmet requirement should block the save.</returns>
        private bool IsMustMeetRequirementEnforceable( GroupRequirementType requirementType, bool areRequirementsHidden )
        {
            if ( requirementType.RequirementCheckType != RequirementCheckType.Manual )
            {
                return true;
            }

            return !areRequirementsHidden
                && requirementType.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Builds the sets of group requirement guids the current person is actually allowed to
        /// resolve from the client-supplied lists, applying the manual-type gate for manual marks
        /// and the override authorization gate for overrides. Shared by validation and the save so
        /// a crafted request cannot pass must-meet validation with a resolution that would not be
        /// written.
        /// </summary>
        /// <returns>The authorized manual and override requirement guids.</returns>
        private ( HashSet<Guid> ManualGuids, HashSet<Guid> OverrideGuids ) GetAuthorizedRequirementResolutions( GroupMember entity, GroupMemberBag bag )
        {
            var manualGuids = new HashSet<Guid>();
            var overrideGuids = new HashSet<Guid>();

            var requestedManualGuids = bag.ManuallyMetRequirementGuids ?? new List<Guid>();
            var requestedOverrideGuids = bag.OverriddenRequirementGuids ?? new List<Guid>();

            if ( !requestedManualGuids.Any() && !requestedOverrideGuids.Any() )
            {
                return ( manualGuids, overrideGuids );
            }

            // The leader status is constant across the loop, so query it at most once.
            var isCurrentPersonLeader = IsCurrentPersonLeaderOfGroup( entity.GroupId );

            foreach ( var groupRequirement in entity.Group.GetGroupRequirements( RockContext ) )
            {
                if ( requestedManualGuids.Contains( groupRequirement.Guid )
                    && groupRequirement.GroupRequirementType.RequirementCheckType == RequirementCheckType.Manual )
                {
                    manualGuids.Add( groupRequirement.Guid );
                }

                if ( requestedOverrideGuids.Contains( groupRequirement.Guid )
                    && ( ( groupRequirement.AllowLeadersToOverride && isCurrentPersonLeader.Value )
                        || groupRequirement.GroupRequirementType.IsAuthorized( Authorization.OVERRIDE, RequestContext.CurrentPerson ) ) )
                {
                    overrideGuids.Add( groupRequirement.Guid );
                }
            }

            return ( manualGuids, overrideGuids );
        }

        /// <summary>
        /// Writes the GroupMemberRequirement rows for the resolutions checked client-side and
        /// held until save. Resolutions are add-only: a stored resolution offers no checkbox,
        /// so nothing here is ever cleared. Only resolutions the current person is authorized to
        /// write are applied.
        /// </summary>
        /// <param name="entity">The saved group member the requirement rows attach to.</param>
        /// <param name="bag">The bag carrying the checked requirement resolutions.</param>
        private void ReconcileRequirementResolutions( GroupMember entity, GroupMemberBag bag )
        {
            var authorizedResolutions = GetAuthorizedRequirementResolutions( entity, bag );

            if ( !authorizedResolutions.ManualGuids.Any() && !authorizedResolutions.OverrideGuids.Any() )
            {
                return;
            }

            var groupMemberRequirementService = new GroupMemberRequirementService( RockContext );
            var existingRows = groupMemberRequirementService.Queryable()
                .Where( r => r.GroupMemberId == entity.Id )
                .ToList();

            foreach ( var groupRequirement in entity.Group.GetGroupRequirements( RockContext ) )
            {
                var isManualDesired = authorizedResolutions.ManualGuids.Contains( groupRequirement.Guid );
                var isOverrideDesired = authorizedResolutions.OverrideGuids.Contains( groupRequirement.Guid );

                if ( !isManualDesired && !isOverrideDesired )
                {
                    continue;
                }

                var memberRequirement = existingRows.FirstOrDefault( r => r.GroupRequirementId == groupRequirement.Id );

                if ( memberRequirement == null )
                {
                    memberRequirement = new GroupMemberRequirement
                    {
                        GroupRequirementId = groupRequirement.Id,
                        GroupMemberId = entity.Id
                    };
                    groupMemberRequirementService.Add( memberRequirement );
                }

                // Stamp only on the unresolved-to-resolved transition so an existing resolution keeps its original who and when.
                if ( isManualDesired && !memberRequirement.WasManuallyCompleted )
                {
                    memberRequirement.WasManuallyCompleted = true;
                    memberRequirement.ManuallyCompletedByPersonAliasId = RequestContext.CurrentPerson?.PrimaryAliasId;
                    memberRequirement.ManuallyCompletedDateTime = RockDateTime.Now;
                }

                if ( isOverrideDesired && !memberRequirement.WasOverridden )
                {
                    memberRequirement.WasOverridden = true;
                    memberRequirement.OverriddenByPersonAliasId = RequestContext.CurrentPerson?.PrimaryAliasId;
                    memberRequirement.OverriddenDateTime = RockDateTime.Now;
                }

                memberRequirement.RequirementMetDateTime = memberRequirement.RequirementMetDateTime ?? RockDateTime.Now;
            }
        }

        /// <summary>
        /// Gets the communication preference radio options.
        /// </summary>
        /// <returns>The communication preference items.</returns>
        private List<ListItemBag> GetCommunicationPreferenceItems()
        {
            return new List<ListItemBag>
            {
                new ListItemBag { Value = ( ( int ) CommunicationType.RecipientPreference ).ToString(), Text = "No Preference" },
                new ListItemBag { Value = ( ( int ) CommunicationType.Email ).ToString(), Text = "Email" },
                new ListItemBag { Value = ( ( int ) CommunicationType.SMS ).ToString(), Text = "SMS" }
            };
        }

        /// <summary>
        /// Gets the schedule templates available for the group's group type.
        /// </summary>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <returns>The schedule template items.</returns>
        private List<ListItemBag> GetScheduleTemplateItems( int groupTypeId )
        {
            return new GroupMemberScheduleTemplateService( RockContext ).Queryable()
                .AsNoTracking()
                .Where( t => !t.GroupTypeId.HasValue || t.GroupTypeId == groupTypeId )
                .OrderBy( t => t.Name )
                .Select( t => new { t.Id, t.Name } )
                .ToList()
                .Select( t => new ListItemBag { Value = t.Id.ToString(), Text = t.Name } )
                .ToList();
        }

        /// <summary>
        /// Sets the role options, excluding or locking roles controlled by
        /// Group Sync. The message doubles as the role field's tooltip in
        /// both the locked and the roles-removed cases.
        /// </summary>
        /// <param name="options">The options bag to populate.</param>
        /// <param name="entity">The group member being viewed or edited.</param>
        /// <param name="groupType">The group's group type cache.</param>
        private void SetRoleOptions( GroupMemberDetailOptionsBag options, GroupMember entity, GroupTypeCache groupType )
        {
            var syncedRoleIds = new GroupSyncService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Where( s => s.GroupId == entity.GroupId )
                .Select( s => s.GroupTypeRoleId )
                .ToList();

            var roles = groupType.Roles.OrderBy( r => r.Order ).ToList();

            if ( syncedRoleIds.Any() )
            {
                if ( syncedRoleIds.Contains( entity.GroupRoleId ) && entity.Id != 0 )
                {
                    options.IsRoleLockedBySync = true;
                    options.RoleLockedMessage = "Role selection disabled because this member was added to this role automatically by Group Sync.";
                }
                else
                {
                    roles = roles.Where( r => !syncedRoleIds.Contains( r.Id ) ).ToList();
                    options.RoleLockedMessage = "Roles used for Group Sync cannot be used for manual additions and so are not being displayed.";
                }
            }

            options.RoleItems = roles
                .Select( r => new ListItemBag { Value = r.Id.ToString(), Text = r.Name } )
                .ToList();
        }

        /// <summary>
        /// Gets the registrations linked to the group member. Text is the
        /// registration instance name, value is the registration page URL.
        /// </summary>
        /// <param name="entity">The group member being viewed or edited.</param>
        /// <returns>The linked registration items.</returns>
        private List<ListItemBag> GetLinkedRegistrations( GroupMember entity )
        {
            if ( entity.Id == 0 )
            {
                return new List<ListItemBag>();
            }

            var registrations = new RegistrationRegistrantService( RockContext )
                .Queryable().AsNoTracking()
                .Where( r =>
                    r.Registration != null &&
                    r.Registration.RegistrationInstance != null &&
                    r.GroupMemberId.HasValue &&
                    r.GroupMemberId.Value == entity.Id )
                .Select( r => new
                {
                    r.Registration.Id,
                    r.Registration.RegistrationInstance.Name
                } )
                .ToList();

            return registrations
                .Select( r => new ListItemBag
                {
                    Text = r.Name,
                    Value = this.GetLinkedPageUrl( AttributeKey.RegistrationPage, new Dictionary<string, string>
                    {
                        [PageParameterKey.RegistrationId] = r.Id.ToString()
                    } )
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the required signature document state, or null when the
        /// group has no required signature document template. The warning
        /// alert state is only set when no signed document exists yet.
        /// </summary>
        /// <param name="entity">The group member being viewed or edited.</param>
        /// <returns>A <see cref="SignatureDocumentStatusBag"/>, or null.</returns>
        private SignatureDocumentStatusBag GetSignatureDocumentStatus( GroupMember entity )
        {
            var template = entity.Group.RequiredSignatureDocumentTemplate;

            if ( template == null )
            {
                return null;
            }

            var statusBag = new SignatureDocumentStatusBag
            {
                BinaryFileTypeGuid = template.BinaryFileType?.Guid
            };

            // No person means nothing to warn about yet (still adding).
            if ( entity.Person == null )
            {
                return statusBag;
            }

            var documents = new SignatureDocumentService( RockContext )
                .Queryable().AsNoTracking()
                .Where( d =>
                    d.SignatureDocumentTemplateId == template.Id &&
                    d.AppliesToPersonAlias.PersonId == entity.PersonId )
                .Select( d => new
                {
                    d.Status,
                    d.LastInviteDate
                } )
                .ToList();

            if ( documents.Any( d => d.Status == SignatureDocumentStatus.Signed ) )
            {
                return statusBag;
            }

            statusBag.IsRequired = true;

            var lastSent = documents.Any( d => d.Status == SignatureDocumentStatus.Sent )
                ? documents.Where( d => d.Status == SignatureDocumentStatus.Sent ).Max( d => d.LastInviteDate )
                : null;

            // The message renders as HTML on the client, so the data-driven parts are encoded.
            if ( lastSent.HasValue )
            {
                statusBag.ButtonText = "Resend Signature Request";
                statusBag.Message = $"A signed {template.Name.EncodeHtml()} document has not yet been received for {entity.Person.FullName.EncodeHtml()}. The last request was sent <strong>{lastSent.Value.ToElapsedString()}</strong>.";
            }
            else
            {
                statusBag.ButtonText = "Send Signature Request";
                statusBag.Message = $"The required {template.Name.EncodeHtml()} document has not yet been sent to {entity.Person.NickName.EncodeHtml()} for signing.";
            }

            return statusBag;
        }

        /// <summary>
        /// Gets the attribute keys the current person may edit. Group
        /// ADMINISTRATE grants every attribute, otherwise per-attribute
        /// EDIT authorization applies, and a read-only form grants none.
        /// </summary>
        /// <param name="attributedEntity">The entity whose attributes are being split.</param>
        /// <param name="group">The group used for the ADMINISTRATE check.</param>
        /// <param name="isReadOnly">Whether the form is read-only for the current person.</param>
        /// <returns>The editable attribute keys.</returns>
        private List<string> GetEditableAttributeKeys( IHasAttributes attributedEntity, Model.Group group, bool isReadOnly )
        {
            if ( isReadOnly )
            {
                return new List<string>();
            }

            if ( group.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
            {
                return attributedEntity.Attributes.Select( a => a.Key ).ToList();
            }

            return attributedEntity.Attributes
                .Where( a => a.Value.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                .Select( a => a.Key )
                .ToList();
        }

        /// <summary>
        /// Gets the attribute keys the current person may view but not
        /// edit. Group ADMINISTRATE grants every non-editable attribute,
        /// otherwise per-attribute VIEW authorization applies.
        /// </summary>
        /// <param name="attributedEntity">The entity whose attributes are being split.</param>
        /// <param name="group">The group used for the ADMINISTRATE check.</param>
        /// <param name="editableKeys">The already-editable keys to exclude.</param>
        /// <returns>The viewable attribute keys.</returns>
        private List<string> GetViewableAttributeKeys( IHasAttributes attributedEntity, Model.Group group, List<string> editableKeys )
        {
            if ( group.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
            {
                return attributedEntity.Attributes
                    .Where( a => !editableKeys.Contains( a.Key ) )
                    .Select( a => a.Key )
                    .ToList();
            }

            return attributedEntity.Attributes
                .Where( a => !editableKeys.Contains( a.Key ) && a.Value.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                .Select( a => a.Key )
                .ToList();
        }

        /// <summary>
        /// Sets the read-only group member attributes on the options bag.
        /// </summary>
        /// <param name="options">The options bag to populate.</param>
        /// <param name="entity">The group member being viewed or edited.</param>
        /// <param name="isReadOnly">Whether the form is read-only for the current person.</param>
        private void SetViewableAttributes( GroupMemberDetailOptionsBag options, GroupMember entity, bool isReadOnly )
        {
            var editableKeys = GetEditableAttributeKeys( entity, entity.Group, isReadOnly );
            var viewableKeys = GetViewableAttributeKeys( entity, entity.Group, editableKeys );

            // Borrow a bag so the standard helper produces the client-shaped view attributes.
            var holder = new GroupMemberBag();
            holder.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson, enforceSecurity: false, attributeFilter: a => viewableKeys.Contains( a.Key ) );

            options.ViewableAttributes = holder.Attributes;
            options.ViewableAttributeValues = holder.AttributeValues;
        }

        /// <summary>
        /// Sets the sign-up assignment attributes on the entity bag and
        /// options bag from the GroupMemberAssignment matching the sign-up
        /// location and schedule, or a new assignment when none exists.
        /// </summary>
        /// <param name="bag">The entity bag to receive the editable values.</param>
        /// <param name="options">The options bag to receive the attribute definitions.</param>
        /// <param name="entity">The group member being viewed or edited.</param>
        /// <param name="isReadOnly">Whether the form is read-only for the current person.</param>
        private void SetSignUpAssignmentAttributes( GroupMemberBag bag, GroupMemberDetailOptionsBag options, GroupMember entity, bool isReadOnly )
        {
            var groupMemberId = entity.Id;
            var assignment = new GroupMemberAssignmentService( RockContext )
                .Queryable()
                .AsNoTracking()
                .FirstOrDefault( a =>
                    a.GroupMemberId == groupMemberId
                    && a.LocationId == LocationId.Value
                    && a.ScheduleId == ScheduleId.Value )
                ?? new GroupMemberAssignment { GroupId = entity.GroupId };

            assignment.LoadAttributes( RockContext );

            var editableKeys = GetEditableAttributeKeys( assignment, entity.Group, isReadOnly );
            var viewableKeys = GetViewableAttributeKeys( assignment, entity.Group, editableKeys );

            var editHolder = new GroupMemberBag();
            editHolder.LoadAttributesAndValuesForPublicEdit( assignment, RequestContext.CurrentPerson, enforceSecurity: false, attributeFilter: a => editableKeys.Contains( a.Key ) );
            options.AssignmentAttributes = editHolder.Attributes;
            bag.AssignmentAttributeValues = editHolder.AttributeValues;

            var viewHolder = new GroupMemberBag();
            viewHolder.LoadAttributesAndValuesForPublicView( assignment, RequestContext.CurrentPerson, enforceSecurity: false, attributeFilter: a => viewableKeys.Contains( a.Key ) );
            options.ViewableAssignmentAttributes = viewHolder.Attributes;
            options.ViewableAssignmentAttributeValues = viewHolder.AttributeValues;
        }

        /// <summary>
        /// Gets the navigation URLs required by the client. Save and Cancel
        /// both go to the returnUrl page parameter when present, otherwise
        /// to the parent page.
        /// </summary>
        /// <param name="entity">The group member being viewed or edited.</param>
        /// <returns>A dictionary of key and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls( GroupMember entity )
        {
            var returnUrl = PageParameter( PageParameterKey.ReturnUrl );

            if ( returnUrl.IsNotNullOrWhiteSpace() )
            {
                // The client is responsible for making this redirect-safe.
                return new Dictionary<string, string>
                {
                    [NavigationUrlKey.ParentPage] = returnUrl
                };
            }

            var queryParams = new Dictionary<string, string>
            {
                [PageParameterKey.GroupId] = entity.GroupId.ToString()
            };

            // CampusId rides along for the Campus Team feature's pages.
            if ( CampusId.HasValue )
            {
                queryParams[PageParameterKey.CampusId] = CampusId.Value.ToString();
            }

            // Sign-up mode sends the occurrence identifiers back to the attendee list.
            if ( IsSignUpMode )
            {
                queryParams[PageParameterKey.LocationId] = LocationId.Value.ToString();
                queryParams[PageParameterKey.ScheduleId] = ScheduleId.Value.ToString();
            }

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl( queryParams )
            };
        }

        /// <summary>
        /// Gets the URL that reloads this page in add mode after a Save Then
        /// Add, carrying the same parameters the WebForms block did.
        /// </summary>
        /// <returns>The current page URL in add mode.</returns>
        private string GetSaveThenAddUrl()
        {
            var queryParams = new Dictionary<string, string>
            {
                [PageParameterKey.GroupMemberId] = "0",
                [PageParameterKey.GroupId] = PageParameter( PageParameterKey.GroupId )
            };

            if ( CampusId.HasValue )
            {
                queryParams[PageParameterKey.CampusId] = CampusId.Value.ToString();
            }

            if ( IsSignUpMode )
            {
                queryParams[PageParameterKey.LocationId] = LocationId.Value.ToString();
                queryParams[PageParameterKey.ScheduleId] = ScheduleId.Value.ToString();
            }

            return this.GetCurrentPageUrl( queryParams );
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="GroupMemberBag"/> that represents the entity.</returns>
        private GroupMemberBag GetCommonEntityBag( GroupMember entity )
        {
            if ( entity == null )
            {
                return null;
            }

            if ( entity.Attributes == null )
            {
                entity.LoadAttributes( RockContext );
            }

            return new GroupMemberBag
            {
                IdKey = entity.IdKey,
                Person = entity.Person?.PrimaryAlias.ToListItemBag( entity.Person.FullName ),
                GroupRoleId = entity.GroupRoleId,
                GroupMemberStatus = entity.GroupMemberStatus,
                Note = entity.Note,
                CommunicationPreference = ( Rock.Enums.Communication.CommunicationType ) entity.CommunicationPreference,
                IsNotified = entity.IsNotified,
                IsChatMuted = entity.IsChatMuted,
                IsChatBanned = entity.IsChatBanned,
                ScheduleTemplateId = entity.ScheduleTemplateId,
                ScheduleStartDate = entity.ScheduleStartDate?.ToRockDateTimeOffset(),
                ScheduleReminderEmailOffsetDays = entity.ScheduleReminderEmailOffsetDays
            };
        }

        /// <inheritdoc/>
        protected override GroupMemberBag GetEntityBagForView( GroupMember entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson, enforceSecurity: true );

            return bag;
        }

        /// <inheritdoc/>
        protected override GroupMemberBag GetEntityBagForEdit( GroupMember entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );
            var isReadOnly = GetIsReadOnly( entity );
            var groupType = GroupTypeCache.Get( entity.Group.GroupTypeId );

            bag.SignedDocument = GetLatestSignedDocumentFile( entity );

            if ( groupType.IsSchedulingEnabled && !IsSignUpMode )
            {
                bag.ScheduleAssignments = GetScheduleAssignments( entity );
            }

            // Security is applied through the filter, which also honors the group ADMINISTRATE override.
            var editableKeys = GetEditableAttributeKeys( entity, entity.Group, isReadOnly );
            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson, enforceSecurity: false, attributeFilter: a => editableKeys.Contains( a.Key ) );

            return bag;
        }

        /// <summary>
        /// Gets the most recently signed document's binary file for the
        /// group's required signature document template, for the manual
        /// signed document uploader. The document belongs to the person and
        /// template, so a new member whose person is already covered sees it.
        /// </summary>
        /// <param name="entity">The group member being viewed or edited, with the selected person applied.</param>
        /// <returns>The binary file reference, or null.</returns>
        private ListItemBag GetLatestSignedDocumentFile( GroupMember entity )
        {
            var templateId = entity.Group.RequiredSignatureDocumentTemplateId;

            if ( !templateId.HasValue || entity.PersonId == 0 )
            {
                return null;
            }

            var binaryFile = new SignatureDocumentService( RockContext )
                .Queryable().AsNoTracking()
                .Where( d =>
                    d.SignatureDocumentTemplateId == templateId.Value &&
                    d.AppliesToPersonAlias != null &&
                    d.AppliesToPersonAlias.PersonId == entity.PersonId &&
                    d.LastStatusDate.HasValue &&
                    d.Status == SignatureDocumentStatus.Signed &&
                    d.BinaryFile != null )
                .OrderByDescending( d => d.LastStatusDate.Value )
                .Select( d => new
                {
                    d.BinaryFile.Guid,
                    d.BinaryFile.FileName
                } )
                .FirstOrDefault();

            if ( binaryFile == null )
            {
                return null;
            }

            return new ListItemBag
            {
                Value = binaryFile.Guid.ToString(),
                Text = binaryFile.FileName
            };
        }

        /// <summary>
        /// Gets the member's schedule and location assignment preferences,
        /// excluding orphaned assignments whose location or schedule is no
        /// longer configured on the group. Ordering happens client-side.
        /// </summary>
        /// <param name="entity">The group member being viewed or edited.</param>
        /// <returns>The assignment rows.</returns>
        private List<GroupScheduleAssignmentBag> GetScheduleAssignments( GroupMember entity )
        {
            if ( entity.Id == 0 )
            {
                return new List<GroupScheduleAssignmentBag>();
            }

            // Base the next start date on the start of the week so schedules order consistently.
            var occurrenceDate = RockDateTime.Now.SundayDate().AddDays( 1 );
            var groupMemberId = entity.Id;

            var groupLocationQuery = new GroupLocationService( RockContext )
                .Queryable()
                .Where( gl => gl.GroupId == entity.GroupId );

            return new GroupMemberAssignmentService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Where( a =>
                    a.GroupMemberId == groupMemberId
                    && (
                        !a.LocationId.HasValue
                        || groupLocationQuery.Any( gl => gl.LocationId == a.LocationId && gl.Schedules.Any( s => s.Id == a.ScheduleId ) )
                    ) )
                .Include( a => a.Schedule )
                .Include( a => a.Location )
                .ToList()
                .Select( a => new GroupScheduleAssignmentBag
                {
                    Guid = a.Guid,
                    ScheduleId = a.ScheduleId.Value,
                    LocationId = a.LocationId,
                    ScheduleName = a.Schedule.Name,
                    FormattedScheduleName = GetFormattedScheduleForListing( a.Schedule.Name, a.Schedule.StartTimeOfDay ),
                    LocationName = a.LocationId.HasValue ? a.Location.ToString( true ) : NoLocationPreference,
                    ScheduleOrder = a.Schedule.Order,
                    ScheduleNextStartDateTime = a.Schedule.GetNextStartDateTime( occurrenceDate )?.ToRockDateTimeOffset()
                } )
                .ToList();
        }

        /// <summary>
        /// Formats a schedule for listing per the Schedule List Format
        /// block setting: by time, by name, or both.
        /// </summary>
        /// <param name="scheduleName">The schedule's name.</param>
        /// <param name="startTimeOfDay">The schedule's start time of day.</param>
        /// <returns>The formatted schedule text.</returns>
        private string GetFormattedScheduleForListing( string scheduleName, TimeSpan startTimeOfDay )
        {
            var scheduleListFormat = GetAttributeValue( AttributeKey.ScheduleListFormat ).AsInteger();

            if ( scheduleListFormat == 1 )
            {
                return startTimeOfDay.ToTimeString();
            }

            if ( scheduleListFormat == 2 )
            {
                return scheduleName;
            }

            return $"{startTimeOfDay.ToTimeString()} {scheduleName}";
        }

        /// <summary>
        /// Builds the group member the requirement calculation should run
        /// against. Uses the saved member when one exists, otherwise an
        /// in-memory member for the selected person and role.
        /// </summary>
        /// <param name="groupMemberIdKey">The IdKey of the member, or null while adding.</param>
        /// <param name="personAliasGuid">The primary alias unique identifier of the selected person, as emitted by the PersonPicker.</param>
        /// <param name="selectedRoleId">The currently selected role identifier.</param>
        /// <returns>The group member to calculate against, or null when it cannot be resolved.</returns>
        private GroupMember GetRequirementCalculationTarget( string groupMemberIdKey, Guid? personAliasGuid, int selectedRoleId )
        {
            if ( groupMemberIdKey.IsNotNullOrWhiteSpace() )
            {
                var existing = new GroupMemberService( RockContext ).Get( groupMemberIdKey, !PageCache.Layout.Site.DisablePredictableIds );

                if ( existing != null )
                {
                    // Match the in-memory role to the selection so the statuses reflect the pending change.
                    existing.GroupRoleId = selectedRoleId;

                    return existing;
                }
            }

            var group = GetGroupFromPageParameter();
            var person = GetPersonFromAliasGuid( personAliasGuid );

            if ( group == null || person == null )
            {
                return null;
            }

            return new GroupMember
            {
                GroupId = group.Id,
                Group = group,
                GroupRoleId = selectedRoleId,
                PersonId = person.Id,
                Person = person
            };
        }

        /// <summary>
        /// Creates or updates the manually uploaded signed document for the
        /// group's required signature document template, mirroring the
        /// WebForms save: the latest signed document is updated when one
        /// exists, otherwise an uploaded file creates a new Signed document.
        /// A replaced binary file is marked temporary so cleanup removes it;
        /// the kept file has the flag cleared.
        /// </summary>
        private void SaveSignedDocument( GroupMember entity, ListItemBag signedDocumentValue )
        {
            var template = entity.Group.RequiredSignatureDocumentTemplate;

            if ( template == null )
            {
                return;
            }

            var binaryFileService = new BinaryFileService( RockContext );
            var binaryFileId = signedDocumentValue.GetEntityId<BinaryFile>( RockContext );

            // Adding a member without a file must not touch the person's existing document.
            if ( entity.Id == 0 && !binaryFileId.HasValue )
            {
                return;
            }

            // The same latest-signed-document query that fed the uploader picks the document to update.
            var personId = entity.PersonId;
            var document = new SignatureDocumentService( RockContext )
                .Queryable()
                .Where( d =>
                    d.SignatureDocumentTemplateId == template.Id &&
                    d.AppliesToPersonAlias != null &&
                    d.AppliesToPersonAlias.PersonId == personId &&
                    d.LastStatusDate.HasValue &&
                    d.Status == SignatureDocumentStatus.Signed &&
                    d.BinaryFile != null )
                .OrderByDescending( d => d.LastStatusDate )
                .FirstOrDefault();

            if ( document == null && binaryFileId.HasValue )
            {
                document = new SignatureDocument
                {
                    SignatureDocumentTemplateId = template.Id,
                    AppliesToPersonAliasId = entity.Person?.PrimaryAliasId,
                    AssignedToPersonAliasId = entity.Person?.PrimaryAliasId,
                    Name = $"{entity.Group.Name.RemoveSpecialCharacters()}_{entity.Person?.FullName.RemoveSpecialCharacters()}",
                    Status = SignatureDocumentStatus.Signed,
                    LastStatusDate = RockDateTime.Now
                };

                new SignatureDocumentService( RockContext ).Add( document );
            }

            if ( document == null )
            {
                return;
            }

            var origBinaryFileId = document.BinaryFileId;
            document.BinaryFileId = binaryFileId;

            // A replaced binary file is marked temporary so the cleanup job removes it.
            if ( origBinaryFileId.HasValue && origBinaryFileId.Value != document.BinaryFileId )
            {
                var oldBinaryFile = binaryFileService.Get( origBinaryFileId.Value );

                if ( oldBinaryFile != null && !oldBinaryFile.IsTemporary )
                {
                    oldBinaryFile.IsTemporary = true;
                }
            }

            // The uploaded file starts temporary; keeping it means clearing that flag.
            if ( document.BinaryFileId.HasValue )
            {
                var binaryFile = binaryFileService.Get( document.BinaryFileId.Value );

                if ( binaryFile != null && binaryFile.IsTemporary )
                {
                    binaryFile.IsTemporary = false;
                }
            }
        }

        /// <summary>
        /// Syncs the member's GroupMemberAssignment records with the
        /// client-edited assignment preference rows, mirroring the WebForms
        /// grid save. Rows the client grid never showed (orphaned location or
        /// schedule pairings) are left alone.
        /// </summary>
        private void SyncScheduleAssignments( GroupMember entity, List<GroupScheduleAssignmentBag> assignments )
        {
            assignments = assignments ?? new List<GroupScheduleAssignmentBag>();

            var assignmentService = new GroupMemberAssignmentService( RockContext );

            // The visible pairings are materialized up front so the removal loop stays in memory.
            var groupId = entity.GroupId;
            var visiblePairs = new GroupLocationService( RockContext )
                .Queryable()
                .Where( gl => gl.GroupId == groupId )
                .SelectMany( gl => gl.Schedules, ( gl, s ) => new { gl.LocationId, ScheduleId = s.Id } )
                .ToList();

            var clientGuids = assignments.Select( a => a.Guid ).ToList();

            var removedAssignments = entity.GroupMemberAssignments
                .Where( a =>
                    !clientGuids.Contains( a.Guid )
                    && ( !a.LocationId.HasValue
                        || visiblePairs.Any( p => p.LocationId == a.LocationId.Value && p.ScheduleId == a.ScheduleId ) ) )
                .ToList();

            foreach ( var removedAssignment in removedAssignments )
            {
                entity.GroupMemberAssignments.Remove( removedAssignment );
                assignmentService.Delete( removedAssignment );
            }

            foreach ( var assignmentBag in assignments )
            {
                var assignment = entity.GroupMemberAssignments.FirstOrDefault( a => a.Guid == assignmentBag.Guid );

                if ( assignment == null )
                {
                    assignment = new GroupMemberAssignment
                    {
                        Guid = assignmentBag.Guid
                    };
                    entity.GroupMemberAssignments.Add( assignment );
                }

                assignment.ScheduleId = assignmentBag.ScheduleId;
                assignment.LocationId = assignmentBag.LocationId;
            }
        }

        /// <summary>
        /// Gets the sign-up GroupMemberAssignment for the member and the
        /// sign-up location and schedule, creating one when none exists, and
        /// applies the editable assignment attribute values from the bag.
        /// Returns null outside sign-up mode.
        /// </summary>
        /// <param name="entity">The group member being saved.</param>
        /// <param name="bag">The bag carrying the assignment attribute values.</param>
        /// <returns>The assignment whose attribute values need saving, or null.</returns>
        private GroupMemberAssignment GetOrCreateSignUpAssignment( GroupMember entity, GroupMemberBag bag )
        {
            if ( !IsSignUpMode )
            {
                return null;
            }

            var assignmentService = new GroupMemberAssignmentService( RockContext );
            var groupId = entity.GroupId;
            var personId = entity.PersonId;

            var assignment = assignmentService
                .Queryable()
                .FirstOrDefault( a =>
                    a.GroupMember.GroupId == groupId
                    && a.GroupMember.PersonId == personId
                    && a.LocationId == LocationId.Value
                    && a.ScheduleId == ScheduleId.Value );

            if ( assignment == null )
            {
                assignment = new GroupMemberAssignment
                {
                    GroupId = entity.GroupId,
                    LocationId = LocationId.Value,
                    ScheduleId = ScheduleId.Value
                };

                // A new member has no Id yet, so the navigation property carries the relationship.
                if ( entity.Id == 0 )
                {
                    assignment.GroupMember = entity;
                }
                else
                {
                    assignment.GroupMemberId = entity.Id;
                }

                assignmentService.Add( assignment );
            }

            if ( bag.AssignmentAttributeValues != null )
            {
                assignment.LoadAttributes( RockContext );

                // Only the keys this person may edit are applied; the split matches the sets sent to the client.
                var editableKeys = GetEditableAttributeKeys( assignment, entity.Group, isReadOnly: false );

                assignment.SetPublicAttributeValues( bag.AssignmentAttributeValues, RequestContext.CurrentPerson, enforceSecurity: false, attributeFilter: a => editableKeys.Contains( a.Key ) );
            }

            return assignment;
        }

        /// <summary>
        /// Gets the workflow entry page URL for a requirement workflow.
        /// </summary>
        /// <param name="workflowTypeGuid">The workflow type unique identifier.</param>
        /// <param name="workflowGuid">The existing workflow unique identifier, when navigating to one already started.</param>
        /// <returns>The workflow entry page URL, or null when the page is not configured.</returns>
        private string GetWorkflowEntryUrl( Guid workflowTypeGuid, Guid? workflowGuid )
        {
            var queryParams = new Dictionary<string, string>
            {
                ["WorkflowTypeGuid"] = workflowTypeGuid.ToString()
            };

            if ( workflowGuid.HasValue )
            {
                queryParams["WorkflowGuid"] = workflowGuid.Value.ToString();
            }

            var url = this.GetLinkedPageUrl( AttributeKey.WorkflowEntryPage, queryParams );

            return url.IsNotNullOrWhiteSpace() ? url : null;
        }

        /// <summary>
        /// Gets the group from the GroupId page parameter.
        /// </summary>
        /// <returns>The group, or null when the parameter is missing or invalid.</returns>
        private Model.Group GetGroupFromPageParameter()
        {
            var groupKey = PageParameter( PageParameterKey.GroupId );

            return groupKey.IsNotNullOrWhiteSpace()
                ? new GroupService( RockContext ).Get( groupKey, !PageCache.Layout.Site.DisablePredictableIds )
                : null;
        }

        /// <summary>
        /// Gets a person from the primary alias unique identifier the
        /// PersonPicker emits.
        /// </summary>
        /// <param name="personAliasGuid">The person's primary alias unique identifier.</param>
        /// <returns>The person, or null when the identifier is missing or invalid.</returns>
        private Person GetPersonFromAliasGuid( Guid? personAliasGuid )
        {
            if ( !personAliasGuid.HasValue || personAliasGuid.Value == Guid.Empty )
            {
                return null;
            }

            return new PersonAliasService( RockContext ).GetPerson( personAliasGuid.Value );
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( GroupMember entity, ValidPropertiesBox<GroupMemberBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.Person ), () =>
            {
                // The person can only be set while adding; it is fixed once the member exists.
                if ( entity.Id == 0 )
                {
                    var person = GetPersonFromAliasGuid( box.Bag.Person?.Value?.AsGuidOrNull() );

                    entity.PersonId = person?.Id ?? 0;
                    entity.Person = person;
                }
            } );

            box.IfValidProperty( nameof( box.Bag.GroupRoleId ), () =>
                entity.GroupRoleId = box.Bag.GroupRoleId ?? 0 );

            box.IfValidProperty( nameof( box.Bag.GroupMemberStatus ), () =>
                entity.GroupMemberStatus = box.Bag.GroupMemberStatus );

            box.IfValidProperty( nameof( box.Bag.Note ), () =>
                entity.Note = box.Bag.Note );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ), () =>
            {
                entity.LoadAttributes( RockContext );

                // Only the keys this person may edit are applied; the split matches the sets sent to the client.
                var editableKeys = GetEditableAttributeKeys( entity, entity.Group, isReadOnly: false );

                entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: false, attributeFilter: a => editableKeys.Contains( a.Key ) );
            } );

            box.IfValidProperty( nameof( box.Bag.CommunicationPreference ), () =>
                entity.CommunicationPreference = ( CommunicationType ) box.Bag.CommunicationPreference );

            box.IfValidProperty( nameof( box.Bag.IsNotified ), () =>
            {
                // Only ADMINISTRATE sees or saves the notified flag (WebForms parity).
                if ( BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
                {
                    entity.IsNotified = box.Bag.IsNotified;
                }
            } );

            // Chat preferences only apply when the section was shown: chat enabled globally and for the group (WebForms parity).
            if ( ChatHelper.IsChatEnabled && entity.Group.GetIsChatEnabled() )
            {
                box.IfValidProperty( nameof( box.Bag.IsChatMuted ), () =>
                    entity.IsChatMuted = box.Bag.IsChatMuted );

                box.IfValidProperty( nameof( box.Bag.IsChatBanned ), () =>
                    entity.IsChatBanned = box.Bag.IsChatBanned );
            }

            box.IfValidProperty( nameof( box.Bag.SignedDocument ), () =>
                SaveSignedDocument( entity, box.Bag.SignedDocument ) );

            // Scheduling only applies when the section was shown: scheduling enabled and not sign-up mode (WebForms parity).
            var groupType = GroupTypeCache.Get( entity.Group.GroupTypeId );

            if ( groupType.IsSchedulingEnabled && !IsSignUpMode )
            {
                box.IfValidProperty( nameof( box.Bag.ScheduleTemplateId ), () =>
                    entity.ScheduleTemplateId = box.Bag.ScheduleTemplateId );

                box.IfValidProperty( nameof( box.Bag.ScheduleStartDate ), () =>
                    entity.ScheduleStartDate = box.Bag.ScheduleStartDate?.DateTime );

                box.IfValidProperty( nameof( box.Bag.ScheduleReminderEmailOffsetDays ), () =>
                    entity.ScheduleReminderEmailOffsetDays = box.Bag.ScheduleReminderEmailOffsetDays );

                box.IfValidProperty( nameof( box.Bag.ScheduleAssignments ), () =>
                    SyncScheduleAssignments( entity, box.Bag.ScheduleAssignments ) );
            }

            return true;
        }

        /// <inheritdoc/>
        protected override bool TryGetEntityForEditAction( string idKey, out GroupMember entity, out BlockActionResult error )
        {
            var entityService = new GroupMemberService( RockContext );
            error = null;

            if ( idKey.IsNotNullOrWhiteSpace() )
            {
                entity = entityService.Get( idKey, !PageCache.Layout.Site.DisablePredictableIds );
            }
            else
            {
                // The group must resolve before the authorization check below can grant group-level rights.
                entity = ApplyNewGroupMemberDefaultValues( new GroupMember() );

                if ( entity != null )
                {
                    entityService.Add( entity );
                }
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{GroupMember.FriendlyTypeName} not found." );
                return false;
            }

            // System members are read-only, so no action may edit them.
            if ( entity.IsSystem || !IsAuthorizedToEdit( entity.Group ) )
            {
                error = ActionBadRequest( $"Not authorized to edit {GroupMember.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            var key = pageReference.GetPageParameter( PageParameterKey.GroupMemberId );

            if ( key.IsNullOrWhiteSpace() )
            {
                return new BreadCrumbResult { BreadCrumbs = new List<IBreadCrumb>() };
            }

            var id = key.AsIntegerOrNull();
            var guid = key.AsGuidOrNull();
            var isAddPath = ( id.HasValue && id.Value == 0 )
                || ( guid.HasValue && guid.Value == Guid.Empty );

            if ( isAddPath )
            {
                var addCrumb = new BreadCrumbLink( "New Group Member", new PageReference( pageReference.PageId, 0 ) );
                return new BreadCrumbResult { BreadCrumbs = new List<IBreadCrumb> { addCrumb } };
            }

            var info = new GroupMemberService( RockContext ).GetSelect( key, gm => new
            {
                gm.Id,
                GroupName = gm.Group.Name,
                PersonName = gm.Person.NickName + " " + gm.Person.LastName
            } );

            if ( info == null )
            {
                return new BreadCrumbResult { BreadCrumbs = new List<IBreadCrumb>() };
            }

            var breadCrumbs = new List<IBreadCrumb>();

            // Sign-up mode is computed from the page reference since breadcrumbs
            // can be built outside a normal block request.
            var locationId = pageReference.GetPageParameter( PageParameterKey.LocationId ).AsIntegerOrNull();
            var scheduleKey = pageReference.GetPageParameter( PageParameterKey.ScheduleId );
            var scheduleId = scheduleKey.AsIntegerOrNull() ?? Rock.Utility.IdHasher.Instance.GetId( scheduleKey );
            var isSignUpMode = locationId.ToIntSafe() > 0 && scheduleId.ToIntSafe() > 0;

            // The group name crumb replaces the WebForms session-history hack (Locked Decision #2).
            if ( !isSignUpMode && GetAttributeValue( AttributeKey.IncludeGroupNameInBreadcrumb ).AsBoolean( true ) )
            {
                breadCrumbs.Add( new BreadCrumbLink( info.GroupName ) );
            }

            var pageParameters = new Dictionary<string, string>
            {
                [PageParameterKey.GroupMemberId] = Rock.Utility.IdHasher.Instance.GetHash( info.Id )
            };
            var breadCrumbPageRef = new PageReference( pageReference.PageId, 0, pageParameters );
            breadCrumbs.Add( new BreadCrumbLink( info.PersonName, breadCrumbPageRef ) );

            return new BreadCrumbResult
            {
                BreadCrumbs = breadCrumbs
            };
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Saves the group member represented by the box.
        /// </summary>
        /// <param name="box">The box containing the valid properties to save.</param>
        /// <param name="isSaveThenAdd">Whether the client will reload in add mode after saving.</param>
        /// <param name="isRestoreDeclined">Whether the user declined restoring a matching archived member.</param>
        /// <returns>A <see cref="SaveGroupMemberResponseBag"/> describing the result.</returns>
        [BlockAction]
        public BlockActionResult Save( ValidPropertiesBox<GroupMemberBag> box, bool isSaveThenAdd, bool isRestoreDeclined )
        {
            if ( !TryGetEntityForEditAction( box.Bag?.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            // The archived-member check only applies when the person or role is changing.
            var previousPersonId = entity.PersonId;
            var previousRoleId = entity.GroupRoleId;

            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            if ( entity.PersonId == 0 )
            {
                return ActionBadRequest( "Please select a Person." );
            }

            var role = GroupTypeCache.Get( entity.Group.GroupTypeId )?.Roles.FirstOrDefault( r => r.Id == entity.GroupRoleId );

            if ( role == null )
            {
                return ActionBadRequest( "Please select a Role." );
            }

            var checkForArchivedGroupMember = !isRestoreDeclined
                && ( entity.Id == 0 || entity.PersonId != previousPersonId || entity.GroupRoleId != previousRoleId );

            // A duplicate takes precedence over the restore prompt; the standard validation below reports it.
            if ( checkForArchivedGroupMember
                && !GroupService.AllowsDuplicateMembers()
                && new GroupService( RockContext ).ExistsAsMember( entity.Group, entity.PersonId, entity.GroupRoleId, out _ ) )
            {
                checkForArchivedGroupMember = false;
            }

            if ( checkForArchivedGroupMember
                && new GroupService( RockContext ).ExistsAsArchived( entity.Group, entity.PersonId, entity.GroupRoleId, out var archivedGroupMember ) )
            {
                // Nothing has been saved; returning here discards the pending changes.
                return ActionOk( new SaveGroupMemberResponseBag
                {
                    IsRestorePromptShown = true,
                    // The message renders as HTML on the client, so the data-driven parts are encoded.
                    RestorePromptMessage = $"{entity.Person.FullName} has an archived record as a {role.Name} in this group.",
                    ArchivedGroupMemberIdKey = archivedGroupMember.IdKey
                } );
            }

            // The entity's own must-meet check runs under this same condition, but excludes manual
            // requirement types and cannot see the client-held resolutions, so this block runs its
            // own complete check and skips the entity's.
            if ( entity.Id == 0 || entity.IsNewOrChangedGroupMember( RockContext ) )
            {
                if ( !TryValidateMustMeetRequirements( entity, box.Bag, out var requirementError ) )
                {
                    return ActionBadRequest( requirementError );
                }

                entity.IsSkipRequirementsCheckingDuringValidationCheck = true;
            }

            var signUpAssignment = GetOrCreateSignUpAssignment( entity, box.Bag );

            try
            {
                RockContext.WrapTransaction( () =>
                {
                    RockContext.SaveChanges();
                    entity.SaveAttributeValues( RockContext );
                    signUpAssignment?.SaveAttributeValues( RockContext );

                    // Requirement rows need the member's Id, so they are reconciled after the insert but in the same transaction.
                    ReconcileRequirementResolutions( entity, box.Bag );
                    RockContext.SaveChanges();
                } );
            }
            catch ( GroupMemberValidationException ex )
            {
                // The model's own rules (duplicate member, capacity, etc.) failed; the break lets the client list each error separately.
                return ActionBadRequest( ex.Message.Replace( "; ", "<br>" ) );
            }

            return ActionOk( new SaveGroupMemberResponseBag
            {
                NavigationUrl = isSaveThenAdd ? GetSaveThenAddUrl() : GetBoxNavigationUrls( entity )[NavigationUrlKey.ParentPage]
            } );
        }

        /// <summary>
        /// Restores a matching archived group member instead of creating a
        /// new record.
        /// </summary>
        /// <param name="archivedGroupMemberIdKey">The IdKey of the archived group member to restore.</param>
        /// <returns>The URL to reload the block on the restored member.</returns>
        [BlockAction]
        public BlockActionResult RestoreArchivedGroupMember( string archivedGroupMemberIdKey )
        {
            var groupMemberService = new GroupMemberService( RockContext );
            var groupMemberId = Rock.Utility.IdHasher.Instance.GetId( archivedGroupMemberIdKey );
            var entity = groupMemberService.GetArchived().FirstOrDefault( m => m.Id == groupMemberId );

            if ( entity == null )
            {
                return ActionBadRequest( $"{GroupMember.FriendlyTypeName} not found." );
            }

            if ( !IsAuthorizedToEdit( entity.Group ) )
            {
                return ActionBadRequest( $"Not authorized to edit {GroupMember.FriendlyTypeName}." );
            }

            groupMemberService.Restore( entity );

            if ( !entity.IsValidGroupMember( RockContext ) )
            {
                return ActionBadRequest( entity.ValidationResults.Select( r => r.ErrorMessage ).ToList().AsDelimited( "<br>" ) );
            }

            RockContext.SaveChanges();

            return ActionOk( this.GetCurrentPageUrl( new Dictionary<string, string>
            {
                [PageParameterKey.GroupMemberId] = entity.IdKey
            } ) );
        }

        /// <summary>
        /// Moves the group member to another group, optionally moving notes
        /// and fundraising transactions. The source member is deleted, or
        /// archived when group history prevents deletion.
        /// </summary>
        /// <param name="bag">The move request.</param>
        /// <returns>The URL that reloads this page on the new member.</returns>
        [BlockAction]
        public BlockActionResult MoveGroupMember( MoveGroupMemberRequestBag bag )
        {
            if ( bag == null )
            {
                return ActionBadRequest( "Invalid request." );
            }

            var groupMemberService = new GroupMemberService( RockContext );
            var groupMember = groupMemberService.Get( bag.GroupMemberIdKey, !PageCache.Layout.Site.DisablePredictableIds );

            if ( groupMember == null )
            {
                return ActionBadRequest( $"{GroupMember.FriendlyTypeName} not found." );
            }

            if ( !IsAuthorizedToEdit( groupMember.Group ) )
            {
                return ActionBadRequest( "Not authorized to move this group member." );
            }

            var destGroup = new GroupService( RockContext ).Get( bag.DestinationGroupIdKey, !PageCache.Layout.Site.DisablePredictableIds );

            if ( destGroup == null )
            {
                return ActionBadRequest( "Please select a Destination Group." );
            }

            // The role must belong to the destination group's type; the client dropdown is not trusted.
            var destRole = GroupTypeCache.Get( destGroup.GroupTypeId ).Roles
                .FirstOrDefault( r => r.Id == bag.DestinationGroupRoleId );

            if ( destRole == null )
            {
                return ActionBadRequest( "Please select a Group Role." );
            }

            var destGroupId = destGroup.Id;
            var isAlreadyMember = groupMemberService.Queryable().Any( a =>
                a.GroupId == destGroupId
                && a.PersonId == groupMember.PersonId
                && a.GroupRoleId == destRole.Id );

            if ( isAlreadyMember )
            {
                return ActionBadRequest( $"{groupMember.Person.FullName} is already in {destGroup.Name}." );
            }

            if ( bag.IsMoveFundraisingTransactionsChecked && !CanMoveFundraisingTransactions( destGroup ) )
            {
                return ActionBadRequest( "The destination group is not properly configured to accept the fundraising transactions." );
            }

            var isArchive = !groupMemberService.CanDelete( groupMember, out _ );

            groupMember.LoadAttributes();

            var destGroupMember = new GroupMember
            {
                GroupId = destGroupId,
                Group = destGroup,
                GroupRoleId = destRole.Id,
                PersonId = groupMember.PersonId
            };
            destGroupMember.LoadAttributes();

            // Only attribute values with a matching key and field type carry over.
            foreach ( var attribute in groupMember.Attributes )
            {
                if ( destGroupMember.Attributes.Any( a => a.Key == attribute.Key && a.Value.FieldTypeId == attribute.Value.FieldTypeId ) )
                {
                    destGroupMember.SetAttributeValue( attribute.Key, groupMember.GetAttributeValue( attribute.Key ) );
                }
            }

            // Un-link any registrant records that point to this group member.
            foreach ( var registrant in new RegistrationRegistrantService( RockContext ).Queryable()
                .Where( r => r.GroupMemberId == groupMember.Id ) )
            {
                registrant.GroupMemberId = null;
            }

            RockContext.WrapTransaction( () =>
            {
                groupMemberService.Add( destGroupMember );
                RockContext.SaveChanges();
                destGroupMember.SaveAttributeValues( RockContext );

                // Move any Note records that were associated with the old member to the new record.
                if ( bag.IsMoveNotesChecked )
                {
                    destGroupMember.Note = groupMember.Note;
                    var groupMemberEntityTypeId = EntityTypeCache.GetId<GroupMember>().Value;
                    var groupMemberNotes = new NoteService( RockContext )
                        .Queryable()
                        .Where( a => a.NoteType.EntityTypeId == groupMemberEntityTypeId && a.EntityId == groupMember.Id );

                    foreach ( var note in groupMemberNotes )
                    {
                        note.EntityId = destGroupMember.Id;
                    }

                    RockContext.SaveChanges();
                }

                if ( bag.IsMoveFundraisingTransactionsChecked )
                {
                    MoveFundraisingTransactions( groupMember, destGroupMember );
                }

                if ( isArchive )
                {
                    groupMemberService.Archive( groupMember, RequestContext.CurrentPerson?.PrimaryAliasId, true );
                }
                else
                {
                    groupMemberService.Delete( groupMember );
                }

                RockContext.SaveChanges();

                destGroupMember.CalculateRequirements( RockContext, true );
            } );

            // Only the new member's id rides along; a stale GroupId or returnUrl from the
            // old record would redirect later navigation (WebForms parity).
            return ActionOk( this.GetCurrentPageUrl(
                new Dictionary<string, string>
                {
                    [PageParameterKey.GroupMemberId] = destGroupMember.Id.ToString()
                },
                skipExistingParameters: true ) );
        }

        /// <summary>
        /// Locates or creates an open Fundraising Transfer batch.
        /// </summary>
        /// <returns>An open <see cref="FinancialBatch"/> for fundraising transfer transactions.</returns>
        private FinancialBatch GetFundraisingTransferBatch()
        {
            var batchService = new FinancialBatchService( RockContext );
            var availableBatch = batchService.Queryable()
                .Where( b => b.Status == BatchStatus.Open )
                .Where( b => b.Note.ToLower() == FundraisingBatchNote.ToLower() )
                .FirstOrDefault();

            // If an open batch already exists, use that.
            if ( availableBatch != null )
            {
                return availableBatch;
            }

            var newBatch = new FinancialBatch
            {
                Name = FundraisingBatchName,
                Note = FundraisingBatchNote,
                Status = BatchStatus.Open,
                ControlAmount = 0,
                BatchStartDateTime = RockDateTime.Now
            };

            batchService.Add( newBatch );
            RockContext.SaveChanges();

            return newBatch;
        }

        /// <summary>
        /// Validates that fundraising transactions can be moved to the
        /// destination group, which must have a valid FinancialAccount
        /// attribute value.
        /// </summary>
        /// <param name="destinationGroup">The destination group.</param>
        /// <returns><c>true</c> when the transactions can be moved.</returns>
        private bool CanMoveFundraisingTransactions( Model.Group destinationGroup )
        {
            destinationGroup.LoadAttributes( RockContext );
            var accountGuid = destinationGroup.GetAttributeValue( "FinancialAccount" ).AsGuidOrNull();

            if ( accountGuid == null )
            {
                return false;
            }

            return new FinancialAccountService( RockContext ).Get( accountGuid.Value ) != null;
        }

        /// <summary>
        /// Moves fundraising transactions from the old member to the new one.
        /// Must run inside the same transaction as the member move. Ported
        /// verbatim from the WebForms block: matching accounts just re-point
        /// the detail row; open batches re-point account and member; closed
        /// batches get a reversal and a replacement transaction in the
        /// Fundraising Transfer batch.
        /// </summary>
        /// <param name="oldGroupMember">The original group member.</param>
        /// <param name="newGroupMember">The new group member.</param>
        private void MoveFundraisingTransactions( GroupMember oldGroupMember, GroupMember newGroupMember )
        {
            var groupMemberTypeId = EntityTypeCache.Get( Rock.SystemGuid.EntityType.GROUP_MEMBER ).Id;
            var oldGroup = oldGroupMember.Group;
            var newGroup = newGroupMember.Group;

            newGroup.LoadAttributes( RockContext );
            var newAccountGuid = newGroup.GetAttributeValue( "FinancialAccount" ).AsGuid();
            var newFinancialAccount = new FinancialAccountService( RockContext ).Get( newAccountGuid );

            var transactionService = new FinancialTransactionService( RockContext );
            var oldTransactions = transactionService.Queryable()
                .Where( t => t.TransactionDetails
                    .Where( d => d.EntityId == oldGroupMember.Id )
                    .Where( d => d.EntityTypeId == groupMemberTypeId )
                    .Any() )
                .ToList();

            foreach ( var oldTransaction in oldTransactions )
            {
                var transactionObjectMoved = false;
                FinancialTransaction creditTransaction = null;
                FinancialTransaction newTransaction = null;
                var financialTransactionDetailService = new FinancialTransactionDetailService( RockContext );

                foreach ( var oldTransDetail in oldTransaction.TransactionDetails )
                {
                    if ( oldTransDetail.AccountId == newFinancialAccount.Id )
                    {
                        // Accounts are the same, so there is no need to adjust batches. Just change the EntityId and move on.
                        oldTransDetail.EntityId = newGroupMember.Id;
                        RockContext.SaveChanges();
                        continue;
                    }

                    if ( oldTransaction.Batch.Status == BatchStatus.Open )
                    {
                        // Batch is open, so we can just change the account on the TransactionDetail (and the EntityId) and move on.
                        oldTransDetail.AccountId = newFinancialAccount.Id;
                        oldTransDetail.EntityId = newGroupMember.Id;
                        RockContext.SaveChanges();
                        continue;
                    }

                    // Batch is not open, so we need to make new transactions.
                    if ( !transactionObjectMoved )
                    {
                        var transferBatch = GetFundraisingTransferBatch();

                        // Create a new credit transaction to cancel out the original transaction.
                        creditTransaction = new FinancialTransaction();
                        creditTransaction.CopyPropertiesFrom( oldTransaction );
                        creditTransaction.Id = 0;
                        creditTransaction.Guid = Guid.NewGuid();
                        creditTransaction.BatchId = transferBatch.Id;
                        creditTransaction.Summary = string.Format(
                            "Reversal created for transaction {0} to move Fundraising Donations from group {1} to {2}.{3}{4}",
                            oldTransaction.Id,
                            oldGroup.Id,
                            newGroup.Id,
                            Environment.NewLine,
                            creditTransaction.Summary );

                        creditTransaction.FinancialPaymentDetail = new FinancialPaymentDetail();
                        creditTransaction.FinancialPaymentDetail.CopyPropertiesFrom( oldTransaction.FinancialPaymentDetail );
                        creditTransaction.FinancialPaymentDetail.Id = 0;
                        creditTransaction.FinancialPaymentDetail.Guid = Guid.NewGuid();
                        transactionService.Add( creditTransaction );

                        // Create a new transaction to replace the original transaction.
                        newTransaction = new FinancialTransaction();
                        newTransaction.CopyPropertiesFrom( oldTransaction );
                        newTransaction.Id = 0;
                        newTransaction.Guid = Guid.NewGuid();
                        newTransaction.BatchId = transferBatch.Id;
                        newTransaction.Summary = string.Format(
                            "New transaction to replace {0} (moved Fundraising Donations from group {1} to {2}).{3}{4}",
                            oldTransaction.Id,
                            oldGroup.Id,
                            newGroup.Id,
                            Environment.NewLine,
                            newTransaction.Summary );

                        newTransaction.FinancialPaymentDetail = new FinancialPaymentDetail();
                        newTransaction.FinancialPaymentDetail.CopyPropertiesFrom( oldTransaction.FinancialPaymentDetail );
                        newTransaction.FinancialPaymentDetail.Id = 0;
                        newTransaction.FinancialPaymentDetail.Guid = Guid.NewGuid();
                        transactionService.Add( newTransaction );

                        RockContext.SaveChanges();

                        // Only do this once per transaction; additional detail rows reuse the same transactions.
                        transactionObjectMoved = true;
                    }

                    if ( creditTransaction == null || newTransaction == null )
                    {
                        // Should not occur; guards against the block above failing silently.
                        throw new Exception( "New distribution transactions were not created." );
                    }

                    // Make the new transaction details.
                    var creditTransDetail = new FinancialTransactionDetail();
                    creditTransDetail.CopyPropertiesFrom( oldTransDetail );
                    creditTransDetail.Id = 0;
                    creditTransDetail.Guid = Guid.NewGuid();
                    creditTransDetail.Amount = oldTransDetail.Amount * -1;
                    creditTransDetail.TransactionId = creditTransaction.Id;
                    creditTransDetail.Summary = string.Format(
                        "Credit for FinancialTransactionDetail {0}.{1}{2}",
                        oldTransDetail.Id,
                        Environment.NewLine,
                        creditTransDetail.Summary );
                    financialTransactionDetailService.Add( creditTransDetail );

                    var newTransDetail = new FinancialTransactionDetail();
                    newTransDetail.CopyPropertiesFrom( oldTransDetail );
                    newTransDetail.Id = 0;
                    newTransDetail.Guid = Guid.NewGuid();
                    newTransDetail.AccountId = newFinancialAccount.Id;
                    newTransDetail.EntityId = newGroupMember.Id;
                    newTransDetail.TransactionId = newTransaction.Id;
                    newTransDetail.Summary = string.Format(
                        "Replacement for FinancialTransactionDetail {0}.{1}{2}",
                        oldTransDetail.Id,
                        Environment.NewLine,
                        newTransDetail.Summary );
                    financialTransactionDetailService.Add( newTransDetail );

                    RockContext.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Gets the role options and warnings for a selected destination
        /// group in the move modal.
        /// </summary>
        /// <param name="groupMemberIdKey">The IdKey of the member being moved.</param>
        /// <param name="destinationGroupIdKey">The IdKey of the selected destination group.</param>
        /// <returns>A <see cref="MoveGroupMemberOptionsBag"/>.</returns>
        [BlockAction]
        public BlockActionResult GetMoveGroupMemberOptions( string groupMemberIdKey, string destinationGroupIdKey )
        {
            var entity = new GroupMemberService( RockContext ).Get( groupMemberIdKey, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{GroupMember.FriendlyTypeName} not found." );
            }

            if ( !IsAuthorizedToEdit( entity.Group ) )
            {
                return ActionBadRequest( "Not authorized to move this group member." );
            }

            var options = new MoveGroupMemberOptionsBag
            {
                CurrentGroupName = entity.Group.Name,
                IsFundraisingOptionShown = IsFundraisingGroupType( entity.Group.GroupTypeId ),
                RoleItems = new List<ListItemBag>(),
                Warnings = new List<string>()
            };

            // No destination selected yet; the modal only needs the open-time state.
            if ( destinationGroupIdKey.IsNullOrWhiteSpace() )
            {
                return ActionOk( options );
            }

            var destinationGroup = new GroupService( RockContext ).Get( destinationGroupIdKey, !PageCache.Layout.Site.DisablePredictableIds );

            if ( destinationGroup == null )
            {
                return ActionOk( options );
            }

            if ( destinationGroup.Id == entity.GroupId )
            {
                options.Warnings.Add( "The destination group is the same as the current group." );
                return ActionOk( options );
            }

            var destinationGroupType = GroupTypeCache.Get( destinationGroup.GroupTypeId );

            options.RoleItems = destinationGroupType.Roles
                .OrderBy( r => r.Order )
                .Select( r => new ListItemBag
                {
                    Text = r.Name,
                    Value = r.Id.ToString()
                } )
                .ToList();
            options.DefaultRoleId = destinationGroupType.DefaultGroupRoleId;

            // Attributes that have no matching key and field type in the destination are lost on move.
            var destinationMember = new GroupMember { Group = destinationGroup, GroupId = destinationGroup.Id };
            destinationMember.LoadAttributes( RockContext );
            entity.LoadAttributes( RockContext );

            var hasLostAttributes = entity.Attributes
                .Any( a => !destinationMember.Attributes.Any( d => d.Key == a.Key && d.Value.FieldTypeId == a.Value.FieldTypeId ) );

            if ( hasLostAttributes )
            {
                options.Warnings.Add( "The destination group has different member attributes than the source group, so some data may be lost." );
            }

            var personId = entity.PersonId;
            var destinationGroupId = destinationGroup.Id;
            var isAlreadyMember = new GroupMemberService( RockContext )
                .Queryable()
                .Any( m => m.GroupId == destinationGroupId && m.PersonId == personId );

            if ( isAlreadyMember )
            {
                options.Warnings.Add( $"{entity.Person.FullName} is already a member of {destinationGroup.Name}." );
            }

            return ActionOk( options );
        }

        /// <summary>
        /// Gets whether the group type is, or directly inherits from, the
        /// fundraising opportunity group type, which is what offers the
        /// fundraising transaction transfer option on a move.
        /// </summary>
        /// <param name="groupTypeId">The group type identifier to check.</param>
        /// <returns><c>true</c> when the group type is fundraising.</returns>
        private bool IsFundraisingGroupType( int groupTypeId )
        {
            var fundraisingGroupTypeId = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FUNDRAISINGOPPORTUNITY.AsGuid() ).Id;
            var groupType = GroupTypeCache.Get( groupTypeId );

            return groupType != null
                && ( groupType.Id == fundraisingGroupTypeId || groupType.InheritedGroupTypeId == fundraisingGroupTypeId );
        }

        /// <summary>
        /// Gets the state required to open the quick communication modal.
        /// </summary>
        /// <param name="groupMemberIdKey">The IdKey of the member to communicate with.</param>
        /// <returns>A <see cref="CommunicationOptionsBag"/>.</returns>
        [BlockAction]
        public BlockActionResult GetCommunicationOptions( string groupMemberIdKey )
        {
            if ( !GetAttributeValue( AttributeKey.EnableCommunications ).AsBoolean( true ) )
            {
                return ActionBadRequest( "Communications are not enabled." );
            }

            var entity = new GroupMemberService( RockContext ).Get( groupMemberIdKey, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{GroupMember.FriendlyTypeName} not found." );
            }

            if ( !IsAuthorizedToEdit( entity.Group ) )
            {
                return ActionBadRequest( "Not authorized to communicate with this group member." );
            }

            var groupType = GroupTypeCache.Get( entity.Group.GroupTypeId );
            var canMemberReceiveEmail = entity.Person.IsEmailActive && entity.Person.CanReceiveEmail();

            var options = new CommunicationOptionsBag
            {
                RecipientName = entity.Person.FullName,
                RecipientRoleName = groupType.Roles.FirstOrDefault( r => r.Id == entity.GroupRoleId )?.Name,
                RecipientPhotoUrl = Person.GetPersonPhotoUrl( entity.Person ),
                RecipientEmail = canMemberReceiveEmail ? entity.Person.Email : null,
                IsSmsTabShown = GetAttributeValue( AttributeKey.EnableSMS ).AsBoolean( true )
            };

            SetEmailOptions( options, entity, canMemberReceiveEmail );
            SetSmsOptions( options, entity );

            return ActionOk( options );
        }

        /// <summary>
        /// Sets the email tab state on the communication options: the From
        /// behavior per the Allow Selecting From setting, and the warning
        /// cases that block sending entirely.
        /// </summary>
        /// <param name="options">The options bag to populate.</param>
        /// <param name="entity">The group member being communicated with.</param>
        /// <param name="canMemberReceiveEmail">Whether the member has a usable email address.</param>
        private void SetEmailOptions( CommunicationOptionsBag options, GroupMember entity, bool canMemberReceiveEmail )
        {
            var currentPerson = RequestContext.CurrentPerson;
            var senderEmail = currentPerson?.Email;

            options.IsFromEditable = GetAttributeValue( AttributeKey.AllowSelectingFrom ).AsBoolean( true );
            options.DefaultFromEmail = senderEmail;

            if ( !canMemberReceiveEmail )
            {
                options.EmailWarningTitle = $"Cannot Send Emails to {entity.Person.FullName}";
                options.EmailWarningMessage = "No email address is available for this person.";
                return;
            }

            if ( options.IsFromEditable )
            {
                return;
            }

            // A locked From means the sender's own email must exist to send anything.
            if ( senderEmail.IsNotNullOrWhiteSpace() )
            {
                options.FromDisplayText = $"{currentPerson.FullName} ({senderEmail})";
            }
            else
            {
                options.EmailWarningTitle = "Cannot Send Emails";
                options.EmailWarningMessage = "To send an email you must first configure an email address in your profile.";
            }
        }

        /// <summary>
        /// Sets the SMS tab state on the communication options: the from
        /// number (static for one, a dropdown for several), and the warning
        /// cases that block sending entirely.
        /// </summary>
        /// <param name="options">The options bag to populate.</param>
        /// <param name="entity">The group member being communicated with.</param>
        private void SetSmsOptions( CommunicationOptionsBag options, GroupMember entity )
        {
            if ( !options.IsSmsTabShown )
            {
                return;
            }

            var memberSmsNumber = GetMemberSmsNumber( entity );
            options.RecipientSmsNumber = memberSmsNumber?.NumberFormatted;

            var smsNumbers = GetAuthorizedSmsNumbers();

            if ( !smsNumbers.Any() )
            {
                options.SmsWarningTitle = "System Cannot Send SMS Messages";
                options.SmsWarningMessage = "To send an SMS you must first configure a system phone number with SMS enabled.";
                return;
            }

            if ( memberSmsNumber == null )
            {
                options.SmsWarningTitle = $"Cannot Send SMS Messages to {entity.Person.FullName}";
                options.SmsWarningMessage = "No SMS-enabled phone number is available for this person.";
                return;
            }

            if ( smsNumbers.Count == 1 )
            {
                options.SmsFromNumberText = $"{smsNumbers[0].Name} ({smsNumbers[0].Number})";
            }
            else
            {
                options.SmsFromNumberItems = smsNumbers
                    .Select( spn => new ListItemBag
                    {
                        Text = spn.Name,
                        Value = spn.Id.ToString()
                    } )
                    .ToList();
            }
        }

        /// <summary>
        /// Gets the member's first valid SMS-enabled phone number, used both
        /// for the displaycard and as the send-time recipient number.
        /// </summary>
        /// <param name="entity">The group member being communicated with.</param>
        /// <returns>The phone number, or null when none exists.</returns>
        private PhoneNumber GetMemberSmsNumber( GroupMember entity )
        {
            return entity.Person.PhoneNumbers.FirstOrDefault( p => p.IsMessagingEnabled && p.IsValid );
        }

        /// <summary>
        /// Gets the SMS-enabled system phone numbers the current person may
        /// send from, filtered by the Allowed SMS Numbers block setting when
        /// any are selected.
        /// </summary>
        /// <returns>The authorized system phone numbers, in display order.</returns>
        private List<SystemPhoneNumberCache> GetAuthorizedSmsNumbers()
        {
            var smsNumbers = SystemPhoneNumberCache.All( false )
                .Where( spn =>
                    spn.IsSmsEnabled
                    && spn.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                .OrderBy( spn => spn.Order )
                .ThenBy( spn => spn.Name )
                .ThenBy( spn => spn.Id )
                .ToList();

            var selectedNumberGuids = GetAttributeValue( AttributeKey.AllowedSMSNumbers ).SplitDelimitedValues( true ).AsGuidList();

            if ( selectedNumberGuids.Any() )
            {
                smsNumbers = smsNumbers.Where( spn => selectedNumberGuids.Contains( spn.Guid ) ).ToList();
            }

            return smsNumbers;
        }

        /// <summary>
        /// Sends a quick email or SMS communication to the group member.
        /// </summary>
        /// <param name="bag">The communication request.</param>
        /// <returns>A success or validation error result.</returns>
        [BlockAction]
        public BlockActionResult SendCommunication( SendCommunicationRequestBag bag )
        {
            if ( bag == null )
            {
                return ActionBadRequest( "Invalid request." );
            }

            if ( !GetAttributeValue( AttributeKey.EnableCommunications ).AsBoolean( true ) )
            {
                return ActionBadRequest( "Communications are not enabled." );
            }

            var entity = new GroupMemberService( RockContext ).Get( bag.GroupMemberIdKey, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{GroupMember.FriendlyTypeName} not found." );
            }

            if ( !IsAuthorizedToEdit( entity.Group ) )
            {
                return ActionBadRequest( "Not authorized to communicate with this group member." );
            }

            return bag.IsSms
                ? SendSmsCommunication( entity, bag )
                : SendEmailCommunication( entity, bag );
        }

        /// <summary>
        /// Validates and sends the quick SMS communication. The recipient's
        /// number and the authorized from numbers are re-resolved server-side
        /// rather than trusted from the client.
        /// </summary>
        /// <param name="entity">The group member to message.</param>
        /// <param name="bag">The communication request.</param>
        /// <returns>A success or validation error result.</returns>
        private BlockActionResult SendSmsCommunication( GroupMember entity, SendCommunicationRequestBag bag )
        {
            var validationErrors = new List<string>();
            var memberSmsNumber = GetMemberSmsNumber( entity );

            if ( memberSmsNumber == null )
            {
                validationErrors.Add( $"No SMS-enabled phone number is available for {entity.Person.FullName}." );
            }

            // One authorized number sends without a selection; more than one requires the client's choice.
            var smsNumbers = GetAuthorizedSmsNumbers();
            var fromNumber = smsNumbers.Count == 1
                ? smsNumbers[0]
                : smsNumbers.FirstOrDefault( spn => spn.Id == bag.FromSystemPhoneNumberId );

            if ( fromNumber == null )
            {
                validationErrors.Add( "A from phone number is required." );
            }

            if ( bag.Message.IsNullOrWhiteSpace() )
            {
                validationErrors.Add( "Message is required." );
            }

            if ( validationErrors.Any() )
            {
                return ActionBadRequest( validationErrors.AsDelimited( "<br>" ) );
            }

            var smsMessage = new RockSMSMessage
            {
                FromSystemPhoneNumber = fromNumber,
                Message = bag.Message,
                CreateCommunicationRecord = false,
                CommunicationName = "Group Member Quick Communication"
            };
            smsMessage.AddRecipient( new RockSMSMessageRecipient( entity.Person, memberSmsNumber.ToSmsNumber(), new Dictionary<string, object>() ) );

            if ( !smsMessage.Send( out var sendErrors ) )
            {
                return ActionBadRequest( sendErrors.Any() ? sendErrors.AsDelimited( "<br>" ) : "Unable to send the SMS message." );
            }

            return ActionOk();
        }

        /// <summary>
        /// Validates and sends the quick email communication. The From
        /// address is only honored from the client when the Allow Selecting
        /// From setting is enabled; otherwise the logged-in person's email is
        /// used regardless of what was sent. The WebForms block let a blank
        /// From or Subject "send" successfully, which the redesign fixes with
        /// these server-side checks.
        /// </summary>
        /// <param name="entity">The group member to email.</param>
        /// <param name="bag">The communication request.</param>
        /// <returns>A success or validation error result.</returns>
        private BlockActionResult SendEmailCommunication( GroupMember entity, SendCommunicationRequestBag bag )
        {
            var currentPerson = RequestContext.CurrentPerson;
            var fromEmail = GetAttributeValue( AttributeKey.AllowSelectingFrom ).AsBoolean( true )
                ? bag.FromEmail
                : currentPerson?.Email;

            var validationErrors = new List<string>();

            if ( fromEmail.IsNullOrWhiteSpace() )
            {
                validationErrors.Add( "From is required." );
            }

            if ( bag.Subject.IsNullOrWhiteSpace() )
            {
                validationErrors.Add( "Subject is required." );
            }

            if ( bag.Message.IsNullOrWhiteSpace() )
            {
                validationErrors.Add( "Message is required." );
            }

            if ( !entity.Person.IsEmailActive || !entity.Person.CanReceiveEmail() )
            {
                validationErrors.Add( $"No email address is available for {entity.Person.FullName}." );
            }

            if ( validationErrors.Any() )
            {
                return ActionBadRequest( validationErrors.AsDelimited( "<br>" ) );
            }

            var message = bag.Message;

            if ( GetAttributeValue( AttributeKey.AppendHeaderFooter ).AsBoolean( true ) )
            {
                var globalAttributes = GlobalAttributesCache.Get();
                message = $"{globalAttributes.GetValue( "EmailHeader" )} {message} {globalAttributes.GetValue( "EmailFooter" )}";
            }

            var emailMessage = new RockEmailMessage
            {
                FromEmail = fromEmail,
                FromName = currentPerson?.FullName ?? fromEmail,
                Subject = bag.Subject,
                Message = message,
                CreateCommunicationRecord = false
            };
            emailMessage.AddRecipient( new RockEmailMessageRecipient( entity.Person, new Dictionary<string, object>() ) );

            if ( !emailMessage.Send( out var sendErrors ) )
            {
                return ActionBadRequest( sendErrors.Any() ? sendErrors.AsDelimited( "<br>" ) : "Unable to send the email." );
            }

            return ActionOk();
        }

        /// <summary>
        /// Sends the required signature document request to the group
        /// member. Pending Open Decision A in the conversion plan.
        /// </summary>
        /// <param name="groupMemberIdKey">The IdKey of the member to send the request to.</param>
        /// <returns>The refreshed <see cref="SignatureDocumentStatusBag"/> on success, or an error result.</returns>
        [BlockAction]
        public BlockActionResult SendSignatureRequest( string groupMemberIdKey )
        {
            // TODO: Blocked on Open Decision A. No working send API exists for legacy providers.
            return ActionBadRequest( "Not implemented." );
        }

        /// <summary>
        /// Gets the existing sign-up group member for the selected person so
        /// the form can re-hydrate. Sign-up mode only.
        /// </summary>
        /// <param name="personAliasGuid">The primary alias unique identifier of the selected person, as emitted by the PersonPicker.</param>
        /// <returns>The existing member's detail box, or null when none exists.</returns>
        [BlockAction]
        public BlockActionResult GetExistingSignUpGroupMember( Guid? personAliasGuid )
        {
            // Outside sign-up mode a duplicate is rejected at save time instead of loading the existing record.
            if ( !IsSignUpMode )
            {
                return ActionOk<object>( null );
            }

            var group = GetGroupFromPageParameter();
            var person = GetPersonFromAliasGuid( personAliasGuid );

            if ( group == null || person == null )
            {
                return ActionOk<object>( null );
            }

            var entity = new GroupMemberService( RockContext )
                .Queryable()
                .Include( m => m.Person )
                .Include( m => m.Group )
                .FirstOrDefault( m => m.GroupId == group.Id && m.PersonId == person.Id );

            if ( entity == null )
            {
                return ActionOk<object>( null );
            }

            if ( !IsAuthorizedToEdit( entity.Group ) && !entity.Group.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "You are not authorized to view this group member." );
            }

            return ActionOk( GetDetailBox( entity ) );
        }

        /// <summary>
        /// Recalculates the group requirements for the given person and role
        /// and returns refreshed inline alert bags, along with whether an
        /// archived record exists for that pairing. Serves the Refresh
        /// Requirements button as well as the person and role changes, all of
        /// which recalculated on postback in the WebForms block.
        /// </summary>
        /// <param name="groupMemberIdKey">The IdKey of the member, or null while adding.</param>
        /// <param name="personAliasGuid">The primary alias unique identifier of the selected person, as emitted by the PersonPicker.</param>
        /// <param name="selectedRoleId">The currently selected role identifier.</param>
        /// <returns>A <see cref="RefreshRequirementsResponseBag"/>.</returns>
        [BlockAction]
        public BlockActionResult RefreshRequirements( string groupMemberIdKey, Guid? personAliasGuid, int selectedRoleId )
        {
            var entity = GetRequirementCalculationTarget( groupMemberIdKey, personAliasGuid, selectedRoleId );

            if ( entity == null )
            {
                return ActionOk( new RefreshRequirementsResponseBag
                {
                    RequirementAlerts = new List<GroupMemberRequirementAlertBag>()
                } );
            }

            if ( !entity.Group.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) && !IsAuthorizedToEdit( entity.Group ) )
            {
                return ActionBadRequest( "Not authorized to view this group." );
            }

            // With requirements hidden, the archived-record state still refreshes but the requirement data stays server-side.
            var areRequirementsHidden = GetAttributeValue( AttributeKey.AreRequirementsPubliclyHidden ).AsBoolean();
            var alerts = new List<GroupMemberRequirementAlertBag>();
            string calculationErrors = null;

            if ( !areRequirementsHidden )
            {
                if ( entity.Id != 0 && !entity.IsNewOrChangedGroupMember( RockContext ) )
                {
                    entity.CalculateRequirements( RockContext, true );
                }

                alerts = GetRequirementAlerts( entity, selectedRoleId, out calculationErrors );
            }

            // The uploader only renders for editors, so view-only callers never receive the file reference.
            var signedDocument = IsAuthorizedToEdit( entity.Group )
                ? GetLatestSignedDocumentFile( entity )
                : null;

            return ActionOk( new RefreshRequirementsResponseBag
            {
                RequirementAlerts = alerts,
                CalculationErrors = calculationErrors,
                IsRequirementInteractionDisabled = entity.Id == 0,
                SignedDocument = signedDocument
            } );
        }

        /// <summary>
        /// Launches a requirement's does-not-meet or warning workflow, or
        /// returns the entry page URL when one was already started.
        /// </summary>
        /// <param name="groupMemberIdKey">The IdKey of the member.</param>
        /// <param name="groupRequirementGuid">The unique identifier of the group requirement.</param>
        /// <param name="isWarningWorkflow">Whether to launch the warning workflow instead of the does-not-meet workflow.</param>
        /// <returns>A <see cref="LaunchRequirementWorkflowResponseBag"/>.</returns>
        [BlockAction]
        public BlockActionResult LaunchRequirementWorkflow( string groupMemberIdKey, Guid groupRequirementGuid, bool isWarningWorkflow )
        {
            var entity = new GroupMemberService( RockContext ).Get( groupMemberIdKey, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{GroupMember.FriendlyTypeName} not found." );
            }

            if ( !entity.Group.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) && !IsAuthorizedToEdit( entity.Group ) )
            {
                return ActionBadRequest( "Not authorized to view this group." );
            }

            var groupRequirement = entity.Group.GetGroupRequirements( RockContext )
                .FirstOrDefault( r => r.Guid == groupRequirementGuid );

            if ( groupRequirement == null )
            {
                return ActionBadRequest( "Group Requirement not found." );
            }

            var requirementType = groupRequirement.GroupRequirementType;
            var workflowTypeId = isWarningWorkflow ? requirementType.WarningWorkflowTypeId : requirementType.DoesNotMeetWorkflowTypeId;

            if ( !workflowTypeId.HasValue )
            {
                return ActionBadRequest( "No workflow is configured for this requirement." );
            }

            var workflowType = WorkflowTypeCache.Get( workflowTypeId.Value );

            if ( workflowType == null || workflowType.IsActive == false )
            {
                return ActionBadRequest( "The configured workflow type is not active." );
            }

            if ( !workflowType.IsPersisted )
            {
                return ActionBadRequest( $"The Workflow Type '{workflowType.Name}' is not configured to be automatically persisted, and could not be started." );
            }

            var groupMemberRequirementService = new GroupMemberRequirementService( RockContext );
            var memberRequirement = groupMemberRequirementService.Queryable()
                .FirstOrDefault( r => r.GroupMemberId == entity.Id && r.GroupRequirementId == groupRequirement.Id );

            // A workflow already started for this requirement just navigates to its entry page.
            var existingWorkflowId = isWarningWorkflow ? memberRequirement?.WarningWorkflowId : memberRequirement?.DoesNotMeetWorkflowId;

            if ( existingWorkflowId.HasValue )
            {
                var existingWorkflow = new WorkflowService( RockContext ).Get( existingWorkflowId.Value );

                return ActionOk( new LaunchRequirementWorkflowResponseBag
                {
                    WorkflowEntryUrl = GetWorkflowEntryUrl( workflowType.Guid, existingWorkflow?.Guid )
                } );
            }

            if ( memberRequirement == null )
            {
                memberRequirement = new GroupMemberRequirement
                {
                    GroupRequirementId = groupRequirement.Id,
                    GroupMemberId = entity.Id
                };
                groupMemberRequirementService.Add( memberRequirement );
                RockContext.SaveChanges();
            }

            memberRequirement = groupMemberRequirementService.GetInclude( memberRequirement.Guid, r => r.GroupMember );

            var workflow = Model.Workflow.Activate( workflowType, workflowType.Name );

            workflow.SetAttributeValue( "Person", entity.Person?.PrimaryAlias?.Guid );

            var workflowService = new WorkflowService( RockContext );

            if ( !workflowService.Process( workflow, memberRequirement, out var workflowErrors ) )
            {
                return ActionBadRequest( $"Unable to start the workflow: {workflowErrors.AsDelimited( " " )}" );
            }

            var interactiveAction = workflow.GetNextInteractiveAction( RequestContext.CurrentPerson, null, false );
            var showHtmlGuard = 0;

            while ( interactiveAction?.ActionTypeCache?.WorkflowAction is Rock.Workflow.Action.ShowHtml && showHtmlGuard++ < 10 )
            {
                interactiveAction.MarkComplete();

                if ( !workflowService.Process( workflow, memberRequirement, out workflowErrors ) )
                {
                    break;
                }

                interactiveAction = workflow.GetNextInteractiveAction( RequestContext.CurrentPerson, null, false );
            }

            if ( isWarningWorkflow )
            {
                memberRequirement.WarningWorkflowId = workflow.Id;
                memberRequirement.RequirementWarningDateTime = RockDateTime.Now;
            }
            else
            {
                memberRequirement.DoesNotMeetWorkflowId = workflow.Id;
                memberRequirement.RequirementFailDateTime = RockDateTime.Now;
            }

            RockContext.SaveChanges();

            var nextInteractiveAction = workflow.GetNextInteractiveAction( RequestContext.CurrentPerson, null, false );

            if ( nextInteractiveAction?.ActionTypeCache?.WorkflowAction is Rock.Workflow.Action.UserEntryForm )
            {
                return ActionOk( new LaunchRequirementWorkflowResponseBag
                {
                    Message = $"A '{workflowType.Name}' workflow has been started. The new workflow has an active form that is ready for input.",
                    WorkflowEntryUrl = GetWorkflowEntryUrl( workflowType.Guid, workflow.Guid )
                } );
            }

            return ActionOk( new LaunchRequirementWorkflowResponseBag
            {
                Message = $"A '{workflowType.Name}' workflow was started."
            } );
        }

        /// <summary>
        /// Gets the schedule and location options for the Assignment
        /// Preference modal.
        /// </summary>
        /// <returns>A <see cref="ScheduleAssignmentOptionsBag"/>.</returns>
        [BlockAction]
        public BlockActionResult GetScheduleAssignmentOptions( string groupMemberIdKey, int? selectedScheduleId )
        {
            var group = groupMemberIdKey.IsNotNullOrWhiteSpace()
                ? new GroupMemberService( RockContext ).Get( groupMemberIdKey, !PageCache.Layout.Site.DisablePredictableIds )?.Group
                : GetGroupFromPageParameter();

            if ( group == null )
            {
                return ActionBadRequest( $"{Model.Group.FriendlyTypeName} not found." );
            }

            if ( !group.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) && !IsAuthorizedToEdit( group ) )
            {
                return ActionBadRequest( "Not authorized to view this group." );
            }

            var groupId = group.Id;

            var schedules = new GroupLocationService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Where( gl => gl.GroupId == groupId )
                .SelectMany( gl => gl.Schedules )
                .Distinct()
                .ToList()
                .OrderByOrderAndNextScheduledDateTime()
                .Where( s => s.IsActive && s.IsPublic == true )
                .ToList();

            // Base the next start date on the start of the week so schedules order consistently.
            var occurrenceDate = RockDateTime.Now.SundayDate().AddDays( 1 );

            var options = new ScheduleAssignmentOptionsBag
            {
                Schedules = schedules
                    .Select( s => new GroupScheduleAssignmentBag
                    {
                        ScheduleId = s.Id,
                        ScheduleName = s.Name,
                        FormattedScheduleName = GetFormattedScheduleForListing( s.Name, s.StartTimeOfDay ),
                        ScheduleOrder = s.Order,
                        ScheduleNextStartDateTime = s.GetNextStartDateTime( occurrenceDate )?.ToRockDateTimeOffset()
                    } )
                    .ToList(),
                LocationItems = new List<ListItemBag>()
            };

            if ( selectedScheduleId.HasValue )
            {
                options.LocationItems = new LocationService( RockContext )
                    .GetByGroupSchedule( selectedScheduleId.Value, groupId )
                    .OrderBy( l => l.Name )
                    .ToList()
                    .Select( l => new ListItemBag
                    {
                        Text = l.Name,
                        Value = l.Id.ToString()
                    } )
                    .ToList();
            }

            return ActionOk( options );
        }

        #endregion Block Actions
    }
}

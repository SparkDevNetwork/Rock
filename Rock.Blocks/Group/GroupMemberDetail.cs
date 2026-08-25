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
        "Show \"Move To Another Group\" Button",
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
        "Are Requirements Refreshed When Block Is Loaded",
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
        "Append Organization Email Header/Footer",
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
            var box = new DetailBlockBox<GroupMemberBag, GroupMemberDetailOptionsBag>();
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                // A lookup was attempted when a non-zero GroupMemberId parameter was supplied, even as an IdKey or Guid.
                var groupMemberKey = PageParameter( PageParameterKey.GroupMemberId );
                var isLookupAttempt = groupMemberKey.IsNotNullOrWhiteSpace() && groupMemberKey.AsIntegerOrNull() != 0;

                box.ErrorMessage = isLookupAttempt
                    ? "Group Member not found. Group Member may have been moved to another group or deleted."
                    : "An incorrect querystring parameter was used. A valid GroupMemberId or GroupId parameter is required.";

                PrepareDetailBox( box, entity );

                return box;
            }

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
            box.NavigationUrls = GetBoxNavigationUrls();

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

            // Interaction stays disabled until the member is saved and unchanged, since requirement writes need a member record.
            options.IsRequirementInteractionDisabled = entity.Id == 0 || entity.IsNewOrChangedGroupMember( RockContext );

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

            // Use the stored requirement results when the member's saved role matches the
            // selected role; otherwise calculate on demand for the person and role.
            // Materialized immediately so later enumeration cannot re-run the calculations.
            if ( entity.Id != 0 && entity.GroupRoleId == selectedRoleId )
            {
                statusList = entity.GetGroupRequirementsStatuses( RockContext )?.ToList();
            }

            if ( statusList?.Any() != true && entity.PersonId != 0 )
            {
                statusList = entity.Group.PersonMeetsGroupRequirements( RockContext, entity.PersonId, selectedRoleId )?.ToList();
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

            var memberRequirementGuidsById = memberRequirementIds.Any()
                ? new GroupMemberRequirementService( RockContext ).Queryable().AsNoTracking()
                    .Where( r => memberRequirementIds.Contains( r.Id ) )
                    .Select( r => new { r.Id, r.Guid } )
                    .ToList()
                    .ToDictionary( r => r.Id, r => r.Guid )
                : new Dictionary<int, Guid>();

            return visibleStatuses
                .Select( s => new GroupMemberRequirementAlertBag
                {
                    Title = s.GroupRequirement.GroupRequirementType.Name,
                    Summary = isSummaryHidden ? string.Empty : s.GroupRequirement.GroupRequirementType.Summary,
                    MeetsGroupRequirement = s.MeetsGroupRequirement,
                    TypeIconCssClass = s.GroupRequirement.GroupRequirementType.IconCssClass,
                    CanOverride = s.MeetsGroupRequirement != MeetsGroupRequirement.Meets
                        && ( ( s.GroupRequirement.AllowLeadersToOverride && isLeader )
                            || s.GroupRequirement.GroupRequirementType.IsAuthorized( Authorization.OVERRIDE, RequestContext.CurrentPerson ) ),
                    GroupRequirementGuid = s.GroupRequirement.Guid,
                    GroupMemberRequirementGuid = s.GroupMemberRequirementId.HasValue && memberRequirementGuidsById.ContainsKey( s.GroupMemberRequirementId.Value )
                        ? memberRequirementGuidsById[s.GroupMemberRequirementId.Value]
                        : ( Guid? ) null
                } )
                .ToList();
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
                UploaderLabel = template.Name,
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

            if ( lastSent.HasValue )
            {
                statusBag.ButtonText = "Resend Signature Request";
                statusBag.Message = $"A signed {template.Name} document has not yet been received for {entity.Person.NickName}. The last request was sent {lastSent.Value.ToElapsedString()}.";
            }
            else
            {
                statusBag.ButtonText = "Send Signature Request";
                statusBag.Message = $"The required {template.Name} document has not yet been sent to {entity.Person.NickName} for signing.";
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
        /// Gets the navigation URLs required by the client.
        /// </summary>
        /// <returns>A dictionary of key and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            // TODO: Parent page URL carrying GroupId, CampusId, and sign-up parameters per conversion plan §7.14.
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl()
            };
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
                Person = entity.Person?.PrimaryAlias.ToListItemBag(),
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
        /// signed document uploader.
        /// </summary>
        /// <param name="entity">The group member being viewed or edited.</param>
        /// <returns>The binary file reference, or null.</returns>
        private ListItemBag GetLatestSignedDocumentFile( GroupMember entity )
        {
            var templateId = entity.Group.RequiredSignatureDocumentTemplateId;

            if ( !templateId.HasValue || entity.Id == 0 )
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

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( GroupMember entity, ValidPropertiesBox<GroupMemberBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            // TODO: Apply valid properties to the entity per conversion plan §8.
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
                // TODO: The add path must resolve the group before the authorization check below can grant group-level rights.
                entity = new GroupMember();
                entityService.Add( entity );
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
            // TODO: Member name crumb, optionally prefixed with the group name per the IncludeGroupNameInBreadcrumb setting.
            return new BreadCrumbResult
            {
                BreadCrumbs = new List<IBreadCrumb>()
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
            // TODO: Implement per conversion plan §8, including must-meet requirement enforcement and the archived-member prompt.
            return ActionBadRequest( "Not implemented." );
        }

        /// <summary>
        /// Restores a matching archived group member instead of creating a
        /// new record.
        /// </summary>
        /// <param name="archivedGroupMemberIdKey">The IdKey of the archived group member to restore.</param>
        /// <returns>The restored member's IdKey for reload.</returns>
        [BlockAction]
        public BlockActionResult RestoreArchivedGroupMember( string archivedGroupMemberIdKey )
        {
            // TODO: Implement per conversion plan §8.
            return ActionBadRequest( "Not implemented." );
        }

        /// <summary>
        /// Moves the group member to another group, optionally moving notes
        /// and fundraising transactions.
        /// </summary>
        /// <param name="bag">The move request.</param>
        /// <returns>The new member's IdKey.</returns>
        [BlockAction]
        public BlockActionResult MoveGroupMember( MoveGroupMemberRequestBag bag )
        {
            // TODO: Implement per conversion plan §8, including the fundraising transfer machinery.
            return ActionBadRequest( "Not implemented." );
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
            // TODO: Implement per conversion plan §8.
            return ActionBadRequest( "Not implemented." );
        }

        /// <summary>
        /// Gets the state required to open the quick communication modal.
        /// </summary>
        /// <param name="groupMemberIdKey">The IdKey of the member to communicate with.</param>
        /// <returns>A <see cref="CommunicationOptionsBag"/>.</returns>
        [BlockAction]
        public BlockActionResult GetCommunicationOptions( string groupMemberIdKey )
        {
            // TODO: Implement per conversion plan §8, including the four email and four SMS state cases.
            return ActionBadRequest( "Not implemented." );
        }

        /// <summary>
        /// Sends a quick email or SMS communication to the group member.
        /// </summary>
        /// <param name="bag">The communication request.</param>
        /// <returns>A success or validation error result.</returns>
        [BlockAction]
        public BlockActionResult SendCommunication( SendCommunicationRequestBag bag )
        {
            // TODO: Implement per conversion plan §8, with server-side required-field validation.
            return ActionBadRequest( "Not implemented." );
        }

        /// <summary>
        /// Sends the required signature document request to the group
        /// member. Pending Open Decision A in the conversion plan.
        /// </summary>
        /// <param name="groupMemberIdKey">The IdKey of the member to send the request to.</param>
        /// <returns>A success or error result.</returns>
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
        /// <param name="personIdKey">The IdKey of the selected person.</param>
        /// <returns>The existing member's bag, or an empty result when none exists.</returns>
        [BlockAction]
        public BlockActionResult GetExistingSignUpGroupMember( string personIdKey )
        {
            // TODO: Implement per conversion plan §8.
            return ActionBadRequest( "Not implemented." );
        }

        /// <summary>
        /// Recalculates the group requirements for the member and returns
        /// refreshed inline alert bags.
        /// </summary>
        /// <param name="groupMemberIdKey">The IdKey of the member.</param>
        /// <param name="selectedRoleId">The currently selected role identifier.</param>
        /// <returns>The refreshed requirement alerts.</returns>
        [BlockAction]
        public BlockActionResult RefreshRequirements( string groupMemberIdKey, int selectedRoleId )
        {
            // TODO: Implement per conversion plan §8.
            return ActionBadRequest( "Not implemented." );
        }

        /// <summary>
        /// Gets the schedule and location options for the Assignment
        /// Preference modal.
        /// </summary>
        /// <param name="groupIdKey">The IdKey of the group.</param>
        /// <param name="selectedScheduleId">The selected schedule identifier, when loading locations.</param>
        /// <returns>A <see cref="ScheduleAssignmentOptionsBag"/>.</returns>
        [BlockAction]
        public BlockActionResult GetScheduleAssignmentOptions( string groupIdKey, int? selectedScheduleId )
        {
            // TODO: Implement per conversion plan §8.
            return ActionBadRequest( "Not implemented." );
        }

        #endregion Block Actions
    }
}

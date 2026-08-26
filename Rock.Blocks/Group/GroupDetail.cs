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
using Rock.Data;
using Rock.Model;
using Rock.Model.Groups.Group.Options;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Group.GroupDetail;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Web;
using Rock.Web.Cache;

using RelationshipStrength = Rock.Enums.Group.RelationshipStrength;

namespace Rock.Blocks.Group
{
    /// <summary>
    /// Displays the details of a particular group.
    /// </summary>
    [DisplayName( "Group Detail" )]
    [Category( "Groups" )]
    [Description( "Displays the details of the given group." )]
    [IconCssClass( "ti ti-users-group" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [GroupTypesField(
        "Group Types: Include",
        Description = "Group types this block will display. If none are selected, all group types are shown except those in the exclusion list.",
        IsRequired = false,
        Order = 0,
        Category = AttributeCategory.GeneralSettings,
        Key = AttributeKey.GroupTypes )]

    [GroupTypesField(
        "Group Types: Exclude",
        Description = "Group types this block will never display, regardless of the inclusion list.",
        IsRequired = false,
        Order = 1,
        Category = AttributeCategory.GeneralSettings,
        Key = AttributeKey.GroupTypesExclude )]

    [BooleanField(
        "Security Role Groups Only",
        Description = "When enabled, only groups that function as security roles will be available in this block.",
        DefaultBooleanValue = false,
        Order = 2,
        Category = AttributeCategory.GeneralSettings,
        Key = AttributeKey.LimittoSecurityRoleGroups )]

    [BooleanField(
        "Navigation Group Types Only",
        Description = "When enabled, only group types marked to show in navigation will be available in this block.",
        DefaultBooleanValue = false,
        Order = 3,
        Category = AttributeCategory.GeneralSettings,
        Key = AttributeKey.LimitToShowInNavigationGroupTypes )]

    [DefinedValueField(
        "Map Style",
        Description = "The visual style applied to maps displayed within this block.",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.MAP_STYLES,
        IsRequired = true,
        AllowMultiple = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.MAP_STYLE_ROCK,
        Order = 4,
        Category = AttributeCategory.GeneralSettings,
        Key = AttributeKey.MapStyle )]

    [BooleanField(
        "Show Copy Button",
        Description = "When enabled, displays a copy button that duplicates the group and its associated authorization rules.",
        DefaultBooleanValue = false,
        Order = 5,
        Category = AttributeCategory.GeneralSettings,
        Key = AttributeKey.ShowCopyButton )]

    [BooleanField(
        "Show Location Addresses",
        Description = "Whether location addresses are visible when viewing group details.",
        DefaultBooleanValue = true,
        Order = 6,
        Category = AttributeCategory.GeneralSettings,
        Key = AttributeKey.ShowLocationAddresses )]

    [BooleanField(
        "Exclude Inactive Campuses",
        Description = "When enabled, inactive campuses are hidden from the campus selector when editing a group.",
        DefaultBooleanValue = false,
        Order = 7,
        Category = AttributeCategory.GeneralSettings,
        Key = AttributeKey.PreventSelectingInactiveCampus )]

    [BooleanField(
        "Enable Group Tags",
        Description = "When enabled, tags are displayed on the group detail view.",
        DefaultBooleanValue = true,
        Order = 8,
        Category = AttributeCategory.GeneralSettings,
        Key = AttributeKey.EnableGroupTags )]

    [BooleanField(
        "Grant Creator Admin Rights",
        Description = "When enabled, the person who creates a new group is automatically granted Administrate security rights to that group. Disable this if your security model assigns rights through roles rather than individual grants.",
        DefaultBooleanValue = false,
        Order = 9,
        Category = AttributeCategory.GeneralSettings,
        Key = AttributeKey.AddAdministrateSecurityToGroupCreator )]

    [LinkedPage(
        "Group Map Page",
        Description = "The page used to display the detailed map for a group.",
        IsRequired = false,
        Order = 10,
        Category = AttributeCategory.PageRouting,
        Key = AttributeKey.GroupMapPage )]

    [LinkedPage(
        "Attendance Page",
        Description = "The page used to display the group's attendance records.",
        IsRequired = false,
        Order = 11,
        Category = AttributeCategory.PageRouting,
        Key = AttributeKey.AttendancePage )]

    [LinkedPage(
        "Registration Instance Page",
        Description = "The page used to display registration instance details.",
        IsRequired = false,
        Order = 12,
        Category = AttributeCategory.PageRouting,
        Key = AttributeKey.RegistrationInstancePage )]

    [LinkedPage(
        "Event Item Occurrence Page",
        Description = "The page used to display details for an event item occurrence.",
        IsRequired = false,
        Order = 13,
        Category = AttributeCategory.PageRouting,
        Key = AttributeKey.EventItemOccurrencePage )]

    [LinkedPage(
        "Content Item Page",
        Description = "The page used to display content channel item details.",
        IsRequired = false,
        Order = 14,
        Category = AttributeCategory.PageRouting,
        Key = AttributeKey.ContentItemPage )]

    [LinkedPage(
        "Group List Page",
        Description = "The page used to display the related group list.",
        IsRequired = false,
        Order = 15,
        Category = AttributeCategory.PageRouting,
        Key = AttributeKey.GroupListPage )]

    [LinkedPage(
        "Fundraising Progress Page",
        Description = "The page used to display fundraising progress for the group's members.",
        IsRequired = false,
        Order = 16,
        Category = AttributeCategory.PageRouting,
        Key = AttributeKey.FundraisingProgressPage )]

    [LinkedPage(
        "Group History Page",
        Description = "The page used to display the group's change history.",
        IsRequired = false,
        Order = 17,
        Category = AttributeCategory.PageRouting,
        Key = AttributeKey.GroupHistoryPage )]

    [LinkedPage(
        "Group Scheduler Page",
        Description = "The page used to manage scheduling for this group.",
        DefaultValue = "1815D8C6-7C4A-4C05-A810-CF23BA937477,D0F198E2-6111-4EC1-8D1D-55AC10E28D04",
        IsRequired = false,
        Order = 18,
        Category = AttributeCategory.PageRouting,
        Key = AttributeKey.GroupSchedulerPage )]

    [LinkedPage(
        "Group RSVP List Page",
        Description = "The page used to manage RSVPs for this group.",
        DefaultValue = Rock.SystemGuid.Page.GROUP_RSVP_LIST,
        IsRequired = false,
        Order = 19,
        Category = AttributeCategory.PageRouting,
        Key = AttributeKey.GroupRSVPPage )]

    [LinkedPage(
        "Group Placement Page",
        Description = "The page used to manage group member placements.",
        IsRequired = false,
        Order = 20,
        Category = AttributeCategory.PageRouting,
        Key = AttributeKey.GroupPlacementPage )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "A0F8323D-B1A3-4DED-A6F6-B2483C0917D2" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "87EEE2AD-F876-4827-9327-280FB1FDA1D1" )]
    [Rock.SystemGuid.BlockTypeGuid( "582BEEA1-5B27-444D-BC0A-F60CEB053981" )]
    public class GroupDetail : RockEntityDetailBlockType<Model.Group, GroupBag>, IBreadCrumbBlock
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string GroupId = "GroupId";
            public const string ParentGroupId = "ParentGroupId";
            public const string ExpandedIds = "ExpandedIds";
            public const string ReturnUrl = "returnUrl";
            public const string AutoEdit = "autoEdit";
        }

        private static class NavigationUrlKey
        {
            public const string AttendancePage = "AttendancePage";
            public const string GroupSchedulerPage = "GroupSchedulerPage";
            public const string GroupRSVPPage = "GroupRSVPPage";
            public const string GroupPlacementPage = "GroupPlacementPage";
            public const string GroupMapPage = "GroupMapPage";
            public const string GroupHistoryPage = "GroupHistoryPage";
            public const string FundraisingProgressPage = "FundraisingProgressPage";
            public const string RegistrationInstancePage = "RegistrationInstancePage";
            public const string EventItemOccurrencePage = "EventItemOccurrencePage";
            public const string ContentItemPage = "ContentItemPage";
        }

        private static class AttributeKey
        {
            public const string GroupTypes = "GroupTypes";
            public const string GroupTypesExclude = "GroupTypesExclude";
            public const string LimittoSecurityRoleGroups = "LimittoSecurityRoleGroups";
            public const string LimitToShowInNavigationGroupTypes = "LimitToShowInNavigationGroupTypes";
            public const string MapStyle = "MapStyle";
            public const string GroupMapPage = "GroupMapPage";
            public const string AttendancePage = "AttendancePage";
            public const string RegistrationInstancePage = "RegistrationInstancePage";
            public const string EventItemOccurrencePage = "EventItemOccurrencePage";
            public const string ContentItemPage = "ContentItemPage";
            public const string ShowCopyButton = "ShowCopyButton";
            public const string GroupListPage = "GroupListPage";
            public const string FundraisingProgressPage = "FundraisingProgressPage";
            public const string ShowLocationAddresses = "ShowLocationAddresses";
            public const string PreventSelectingInactiveCampus = "PreventSelectingInactiveCampus";
            public const string GroupHistoryPage = "GroupHistoryPage";
            public const string GroupSchedulerPage = "GroupSchedulerPage";
            public const string GroupRSVPPage = "GroupRSVPPage";
            public const string EnableGroupTags = "EnableGroupTags";
            public const string AddAdministrateSecurityToGroupCreator = "AddAdministrateSecurityToGroupCreator";
            public const string GroupPlacementPage = "GroupPlacementPage";
        }

        private static class AttributeCategory
        {
            public const string GeneralSettings = "General Settings";
            public const string PageRouting = "Page Routing";
        }

        private static class EntityKey
        {
            public const string GroupRequirement = "GroupRequirement";
            public const string GroupMemberWorkflowTrigger = "GroupMemberWorkflowTrigger";
            public const string GroupSync = "GroupSync";
            public const string GroupLocation = "GroupLocation";
        }

        #endregion Keys

        #region Fields

        /// <summary>
        /// Per-request memo for the active <see cref="GroupTypeCache"/>.
        /// </summary>
        private GroupTypeCache _cachedGroupType;

        /// <summary>
        /// Per-request memo for the GROUP_ADMINISTRATORS membership lookup.
        /// </summary>
        private bool? _isCurrentPersonGroupAdministrator;

        #endregion Fields

        #region Properties

        /// <summary>
        /// System-wide chat feature flag. Distinct from
        /// <see cref="GroupBag.IsChatEnabled"/> (effective per-group state)
        /// and <see cref="Group.IsChatEnabledOverride"/> (per-group override).
        /// </summary>
        private static bool IsSystemChatEnabled => ChatHelper.IsChatEnabled;

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<GroupBag, GroupDetailOptionsBag>();
            var entity = GetInitialEntity();

            SetBoxInitialEntityState( box, entity );

            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions( entity, box.NavigationUrls );
            box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<Model.Group>();

            return box;
        }

        /// <inheritdoc/>
        protected override Model.Group GetInitialEntity()
        {
            var entity = GetInitialEntity<Model.Group, GroupService>( RockContext, PageParameterKey.GroupId );

            ApplyNewGroupDefaultValues( entity );

            return entity;
        }

        /// <summary>
        /// Applies default values to a new <see cref="Model.Group"/>. This
        /// runs only on the Add path and returns immediately when the
        /// entity is null or already has an Id.
        /// </summary>
        /// <param name="entity">The group entity.</param>
        /// <param name="groupService">An optional service instance to use for queries.</param>
        private void ApplyNewGroupDefaultValues( Model.Group entity, GroupService groupService = null )
        {
            if ( entity == null || entity.Id != 0 )
            {
                return;
            }

            groupService = groupService ?? new GroupService( RockContext );

            // Pre-populate parent group from ?ParentGroupId=N.
            var parentGroupParam = PageParameter( PageParameterKey.ParentGroupId );
            if ( parentGroupParam.IsNotNullOrWhiteSpace() )
            {
                var parentGroup = groupService.Get( parentGroupParam, !PageCache.Layout.Site.DisablePredictableIds );
                if ( parentGroup != null )
                {
                    entity.ParentGroupId = parentGroup.Id;
                    entity.ParentGroup = parentGroup;
                }
            }

            // When the block is locked to security-role groups, default the
            // group type to the security-role group type and skip the
            // parent-driven auto-pick since the dropdown is already
            // constrained to a single option.
            if ( GetAttributeValue( AttributeKey.LimittoSecurityRoleGroups ).AsBoolean() )
            {
                var securityRoleGroupType = GroupTypeCache.GetSecurityRoleGroupType();
                if ( securityRoleGroupType != null )
                {
                    entity.GroupTypeId = securityRoleGroupType.Id;
                }
                return;
            }

            // When a parent group narrows the allowed child types, auto-pick
            // the single option the current person is authorized to edit.
            // Otherwise leave the field blank so the user makes the choice.
            if ( entity.ParentGroup != null )
            {
                var allowedChildGroupTypes = GetAllowedGroupTypes( GroupTypeCache.Get( entity.ParentGroup.GroupTypeId ), RockContext ).ToList();

                var authorizedGroupTypes = new List<Model.GroupType>();
                foreach ( var allowedGroupType in allowedChildGroupTypes )
                {
                    // Probe authorization by temporarily assigning each
                    // candidate group type to the entity.
                    entity.GroupTypeId = allowedGroupType.Id;
                    entity.GroupType = allowedGroupType;

                    if ( entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                    {
                        authorizedGroupTypes.Add( allowedGroupType );
                    }
                }

                if ( authorizedGroupTypes.Count == 1 )
                {
                    entity.GroupType = authorizedGroupTypes[0];
                    entity.GroupTypeId = authorizedGroupTypes[0].Id;
                }
                else
                {
                    // Reset so the user makes the selection. When no group
                    // types are authorized, the downstream IsAuthorized
                    // check falls back to parent-group authorization.
                    entity.GroupType = null;
                    entity.GroupTypeId = 0;
                }
            }
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<GroupBag, GroupDetailOptionsBag> box, Model.Group entity )
        {
            if ( entity == null )
            {
                box.ErrorMessage = $"The {Model.Group.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = entity.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            if ( entity.Id != 0 )
            {
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( Model.Group.FriendlyTypeName );
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( Model.Group.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the box options required for the component to render the view.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private GroupDetailOptionsBag GetBoxOptions( Model.Group entity, Dictionary<string, string> navigationUrls )
        {
            var allowedGroupTypes = BuildAllowedGroupTypeListItems( entity?.ParentGroup );

            var options = new GroupDetailOptionsBag
            {
                PreventSelectingInactiveCampus = GetAttributeValue( AttributeKey.PreventSelectingInactiveCampus ).AsBoolean(),
                IsLimitedToSecurityRoleGroups = GetAttributeValue( AttributeKey.LimittoSecurityRoleGroups ).AsBoolean(),
                IsCurrentPersonGroupAdministrator = IsCurrentPersonGroupAdministrator(),
                AllowedGroupTypes = allowedGroupTypes,
                AllowedGroupTypesWarning = GetEmptyAllowedGroupTypesWarning( entity?.ParentGroup, allowedGroupTypes.Any() ),
                SignatureDocumentTemplates = BuildSignatureDocumentTemplateListItems( entity ),
                RsvpSystemCommunicationOptions = BuildRsvpSystemCommunicationOptions(),
                AddModeCancelUrl = GetAddModeCancelUrl()
            };

            var groupType = GetGroupTypeCache( entity );

            if ( entity == null || entity.Id == 0 || groupType == null )
            {
                return options;
            }

            /*
                5/19/2026 - MSE

                The member-with-address probe is an expensive multi-join (GroupMember →
                Person → PrimaryFamily → GroupLocations). Skip it when hasGroupLocations
                already satisfies the visibility condition or when the Map page URL isn't
                configured.
            */
            // An EXISTS query rather than entity.GroupLocations.Any(), which would
            // lazy-load every GroupLocation row for the group just to test for one.
            var hasGroupLocations = new GroupLocationService( RockContext ).Queryable()
                .Any( gl => gl.GroupId == entity.Id );

            bool hasMemberWithAddress = false;
            if ( !hasGroupLocations
                && navigationUrls.TryGetValue( NavigationUrlKey.GroupMapPage, out var mapUrl )
                && mapUrl.IsNotNullOrWhiteSpace() )
            {
                hasMemberWithAddress = new GroupMemberService( RockContext ).Queryable()
                    .Where( m => m.GroupId == entity.Id )
                    .Any( m => m.Person.PrimaryFamily != null
                        && m.Person.PrimaryFamily.GroupLocations
                            .Any( gl => gl.Location != null && !string.IsNullOrEmpty( gl.Location.Street1 ) ) );
            }

            var isSchedulingActive = !entity.DisableScheduling;

            // Group Tools visibility.
            bool IsToolVisible( string urlKey, bool gateCondition )
                => navigationUrls.TryGetValue( urlKey, out var url )
                    && url.IsNotNullOrWhiteSpace()
                    && gateCondition;

            options.IsAttendanceVisible  = IsToolVisible( NavigationUrlKey.AttendancePage,          groupType.TakesAttendance );
            options.IsSchedulerVisible   = IsToolVisible( NavigationUrlKey.GroupSchedulerPage,      groupType.IsSchedulingEnabled && isSchedulingActive );
            options.IsRsvpVisible        = IsToolVisible( NavigationUrlKey.GroupRSVPPage,           groupType.EnableRSVP );
            options.IsPlacementVisible   = IsToolVisible( NavigationUrlKey.GroupPlacementPage,      isSchedulingActive );
            options.IsMapVisible         = IsToolVisible( NavigationUrlKey.GroupMapPage,            hasGroupLocations || hasMemberWithAddress );
            options.IsHistoryVisible     = IsToolVisible( NavigationUrlKey.GroupHistoryPage,        groupType.EnableGroupHistory );
            options.IsFundraisingVisible = IsToolVisible( NavigationUrlKey.FundraisingProgressPage, IsFundraisingGroupType( entity.GroupTypeId ) );

            var canEdit = entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
            options.IsCopyButtonShown = GetAttributeValue( AttributeKey.ShowCopyButton ).AsBoolean() && canEdit;

            var hasHistory = groupType.EnableGroupHistory
                && (
                    new GroupHistoricalService( RockContext ).Queryable().Any( a => a.GroupId == entity.Id )
                    || new GroupMemberHistoricalService( RockContext ).Queryable().Any( a => a.GroupId == entity.Id )
                );

            // Archive replaces Delete when the group type has history enabled
            // and at least one history row exists for the group.
            options.IsArchiveVisible = !entity.IsSystem && !entity.IsArchived && canEdit && hasHistory;
            options.IsDeleteVisible  = !entity.IsSystem && !entity.IsArchived && canEdit && !hasHistory;

            options.IsTagListShown = GetAttributeValue( AttributeKey.EnableGroupTags ).AsBoolean() && groupType.EnableGroupTag;
            options.MapStyleValueGuid = GetAttributeValue( AttributeKey.MapStyle ).AsGuidOrNull();

            return options;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var groupIdParam = new Dictionary<string, string>
            {
                [PageParameterKey.GroupId] = "((Key))"
            };

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.AttendancePage]           = this.GetLinkedPageUrl( AttributeKey.AttendancePage, groupIdParam ),
                [NavigationUrlKey.GroupSchedulerPage]       = this.GetLinkedPageUrl( AttributeKey.GroupSchedulerPage, groupIdParam ),
                [NavigationUrlKey.GroupRSVPPage]            = this.GetLinkedPageUrl( AttributeKey.GroupRSVPPage, groupIdParam ),
                [NavigationUrlKey.GroupMapPage]             = this.GetLinkedPageUrl( AttributeKey.GroupMapPage, groupIdParam ),
                [NavigationUrlKey.GroupHistoryPage]         = this.GetLinkedPageUrl( AttributeKey.GroupHistoryPage, groupIdParam ),
                [NavigationUrlKey.FundraisingProgressPage]  = this.GetLinkedPageUrl( AttributeKey.FundraisingProgressPage, groupIdParam ),
                [NavigationUrlKey.RegistrationInstancePage] = this.GetLinkedPageUrl( AttributeKey.RegistrationInstancePage, groupIdParam ),
                [NavigationUrlKey.EventItemOccurrencePage]  = this.GetLinkedPageUrl( AttributeKey.EventItemOccurrencePage, groupIdParam ),
                [NavigationUrlKey.ContentItemPage]          = this.GetLinkedPageUrl( AttributeKey.ContentItemPage, groupIdParam ),
                [NavigationUrlKey.GroupPlacementPage]       = this.GetLinkedPageUrl( AttributeKey.GroupPlacementPage, new Dictionary<string, string>
                {
                    ["SourceGroup"] = "((Key))",
                    ["AllowMultiplePlacements"] = "false"
                } )
            };
        }

        /// <inheritdoc/>
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            var key = pageReference.GetPageParameter( PageParameterKey.GroupId );

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
                var addCrumb = new BreadCrumbLink( "New Group", new PageReference( pageReference.PageId, 0 ) );
                return new BreadCrumbResult { BreadCrumbs = new List<IBreadCrumb> { addCrumb } };
            }

            var info = new GroupService( RockContext ).GetSelect( key, g => new { g.Name, g.Id } );
            if ( info == null )
            {
                return new BreadCrumbResult { BreadCrumbs = new List<IBreadCrumb>() };
            }

            var pageParameters = new Dictionary<string, string>
            {
                [PageParameterKey.GroupId] = Rock.Utility.IdHasher.Instance.GetHash( info.Id )
            };
            var breadCrumbPageRef = new PageReference( pageReference.PageId, 0, pageParameters );
            var breadCrumb = new BreadCrumbLink( info.Name, breadCrumbPageRef );

            return new BreadCrumbResult
            {
                BreadCrumbs = new List<IBreadCrumb> { breadCrumb }
            };
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="GroupBag"/> that represents the entity.</returns>
        private GroupBag GetCommonEntityBag( Model.Group entity )
        {
            if ( entity == null )
            {
                return null;
            }

            if ( entity.Attributes == null )
            {
                entity.LoadAttributes( RockContext );
            }

            var groupType = GetGroupTypeCache( entity );
            var canEdit = entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
            var canAdministrate = entity.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson );

            var hasChildGroups = entity.Id > 0 && new GroupService( RockContext ).Queryable().Any( g => g.ParentGroupId == entity.Id );

            return new GroupBag
            {
                IdKey = entity.IdKey,
                IsActive = entity.IsActive,
                IsSystem = entity.IsSystem,
                IsArchived = entity.IsArchived,
                CanEdit = canEdit,
                CanAdministrate = canAdministrate,
                HasChildGroups = hasChildGroups,

                // Header chrome.
                Name = entity.Name,
                IconCssClass = groupType?.IconCssClass,
                GroupType = BuildGroupTypeRef( groupType ),
                CampusName = entity.CampusId.HasValue ? CampusCache.Get( entity.CampusId.Value )?.Name : null,

                // Subheader.
                IsPublic = entity.IsPublic,
                IsSecurityRole = entity.IsSecurityRole,
                ElevatedSecurityLevel = entity.ElevatedSecurityLevel,
                RelationshipStrength = GetRelationshipStrength( entity, groupType ),
                IsOverridingPeerNetwork = entity.IsOverridingGroupTypePeerNetworkConfiguration,

                PhotoUrl = entity.PhotoUrl,
                Description = entity.Description,
                MeetingStyle = entity.MeetingStyle,
                Administrator = BuildAdministratorRef( entity.GroupAdministratorPersonAlias, groupType ),
                AdministratorLabel = BuildAdministratorLabel( groupType ),
                ParentGroup = BuildParentGroupRef( entity.ParentGroup ),
                ScheduleFriendlyText = entity.Schedule?.FriendlyScheduleText,
                GroupCapacity = entity.GroupCapacity
            };
        }

        /// <inheritdoc/>
        protected override GroupBag GetEntityBagForView( Model.Group entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );
            bag.Linkages = BuildLinkages( entity );
            bag.MeetingLocations = BuildMeetingLocations( entity );

            if ( entity.GetGroupTypeRoleLimitWarnings( out var roleLimitWarning ) )
            {
                bag.RoleLimitWarning = roleLimitWarning;
            }

            bag.IsChatEnabled = IsSystemChatEnabled && entity.GetIsChatEnabled();

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson, enforceSecurity: true );

            return bag;
        }

        /// <inheritdoc/>
        protected override GroupBag GetEntityBagForEdit( Model.Group entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.MemberCount = entity.Id > 0
                ? new GroupMemberService( RockContext ).Queryable()
                    .Count( m => m.GroupId == entity.Id && m.GroupMemberStatus == GroupMemberStatus.Active )
                : 0;

            bag.HasActiveDescendants = entity.Id > 0
                && new GroupService( RockContext ).HasDescendantGroups( entity.Id, false );

            // Inactive flow + photo.
            bag.InactiveReasonValueId = entity.InactiveReasonValueId;
            bag.InactiveReasonNote = entity.InactiveReasonNote;
            bag.InactivateChildGroups = false;
            bag.PhotoBinaryFile = BuildBinaryFileRef( entity.Photo, entity.PhotoId );

            // Overview.
            bag.GroupTypeId = entity.GroupTypeId > 0 ? entity.GroupTypeId : ( int? ) null;
            bag.Campus = BuildCampusListItem( entity.CampusId );
            bag.StatusValueId = entity.StatusValueId;

            // Admin & Security.
            bag.RequiredSignatureDocumentTemplateId = entity.RequiredSignatureDocumentTemplateId;
            bag.GroupMemberRecordSource = BuildDefinedValueListItem( entity.GroupMemberRecordSourceValueId );

            // Relationships.
            bag.OverrideRelationshipStrength = entity.IsOverridingGroupTypePeerNetworkConfiguration;
            bag.RelationshipStrengthOverride = entity.RelationshipStrengthOverride.HasValue
                ? ( RelationshipStrength? ) entity.RelationshipStrengthOverride.Value
                : null;
            bag.RelationshipGrowthEnabledOverride = entity.RelationshipGrowthEnabledOverride;
            bag.LeaderToLeaderRelationshipMultiplierOverride = entity.LeaderToLeaderRelationshipMultiplierOverride;
            bag.LeaderToNonLeaderRelationshipMultiplierOverride = entity.LeaderToNonLeaderRelationshipMultiplierOverride;
            bag.NonLeaderToLeaderRelationshipMultiplierOverride = entity.NonLeaderToLeaderRelationshipMultiplierOverride;
            bag.NonLeaderToNonLeaderRelationshipMultiplierOverride = entity.NonLeaderToNonLeaderRelationshipMultiplierOverride;

            // RSVP.
            bag.RsvpReminderOffsetDays = entity.RSVPReminderOffsetDays;
            bag.RsvpReminderSystemCommunication = BuildSystemCommunicationListItem( entity.RSVPReminderSystemCommunicationId );

            // Inline schedule.
            HydrateScheduleFields( bag, entity );

            // Member scheduling.
            bag.SchedulingMustMeetRequirements = entity.SchedulingMustMeetRequirements;
            bag.DisableScheduling = entity.DisableScheduling;
            bag.DisableScheduleToolboxAccess = entity.DisableScheduleToolboxAccess;
            bag.ScheduleConfirmationLogic = entity.ScheduleConfirmationLogic;
            bag.ScheduleCoordinatorPerson = entity.ScheduleCoordinatorPersonAlias.ToListItemBag();
            bag.HasCoordinatorNotificationOverride = entity.ScheduleCoordinatorNotificationTypes.HasValue;
            bag.ScheduleCoordinatorNotificationTypes = entity.ScheduleCoordinatorNotificationTypes ?? ScheduleCoordinatorNotificationType.None;
            bag.AttendanceRecordRequiredForCheckIn = entity.AttendanceRecordRequiredForCheckIn;

            // Chat.
            bag.IsChatEnabledOverride = entity.IsChatEnabledOverride;
            bag.IsLeavingChatChannelAllowedOverride = entity.IsLeavingChatChannelAllowedOverride;
            bag.IsChatChannelPublicOverride = entity.IsChatChannelPublicOverride;
            bag.IsChatChannelAlwaysShownOverride = entity.IsChatChannelAlwaysShownOverride;
            bag.ChatPushNotificationModeOverride = entity.ChatPushNotificationModeOverride;
            bag.ChatChannelAvatarBinaryFile = BuildBinaryFileRef( entity.ChatChannelAvatarBinaryFile, entity.ChatChannelAvatarBinaryFileId );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson, enforceSecurity: true );

            bag.GroupMemberWorkflowTriggers = LoadGroupMemberWorkflowTriggers( entity );

            if ( bag.CanAdministrate )
            {
                bag.GroupMemberAttributes = LoadGroupMemberAttributes( entity );
                bag.GroupRequirements = LoadGroupRequirements( entity );
                bag.GroupSyncs = LoadGroupSyncs( entity );
            }

            bag.GroupLocations = LoadGroupLocations( entity );

            return bag;
        }

        /// <inheritdoc/>
        protected override bool TryGetEntityForEditAction( string idKey, out Model.Group entity, out BlockActionResult error )
        {
            var entityService = new GroupService( RockContext );
            error = null;

            if ( idKey.IsNotNullOrWhiteSpace() )
            {
                entity = entityService.Get( idKey, !PageCache.Layout.Site.DisablePredictableIds );
            }
            else
            {
                entity = new Model.Group();
                entityService.Add( entity );

                ApplyNewGroupDefaultValues( entity, entityService );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{Model.Group.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit {Model.Group.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( Model.Group entity, ValidPropertiesBox<GroupBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.Description = box.Bag.Description );

            box.IfValidProperty( nameof( box.Bag.MeetingStyle ),
                () => entity.MeetingStyle = box.Bag.MeetingStyle );

            box.IfValidProperty( nameof( box.Bag.PhotoBinaryFile ),
                () => ApplyPhotoBinaryFile( entity, box.Bag ) );

            box.IfValidProperty( nameof( box.Bag.IsActive ), () =>
            {
                entity.IsActive = box.Bag.IsActive;

                // Clear the inactive-reason fields when the group is active.
                if ( box.Bag.IsActive )
                {
                    entity.InactiveReasonValueId = null;
                    entity.InactiveReasonNote = null;
                }
                else
                {
                    entity.InactiveReasonValueId = box.Bag.InactiveReasonValueId;
                    entity.InactiveReasonNote = box.Bag.InactiveReasonNote;
                }
            } );

            box.IfValidProperty( nameof( box.Bag.IsPublic ),
                () => entity.IsPublic = box.Bag.IsPublic );

            box.IfValidProperty( nameof( box.Bag.GroupTypeId ), () =>
            {
                if ( box.Bag.GroupTypeId.HasValue && box.Bag.GroupTypeId.Value > 0 )
                {
                    entity.GroupTypeId = box.Bag.GroupTypeId.Value;
                }
            } );

            // Resolve the GroupType after GroupTypeId has been assigned so the
            // downstream cascades observe the new value.
            var groupType = GetGroupTypeCache( entity );

            box.IfValidProperty( nameof( box.Bag.ParentGroup ),
                () => entity.ParentGroupId = box.Bag.ParentGroup?.GetEntityId<Model.Group>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.Campus ),
                () => entity.CampusId = box.Bag.Campus?.GetEntityId<Campus>( RockContext ) );

            // Fall back to the sole campus when the group type requires a
            // campus and the user did not select one (single-campus installs).
            if ( !entity.CampusId.HasValue && groupType?.GroupsRequireCampus == true )
            {
                entity.CampusId = CampusCache.SingleCampusId;
            }

            box.IfValidProperty( nameof( box.Bag.StatusValueId ),
                () => entity.StatusValueId = box.Bag.StatusValueId );

            box.IfValidProperty( nameof( box.Bag.GroupCapacity ),
                () => entity.GroupCapacity = box.Bag.GroupCapacity );

            box.IfValidProperty( nameof( box.Bag.RequiredSignatureDocumentTemplateId ),
                () => entity.RequiredSignatureDocumentTemplateId = box.Bag.RequiredSignatureDocumentTemplateId );

            box.IfValidProperty( nameof( box.Bag.Administrator ), () =>
            {
                if ( groupType != null && groupType.ShowAdministrator )
                {
                    entity.GroupAdministratorPersonAliasId = ResolvePersonAliasIdFromBag( box.Bag.Administrator );
                }
            } );

            box.IfValidProperty( nameof( box.Bag.GroupMemberRecordSource ), () =>
            {
                if ( groupType != null && groupType.AllowGroupSpecificRecordSource )
                {
                    entity.GroupMemberRecordSourceValueId = box.Bag.GroupMemberRecordSource?.GetEntityId<DefinedValue>( RockContext );
                }
                else
                {
                    entity.GroupMemberRecordSourceValueId = null;
                }
            } );

            var isCurrentPersonGroupAdministrator = IsCurrentPersonGroupAdministrator();

            /*
                5/26/2026 - MSE

                IsSecurityRole precedence:
                1. LimittoSecurityRoleGroups attribute forces every group in
                   this block to be a security role regardless of user input.
                2. Non-administrators have no authority over security-role
                   status, so the bag value is ignored.
                3. Administrators get the bag value applied directly.
            */
            box.IfValidProperty( nameof( box.Bag.IsSecurityRole ), () =>
            {
                if ( GetAttributeValue( AttributeKey.LimittoSecurityRoleGroups ).AsBoolean() )
                {
                    entity.IsSecurityRole = true;
                    return;
                }

                if ( !isCurrentPersonGroupAdministrator )
                {
                    return;
                }

                entity.IsSecurityRole = box.Bag.IsSecurityRole;
            } );

            box.IfValidProperty( nameof( box.Bag.ElevatedSecurityLevel ), () =>
            {
                if ( !isCurrentPersonGroupAdministrator )
                {
                    return;
                }

                entity.ElevatedSecurityLevel = box.Bag.ElevatedSecurityLevel;
                if ( !entity.IsSecurityRole )
                {
                    entity.ElevatedSecurityLevel = Rock.Utility.Enums.ElevatedSecurityLevel.None;
                }
            } );

            box.IfValidProperty( nameof( box.Bag.OverrideRelationshipStrength ), () =>
            {
                var isPeerNetworkEnabled = groupType?.IsPeerNetworkEnabled == true;
                if ( !isPeerNetworkEnabled )
                {
                    return;
                }

                var bagStrength = box.Bag.RelationshipStrengthOverride ?? RelationshipStrength.None;
                var isStrengthOverridden = box.Bag.OverrideRelationshipStrength
                    && bagStrength != RelationshipStrength.None;

                if ( box.Bag.OverrideRelationshipStrength )
                {
                    // Persist the chosen strength. None is a valid override.
                    entity.RelationshipStrengthOverride = ( int ) bagStrength;
                }
                else
                {
                    // No override; inherit from the group type.
                    entity.RelationshipStrengthOverride = null;
                }

                // The growth flag and multipliers only apply when a non-None
                // strength is overridden.
                entity.RelationshipGrowthEnabledOverride = isStrengthOverridden
                    ? box.Bag.RelationshipGrowthEnabledOverride
                    : null;
                entity.LeaderToLeaderRelationshipMultiplierOverride = isStrengthOverridden
                    ? box.Bag.LeaderToLeaderRelationshipMultiplierOverride
                    : null;
                entity.LeaderToNonLeaderRelationshipMultiplierOverride = isStrengthOverridden
                    ? box.Bag.LeaderToNonLeaderRelationshipMultiplierOverride
                    : null;
                entity.NonLeaderToLeaderRelationshipMultiplierOverride = isStrengthOverridden
                    ? box.Bag.NonLeaderToLeaderRelationshipMultiplierOverride
                    : null;
                entity.NonLeaderToNonLeaderRelationshipMultiplierOverride = isStrengthOverridden
                    ? box.Bag.NonLeaderToNonLeaderRelationshipMultiplierOverride
                    : null;
            } );

            // RSVP overrides are cleared when the group type pins the value
            // or when the group type does not have RSVP enabled.
            box.IfValidProperty( nameof( box.Bag.RsvpReminderOffsetDays ), () =>
            {
                if ( groupType?.EnableRSVP == true )
                {
                    entity.RSVPReminderOffsetDays = groupType.RSVPReminderOffsetDays.HasValue
                        ? ( int? ) null
                        : box.Bag.RsvpReminderOffsetDays;
                }
                else
                {
                    entity.RSVPReminderOffsetDays = null;
                }
            } );

            box.IfValidProperty( nameof( box.Bag.RsvpReminderSystemCommunication ), () =>
            {
                if ( groupType?.EnableRSVP == true )
                {
                    entity.RSVPReminderSystemCommunicationId = groupType.RSVPReminderSystemCommunicationId.HasValue
                        ? ( int? ) null
                        : box.Bag.RsvpReminderSystemCommunication?.GetEntityId<SystemCommunication>( RockContext );
                }
                else
                {
                    entity.RSVPReminderSystemCommunicationId = null;
                }
            } );

            box.IfValidProperty( nameof( box.Bag.SchedulingMustMeetRequirements ),
                () => entity.SchedulingMustMeetRequirements = box.Bag.SchedulingMustMeetRequirements );

            box.IfValidProperty( nameof( box.Bag.DisableScheduling ),
                () => entity.DisableScheduling = box.Bag.DisableScheduling );

            box.IfValidProperty( nameof( box.Bag.DisableScheduleToolboxAccess ),
                () => entity.DisableScheduleToolboxAccess = box.Bag.DisableScheduleToolboxAccess );

            box.IfValidProperty( nameof( box.Bag.ScheduleConfirmationLogic ),
                () => entity.ScheduleConfirmationLogic = box.Bag.ScheduleConfirmationLogic );

            box.IfValidProperty( nameof( box.Bag.ScheduleCoordinatorPerson ), () =>
            {
                entity.ScheduleCoordinatorPersonAliasId = ResolvePersonAliasIdFromBag( box.Bag.ScheduleCoordinatorPerson );
            } );

            // The coordinator-notifications bitmask is persisted only when the
            // override is enabled. Otherwise the column is left null so the
            // group inherits the value from its GroupType. A zero bitmask
            // represents an explicit None selection.
            box.IfValidProperty( nameof( box.Bag.ScheduleCoordinatorNotificationTypes ),
                () => entity.ScheduleCoordinatorNotificationTypes = box.Bag.HasCoordinatorNotificationOverride
                    ? box.Bag.ScheduleCoordinatorNotificationTypes
                    : ( ScheduleCoordinatorNotificationType? ) null );

            box.IfValidProperty( nameof( box.Bag.AttendanceRecordRequiredForCheckIn ),
                () => entity.AttendanceRecordRequiredForCheckIn = box.Bag.AttendanceRecordRequiredForCheckIn );

            box.IfValidProperty( nameof( box.Bag.ScheduleType ),
                () => ApplyInlineSchedule( entity, box.Bag ) );

            // Chat overrides and the avatar are only persisted when chat is
            // enabled system-wide and the group type permits it.
            box.IfValidProperty( nameof( box.Bag.IsChatEnabledOverride ), () =>
            {
                if ( IsSystemChatEnabled && groupType?.IsChatAllowed == true )
                {
                    entity.IsChatEnabledOverride = box.Bag.IsChatEnabledOverride;
                    entity.IsLeavingChatChannelAllowedOverride = box.Bag.IsLeavingChatChannelAllowedOverride;
                    entity.IsChatChannelPublicOverride = box.Bag.IsChatChannelPublicOverride;
                    entity.IsChatChannelAlwaysShownOverride = box.Bag.IsChatChannelAlwaysShownOverride;
                    entity.ChatPushNotificationModeOverride = box.Bag.ChatPushNotificationModeOverride;
                }
            } );

            box.IfValidProperty( nameof( box.Bag.ChatChannelAvatarBinaryFile ),
                () => ApplyChatChannelAvatarBinaryFile( entity, box.Bag ) );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ), () =>
            {
                entity.LoadAttributes( RockContext );
                entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: true );
            } );

            return true;
        }

        /// <summary>
        /// Performs cross-field / collection-level validation that
        /// cannot be expressed by <c>Group.IsValid</c> alone.
        /// </summary>
        /// <param name="group">The group entity (already mutated from the bag).</param>
        /// <param name="bag">The bag containing the data from the client.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the bag passes Group-specific validation, <c>false</c> otherwise.</returns>
        private bool ValidateGroup( Model.Group group, GroupBag bag, out string errorMessage )
        {
            errorMessage = null;

            if ( group == null || bag == null )
            {
                return true;
            }

            // A group type must be chosen before any other validation is
            // meaningful, so it is checked first.
            if ( group.GroupTypeId <= 0 )
            {
                errorMessage = WarningMessage.CannotBeBlank( Model.GroupType.FriendlyTypeName );
                return false;
            }

            // A saved group cannot list itself as its own parent.
            if ( group.Id != 0 && group.ParentGroupId == group.Id )
            {
                errorMessage = "Group cannot be a Parent Group of itself.";
                return false;
            }

            // The chosen group type must pass the block's allow-list and,
            // when a parent is set, the parent's AllowedChildGroupTypes
            // list. GetAllowedGroupTypes encodes both; passing a null
            // parent applies only the block-level filter.
            Model.Group parentGroup = null;
            GroupTypeCache parentGroupType = null;

            if ( group.ParentGroupId.HasValue )
            {
                var groupService = new GroupService( RockContext );
                parentGroup = group.ParentGroup ?? groupService.Get( group.ParentGroupId.Value );

                if ( parentGroup != null )
                {
                    if ( !parentGroup.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                    {
                        errorMessage = "Not authorized to use the selected parent group.";
                        return false;
                    }

                    if ( group.Id != 0 )
                    {
                        var parentAncestorIds = groupService.GetAllAncestorIds( parentGroup.Id ).ToList();
                        if ( parentAncestorIds.Contains( group.Id ) )
                        {
                            errorMessage = $"The '{System.Net.WebUtility.HtmlEncode( parentGroup.Name )}' group cannot be selected as the parent because it would create a circular reference (the selected parent is already a descendant of this group).";
                            return false;
                        }
                    }

                    parentGroupType = GroupTypeCache.Get( parentGroup.GroupTypeId );
                }
            }

            var allowedGroupTypeIds = GetAllowedGroupTypes( parentGroupType, RockContext )
                .Select( gt => gt.Id )
                .ToList();
            if ( !allowedGroupTypeIds.Contains( group.GroupTypeId ) )
            {
                var groupType = GroupTypeCache.Get( group.GroupTypeId );
                var groupTypeName = System.Net.WebUtility.HtmlEncode( groupType?.Name ?? string.Empty );

                /*
                    06/01/2026 - MSE

                    GetAllowedGroupTypes() applies two independent restrictions: this block's group type
                    settings (the include/exclude, navigation, and security-role filters) and the parent
                    group's allowed child group types. Blaming the parent group is misleading when the
                    block settings are what actually excluded the type, so re-check the block settings on
                    their own (null parent) to report the correct reason.

                    Reason: https://github.com/SparkDevNetwork/Rock/issues/6851
                */
                var blockAllowedGroupTypeIds = GetAllowedGroupTypes( null, RockContext )
                    .Select( gt => gt.Id )
                    .ToList();

                errorMessage = !blockAllowedGroupTypeIds.Contains( group.GroupTypeId )
                    ? $"Groups with a '{groupTypeName}' group type cannot be saved here because of this block's group type settings (e.g. 'Group Types: Include' / 'Group Types: Exclude')."
                    : $"The '{System.Net.WebUtility.HtmlEncode( parentGroup.Name )}' group does not allow child groups with a '{groupTypeName}' group type.";
                return false;
            }

            // Re-check EDIT authorization. The group type or parent group
            // may have changed since the block first authorized the user,
            // and the new combination may resolve to a different result.
            if ( !group.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                errorMessage = $"Not authorized to edit {Model.Group.FriendlyTypeName}.";
                return false;
            }

            // Model-level rules from Group.IsValid (e.g. GroupsRequireCampus).
            if ( !group.IsValid )
            {
                errorMessage = group.ValidationResults
                    .Select( r => r.ErrorMessage )
                    .ToList()
                    .AsDelimited( "<br />" );
                return false;
            }

            if ( !ValidateChildCollections( group, bag, out errorMessage ) )
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates the per-group child collections (requirements, syncs,
        /// workflow triggers) carried by the save bag. Catches missing
        /// required references that the inline <c>?? 0</c> coercion would
        /// otherwise convert into opaque SQL FK violations, and rejects
        /// crafted bags that attach a Group Sync role from a different
        /// group type than the active one.
        /// </summary>
        /// <param name="group">The group entity (already mutated from the bag).</param>
        /// <param name="bag">The bag containing the data from the client.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the child collections pass validation, <c>false</c> otherwise.</returns>
        private bool ValidateChildCollections( Model.Group group, GroupBag bag, out string errorMessage )
        {
            errorMessage = null;

            // Group Requirements: Requirement Type is a required FK.
            if ( bag.GroupRequirements?.Any( r => r != null && r.GroupRequirementType == null ) == true )
            {
                errorMessage = "One or more Group Requirements are missing a Requirement Type.";
                return false;
            }

            // Group Requirements: each (Requirement Type, Role) pair must be
            // unique on a single group. The UI modal blocks duplicates client
            // side; this catches crafted bags that bypass the modal.
            if ( bag.GroupRequirements?.Any() == true )
            {
                var hasDuplicate = bag.GroupRequirements
                    .Where( r => r != null && r.GroupRequirementType != null )
                    .GroupBy( r => new
                    {
                        TypeGuid = r.GroupRequirementType.Value.AsGuidOrNull() ?? Guid.Empty,
                        RoleGuid = r.Role?.Value.AsGuidOrNull() ?? Guid.Empty
                    } )
                    .Any( g => g.Count() > 1 );

                if ( hasDuplicate )
                {
                    errorMessage = "Two or more Group Requirements share the same Requirement Type and Role. Each combination may appear only once.";
                    return false;
                }
            }

            // Group Syncs: Role and Data View are required FKs.
            if ( bag.GroupSyncs?.Any( s => s != null && ( s.GroupTypeRole == null || s.SyncDataView == null ) ) == true )
            {
                errorMessage = "One or more Group Sync rules are missing a required Role or Data View.";
                return false;
            }

            // Group Syncs: the chosen Role must belong to the active group type.
            if ( bag.GroupSyncs?.Any() == true && group.GroupTypeId > 0 )
            {
                var validRoleGuids = GroupTypeCache.Get( group.GroupTypeId )?.Roles?
                    .Select( r => r.Guid )
                    .ToHashSet() ?? new HashSet<Guid>();

                var hasForeignRole = bag.GroupSyncs
                    .Where( s => s != null && s.GroupTypeRole != null )
                    .Any( s => !validRoleGuids.Contains( s.GroupTypeRole.Value.AsGuidOrNull() ?? Guid.Empty ) );

                if ( hasForeignRole )
                {
                    errorMessage = "One or more Group Sync rules reference a role that does not belong to this group's type.";
                    return false;
                }
            }

            // Member Workflow Triggers: Workflow Type is a required FK.
            if ( bag.GroupMemberWorkflowTriggers?.Any( t => t != null && t.WorkflowType == null ) == true )
            {
                errorMessage = "One or more Member Workflow Triggers are missing a Workflow Type.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Synchronizes a collection of related entities against an incoming
        /// list of bags. Entities absent from the incoming list are deleted;
        /// missing entities are created; existing entities are updated.
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
            var existingEntities = existingEntitiesQuery.ToList();
            var existingByKey = existingEntities.ToDictionary( existingKeySelector );

            var incomingList = ( incomingBags ?? Enumerable.Empty<TBag>() ).ToList();
            var incomingKeys = incomingList.Select( incomingKeySelector ).ToHashSet();

            foreach ( var entity in existingEntities.Where( e => !incomingKeys.Contains( existingKeySelector( e ) ) ).ToList() )
            {
                service.Delete( entity );
            }

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

            var bag = GetEntityBagForEdit( entity );

            var groupTypeOptions = entity.GroupTypeId > 0
                ? BuildGroupTypeOptionsBag( entity.GroupTypeId )
                : new GroupTypeOptionsBag();

            return ActionOk( new
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList(),
                GroupTypeOptions = groupTypeOptions
            } );
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( ValidPropertiesBox<GroupBag> box )
        {
            if ( box?.Bag == null )
            {
                return ActionBadRequest( "Invalid request." );
            }

            if ( !TryGetEntityForEditAction( box.Bag.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            var roleGroupTypeId = GroupTypeCache.GetId( Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid() ) ?? int.MinValue;
            var isNew = entity.Id == 0;

            // Capture pre-save state for post-save cache invalidation and
            // BinaryFile orphan tracking.
            var wasSecurityRole = !isNew
                && entity.IsActive
                && ( entity.IsSecurityRole || entity.GroupTypeId == roleGroupTypeId );
            var oldPhotoId = entity.PhotoId;
            var oldChatChannelAvatarId = entity.ChatChannelAvatarBinaryFileId;
            var oldScheduleId = entity.ScheduleId;

            // Apply scalar field assignments from the bag.
            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            if ( !ValidateGroup( entity, box.Bag, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            /*
                5/19/2026 - MSE

                Per-row EDIT auth on the inactive cascade matches the same
                gate ArchiveWithChildren applies, so the two destructive
                cascades behave consistently. Runs outside WrapTransaction
                so the parent's own mutations don't partially persist when
                the descendant check fails; the resolved descendant list is
                reused inside the transaction.

                Reason: Honor per-entity security on the inactive cascade,
                matching the archive cascade.
            */
            List<Model.Group> descendantsToInactivate = null;
            if ( !isNew && !entity.IsActive && box.Bag.InactivateChildGroups )
            {
                var groupService = new GroupService( RockContext );
                descendantsToInactivate = groupService.GetAllDescendentGroups( entity.Id, includeInactiveChildGroups: false );

                var unauthorizedCount = descendantsToInactivate.Count( d => !d.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) );
                if ( unauthorizedCount > 0 )
                {
                    return ActionBadRequest( $"Not authorized to inactivate {unauthorizedCount} of {descendantsToInactivate.Count} sub-{Model.Group.FriendlyTypeName.Pluralize().ToLower()}. No groups were inactivated." );
                }
            }

            var addAdministrateSecurity = isNew
                && GetAttributeValue( AttributeKey.AddAdministrateSecurityToGroupCreator ).AsBoolean();
            var triggersUpdated = false;
            var checkinDataUpdated = false;

            RockContext.WrapTransaction( () =>
            {
                // Persist the group itself first so Id is assigned and the
                // collection syncs below can reference it.
                RockContext.SaveChanges();

                // Grant ADMINISTRATE to the creator on Add when the block
                // is configured to do so.
                if ( addAdministrateSecurity )
                {
                    Authorization.AllowPerson( entity, Authorization.ADMINISTRATE, RequestContext.CurrentPerson, RockContext );
                }

                // Persist group attribute values and sync the per-group
                // child collections. Each helper performs its own diff
                // (add / update / delete) against the incoming bag.
                entity.SaveAttributeValues( RockContext );

                var canAdministrate = entity.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson );

                if ( canAdministrate )
                {
                    box.IfValidProperty( nameof( box.Bag.GroupMemberAttributes ),
                        () => SaveGroupMemberAttributes( "GroupId", entity.Id.ToString(), box.Bag.GroupMemberAttributes ) );

                    box.IfValidProperty( nameof( box.Bag.GroupRequirements ),
                        () => SaveGroupRequirements( entity, box.Bag.GroupRequirements ) );

                    box.IfValidProperty( nameof( box.Bag.GroupSyncs ),
                        () => SaveGroupSyncs( entity, box.Bag.GroupSyncs ) );
                }

                // Workflow triggers and group locations both report whether
                // any change occurred so the post-transaction cache
                // invalidations only fire when necessary.
                if ( box.IfValidProperty( nameof( box.Bag.GroupMemberWorkflowTriggers ),
                    () => SaveGroupMemberWorkflowTriggers( entity, box.Bag.GroupMemberWorkflowTriggers ),
                    false ) )
                {
                    triggersUpdated = true;
                }

                if ( box.IfValidProperty( nameof( box.Bag.GroupLocations ),
                    () => SaveGroupLocations( entity, box.Bag.GroupLocations ),
                    false ) )
                {
                    checkinDataUpdated = true;
                }

                // Apply the inactive cascade. Authorization for each
                // descendant was verified above; mutations here ride the
                // same RockContext so they participate in the transaction.
                if ( descendantsToInactivate != null )
                {
                    foreach ( var descendant in descendantsToInactivate )
                    {
                        if ( descendant.IsActive )
                        {
                            descendant.IsActive = false;
                            descendant.InactiveReasonValueId = box.Bag.InactiveReasonValueId;
                            descendant.InactiveReasonNote = "Parent Deactivated";
                            if ( box.Bag.InactiveReasonNote.IsNotNullOrWhiteSpace() )
                            {
                                descendant.InactiveReasonNote += ": " + box.Bag.InactiveReasonNote;
                            }
                        }
                    }
                }

                // Toggle the IsTemporary flag on the chat-avatar and photo
                // BinaryFiles so newly-attached files are retained and
                // orphaned files become eligible for cleanup.
                ToggleBinaryFileIsTemporary( oldChatChannelAvatarId, entity.ChatChannelAvatarBinaryFileId );
                ToggleBinaryFileIsTemporary( oldPhotoId, entity.PhotoId );

                // Delete the prior inline schedule when it has been replaced
                // or cleared. Named schedules are preserved.
                if ( oldScheduleId.HasValue && oldScheduleId.Value != ( entity.ScheduleId ?? 0 ) )
                {
                    DeleteInlineSchedule( oldScheduleId.Value );
                }

                RockContext.SaveChanges();
            } );

            /*
                5/26/2026 - MSE

                Clear the authorization cache when the group's effective
                security-role status flips. Downstream IsAuthorized checks
                read from the cache, so a flip without an invalidation would
                leave them seeing the previous role state until the cache
                refreshes on its own schedule.

                Reason: Keep auth-cache state coherent with security-role
                transitions.
            */
            var isNowSecurityRole = entity.IsActive && ( entity.IsSecurityRole || entity.GroupTypeId == roleGroupTypeId );
            if ( wasSecurityRole != isNowSecurityRole )
            {
                Authorization.Clear();
            }

            // Invalidate the workflow-trigger registry when triggers were
            // added, updated, or removed.
            if ( triggersUpdated )
            {
                GroupMemberWorkflowTriggerService.RemoveCachedTriggers();
            }

            /*
                5/26/2026 - MSE

                Invalidate the KioskDevice cache only when both conditions
                hold: group locations changed AND the group type takes
                attendance. Other group types do not feed the check-in kiosk
                surface, so flushing the cache for their changes would be
                pure waste.

                Reason: Scope KioskDevice invalidation to attendance-taking
                group types.
            */
            if ( checkinDataUpdated )
            {
                var groupTypeCacheForKiosk = GetGroupTypeCache( entity );
                if ( groupTypeCacheForKiosk?.TakesAttendance == true )
                {
                    Rock.CheckIn.KioskDevice.Clear();
                }
            }

            var saveReturnUrl = PageParameter( PageParameterKey.ReturnUrl );
            if ( saveReturnUrl.IsNotNullOrWhiteSpace() && IsSafeReturnUrl( saveReturnUrl ) )
            {
                return ActionContent( System.Net.HttpStatusCode.OK, saveReturnUrl );
            }

            if ( isNew )
            {
                // Preserve ExpandedIds across the post-Add reload so the
                // tree-navigation context the user came in with is kept.
                var qryParams = new Dictionary<string, string>
                {
                    [PageParameterKey.GroupId] = entity.IdKey
                };

                var expandedIds = PageParameter( PageParameterKey.ExpandedIds );
                if ( expandedIds.IsNotNullOrWhiteSpace() )
                {
                    qryParams[PageParameterKey.ExpandedIds] = expandedIds;
                }

                var redirectUrl = this.GetCurrentPageUrl( qryParams );
                return ActionContent( System.Net.HttpStatusCode.Created, redirectUrl );
            }

            var parentGroupId = entity.ParentGroupId;
            entity = new GroupService( RockContext ).Get( entity.Id );

            var refreshedBag = GetEntityBagForView( entity );

            if ( refreshedBag == null )
            {
                return ActionContent( System.Net.HttpStatusCode.Created, NavigateAfterDeleteOrArchive( parentGroupId ) );
            }

            return ActionOk( new ValidPropertiesBox<GroupBag>
            {
                Bag = refreshedBag,
                ValidProperties = refreshedBag.GetType().GetProperties().Select( p => p.Name ).ToList()
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
            var groupService = new GroupService( RockContext );
            var entity = groupService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{Model.Group.FriendlyTypeName} not found." );
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete {Model.Group.FriendlyTypeName}." );
            }

            if ( !groupService.CanDelete( entity, out var errorMessage, includeSecondLvl: true ) )
            {
                return ActionBadRequest( errorMessage );
            }

            var parentGroupId = entity.ParentGroupId;

            // Delete any inline (non-named) schedule attached to the group
            // when no other group references it. Named schedules are
            // shared resources and are left in place.
            if ( entity.ScheduleId.HasValue )
            {
                var scheduleService = new ScheduleService( RockContext );
                var schedule = scheduleService.Get( entity.ScheduleId.Value );
                if ( schedule != null && schedule.ScheduleType != ScheduleType.Named )
                {
                    var isReferencedByOtherGroup = groupService.Queryable()
                        .Any( g => g.ScheduleId == schedule.Id && g.Id != entity.Id );
                    if ( !isReferencedByOtherGroup )
                    {
                        scheduleService.Delete( schedule );
                    }
                }
            }

            // Security-role groups require the dedicated cleanup path so
            // related Auth rows are removed and the global authorization
            // cache is invalidated. Other groups use the standard delete
            // flow.
            if ( entity.IsSecurityRoleOrSecurityGroupType() )
            {
                GroupService.DeleteSecurityRoleGroup( entity.Id );
            }
            else
            {
                groupService.Delete( entity );
            }

            RockContext.SaveChanges();

            return ActionOk( NavigateAfterDeleteOrArchive( parentGroupId ) );
        }

        /// <summary>
        /// Archives the specified group only. Descendant groups are not
        /// affected. The Vue layer reads <see cref="GroupBag.HasChildGroups"/>
        /// and prompts the user before deciding whether to invoke this
        /// action or <see cref="ArchiveWithChildren(string)"/>.
        /// </summary>
        [BlockAction]
        public BlockActionResult Archive( string key )
        {
            var groupService = new GroupService( RockContext );
            var entity = groupService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{Model.Group.FriendlyTypeName} not found." );
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to archive {Model.Group.FriendlyTypeName}." );
            }

            var personAliasId = RequestContext.CurrentPerson?.PrimaryAliasId;

            groupService.Archive( entity, personAliasId, true );
            RockContext.SaveChanges();

            return ActionOk( NavigateAfterDeleteOrArchive( entity.ParentGroupId ) );
        }

        /// <summary>
        /// Archives the specified group and every descendant returned by
        /// <c>GetAllDescendentGroups</c>. The cascade enforces EDIT
        /// authorization on each descendant before any group is archived.
        /// </summary>
        [BlockAction]
        public BlockActionResult ArchiveWithChildren( string key )
        {
            var groupService = new GroupService( RockContext );
            var entity = groupService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{Model.Group.FriendlyTypeName} not found." );
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to archive {Model.Group.FriendlyTypeName}." );
            }

            var personAliasId = RequestContext.CurrentPerson?.PrimaryAliasId;
            var descendantGroups = groupService.GetAllDescendentGroups( entity.Id, true ).ToList();

            /*
                5/19/2026 - MSE

                Check EDIT on every descendant before archiving so we honor each
                group's own security, not just the parent's.

                Reason: Honor per-entity security on archive cascade.
            */
            var unauthorizedCount = descendantGroups.Count( g => !g.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) );

            if ( unauthorizedCount > 0 )
            {
                return ActionBadRequest( $"Not authorized to archive {unauthorizedCount} of {descendantGroups.Count} sub-{Model.Group.FriendlyTypeName.Pluralize().ToLower()}. No groups were archived." );
            }

            foreach ( var descendant in descendantGroups )
            {
                groupService.Archive( descendant, personAliasId, true );
            }

            groupService.Archive( entity, personAliasId, true );
            RockContext.SaveChanges();

            return ActionOk( NavigateAfterDeleteOrArchive( entity.ParentGroupId ) );
        }

        /// <summary>
        /// Copies the specified group via <see cref="GroupService.CopyGroup"/>.
        /// The Vue modal defaults <c>IncludeChildGroups</c> to <c>false</c>,
        /// so child groups are only included when the user explicitly opts in.
        /// </summary>
        [BlockAction]
        public BlockActionResult Copy( CopyGroupRequestBag bag )
        {
            if ( bag == null || bag.Key.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Invalid request." );
            }

            var groupService = new GroupService( RockContext );
            var entity = groupService.Get( bag.Key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{Model.Group.FriendlyTypeName} not found." );
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to copy {Model.Group.FriendlyTypeName}." );
            }

            var copyOptions = new CopyGroupOptions
            {
                GroupId = entity.Id,
                IncludeChildGroups = bag.IncludeChildGroups,
                CreatedByPersonAliasId = RequestContext.CurrentPerson?.PrimaryAliasId
            };

            var newGroupId = GroupService.CopyGroup( copyOptions );

            // Copy navigation always reloads to the new group and does
            // not honor ?returnUrl=.
            var newGroupKey = newGroupId.HasValue && newGroupId.Value > 0
                ? Rock.Utility.IdHasher.Instance.GetHash( newGroupId.Value )
                : entity.IdKey;

            var qryParams = new Dictionary<string, string>
            {
                [PageParameterKey.GroupId] = newGroupKey
            };

            var expandedIds = PageParameter( PageParameterKey.ExpandedIds );
            if ( expandedIds.IsNotNullOrWhiteSpace() )
            {
                qryParams[PageParameterKey.ExpandedIds] = expandedIds;
            }

            return ActionOk( this.GetCurrentPageUrl( qryParams ) );
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

            if ( entityKey == EntityKey.GroupRequirement )
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
            else if ( entityKey == EntityKey.GroupSync )
            {
                var service = new GroupSyncService( RockContext );
                var entity = service.Get( request.EntityGuid );
                if ( entity == null )
                {
                    return ActionOk( new CanDeleteResponseBag { CanDelete = true } );
                }

                canDelete = service.CanDelete( entity, out errorMessage );
            }
            else if ( entityKey == EntityKey.GroupLocation )
            {
                var service = new GroupLocationService( RockContext );
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
        /// Returns the per-group-type options payload for the selected
        /// group type.
        /// </summary>
        [BlockAction]
        public BlockActionResult GetGroupTypeOptions( int groupTypeId )
        {
            // Re-check EDIT authorization on the entity. On the Add path
            // the entity is fresh and inherits page-level authorization.
            var entity = GetInitialEntity();
            if ( entity == null )
            {
                return ActionBadRequest( $"{Model.Group.FriendlyTypeName} not found." );
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to edit {Model.Group.FriendlyTypeName}." );
            }

            if ( groupTypeId <= 0 )
            {
                return ActionBadRequest( "Group Type is required." );
            }

            var groupType = GroupTypeCache.Get( groupTypeId );
            if ( groupType == null )
            {
                return ActionBadRequest( "Group Type not found." );
            }

            return ActionOk( BuildGroupTypeOptionsBag( groupTypeId ) );
        }

        /// <summary>
        /// Returns the allowed child group types for the supplied parent
        /// group key, or the unfiltered allowed list when the key is empty.
        /// Used in Add mode to re-filter the group-type dropdown whenever
        /// the parent selection changes.
        /// </summary>
        [BlockAction]
        public BlockActionResult GetAllowedChildGroupTypes( string parentGroupKey )
        {
            Model.Group parentGroup = null;

            if ( parentGroupKey.IsNotNullOrWhiteSpace() )
            {
                parentGroup = new GroupService( RockContext ).Get( parentGroupKey, !PageCache.Layout.Site.DisablePredictableIds );

                if ( parentGroup == null || !parentGroup.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( "Not authorized to view the selected parent group." );
                }
            }

            var allowedGroupTypes = BuildAllowedGroupTypeListItems( parentGroup );

            return ActionOk( new AllowedGroupTypesBag
            {
                Items = allowedGroupTypes,
                Warning = GetEmptyAllowedGroupTypesWarning( parentGroup, allowedGroupTypes.Any() )
            } );
        }

        /// <summary>
        /// Returns whether the supplied parent group is active. Drives the
        /// inactive-parent warning banner in the edit panel.
        /// </summary>
        [BlockAction]
        public BlockActionResult GetParentGroupInfo( string parentGroupKey )
        {
            if ( parentGroupKey.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Parent group key is required." );
            }

            var parentGroup = new GroupService( RockContext ).Get( parentGroupKey, !PageCache.Layout.Site.DisablePredictableIds );

            if ( parentGroup == null || !parentGroup.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( "Not authorized to view the selected parent group." );
            }

            return ActionOk( parentGroup.IsActive );
        }

        /// <summary>
        /// Returns the dropdown sources for the Group Requirement modal:
        /// the GroupRequirementType list and the date-typed group
        /// attributes inherited from the supplied group type.
        /// </summary>
        /// <param name="groupTypeId">The currently-selected group type, used to scope the date attribute walk.</param>
        [BlockAction]
        public BlockActionResult GetGroupRequirementOptions( int? groupTypeId )
        {
            var groupType = groupTypeId.HasValue && groupTypeId.Value > 0
                ? GroupTypeCache.Get( groupTypeId.Value )
                : null;

            return ActionOk( new GroupRequirementOptionsBag
            {
                GroupRequirementTypes = BuildGroupRequirementTypeOptions(),
                GroupAttributes = BuildGroupDateAttributeOptions( groupType )
            } );
        }

        /// <summary>
        /// Returns the SystemCommunication dropdown options for the Group
        /// Sync modal's Welcome and Exit communication pickers.
        /// </summary>
        [BlockAction]
        public BlockActionResult GetGroupSyncOptions()
        {
            return ActionOk( BuildSystemCommunicationOptions() );
        }

        /// <summary>
        /// Returns the Family Member dropdown options for the Group
        /// Location modal.
        /// </summary>
        /// <param name="key">The IdKey of the group.</param>
        [BlockAction]
        public BlockActionResult GetFamilyMemberLocationOptions( string key )
        {
            if ( !TryGetEntityForEditAction( key, out var entity, out var error ) )
            {
                return error;
            }

            return ActionOk( BuildFamilyMemberLocationOptions( entity ) );
        }

        #endregion Block Actions

        #region Helper Methods

        #region Lookups & Authorization

        /// <summary>
        /// Returns the <see cref="GroupTypeCache"/> for the entity, or null
        /// when no group type is set. Memoized per-request; re-resolves if
        /// the entity's <c>GroupTypeId</c> changes.
        /// </summary>
        private GroupTypeCache GetGroupTypeCache( Model.Group entity )
        {
            if ( entity == null || entity.GroupTypeId <= 0 )
            {
                return null;
            }

            if ( _cachedGroupType?.Id == entity.GroupTypeId )
            {
                return _cachedGroupType;
            }

            _cachedGroupType = GroupTypeCache.Get( entity.GroupTypeId );
            return _cachedGroupType;
        }

        /// <summary>
        /// Gets a value indicating whether the current user is a member of
        /// the GROUP_ADMINISTRATORS system group. Memoized per request via
        /// <see cref="_isCurrentPersonGroupAdministrator"/>.
        /// </summary>
        private bool IsCurrentPersonGroupAdministrator()
        {
            if ( _isCurrentPersonGroupAdministrator.HasValue )
            {
                return _isCurrentPersonGroupAdministrator.Value;
            }

            var currentPersonId = RequestContext.CurrentPerson?.Id;
            if ( !currentPersonId.HasValue )
            {
                _isCurrentPersonGroupAdministrator = false;
                return false;
            }

            _isCurrentPersonGroupAdministrator = new GroupService( RockContext ).GroupHasMember(
                Rock.SystemGuid.Group.GROUP_ADMINISTRATORS.AsGuid(),
                currentPersonId.Value );

            return _isCurrentPersonGroupAdministrator.Value;
        }

        /// <summary>
        /// Gets the effective relationship strength for the group, falling
        /// back to the group type default when no override is set.
        /// </summary>
        private static RelationshipStrength? GetRelationshipStrength( Model.Group entity, GroupTypeCache groupType )
        {
            return groupType?.IsPeerNetworkEnabled == true
                ? ( RelationshipStrength? )( entity.RelationshipStrengthOverride ?? groupType.RelationshipStrength )
                : null;
        }

        /// <summary>
        /// Gets a value indicating whether the supplied group type is the
        /// Fundraising Opportunity group type or inherits from it.
        /// </summary>
        private static bool IsFundraisingGroupType( int groupTypeId )
        {
            var fundraisingGroupTypeId = GroupTypeCache.GetId( Rock.SystemGuid.GroupType.GROUPTYPE_FUNDRAISINGOPPORTUNITY.AsGuid() );
            if ( !fundraisingGroupTypeId.HasValue )
            {
                return false;
            }

            var groupType = GroupTypeCache.Get( groupTypeId );
            if ( groupType == null )
            {
                return false;
            }

            return groupType.Id == fundraisingGroupTypeId.Value
                || groupType.InheritedGroupTypeId == fundraisingGroupTypeId.Value;
        }

        /// <summary>
        /// Returns the <see cref="GroupType"/> queryable filtered by the
        /// block's <c>GroupTypes</c> / <c>GroupTypesExclude</c> attributes,
        /// the parent group's allowed child group types, the
        /// <c>LimittoSecurityRoleGroups</c> attribute, and the
        /// <c>LimitToShowInNavigationGroupTypes</c> attribute.
        /// </summary>
        private IQueryable<Model.GroupType> GetAllowedGroupTypes( GroupTypeCache parentGroupGroupType, RockContext rockContext )
        {
            var groupTypeService = new GroupTypeService( rockContext );
            var groupTypeQry = groupTypeService.Queryable();

            // Block attribute include/exclude.
            var includeGuids = GetAttributeValue( AttributeKey.GroupTypes ).SplitDelimitedValues().AsGuidList();
            var excludeGuids = GetAttributeValue( AttributeKey.GroupTypesExclude ).SplitDelimitedValues().AsGuidList();
            if ( includeGuids.Any() )
            {
                groupTypeQry = groupTypeQry.Where( a => includeGuids.Contains( a.Guid ) );
            }
            else if ( excludeGuids.Any() )
            {
                groupTypeQry = groupTypeQry.Where( a => !excludeGuids.Contains( a.Guid ) );
            }

            // Parent group type's allowed child group types.
            if ( parentGroupGroupType != null && !parentGroupGroupType.AllowAnyChildGroupType )
            {
                var allowedChildGroupTypeIds = parentGroupGroupType.ChildGroupTypes.Select( a => a.Id ).ToList();
                groupTypeQry = groupTypeQry.Where( a => allowedChildGroupTypeIds.Contains( a.Id ) );
            }

            // LimitToShowInNavigationGroupTypes.
            if ( GetAttributeValue( AttributeKey.LimitToShowInNavigationGroupTypes ).AsBoolean() )
            {
                groupTypeQry = groupTypeQry.Where( a => a.ShowInNavigation );
            }

            // LimittoSecurityRoleGroups.
            if ( GetAttributeValue( AttributeKey.LimittoSecurityRoleGroups ).AsBoolean() )
            {
                var securityRoleGroupTypeId = GroupTypeCache.GetId( Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid() );
                if ( securityRoleGroupTypeId.HasValue )
                {
                    groupTypeQry = groupTypeQry.Where( a => a.Id == securityRoleGroupTypeId.Value );
                }
            }

            return groupTypeQry;
        }

        #endregion Lookups & Authorization

        #region ListItemBag Conveniences

        /// <summary>
        /// Builds a <see cref="ListItemBag"/> for the
        /// <c>&lt;CampusPicker&gt;</c> from a campus Id.
        /// </summary>
        private static ListItemBag BuildCampusListItem( int? campusId )
        {
            return campusId.HasValue
                ? CampusCache.Get( campusId.Value )?.ToListItemBag()
                : null;
        }

        /// <summary>
        /// Builds a <see cref="ListItemBag"/> for a
        /// <c>&lt;DefinedValuePicker&gt;</c> from a defined value Id.
        /// </summary>
        private static ListItemBag BuildDefinedValueListItem( int? definedValueId )
        {
            return definedValueId.HasValue
                ? DefinedValueCache.Get( definedValueId.Value )?.ToListItemBag()
                : null;
        }

        /// <summary>
        /// Builds a <see cref="ListItemBag"/> for an
        /// <c>&lt;ImageUploader&gt;</c>-bound BinaryFile, using the loaded
        /// navigation property when available and falling back to a
        /// service lookup by Id.
        /// </summary>
        private ListItemBag BuildBinaryFileRef( BinaryFile attachedFile, int? binaryFileId )
        {
            if ( attachedFile != null )
            {
                return new ListItemBag
                {
                    Value = attachedFile.Guid.ToString(),
                    Text = attachedFile.FileName
                };
            }

            if ( !binaryFileId.HasValue )
            {
                return null;
            }

            var file = new BinaryFileService( RockContext ).GetSelect( binaryFileId.Value, bf => new { bf.Guid, bf.FileName } );
            if ( file == null )
            {
                return null;
            }

            return new ListItemBag
            {
                Value = file.Guid.ToString(),
                Text = file.FileName
            };
        }

        /// <summary>
        /// Builds a <see cref="ListItemBag"/> for a single SystemCommunication
        /// by Id.
        /// </summary>
        /// <param name="systemCommunicationId">The system communication Id.</param>
        private ListItemBag BuildSystemCommunicationListItem( int? systemCommunicationId )
        {
            if ( !systemCommunicationId.HasValue )
            {
                return null;
            }

            return new SystemCommunicationService( RockContext )
                .Queryable()
                .Where( c => c.Id == systemCommunicationId.Value )
                .Select( c => new ListItemBag { Value = c.Guid.ToString(), Text = c.Title } )
                .FirstOrDefault();
        }

        #endregion ListItemBag Conveniences

        #region Header & Overview Refs

        /// <summary>
        /// Builds the administrator reference for the Overview card.
        /// Returns null when no administrator is set or the group type
        /// hides the row via <c>ShowAdministrator</c>. The URL honors any
        /// customer-customized Person <c>LinkUrlLavaTemplate</c>.
        /// </summary>
        private static GroupAdministratorBag BuildAdministratorRef( PersonAlias personAlias, GroupTypeCache groupType )
        {
            var person = personAlias?.Person;

            if ( person == null || groupType == null || !groupType.ShowAdministrator )
            {
                return null;
            }

            return new GroupAdministratorBag
            {
                Value = personAlias.Guid.ToString(),
                Text = person.FullName,
                Url = ResolveEntityUrl( typeof( Person ), person, fallbackUrl: $"/Person/{person.IdKey}" )
            };
        }

        /// <summary>
        /// Builds the administrator field label for the Overview card by
        /// combining the group type's group term and administrator term
        /// (e.g. "Group Administrator", "Group Director"). Mirrors the edit
        /// panel's label composition so view and edit modes stay consistent.
        /// Each term falls back to its default when blank.
        /// </summary>
        private static string BuildAdministratorLabel( GroupTypeCache groupType )
        {
            var groupTerm = groupType?.GroupTerm.IsNotNullOrWhiteSpace() == true ? groupType.GroupTerm : "Group";
            var administratorTerm = groupType?.AdministratorTerm.IsNotNullOrWhiteSpace() == true ? groupType.AdministratorTerm : "Administrator";

            return $"{groupTerm} {administratorTerm}";
        }

        /// <summary>
        /// Builds the parent group reference for the Overview card, or
        /// null when the group has no parent. The URL honors any
        /// customer-customized Group <c>LinkUrlLavaTemplate</c>.
        /// </summary>
        private static ParentGroupBag BuildParentGroupRef( Model.Group parentGroup )
        {
            if ( parentGroup == null )
            {
                return null;
            }

            return new ParentGroupBag
            {
                Value = parentGroup.Guid.ToString(),
                Text = parentGroup.Name,
                Url = ResolveEntityUrl( typeof( Model.Group ), parentGroup, fallbackUrl: $"/Group/{parentGroup.IdKey}" ),
                IsActive = parentGroup.IsActive
            };
        }

        /// <summary>
        /// Builds the group type reference for the panel-header chip.
        /// <c>Url</c> is only populated when the current user has
        /// ADMINISTRATE on the group type; otherwise the chip renders
        /// as plain text.
        /// </summary>
        private GroupDetailGroupTypeBag BuildGroupTypeRef( GroupTypeCache groupType )
        {
            if ( groupType == null )
            {
                return null;
            }

            var canAdministrate = groupType.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson );

            string url = null;
            if ( canAdministrate )
            {
                var idKey = Rock.Utility.IdHasher.Instance.GetHash( groupType.Id );
                var resolved = ResolveEntityUrl(
                    typeof( Model.GroupType ),
                    groupType,
                    fallbackUrl: $"/admin/general/group-types/{idKey}" );

                if ( resolved.IsNotNullOrWhiteSpace() )
                {
                    var separator = resolved.Contains( "?" ) ? "&" : "?";
                    var returnUrl = RequestContext?.RequestUri?.PathAndQuery;
                    url = returnUrl.IsNotNullOrWhiteSpace()
                        ? $"{resolved}{separator}{PageParameterKey.AutoEdit}=true&{PageParameterKey.ReturnUrl}={Uri.EscapeDataString( returnUrl )}"
                        : $"{resolved}{separator}{PageParameterKey.AutoEdit}=true";
                }
            }

            return new GroupDetailGroupTypeBag
            {
                Name = groupType.Name,
                Url = url,
                Color = groupType.GroupTypeColor,
                IconCssClass = groupType.IconCssClass
            };
        }

        /// <summary>
        /// Resolves a detail URL for the supplied entity, preferring the
        /// entity type's <c>LinkUrlLavaTemplate</c> with
        /// <paramref name="lavaEntity"/> as the <c>Entity</c> merge field
        /// and falling back to <paramref name="fallbackUrl"/>.
        /// </summary>
        private static string ResolveEntityUrl( System.Type entityType, object lavaEntity, string fallbackUrl )
        {
            if ( lavaEntity == null )
            {
                return null;
            }

            var entityTypeCache = EntityTypeCache.Get( entityType );
            if ( !string.IsNullOrWhiteSpace( entityTypeCache?.LinkUrlLavaTemplate ) )
            {
                var mergeFields = new Dictionary<string, object>
                {
                    ["Entity"] = lavaEntity
                };

                var url = entityTypeCache.LinkUrlLavaTemplate.ResolveMergeFields( mergeFields );
                if ( !string.IsNullOrWhiteSpace( url ) )
                {
                    if ( url.StartsWith( "~/" ) )
                    {
                        var baseUrl = GlobalAttributesCache.Value( "InternalApplicationRoot" );
                        url = url.Replace( "~/", baseUrl.EnsureTrailingForwardslash() );
                    }
                    return url;
                }
            }

            return fallbackUrl;
        }

        #endregion Header & Overview Refs

        #region Generic Utilities

        /// <summary>
        /// Parses an ISO-8601 time-of-day string (e.g., <c>"13:30:00"</c>)
        /// into a <see cref="TimeSpan"/>, or null on parse failure.
        /// </summary>
        private static TimeSpan? ParseTimeSpanOrNull( string isoTime )
        {
            if ( isoTime.IsNullOrWhiteSpace() )
            {
                return null;
            }

            return TimeSpan.TryParse( isoTime, out var ts ) ? ts : ( TimeSpan? ) null;
        }

        /// <summary>
        /// Resolves a PersonPicker <see cref="ListItemBag"/> (whose
        /// <c>Value</c> carries the <c>PersonAlias.Guid</c>) to the
        /// underlying <c>PersonAlias.Id</c>.
        /// </summary>
        /// <param name="bag">The bag emitted by a PersonPicker control.</param>
        private int? ResolvePersonAliasIdFromBag( ListItemBag bag )
        {
            var aliasGuid = bag?.Value.AsGuidOrNull();
            if ( !aliasGuid.HasValue )
            {
                return null;
            }

            return new PersonAliasService( RockContext ).GetSelect( aliasGuid.Value, pa => ( int? ) pa.Id );
        }

        /// <summary>
        /// Validates that a candidate <c>returnUrl</c> is same-origin
        /// before any redirect honors it. Relative paths are accepted;
        /// absolute URLs are accepted only when their host matches the
        /// current request host.
        /// </summary>
        private bool IsSafeReturnUrl( string url )
        {
            if ( url.IsNullOrWhiteSpace() )
            {
                return true;
            }

            if ( !Uri.TryCreate( url, UriKind.RelativeOrAbsolute, out var parsedUri ) )
            {
                return false;
            }

            if ( !parsedUri.IsAbsoluteUri )
            {
                // Reject protocol-relative ("//attacker.example/...") URLs
                // that some parsers treat as relative.
                return !url.StartsWith( "//", StringComparison.Ordinal );
            }

            var requestHost = RequestContext?.RequestUri?.Host;
            return requestHost.IsNotNullOrWhiteSpace()
                && string.Equals( parsedUri.Host, requestHost, StringComparison.OrdinalIgnoreCase );
        }

        /// <summary>
        /// Computes the post-Delete / post-Archive redirect URL. Honors a
        /// same-origin <c>returnUrl</c> first; otherwise resolves
        /// <c>GroupListPage</c> with the parent group's IdKey and the
        /// current <c>ExpandedIds</c>; otherwise reloads the current page.
        /// </summary>
        private string NavigateAfterDeleteOrArchive( int? parentGroupId )
        {
            var returnUrl = PageParameter( PageParameterKey.ReturnUrl );
            if ( returnUrl.IsNotNullOrWhiteSpace() && IsSafeReturnUrl( returnUrl ) )
            {
                return returnUrl;
            }

            var qryParams = new Dictionary<string, string>();
            if ( parentGroupId.HasValue && parentGroupId.Value > 0 )
            {
                qryParams[PageParameterKey.GroupId] = Rock.Utility.IdHasher.Instance.GetHash( parentGroupId.Value );
            }

            var expandedIds = PageParameter( PageParameterKey.ExpandedIds );
            if ( expandedIds.IsNotNullOrWhiteSpace() )
            {
                qryParams[PageParameterKey.ExpandedIds] = expandedIds;
            }

            if ( GetAttributeValue( AttributeKey.GroupListPage ).AsGuid() != Guid.Empty )
            {
                return this.GetLinkedPageUrl( AttributeKey.GroupListPage, qryParams );
            }

            return this.GetCurrentPageUrl( qryParams );
        }

        /// <summary>
        /// Computes the URL used when the user cancels out of Add mode, mirroring
        /// the legacy WebForms btnCancel_Click behavior: honor a same-origin
        /// <c>returnUrl</c> first; otherwise, when arriving from the tree view
        /// (<c>ParentGroupId</c> present), return to the parent group preserving
        /// <c>ExpandedIds</c>; otherwise fall back to <c>GroupListPage</c> or a
        /// cleared current page. Computed server-side because both route-aware
        /// URL building and returnUrl validation require page context the Vue
        /// layer does not have.
        /// </summary>
        /// <returns>The cancel destination URL, or an empty string when none applies.</returns>
        private string GetAddModeCancelUrl()
        {
            var returnUrl = PageParameter( PageParameterKey.ReturnUrl );
            if ( returnUrl.IsNotNullOrWhiteSpace() && IsSafeReturnUrl( returnUrl ) )
            {
                return returnUrl;
            }

            // A present ParentGroupId means the user arrived from the tree view,
            // so return them to the parent group (preserving the expanded tree
            // state) rather than the list page. A root-level add (ParentGroupId 0
            // or an unresolvable empty parent) clears the selection instead of
            // selecting a group.
            //
            // ParentGroupId may be a raw integer (legacy WebForms tree), an IdKey
            // (Obsidian Group Tree View), or a Guid — resolve the same way Add
            // mode pre-populates the parent.
            var parentGroupParam = PageParameter( PageParameterKey.ParentGroupId );
            if ( parentGroupParam.IsNotNullOrWhiteSpace() )
            {
                var qryParams = new Dictionary<string, string>();

                // "0" is the explicit root-level add marker (no parent to select).
                if ( parentGroupParam != "0" )
                {
                    var parentGroup = new GroupService( RockContext )
                        .Get( parentGroupParam, !PageCache.Layout.Site.DisablePredictableIds );

                    if ( parentGroup != null )
                    {
                        qryParams[PageParameterKey.GroupId] = parentGroup.IdKey;
                    }
                }

                var expandedIds = PageParameter( PageParameterKey.ExpandedIds );
                if ( expandedIds.IsNotNullOrWhiteSpace() )
                {
                    qryParams[PageParameterKey.ExpandedIds] = expandedIds;
                }

                // skipExistingParameters drops the stale GroupId=0 from the Add
                // URL so the cancel doesn't reload straight back into Add mode.
                return this.GetCurrentPageUrl( qryParams, skipExistingParameters: true );
            }

            // No tree context: prefer the configured Group List page, otherwise
            // reload the current page with no group selected.
            if ( GetAttributeValue( AttributeKey.GroupListPage ).AsGuid() != Guid.Empty )
            {
                return this.GetLinkedPageUrl( AttributeKey.GroupListPage );
            }

            return this.GetCurrentPageUrl( new Dictionary<string, string>(), skipExistingParameters: true );
        }

        #endregion Generic Utilities

        #region Save-Flow Helpers

        /// <summary>
        /// Hydrates the bag's inline-schedule fields from the entity's
        /// <c>Schedule</c> navigation. Inline schedules (those with an
        /// empty Name) surface as Weekly or Custom according to the
        /// Schedule's own type; named schedules surface their Id; a null
        /// schedule surfaces as <see cref="ScheduleType.None"/>.
        /// </summary>
        private static void HydrateScheduleFields( GroupBag bag, Model.Group entity )
        {
            bag.ScheduleType = ScheduleType.None;
            bag.WeeklyDayOfWeek = null;
            bag.WeeklyTimeOfDay = null;
            bag.ICalendarContent = null;
            bag.NamedSchedule = null;

            var schedule = entity.Schedule;
            if ( schedule == null )
            {
                return;
            }

            switch ( schedule.ScheduleType )
            {
                case ScheduleType.Named:
                    bag.ScheduleType = ScheduleType.Named;
                    bag.NamedSchedule = schedule.ToListItemBag();
                    break;

                case ScheduleType.Custom:
                    bag.ScheduleType = ScheduleType.Custom;
                    bag.ICalendarContent = schedule.iCalendarContent;
                    break;

                case ScheduleType.Weekly:
                    bag.ScheduleType = ScheduleType.Weekly;
                    bag.WeeklyDayOfWeek = ( int? ) schedule.WeeklyDayOfWeek;
                    bag.WeeklyTimeOfDay = schedule.WeeklyTimeOfDay?.ToString();
                    break;
            }
        }

        /// <summary>
        /// Applies the photo BinaryFile change from the bag to the entity.
        /// </summary>
        private void ApplyPhotoBinaryFile( Model.Group entity, GroupBag bag )
        {
            var newGuid = bag.PhotoBinaryFile?.Value.AsGuidOrNull();
            entity.PhotoId = newGuid.HasValue
                ? new BinaryFileService( RockContext ).GetSelect( newGuid.Value, bf => ( int? ) bf.Id )
                : null;
        }

        /// <summary>
        /// Applies the chat-channel-avatar BinaryFile change to the entity.
        /// The assignment is gated on chat being enabled system-wide and
        /// on the active group type permitting chat.
        /// </summary>
        private void ApplyChatChannelAvatarBinaryFile( Model.Group entity, GroupBag bag )
        {
            var groupType = GetGroupTypeCache( entity );
            if ( !IsSystemChatEnabled || groupType?.IsChatAllowed != true )
            {
                return;
            }

            var newGuid = bag.ChatChannelAvatarBinaryFile?.Value.AsGuidOrNull();
            entity.ChatChannelAvatarBinaryFileId = newGuid.HasValue
                ? new BinaryFileService( RockContext ).GetSelect( newGuid.Value, bf => ( int? ) bf.Id )
                : null;
        }

        /// <summary>
        /// Applies the inline-schedule lifecycle on save. Custom and
        /// Weekly types are downgraded to None when their required inputs
        /// are missing, then <see cref="Model.Group.Schedule"/> and
        /// <see cref="Model.Group.ScheduleId"/> are mutated to match the
        /// resolved selection.
        /// </summary>
        private void ApplyInlineSchedule( Model.Group entity, GroupBag bag )
        {
            var scheduleType = bag.ScheduleType;

            // Custom schedules require parseable iCalendar content with a
            // start date. Fall back to None when either is missing.
            if ( scheduleType == ScheduleType.Custom )
            {
                if ( bag.ICalendarContent.IsNullOrWhiteSpace() )
                {
                    scheduleType = ScheduleType.None;
                }
                else
                {
                    var calEvent = InetCalendarHelper.CreateCalendarEvent( bag.ICalendarContent );
                    if ( calEvent == null || calEvent.DtStart == null )
                    {
                        scheduleType = ScheduleType.None;
                    }
                }
            }

            // Weekly schedules require both a day-of-week and a parseable
            // time-of-day. Fall back to None when either is missing.
            if ( scheduleType == ScheduleType.Weekly )
            {
                if ( !bag.WeeklyDayOfWeek.HasValue
                    || !ParseTimeSpanOrNull( bag.WeeklyTimeOfDay ).HasValue )
                {
                    scheduleType = ScheduleType.None;
                }
            }

            if ( scheduleType == ScheduleType.Custom || scheduleType == ScheduleType.Weekly )
            {
                /*
                    5/26/2026 - MSE

                    Reuse the existing inline schedule when one is attached so
                    Schedule.Id stays stable across Weekly/Custom toggles. A
                    new schedule with an empty Name (the inline marker) is
                    created only when the entity has no schedule or is bound
                    to a named one.

                    The empty-Name guard is CRITICAL: reusing a named schedule
                    here would overwrite its iCal and Weekly fields, corrupting
                    every other group that references that shared resource.

                    Reason: Prevent corruption of shared named Schedules during
                    inline-schedule edits.
                */
                if ( entity.Schedule == null || !string.IsNullOrEmpty( entity.Schedule.Name ) )
                {
                    entity.Schedule = new Schedule
                    {
                        Name = string.Empty
                    };
                    entity.ScheduleId = null;
                }

                if ( scheduleType == ScheduleType.Custom )
                {
                    entity.Schedule.iCalendarContent = bag.ICalendarContent;
                    entity.Schedule.WeeklyDayOfWeek = null;
                    entity.Schedule.WeeklyTimeOfDay = null;
                }
                else // Weekly
                {
                    entity.Schedule.iCalendarContent = null;
                    entity.Schedule.WeeklyDayOfWeek = ( DayOfWeek? ) bag.WeeklyDayOfWeek;
                    entity.Schedule.WeeklyTimeOfDay = ParseTimeSpanOrNull( bag.WeeklyTimeOfDay );
                }
            }
            else if ( scheduleType == ScheduleType.Named )
            {
                var namedScheduleId = bag.NamedSchedule?.GetEntityId<Schedule>( RockContext );
                entity.ScheduleId = namedScheduleId;
                if ( namedScheduleId.HasValue )
                {
                    /*
                        5/26/2026 - MSE

                        Null the Schedule nav so EF doesn't override the
                        ScheduleId we just set with the previously tracked
                        Schedule's Id (EF prefers nav properties over FK
                        columns).

                        Reason: Force the explicit ScheduleId to win.
                    */
                    entity.Schedule = null;
                }
            }
            else // None
            {
                entity.ScheduleId = null;
                entity.Schedule = null;
            }
        }

        /// <summary>
        /// Deletes the inline schedule referenced by
        /// <paramref name="oldScheduleId"/> when no other consumer holds
        /// it. Named schedules are excluded by the empty-Name check and
        /// are never deleted by this method.
        /// </summary>
        private void DeleteInlineSchedule( int oldScheduleId )
        {
            var scheduleService = new ScheduleService( RockContext );
            var schedule = scheduleService.Get( oldScheduleId );
            if ( schedule == null || !string.IsNullOrEmpty( schedule.Name ) )
            {
                return;
            }

            if ( !scheduleService.CanDelete( schedule, out _ ) )
            {
                return;
            }

            scheduleService.Delete( schedule );
        }

        /// <summary>
        /// Toggles the <see cref="BinaryFile.IsTemporary"/> flag so the
        /// orphaned file is marked temporary (eligible for cleanup) and
        /// the current file is marked permanent. No work is performed
        /// when <paramref name="oldId"/> equals <paramref name="newId"/>.
        /// </summary>
        private void ToggleBinaryFileIsTemporary( int? oldId, int? newId )
        {
            var binaryFileService = new BinaryFileService( RockContext );

            // Orphan the old file only when it differs from the new.
            if ( oldId.HasValue && oldId != newId )
            {
                var orphanedFile = binaryFileService.Get( oldId.Value );
                if ( orphanedFile != null )
                {
                    orphanedFile.IsTemporary = true;
                }
            }

            if ( newId.HasValue )
            {
                var currentFile = binaryFileService.Get( newId.Value );
                if ( currentFile != null )
                {
                    currentFile.IsTemporary = false;
                }
            }
        }

        #endregion Save-Flow Helpers

        #region Dropdown Option Lists & GroupType Inheritance

        /// <summary>
        /// Builds the Group Type dropdown options, filtered to those the
        /// supplied parent group permits as children. Pass <c>null</c>
        /// to retrieve the unfiltered list.
        /// </summary>
        private List<ListItemBag> BuildAllowedGroupTypeListItems( Model.Group parentGroup )
        {
            var parentGroupType = parentGroup != null
                ? GroupTypeCache.Get( parentGroup.GroupTypeId )
                : null;

            return GetAllowedGroupTypes( parentGroupType, RockContext )
                .OrderBy( gt => gt.Order )
                .ThenBy( gt => gt.Name )
                .Select( gt => new { gt.Id, gt.Name } )
                .ToList()
                .Select( gt => new ListItemBag
                {
                    Value = gt.Id.ToString(),
                    Text = gt.Name
                } )
                .ToList();
        }

        /// <summary>
        /// Builds the warning shown when the Add-mode Group Type dropdown has no options to
        /// choose from, identifying whether this block's group type settings or the parent
        /// group's allowed child group types caused it. Returns <c>null</c> when at least one
        /// group type is available.
        /// </summary>
        /// <param name="parentGroup">The currently selected parent group, or <c>null</c> when none is selected.</param>
        /// <param name="hasAllowedGroupTypes">Whether the dropdown has at least one group type available for selection.</param>
        private string GetEmptyAllowedGroupTypesWarning( Model.Group parentGroup, bool hasAllowedGroupTypes )
        {
            if ( hasAllowedGroupTypes )
            {
                return null;
            }

            /*
                06/04/2026 - MSE

                GetAllowedGroupTypes() applies two independent restrictions: this block's group type
                settings (the include/exclude, navigation, and security-role filters) and the parent
                group's allowed child group types. Either can leave nothing to choose from, which
                rendered the required Group Type dropdown empty with no explanation. Re-check the block
                settings on their own (null parent) to attribute the empty list to the correct cause.
                The text is rendered through Vue interpolation, which escapes it, so no HTML encoding
                is applied here.

                Reason: https://github.com/SparkDevNetwork/Rock/issues/6851
            */
            var isAnyGroupTypeAllowedByBlock = GetAllowedGroupTypes( null, RockContext ).Any();
            if ( !isAnyGroupTypeAllowedByBlock )
            {
                return "There are no group types available to select because of this block's group type settings (e.g. 'Group Types: Include' / 'Group Types: Exclude').";
            }

            var parentGroupName = parentGroup?.Name ?? string.Empty;
            var parentGroupType = parentGroup != null ? GroupTypeCache.Get( parentGroup.GroupTypeId ) : null;
            var doesParentAllowAnyChildGroupTypes = parentGroupType == null
                || parentGroupType.AllowAnyChildGroupType
                || parentGroupType.ChildGroupTypes.Any();
            if ( !doesParentAllowAnyChildGroupTypes )
            {
                return $"The '{parentGroupName}' group does not allow any child group types.";
            }

            return $"The child group types allowed by the '{parentGroupName}' group are excluded by this block's group type settings (e.g. 'Group Types: Include' / 'Group Types: Exclude').";
        }

        /// <summary>
        /// Builds the Required Signature Document dropdown options. Active
        /// templates are included along with the group's currently-bound
        /// template so an inactive binding still surfaces its value.
        /// </summary>
        private List<ListItemBag> BuildSignatureDocumentTemplateListItems( Model.Group entity )
        {
            var currentTemplateId = entity?.RequiredSignatureDocumentTemplateId;

            return new SignatureDocumentTemplateService( RockContext )
                .Queryable()
                .Where( t => t.IsActive || t.Id == currentTemplateId )
                .OrderBy( t => t.Name )
                .Select( t => new { t.Id, t.Name } )
                .ToList()
                .Select( t => new ListItemBag
                {
                    Value = t.Id.ToString(),
                    Text = t.Name
                } )
                .ToList();
        }

        /// <summary>
        /// Builds the SystemCommunication dropdown options for the Group
        /// Sync Welcome and Exit pickers. Returns every SystemCommunication
        /// regardless of category.
        /// </summary>
        private List<ListItemBag> BuildSystemCommunicationOptions()
        {
            return new SystemCommunicationService( RockContext ).Queryable()
                .OrderBy( c => c.Title )
                .Select( c => new ListItemBag
                {
                    Value = c.Guid.ToString(),
                    Text = c.Title
                } )
                .ToList();
        }

        /// <summary>
        /// Builds the SystemCommunication dropdown options for the RSVP
        /// Reminder picker, filtered to the RSVP Confirmation category.
        /// </summary>
        private List<ListItemBag> BuildRsvpSystemCommunicationOptions()
        {
            var rsvpCategoryGuid = Rock.SystemGuid.Category.SYSTEM_COMMUNICATION_RSVP_CONFIRMATION.AsGuid();

            return new SystemCommunicationService( RockContext ).Queryable()
                .Where( c => c.Category.Guid == rsvpCategoryGuid )
                .OrderBy( c => c.Title )
                .Select( c => new ListItemBag { Value = c.Guid.ToString(), Text = c.Title } )
                .ToList();
        }

        /// <summary>
        /// Builds the GroupRequirementType dropdown options for the Group
        /// Requirement modal. Each entry carries the type's
        /// <see cref="Model.DueDateType"/> so the Due Date well can react
        /// to the selection without a server round-trip.
        /// </summary>
        private List<GroupRequirementTypeBag> BuildGroupRequirementTypeOptions()
        {
            return new GroupRequirementTypeService( RockContext ).Queryable()
                .OrderBy( req => req.Name )
                .Select( req => new GroupRequirementTypeBag
                {
                    Text = req.Name,
                    Value = req.Guid.ToString(),
                    DueDateType = req.DueDateType
                } )
                .ToList();
        }

        /// <summary>
        /// Builds the Group Role dropdown options from
        /// <c>GroupType.Roles</c> on the immediate group type. No
        /// inheritance walk is performed. The role's
        /// <see cref="GroupTypeRole.Guid"/> is used as the value to match
        /// how it is persisted in the workflow-trigger
        /// <c>TypeQualifier</c>.
        /// </summary>
        /// <param name="groupType">The active group type cache.</param>
        private List<ListItemBag> BuildGroupRoleOptions( GroupTypeCache groupType )
        {
            if ( groupType?.Roles == null )
            {
                return new List<ListItemBag>();
            }

            return groupType.Roles
                .OrderBy( r => r.Order )
                .ThenBy( r => r.Name )
                .ToListItemBagList();
        }

        /// <summary>
        /// Walks the GroupType inheritance chain from
        /// <paramref name="groupType"/> upward and invokes
        /// <paramref name="visit"/> for each ancestor. Cycle-protected
        /// via a visited-id set.
        /// </summary>
        /// <param name="groupType">The active group type to start from.</param>
        /// <param name="visit">Per-ancestor callback receiving the ancestor <see cref="GroupTypeCache"/>.</param>
        private void WalkGroupTypeInheritancePath( GroupTypeCache groupType, Action<GroupTypeCache> visit )
        {
            var current = groupType;
            if ( current == null )
            {
                return;
            }

            var visited = new HashSet<int>();
            do
            {
                if ( !visited.Add( current.Id ) )
                {
                    break;
                }

                visit( current );

                current = current.InheritedGroupTypeId.HasValue
                    ? GroupTypeCache.Get( current.InheritedGroupTypeId.Value )
                    : null;
            } while ( current != null );
        }

        /// <summary>
        /// Resolves the GroupType <c>LinkUrlLavaTemplate</c> against the
        /// supplied ancestor entity, returning null when no template is
        /// configured.
        /// </summary>
        /// <param name="urlTemplate">The Lava template (typically the GroupType <c>LinkUrlLavaTemplate</c>).</param>
        /// <param name="inheritedGroupType">The ancestor cache entry used as the <c>Entity</c> merge field.</param>
        private string ResolveGroupTypeInheritedFromUrl( string urlTemplate, GroupTypeCache inheritedGroupType )
        {
            if ( urlTemplate.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var resolved = urlTemplate.ResolveMergeFields( new Dictionary<string, object>
            {
                ["Entity"] = inheritedGroupType
            } );
            return this.RequestContext.ResolveRockUrl( resolved );
        }

        /// <summary>
        /// Collects every group-member attribute defined on the supplied
        /// group type or any of its ancestors. From the Group's
        /// perspective every GroupType-level attribute is inherited; the
        /// Group itself only owns attributes qualified by GroupId.
        /// </summary>
        /// <param name="groupType">The active group type whose chain is walked.</param>
        private List<GroupMemberInheritedAttributeBag> BuildInheritedMemberAttributes( GroupTypeCache groupType )
        {
            var inheritedAttributes = new List<GroupMemberInheritedAttributeBag>();

            // The group type itself first, then each ancestor in inheritance order.
            var groupTypeChain = new List<GroupTypeCache>();
            WalkGroupTypeInheritancePath( groupType, groupTypeChain.Add );

            if ( !groupTypeChain.Any() )
            {
                return inheritedAttributes;
            }

            // Resolve the URL template once per cascade.
            var urlTemplate = EntityTypeCache.Get( typeof( GroupType ) )?.LinkUrlLavaTemplate;

            /*
                8/26/26 - MSE

                One query for the whole inheritance chain instead of one per ancestor.
                The rows come back ordered by Order then Name and are then grouped per
                ancestor in chain order, which is the same output the per-ancestor
                queries produced.

                Reason: Fewer round trips while building the edit panel options.
            */
            var qualifierValues = groupTypeChain.Select( gt => gt.Id.ToString() ).ToList();
            var attributes = new AttributeService( RockContext ).GetByEntityTypeId( new GroupMember().TypeId, false )
                .AsNoTracking()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "GroupTypeId", StringComparison.OrdinalIgnoreCase ) &&
                    qualifierValues.Contains( a.EntityTypeQualifierValue ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .Select( a => new
                {
                    a.Name,
                    a.Description,
                    a.Key,
                    a.Guid,
                    a.EntityTypeQualifierValue
                } )
                .ToList();

            foreach ( var inheritedGroupType in groupTypeChain )
            {
                var inheritedFromUrl = ResolveGroupTypeInheritedFromUrl( urlTemplate, inheritedGroupType );
                var qualifierValue = inheritedGroupType.Id.ToString();

                inheritedAttributes.AddRange( attributes
                    .Where( a => a.EntityTypeQualifierValue == qualifierValue )
                    .Select( a => new GroupMemberInheritedAttributeBag
                    {
                        Name = a.Name,
                        Description = a.Description,
                        Key = a.Key,
                        Guid = a.Guid,
                        InheritedFromGroupTypeName = inheritedGroupType.Name,
                        InheritedFromGroupTypeUrl = inheritedFromUrl
                    } ) );
            }

            return inheritedAttributes;
        }

        /// <summary>
        /// Collects every group requirement defined on the supplied group
        /// type or any of its ancestors. The Group's own requirements are
        /// edited separately and are not included here.
        /// </summary>
        /// <param name="groupType">The group type to start from.</param>
        private List<InheritedGroupRequirementBag> BuildInheritedGroupRequirements( GroupTypeCache groupType )
        {
            var inheritedRequirements = new List<InheritedGroupRequirementBag>();

            // The group type itself first, then each ancestor in inheritance order.
            var groupTypeChain = new List<GroupTypeCache>();
            WalkGroupTypeInheritancePath( groupType, groupTypeChain.Add );

            if ( !groupTypeChain.Any() )
            {
                return inheritedRequirements;
            }

            // Resolve the URL template once per cascade.
            var urlTemplate = EntityTypeCache.Get( typeof( Model.GroupType ) )?.LinkUrlLavaTemplate;

            // One query for the whole inheritance chain, grouped per ancestor below
            // in chain order. See BuildInheritedMemberAttributes.
            var ancestorIds = groupTypeChain.Select( gt => gt.Id ).ToList();
            var requirements = new GroupRequirementService( RockContext ).Queryable()
                .AsNoTracking()
                .Include( r => r.GroupRequirementType )
                .Include( r => r.GroupRole )
                .Where( r => r.GroupTypeId.HasValue && ancestorIds.Contains( r.GroupTypeId.Value ) )
                .ToList();

            foreach ( var inheritedGroupType in groupTypeChain )
            {
                var inheritedFromUrl = ResolveGroupTypeInheritedFromUrl( urlTemplate, inheritedGroupType );
                var ancestorId = inheritedGroupType.Id;
                var ancestorName = inheritedGroupType.Name;

                inheritedRequirements.AddRange( requirements
                    .Where( r => r.GroupTypeId.Value == ancestorId )
                    .Select( r => new InheritedGroupRequirementBag
                    {
                        Guid = r.Guid,
                        Name = r.GroupRequirementType?.Name ?? string.Empty,
                        GroupRoleName = r.GroupRole?.Name ?? string.Empty,
                        AppliesToAgeClassification = r.AppliesToAgeClassification,
                        InheritedFromGroupTypeName = ancestorName,
                        InheritedFromGroupTypeUrl = inheritedFromUrl
                    } )
                    .OrderBy( r => r.Name ) );
            }

            return inheritedRequirements;
        }

        /// <summary>
        /// Builds the dropdown options for the Group Requirement modal's
        /// Due Date Attribute well: every Date or DateTime group-attribute
        /// inherited through the group type chain.
        /// </summary>
        /// <param name="groupType">The active group type cache.</param>
        private List<ListItemBag> BuildGroupDateAttributeOptions( GroupTypeCache groupType )
        {
            var results = new List<ListItemBag>();

            if ( groupType == null )
            {
                return results;
            }

            var dateFieldTypeIds = new List<int>();
            var dateFieldTypeId = FieldTypeCache.GetId( Rock.SystemGuid.FieldType.DATE.AsGuid() );
            var dateTimeFieldTypeId = FieldTypeCache.GetId( Rock.SystemGuid.FieldType.DATE_TIME.AsGuid() );

            if ( dateFieldTypeId.HasValue )
            {
                dateFieldTypeIds.Add( dateFieldTypeId.Value );
            }
            if ( dateTimeFieldTypeId.HasValue )
            {
                dateFieldTypeIds.Add( dateTimeFieldTypeId.Value );
            }

            if ( !dateFieldTypeIds.Any() )
            {
                return results;
            }

            // Walk the inheritance chain to gather every group-scope
            // attribute qualified by GroupTypeId on the Group entity type.
            // The Group entity itself does not own attributes here; every
            // Group attribute is inherited from an ancestor.
            var groupTypeChain = new List<GroupTypeCache>();
            WalkGroupTypeInheritancePath( groupType, groupTypeChain.Add );

            // One query for the whole inheritance chain, grouped per ancestor below
            // in chain order. See BuildInheritedMemberAttributes.
            var qualifierValues = groupTypeChain.Select( gt => gt.Id.ToString() ).ToList();
            var dateAttributes = new AttributeService( RockContext ).GetByEntityTypeId( new Model.Group().TypeId, false )
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "GroupTypeId", StringComparison.OrdinalIgnoreCase ) &&
                    qualifierValues.Contains( a.EntityTypeQualifierValue ) &&
                    dateFieldTypeIds.Contains( a.FieldTypeId ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .Select( a => new
                {
                    a.Guid,
                    a.Name,
                    a.EntityTypeQualifierValue
                } )
                .ToList();

            foreach ( var inheritedGroupType in groupTypeChain )
            {
                var qualifierValue = inheritedGroupType.Id.ToString();

                results.AddRange( dateAttributes
                    .Where( a => a.EntityTypeQualifierValue == qualifierValue )
                    .Select( a => new ListItemBag
                    {
                        Value = a.Guid.ToString(),
                        Text = a.Name
                    } ) );
            }

            return results;
        }

        /// <summary>
        /// Builds the per-group-type options bag for the supplied group type.
        /// </summary>
        private GroupTypeOptionsBag BuildGroupTypeOptionsBag( int groupTypeId )
        {
            var bag = new GroupTypeOptionsBag
            {
                StatusValues = new List<ListItemBag>(),
                InactiveReasons = new List<ListItemBag>(),
                InheritedMemberAttributes = new List<GroupMemberInheritedAttributeBag>(),
                InheritedGroupRequirements = new List<InheritedGroupRequirementBag>()
            };

            if ( groupTypeId <= 0 )
            {
                return bag;
            }

            var groupType = GroupTypeCache.Get( groupTypeId );
            if ( groupType == null )
            {
                return bag;
            }

            // Visibility flags.
            bag.IsRsvpSectionVisible = groupType.EnableRSVP;
            bag.IsChatSectionVisible = IsSystemChatEnabled && groupType.IsChatAllowed;
            bag.IsOverallScheduleStackVisible = ( groupType.AllowedScheduleTypes & ( ScheduleType.Weekly | ScheduleType.Custom | ScheduleType.Named ) ) != 0;
            bag.IsPeerNetworkSectionVisible = groupType.IsPeerNetworkEnabled;
            bag.IsAdministratorVisible = groupType.ShowAdministrator;
            bag.IsGroupSpecificRecordSourceVisible = groupType.AllowGroupSpecificRecordSource;
            bag.IsCheckInRequirementsVisible = groupType.TakesAttendance;
            bag.IsGroupCapacityVisible = groupType.GroupCapacityRule != GroupCapacityRule.None;
            bag.IsGroupCapacityRequired = groupType.IsCapacityRequired;
            bag.IsInactiveReasonVisible = groupType.EnableInactiveReason;
            bag.IsInactiveReasonRequired = groupType.RequiresInactiveReason;
            bag.IsStatusVisible = groupType.GroupStatusDefinedTypeId.HasValue;
            bag.RequiresCampus = groupType.GroupsRequireCampus;

            // Allowed flags.
            bag.AllowedScheduleTypes = groupType.AllowedScheduleTypes;
            bag.LocationSelectionMode = groupType.LocationSelectionMode;
            bag.EnableLocationSchedules = groupType.EnableLocationSchedules ?? false;
            bag.IsSchedulingEnabled = groupType.IsSchedulingEnabled;
            bag.AllowMultipleLocations = groupType.AllowMultipleLocations;
            bag.IsMeetingStyleEnabled = groupType.IsMeetingStyleEnabled;

            // The Location Type dropdown sources its options from the
            // group type's configured LocationTypeValues. MapStyleValueGuid
            // is block-scoped and lives on GroupDetailOptionsBag rather
            // than being re-emitted on every group-type cascade.
            bag.LocationTypeValueOptions = ( groupType.LocationTypeValues ?? new List<DefinedValueCache>() )
                .OrderBy( dv => dv.Order )
                .ThenBy( dv => dv.Value )
                .ToListItemBagList();

            // Localization.
            bag.AdministratorLabel = BuildAdministratorLabel( groupType );

            // Peer network defaults / placeholders.
            bag.RelationshipStrengthDefault = ( RelationshipStrength ) groupType.RelationshipStrength;
            bag.RelationshipGrowthEnabledDefault = groupType.RelationshipGrowthEnabled;
            bag.LeaderToLeaderMultiplierDefault = groupType.LeaderToLeaderRelationshipMultiplier;
            bag.LeaderToNonLeaderMultiplierDefault = groupType.LeaderToNonLeaderRelationshipMultiplier;
            bag.NonLeaderToLeaderMultiplierDefault = groupType.NonLeaderToLeaderRelationshipMultiplier;
            bag.NonLeaderToNonLeaderMultiplierDefault = groupType.NonLeaderToNonLeaderRelationshipMultiplier;
            bag.AreAnyRelationshipMultipliersCustomized = groupType.AreAnyRelationshipMultipliersCustomized;

            // RSVP pinned values. The group type wins when set; a null
            // value lets the group override. RsvpSystemCommunicationOptions
            // is block-scoped and lives on GroupDetailOptionsBag rather
            // than being re-emitted on every group-type cascade.
            bag.RsvpReminderOffsetDays = groupType.RSVPReminderOffsetDays;
            bag.RsvpReminderSystemCommunication = BuildSystemCommunicationListItem( groupType.RSVPReminderSystemCommunicationId );

            // Status defined values.
            if ( groupType.GroupStatusDefinedTypeId.HasValue )
            {
                var definedType = DefinedTypeCache.Get( groupType.GroupStatusDefinedTypeId.Value );
                if ( definedType != null )
                {
                    bag.StatusValues = definedType.DefinedValues
                        .Where( dv => dv.IsActive )
                        .OrderBy( dv => dv.Order )
                        .Select( dv => new ListItemBag
                        {
                            Value = dv.Id.ToString(),
                            Text = dv.Value
                        } )
                        .ToList();
                }
            }

            // Inactive reasons.
            if ( groupType.EnableInactiveReason )
            {
                bag.InactiveReasons = new GroupTypeService( RockContext )
                    .GetInactiveReasonsForGroupType( groupType.Id )
                    .Select( dv => new ListItemBag
                    {
                        Value = dv.Id.ToString(),
                        Text = dv.Value
                    } )
                    .ToList();
            }

            // Inherited group-member attributes for the read-only grid.
            // The chain walk runs server-side and emits each attribute
            // along with the source ancestor's name and URL.
            bag.InheritedMemberAttributes = BuildInheritedMemberAttributes( groupType );

            // Panel-level visibility gates and dropdown sources.
            bag.AllowSpecificGroupMemberAttributes = groupType.AllowSpecificGroupMemberAttributes;
            bag.EnableSpecificGroupRequirements = groupType.EnableSpecificGroupRequirements;
            bag.AllowGroupSync = groupType.AllowGroupSync;
            bag.AllowSpecificGroupMemberWorkflows = groupType.AllowSpecificGroupMemberWorkflows;

            bag.InheritedGroupRequirements = BuildInheritedGroupRequirements( groupType );

            bag.GroupRoleOptions = BuildGroupRoleOptions( groupType );

            return bag;
        }

        #endregion Dropdown Option Lists & GroupType Inheritance

        #region Child Collections

        /// <summary>
        /// Loads the editable per-group member attribute definitions for the
        /// supplied entity. Returns an empty list for new groups because the
        /// qualifier value depends on the persisted Id.
        /// </summary>
        private List<PublicEditableAttributeBag> LoadGroupMemberAttributes( Model.Group entity )
        {
            if ( entity == null || entity.Id == 0 )
            {
                return new List<PublicEditableAttributeBag>();
            }

            var attributeService = new AttributeService( RockContext );
            var qualifierValue = entity.Id.ToString();

            return attributeService.GetByEntityTypeId( new GroupMember().TypeId, true )
                .AsNoTracking()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "GroupId", StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( qualifierValue ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList()
                .ConvertAll( a => PublicAttributeHelper.GetPublicEditableAttribute( a ) );
        }

        /// <summary>
        /// Saves the per-group member attribute definitions for the
        /// specified qualifier.
        /// </summary>
        /// <param name="qualifierColumn">The attribute qualifier column.</param>
        /// <param name="qualifierValue">The qualifier value.</param>
        /// <param name="attributes">The attributes as edited in the UI.</param>
        private void SaveGroupMemberAttributes( string qualifierColumn, string qualifierValue, List<PublicEditableAttributeBag> attributes )
        {
            if ( attributes == null )
            {
                return;
            }

            var entityTypeId = new GroupMember().TypeId;

            // Load the existing attributes for this entity type and qualifier.
            var attributeService = new AttributeService( RockContext );
            var existingAttributes = attributeService.GetByEntityTypeQualifier( entityTypeId, qualifierColumn, qualifierValue, true ).ToList();

            // Delete any attributes that were removed in the UI.
            var remainingAttributeGuids = attributes.Select( a => a.Guid );
            foreach ( var attr in existingAttributes.Where( a => !remainingAttributeGuids.Contains( a.Guid ) ) )
            {
                attributeService.Delete( attr );
                RockContext.SaveChanges();
            }

            // The incoming attributes are already sorted in the correct order.
            int attributeOrder = 0;
            foreach ( var attrBag in attributes )
            {
                var attr = Helper.SaveAttributeEdits( attrBag, entityTypeId, qualifierColumn, qualifierValue, RockContext );
                if ( attr != null )
                {
                    attr.Order = attributeOrder++;
                }
            }
        }

        /// <summary>
        /// Loads the per-group requirements for the supplied entity, or
        /// an empty list for new groups.
        /// </summary>
        /// <param name="entity">The group entity.</param>
        private List<GroupRequirementBag> LoadGroupRequirements( Model.Group entity )
        {
            if ( entity == null || entity.Id == 0 )
            {
                return new List<GroupRequirementBag>();
            }

            return new GroupRequirementService( RockContext ).Queryable()
                .AsNoTracking()
                .Include( r => r.GroupRequirementType )
                .Include( r => r.GroupRole )
                .Include( r => r.AppliesToDataView )
                .Include( r => r.DueDateAttribute )
                .Where( r => r.GroupId.HasValue && r.GroupId.Value == entity.Id )
                .ToList()
                .Select( r => new GroupRequirementBag
                {
                    Guid = r.Guid,
                    GroupRequirementType = r.GroupRequirementType.ToListItemBag(),
                    Role = r.GroupRole != null ? new ListItemBag { Value = r.GroupRole.Guid.ToString(), Text = r.GroupRole.Name } : null,
                    AppliesToAgeClassification = r.AppliesToAgeClassification,
                    AppliesToDataView = r.AppliesToDataView.ToListItemBag(),
                    AllowLeadersToOverride = r.AllowLeadersToOverride,
                    MustMeetRequirementToAddMember = r.MustMeetRequirementToAddMember,
                    DueDateType = r.GroupRequirementType?.DueDateType ?? Model.DueDateType.Immediate,
                    DueDateStaticDate = r.DueDateStaticDate?.ToRockDateTimeOffset(),
                    DueDateAttribute = r.DueDateAttribute != null
                        ? new ListItemBag { Value = r.DueDateAttribute.Guid.ToString(), Text = r.DueDateAttribute.Name }
                        : null
                } )
                .OrderBy( r => r.GroupRequirementType?.Text )
                .ToList();
        }

        /// <summary>
        /// Persists the group requirements list for the supplied entity.
        /// </summary>
        /// <param name="entity">The group entity.</param>
        /// <param name="bags">The group requirement bags from the save payload.</param>
        private void SaveGroupRequirements( Model.Group entity, List<GroupRequirementBag> bags )
        {
            var service = new GroupRequirementService( RockContext );
            var bagList = ( bags ?? new List<GroupRequirementBag>() ).Where( b => b != null ).ToList();

            foreach ( var b in bagList.Where( b => b.Guid == Guid.Empty ) )
            {
                b.Guid = Guid.NewGuid();
            }

            SyncRelatedEntities(
                service,
                service.Queryable().Where( r => r.GroupId.HasValue && r.GroupId.Value == entity.Id ),
                bagList,
                existingKeySelector: r => r.Guid,
                incomingKeySelector: b => b.Guid,
                createNew: b => new GroupRequirement { Guid = b.Guid, GroupId = entity.Id },
                updateEntity: ( requirement, bag ) =>
                {
                    requirement.GroupId = entity.Id;
                    requirement.GroupRequirementTypeId = bag.GroupRequirementType?.GetEntityId<GroupRequirementType>( RockContext ) ?? 0;
                    requirement.GroupRoleId = bag.Role?.GetEntityId<GroupTypeRole>( RockContext );
                    requirement.MustMeetRequirementToAddMember = bag.MustMeetRequirementToAddMember;
                    requirement.AppliesToAgeClassification = bag.AppliesToAgeClassification;
                    requirement.AppliesToDataViewId = bag.AppliesToDataView?.GetEntityId<DataView>( RockContext );
                    requirement.AllowLeadersToOverride = bag.AllowLeadersToOverride;

                    requirement.DueDateStaticDate = null;
                    requirement.DueDateAttributeId = null;

                    if ( bag.DueDateType == Model.DueDateType.ConfiguredDate )
                    {
                        requirement.DueDateStaticDate = bag.DueDateStaticDate?.DateTime;
                    }
                    else if ( bag.DueDateType == Model.DueDateType.GroupAttribute )
                    {
                        requirement.DueDateAttributeId = bag.DueDateAttribute?.GetEntityId<Rock.Model.Attribute>( RockContext );
                    }
                } );
        }

        /// <summary>
        /// Loads the per-group sync rows for the supplied entity, or an
        /// empty list for new groups.
        /// </summary>
        /// <param name="entity">The group entity.</param>
        private List<GroupSyncBag> LoadGroupSyncs( Model.Group entity )
        {
            if ( entity == null || entity.Id == 0 )
            {
                return new List<GroupSyncBag>();
            }

            return new GroupSyncService( RockContext ).Queryable()
                .AsNoTracking()
                .Include( s => s.GroupTypeRole )
                .Include( s => s.SyncDataView )
                .Include( s => s.WelcomeSystemCommunication )
                .Include( s => s.ExitSystemCommunication )
                .Where( s => s.GroupId == entity.Id )
                .ToList()
                .Select( s => new GroupSyncBag
                {
                    Guid = s.Guid,
                    GroupTypeRole = s.GroupTypeRole != null
                        ? new ListItemBag { Value = s.GroupTypeRole.Guid.ToString(), Text = s.GroupTypeRole.Name }
                        : null,
                    SyncDataView = s.SyncDataView.ToListItemBag(),
                    WelcomeSystemCommunication = s.WelcomeSystemCommunication != null
                        ? new ListItemBag { Value = s.WelcomeSystemCommunication.Guid.ToString(), Text = s.WelcomeSystemCommunication.Title }
                        : null,
                    ExitSystemCommunication = s.ExitSystemCommunication != null
                        ? new ListItemBag { Value = s.ExitSystemCommunication.Guid.ToString(), Text = s.ExitSystemCommunication.Title }
                        : null,
                    AddUserAccountsDuringSync = s.AddUserAccountsDuringSync,
                    ScheduleIntervalMinutes = s.ScheduleIntervalMinutes,
                    LastRefreshDateTime = s.LastRefreshDateTime?.ToRockDateTimeOffset()
                } )
                .ToList();
        }

        /// <summary>
        /// Persists the group sync rows for the supplied entity.
        /// </summary>
        /// <param name="entity">The group entity.</param>
        /// <param name="bags">The group sync bags from the save payload.</param>
        private void SaveGroupSyncs( Model.Group entity, List<GroupSyncBag> bags )
        {
            var service = new GroupSyncService( RockContext );
            var bagList = ( bags ?? new List<GroupSyncBag>() ).Where( b => b != null ).ToList();

            foreach ( var b in bagList.Where( b => b.Guid == Guid.Empty ) )
            {
                b.Guid = Guid.NewGuid();
            }

            SyncRelatedEntities(
                service,
                service.Queryable().Where( s => s.GroupId == entity.Id ),
                bagList,
                existingKeySelector: s => s.Guid,
                incomingKeySelector: b => b.Guid,
                createNew: b => new GroupSync { Guid = b.Guid, GroupId = entity.Id },
                updateEntity: ( sync, bag ) =>
                {
                    sync.GroupId = entity.Id;
                    sync.GroupTypeRoleId = bag.GroupTypeRole?.GetEntityId<GroupTypeRole>( RockContext ) ?? 0;
                    sync.SyncDataViewId = bag.SyncDataView?.GetEntityId<DataView>( RockContext ) ?? 0;
                    sync.WelcomeSystemCommunicationId = bag.WelcomeSystemCommunication?.GetEntityId<SystemCommunication>( RockContext );
                    sync.ExitSystemCommunicationId = bag.ExitSystemCommunication?.GetEntityId<SystemCommunication>( RockContext );
                    sync.AddUserAccountsDuringSync = bag.AddUserAccountsDuringSync;
                    sync.ScheduleIntervalMinutes = bag.ScheduleIntervalMinutes;
                    // LastRefreshDateTime is owned by the sync job; do
                    // not overwrite from the UI bag.
                } );
        }

        /// <summary>
        /// Loads the per-group member workflow triggers for the supplied
        /// entity, parsing the pipe-delimited 7-tuple <c>TypeQualifier</c>
        /// into typed bag fields. Returns an empty list for new groups.
        /// </summary>
        /// <param name="entity">The group entity.</param>
        private List<GroupMemberWorkflowTriggerBag> LoadGroupMemberWorkflowTriggers( Model.Group entity )
        {
            if ( entity == null || entity.Id == 0 )
            {
                return new List<GroupMemberWorkflowTriggerBag>();
            }

            var triggers = new GroupMemberWorkflowTriggerService( RockContext ).Queryable()
                .AsNoTracking()
                .Include( t => t.WorkflowType )
                .Where( t => t.GroupId.HasValue && t.GroupId.Value == entity.Id )
                .OrderBy( t => t.Name )
                .ToList();

            var bags = new List<GroupMemberWorkflowTriggerBag>( triggers.Count );

            foreach ( var t in triggers )
            {
                // {ToStatus}|{ToRoleGuid}|{FromStatus}|{FromRoleGuid}|{TriggerOnFirstAttendance}|{ShowNoteOnPlacement}|{RequireNoteOnPlacement}
                var parts = ( t.TypeQualifier ?? string.Empty ).Split( '|' );

                var bag = new GroupMemberWorkflowTriggerBag
                {
                    Guid = t.Guid,
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
                            5/19/2026 - MSE

                            For these trigger types, the UI displays these qualifiers using the label "With Status / With Role",
                            however the persisted qualifier format stores these values in the "To" slots (part[0] and part[1]).

                            Reason: Honor the persisted slot layout when parsing the qualifier.
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
        /// Serializes a workflow trigger bag into the pipe-delimited
        /// 7-tuple <c>TypeQualifier</c> string. The format is load-bearing
        /// and must match what other trigger consumers expect.
        /// </summary>
        /// <param name="bag">The trigger bag to serialize.</param>
        private static string BuildGroupMemberWorkflowTriggerTypeQualifier( GroupMemberWorkflowTriggerBag bag )
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
                        /*
                             5/19/2026 - MSE

                             For these trigger types, the UI displays these qualifiers using the label "With Status/ With Role",
                             However, the persisted qualifier format actually stores these values in the "To" slots (part[0] and part[1]).
                        */
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
        /// Checks whether the incoming workflow trigger bags differ from
        /// the persisted set (additions, deletions, or any field change).
        /// </summary>
        /// <param name="existing">The currently persisted trigger entities for the group.</param>
        /// <param name="incomingBags">The incoming trigger bag list.</param>
        private bool HaveGroupMemberWorkflowTriggersChanged( List<GroupMemberWorkflowTrigger> existing, List<GroupMemberWorkflowTriggerBag> incomingBags )
        {
            // Deletions.
            if ( existing.Any( e => !incomingBags.Any( b => b.Guid == e.Guid ) ) )
            {
                return true;
            }

            // Additions / mutations.
            foreach ( var bag in incomingBags )
            {
                var match = existing.FirstOrDefault( e => e.Guid == bag.Guid );
                if ( match == null )
                {
                    return true;
                }

                if ( match.Name != bag.Name
                    || match.IsActive != bag.IsActive
                    || match.WorkflowTypeId != ( bag.WorkflowType?.GetEntityId<WorkflowType>( RockContext ) ?? 0 )
                    || match.TriggerType != bag.TriggerType
                    || match.TypeQualifier != BuildGroupMemberWorkflowTriggerTypeQualifier( bag ) )
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Persists the group member workflow triggers for the supplied
        /// entity. Returns true when any add, update, or delete occurred.
        /// </summary>
        /// <param name="entity">The group entity.</param>
        /// <param name="bags">The trigger bags from the save payload.</param>
        private bool SaveGroupMemberWorkflowTriggers( Model.Group entity, List<GroupMemberWorkflowTriggerBag> bags )
        {
            var service = new GroupMemberWorkflowTriggerService( RockContext );
            var bagList = ( bags ?? new List<GroupMemberWorkflowTriggerBag>() ).Where( b => b != null ).ToList();

            foreach ( var b in bagList.Where( b => b.Guid == Guid.Empty ) )
            {
                b.Guid = Guid.NewGuid();
            }

            var existingTriggers = entity != null && entity.Id != 0
                ? service.Queryable().Where( t => t.GroupId.HasValue && t.GroupId.Value == entity.Id ).ToList()
                : new List<GroupMemberWorkflowTrigger>();

            if ( !HaveGroupMemberWorkflowTriggersChanged( existingTriggers, bagList ) )
            {
                return false;
            }

            SyncRelatedEntities(
                service,
                existingTriggers.AsQueryable(),
                bagList,
                existingKeySelector: t => t.Guid,
                incomingKeySelector: b => b.Guid,
                createNew: b => new GroupMemberWorkflowTrigger { Guid = b.Guid, GroupId = entity.Id },
                updateEntity: ( trigger, bag ) =>
                {
                    trigger.GroupId = entity.Id;
                    trigger.Name = bag.Name;
                    trigger.IsActive = bag.IsActive;
                    trigger.WorkflowTypeId = bag.WorkflowType?.GetEntityId<WorkflowType>( RockContext ) ?? 0;
                    trigger.TriggerType = bag.TriggerType;
                    trigger.TypeQualifier = BuildGroupMemberWorkflowTriggerTypeQualifier( bag );
                } );

            return true;
        }

        /// <summary>
        /// Loads the editable Group Locations for the supplied entity.
        /// Active schedules only; inactive schedules attached to a
        /// <see cref="GroupLocation"/> are re-merged by the save flow.
        /// </summary>
        /// <param name="entity">The group entity.</param>
        private List<GroupLocationStateBag> LoadGroupLocations( Model.Group entity )
        {
            if ( entity == null || entity.Id == 0 )
            {
                return new List<GroupLocationStateBag>();
            }

            var groupLocations = new GroupLocationService( RockContext ).Queryable()
                .AsNoTracking()
                .Include( gl => gl.Location )
                .Include( gl => gl.Schedules )
                .Include( gl => gl.GroupLocationTypeValue )
                .Include( gl => gl.GroupLocationScheduleConfigs.Select( c => c.Schedule ) )
                .Include( gl => gl.GroupMemberPersonAlias )
                .Where( gl => gl.GroupId == entity.Id )
                .OrderBy( gl => gl.GroupLocationTypeValue.Order )
                .ThenBy( gl => gl.Order )
                .ThenBy( gl => gl.Id )
                .ToList();

            return groupLocations.Select( BuildGroupLocationStateBag ).ToList();
        }

        /// <summary>
        /// Builds a single <see cref="GroupLocationStateBag"/> from an
        /// existing <see cref="GroupLocation"/>. The selected-location
        /// mode is rebuilt from the underlying Location's geo state and
        /// the GroupMember alias presence.
        /// </summary>
        private GroupLocationStateBag BuildGroupLocationStateBag( GroupLocation gl )
        {
            var location = gl.Location;

            // Mode classification, in priority order:
            //   1. GroupMember (the row was added via the Member tab; the
            //      PersonAlias FK is the discriminator).
            //   2. Polygon / Point (geo data, unambiguous).
            //   3. Named (the Location has a Name - the user picked a
            //      pre-existing Location tree node). Named takes priority
            //      over Address because Named Locations can carry an
            //      attached address (e.g., a Building room with a street),
            //      and the user's original choice was the Name.
            //   4. Address (no Name; user typed an address).
            //   5. None (defensive fallback for rows with a null Location;
            //      should not happen in practice but keeps the hydration
            //      total).
            GroupLocationPickerMode mode;
            object selectedLocation;
            if ( gl.GroupMemberPersonAliasId.HasValue && location != null )
            {
                mode = GroupLocationPickerMode.GroupMember;
                selectedLocation = new ListItemBag
                {
                    Value = location.Guid.ToString(),
                    Text = location.ToString( false )
                };
            }
            else if ( location?.GeoFence != null )
            {
                mode = GroupLocationPickerMode.Polygon;
                selectedLocation = location.GeoFence.AsText();
            }
            else if ( location?.GeoPoint != null )
            {
                mode = GroupLocationPickerMode.Point;
                selectedLocation = location.GeoPoint.AsText();
            }
            else if ( location != null && location.Name.IsNotNullOrWhiteSpace() )
            {
                mode = GroupLocationPickerMode.Named;
                selectedLocation = new ListItemBag
                {
                    Value = location.Guid.ToString(),
                    Text = location.ToString( false )
                };
            }
            else if ( location != null && ( location.Street1.IsNotNullOrWhiteSpace() || location.City.IsNotNullOrWhiteSpace() ) )
            {
                mode = GroupLocationPickerMode.Address;
                selectedLocation = new AddressControlBag
                {
                    Street1 = location.Street1,
                    Street2 = location.Street2,
                    City = location.City,
                    State = location.State,
                    Locality = location.County,
                    PostalCode = location.PostalCode,
                    Country = location.Country
                };
            }
            else
            {
                mode = GroupLocationPickerMode.None;
                selectedLocation = null;
            }

            var bag = new GroupLocationStateBag
            {
                Guid = gl.Guid,
                LocationName = location?.ToString( false ) ?? string.Empty,
                SelectedLocationMode = mode,
                SelectedLocation = selectedLocation,
                GroupLocationTypeValueGuid = gl.GroupLocationTypeValue?.Guid,
                GroupLocationTypeValueName = gl.GroupLocationTypeValue?.Value,
                GroupMemberPersonAliasGuid = gl.GroupMemberPersonAlias?.Guid,
                Schedules = ( gl.Schedules ?? new List<Schedule>() )
                    .Where( s => s.IsActive )
                    .OrderBy( s => s.GetNextStartDateTime( RockDateTime.Now.SundayDate().AddDays( 1 ) ) ?? DateTime.MaxValue )
                    .ThenBy( s => s.Order )
                    .ThenBy( s => s.Id )
                    .Select( s => new ListItemBag
                    {
                        Value = s.Guid.ToString(),
                        Text = s.Name.IsNotNullOrWhiteSpace() ? s.Name : s.FriendlyScheduleText
                    } )
                    .ToList(),
                ScheduleConfigs = ( gl.GroupLocationScheduleConfigs ?? new List<GroupLocationScheduleConfig>() )
                    .Where( c => c.Schedule != null )
                    .Select( c => new GroupLocationScheduleConfigBag
                    {
                        ScheduleGuid = c.Schedule.Guid,
                        MinimumCapacity = c.MinimumCapacity,
                        DesiredCapacity = c.DesiredCapacity,
                        MaximumCapacity = c.MaximumCapacity
                    } )
                    .ToList()
            };

            return bag;
        }

        /// <summary>
        /// Builds the Location modal's Member-tab dropdown source: one row
        /// per (Group Member, Family, Mapped Address) tuple, excluding
        /// Previous-type addresses.
        /// </summary>
        /// <param name="entity">The group entity.</param>
        private List<FamilyMemberLocationBag> BuildFamilyMemberLocationOptions( Model.Group entity )
        {
            if ( entity == null || entity.Id == 0 )
            {
                return new List<FamilyMemberLocationBag>();
            }

            var familyGroupTypeId = GroupTypeCache.GetFamilyGroupType()?.Id;
            if ( !familyGroupTypeId.HasValue )
            {
                return new List<FamilyMemberLocationBag>();
            }

            var previousLocationTypeGuid = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_PREVIOUS.AsGuid();
            var groupMemberService = new GroupMemberService( RockContext );

            var members = groupMemberService.GetByGroupId( entity.Id )
                .AsNoTracking()
                .Where( gm => gm.Person != null && gm.Person.PrimaryAliasGuid.HasValue )
                .ToList();

            if ( !members.Any() )
            {
                return new List<FamilyMemberLocationBag>();
            }

            // Resolve every PersonId-to-FamilyGroupId link in one query.
            // Families are ordered by GroupOrder so the primary family
            // comes first; the order survives Distinct() because
            // LINQ-to-objects emits first-occurrence.
            var personIds = members.Select( gm => gm.PersonId ).Distinct().ToList();
            var familyIdsByPersonId = groupMemberService.Queryable()
                .AsNoTracking()
                .Where( gm => personIds.Contains( gm.PersonId )
                    && gm.Group.GroupTypeId == familyGroupTypeId.Value )
                .Select( gm => new { gm.PersonId, FamilyId = gm.GroupId, gm.GroupOrder } )
                .ToList()
                .GroupBy( x => x.PersonId )
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy( x => x.GroupOrder ?? int.MaxValue )
                        .Select( x => x.FamilyId )
                        .Distinct()
                        .ToList() );

            if ( !familyIdsByPersonId.Any() )
            {
                return new List<FamilyMemberLocationBag>();
            }

            // Load every relevant family GroupLocation (mapped, non-Previous)
            // with the Location and GroupLocationTypeValue navigations
            // hydrated so the loop below does not lazy-load per row.
            var allFamilyIds = familyIdsByPersonId.Values.SelectMany( ids => ids ).Distinct().ToList();
            var familyLocationsById = new GroupLocationService( RockContext ).Queryable()
                .AsNoTracking()
                .Include( gl => gl.Location )
                .Include( gl => gl.GroupLocationTypeValue )
                .Where( gl => allFamilyIds.Contains( gl.GroupId )
                    && gl.IsMappedLocation
                    && gl.GroupLocationTypeValue != null
                    && gl.GroupLocationTypeValue.Guid != previousLocationTypeGuid
                    && gl.Location != null )
                .ToList()
                .GroupBy( gl => gl.GroupId )
                .ToDictionary( g => g.Key, g => g.ToList() );

            var options = new List<FamilyMemberLocationBag>();
            var seen = new HashSet<(Guid LocationGuid, Guid PersonAliasGuid)>();

            foreach ( var member in members )
            {
                var primaryAliasGuid = member.Person.PrimaryAliasGuid.Value;

                if ( !familyIdsByPersonId.TryGetValue( member.PersonId, out var familyIds ) )
                {
                    continue;
                }

                foreach ( var familyId in familyIds )
                {
                    if ( !familyLocationsById.TryGetValue( familyId, out var locations ) )
                    {
                        continue;
                    }

                    foreach ( var gl in locations )
                    {
                        var key = (gl.Location.Guid, primaryAliasGuid);
                        if ( !seen.Add( key ) )
                        {
                            continue;
                        }

                        options.Add( new FamilyMemberLocationBag
                        {
                            LocationGuid = gl.Location.Guid,
                            PersonAliasGuid = primaryAliasGuid,
                            Text = $"{member.Person.FullName} {gl.GroupLocationTypeValue.Value} ({gl.Location})"
                        } );
                    }
                }
            }

            return options;
        }

        /// <summary>
        /// Resolves the LocationPicker payload on a
        /// <see cref="GroupLocationStateBag"/> to a tracked
        /// <see cref="Location"/>, routing by
        /// <see cref="GroupLocationStateBag.SelectedLocationMode"/>.
        /// </summary>
        private Location ResolveLocationFromBag( GroupLocationStateBag bag, LocationService locationService )
        {
            if ( bag?.SelectedLocation == null )
            {
                return null;
            }

            switch ( bag.SelectedLocationMode )
            {
                case GroupLocationPickerMode.Named:
                case GroupLocationPickerMode.GroupMember:
                {
                    // Named and GroupMember picks arrive as a ListItemBag
                    // whose Value is the Location's Guid. Round-trip
                    // through ToJson so the deserialized payload is
                    // serializer-agnostic.
                    var listItem = bag.SelectedLocation.ToJson().FromJsonOrNull<ListItemBag>();
                    var locationGuid = listItem?.Value.AsGuidOrNull();
                    return locationGuid.HasValue ? locationService.Get( locationGuid.Value ) : null;
                }

                case GroupLocationPickerMode.Address:
                {
                    var address = bag.SelectedLocation.ToJson().FromJsonOrNull<AddressControlBag>();
                    if ( address == null )
                    {
                        return null;
                    }

                    if ( address.Street1.IsNullOrWhiteSpace() && address.City.IsNullOrWhiteSpace() )
                    {
                        return null;
                    }

                    return locationService.Get(
                        address.Street1,
                        address.Street2,
                        address.City,
                        address.State,
                        address.PostalCode,
                        address.Country,
                        verifyLocation: false );
                }

                case GroupLocationPickerMode.Point:
                {
                    var wkt = bag.SelectedLocation as string ?? bag.SelectedLocation.ToString();
                    if ( wkt.IsNullOrWhiteSpace() )
                    {
                        return null;
                    }
                    System.Data.Entity.Spatial.DbGeography point;
                    try
                    {
                        point = System.Data.Entity.Spatial.DbGeography.FromText( wkt );
                    }
                    catch
                    {
                        // The picker emits invalid WKT for empty or partial
                        // selections; treat that as a no-op rather than
                        // throwing into the save flow.
                        return null;
                    }
                    return point != null ? locationService.GetByGeoPoint( point ) : null;
                }

                case GroupLocationPickerMode.Polygon:
                {
                    var wkt = bag.SelectedLocation as string ?? bag.SelectedLocation.ToString();
                    if ( wkt.IsNullOrWhiteSpace() )
                    {
                        return null;
                    }
                    System.Data.Entity.Spatial.DbGeography fence;
                    try
                    {
                        fence = System.Data.Entity.Spatial.DbGeography.PolygonFromText( wkt, System.Data.Entity.Spatial.DbGeography.DefaultCoordinateSystemId );
                    }
                    catch
                    {
                        return null;
                    }
                    return fence != null ? locationService.GetByGeoFence( fence ) : null;
                }

                default:
                    return null;
            }
        }

        /// <summary>
        /// Persists the group locations for the supplied entity, including
        /// schedule configs, member-assignment cleanup, inactive-schedule
        /// preservation, and Location resolution from the picker payload.
        /// Returns true when any add, update, or delete occurred.
        /// </summary>
        /// <param name="entity">The group entity.</param>
        /// <param name="bags">The location bags from the save payload.</param>
        private bool SaveGroupLocations( Model.Group entity, List<GroupLocationStateBag> bags )
        {
            if ( entity == null || entity.Id == 0 )
            {
                // The FK requires a persisted parent, so locations cannot
                // be saved before the group itself has an Id.
                return false;
            }

            var bagList = ( bags ?? new List<GroupLocationStateBag>() ).Where( b => b != null ).ToList();
            foreach ( var b in bagList.Where( b => b.Guid == Guid.Empty ) )
            {
                b.Guid = Guid.NewGuid();
            }

            var groupLocationService = new GroupLocationService( RockContext );
            var groupMemberAssignmentService = new GroupMemberAssignmentService( RockContext );
            var locationService = new LocationService( RockContext );
            var scheduleService = new ScheduleService( RockContext );

            // Bulk-resolve the PersonAlias Guids referenced by Member-tab
            // locations so the upsert loop below does not query per row.
            var personAliasGuids = bagList
                .Where( b => b.GroupMemberPersonAliasGuid.HasValue )
                .Select( b => b.GroupMemberPersonAliasGuid.Value )
                .Distinct()
                .ToList();

            var personAliasIdByGuid = personAliasGuids.Any()
                ? new PersonAliasService( RockContext ).Queryable()
                    .Where( pa => personAliasGuids.Contains( pa.Guid ) )
                    .Select( pa => new { pa.Guid, pa.Id } )
                    .ToDictionary( pa => pa.Guid, pa => pa.Id )
                : new Dictionary<Guid, int>();

            // Reload the persisted GroupLocations with their navigations
            // so we can diff against the incoming bags. The entity's
            // GroupLocations navigation may or may not be hydrated.
            var existingLocations = groupLocationService.Queryable()
                .Include( gl => gl.Schedules )
                .Include( gl => gl.GroupLocationScheduleConfigs )
                .Where( gl => gl.GroupId == entity.Id )
                .ToList();

            var incomingGuids = bagList.Select( b => b.Guid ).ToHashSet();
            var changed = false;

            // 1. Delete removed locations. Cascade-clean their schedule
            // configs and any GroupMemberAssignments that reference the
            // (scheduleId, locationId, groupId) tuple.
            foreach ( var existing in existingLocations.Where( gl => !incomingGuids.Contains( gl.Guid ) ).ToList() )
            {
                /*
                    5/26/2026 - MSE

                    GroupLocationScheduleConfig cascades on GroupLocation
                    delete (GroupLocationScheduleConfig.cs:148), so no
                    explicit child cleanup is needed.

                    Reason: Rely on EF cascade for child config rows.
                */

                foreach ( var schedule in existing.Schedules )
                {
                    var assignmentsToDelete = groupMemberAssignmentService.Queryable()
                        .Where( a => a.ScheduleId == schedule.Id
                            && a.LocationId == existing.LocationId
                            && a.GroupMember.GroupId == existing.GroupId )
                        .ToList();
                    groupMemberAssignmentService.DeleteRange( assignmentsToDelete );
                }

                groupLocationService.Delete( existing );
                changed = true;
            }

            // Compute the next Order value for any new rows. Order is
            // assigned once on add and never re-sequenced from the UI.
            var nextOrder = existingLocations.Any()
                ? existingLocations.Max( gl => gl.Order ) + 1
                : 0;

            // 2. Upsert each incoming location.
            foreach ( var bag in bagList )
            {
                var existing = existingLocations.FirstOrDefault( gl => gl.Guid == bag.Guid );
                var isNewLocation = existing == null;
                int? oldLocationId = isNewLocation ? null : ( int? ) existing.LocationId;

                if ( isNewLocation )
                {
                    existing = new GroupLocation
                    {
                        Guid = bag.Guid,
                        GroupId = entity.Id,
                        Order = nextOrder++
                    };
                    groupLocationService.Add( existing );
                    existingLocations.Add( existing );
                }

                // Resolve the LocationPicker bag. Skip the GroupLocation
                // entirely when the resolver returns null: the picker has
                // no valid selection and there is nothing to persist.
                var resolvedLocation = ResolveLocationFromBag( bag, locationService );
                if ( resolvedLocation == null )
                {
                    if ( isNewLocation )
                    {
                        groupLocationService.Delete( existing );
                        existingLocations.Remove( existing );
                    }
                    continue;
                }

                // Newly-created Location entities have Id == 0 until the
                // outer transaction's SaveChanges flushes them; the FK
                // resolves either way because EF copies the Id from the
                // principal on flush.
                if ( !isNewLocation && resolvedLocation.Id != existing.LocationId )
                {
                    // The user swapped the Location attached to this row.
                    // Cascade-clean any GroupMemberAssignments that
                    // referenced the previous (scheduleId, oldLocationId,
                    // groupId) tuple. Iterating the currently-attached
                    // schedules catches both surviving and removed
                    // schedule assignments in a single pass.
                    foreach ( var schedule in existing.Schedules )
                    {
                        var assignmentsToDelete = groupMemberAssignmentService.Queryable()
                            .Where( a => a.ScheduleId == schedule.Id
                                && a.LocationId == oldLocationId.Value
                                && a.GroupMember.GroupId == existing.GroupId )
                            .ToList();
                        groupMemberAssignmentService.DeleteRange( assignmentsToDelete );
                    }
                }

                // Assign the navigation reference. The scalar LocationId
                // is only set when the resolved Location is already
                // persisted; for a brand-new Location, EF relationship
                // fix-up copies the Id on flush. Assigning a zero Id
                // directly would violate the FK constraint.
                existing.Location = resolvedLocation;
                if ( resolvedLocation.Id != 0 )
                {
                    existing.LocationId = resolvedLocation.Id;
                }

                // The schedule-removal cleanup below targets a specific
                // LocationId. When the resolved Location is not yet
                // flushed, no assignments can reference it yet, so the
                // cleanup is skipped.
                var cleanupLocationId = resolvedLocation.Id != 0
                    ? ( int? ) resolvedLocation.Id
                    : null;

                existing.GroupLocationTypeValueId = bag.GroupLocationTypeValueGuid.HasValue
                    ? DefinedValueCache.GetId( bag.GroupLocationTypeValueGuid.Value )
                    : null;

                // Resolve the Member-tab PersonAlias from the dictionary
                // built before this loop to avoid a per-row query.
                existing.GroupMemberPersonAliasId = bag.GroupMemberPersonAliasGuid.HasValue
                        && personAliasIdByGuid.TryGetValue( bag.GroupMemberPersonAliasGuid.Value, out var resolvedAliasId )
                    ? ( int? ) resolvedAliasId
                    : null;

                // 3. Schedule reconciliation. Union active (from the bag)
                // with inactive (from the DB) so previously-attached but
                // now-inactive schedules are not silently dropped.
                var incomingActiveScheduleGuids = ( bag.Schedules ?? new List<ListItemBag>() )
                    .Select( s => s.Value.AsGuidOrNull() )
                    .Where( g => g.HasValue )
                    .Select( g => g.Value )
                    .ToHashSet();

                // Detach any active schedules the bag omits. Inactive
                // schedules are preserved.
                var deletedScheduleIds = new List<int>();
                foreach ( var attached in existing.Schedules.ToList() )
                {
                    if ( !attached.IsActive )
                    {
                        // Inactive schedules survive the bag round-trip.
                        continue;
                    }

                    if ( !incomingActiveScheduleGuids.Contains( attached.Guid ) )
                    {
                        deletedScheduleIds.Add( attached.Id );
                        existing.Schedules.Remove( attached );
                    }
                }

                // Attach any active schedules the bag introduces.
                var currentlyAttachedGuids = existing.Schedules.Select( s => s.Guid ).ToHashSet();
                foreach ( var newGuid in incomingActiveScheduleGuids.Where( g => !currentlyAttachedGuids.Contains( g ) ) )
                {
                    var schedule = scheduleService.Get( newGuid );
                    if ( schedule != null )
                    {
                        existing.Schedules.Add( schedule );
                    }
                }

                // 4. GroupLocationScheduleConfig diff
                // (existing / modified / new / deleted).
                var incomingConfigs = bag.ScheduleConfigs ?? new List<GroupLocationScheduleConfigBag>();
                var incomingByGuid = incomingConfigs
                    .GroupBy( c => c.ScheduleGuid )
                    .ToDictionary( g => g.Key, g => g.First() );

                // Resolve Schedule.Guid -> Schedule.Id for the configs.
                var attachedScheduleByGuid = existing.Schedules.ToDictionary( s => s.Guid, s => s );

                // Drop any existing configs not present in the incoming
                // list. Covers both schedule removals and capacity-row
                // removals.
                foreach ( var cfg in existing.GroupLocationScheduleConfigs.ToList() )
                {
                    var cfgScheduleGuid = cfg.Schedule?.Guid
                        ?? attachedScheduleByGuid.FirstOrDefault( kv => kv.Value.Id == cfg.ScheduleId ).Key;

                    if ( cfgScheduleGuid == Guid.Empty || !incomingByGuid.ContainsKey( cfgScheduleGuid ) )
                    {
                        existing.GroupLocationScheduleConfigs.Remove( cfg );
                    }
                }

                // Upsert each incoming config.
                foreach ( var incoming in incomingConfigs )
                {
                    if ( !attachedScheduleByGuid.TryGetValue( incoming.ScheduleGuid, out var schedule ) )
                    {
                        // The capacity row's schedule was removed in this
                        // same edit pass; skip the orphan.
                        continue;
                    }

                    var existingCfg = existing.GroupLocationScheduleConfigs
                        .FirstOrDefault( c => c.ScheduleId == schedule.Id );
                    if ( existingCfg == null )
                    {
                        existing.GroupLocationScheduleConfigs.Add( new GroupLocationScheduleConfig
                        {
                            ScheduleId = schedule.Id,
                            MinimumCapacity = incoming.MinimumCapacity,
                            DesiredCapacity = incoming.DesiredCapacity,
                            MaximumCapacity = incoming.MaximumCapacity
                        } );
                    }
                    else
                    {
                        existingCfg.MinimumCapacity = incoming.MinimumCapacity;
                        existingCfg.DesiredCapacity = incoming.DesiredCapacity;
                        existingCfg.MaximumCapacity = incoming.MaximumCapacity;
                    }
                }

                // 5. GroupMemberAssignment cleanup for schedules removed
                // from this location. Skipped when cleanupLocationId is
                // null (the swap landed on a not-yet-flushed Location
                // row, so no assignments can yet reference it).
                if ( cleanupLocationId.HasValue )
                {
                    foreach ( var deletedScheduleId in deletedScheduleIds )
                    {
                        var assignmentsToDelete = groupMemberAssignmentService.Queryable()
                            .Where( a => a.ScheduleId == deletedScheduleId
                                && a.LocationId == cleanupLocationId.Value
                                && a.GroupMember.GroupId == existing.GroupId )
                            .ToList();
                        groupMemberAssignmentService.DeleteRange( assignmentsToDelete );
                    }
                }

                changed = true;
            }

            return changed;
        }

        #endregion Child Collections

        #region View Panel

        /// <summary>
        /// Builds the Linkages payload for the Overview card, or null when
        /// the group has no registrations, event occurrences, or content
        /// items.
        /// </summary>
        private GroupLinkagesBag BuildLinkages( Model.Group entity )
        {
            if ( entity == null || entity.Id == 0 )
            {
                return null;
            }

            // EventItemOccurrenceGroupMap rows are the basis for both
            // registrations and event item occurrences (and indirectly
            // content items via EventItemOccurrenceChannelItem). One query
            // surfaces all three.
            var linkageRows = new EventItemOccurrenceGroupMapService( RockContext ).Queryable()
                .AsNoTracking()
                .Include( l => l.RegistrationInstance )
                .Include( l => l.EventItemOccurrence )
                .Include( l => l.EventItemOccurrence.EventItem )
                .Include( l => l.EventItemOccurrence.ContentChannelItems.Select( c => c.ContentChannelItem ) )
                .Where( l => l.GroupId == entity.Id )
                .ToList();

            // Each linkage URL emits the receiving entity's IdKey on its
            // corresponding page parameter.
            var registrations = linkageRows
                .Where( l => l.RegistrationInstance != null )
                .Select( l => l.RegistrationInstance )
                .GroupBy( r => r.Id )
                .Select( g => g.First() )
                .Select( r => new GroupLinkageBag
                {
                    Name = r.Name,
                    Url = this.GetLinkedPageUrl( AttributeKey.RegistrationInstancePage, "RegistrationInstanceId", r.IdKey )
                } )
                .Where( l => l.Url.IsNotNullOrWhiteSpace() )
                .ToList();

            var eventItemOccurrences = linkageRows
                .Where( l => l.EventItemOccurrence != null && l.EventItemOccurrence.EventItem != null )
                .Select( l => l.EventItemOccurrence )
                .GroupBy( e => e.Id )
                .Select( g => g.First() )
                .Select( e => new GroupLinkageBag
                {
                    Name = e.EventItem.Name,
                    Url = this.GetLinkedPageUrl( AttributeKey.EventItemOccurrencePage, "EventItemOccurrenceId", e.IdKey )
                } )
                .Where( l => l.Url.IsNotNullOrWhiteSpace() )
                .ToList();

            var contentItems = linkageRows
                .Where( l => l.EventItemOccurrence != null )
                .SelectMany( l => l.EventItemOccurrence.ContentChannelItems )
                .Select( c => c.ContentChannelItem )
                .Where( c => c != null )
                .GroupBy( c => c.Id )
                .Select( g => g.First() )
                .Select( c => new GroupLinkageBag
                {
                    Name = c.Title,
                    // Open the content item in edit mode, matching the WebForms
                    // behavior for items selected from an event linkage.
                    Url = this.GetLinkedPageUrl( AttributeKey.ContentItemPage, new Dictionary<string, string>
                    {
                        { "ContentItemId", c.IdKey },
                        { PageParameterKey.AutoEdit, "true" }
                    } )
                } )
                .Where( l => l.Url.IsNotNullOrWhiteSpace() )
                .ToList();

            if ( !registrations.Any() && !eventItemOccurrences.Any() && !contentItems.Any() )
            {
                return null;
            }

            return new GroupLinkagesBag
            {
                Registrations = registrations,
                EventItemOccurrences = eventItemOccurrences,
                ContentItems = contentItems
            };
        }

        /// <summary>
        /// Builds the Meeting Location cards rendered on the View panel.
        /// The <c>ShowLocationAddresses</c> block attribute hides the
        /// formatted address on every non-Polygon card; Polygon cards
        /// never render an address. Every card on a given group shares
        /// the same group-level <see cref="GroupMeetingLocationBag.MapUrl"/>.
        /// </summary>
        private List<GroupMeetingLocationBag> BuildMeetingLocations( Model.Group entity )
        {
            if ( entity == null || entity.Id == 0 )
            {
                return new List<GroupMeetingLocationBag>();
            }

            var groupLocations = new GroupLocationService( RockContext ).Queryable()
                .AsNoTracking()
                .Include( gl => gl.Location )
                .Include( gl => gl.Schedules )
                .Include( gl => gl.GroupLocationTypeValue )
                .Where( gl => gl.GroupId == entity.Id )
                .OrderBy( gl => gl.GroupLocationTypeValue.Order )
                .ThenBy( gl => gl.Order )
                .ThenBy( gl => gl.Id )
                .ToList();

            if ( !groupLocations.Any() )
            {
                return new List<GroupMeetingLocationBag>();
            }

            var showAddresses = GetAttributeValue( AttributeKey.ShowLocationAddresses ).AsBoolean( true );

            // Every card on this group shares the same group-level MapUrl.
            // Build it once with the entity's IdKey substituted so the
            // Vue layer doesn't have to.
            var mapUrl = this.GetLinkedPageUrl(
                AttributeKey.GroupMapPage,
                new Dictionary<string, string>
                {
                    [PageParameterKey.GroupId] = entity.IdKey
                } );

            return groupLocations
                .Select( gl => BuildMeetingLocationBag( gl, showAddresses, mapUrl ) )
                .ToList();
        }

        /// <summary>
        /// Builds a single <see cref="GroupMeetingLocationBag"/> from a
        /// <see cref="GroupLocation"/>. The <c>MapData</c> field carries
        /// raw Well-Known Text that the Vue layer parses through
        /// <c>@Obsidian/Utility/geo</c>'s <c>wellKnownToCoordinates</c>.
        /// </summary>
        private static GroupMeetingLocationBag BuildMeetingLocationBag( GroupLocation gl, bool showAddresses, string mapUrl )
        {
            var location = gl.Location;
            var hasGeoFence = location?.GeoFence != null;
            var hasGeoPoint = location?.GeoPoint != null;

            // GroupMember > Polygon > Point > Address. GroupMember is
            // set by the Member tab regardless of the underlying
            // Location's geo state, so it takes priority over the
            // geo-derived modes.
            GroupLocationPickerMode mode;
            if ( gl.GroupMemberPersonAliasId.HasValue )
            {
                mode = GroupLocationPickerMode.GroupMember;
            }
            else if ( hasGeoFence )
            {
                mode = GroupLocationPickerMode.Polygon;
            }
            else if ( hasGeoPoint )
            {
                mode = GroupLocationPickerMode.Point;
            }
            else
            {
                mode = GroupLocationPickerMode.Address;
            }

            // Polygon cards never render an address; other modes render
            // the formatted address when ShowLocationAddresses is on.
            string address = null;
            if ( mode != GroupLocationPickerMode.Polygon && showAddresses )
            {
                address = location?.FormattedAddress;
            }

            // Polygon cards emit the GeoFence WKT; everything else emits
            // the GeoPoint WKT. An empty string leaves the map empty
            // (no marker or shape).
            string mapData;
            if ( mode == GroupLocationPickerMode.Polygon )
            {
                mapData = location?.GeoFence?.AsText() ?? string.Empty;
            }
            else
            {
                mapData = location?.GeoPoint?.AsText() ?? string.Empty;
            }

            // The first attached schedule's friendly text. Multi-schedule
            // locations show only the first; the editing surface manages
            // the full schedule list.
            var scheduleText = gl.Schedules
                .OrderBy( s => s.Order )
                .ThenBy( s => s.Id )
                .Select( s => s.FriendlyScheduleText )
                .FirstOrDefault( t => t.IsNotNullOrWhiteSpace() );

            return new GroupMeetingLocationBag
            {
                Guid = gl.Guid,
                Address = address,
                ScheduleText = scheduleText,
                Mode = mode,
                MapData = mapData,
                MapUrl = mapUrl
            };
        }

        #endregion View Panel

        #endregion Helper Methods
    }
}

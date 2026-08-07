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
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Utility;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Group.GroupList;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using System.Data.Entity;

namespace Rock.Blocks.Group
{
    /// <summary>
    /// Lists all groups for the configured group types, or all groups the context person is a member of.
    /// </summary>
    [DisplayName( "Group List" )]
    [Category( "Groups" )]
    [Description( "Lists all groups for the configured group types or all groups for the specified person context." )]
    [IconCssClass( "ti ti-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]
    [ContextAware]
    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Primary )]
    [CustomizedGrid]

    [LinkedPage(
        "Detail Page",
        Description = "The page that will show the group details.",
        Key = AttributeKey.DetailPage,
        Order = 0 )]

    [GroupTypesField(
        "Include Group Types",
        Description = "The group types to display in the list. If none are selected, all group types will be included.",
        IsRequired = false,
        Key = AttributeKey.IncludeGroupTypes,
        Order = 1 )]

    [BooleanField(
        "Limit to Security Role Groups",
        Description = "Any groups can be flagged as a security group (even if they're not a security role). Should the list of groups be limited to these groups?",
        DefaultBooleanValue = false,
        Key = AttributeKey.LimitToSecurityRoleGroups,
        Order = 2 )]

    [GroupTypesField(
        "Exclude Group Types",
        Description = "The group types to exclude from the list (only valid if including all groups).",
        IsRequired = false,
        Key = AttributeKey.ExcludeGroupTypes,
        Order = 3 )]

    [BooleanField(
        "Display Group Path",
        Description = "Should the Group path be displayed?",
        DefaultBooleanValue = false,
        Key = AttributeKey.DisplayGroupPath,
        Order = 4 )]

    [BooleanField(
        "Display Group Type Column",
        Description = "Should the Group Type column be displayed?",
        DefaultBooleanValue = true,
        Key = AttributeKey.DisplayGroupTypeColumn,
        Order = 5 )]

    [BooleanField(
        "Display Description Column",
        Description = "Should the Description column be displayed?",
        DefaultBooleanValue = true,
        Key = AttributeKey.DisplayDescriptionColumn,
        Order = 6 )]

    [BooleanField(
        "Display Active Status Column",
        Description = "Should the Active Status column be displayed?",
        DefaultBooleanValue = false,
        Key = AttributeKey.DisplayActiveStatusColumn,
        Order = 7 )]

    [BooleanField(
        "Display Member Count Column",
        Description = "Should the Member Count column be displayed? Does not affect lists with a person context.",
        DefaultBooleanValue = true,
        Key = AttributeKey.DisplayMemberCountColumn,
        Order = 8 )]

    [BooleanField(
        "Display System Column",
        Description = "Should the System column be displayed?",
        DefaultBooleanValue = true,
        Key = AttributeKey.DisplaySystemColumn,
        Order = 9 )]

    [BooleanField(
        "Display Security Column",
        Description = "Should the Security column be displayed?",
        DefaultBooleanValue = false,
        Key = AttributeKey.DisplaySecurityColumn,
        Order = 10 )]

    [BooleanField(
        "Display Filter",
        Description = "Should the filter be displayed to allow filtering by group type?",
        DefaultBooleanValue = false,
        Key = AttributeKey.DisplayFilter,
        Order = 11 )]

    [CustomDropdownListField(
        "Limit to Active Status",
        Description = "Select which groups (and group members) to show, based on active status. Select [All] to filter by any status. Selecting Active will not show inactive/archived groups/group members.",
        ListSource = "all^[All], active^Active, inactive^Inactive",
        IsRequired = false,
        DefaultValue = "all",
        Key = AttributeKey.LimitToActiveStatus,
        Order = 12 )]

    [TextField(
        "Set Panel Title",
        Description = "The title to display in the panel header. Leave empty to have the title be set automatically based on the group type or block name.",
        IsRequired = false,
        Key = AttributeKey.SetPanelTitle,
        Order = 13 )]

    [TextField(
        "Set Panel Icon",
        Description = "The icon to display in the panel header. Leave empty to have the icon be set automatically based on the group type or default icon.",
        IsRequired = false,
        Key = AttributeKey.SetPanelIcon,
        Order = 14 )]

    [BooleanField(
        "Allow Add",
        Description = "Should block support adding a new group?",
        DefaultBooleanValue = true,
        Key = AttributeKey.AllowAdd,
        Order = 15 )]

    [CustomDropdownListField(
        "Group Picker Type",
        Description = "Used to control which kind of picker is used when adding a person to a group.",
        ListSource = "GroupPicker^Group Picker, Dropdown^Drop-down",
        IsRequired = false,
        DefaultValue = "Dropdown",
        Category = "Add Group",
        Key = AttributeKey.GroupPickerType,
        Order = 16 )]

    [GroupField(
        "Root Group (for Add Group)",
        Description = "Select the root group to use as a starting point for the tree view when using the \"Group Picker\" Group Picker Type.",
        IsRequired = false,
        Category = "Add Group",
        Key = AttributeKey.RootGroup,
        Order = 17 )]

    [Rock.SystemGuid.EntityTypeGuid( "7F6A9579-86D1-4FD2-82DE-926AC36470E2" )]
    // WAS [Rock.SystemGuid.BlockTypeGuid( "64DB9EB3-FB78-4B5D-BA55-BA0D9F46ADB8" )]
    [Rock.SystemGuid.BlockTypeGuid( "3D7FB6BE-6BBD-49F7-96B4-96310AF3048A" )]
    public class GroupList : RockListBlockType<GroupListRowBag>
    {
        /// <summary>
        /// The maximum number of candidate groups allowed before the add-member drop-down falls
        /// back to the tree group picker. Above this a drop-down is both a slow query (per-group
        /// authorization) and unusable UI, so the modal renders the picker instead.
        /// </summary>
        private const int DropdownGroupCountThreshold = 1000;

        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string IncludeGroupTypes = "IncludeGroupTypes";
            public const string LimitToSecurityRoleGroups = "LimittoSecurityRoleGroups";
            public const string ExcludeGroupTypes = "ExcludeGroupTypes";
            public const string DisplayGroupPath = "DisplayGroupPath";
            public const string DisplayGroupTypeColumn = "DisplayGroupTypeColumn";
            public const string DisplayDescriptionColumn = "DisplayDescriptionColumn";
            public const string DisplayActiveStatusColumn = "DisplayActiveStatusColumn";
            public const string DisplayMemberCountColumn = "DisplayMemberCountColumn";
            public const string DisplaySystemColumn = "DisplaySystemColumn";
            public const string DisplaySecurityColumn = "DisplaySecurityColumn";
            public const string DisplayFilter = "DisplayFilter";
            public const string LimitToActiveStatus = "LimittoActiveStatus";
            public const string SetPanelTitle = "SetPanelTitle";
            public const string SetPanelIcon = "SetPanelIcon";
            public const string AllowAdd = "AllowAdd";
            public const string GroupPickerType = "GroupPickerType";
            public const string RootGroup = "RootGroup";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class PageParameterKey
        {
            /// <summary>
            /// Optional ?GroupTypeId= query string parameter. When supplied, the grid
            /// is scoped to just that one group type — overriding both the configured
            /// Include/Exclude attributes and the user's saved filter preference.
            /// Accepts Id, IdKey, or Guid (resolved via GroupTypeCache.Get).
            /// </summary>
            public const string GroupTypeId = "GroupTypeId";
        }

        private static class PreferenceKey
        {
            public const string FilterGroupTypeIdKey = "filter-group-type-id-key";
            public const string FilterPurposeIdKey = "filter-purpose-id-key";
            public const string FilterActiveStatus = "filter-active-status";
        }

        #endregion Keys

        #region Properties

        /// <summary>
        /// Cached result of the person-context lookup. Evaluated lazily on first access and
        /// reused for the rest of the request — block instances are scoped per-request so
        /// this cache lives exactly as long as it should.
        /// </summary>
        private bool? _isPersonMode;

        /// <summary>
        /// Gets a value indicating whether the block is in person mode (a Person context
        /// entity is present so the grid lists groups the person is a member of).
        /// </summary>
        private bool IsPersonMode
        {
            get
            {
                if ( !_isPersonMode.HasValue )
                {
                    _isPersonMode = RequestContext.GetContextEntity<Person>() != null;
                }

                return _isPersonMode.Value;
            }
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<GroupListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = GetIsAddEnabled();
            box.IsDeleteEnabled = true;
            box.ExpectedRowCount = null;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        private GroupListOptionsBag GetBoxOptions()
        {
            var isPersonMode = IsPersonMode;
            var showFilter = GetAttributeValue( AttributeKey.DisplayFilter ).AsBoolean();
            var groupTypeIds = GetAvailableGroupTypeIds( RockContext );

            var options = new GroupListOptionsBag
            {
                IsPersonMode = isPersonMode,
                ShowElevatedSecurityColumn = !isPersonMode && GetAttributeValue( AttributeKey.LimitToSecurityRoleGroups ).AsBoolean(),
                ShowGroupTypeColumn = GetAttributeValue( AttributeKey.DisplayGroupTypeColumn ).AsBoolean(),
                ShowDescriptionColumn = GetAttributeValue( AttributeKey.DisplayDescriptionColumn ).AsBoolean(),
                ShowActiveStatusColumn = GetAttributeValue( AttributeKey.DisplayActiveStatusColumn ).AsBoolean(),
                ShowMemberCountColumn = GetAttributeValue( AttributeKey.DisplayMemberCountColumn ).AsBoolean(),
                ShowSystemColumn = GetAttributeValue( AttributeKey.DisplaySystemColumn ).AsBoolean(),
                ShowSecurityColumn = GetAttributeValue( AttributeKey.DisplaySecurityColumn ).AsBoolean(),
                ShowGroupPath = GetAttributeValue( AttributeKey.DisplayGroupPath ).AsBoolean(),
                ShowFilter = showFilter,
                ShowActiveFilter = GetAttributeValue( AttributeKey.LimitToActiveStatus ) == "all",
                GroupPickerType = GetAttributeValue( AttributeKey.GroupPickerType ),
            };

            SetPanelTitleAndIcon( groupTypeIds, options );

            if ( showFilter )
            {
                // Only include the group type filter when more than one type is available.
                if ( groupTypeIds.Count > 1 )
                {
                    options.FilterGroupTypeItems = new GroupTypeService( RockContext )
                        .Queryable()
                        .Where( gt => groupTypeIds.Contains( gt.Id ) )
                        .OrderBy( gt => gt.Order )
                        .ThenBy( gt => gt.Name )
                        .ToList()
                        .Select( gt => new ListItemBag { Value = gt.IdKey, Text = gt.Name } )
                        .ToList();
                }

                var purposeDefinedType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.GROUPTYPE_PURPOSE.AsGuid() );
                if ( purposeDefinedType != null )
                {
                    options.FilterPurposeItems = purposeDefinedType.DefinedValues
                        .OrderBy( dv => dv.Order )
                        .Select( dv => new ListItemBag { Value = dv.IdKey, Text = dv.Value } )
                        .ToList();
                }
            }

            // Restrict the tree picker to the configured group types. Mirrors the
            // WebForms gpGroup.IncludedGroupTypeIds = groupTypeIds setup so users
            // can only browse groups of the block's configured types when adding.
            // Populated for all person mode (not just GroupPicker) so the drop-down
            // can fall back to the tree picker when its dataset is too large.
            if ( isPersonMode )
            {
                options.IncludedGroupTypeGuids = groupTypeIds
                    .Select( id => GroupTypeCache.Get( id )?.Guid )
                    .Where( g => g.HasValue )
                    .Select( g => g.Value )
                    .ToList();
            }

            // Set root group Guid for the tree picker. The Obsidian GroupPicker control
            // expects a Guid, so we hand it the attribute value directly without a
            // GroupService lookup.
            options.RootGroupGuid = GetAttributeValue( AttributeKey.RootGroup ).AsGuidOrNull();

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled.
        /// </summary>
        private bool GetIsAddEnabled()
        {
            if ( !GetAttributeValue( AttributeKey.AllowAdd ).AsBoolean() )
            {
                return false;
            }

            if ( IsPersonMode )
            {
                return true;
            }

            return BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, new Dictionary<string, string> { ["GroupId"] = "((Key))" } )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<GroupListRowBag> GetListQueryable( RockContext rockContext )
        {
            var groupTypeIds = GetAvailableGroupTypeIds( rockContext );
            var preferences = GetBlockPersonPreferences();

            var isFilterDisplayed = GetAttributeValue( AttributeKey.DisplayFilter ).AsBoolean();

            // Apply filter preferences.
            var filterGroupTypeIdKey = preferences.GetValue( PreferenceKey.FilterGroupTypeIdKey );
            var filterPurposeIdKey = preferences.GetValue( PreferenceKey.FilterPurposeIdKey );
            var filterActiveStatus = preferences.GetValue( PreferenceKey.FilterActiveStatus );

            // Resolve filter group type from IdKey.
            if ( isFilterDisplayed && filterGroupTypeIdKey.IsNotNullOrWhiteSpace() )
            {
                var filterGroupType = GroupTypeCache.GetByIdKey( filterGroupTypeIdKey, rockContext );
                if ( filterGroupType != null && groupTypeIds.Contains( filterGroupType.Id ) )
                {
                    groupTypeIds = new List<int> { filterGroupType.Id };
                }
            }

            // Apply the ?GroupTypeId= query string override last. Mirrors the WebForms
            // behavior of clearing the include list and scoping the grid to just the
            // requested type, even if it isn't in the configured Include attribute.
            var groupTypeIdParam = PageParameter( PageParameterKey.GroupTypeId );
            if ( groupTypeIdParam.IsNotNullOrWhiteSpace() )
            {
                var requestedGroupType = GroupTypeCache.Get( groupTypeIdParam, !PageCache.Layout.Site.DisablePredictableIds );
                if ( requestedGroupType != null )
                {
                    groupTypeIds = new List<int> { requestedGroupType.Id };
                }
            }

            var onlySecurityGroups = GetAttributeValue( AttributeKey.LimitToSecurityRoleGroups ).AsBoolean();
            var groupService = new GroupService( rockContext );

            var qryGroups = groupService.AsNoFilter()
                .Where( g => groupTypeIds.Contains( g.GroupTypeId ) && ( !onlySecurityGroups || g.IsSecurityRole ) );

            // Determine active status filtering.
            var limitToActiveStatus = GetAttributeValue( AttributeKey.LimitToActiveStatus );
            bool showActive = true;
            bool showInactive = true;

            if ( limitToActiveStatus != "all" )
            {
                showActive = limitToActiveStatus == "active";
                showInactive = limitToActiveStatus == "inactive";
            }
            else if ( isFilterDisplayed && filterActiveStatus.IsNotNullOrWhiteSpace() )
            {
                showActive = filterActiveStatus != "inactive";
                showInactive = filterActiveStatus != "active";
            }

            // Resolve purpose filter from IdKey.
            int? filterPurposeId = null;
            if ( isFilterDisplayed && filterPurposeIdKey.IsNotNullOrWhiteSpace() )
            {
                var purposeValue = DefinedValueCache.GetByIdKey( filterPurposeIdKey, rockContext );
                filterPurposeId = purposeValue?.Id;
            }

            if ( IsPersonMode )
            {
                return GetPersonModeQueryable( rockContext, qryGroups, showActive, showInactive, filterPurposeId );
            }
            else
            {
                return GetGroupListModeQueryable( rockContext, qryGroups, groupTypeIds, showActive, showInactive, filterPurposeId );
            }
        }

        /// <summary>
        /// Builds the queryable for GroupList mode.
        /// </summary>
        private IQueryable<GroupListRowBag> GetGroupListModeQueryable( RockContext rockContext, IQueryable<Rock.Model.Group> qryGroups, List<int> groupTypeIds, bool showActive, bool showInactive, int? filterPurposeId )
        {
            if ( !showInactive )
            {
                qryGroups = qryGroups.Where( g => g.IsActive );
            }
            else if ( !showActive )
            {
                qryGroups = qryGroups.Where( g => !g.IsActive );
            }

            if ( filterPurposeId.HasValue )
            {
                qryGroups = qryGroups.Where( g => g.GroupType.GroupTypePurposeValueId == filterPurposeId.Value );
            }

            var onlySecurityGroups = GetAttributeValue( AttributeKey.LimitToSecurityRoleGroups ).AsBoolean();
            var roleGroupTypeId = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid() )?.Id ?? 0;
            bool useRolePrefix = onlySecurityGroups || groupTypeIds.Contains( roleGroupTypeId );

            // Capture IQueryable references outside the lambda so EF can build correlated subqueries.
            var groupMemberQry = new GroupMemberService( rockContext ).Queryable();
            var groupSyncQry = new GroupSyncService( rockContext ).Queryable();

            return qryGroups.Select( g => new GroupListRowBag
            {
                Id = g.Id,
                GroupIdKey = null,
                GroupMemberId = null,
                GroupMemberIdKey = null,
                Name = g.Name,
                UseRolePrefix = useRolePrefix && g.GroupTypeId != roleGroupTypeId,
                GroupTypeId = g.GroupTypeId,
                GroupTypeName = g.GroupType.Name,
                GroupTypeOrder = g.GroupType.Order,
                GroupOrder = g.Order,
                Description = g.Description,
                IsSystem = g.IsSystem,
                IsActive = g.IsActive,
                IsArchived = g.IsArchived,
                GroupRole = string.Empty,
                ElevatedSecurityLevel = g.ElevatedSecurityLevel,
                IsSecurityRole = g.IsSecurityRole,
                DateAdded = null,
                IsSynced = groupSyncQry.Any( gs => gs.GroupId == g.Id ),
                MemberCount = groupMemberQry.Count( gm => gm.GroupId == g.Id ),
                HasChatChannel = false,
                // Auth and history checks done in GetListItems
                CanDelete = false,
                NeedsArchive = false,
                EnableGroupHistory = g.GroupType.EnableGroupHistory
            } );
        }

        /// <summary>
        /// Builds the queryable for person mode (groups the person is a member of).
        /// </summary>
        private IQueryable<GroupListRowBag> GetPersonModeQueryable( RockContext rockContext, IQueryable<Rock.Model.Group> qryGroups, bool showActive, bool showInactive, int? filterPurposeId )
        {
            var personContext = RequestContext.GetContextEntity<Person>();
            if ( personContext == null )
            {
                return Enumerable.Empty<GroupListRowBag>().AsQueryable();
            }

            var qry = new GroupMemberService( rockContext )
                .Queryable( true, true )
                .Where( m => m.PersonId == personContext.Id )
                .Join( qryGroups, gm => gm.GroupId, g => g.Id, ( gm, g ) => new { Group = g, GroupMember = gm } );

            if ( showActive && !showInactive )
            {
                qry = qry.Where( x => x.Group.IsActive && !x.Group.IsArchived
                    && x.GroupMember.GroupMemberStatus == GroupMemberStatus.Active
                    && !x.GroupMember.IsArchived );
            }
            else if ( !showActive )
            {
                qry = qry.Where( x => !x.Group.IsActive || x.Group.IsArchived
                    || x.GroupMember.IsArchived
                    || x.GroupMember.GroupMemberStatus == GroupMemberStatus.Inactive );
            }

            if ( filterPurposeId.HasValue )
            {
                qry = qry.Where( x => x.Group.GroupType.GroupTypePurposeValueId == filterPurposeId.Value );
            }

            return qry.Select( x => new GroupListRowBag
            {
                Id = x.Group.Id,
                GroupIdKey = null,
                GroupMemberId = x.GroupMember.Id,
                GroupMemberIdKey = null,
                Name = x.Group.Name,
                UseRolePrefix = false,
                GroupTypeId = x.Group.GroupTypeId,
                GroupTypeName = x.Group.GroupType.Name,
                GroupTypeOrder = x.Group.GroupType.Order,
                GroupOrder = x.Group.Order,
                Description = x.Group.Description,
                IsSystem = x.Group.IsSystem,
                IsActive = x.Group.IsActive && x.GroupMember.GroupMemberStatus == GroupMemberStatus.Active,
                IsArchived = x.Group.IsArchived || x.GroupMember.IsArchived,
                GroupRole = x.GroupMember.GroupRole.Name,
                ElevatedSecurityLevel = x.GroupMember.Group.ElevatedSecurityLevel,
                IsSecurityRole = x.GroupMember.Group.IsSecurityRole,
                DateAdded = x.GroupMember.DateTimeAdded ?? x.GroupMember.CreatedDateTime,
                IsSynced = x.Group.GroupSyncs.Any( s => s.GroupTypeRoleId == x.GroupMember.GroupRoleId ),
                MemberCount = 0,
                HasChatChannel = false,
                CanDelete = false,
                NeedsArchive = false,
                EnableGroupHistory = x.Group.GroupType.EnableGroupHistory
            } );
        }

        /// <inheritdoc/>
        protected override IQueryable<GroupListRowBag> GetOrderedListQueryable( IQueryable<GroupListRowBag> queryable, RockContext rockContext )
        {
            // Default sort matches the WebForms block (Name ascending). Users can override
            // by clicking column headers; this only applies when no sort is selected.
            return queryable.OrderBy( r => r.Name );
        }

        /// <inheritdoc/>
        protected override List<GroupListRowBag> GetListItems( IQueryable<GroupListRowBag> queryable, RockContext rockContext )
        {
            var currentPerson = RequestContext.CurrentPerson;
            var projectedRows = queryable.ToList();

            foreach ( var row in projectedRows )
            {
                row.GroupIdKey = IdHasher.Instance.GetHash( row.Id );
                if ( row.GroupMemberId.HasValue )
                {
                    row.GroupMemberIdKey = IdHasher.Instance.GetHash( row.GroupMemberId.Value );
                }
            }

            // Batch-load all relevant Group entities for auth checks to avoid N+1 queries.
            // AsNoFilter is required so archived groups still resolve here — qryGroups
            // is also AsNoFilter, so a regular Queryable() would silently drop archived
            // groups from the final result set
            var groupIds = projectedRows.Select( r => r.Id ).Distinct().ToList();
            var groupsById = new GroupService( rockContext )
                .AsNoFilter()
                .Where( g => groupIds.Contains( g.Id ) )
                .ToDictionary( g => g.Id );

            // Auth filter — mirrors the source block's .Where(g => g.IsAuthorized(VIEW, CurrentPerson))
            var rows = projectedRows
                .Where( r => groupsById.TryGetValue( r.Id, out var g ) && g.IsAuthorized( Authorization.VIEW, currentPerson ) )
                .ToList();

            // Refresh the working set of group IDs after auth filtering.
            groupIds = rows.Select( r => r.Id ).Distinct().ToList();

            HashSet<int> rowsWithHistory;
            if ( IsPersonMode )
            {
                var groupMemberIds = rows.Where( r => r.GroupMemberId.HasValue ).Select( r => r.GroupMemberId.Value ).ToList();
                rowsWithHistory = new HashSet<int>(
                    new GroupMemberHistoricalService( rockContext )
                        .Queryable()
                        .Where( h => groupMemberIds.Contains( h.GroupMemberId ) )
                        .Select( h => h.GroupMemberId )
                        .Distinct()
                        .ToList() );
            }
            else
            {
                rowsWithHistory = new HashSet<int>(
                    new GroupHistoricalService( rockContext )
                        .Queryable()
                        .Where( h => groupIds.Contains( h.GroupId ) )
                        .Select( h => h.GroupId )
                        .Distinct()
                        .ToList() );
            }

            // Resolve chat channel groups in GroupList mode.
            HashSet<int> chatChannelGroupIds = null;
            if ( !IsPersonMode )
            {
                chatChannelGroupIds = new HashSet<int>(
                    new GroupService( rockContext ).GetChatChannelGroupsQuery()
                        .Where( g => groupIds.Contains( g.Id ) )
                        .Select( g => g.Id )
                        .ToList() );
            }

            // Compute per-row CanDelete, NeedsArchive, path, and display name.
            var showGroupPath = GetAttributeValue( AttributeKey.DisplayGroupPath ).AsBoolean();

            var hasBlockEditAuth = BlockCache.IsAuthorized( Authorization.EDIT, currentPerson );

            foreach ( var row in rows )
            {
                // Apply "GROUP - " prefix for security role groups shown alongside other types.
                if ( row.UseRolePrefix )
                {
                    row.Name = "GROUP - " + row.Name;
                }

                if ( showGroupPath )
                {
                    // Walk the ancestor chain in-memory via GroupCache instead of running
                    // a recursive SQL CTE per row (the WebForms block used
                    // GroupService.GroupAncestorPathName which did one DB round-trip per
                    // call — N+1 across the grid).
                    row.Path = BuildGroupAncestorPath( row.Id );
                }

                // Person mode keys the history set by GroupMemberId (per-membership snapshots);
                // GroupList mode keys it by GroupId (per-group snapshots).
                var historyKey = IsPersonMode ? ( row.GroupMemberId ?? 0 ) : row.Id;
                row.NeedsArchive = row.EnableGroupHistory && rowsWithHistory.Contains( historyKey );

                groupsById.TryGetValue( row.Id, out var group );

                if ( !IsPersonMode )
                {
                    row.HasChatChannel = chatChannelGroupIds != null && chatChannelGroupIds.Contains( row.Id );
                    row.CanDelete = hasBlockEditAuth
                        && !row.IsSystem
                        && !row.IsArchived
                        && group != null
                        && group.IsAuthorized( Authorization.EDIT, currentPerson );
                }
                else
                {
                    // In person mode: delete (remove membership) allowed if not synced,
                    // not already archived, and user has EDIT or MANAGE_MEMBERS on the group.
                    if ( !row.IsSynced && !row.IsArchived )
                    {
                        row.CanDelete = group != null
                            && ( group.IsAuthorized( Authorization.EDIT, currentPerson )
                                || group.IsAuthorized( Authorization.MANAGE_MEMBERS, currentPerson ) );
                    }
                }
            }

            return rows;
        }

        /// <inheritdoc/>
        protected override GridBuilder<GroupListRowBag> GetGridBuilder()
        {
            var isPersonMode = IsPersonMode;

            return new GridBuilder<GroupListRowBag>()
                .WithBlock( this, new GridBuilderGridOptions<GroupListRowBag>
                {
                    LavaObject = r => new
                    {
                        r.Id,
                        r.Name,
                        r.Description,
                        r.GroupTypeName,
                        r.GroupTypeId,
                        r.IsActive,
                        r.IsArchived,
                        r.IsSynced,
                        r.IsSecurityRole,
                        r.IsSystem,
                        r.MemberCount,
                        r.GroupRole,
                        r.DateAdded,
                        r.Path,
                        r.ElevatedSecurityLevel,
                        r.GroupMemberId,
                        r.HasChatChannel
                    }
                } )
                .AddTextField( "idKey", r => isPersonMode ? r.GroupMemberIdKey : r.GroupIdKey )
                .AddTextField( "groupIdKey", r => r.GroupIdKey )
                .AddTextField( "groupMemberIdKey", r => r.GroupMemberIdKey )
                .AddTextField( "name", r => r.Name )
                .AddTextField( "path", r => r.Path )
                .AddTextField( "groupType", r => r.GroupTypeName )
                .AddTextField( "description", r => r.Description )
                .AddField( "elevatedSecurityLevel", r => r.ElevatedSecurityLevel )
                .AddField( "memberCount", r => r.MemberCount )
                .AddField( "isSystem", r => r.IsSystem )
                .AddField( "isActive", r => r.IsActive )
                .AddField( "isArchived", r => r.IsArchived )
                .AddField( "isSynced", r => r.IsSynced )
                .AddField( "isSecurityRole", r => r.IsSecurityRole )
                .AddTextField( "groupRole", r => r.GroupRole )
                .AddDateTimeField( "dateAdded", r => r.DateAdded )
                .AddField( "canDelete", r => r.CanDelete )
                .AddField( "needsArchive", r => r.NeedsArchive )
                .AddField( "hasChatChannel", r => r.HasChatChannel );
        }

        /// <summary>
        /// Gets the list of available group type IDs based on the block attribute settings.
        /// </summary>
        private List<int> GetAvailableGroupTypeIds( RockContext rockContext )
        {
            var groupTypeService = new GroupTypeService( rockContext );
            var qry = groupTypeService.Queryable().Where( t => t.ShowInGroupList );

            var includeGuids = GetAttributeValue( AttributeKey.IncludeGroupTypes )
                .SplitDelimitedValues()
                .Select( a => a.AsGuid() )
                .Where( g => g != Guid.Empty )
                .ToList();

            if ( includeGuids.Count > 0 )
            {
                qry = qry.Where( t => includeGuids.Contains( t.Guid ) );
            }

            var excludeGuids = GetAttributeValue( AttributeKey.ExcludeGroupTypes )
                .SplitDelimitedValues()
                .Select( a => a.AsGuid() )
                .Where( g => g != Guid.Empty )
                .ToList();

            if ( excludeGuids.Count > 0 )
            {
                qry = qry.Where( t => !excludeGuids.Contains( t.Guid ) );
            }

            return qry.Select( t => t.Id ).ToList();
        }

        /// <summary>
        /// Builds the " > "-joined ancestor path for the specified group by walking
        /// <see cref="GroupCache"/> in memory. Avoids the per-row SQL CTE that the WebForms
        /// block ran via <c>GroupService.GroupAncestorPathName</c>, which was an N+1 query
        /// across the grid whenever DisplayGroupPath was on. Mirrors the original SQL's
        /// behavior of returning an empty string when any group in the chain is missing or
        /// archived (those rows broke the WHERE ParentGroupId IS NULL clause of the CTE).
        /// </summary>
        /// <param name="groupId">The leaf group whose ancestor path to build.</param>
        private static string BuildGroupAncestorPath( int groupId )
        {
            var segments = new Stack<string>();
            var visited = new HashSet<int>();
            int? currentId = groupId;

            while ( currentId.HasValue )
            {
                // Cycle guard — pathological parent loops would otherwise spin forever.
                if ( !visited.Add( currentId.Value ) )
                {
                    return string.Empty;
                }

                var cache = GroupCache.Get( currentId.Value );
                if ( cache == null || cache.IsArchived )
                {
                    // Original SQL returned no rows when any link in the chain was archived
                    // or missing; reproduce that by abandoning the partial path.
                    return string.Empty;
                }

                segments.Push( cache.Name );
                currentId = cache.ParentGroupId;
            }

            return string.Join( " > ", segments );
        }

        /// <summary>
        /// Sets the panel title and icon on the options bag using the same logic as the source block.
        /// </summary>
        private void SetPanelTitleAndIcon( List<int> groupTypeIds, GroupListOptionsBag options )
        {
            string title;
            string icon;

            if ( groupTypeIds.Count == 1 )
            {
                var singleGroupType = GroupTypeCache.Get( groupTypeIds[0] );
                title = singleGroupType?.GroupTerm?.Pluralize() ?? BlockCache.Name;
                icon = singleGroupType?.IconCssClass ?? "ti ti-users";
            }
            else
            {
                title = BlockCache.Name;
                icon = "ti ti-users";
            }

            var customTitle = GetAttributeValue( AttributeKey.SetPanelTitle );
            var customIcon = GetAttributeValue( AttributeKey.SetPanelIcon );

            options.PanelTitle = customTitle.IsNotNullOrWhiteSpace() ? customTitle : title;
            options.PanelIcon = customIcon.IsNotNullOrWhiteSpace() ? customIcon : icon;
        }

        /// <summary>
        /// Gets available groups for the dropdown add-member picker in person mode.
        /// Mirrors the source block's BindModelDropDown logic. Called on demand from
        /// the <see cref="GetAvailableGroups"/> block action, not during initialization.
        /// </summary>
        private GroupListAvailableGroupsBag GetAvailableGroupsForDropdown( List<int> groupTypeIds )
        {
            var onlySecurityGroups = GetAttributeValue( AttributeKey.LimitToSecurityRoleGroups ).AsBoolean();
            var limitToActiveStatus = GetAttributeValue( AttributeKey.LimitToActiveStatus );

            var qry = new GroupService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Where( g => groupTypeIds.Contains( g.GroupTypeId ) && ( !onlySecurityGroups || g.IsSecurityRole ) );

            if ( limitToActiveStatus == "active" )
            {
                qry = qry.Where( g => g.IsActive );
            }

            // Guard against loading an unbounded dataset into a drop-down. The count is a cheap SQL
            // aggregate that never triggers the per-group authorization walk below. Above the threshold
            // the modal falls back to the tree group picker, which loads one level at a time.
            if ( qry.Count() > DropdownGroupCountThreshold )
            {
                return new GroupListAvailableGroupsBag { IsDatasetTooLarge = true };
            }

            var currentPerson = RequestContext.CurrentPerson;

            var personActiveRoleIdsByGroupId = new Dictionary<int, List<int>>();
            if ( currentPerson != null )
            {
                var candidateGroupIdQuery = qry.Select( g => g.Id );
                personActiveRoleIdsByGroupId = new GroupMemberService( RockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( m => m.PersonId == currentPerson.Id
                        && candidateGroupIdQuery.Contains( m.GroupId )
                        && m.GroupMemberStatus == GroupMemberStatus.Active )
                    .Select( m => new { m.GroupId, m.GroupRoleId } )
                    .ToList()
                    .GroupBy( m => m.GroupId )
                    .ToDictionary( g => g.Key, g => g.Select( m => m.GroupRoleId ).ToList() );
            }

            var groups = qry
                .OrderBy( g => g.Name )
                .ToList()
                .Where( g => g.IsAuthorized( Authorization.EDIT, currentPerson, personActiveRoleIdsByGroupId )
                    || g.IsAuthorized( Authorization.MANAGE_MEMBERS, currentPerson, personActiveRoleIdsByGroupId ) )
                .Select( g => new ListItemBag { Value = g.IdKey, Text = g.Name } )
                .ToList();

            return new GroupListAvailableGroupsBag { Groups = groups };
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Deletes or archives the specified group (GroupList mode only).
        /// </summary>
        /// <param name="key">The IdKey of the group to delete.</param>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var groupService = new GroupService( RockContext );
            var group = groupService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( group == null )
            {
                return ActionBadRequest( "Group not found." );
            }

            if ( group.IsSystem )
            {
                return ActionBadRequest( "System groups cannot be deleted." );
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson )
                || !group.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( "You are not authorized to delete this group." );
            }

            bool needsArchive = group.GroupType.EnableGroupHistory
                && new GroupHistoricalService( RockContext ).Queryable().Any( h => h.GroupId == group.Id );

            if ( needsArchive )
            {
                groupService.Archive( group, RequestContext.CurrentPerson?.PrimaryAliasId, true );
            }
            else
            {
                string errorMessage;
                if ( !groupService.CanDelete( group, out errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                if ( group.IsSecurityRoleOrSecurityGroupType() )
                {
                    GroupService.DeleteSecurityRoleGroup( group.Id );
                }
                else
                {
                    groupService.Delete( group );
                }
            }

            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Deletes or archives the specified group membership (person mode only).
        /// </summary>
        /// <param name="key">The IdKey of the group member to remove.</param>
        [BlockAction]
        public BlockActionResult DeleteMember( string key )
        {
            var groupMemberService = new GroupMemberService( RockContext );
            var groupMember = groupMemberService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( groupMember == null )
            {
                return ActionBadRequest( "Group member not found." );
            }

            var group = groupMember.Group ?? new GroupService( RockContext ).Get( groupMember.GroupId );

            // Prevent removing synced members — caller should have disabled the button but double-check here.
            bool isSynced = new GroupSyncService( RockContext )
                .Queryable()
                .Any( s => s.GroupId == groupMember.GroupId && s.GroupTypeRoleId == groupMember.GroupRoleId );

            if ( isSynced )
            {
                return ActionBadRequest( "This member is managed by group sync and cannot be manually removed." );
            }

            var currentPerson = RequestContext.CurrentPerson;
            bool hasAuth = group != null
                && ( group.IsAuthorized( Authorization.EDIT, currentPerson )
                    || group.IsAuthorized( Authorization.MANAGE_MEMBERS, currentPerson ) );

            if ( !hasAuth )
            {
                return ActionBadRequest( "You are not authorized to remove members from this group." );
            }

            bool needsArchive = group?.GroupType.EnableGroupHistory == true
                && new GroupMemberHistoricalService( RockContext ).Queryable().Any( h => h.GroupMemberId == groupMember.Id );

            if ( needsArchive )
            {
                groupMemberService.Archive( groupMember, currentPerson?.PrimaryAliasId, true );
            }
            else
            {
                string errorMessage;
                if ( !groupMemberService.CanDelete( groupMember, out errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                groupMemberService.Delete( groupMember, true );
            }

            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Gets the groups the current person may add the context person to, for the
        /// Dropdown add-member picker. Loaded on demand when the Add modal opens so the
        /// (potentially large) group query stays off the initial page render.
        /// </summary>
        [BlockAction]
        public BlockActionResult GetAvailableGroups()
        {
            if ( !IsPersonMode )
            {
                return ActionBadRequest( "Available groups are only applicable in person mode." );
            }

            var groupTypeIds = GetAvailableGroupTypeIds( RockContext );

            return ActionOk( GetAvailableGroupsForDropdown( groupTypeIds ) );
        }

        /// <summary>
        /// Gets the available non-synced group roles for the specified group.
        /// Used to populate the role dropdown in the add-member modal.
        /// </summary>
        /// <param name="groupIdKey">The IdKey of the group.</param>
        [BlockAction]
        public BlockActionResult GetGroupRoles( string groupIdKey )
        {
            var group = new GroupService( RockContext ).Get( groupIdKey, !PageCache.Layout.Site.DisablePredictableIds );
            if ( group == null )
            {
                return ActionBadRequest( "Group not found." );
            }

            var syncedRoleIds = new GroupSyncService( RockContext )
                .Queryable()
                .Where( s => s.GroupId == group.Id )
                .Select( s => s.GroupTypeRoleId )
                .ToList();

            var roles = new GroupTypeRoleService( RockContext )
                .Queryable()
                .Where( r => r.GroupTypeId == group.GroupTypeId && !syncedRoleIds.Contains( r.Id ) )
                .OrderBy( r => r.Order )
                .ThenBy( r => r.Name )
                .ToList()
                .Select( r => new ListItemBag { Value = r.IdKey, Text = r.Name } )
                .ToList();

            return ActionOk( new
            {
                Roles = roles,
                HasSyncedRoles = syncedRoleIds.Count > 0
            } );
        }

        /// <summary>
        /// Adds the context person to the specified group with the specified role.
        /// Returns a special status of "ArchiveFound" if an archived record exists.
        /// </summary>
        [BlockAction]
        public BlockActionResult AddGroupMember( GroupListAddGroupMemberRequestBag bag )
        {
            var personContext = RequestContext.GetContextEntity<Person>();
            if ( personContext == null )
            {
                return ActionBadRequest( "No person context found." );
            }

            var groupService = new GroupService( RockContext );
            var group = groupService.Get( bag.GroupIdKey, !PageCache.Layout.Site.DisablePredictableIds );

            if ( group == null )
            {
                return ActionBadRequest( "Group not found." );
            }

            var currentPerson = RequestContext.CurrentPerson;
            bool hasAuth = group.IsAuthorized( Authorization.EDIT, currentPerson )
                || group.IsAuthorized( Authorization.MANAGE_MEMBERS, currentPerson );

            if ( !hasAuth )
            {
                return ActionBadRequest( "You are not authorized to add members to this group." );
            }

            var role = new GroupTypeRoleService( RockContext ).Get( bag.RoleIdKey, !PageCache.Layout.Site.DisablePredictableIds );
            if ( role == null )
            {
                return ActionBadRequest( "Role not found." );
            }

            // Check for an existing archived group member with the same person and role,
            // unless the caller explicitly skipped this check (i.e. the user chose not to restore).
            GroupMember archivedGroupMember;
            if ( !bag.SkipArchiveCheck && groupService.ExistsAsArchived( group, personContext.Id, role.Id, out archivedGroupMember ) )
            {
                return ActionOk( new
                {
                    Status = "ArchiveFound",
                    ArchivedMemberIdKey = archivedGroupMember.IdKey,
                    Message = $"There is an archived record for {personContext} as a {role.Name} in this group. Do you want to restore the previous settings? Notes will be retained."
                } );
            }

            // Check for duplicate active member.
            var groupMemberService = new GroupMemberService( RockContext );
            if ( !GroupService.AllowsDuplicateMembers()
                && groupMemberService.Queryable().Any( m => m.PersonId == personContext.Id && m.GroupId == group.Id && m.GroupRoleId == role.Id ) )
            {
                return ActionBadRequest( "This person is already a member of the selected group in the selected role." );
            }

            var groupMember = new GroupMember
            {
                Id = 0,
                GroupId = group.Id,
                PersonId = personContext.Id,
                GroupRoleId = role.Id,
                GroupMemberStatus = GroupMemberStatus.Active
            };

            if ( !groupMember.IsValidGroupMember( RockContext ) )
            {
                var errors = groupMember.ValidationResults.Select( r => r.ErrorMessage ).ToList().AsDelimited( " " );
                return ActionBadRequest( errors );
            }

            groupMemberService.Add( groupMember );
            RockContext.SaveChanges();

            return ActionOk( new { Status = "Added" } );
        }

        /// <summary>
        /// Restores a previously archived group member.
        /// </summary>
        /// <param name="archivedMemberIdKey">The IdKey of the archived group member.</param>
        [BlockAction]
        public BlockActionResult RestoreArchivedMember( string archivedMemberIdKey )
        {
            var groupMemberService = new GroupMemberService( RockContext );
            var archivedMemberId = IdHasher.Instance.GetId( archivedMemberIdKey );
            var groupMember = groupMemberService.GetArchived()
                .FirstOrDefault( m => m.Id == archivedMemberId );

            if ( groupMember == null )
            {
                return ActionBadRequest( "Archived group member not found." );
            }

            var group = groupMember.Group ?? new GroupService( RockContext ).Get( groupMember.GroupId );
            var currentPerson = RequestContext.CurrentPerson;

            bool hasAuth = group != null
                && ( group.IsAuthorized( Authorization.EDIT, currentPerson )
                    || group.IsAuthorized( Authorization.MANAGE_MEMBERS, currentPerson ) );

            if ( !hasAuth )
            {
                return ActionBadRequest( "You are not authorized to restore members to this group." );
            }

            groupMemberService.Restore( groupMember );

            if ( !groupMember.IsValidGroupMember( RockContext ) )
            {
                var errors = groupMember.ValidationResults.Select( r => r.ErrorMessage ).ToList().AsDelimited( " " );
                return ActionBadRequest( errors );
            }

            RockContext.SaveChanges();

            return ActionOk( new { Status = "Restored" } );
        }

        #endregion
    }
}

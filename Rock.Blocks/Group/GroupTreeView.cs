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
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Group.GroupTreeView;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.Group
{
    /// <summary>
    /// Displays a navigation tree for groups of the configured group type(s). Selecting a node navigates to
    /// the configured Detail Page (or reloads the current page) with the selection and expanded nodes
    /// on the query string so sibling blocks read them as page parameters.
    /// </summary>
    [DisplayName( "Group Tree View" )]
    [Category( "Groups" )]
    [Description( "Creates a navigation tree for groups of the configured group type(s)." )]
    [IconCssClass( "ti ti-list-tree" )]
    [SupportedSiteTypes( SiteType.Web )]

    #region Block Attributes

    [TextField(
        "Treeview Title",
        Key = AttributeKey.TreeviewTitle,
        Description = "Group Tree View",
        IsRequired = false,
        Order = 1 )]

    [GroupTypesField(
        "Group Types Include",
        Key = AttributeKey.GroupTypesInclude,
        Description = "Select any specific group types to show in this block. Leave all unchecked to show all group types where 'Show in Navigation' is enabled ( except for excluded group types )",
        IsRequired = false,
        Order = 2 )]

    [GroupTypesField(
        "Group Types Exclude",
        Key = AttributeKey.GroupTypesExclude,
        Description = "Select group types to exclude from this block. Note that this setting is only effective if 'Group Types Include' has no specific group types selected.",
        IsRequired = false,
        Order = 3 )]

    [GroupField(
        "Root Group",
        Key = AttributeKey.RootGroup,
        Description = "Select the root group to use as a starting point for the tree view.",
        IsRequired = false,
        Order = 4 )]

    [BooleanField(
        "Limit to Security Role Groups",
        Key = AttributeKey.LimitToSecurityRoleGroups,
        Order = 5 )]

    [BooleanField(
        "Show Settings Panel",
        Key = AttributeKey.ShowSettingsPanel,
        DefaultBooleanValue = true,
        Order = 6 )]

    [BooleanField(
        "Display Inactive Campuses",
        Key = AttributeKey.DisplayInactiveCampuses,
        Description = "Include inactive campuses in the Campus Filter",
        DefaultBooleanValue = true )]

    [CustomDropdownListField(
        "Initial Count Setting",
        Key = AttributeKey.InitialCountSetting,
        Description = "Select the counts that should be initially shown in the treeview.",
        ListSource = "0^None,1^Child Groups,2^Group Members",
        IsRequired = false,
        DefaultValue = AttributeDefault.InitialCountSettingNone,
        Order = 7 )]

    [CustomDropdownListField(
        "Initial Active Setting",
        Key = AttributeKey.InitialActiveSetting,
        Description = "Select whether to initially show all or just active groups in the treeview",
        ListSource = "0^All,1^Active",
        IsRequired = false,
        DefaultValue = AttributeDefault.InitialActiveSettingActive,
        Order = 8 )]

    [LinkedPage(
        "Detail Page",
        Key = AttributeKey.DetailPage,
        Order = 9 )]

    [BooleanField(
        "Disable Auto-Select First Group",
        Key = AttributeKey.DisableAutoSelectFirstGroup,
        Description = "Whether to disable the default behavior of auto-selecting the first group (ordered by name) in the tree view.",
        Order = 10 )]

    [LinkedPage(
        "Search Results Page",
        Key = AttributeKey.SearchResultsPage,
        IsRequired = false,
        Category = "Advanced",
        Description = "If set, this is the page where search results will be shown when using the quick find feature. The selected page must include a Group Search block, and that block should be configured to link back to the appropriate Group Detail page.",
        Order = 11 )]

    #endregion Block Attributes

    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Navigation )]
    [Rock.SystemGuid.EntityTypeGuid( "8AC54632-3828-470B-8B43-ADD42BC710A9" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "A12308BF-C00A-45F7-9240-275FD7B1CAC6" )]
    [Rock.SystemGuid.BlockTypeGuid( "2D26A2C4-62DC-4680-8219-A52EB2BC0F65" )]
    public class GroupTreeView : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string TreeviewTitle = "TreeviewTitle";
            public const string GroupTypesInclude = "GroupTypes";
            public const string GroupTypesExclude = "GroupTypesExclude";
            public const string RootGroup = "RootGroup";
            public const string LimitToSecurityRoleGroups = "LimittoSecurityRoleGroups";
            public const string ShowSettingsPanel = "ShowFilterOption";
            public const string DisplayInactiveCampuses = "DisplayInactiveCampuses";
            public const string InitialCountSetting = "InitialCountSetting";
            public const string InitialActiveSetting = "InitialActiveSetting";
            public const string DetailPage = "DetailPage";
            public const string SearchResultsPage = "SearchResultsPage";
            public const string DisableAutoSelectFirstGroup = "DisableAutoSelectFirstGroup";
        }

        /// <summary>
        /// Default and list values for block attributes that are stored as opaque strings
        /// (custom dropdowns). Named here so comparisons are readable.
        /// </summary>
        private static class AttributeDefault
        {
            /// <summary>
            /// <see cref="AttributeKey.InitialActiveSetting"/>: show active groups only (ListSource 1^Active).
            /// </summary>
            public const string InitialActiveSettingActive = "1";

            /// <summary>
            /// <see cref="AttributeKey.InitialCountSetting"/>: no counts (ListSource 0^None).
            /// </summary>
            public const string InitialCountSettingNone = "0";
        }

        private static class PageParameterKey
        {
            public const string GroupId = "GroupId";
            public const string ExpandedIds = "ExpandedIds";
            public const string ParentGroupId = "ParentGroupId";
        }

        private static class NavigationUrlKey
        {
            public const string SearchResultsPage = "SearchResultsPage";
        }

        private static class PersonPreferenceKey
        {
            public const string HideInactiveGroups = "hide-inactive-groups";
            public const string LimitToPublic = "limit-to-public";
            public const string CountsType = "counts-type";
            public const string CampusFilter = "campus-filter";
            public const string IncludeNoCampus = "include-no-campus";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<GroupTreeViewBag, GroupTreeViewOptionsBag>
            {
                Options = GetBoxOptions(),
                NavigationUrls = GetBoxNavigationUrls()
            };

            box.Bag = GetBag();

            return box;
        }

        /// <summary>
        /// Builds the block's configured settings for the client.
        /// </summary>
        /// <returns>The options bag describing how the tree should be displayed and scoped.</returns>
        private GroupTreeViewOptionsBag GetBoxOptions()
        {
            return new GroupTreeViewOptionsBag
            {
                BlockProperties = new GroupTreeViewBlockAttributesBag
                {
                    PanelTitle = GetAttributeValue( AttributeKey.TreeviewTitle ),
                    RootGroupGuid = GetAttributeValue( AttributeKey.RootGroup ).AsGuidOrNull(),
                    IncludedGroupTypeGuids = GetGroupTypeGuids( AttributeKey.GroupTypesInclude ),
                    ExcludedGroupTypeGuids = GetGroupTypeGuids( AttributeKey.GroupTypesExclude ),
                    LimitToSecurityRoleGroups = GetAttributeValue( AttributeKey.LimitToSecurityRoleGroups ).AsBoolean(),
                    ShowSettingsPanel = GetAttributeValue( AttributeKey.ShowSettingsPanel ).AsBooleanOrNull() ?? false,
                    DisplayInactiveCampuses = GetAttributeValue( AttributeKey.DisplayInactiveCampuses ).AsBoolean(),
                    DisableAutoSelectFirstGroup = GetAttributeValue( AttributeKey.DisableAutoSelectFirstGroup ).AsBoolean(),
                    InitialCountSetting = GetAttributeValue( AttributeKey.InitialCountSetting ).AsInteger()
                }
            };
        }

        /// <summary>
        /// Builds the navigation URLs for the configured linked pages.
        /// </summary>
        /// <returns>A map of navigation key to URL.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.SearchResultsPage] = GetSearchResultsPageUrl()
            };
        }

        /// <summary>
        /// Resolves the search results page URL, falling back to the system Group Search Results page.
        /// </summary>
        /// <returns>The search results page URL, or empty when neither is available.</returns>
        private string GetSearchResultsPageUrl()
        {
            var linkedUrl = this.GetLinkedPageUrl( AttributeKey.SearchResultsPage );
            if ( linkedUrl.IsNotNullOrWhiteSpace() )
            {
                return linkedUrl;
            }

            var systemPage = new PageReference( SystemGuid.Page.GROUP_SEARCH_RESULTS );
            return systemPage.BuildUrl() ?? string.Empty;
        }

        /// <summary>
        /// Builds the runtime data for the client: selection, expansion, auth, preferences, and auto-select URL.
        /// </summary>
        /// <returns>The populated runtime bag.</returns>
        private GroupTreeViewBag GetBag()
        {
            var showSettingsPanel = GetAttributeValue( AttributeKey.ShowSettingsPanel ).AsBooleanOrNull() ?? false;
            var preferences = GetBlockPersonPreferences();
            var typePreferences = GetBlockTypePersonPreferences();

            var hideInactiveGroups = preferences.GetValue( PersonPreferenceKey.HideInactiveGroups ).AsBooleanOrNull();
            if ( !hideInactiveGroups.HasValue )
            {
                hideInactiveGroups = GetAttributeValue( AttributeKey.InitialActiveSetting ) == AttributeDefault.InitialActiveSettingActive;
            }

            if ( !showSettingsPanel )
            {
                // When the filter panel is hidden, only active groups are shown.
                hideInactiveGroups = true;
            }

            var countsTypePreferenceText = preferences.GetValue( PersonPreferenceKey.CountsType );
            var hasCountsTypePreference = countsTypePreferenceText.IsNotNullOrWhiteSpace();
            var countsTypeText = hasCountsTypePreference
                ? countsTypePreferenceText
                : GetAttributeValue( AttributeKey.InitialCountSetting );

            var countsType = showSettingsPanel ? countsTypeText.AsInteger() : 0;

            Guid? campusGuid = null;
            var campusFilterValue = typePreferences.GetValue( PersonPreferenceKey.CampusFilter );
            if ( showSettingsPanel && campusFilterValue.IsNotNullOrWhiteSpace() )
            {
                // Campus filter may be stored as Guid (new) or integer Id (legacy person preference).
                campusGuid = campusFilterValue.AsGuidOrNull();
                if ( !campusGuid.HasValue )
                {
                    var campusId = campusFilterValue.AsIntegerOrNull();
                    if ( campusId.HasValue )
                    {
                        campusGuid = CampusCache.Get( campusId.Value )?.Guid;
                    }
                }
            }

            var includeNoCampus = showSettingsPanel
                && typePreferences.GetValue( PersonPreferenceKey.IncludeNoCampus ).AsBoolean();

            var limitToPublic = showSettingsPanel
                && ( preferences.GetValue( PersonPreferenceKey.LimitToPublic ).AsBooleanOrNull() ?? false );

            var canEditBlock = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            var bag = new GroupTreeViewBag
            {
                SelectedGroupGuids = new List<Guid>(),
                ExpandedGroupGuids = new List<Guid>(),
                IsAddRootEnabled = canEditBlock,
                IsAddChildEnabled = false,
                IsAddGroupVisible = canEditBlock,
                HideInactiveGroups = hideInactiveGroups ?? true,
                LimitToPublic = limitToPublic,
                CountsType = countsType,
                HasCountsTypePreference = showSettingsPanel && hasCountsTypePreference,
                CampusGuid = campusGuid,
                IncludeNoCampus = includeNoCampus
            };

            var groupKey = RequestContext.GetPageParameter( PageParameterKey.GroupId );
            GroupCache selectedGroup = null;

            if ( groupKey.IsNotNullOrWhiteSpace() && groupKey != "0" )
            {
                selectedGroup = GroupCache.Get( groupKey, !PageCache.Layout.Site.DisablePredictableIds );
            }

            if ( selectedGroup != null )
            {
                // Walk the selection and its ancestors for expansion. If the selected
                // group or any ancestor is outside the block's include/exclude group
                // types, clear the selection (WebForms OnLoad parity).
                if ( TryAddAncestorGroupGuids( selectedGroup, bag.ExpandedGroupGuids, GetAttributeValue( AttributeKey.RootGroup ).AsGuidOrNull() ) )
                {
                    bag.SelectedGroupGuids.Add( selectedGroup.Guid );
                    ApplyAddChildAuthorization( bag, selectedGroup, canEditBlock );
                }
                else
                {
                    selectedGroup = null;
                }
            }

            var expandedValue = RequestContext.GetPageParameter( PageParameterKey.ExpandedIds );
            if ( expandedValue.IsNotNullOrWhiteSpace() )
            {
                foreach ( var key in expandedValue.SplitDelimitedValues() )
                {
                    var expandedGroup = GroupCache.Get( key, !PageCache.Layout.Site.DisablePredictableIds );
                    if ( expandedGroup != null && !bag.ExpandedGroupGuids.Contains( expandedGroup.Guid ) )
                    {
                        bag.ExpandedGroupGuids.Add( expandedGroup.Guid );
                    }
                }
            }

            // Auto-select the first authorized group only when the page has no GroupId
            // (WebForms OnInit). Do not auto-select when GroupId is present but invalid,
            // is "0" (add mode), or was cleared by the include/exclude type walk above.
            var disableAutoSelect = GetAttributeValue( AttributeKey.DisableAutoSelectFirstGroup ).AsBoolean();
            if ( selectedGroup == null && !disableAutoSelect && groupKey.IsNullOrWhiteSpace() )
            {
                var firstGroup = FindFirstGroup( bag.HideInactiveGroups, bag.LimitToPublic, bag.CampusGuid, bag.IncludeNoCampus );
                if ( firstGroup != null )
                {
                    // Auto-select stays on the current page (only reflects the first group in the
                    // URL); it must never redirect to a separate Detail Page on load.
                    var autoSelectUrl = GetNavigationUrl( firstGroup.Guid, Guid.Empty, bag.ExpandedGroupGuids, out _, forceCurrentPage: true );
                    if ( autoSelectUrl.IsNotNullOrWhiteSpace() )
                    {
                        /*
                            7/13/26 - MSE

                            Redirect on the server (like the WebForms OnInit Response.Redirect) so the
                            page loads once with the first group selected, instead of rendering the
                            unselected tree and then doing a second client-side navigation. AutoSelectUrl
                            is still returned as a client-side fallback for non-page-render contexts
                            (e.g. a block reload triggered by a settings change).

                            Reason: Avoid the double page load on initial navigation to the tree.
                        */
                        RequestContext.Response.RedirectToUrl( autoSelectUrl );
                        bag.AutoSelectUrl = autoSelectUrl;
                    }
                }
            }

            return bag;
        }

        /// <summary>
        /// Applies add-child visibility rules based on the selected group's group type and authorization.
        /// </summary>
        /// <param name="bag">The bag to update.</param>
        /// <param name="selectedGroup">The currently selected group.</param>
        /// <param name="canEditBlock">Whether the person has EDIT on the block.</param>
        private void ApplyAddChildAuthorization( GroupTreeViewBag bag, GroupCache selectedGroup, bool canEditBlock )
        {
            var selectedGroupType = GroupTypeCache.Get( selectedGroup.GroupTypeId );
            if ( selectedGroupType == null )
            {
                return;
            }

            var canHaveAllowedChildren = selectedGroupType.AllowAnyChildGroupType
                || selectedGroupType.ChildGroupTypes.Any( c => IsGroupTypeIncluded( c.Id ) );

            if ( !canHaveAllowedChildren )
            {
                return;
            }

            var showAddChild = canEditBlock;

            if ( !showAddChild )
            {
                showAddChild = selectedGroup.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
            }

            if ( !showAddChild )
            {
                List<GroupTypeCache> allowedChildGroupTypes;
                if ( selectedGroupType.AllowAnyChildGroupType )
                {
                    allowedChildGroupTypes = GroupTypeCache.All().ToList();
                }
                else
                {
                    allowedChildGroupTypes = selectedGroupType.ChildGroupTypes;
                }

                foreach ( var childGroupType in allowedChildGroupTypes )
                {
                    if ( childGroupType != null && childGroupType.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                    {
                        showAddChild = true;
                        break;
                    }
                }
            }

            bag.IsAddChildEnabled = showAddChild;
            bag.IsAddGroupVisible = canEditBlock || showAddChild;
        }

        /// <summary>
        /// Builds the navigate-mode URL for a selected (or to-be-added) group.
        /// </summary>
        /// <param name="groupGuid">The selected group, or an empty Guid when adding.</param>
        /// <param name="parentGuid">The parent group, used when adding a child.</param>
        /// <param name="expandedGuids">The groups currently expanded in the tree.</param>
        /// <param name="error">When this returns, indicates whether the URL could not be built.</param>
        /// <param name="forceCurrentPage">
        /// When <c>true</c>, always builds the current-page URL and ignores the Detail Page.
        /// Used for auto-select so arriving at the tree page never redirects to a separate
        /// Detail Page on load; only a deliberate selection navigates there.
        /// </param>
        /// <returns>The Detail Page URL (or current-page URL) with the page parameters applied.</returns>
        private string GetNavigationUrl( Guid groupGuid, Guid parentGuid, List<Guid> expandedGuids, out ErrorPouch error, bool forceCurrentPage = false )
        {
            error = new ErrorPouch();
            expandedGuids = expandedGuids ?? new List<Guid>();

            var qryParams = new Dictionary<string, string>();

            if ( groupGuid == Guid.Empty )
            {
                // An add action targets a new group; the Detail Page treats GroupId=0 as "new".
                qryParams[PageParameterKey.GroupId] = "0";
            }
            else
            {
                var selectedGroup = GroupCache.Get( groupGuid );
                if ( selectedGroup == null )
                {
                    error = new ErrorPouch { IsError = true, Message = "The selected group could not be found." };
                    return string.Empty;
                }

                qryParams[PageParameterKey.GroupId] = selectedGroup.IdKey;
            }

            // Resolve the parent for add actions.
            GroupCache parentGroup = null;
            if ( parentGuid != Guid.Empty )
            {
                parentGroup = GroupCache.Get( parentGuid );
            }
            else if ( groupGuid == Guid.Empty )
            {
                // Adding a top-level group parents it under the configured Root Group when one is set.
                var rootGroupGuid = GetAttributeValue( AttributeKey.RootGroup ).AsGuidOrNull();
                if ( rootGroupGuid.HasValue )
                {
                    parentGroup = GroupCache.Get( rootGroupGuid.Value );
                }
            }

            if ( parentGroup != null )
            {
                qryParams[PageParameterKey.ParentGroupId] = parentGroup.IdKey;
            }

            var expandedIds = expandedGuids
                .Select( guid => GroupCache.Get( guid )?.IdKey )
                .Where( idKey => idKey.IsNotNullOrWhiteSpace() )
                .Distinct()
                .ToList();

            if ( expandedIds.Any() )
            {
                qryParams[PageParameterKey.ExpandedIds] = string.Join( ",", expandedIds );
            }

            var detailPageReference = new PageReference( GetAttributeValue( AttributeKey.DetailPage ) );
            if ( forceCurrentPage || detailPageReference.PageId <= 0 || detailPageReference.PageId == PageCache.Id )
            {
                return this.GetCurrentPageUrl( qryParams );
            }

            return this.GetLinkedPageUrl( AttributeKey.DetailPage, qryParams );
        }

        /// <summary>
        /// Finds the first authorized group matching the current tree filters, ordered by name.
        /// </summary>
        /// <param name="hideInactiveGroups">Whether inactive groups are hidden.</param>
        /// <param name="limitToPublic">Whether only public groups are considered.</param>
        /// <param name="campusGuid">Optional campus filter.</param>
        /// <param name="includeNoCampus">Whether groups with no campus are included under a campus filter.</param>
        /// <returns>The first authorized group, or null when none is found.</returns>
        private Rock.Model.Group FindFirstGroup( bool hideInactiveGroups, bool limitToPublic, Guid? campusGuid, bool includeNoCampus )
        {
            var includedGroupTypeIds = GetGroupTypeIds( AttributeKey.GroupTypesInclude );
            var excludedGroupTypeIds = GetGroupTypeIds( AttributeKey.GroupTypesExclude );
            var limitToShowInNavigation = !includedGroupTypeIds.Any();
            var rootGroupId = 0;
            var rootGroupGuid = GetAttributeValue( AttributeKey.RootGroup ).AsGuidOrNull();
            if ( rootGroupGuid.HasValue )
            {
                rootGroupId = GroupCache.Get( rootGroupGuid.Value )?.Id ?? 0;
            }

            var campusId = 0;
            if ( campusGuid.HasValue )
            {
                campusId = CampusCache.Get( campusGuid.Value )?.Id ?? 0;
            }

            var groupService = new GroupService( RockContext );
            var qry = groupService.GetChildren(
                0,
                rootGroupId,
                GetAttributeValue( AttributeKey.LimitToSecurityRoleGroups ).AsBoolean(),
                includedGroupTypeIds,
                excludedGroupTypeIds,
                !hideInactiveGroups,
                limitToShowInNavigation,
                campusId,
                includeNoCampus,
                limitToPublic );

            foreach ( var group in qry.OrderBy( g => g.Name ) )
            {
                if ( group.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                {
                    return group;
                }
            }

            return null;
        }

        /// <summary>
        /// Determines whether the given group type is allowed by the block's include/exclude settings.
        /// </summary>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <returns><c>true</c> when the group type is included in this tree.</returns>
        private bool IsGroupTypeIncluded( int groupTypeId )
        {
            var includeGroupTypes = GetGroupTypeIds( AttributeKey.GroupTypesInclude );
            var excludeGroupTypes = GetGroupTypeIds( AttributeKey.GroupTypesExclude );

            if ( includeGroupTypes.Any() )
            {
                return includeGroupTypes.Contains( groupTypeId );
            }

            if ( excludeGroupTypes.Any() )
            {
                return !excludeGroupTypes.Contains( groupTypeId );
            }

            return true;
        }

        /// <summary>
        /// Resolves group type Guids from a block attribute value.
        /// </summary>
        /// <param name="attributeKey">The attribute key holding delimited Guids.</param>
        /// <returns>The list of Guids (empty when none are configured).</returns>
        private List<Guid> GetGroupTypeGuids( string attributeKey )
        {
            return GetAttributeValue( attributeKey )
                .SplitDelimitedValues()
                .Select( v => v.AsGuidOrNull() )
                .Where( g => g.HasValue )
                .Select( g => g.Value )
                .ToList();
        }

        /// <summary>
        /// Resolves group type Ids from a block attribute value via cache.
        /// </summary>
        /// <param name="attributeKey">The attribute key holding delimited Guids.</param>
        /// <returns>The list of Ids (empty when none are configured).</returns>
        private List<int> GetGroupTypeIds( string attributeKey )
        {
            return GetGroupTypeGuids( attributeKey )
                .Select( guid => GroupTypeCache.Get( guid )?.Id ?? 0 )
                .Where( id => id != 0 )
                .ToList();
        }

        /// <summary>
        /// Validates that the selected group and every ancestor are allowed by the block's
        /// include/exclude group-type settings, and adds those ancestors to the expanded set
        /// so a deep-link selection opens the tree far enough to reveal it. Stops at the
        /// configured root group (if any) and guards against recursive parent loops.
        /// </summary>
        /// <param name="group">The selected group whose chain should be validated and expanded.</param>
        /// <param name="expandedGuids">The expanded-group set to add the ancestors to.</param>
        /// <param name="rootGroupGuid">Optional root group Guid; walking stops when this node is reached.</param>
        /// <returns>
        /// <c>true</c> when the selection is valid for this tree; <c>false</c> when the selected
        /// group or any ancestor fails <see cref="IsGroupTypeIncluded"/> (caller should clear selection).
        /// </returns>
        private bool TryAddAncestorGroupGuids( GroupCache group, List<Guid> expandedGuids, Guid? rootGroupGuid )
        {
            if ( group == null )
            {
                return false;
            }

            var visited = new HashSet<Guid>();
            var current = group;

            while ( current != null )
            {
                if ( !IsGroupTypeIncluded( current.GroupTypeId ) )
                {
                    return false;
                }

                // Stop at the configured root; do not expand or walk above it.
                if ( rootGroupGuid.HasValue && current.Guid == rootGroupGuid.Value )
                {
                    break;
                }

                var parent = current.ParentGroup;
                if ( parent == null )
                {
                    break;
                }

                if ( !visited.Add( parent.Guid ) )
                {
                    // Parent list already contains this node — recursive loop; stop expanding.
                    break;
                }

                if ( !expandedGuids.Contains( parent.Guid ) )
                {
                    expandedGuids.Add( parent.Guid );
                }

                current = parent;
            }

            return true;
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Saves the person's hide-inactive-groups preference.
        /// </summary>
        /// <param name="hideInactiveGroups">Whether inactive groups should be hidden.</param>
        /// <returns>An OK result.</returns>
        [BlockAction]
        public BlockActionResult SetHideInactiveGroups( bool hideInactiveGroups )
        {
            var preferences = GetBlockPersonPreferences();
            preferences.SetValue( PersonPreferenceKey.HideInactiveGroups, hideInactiveGroups.ToTrueFalse() );
            preferences.Save();

            return ActionOk();
        }

        /// <summary>
        /// Saves the person's limit-to-public preference.
        /// </summary>
        /// <param name="limitToPublic">Whether only public groups should be shown.</param>
        /// <returns>An OK result.</returns>
        [BlockAction]
        public BlockActionResult SetLimitToPublic( bool limitToPublic )
        {
            var preferences = GetBlockPersonPreferences();
            preferences.SetValue( PersonPreferenceKey.LimitToPublic, limitToPublic.ToTrueFalse() );
            preferences.Save();

            return ActionOk();
        }

        /// <summary>
        /// Saves or clears the person's counts-type preference.
        /// </summary>
        /// <param name="countsType">
        /// 0 = None, 1 = Child Groups, 2 = Group Members. Pass <c>null</c> to clear
        /// the preference so the Initial Count Setting block attribute applies again.
        /// </param>
        /// <returns>An OK result.</returns>
        [BlockAction]
        public BlockActionResult SetCountsType( int? countsType )
        {
            var preferences = GetBlockPersonPreferences();
            preferences.SetValue(
                PersonPreferenceKey.CountsType,
                countsType.HasValue ? countsType.Value.ToString() : string.Empty );
            preferences.Save();

            return ActionOk();
        }

        /// <summary>
        /// Saves the block-type campus filter preference.
        /// </summary>
        /// <param name="campusGuid">The campus Guid, or null to clear the filter.</param>
        /// <returns>An OK result.</returns>
        [BlockAction]
        public BlockActionResult SetCampusFilter( Guid? campusGuid )
        {
            var typePreferences = GetBlockTypePersonPreferences();
            typePreferences.SetValue( PersonPreferenceKey.CampusFilter, campusGuid?.ToString() ?? string.Empty );
            typePreferences.Save();

            return ActionOk();
        }

        /// <summary>
        /// Saves the block-type include-no-campus preference.
        /// </summary>
        /// <param name="includeNoCampus">Whether groups with no campus are included under a campus filter.</param>
        /// <returns>An OK result.</returns>
        [BlockAction]
        public BlockActionResult SetIncludeNoCampus( bool includeNoCampus )
        {
            var typePreferences = GetBlockTypePersonPreferences();
            typePreferences.SetValue( PersonPreferenceKey.IncludeNoCampus, includeNoCampus.ToTrueFalse() );
            typePreferences.Save();

            return ActionOk();
        }

        /// <summary>
        /// Builds the navigate-mode URL for a group selection or add action.
        /// </summary>
        /// <param name="groupGuid">The selected group, or an empty Guid when adding.</param>
        /// <param name="parentGuid">The parent group, used when adding a child.</param>
        /// <param name="expandedGuids">The groups currently expanded in the tree.</param>
        /// <returns>The navigation URL, or a bad-request result when it could not be built.</returns>
        [BlockAction]
        public BlockActionResult GetNavigationUrl( Guid groupGuid, Guid parentGuid, List<Guid> expandedGuids )
        {
            var url = GetNavigationUrl( groupGuid, parentGuid, expandedGuids, out var error );

            if ( url.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( error.IsError ? error.Message : "Could not determine the navigation URL for the provided group." );
            }

            return ActionOk( url );
        }

        #endregion Block Actions

        #region Helper Classes

        private class ErrorPouch
        {
            public bool IsError { get; set; } = false;

            public string Message { get; set; } = string.Empty;
        }

        #endregion Helper Classes
    }
}

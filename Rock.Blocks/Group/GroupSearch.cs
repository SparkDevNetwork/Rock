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

using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Group.GroupSearch;
using Rock.Web.Cache;

namespace Rock.Blocks.Group
{
    /// <summary>
    /// Handles displaying group search results and navigates to the group detail page when only one match was found.
    /// </summary>
    [DisplayName( "Group Search" )]
    [Category( "Groups" )]
    [Description( "Handles displaying group search results and navigates to the group detail page when only one match was found." )]
    [IconCssClass( "ti ti-search" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [CodeEditorField( "Group URL Format",
        Description = "The URL to use for linking to a group. <span class='tip tip-lava'></span>",
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Lava,
        EditorHeight = 200,
        IsRequired = false,
        DefaultValue = "~/Group/{{ Group.IdKey }}",
        Key = AttributeKey.GroupUrlFormat )]

    [Rock.SystemGuid.EntityTypeGuid( "758D1E2F-9E88-41B6-A444-09828904D8DE" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "163B418E-63F6-454F-83AF-C2BFA6BA53FA" )]
    [Rock.SystemGuid.BlockTypeGuid( "F1E188A5-2F9D-4BA6-BCA1-82B2450DAC1C" )]
    [CustomizedGrid]
    public class GroupSearch : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string GroupUrlFormat = "GroupURLFormat";
        }

        private static class PageParameterKey
        {
            public const string SearchType = "SearchType";
            public const string SearchTerm = "SearchTerm";
        }

        private static class SearchTypeValue
        {
            public const string Name = "name";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return new ListBlockBox<GroupSearchOptionsBag>
            {
                IsAddEnabled = false,
                IsDeleteEnabled = false,
                ExpectedRowCount = null,
                Options = GetBoxOptions(),
                GridDefinition = GetGridBuilder().BuildDefinition()
            };
        }

        /// <summary>
        /// Gets the box options required for the component to render the list,
        /// including a single-result redirect URL when applicable.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private GroupSearchOptionsBag GetBoxOptions()
        {
            var options = new GroupSearchOptionsBag();

            // Only the single-result check is needed, so avoid materializing the entire match set.
            var matchingGroups = GetMatchingGroupsQueryable()?.Take( 2 ).ToList();

            if ( matchingGroups != null && matchingGroups.Count == 1 )
            {
                var commonMergeFields = RequestContext.GetCommonMergeFields( RequestContext.CurrentPerson );
                options.RedirectUrl = ResolveGroupUrl( matchingGroups[0], commonMergeFields );
            }

            // Redirect server-side so a single-result search never paints the (empty) grid first.
            // RedirectUrl is still returned on the bag as a client-side fallback.
            if ( options.RedirectUrl.IsNotNullOrWhiteSpace() )
            {
                RequestContext.Response.RedirectToUrl( options.RedirectUrl );
            }

            return options;
        }

        /// <summary>
        /// Gets the grid builder that describes the search result columns.
        /// </summary>
        /// <returns>A configured grid builder for group search results.</returns>
        private GridBuilder<GroupSearchResultBag> GetGridBuilder()
        {
            return new GridBuilder<GroupSearchResultBag>()
                .WithBlock( this )
                .AddField( "id", r => r.Id )
                .AddTextField( "name", r => r.Name )
                .AddTextField( "structure", r => r.Structure )
                .AddTextField( "groupType", r => r.GroupType )
                .AddField( "memberCount", r => r.MemberCount )
                .AddTextField( "campus", r => r.Campus )
                .AddTextField( "url", r => r.Url );
        }

        /// <summary>
        /// Gets the queryable of groups that match the current page-parameter search.
        /// </summary>
        /// <returns>A queryable of matching groups, or <c>null</c> when no search should run.</returns>
        private IQueryable<Model.Group> GetMatchingGroupsQueryable()
        {
            var searchType = PageParameter( PageParameterKey.SearchType );
            var searchTerm = PageParameter( PageParameterKey.SearchTerm );

            if ( searchType.IsNullOrWhiteSpace()
                || searchTerm.IsNullOrWhiteSpace()
                || searchTerm.IsSingleSpecialCharacter() )
            {
                return null;
            }

            if ( !string.Equals( searchType.Trim(), SearchTypeValue.Name, StringComparison.OrdinalIgnoreCase ) )
            {
                return null;
            }

            return new GroupService( RockContext )
                .Queryable()
                .Where( g =>
                    g.GroupType.ShowInNavigation
                    && g.Name.Contains( searchTerm ) )
                .OrderBy( g => g.Order )
                .ThenBy( g => g.Name );
        }

        /// <summary>
        /// Gets the current group search results.
        /// </summary>
        /// <returns>The search results.</returns>
        private List<GroupSearchResultBag> GetSearchResults()
        {
            var matchingQueryable = GetMatchingGroupsQueryable();
            var groups = matchingQueryable?.ToList() ?? new List<Model.Group>();

            if ( !groups.Any() )
            {
                return new List<GroupSearchResultBag>();
            }

            var commonMergeFields = RequestContext.GetCommonMergeFields( RequestContext.CurrentPerson );
            var memberCounts = GetMemberCounts( groups.Select( g => g.Id ).ToList() );
            var groupLookup = BuildGroupLookup( groups );

            // Cache Lava-resolved URLs so shared ancestors are not re-resolved for every row.
            var urlCache = new Dictionary<int, string>();

            return groups
                .Select( g => new GroupSearchResultBag
                {
                    Id = g.Id,
                    Name = g.Name,
                    Structure = BuildParentStructure( g, groupLookup, commonMergeFields, urlCache ),
                    GroupType = GroupTypeCache.Get( g.GroupTypeId )?.Name,
                    MemberCount = memberCounts.GetValueOrDefault( g.Id, 0 ),
                    Campus = g.CampusId.HasValue ? CampusCache.Get( g.CampusId.Value )?.Name : null,
                    Url = GetCachedGroupUrl( g, commonMergeFields, urlCache )
                } )
                .ToList();
        }

        /// <summary>
        /// Loads member counts for the given group identifiers in a single query.
        /// </summary>
        /// <remarks>
        /// Uses an unfiltered member query so the count matches the entity collection
        /// size (all members for the group), not the default GroupMemberService filters
        /// that exclude deceased and archived records.
        /// </remarks>
        /// <param name="groupIds">The group identifiers to count members for.</param>
        /// <returns>A dictionary of group Id to member count.</returns>
        private Dictionary<int, int> GetMemberCounts( List<int> groupIds )
        {
            if ( !groupIds.Any() )
            {
                return new Dictionary<int, int>();
            }

            return new GroupMemberService( RockContext )
                .AsNoFilter()
                .Where( m => groupIds.Contains( m.GroupId ) )
                .GroupBy( m => m.GroupId )
                .Select( g => new
                {
                    GroupId = g.Key,
                    Count = g.Count()
                } )
                .ToDictionary( x => x.GroupId, x => x.Count );
        }

        /// <summary>
        /// Builds a lookup of the matched groups plus all of their ancestors for hierarchy rendering.
        /// </summary>
        /// <param name="matchedGroups">The groups returned by the search.</param>
        /// <returns>A dictionary of group Id to group.</returns>
        private Dictionary<int, Model.Group> BuildGroupLookup( List<Model.Group> matchedGroups )
        {
            var groupLookup = matchedGroups.ToDictionary( g => g.Id, g => g );
            var groupService = new GroupService( RockContext );

            var parentIdsToLoad = matchedGroups
                .Where( g => g.ParentGroupId.HasValue && !groupLookup.ContainsKey( g.ParentGroupId.Value ) )
                .Select( g => g.ParentGroupId.Value )
                .Distinct()
                .ToList();

            while ( parentIdsToLoad.Any() )
            {
                // Use AsNoFilter so archived parents still appear in the hierarchy path
                // the same way a lazy-loaded ParentGroup navigation would.
                var parents = groupService.AsNoFilter()
                    .Where( g => parentIdsToLoad.Contains( g.Id ) )
                    .ToList();

                foreach ( var parent in parents )
                {
                    groupLookup[parent.Id] = parent;
                }

                parentIdsToLoad = parents
                    .Where( g => g.ParentGroupId.HasValue && !groupLookup.ContainsKey( g.ParentGroupId.Value ) )
                    .Select( g => g.ParentGroupId.Value )
                    .Distinct()
                    .ToList();
            }

            return groupLookup;
        }

        /// <summary>
        /// Builds the parent hierarchy HTML that appears before the matched group link.
        /// </summary>
        /// <param name="group">The matched group.</param>
        /// <param name="groupLookup">Lookup of groups available for parent resolution.</param>
        /// <param name="commonMergeFields">The merge fields shared across all rows.</param>
        /// <param name="urlCache">Per-request cache of resolved group URLs.</param>
        /// <returns>A formatted parent structure string.</returns>
        private string BuildParentStructure( Model.Group group, Dictionary<int, Model.Group> groupLookup, Dictionary<string, object> commonMergeFields, Dictionary<int, string> urlCache )
        {
            if ( group?.ParentGroupId == null || !groupLookup.TryGetValue( group.ParentGroupId.Value, out var parentGroup ) )
            {
                return string.Empty;
            }

            return BuildStructurePath( parentGroup, groupLookup, commonMergeFields, urlCache, null );
        }

        /// <summary>
        /// Recursively builds a linked hierarchy path for the given group and its ancestors.
        /// </summary>
        /// <param name="group">The group at the current path segment.</param>
        /// <param name="groupLookup">Lookup of groups available for parent resolution.</param>
        /// <param name="commonMergeFields">The merge fields shared across all rows.</param>
        /// <param name="urlCache">Per-request cache of resolved group URLs.</param>
        /// <param name="parentIds">The groups already visited while building the tree path.</param>
        /// <returns>A formatted structure string for this group and its ancestors.</returns>
        private string BuildStructurePath( Model.Group group, Dictionary<int, Model.Group> groupLookup, Dictionary<string, object> commonMergeFields, Dictionary<int, string> urlCache, List<int> parentIds )
        {
            if ( group == null )
            {
                return string.Empty;
            }

            // Create or add this node to the history stack for this tree walk.
            if ( parentIds == null )
            {
                parentIds = new List<int>();
            }
            else
            {
                // If we have encountered this node before during this tree walk, we have found an infinite recursion in the tree.
                // Truncate the path with an error message and exit.
                if ( parentIds.Contains( group.Id ) )
                {
                    return "#Invalid-Parent-Reference#";
                }
            }

            parentIds.Add( group.Id );

            var prefix = string.Empty;

            if ( group.ParentGroupId.HasValue && groupLookup.TryGetValue( group.ParentGroupId.Value, out var parentGroup ) )
            {
                prefix = BuildStructurePath( parentGroup, groupLookup, commonMergeFields, urlCache, parentIds );
            }

            if ( !string.IsNullOrWhiteSpace( prefix ) )
            {
                prefix += " <i class='ti ti-chevron-right'></i> ";
            }

            var url = GetCachedGroupUrl( group, commonMergeFields, urlCache );
            return $"{prefix}<a href='{url}'>{group.Name}</a>";
        }

        /// <summary>
        /// Resolves and caches the group details URL for the current request.
        /// </summary>
        /// <param name="group">The group that will be linked to.</param>
        /// <param name="commonMergeFields">The merge fields shared across all rows.</param>
        /// <param name="urlCache">Per-request cache of resolved group URLs.</param>
        /// <returns>The resolved group details URL.</returns>
        private string GetCachedGroupUrl( Model.Group group, Dictionary<string, object> commonMergeFields, Dictionary<int, string> urlCache )
        {
            if ( urlCache.TryGetValue( group.Id, out var cachedUrl ) )
            {
                return cachedUrl;
            }

            var url = ResolveGroupUrl( group, commonMergeFields );
            urlCache[group.Id] = url;
            return url;
        }

        /// <summary>
        /// Resolves the group details URL using the configured Lava format.
        /// </summary>
        /// <param name="group">The group that will be linked to.</param>
        /// <param name="commonMergeFields">The merge fields shared across all rows.</param>
        /// <returns>The resolved group details URL.</returns>
        private string ResolveGroupUrl( Model.Group group, Dictionary<string, object> commonMergeFields )
        {
            var mergeFields = new Dictionary<string, object>( commonMergeFields )
            {
                ["Group"] = group
            };

            var url = GetAttributeValue( AttributeKey.GroupUrlFormat ).ResolveMergeFields( mergeFields );
            return RequestContext.ResolveRockUrl( url );
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Gets the group search result grid data.
        /// </summary>
        /// <returns>A bag containing the group search grid data.</returns>
        [BlockAction]
        public BlockActionResult GetGridData()
        {
            var results = GetSearchResults();
            var gridDataBag = GetGridBuilder().Build( results );

            return ActionOk( gridDataBag );
        }

        #endregion Block Actions
    }
}

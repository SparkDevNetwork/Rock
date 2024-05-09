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
using Rock.Data;
using Rock.Lava.RockLiquid.Blocks;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Reporting.DataFilter.Group;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Group.GroupArchivedList;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Group
{
    /// <summary>
    /// Displays a list of groups.
    /// </summary>
    [DisplayName( "Group Archived List" )]
    [Category( "Utility" )]
    [Description( "Lists Groups that have been archived." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "b67a0c89-1550-4960-8aaf-baa713be3277" )]
    [Rock.SystemGuid.BlockTypeGuid( "972ad143-8294-4462-b2a7-1b36ea127374" )]
    [CustomizedGrid]
    public class GroupArchivedList : RockEntityListBlockType<Rock.Model.Group>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class PreferenceKey
        {
            public const string FilterGroupType = "Group Type";
            public const string FilterGroupName = "Group Name";
        }

        #endregion Keys

        #region Properties

        /// <summary>
        /// Gets the group type of the groups to be included in the results.
        /// </summary>
        /// <value>
        /// The type of the filter group.
        /// </value>
        protected Guid? FilterGroupType => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterGroupType )
            .FromJsonOrNull<ListItemBag>()?.Value?.AsGuidOrNull();

        /// <summary>
        /// Gets the name of the groups to be included in the results.
        /// </summary>
        /// <value>
        /// The name of the filter group.
        /// </value>
        protected string FilterGroupName => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterGroupName );

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<GroupArchivedListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = false;
            box.IsDeleteEnabled = false;
            box.ExpectedRowCount = null;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private GroupArchivedListOptionsBag GetBoxOptions()
        {
            var options = new GroupArchivedListOptionsBag();

            options.GroupTypeGuids = new GroupTypeService( new RockContext() ).AsNoFilter()
                .Where( a => a.Groups.Any( x => x.IsArchived ) )
                .OrderBy( a => a.Name )
                .AsNoTracking()
                .Select( g => g.Guid )
                .ToList();

            return options;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "GroupId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<Rock.Model.Group> GetListQueryable( RockContext rockContext )
        {
            var queryable = new GroupService( rockContext ).GetArchived()
                .Include( a => a.ArchivedByPersonAlias );

            if ( FilterGroupType.HasValue )
            {
                queryable = queryable.Where( a => a.GroupType.Guid == FilterGroupType.Value );
            }

            if ( !string.IsNullOrWhiteSpace( FilterGroupName ) )
            {
                queryable = queryable.Where( a => a.Name.Contains( FilterGroupName ) );
            }

            return queryable;
        }

        protected override IQueryable<Model.Group> GetOrderedListQueryable( IQueryable<Model.Group> queryable, RockContext rockContext )
        {
            return queryable.OrderByDescending( g => g.ArchivedDateTime );
        }

        /// <inheritdoc/>
        protected override GridBuilder<Rock.Model.Group> GetGridBuilder()
        {
            return new GridBuilder<Rock.Model.Group>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "groupType", a => a.GroupType.Name )
                .AddTextField( "name", a => a.Name )
                .AddTextField( "description", a => a.Description )
                .AddDateTimeField( "createdDate", a => a.CreatedDateTime )
                .AddDateTimeField( "archivedDate", a => a.ArchivedDateTime )
                .AddPersonField( "archivedBy", a => a.ArchivedByPersonAlias?.Person )
                .AddField( "isSystem", a => a.IsSystem );
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Unarchives the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be unarchived.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult UndoArchive( string key )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new GroupService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{Rock.Model.Group.FriendlyTypeName} not found." );
                }

                if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to edit ${Rock.Model.Group.FriendlyTypeName}." );
                }

                entity.IsArchived = false;
                entity.ArchivedByPersonAliasId = null;
                entity.ArchivedDateTime = null;

                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        #endregion
    }
}

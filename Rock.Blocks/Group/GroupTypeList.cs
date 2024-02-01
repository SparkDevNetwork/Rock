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
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Group.GroupTypeList;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Group
{
    /// <summary>
    /// Displays a list of group types.
    /// </summary>
    [DisplayName( "Group Type List" )]
    [Category( "Group" )]
    [Description( "Displays a list of group types." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the group type details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.EntityTypeGuid( "562ed873-bd66-4287-ae9f-d7c43fecd7a8" )]
    [Rock.SystemGuid.BlockTypeGuid( "8885f47d-9262-48b0-b969-9bee003370eb" )]
    [CustomizedGrid]
    public class GroupTypeList : RockEntityListBlockType<GroupType>
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
            public const string FilterPurpose = "filter-purpose";
            public const string FilterSystemGroupTypes = "filter-system-group-types";
            public const string FilterShowInNavigation = "filter-show-in-navigation";
        }

        #endregion Keys

        #region Properties

        /// <summary>
        /// Gets purpose guid with which to filter the results.
        /// </summary>
        /// <value>
        /// The name of the account.
        /// </value>
        protected Guid? FilterPurpose => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterPurpose )
            .FromJsonOrNull<ListItemBag>()?.Value?.AsGuidOrNull();

        /// <summary>
        /// If yes only system group types are returned.
        /// </summary>
        /// <value>
        /// The name of the account.
        /// </value>
        protected bool? FilterSystemGroupTypes => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterSystemGroupTypes )
            .AsBooleanOrNull();

        /// <summary>
        /// If yes only group types with ShowInNavigation set to true are returned.
        /// </summary>
        /// <value>
        /// The name of the account.
        /// </value>
        protected bool? FilterShowInNavigation => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterShowInNavigation )
            .AsBooleanOrNull();

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<GroupTypeListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = GetIsAddEnabled();
            box.IsDeleteEnabled = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
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
        private GroupTypeListOptionsBag GetBoxOptions()
        {
            var options = new GroupTypeListOptionsBag();

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEnabled()
        {
            return BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "GroupTypeId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<GroupType> GetListQueryable( RockContext rockContext )
        {
            var queryable = base.GetListQueryable( rockContext );

            if ( FilterPurpose.HasValue )
            {
                queryable = queryable.Where( t => t.GroupTypePurposeValue.Guid == FilterPurpose.Value );
            }

            if ( FilterSystemGroupTypes.HasValue )
            {
                queryable = FilterSystemGroupTypes.Value ? queryable.Where( t => t.IsSystem ) : queryable.Where( t => !t.IsSystem );
            }

            if ( FilterShowInNavigation.HasValue )
            {
                queryable = FilterShowInNavigation.Value ? queryable.Where( t => t.ShowInNavigation ) : queryable.Where( t => !t.ShowInNavigation );
            }

            return queryable;
        }

        /// <inheritdoc/>
        protected override IQueryable<GroupType> GetOrderedListQueryable( IQueryable<GroupType> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( g => g.Order ).ThenBy( g => g.Name );
        }

        /// <inheritdoc/>
        protected override GridBuilder<GroupType> GetGridBuilder()
        {
            return new GridBuilder<GroupType>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "purpose", a => a.GroupTypePurposeValue?.Value )
                .AddField( "groupsCount", a => a.Groups.Count )
                .AddTextField( "name", a => a.Name )
                .AddField( "showInNavigation", a => a.ShowInNavigation )
                .AddField( "isSystem", a => a.IsSystem )
                .AddField( "isSecurityDisabled", a => !a.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) );
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Changes the ordered position of a single item.
        /// </summary>
        /// <param name="key">The identifier of the item that will be moved.</param>
        /// <param name="beforeKey">The identifier of the item it will be placed before.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult ReorderItem( string key, string beforeKey )
        {
            using ( var rockContext = new RockContext() )
            {
                // Get the queryable and make sure it is ordered correctly.
                var qry = GetListQueryable( rockContext );
                qry = GetOrderedListQueryable( qry, rockContext );

                // Get the entities from the database.
                var items = GetListItems( qry, rockContext );

                if ( !items.ReorderEntity( key, beforeKey ) )
                {
                    return ActionBadRequest( "Invalid reorder attempt." );
                }

                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new GroupTypeService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{GroupType.FriendlyTypeName} not found." );
                }

                if ( !entity.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete ${GroupType.FriendlyTypeName}." );
                }

                if ( !entityService.CanDelete( entity, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                entity.ParentGroupTypes.Clear();
                entity.ChildGroupTypes.Clear();

                entityService.Delete( entity );
                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        #endregion
    }
}

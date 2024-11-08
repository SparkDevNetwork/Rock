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

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Tv.AppleTvPageList;
using Rock.Web.Cache;

namespace Rock.Blocks.Tv
{
    /// <summary>
    /// Displays a list of pages.
    /// </summary>
    [DisplayName( "Apple TV Page List" )]
    [Category( "TV > TV Apps" )]
    [Description( "Lists pages for TV apps (Apple or other)." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the page details.",
        Key = AttributeKey.DetailPage )]

    [SystemGuid.EntityTypeGuid( "4e89a96e-88a2-4ca4-a86b-b9ffdcacf49f" )]
    [SystemGuid.BlockTypeGuid( "a759218b-1c72-446c-8994-8559ba72941e" )]
    [CustomizedGrid]
    public class AppleTvPageList : RockEntityListBlockType<Page>
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

        private static class PageParameterKey
        {
            public const string SiteId = "SiteId";
            public const string SitePageId = "SitePageId";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<AppleTvPageListOptionsBag>();
            var builder = GetGridBuilder();

            var isAddDeleteEnabled = GetIsAddDeleteEnabled();
            box.IsAddEnabled = isAddDeleteEnabled;
            box.IsDeleteEnabled = isAddDeleteEnabled;
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
        private AppleTvPageListOptionsBag GetBoxOptions()
        {
            var options = new AppleTvPageListOptionsBag()
            {
                DefaultPageIdKey = GetDefaultPageIdKey(),
                IsBlockVisible = GetSiteId() > 0,
            };

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddDeleteEnabled()
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
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, new Dictionary<string, string>()
                {
                    { PageParameterKey.SitePageId, "((Key))" },
                    { PageParameterKey.SiteId, PageParameter( PageParameterKey.SiteId ) },
                } )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<Page> GetListQueryable( RockContext rockContext )
        {
            var applicationId = GetSiteId();
            var queryable = new PageService( rockContext ).GetBySiteId( applicationId );
            return queryable;
        }

        /// <inheritdoc/>
        protected override IQueryable<Page> GetOrderedListQueryable( IQueryable<Page> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( p => p.Order ).ThenBy( p => p.InternalName );
        }

        /// <inheritdoc/>
        protected override GridBuilder<Page> GetGridBuilder()
        {
            return new GridBuilder<Page>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.InternalName )
                .AddTextField( "description", a => a.Description )
                .AddField( "rockCacheabilityType", a => a.CacheControlHeader?.RockCacheablityType )
                .AddField( "cacheSharedMaxAge", a => a.CacheControlHeader?.SharedMaxAge?.ToSeconds() )
                .AddField( "cacheMaxAge", a => a.CacheControlHeader?.MaxAge?.ToSeconds() )
                .AddField( "displayInNav", a => a.DisplayInNavWhen != DisplayInNavWhen.Never )
                .AddField( "isSecurityDisabled", a => !a.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) );
        }

        /// <summary>
        /// Gets the default page identifier key of the current application.
        /// </summary>
        /// <returns></returns>
        private string GetDefaultPageIdKey()
        {
            var applicationId = GetSiteId();
            return applicationId.HasValue ? SiteCache.Get( applicationId.Value )?.DefaultPage?.IdKey : string.Empty;
        }

        /// <summary>
        /// Gets the site identifier passed as a query parameter.
        /// </summary>
        /// <returns></returns>
        private int? GetSiteId()
        {
            var siteId = PageParameter( PageParameterKey.SiteId );
            return siteId.AsIntegerOrNull() ?? Rock.Utility.IdHasher.Instance.GetId( siteId );
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
                var entityService = new PageService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{Page.FriendlyTypeName} not found." );
                }

                if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete {Page.FriendlyTypeName}." );
                }

                if ( !entityService.CanDelete( entity, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                entityService.Delete( entity );
                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        #endregion
    }
}

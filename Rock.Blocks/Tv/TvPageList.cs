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
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Tv.TvPageList;
using Rock.Web.Cache;

namespace Rock.Blocks.Tv
{
    /// <summary>
    /// Displays a list of pages.
    /// </summary>

    [DisplayName( "TV Page List" )]
    [Category( "TV > TV Apps" )]
    [Description( "Displays a list of pages." )]
    [IconCssClass( "fa fa-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the page details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.EntityTypeGuid( "bfe024a8-bdf2-4f11-8266-8ae4f4ea483b" )]
    [Rock.SystemGuid.BlockTypeGuid( "11616362-6f7f-4b98-bc2a-dfd18ab983d9" )]
    [CustomizedGrid]
    public class TvPageList : RockEntityListBlockType<Page>
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string SiteId = "SiteId";
        }

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<TvPageListOptionsBag>();
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
        /// <returns>The options that provide additional details to the block.</returns>
        private TvPageListOptionsBag GetBoxOptions()
        {
            var options = new TvPageListOptionsBag();
            options.SiteId = PageParameter( PageParameterKey.SiteId );

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEnabled()
        {
            var entity = new Page();

            return entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var parameters = new Dictionary<string, string>
            {
                ["SitePageId"] = "((Key))",
                ["SiteId"] = "((SiteKey))"
            };

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, parameters )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<Page> GetListQueryable( RockContext rockContext )
        {
            var queryable = base.GetListQueryable( rockContext );
            var siteService = new SiteService( rockContext );
            var layoutService = new LayoutService( rockContext );

            // Join pages with their layouts.
            var layoutJoin = queryable.Join(
                layoutService.Queryable(),
                p => p.LayoutId,
                l => l.Id,
                ( p, l ) => new
                {
                    Page = p,
                    Layout = l
                } );

            // Join the layout with its site.
            var siteJoin = layoutJoin.Join(
                siteService.Queryable(),
                l => l.Layout.SiteId,
                s => s.Id,
                ( l, s ) => new
                {
                    Page = l.Page,
                    Site = s
                } );

            return siteJoin
                .Where( a => a.Site.SiteType == SiteType.Tv )
                .Select( a => a.Page );
        }

        /// <inheritdoc />
        protected override List<Page> GetListItems( IQueryable<Page> queryable, RockContext rockContext )
        {
            var listItems = base.GetListItems( queryable, rockContext );
            var siteIdKey = PageParameter( PageParameterKey.SiteId );

            var site = SiteCache.Get( siteIdKey, !PageCache.Layout.Site.DisablePredictableIds );
            if ( site == null ) {
                return listItems;
            }

            return listItems.Where( a => a.Layout.SiteId == site.Id ).ToList();
        }

        /// <inheritdoc/>
        protected override GridBuilder<Page> GetGridBuilder()
        {
            return new GridBuilder<Page>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.InternalName )
                .AddTextField( "description", a => a.Description )
                .AddTextField( "cacheSettings", a =>  GetCacheabilityType( a )?.ConvertToString() )
                .AddField( "displayInNav", a => a.DisplayInNavWhen == DisplayInNavWhen.WhenAllowed )
                .AddField( "isSystem", a => a.IsSystem )
                .AddField( "isSecurityDisabled", a => !a.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
                .AddAttributeFields( GetGridAttributes() );
        }

        /// <summary>
        /// Gets the cacheability type of the page from the cache control header settings.
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        private Enums.Controls.RockCacheabilityType? GetCacheabilityType( Page page )
        {
            var cacheability = page.CacheControlHeaderSettings.FromJsonOrNull<RockCacheability>();
            if( cacheability == null )
            {
                return null;
            }

            return cacheability.ToCacheabilityBag().RockCacheabilityType;
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
            // Get the queryable and make sure it is ordered correctly.
            var qry = GetListQueryable( RockContext );
            qry = GetOrderedListQueryable( qry, RockContext );

            // Get the entities from the database.
            var items = GetListItems( qry, RockContext );

            if ( !items.ReorderEntity( key, beforeKey ) )
            {
                return ActionBadRequest( "Invalid reorder attempt." );
            }

            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new PageService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{Page.FriendlyTypeName} not found." );
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete {Page.FriendlyTypeName}." );
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk();
        }

        #endregion
    }
}

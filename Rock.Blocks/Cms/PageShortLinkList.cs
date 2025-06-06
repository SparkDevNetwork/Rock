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
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Cms.PageShortLinkList;
using Rock.Web.Cache;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays a list of page short links.
    /// </summary>

    [DisplayName( "Page Short Link List" )]
    [Category( "CMS" )]
    [Description( "Displays a list of page short links." )]
    [IconCssClass( "fa fa-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the page short link details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.EntityTypeGuid( "b9825e53-d074-4280-a1a3-e20771e34625" )]
    [Rock.SystemGuid.BlockTypeGuid( "d25ff675-07c8-4e2d-a3fa-38ba3468b4ae" )]
    [CustomizedGrid]
    public class PageShortLinkList : RockListBlockType<PageShortLinkList.PageShortLinkWithClicks>
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

        #endregion Keys

        #region Fields

        /// <summary>
        /// The Short Link attributes that are configured to show on the grid.
        /// </summary>
        private readonly Lazy<List<AttributeCache>> _gridAttributes = new Lazy<List<AttributeCache>>( BuildGridAttributes );

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<PageShortLinkListOptionsBag>();
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
        private PageShortLinkListOptionsBag GetBoxOptions()
        {
            var options = new PageShortLinkListOptionsBag
            {
                SiteItems = SiteCache.All().OrderBy( s => s.Name ).ToListItemBagList()
            };

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
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, new Dictionary<string, string> { ["ShortLinkId"] = "((Key))" } )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<PageShortLinkWithClicks> GetListQueryable( RockContext rockContext )
        {
            var urlShortenerChannelTypeId = DefinedValueCache
                .Get( Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_URLSHORTENER )?.Id;

            // Get interaction counts grouped by ShortLink ID
            var interactionCounts = new InteractionService( rockContext )
                .Queryable().AsNoTracking()
                .Where( i =>
                    i.InteractionComponent.InteractionChannel.ChannelTypeMediumValueId == urlShortenerChannelTypeId &&
                    i.InteractionComponent.EntityId.HasValue )
                .GroupBy( i => i.InteractionComponent.EntityId.Value )
                .Select( g => new
                {
                    ShortLinkId = g.Key,
                    ClickCount = g.Count()
                } );

            var pageShortLinkService = new PageShortLinkService( RockContext );

            var queryable = pageShortLinkService.Queryable()
                .GroupJoin(
                    interactionCounts,
                    shortLink => shortLink.Id,
                    ic => ic.ShortLinkId,
                    ( shortLink, counts ) => new
                    {
                        PageShortLink = shortLink,
                        ClickCount = counts.Select( c => c.ClickCount ).FirstOrDefault()
                    } )
                .Select( x => new PageShortLinkWithClicks
                {
                    PageShortLink = x.PageShortLink,
                    ClickCount = x.ClickCount
                } );

            return queryable;
        }

        /// <inheritdoc/>
        protected override List<PageShortLinkWithClicks> GetListItems( IQueryable<PageShortLinkWithClicks> queryable, RockContext rockContext )
        {
            // Load all the Short Links into memory.
            var items = queryable.ToList();

            // Get all SiteIds referenced by the short links
            var siteIds = items
                .Select( x => x.PageShortLink.SiteId )
                .Distinct()
                .ToList();

            // Now query the Sites by SiteId
            var sitesById = new SiteService( rockContext )
                .Queryable()
                .Include( s => s.SiteDomains )
                .Where( s => siteIds.Contains( s.Id ) )
                .ToDictionary( s => s.Id );

            foreach ( var item in items )
            {
                if ( sitesById.TryGetValue( item.PageShortLink.SiteId, out var site ) )
                {
                    item.PageShortLink.Site = site;
                }
            }

            return items;
        }

        /// <inheritdoc/>
        protected override GridBuilder<PageShortLinkWithClicks> GetGridBuilder()
        {
            return new GridBuilder<PageShortLinkWithClicks>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.PageShortLink.IdKey )
                .AddTextField( "url", a => a.PageShortLink.Url )
                .AddTextField( "site", a => a.PageShortLink.Site?.Name )
                .AddTextField( "token", a => a.PageShortLink.Token )
                .AddField( "clickCount", a => a.ClickCount )
                .AddTextField( "shortLink", a => a.PageShortLink.ShortLinkUrl )
                .AddAttributeFieldsFrom( a => a.PageShortLink, _gridAttributes.Value );
        }

        /// <summary>
        /// Builds the list of grid attributes that should be included on the Grid.
        /// </summary>
        /// <remarks>
        /// The default implementation returns only attributes that are not qualified.
        /// </remarks>
        /// <returns>A list of <see cref="AttributeCache"/> objects.</returns>
        private static List<AttributeCache> BuildGridAttributes()
        {
            var entityTypeId = EntityTypeCache.Get<PageShortLink>( false )?.Id;

            if ( entityTypeId.HasValue )
            {
                return AttributeCache.GetOrderedGridAttributes( entityTypeId, string.Empty, string.Empty );
            }

            return new List<AttributeCache>();
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new PageShortLinkService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{PageShortLink.FriendlyTypeName} not found." );
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete {PageShortLink.FriendlyTypeName}." );
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

        #region Support Classes
        public class PageShortLinkWithClicks
        {
            public PageShortLink PageShortLink { get; set; }
            public int ClickCount { get; set; }
        }

        #endregion
    }
}

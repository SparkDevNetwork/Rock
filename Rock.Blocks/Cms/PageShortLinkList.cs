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
    public class PageShortLinkList : RockEntityListBlockType<PageShortLink>
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
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "ShortLinkId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<PageShortLink> GetListQueryable( RockContext rockContext )
        {
            var queryable = base.GetListQueryable( rockContext );

            return queryable;
        }

        /// <inheritdoc/>
        protected override GridBuilder<PageShortLink> GetGridBuilder()
        {
            return new GridBuilder<PageShortLink>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "url", a => a.Url )
                .AddTextField( "site", a => a.Site?.Name )
                .AddTextField( "token", a => a.Token )
                .AddTextField( "shortLink", a => new ShortLinkRow( a ).ShortLink )
                .AddAttributeFields( GetGridAttributes() );
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

        #region Nested Classes

        protected class ShortLinkRow : RockDynamic
        {
            public int Id { get; set; }
            public int SiteId { get; set; }
            public string SiteName { get; set; }
            public string Token { get; set; }
            public string Url { get; set; }
            public string ShortLink { get; set; }

            public ShortLinkRow( PageShortLink pageShortLink )
            {
                Id = pageShortLink.Id;
                SiteId = pageShortLink.Site.Id;
                SiteName = pageShortLink.Site.Name;
                Token = pageShortLink.Token;
                Url = pageShortLink.Url;

                var url = pageShortLink.Site.DefaultDomainUri.ToString();
                ShortLink = url.EnsureTrailingForwardslash() + Token;
            }
        }

        #endregion
    }
}

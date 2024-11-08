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
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.Tv.Classes;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Cms.SiteList;
using Rock.Web.Cache;

namespace Rock.Blocks.Tv
{
    /// <summary>
    /// Displays a list of TV applications (sites).
    /// </summary>

    [DisplayName( "TV Application List" )]
    [Category( "TV > TV Apps" )]
    [Description( "Displays a list of TV applications." )]
    [IconCssClass( "fa fa-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Apple TV Detail Page",
        Description = "The page that will show the site details for an Apple TV application.",
        Key = AttributeKey.AppleTvDetailPage,
        DefaultValue = "3D874455-7FE1-407B-A817-B0F82A51CEB8",
        Order = 0 )]

    [LinkedPage( "Roku Detail Page",
        Description = "The page that will show the site details for a Roku application.",
        Key = AttributeKey.RokuDetailPage,
        DefaultValue = "867EC436-7F72-4108-81B6-ADBCFFC3918A",
        Order = 1 )]

    [Rock.SystemGuid.EntityTypeGuid( "869B2D70-4AE6-40A0-8899-A3EB9EDFB3B3" )]
    [Rock.SystemGuid.BlockTypeGuid( "5DA60F71-DD30-4333-9863-1CCFCE241CDF" )]
    [CustomizedGrid]
    public class TvApplicationList : RockEntityListBlockType<Site>
    {
        #region Keys

        /// <summary>
        /// The attribute keys for the block settings.
        /// </summary>
        private static class AttributeKey
        {
            public const string AppleTvDetailPage = "AppleTvDetailPage";
            public const string RokuDetailPage = "RokuDetailPage";
        }

        /// <summary>
        /// The keys for the navigation URLs.
        /// </summary>
        private static class NavigationUrlKey
        {
            public const string AppleTvDetailPage = "AppleTvDetailPage";
            public const string RokuDetailPage = "RokuDetailPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<SiteListOptionsBag>();
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
        private SiteListOptionsBag GetBoxOptions()
        {
            var options = new SiteListOptionsBag();

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEnabled()
        {
            var entity = new Site();

            return entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.AppleTvDetailPage] = this.GetLinkedPageUrl( AttributeKey.AppleTvDetailPage, "SiteId", "((Key))" ),
                [NavigationUrlKey.RokuDetailPage] = this.GetLinkedPageUrl( AttributeKey.RokuDetailPage, "SiteId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<Site> GetListQueryable( RockContext rockContext )
        {
            var tvSiteTypes = new List<SiteType> { SiteType.Tv };
            var listQueryable = base.GetListQueryable( rockContext );

            return listQueryable.Where( a => tvSiteTypes.Contains( a.SiteType ) );
        }

        /// <inheritdoc/>
        protected override GridBuilder<Site> GetGridBuilder()
        {
            return new GridBuilder<Site>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.Name )
                .AddTextField( "description", a => a.Description )
                .AddField( "tvPlatform", a => GetApplicationSettings( a )?.TvApplicationType.ConvertToString() )
                .AddField( "isActive", a => a.IsActive )
                .AddField( "isSystem", a => a.IsSystem )
                .AddField( "isSecurityDisabled", a => !a.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
                .AddAttributeFields( GetGridAttributes() );
        }

        /// <summary>
        /// Gets the application settings for the site.
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        private ApplicationSettings GetApplicationSettings( Site site )
        {
            var applicationSettings = site.AdditionalSettings?.FromJsonOrNull<ApplicationSettings>();
            return applicationSettings;
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
            var entityService = new SiteService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{Site.FriendlyTypeName} not found." );
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete {Site.FriendlyTypeName}." );
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            var sitePages = new List<int> {
                    entity.DefaultPageId ?? -1,
                    entity.LoginPageId ?? -1,
                    entity.RegistrationPageId ?? -1,
                    entity.PageNotFoundPageId ?? -1
                };

            entity.DefaultPageId = null;
            RockContext.SaveChanges();

            var pageService = new PageService( RockContext );
            foreach ( var page in pageService.Queryable( "Layout" )
                .Where( t => !t.IsSystem && ( t.Layout.SiteId == entity.Id || sitePages.Contains( t.Id ) ) ) )
                {
                    if ( pageService.CanDelete( page, out string deletePageErrorMessage ) )
                    {
                        pageService.Delete( page );
                    }
                }

            var layoutService = new LayoutService( RockContext );
            var layoutQry = layoutService.Queryable()
                .Where( l =>
                l.SiteId == entity.Id );
            layoutService.DeleteRange( layoutQry );

            RockContext.SaveChanges( true );

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk();
        }

        #endregion
    }
}

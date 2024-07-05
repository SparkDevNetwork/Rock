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
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Cms.SiteList;
using Rock.Web.Cache;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays a list of sites.
    /// </summary>

    [DisplayName( "Site List" )]
    [Category( "CMS" )]
    [Description( "Displays a list of sites." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the site details.",
        Key = AttributeKey.DetailPage )]
    [EnumsField(
        "Site Type",
        "Includes Items with the following Type.",
        typeof( SiteType ),
        false, "",
        order: 1,
        key: AttributeKey.SiteType )]

    [BooleanField( "Show Delete Column",
        Description = "Determines if the delete column should be shown.",
        DefaultBooleanValue = false,
        IsRequired = true,
        Key = AttributeKey.ShowDeleteColumn,
        Order = 2 )]

    [Rock.SystemGuid.EntityTypeGuid( "12a8c8ae-7bbe-41d2-9448-8d7eae298099" )]
    [Rock.SystemGuid.BlockTypeGuid( "d27a9c0d-e118-4172-8f8e-368c973f5486" )]
    [CustomizedGrid]
    public class SiteList : RockEntityListBlockType<Site>
    {
        #region Keys

        private static class AttributeKey
        {
             public const string SiteType = "SiteType";
            public const string ShowDeleteColumn = "ShowDeleteColumn";
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
            var box = new ListBlockBox<SiteListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = GetIsAddEnabled();
            box.IsDeleteEnabled = GetAttributeValue(AttributeKey.ShowDeleteColumn).AsBoolean();
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
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "SiteId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<Site> GetListQueryable( RockContext rockContext )
        {
            var siteType = GetAttributeValue( AttributeKey.SiteType ).SplitDelimitedValues().Select( a => a.ConvertToEnumOrNull<SiteType>() ).ToList();

            var qry = base.GetListQueryable( rockContext );
            if ( siteType.Count() > 0 )
            {
                // Filter by block setting Site type
                qry = qry.Where( s => siteType.Contains( s.SiteType ) );
            }

            return qry;
        }

        /// <inheritdoc/>
        protected override GridBuilder<Site> GetGridBuilder()
        {
            return new GridBuilder<Site>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddField( "isSystem", a => a.IsSystem )
                .AddTextField( "name", a => a.Name )
                .AddTextField( "description", a => a.Description )
                .AddTextField( "theme", a => a.Theme )
                .AddTextField( "siteIconUrl", p => GetSiteIconUrl( p ) )
                .AddTextField( "domains", p => p.SiteDomains.Select( a => a.Domain ).JoinStringsWithCommaAnd() )
                .AddField( "isSecurityDisabled", a => !a.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
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
            using ( var rockContext = new RockContext() )
            {
                var entityService = new SiteService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{Site.FriendlyTypeName} not found." );
                }

                if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete {Site.FriendlyTypeName}." );
                }

                var sitePages = new List<int> {
                    entity.DefaultPageId ?? -1,
                    entity.LoginPageId ?? -1,
                    entity.RegistrationPageId ?? -1,
                    entity.PageNotFoundPageId ?? -1
                };

                var pageService = new PageService( rockContext );
                foreach ( var page in pageService.Queryable( "Layout" )
                    .Where( t => !t.IsSystem && ( t.Layout.SiteId == entity.Id || sitePages.Contains( t.Id ) ) ) )
                {
                    if ( pageService.CanDelete( page, out string deletePageErrorMessage ) )
                    {
                        pageService.Delete( page );
                    }
                }

                var layoutService = new LayoutService( rockContext );
                var layoutQry = layoutService.Queryable()
                    .Where( l =>
                    l.SiteId == entity.Id );
                layoutService.DeleteRange( layoutQry );

                rockContext.SaveChanges( true );

                if ( !entityService.CanDelete( entity, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                entityService.Delete( entity );
                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        /// <summary>
        /// Get the site icon.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <returns>A <see cref="string"/> site icon url.</returns>
        private string GetSiteIconUrl( Site site )
        {
            string path;

            // If this is a Person, use the Person properties.
            if ( site != null && site.SiteLogoBinaryFileId.HasValue )
            {
                var options = new GetImageUrlOptions { Height = 50 };
                path = FileUrlHelper.GetImageUrl( site.SiteLogoBinaryFileId.Value, options );
            }
            // Otherwise, use the first letter of the entity type.
            else
            {
                path = $"/GetAvatar.ashx?text={site.Name.SubstringSafe( 0, 1 )}";
            }

            return path;
        }

        #endregion
    }
}

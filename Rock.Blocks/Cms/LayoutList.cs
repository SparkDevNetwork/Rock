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
// </copyright//

using System.Web.Http;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.IO;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Cms.LayoutList;
using Rock.Web.Cache;
using Rock;
using Rock.Web;
using System;
using Rock.SystemGuid;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays a list of layouts.
    /// </summary>
    [DisplayName( "Layout List" )]
    [Category( "CMS" )]
    [Description( "Displays a list of layouts." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes(Model.SiteType.Web)]
    [LinkedPage( "Detail Page", Description = "The page that will show the layout details.", Key = AttributeKey.DetailPage )]
    [Rock.SystemGuid.EntityTypeGuid( "6e1d987d-de38-4440-b54f-717c102795fe" )]
    [Rock.SystemGuid.BlockTypeGuid( "6a10a280-65b8-4988-96b2-974fcd80604b" )]
    [CustomizedGrid]
    public class LayoutList : RockEntityListBlockType<Rock.Model.Layout>
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
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<LayoutListOptionsBag>();
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
        private LayoutListOptionsBag GetBoxOptions()
        {
            var options = new LayoutListOptionsBag();
            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEnabled()
        {
            var entity = new Rock.Model.Layout();
            return entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var siteIdParam = PageParameter( PageParameterKey.SiteId );
            var siteId = Rock.Utility.IdHasher.Instance.GetId( siteIdParam ).ToString() ?? siteIdParam;
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, new Dictionary<string, string>
        {
            { "LayoutId", "((Key))" },
            { "SiteId", siteId }
        } )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<Rock.Model.Layout> GetListQueryable( RockContext rockContext )
        {
            var siteIdParam = PageParameter( PageParameterKey.SiteId );
            var siteId = Rock.Utility.IdHasher.Instance.GetId( siteIdParam ) ?? siteIdParam.AsIntegerOrNull();
            if ( siteId.HasValue )
            {
                var site = new SiteService( rockContext ).Get( siteId.Value );
                if ( site != null )
                {
                    var layouts = new LayoutService( rockContext )
                        .Queryable()
                        .Include( l => l.Site )
                        .Where( l => l.SiteId == siteId );
                    return layouts;
                }
            }
            // Return an empty queryable if no valid siteId is provided or site doesn't exist
            return new LayoutService( rockContext ).Queryable().Where( l => false );
        }

        /// <inheritdoc/>
        protected override IQueryable<Rock.Model.Layout> GetOrderedListQueryable( IQueryable<Rock.Model.Layout> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( e => e.Name );
        }

        /// <summary>
        /// Gets the list items with the file paths updated and missing files marked.
        /// </summary>
        /// <param name="queryable">The queryable.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>A list of layouts.</returns>
        protected override List<Rock.Model.Layout> GetListItems( IQueryable<Rock.Model.Layout> queryable, RockContext rockContext )
        {
            var layouts = queryable.ToList();
            foreach ( var layout in layouts )
            {
                if ( layout.Site != null && layout.FileName != null )
                {
                    var virtualPath = $"~/Themes/{layout.Site.Theme}/Layouts/{layout.FileName}.aspx";
                    // Check if the file exists
                    var site = SiteCache.Get( layout.SiteId );
                    if ( site != null )
                    {
                        var physicalRootFolder = AppDomain.CurrentDomain.BaseDirectory;
                        var physicalPath = Path.Combine( physicalRootFolder, "Themes", site.Theme, "Layouts", $"{layout.FileName}.aspx" );

                        layout.FileName = virtualPath;
                        if ( !File.Exists( physicalPath ) )
                        {
                            layout.FileName += "|Missing";
                        }
                    }
                }
            }
            return layouts;
        }

        /// <inheritdoc/>
        protected override GridBuilder<Rock.Model.Layout> GetGridBuilder()
        {
            return new GridBuilder<Rock.Model.Layout>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddField( "id", a => a.Id )
                .AddTextField( "theme", a => a.Site.Theme )
                .AddTextField( "name", a => a.Name )
                .AddTextField( "fileName", a => a.FileName )
                .AddTextField( "description", a => a.Description )
                .AddField( "isSystem", a => a.IsSystem )
                .AddField( "siteId", a => a.SiteId )
                .AddTextField( "layoutMobilePhone", a => a.LayoutMobilePhone )
                .AddTextField( "layoutMobileTablet", a => a.LayoutMobileTablet )
                .AddDateTimeField( "createdDateTime", a => a.CreatedDateTime )
                .AddDateTimeField( "modifiedDateTime", a => a.ModifiedDateTime )
                .AddField( "createdByPersonAliasId", a => a.CreatedByPersonAliasId )
                .AddField( "modifiedByPersonAliasId", a => a.ModifiedByPersonAliasId )
                .AddField( "guid", a => a.Guid )
                .AddField( "foreignId", a => a.ForeignId )
                .AddField( "foreignGuid", a => a.ForeignGuid )
                .AddTextField( "foreignKey", a => a.ForeignKey )
                .AddField( "isSecurityDisabled", a => !a.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) );
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
                var entityService = new LayoutService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );
                if ( entity == null )
                {
                    return ActionBadRequest( $"{Rock.Model.Layout.FriendlyTypeName} not found." );
                }
                if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete {Rock.Model.Layout.FriendlyTypeName}." );
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
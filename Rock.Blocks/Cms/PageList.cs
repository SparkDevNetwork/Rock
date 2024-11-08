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
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Cms.PageList;
using Rock.Web.Cache;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays a list of pages.
    /// </summary>

    [DisplayName( "Page List" )]
    [Category( "CMS" )]
    [Description( "Displays a list of pages." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [BooleanField(
            "Show Page Id",
            Description = "Enables the hiding of the page id column.",
            DefaultBooleanValue = true,
            Key = AttributeKey.ShowPageId )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "b49f5c5b-95d4-448d-8a82-be7661e4ff1d" )]
    [Rock.SystemGuid.BlockTypeGuid( "39b02b93-b1af-4f9b-a535-33f470d91106" )]
    [CustomizedGrid]
    public class PageList : RockEntityListBlockType<Page>
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string ShowPageId = "ShowPageId";
        }

        #endregion Attribute Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<PageListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsDeleteEnabled = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
            box.ExpectedRowCount = null;
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private PageListOptionsBag GetBoxOptions()
        {
            var options = new PageListOptionsBag()
            {
                ShowPageId = GetAttributeValue( AttributeKey.ShowPageId ).AsBoolean()
            };

            return options;
        }

        /// <inheritdoc/>
        protected override IQueryable<Page> GetListQueryable( RockContext rockContext )
        {
            // Retrieve the siteId from page parameters
            int siteId = RequestContext?.PageParameters?["SiteId"]?.AsInteger() ?? 0;

            // Fetch pages directly associated with the site
            var pages = new PageService( rockContext )
                .Queryable().AsNoTracking()
                .Where( p => p.Layout.SiteId == siteId );

            return pages;
        }

        /// <inheritdoc/>
        protected override GridBuilder<Page> GetGridBuilder()
        {
            return new GridBuilder<Page>()
                .WithBlock( this )
                .AddField( "id", a => a.Id )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "pageTitle", a => a.PageTitle )
                .AddTextField( "description", a => a.Description )
                .AddTextField( "layout", a => a.Layout?.Name )
                .AddField( "isSystem", a => a.IsSystem )
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
                var entityService = new PageService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{Page.FriendlyTypeName} not found." );
                }

                if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete ${Page.FriendlyTypeName}." );
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

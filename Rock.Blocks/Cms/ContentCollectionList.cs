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
using Rock.ViewModels.Blocks.Cms.ContentCollectionList;
using Rock.Web.Cache;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays a list of content collections.
    /// </summary>

    [DisplayName( "Content Collection List" )]
    [Category( "CMS" )]
    [Description( "Displays a list of content collections." )]
    [IconCssClass( "fa fa-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the content collection details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.EntityTypeGuid( "87323cba-9fb8-4dde-84ed-953f1da19049" )]
    // WAS [Rock.SystemGuid.BlockTypeGuid( "55fde870-57b0-476e-bd86-63597a8de6c1" )]
    [Rock.SystemGuid.BlockTypeGuid( "F305FE35-2EFA-4653-AA1A-87AE990EAFEB" )]
    [CustomizedGrid]
    public class ContentCollectionList : RockEntityListBlockType<ContentCollection>
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
            var box = new ListBlockBox<ContentCollectionListOptionsBag>();
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
        private ContentCollectionListOptionsBag GetBoxOptions()
        {
            var options = new ContentCollectionListOptionsBag();
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
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "ContentCollectionId", "((Key))" )
            };
        }

        /// <inheritdoc/>p
        protected override IQueryable<ContentCollection> GetListQueryable( RockContext rockContext )
        {
            return base.GetListQueryable( rockContext ).Include( a => a.ContentCollectionSources);
        }

        protected override IQueryable<ContentCollection> GetOrderedListQueryable( IQueryable<ContentCollection> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( c => c.Name);
        }

        /// <inheritdoc/>
        protected override GridBuilder<ContentCollection> GetGridBuilder()
        {
            return new GridBuilder<ContentCollection>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.Name )
                .AddField( "sources", a => a.ContentCollectionSources.Count() )
                .AddField( "itemCount", a => a.LastIndexItemCount )
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
            var entityService = new ContentCollectionService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{ContentCollection.FriendlyTypeName} not found." );
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete {ContentCollection.FriendlyTypeName}." );
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
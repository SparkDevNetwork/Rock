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
using Rock.ViewModels.Blocks.Cms.LavaApplicationList;
using Rock.Web.Cache;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays a list of lava applications.
    /// </summary>

    [DisplayName( "Lava Application List" )]
    [Category( "CMS" )]
    [Description( "Displays a list of lava applications." )]
    [IconCssClass( "fa fa-list" )]
    [SupportedSiteTypes( SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the lava application details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.EntityTypeGuid( "B1E023F7-DAF1-4F90-9002-3134ECC5FD6E" )]
    [Rock.SystemGuid.BlockTypeGuid( "A59ED0DD-CD86-4A0A-A7B7-95D7966D90A5" )]
    [CustomizedGrid]
    public class LavaApplicationList : RockEntityListBlockType<LavaApplication>
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
            var box = new ListBlockBox<LavaApplicationListOptionsBag>();
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
        private LavaApplicationListOptionsBag GetBoxOptions()
        {
            var options = new LavaApplicationListOptionsBag();

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEnabled()
        {
            var entity = new LavaApplication();

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
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "LavaApplicationId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<LavaApplication> GetListQueryable( RockContext rockContext )
        {
            var qry = base.GetListQueryable( rockContext );
            return qry;
        }

        /// <inheritdoc/>
        protected override GridBuilder<LavaApplication> GetGridBuilder()
        {
            var blockOptions = new GridBuilderGridOptions<LavaApplication>
            {
                LavaObject = row => row
            };

            return new GridBuilder<LavaApplication>()
                .WithBlock( this, blockOptions )
                .AddTextField( "idKey", a => a.IdKey )
                .AddField( "isSystem", a => a.IsSystem )
                .AddTextField( "name", a => a.Name )
                .AddTextField( "slug", a => a.Slug )
                .AddField( "isActive", a => a.IsActive )
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
                var entityService = new LavaApplicationService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{LavaApplication.FriendlyTypeName} not found." );
                }

                if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete ${LavaApplication.FriendlyTypeName}." );
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
    }

    #endregion
}

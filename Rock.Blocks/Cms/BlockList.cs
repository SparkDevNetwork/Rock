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
#if REVIEW_NET5_0_OR_GREATER
using Microsoft.EntityFrameworkCore;
#else
using System.Data.Entity;
#endif
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Cms.BlockList;
using Rock.Web.Cache;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays a list of blocks.
    /// </summary>

    [DisplayName( "Block List" )]
    [Category( "CMS" )]
    [Description( "Displays a list of blocks." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "9cf1aa10-24e4-4530-a345-57da4cfe9595" )]
    [Rock.SystemGuid.BlockTypeGuid( "ea8be085-d420-4d1b-a538-2c0d4d116e0a" )]
    [CustomizedGrid]
    public class BlockList : RockEntityListBlockType<Block>
    {
        #region Keys

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<BlockListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsDeleteEnabled = true;
            box.ExpectedRowCount = null;
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private BlockListOptionsBag GetBoxOptions()
        {
            var options = new BlockListOptionsBag();

            return options;
        }

        /// <inheritdoc/>
        protected override IQueryable<Block> GetListQueryable( RockContext rockContext )
        {
            if ( int.TryParse( RequestContext.PageParameters["layoutId"], out int layoutId ) )
            {
                return base.GetListQueryable( rockContext )
                    .Include( a => a.BlockType )
                    .Include( a => a.Layout )
                    .Include( a => a.Page )
                    .Include( a => a.Site )
                    .Where( a => a.LayoutId == layoutId ); 
            }
            else
            {
                return new List<Block>().AsQueryable();
            }
        }

        /// <inheritdoc/>
        protected override GridBuilder<Block> GetGridBuilder()
        {
            return new GridBuilder<Block>()
                .WithBlock( this )
                .AddField( "id", a => a.Id )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.Name )
                .AddTextField( "blockType", a => a.BlockType?.Name )
                .AddTextField( "path", a => a.BlockType.Path )
                .AddTextField( "zone", a => a.Zone )
                .AddTextField( "additionalSettings", a => a.AdditionalSettings )
                .AddTextField( "preHtml", a => a.PreHtml )
                .AddTextField( "cssClass", a => a.CssClass )
                .AddTextField( "layout", a => a.Layout?.Name )
                .AddField( "order", a => a.Order )
                .AddField( "outputCacheDuration", a => a.OutputCacheDuration )
                .AddTextField( "postHtml", a => a.PostHtml )
                .AddTextField( "site", a => a.Site?.Name )
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
                var entityService = new BlockService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{Block.FriendlyTypeName} not found." );
                }

                if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete ${Block.FriendlyTypeName}." );
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

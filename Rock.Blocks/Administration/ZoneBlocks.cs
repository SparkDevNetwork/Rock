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
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Administration.ZoneBlocks;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Administration
{
    /// <summary>
    /// Displays and manages the blocks configured for a page zone at the page, layout, and site scopes.
    /// </summary>
    [DisplayName( "Zone Blocks" )]
    [Category( "Administration" )]
    [Description( "Displays the blocks for a given zone." )]
    [IconCssClass( "ti ti-layout-2" )]

    [Rock.SystemGuid.EntityTypeGuid( "FDD5808D-6AD6-4123-9796-EAC0976A91D8" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "8B5EECBD-8795-4921-B4C5-448037913E39" )]
    [Rock.SystemGuid.BlockTypeGuid( "72CAAF77-A015-45F0-A549-F941B9AB4D75" )]
    public class ZoneBlocks : RockBlockType
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string EditPage = "EditPage";
            public const string ZoneName = "ZoneName";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<ZoneBlocksBag, ZoneBlocksOptionsBag>();
            var editPage = GetEditPage();

            // Without an administrable page there is nothing to edit; the client renders a notice.
            if ( editPage == null || !CanAdministrate( editPage ) )
            {
                box.Bag = new ZoneBlocksBag
                {
                    ZoneName = PageParameter( PageParameterKey.ZoneName ),
                    CanAdministrate = false
                };

                return box;
            }

            var blockService = new BlockService( RockContext );
            var zoneName = PageParameter( PageParameterKey.ZoneName );

            box.Bag = new ZoneBlocksBag
            {
                ZoneName = zoneName,
                LayoutName = editPage.Layout?.Name,
                CanAdministrate = true,
                PageBlockCount = blockService.GetByPageAndZone( editPage.Id, zoneName ).Count(),
                LayoutBlockCount = blockService.GetByLayoutAndZone( editPage.LayoutId, zoneName ).Count(),
                SiteBlockCount = blockService.GetBySiteAndZone( editPage.SiteId, zoneName ).Count()
            };

            box.Options.BlockTypes = GetBlockTypeItems( editPage.Layout.Site.SiteType );
            box.Options.CommonBlockTypes = GetCommonBlockTypeItems();
            box.Options.DefaultBlockTypeValue = Rock.SystemGuid.BlockType.HTML_CONTENT;

            return box;
        }

        /// <summary>
        /// Resolves the page being edited from the <see cref="PageParameterKey.EditPage"/> parameter,
        /// accepting an Id, IdKey, or Guid.
        /// </summary>
        /// <returns>The page being edited, or <c>null</c> if it could not be resolved.</returns>
        private PageCache GetEditPage()
        {
            var allowIntegerId = !PageCache.Layout.Site.DisablePredictableIds;

            return PageCache.Get( PageParameter( PageParameterKey.EditPage ), allowIntegerId );
        }

        /// <summary>
        /// Determines whether the current person may administrate the specified page.
        /// </summary>
        /// <param name="editPage">The page being edited.</param>
        /// <returns><c>true</c> if the current person may administrate the page; otherwise <c>false</c>.</returns>
        private bool CanAdministrate( PageCache editPage )
        {
            return editPage?.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) == true;
        }

        /// <summary>
        /// Creates the grid builder shared by the page, layout, and site block grids.
        /// </summary>
        /// <param name="blockTypesById">The block types used by the blocks being displayed, keyed by their identifier, so each is resolved from cache only once.</param>
        /// <returns>A configured grid builder for blocks.</returns>
        private GridBuilder<Block> GetGridBuilder( Dictionary<int, BlockTypeCache> blockTypesById )
        {
            return new GridBuilder<Block>()
                .WithBlock( this )
                .AddTextField( "idKey", b => b.IdKey )
                .AddTextField( "name", b => b.Name )
                .AddTextField( "blockTypeName", b => BlockTypeDisplayName( blockTypesById[b.BlockTypeId] ) )
                .AddTextField( "blockTypeCategory", b => blockTypesById[b.BlockTypeId]?.Category )
                .AddTextField( "blockTypeValue", b => blockTypesById[b.BlockTypeId]?.Guid.ToString() )
                .AddField( "isSystem", b => b.IsSystem );
        }

        /// <summary>
        /// Gets the block type list items for the Type dropdown, grouped by category and ordered by
        /// category then name.
        /// </summary>
        /// <param name="siteType">The site type used to filter which block types may be added.</param>
        /// <returns>The categorized block type list items.</returns>
        private List<ListItemBag> GetBlockTypeItems( SiteType siteType )
        {
            const string uncategorized = "Other (not categorized)";

            return BlockTypeService.BlockTypesToDisplay( siteType, true )
                .Select( bt => new ListItemBag
                {
                    Value = bt.Guid.ToString(),
                    Text = BlockTypeDisplayName( bt ),
                    Category = bt.Category.IsNotNullOrWhiteSpace() ? bt.Category : uncategorized
                } )
                .OrderBy( i => i.Category )
                .ThenBy( i => i.Text )
                .ToList();
        }

        /// <summary>
        /// Gets the common block types offered as quick-pick buttons, ordered by name.
        /// </summary>
        /// <returns>The common block type list items.</returns>
        private List<ListItemBag> GetCommonBlockTypeItems()
        {
            return BlockTypeCache.All()
                .Where( bt => bt.IsCommon )
                .OrderBy( bt => bt.Name )
                .Select( bt => new ListItemBag
                {
                    Value = bt.Guid.ToString(),
                    Text = bt.Name
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the display name for a block type, appending a marker to Obsidian block types so they
        /// are distinguishable from legacy block types.
        /// </summary>
        /// <param name="blockType">The block type.</param>
        /// <returns>The display name.</returns>
        private static string BlockTypeDisplayName( BlockTypeCache blockType )
        {
            if ( blockType == null )
            {
                return string.Empty;
            }

            return IsObsidianBlockType( blockType ) ? $"{blockType.Name} \U0001F389" : blockType.Name;
        }

        /// <summary>
        /// Determines whether a block type is an Obsidian (entity-based) block type. Obsidian block
        /// types have no file path, unlike legacy block types.
        /// </summary>
        /// <param name="blockType">The block type.</param>
        /// <returns><c>true</c> if the block type is an Obsidian block type; otherwise <c>false</c>.</returns>
        private static bool IsObsidianBlockType( BlockTypeCache blockType )
        {
            return blockType != null && blockType.Path.IsNullOrWhiteSpace();
        }

        /// <summary>
        /// Gets the blocks for the specified location within the current zone, ordered for display.
        /// </summary>
        /// <param name="blockService">The block service.</param>
        /// <param name="editPage">The page being edited.</param>
        /// <param name="location">The scope whose blocks should be returned.</param>
        /// <returns>The ordered list of blocks.</returns>
        private List<Block> GetBlocksForLocation( BlockService blockService, PageCache editPage, BlockLocation location )
        {
            var zone = PageParameter( PageParameterKey.ZoneName );

            switch ( location )
            {
                case BlockLocation.Site:
                    return blockService.GetBySiteAndZone( editPage.SiteId, zone ).ToList();
                case BlockLocation.Layout:
                    return blockService.GetByLayoutAndZone( editPage.LayoutId, zone ).ToList();
                default:
                    return blockService.GetByPageAndZone( editPage.Id, zone ).ToList();
            }
        }

        /// <summary>
        /// Flushes the appropriate cached pages after a block change, based on the block's scope.
        /// </summary>
        /// <param name="block">The block that changed.</param>
        /// <param name="editPage">The page being edited.</param>
        private void FlushCacheForBlock( Block block, PageCache editPage )
        {
            if ( block.LayoutId.HasValue )
            {
                PageCache.FlushPagesForLayout( block.LayoutId.Value );
            }
            else if ( block.SiteId.HasValue )
            {
                PageCache.FlushPagesForSite( block.SiteId.Value );
            }
            else
            {
                PageCache.Remove( editPage.Id );
            }
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Gets the grid data for the blocks in the current zone at the specified location.
        /// </summary>
        /// <param name="location">The scope (Page, Layout, or Site) to load.</param>
        /// <returns>The grid data for the requested location.</returns>
        [BlockAction]
        public BlockActionResult GetBlockGridData( BlockLocation location )
        {
            var editPage = GetEditPage();

            if ( editPage == null )
            {
                return ActionNotFound( "The page could not be found." );
            }

            if ( !CanAdministrate( editPage ) )
            {
                return ActionUnauthorized( "You are not authorized to edit these blocks." );
            }

            var blocks = GetBlocksForLocation( new BlockService( RockContext ), editPage, location );

            // Resolve each distinct block type from cache once, rather than repeatedly per grid field.
            var blockTypesById = blocks
                .Select( b => b.BlockTypeId )
                .Distinct()
                .ToDictionary( id => id, id => BlockTypeCache.Get( id ) );

            return ActionOk( GetGridBuilder( blockTypesById ).Build( blocks ) );
        }

        /// <summary>
        /// Creates or updates a block within the current zone.
        /// </summary>
        /// <param name="bag">The details of the block to save.</param>
        /// <returns>An empty successful result, or an error.</returns>
        [BlockAction]
        public BlockActionResult SaveBlock( ZoneBlocksSaveBlockBag bag )
        {
            if ( bag == null )
            {
                return ActionBadRequest( "No block information was provided." );
            }

            var editPage = GetEditPage();

            if ( editPage == null )
            {
                return ActionNotFound( "The page could not be found." );
            }

            if ( !CanAdministrate( editPage ) )
            {
                return ActionUnauthorized( "You are not authorized to edit these blocks." );
            }

            var blockTypeId = BlockTypeCache.Get( bag.BlockTypeValue.AsGuid() )?.Id;

            if ( !blockTypeId.HasValue )
            {
                return ActionBadRequest( "The selected block type could not be found." );
            }

            var blockService = new BlockService( RockContext );
            var block = bag.IdKey.IsNotNullOrWhiteSpace()
                ? blockService.Get( bag.IdKey, !PageCache.Layout.Site.DisablePredictableIds )
                : null;
            var isNew = block == null;

            if ( isNew )
            {
                block = new Block
                {
                    Zone = PageParameter( PageParameterKey.ZoneName )
                };

                switch ( bag.Location )
                {
                    case BlockLocation.Site:
                        block.SiteId = editPage.SiteId;
                        break;
                    case BlockLocation.Layout:
                        block.LayoutId = editPage.LayoutId;
                        break;
                    default:
                        block.PageId = editPage.Id;
                        break;
                }

                blockService.Add( block );

                // Place the new block at the end of its own scope + zone.
                block.Order = blockService.GetMaxOrder( block );
            }

            block.Name = bag.Name;
            block.BlockTypeId = blockTypeId.Value;

            RockContext.SaveChanges();

            // New blocks inherit the page's authorization rules.
            if ( isNew )
            {
                Authorization.CopyAuthorization( editPage, block, RockContext );
            }

            FlushCacheForBlock( block, editPage );

            return ActionOk();
        }

        /// <summary>
        /// Deletes a block from the current zone.
        /// </summary>
        /// <param name="key">The identifier key of the block to delete.</param>
        /// <returns>An empty successful result, or an error.</returns>
        [BlockAction]
        public BlockActionResult DeleteBlock( string key )
        {
            var editPage = GetEditPage();

            if ( editPage == null )
            {
                return ActionNotFound( "The page could not be found." );
            }

            if ( !CanAdministrate( editPage ) )
            {
                return ActionUnauthorized( "You are not authorized to edit these blocks." );
            }

            var blockService = new BlockService( RockContext );
            var block = blockService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( block == null )
            {
                return ActionNotFound( "The block could not be found." );
            }

            blockService.Delete( block );
            RockContext.SaveChanges();

            FlushCacheForBlock( block, editPage );

            return ActionOk();
        }

        /// <summary>
        /// Reorders a block within the current zone at the specified location.
        /// </summary>
        /// <param name="location">The scope the block belongs to.</param>
        /// <param name="key">The identifier key of the block being moved.</param>
        /// <param name="beforeKey">The identifier key of the block it should be placed before, or <c>null</c> for the end.</param>
        /// <returns>An empty successful result, or an error.</returns>
        [BlockAction]
        public BlockActionResult ReorderBlock( BlockLocation location, string key, string beforeKey )
        {
            var editPage = GetEditPage();

            if ( editPage == null )
            {
                return ActionNotFound( "The page could not be found." );
            }

            if ( !CanAdministrate( editPage ) )
            {
                return ActionUnauthorized( "You are not authorized to edit these blocks." );
            }

            var blockService = new BlockService( RockContext );
            var blocks = GetBlocksForLocation( blockService, editPage, location );

            if ( !blocks.ReorderEntity( key, beforeKey ) )
            {
                return ActionBadRequest( "Invalid reorder attempt." );
            }

            RockContext.SaveChanges();

            // A reorder affects every page rendered from this scope, so flush that scope's cache.
            switch ( location )
            {
                case BlockLocation.Site:
                    PageCache.FlushPagesForSite( editPage.SiteId );
                    break;
                case BlockLocation.Layout:
                    PageCache.FlushPagesForLayout( editPage.LayoutId );
                    break;
                default:
                    PageCache.Remove( editPage.Id );
                    break;
            }

            return ActionOk();
        }

        #endregion Block Actions
    }
}

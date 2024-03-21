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
using System.IO;
using System.Linq;
using System.Web.Hosting;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Cms.BlockTypeList;
using Rock.Web.Cache;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays a list of block types.
    /// </summary>

    [DisplayName( "Block Type List" )]
    [Category( "CMS" )]
    [Description( "Displays a list of block types." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the block type details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.EntityTypeGuid( "8fcee05f-6757-4b16-8718-63cd80ff07d6" )]
    [Rock.SystemGuid.BlockTypeGuid( "1c3d7f3d-e8c7-4f27-871c-7ec20483b416" )]
    [CustomizedGrid]
    public class BlockTypeList : RockListBlockType<BlockTypeListBag>
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
        private static class PreferenceKey
        {
            public const string FilterName = "filter-name";

            public const string FilterPath = "filter-path";

            public const string FilterCategory = "filter-category";

            public const string FilterSystemTypes = "filter-system-types";
        }

        #endregion Keys

        #region Properties

        protected string FilterName => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterName );

        protected string FilterPath => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterPath );

        protected string FilterCategory => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterCategory );

        protected string FilterSystemTypes => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterSystemTypes );

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<BlockTypeListOptionsBag>();
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
        private BlockTypeListOptionsBag GetBoxOptions()
        {
            var options = new BlockTypeListOptionsBag();

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
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "BlockTypeId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<BlockTypeListBag> GetListQueryable( RockContext rockContext )
        {
            var blockTypeService = new BlockTypeService( rockContext );
            var query = blockTypeService.Queryable().AsNoTracking();


            // Filters
            if ( !string.IsNullOrEmpty( FilterName ) )
            {
                query = query.Where( bt => bt.Name.Contains( FilterName ) );
            }
            if ( !string.IsNullOrEmpty( FilterPath ) )
            {
                query = query.Where( bt => bt.Path.Contains( FilterPath ) );
            }
            if ( !string.IsNullOrEmpty( FilterCategory ) )
            {
                query = query.Where( bt => bt.Category.Contains( FilterCategory ) );
            }
            if ( !string.IsNullOrEmpty( FilterSystemTypes ) )
            {
                bool systemTypeFilter = bool.Parse( FilterSystemTypes );
                if ( systemTypeFilter )
                {
                    query = query.Where( bt => !bt.IsSystem );
                }
            }

            var filteredData = query.Select( bt => new
            {
                bt.Id,
                bt.Name,
                bt.Category,
                bt.Description,
                Path = bt.Path ?? "",
                bt.EntityTypeId,
                BlocksCount = bt.Blocks.Count(),
                bt.IsSystem
            } ).ToList();

            return filteredData.Select( bt => new BlockTypeListBag
            {
                IdKey = IdHasher.Instance.GetHash( bt.Id ),
                Id = bt.Id,
                Name = bt.Name,
                Category = bt.Category,
                Description = bt.Description,
                Path = bt.Path.Replace( "~", "" ),
                EntityTypeId = bt.EntityTypeId,
                BlocksCount = bt.BlocksCount,
                IsSystem = bt.IsSystem,
                Status = GetBlockTypeStatus( bt.Path, bt.EntityTypeId, rockContext )
            } ).AsQueryable();
        }

        /// <inheritdoc/>
        protected override GridBuilder<BlockTypeListBag> GetGridBuilder()
        {
            return new GridBuilder<BlockTypeListBag>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "id", a => a.Id.ToString() )
                .AddTextField( "name", a => a.Name )
                .AddTextField( "category", a => a.Category )
                .AddTextField( "description", a => a.Description )
                .AddTextField( "path", a => a.Path )
                .AddTextField( "entityTypeId", a => a.EntityTypeId.HasValue ? a.EntityTypeId.Value.ToString() : string.Empty )
                .AddField( "blocksCount", a => a.BlocksCount )
                .AddTextField( "status", a => a.Status )
                .AddField( "isSystem", a => a.IsSystem );
        }

        /// <inheritdoc/>
        /// <summary>
        /// Gets the block type status.
        /// </summary>
        /// <param name="virtualPath">
        /// The virtual path to the block's ascx file. This is the path that was used to register the
        /// block with Rock.
        /// </param>
        /// <param name="entityTypeId">
        /// The entity type identifier of the block. This is the entity type that was used to register
        /// the block with Rock.
        /// </param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>A string that indicates the status of the block type or the name of the entity type itself.</returns>
        private string GetBlockTypeStatus( string virtualPath, int? entityTypeId, RockContext rockContext )
        {
            if ( !string.IsNullOrEmpty( virtualPath ) && virtualPath.StartsWith( "~/" ) )
            {
                string applicationBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string adjustedVirtualPath = virtualPath.TrimStart( '~' ).TrimStart( '/' );
                string fullPath = Path.Combine( applicationBaseDirectory, adjustedVirtualPath );

                if ( File.Exists( fullPath ) )
                {
                    return "Found";
                }
            }

            if ( entityTypeId.HasValue && rockContext != null )
            {
                var entityTypeService = new EntityTypeService( rockContext );
                var entityType = entityTypeService.Get( entityTypeId.Value );
                if ( entityType != null )
                {
                    return entityType.Name;
                }
            }

            return "Missing";
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
                var entityService = new BlockTypeService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{BlockType.FriendlyTypeName} not found." );
                }

                if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete ${BlockType.FriendlyTypeName}." );
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

        /// <summary>
        /// Action to reload all block type attributes.
        /// </summary>
        /// <returns>An empty result indicating the operation's success.</returns>
        [BlockAction( "ReloadAllBlockTypeAttributes" )]
        public BlockActionResult ReloadAllBlockTypeAttributes()
        {
            try
            {
                // Get the application root path.
                string applicationRootPath = HostingEnvironment.MapPath( "~" );

                // Call the RegisterBlockTypes method.
                BlockTypeService.RegisterBlockTypes( applicationRootPath, true );

                return ActionOk( "Block type attributes reloaded successfully." );
            }
            catch ( Exception ex )
            {
                return ActionBadRequest( "Error reloading block type attributes: " + ex.Message );
            }
        }

        #endregion
    }
}
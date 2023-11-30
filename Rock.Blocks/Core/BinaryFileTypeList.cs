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
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.BinaryFileTypeList;
using Rock.Web.Cache;

using static Rock.Blocks.Cms.AdaptiveMessageList;
using static Rock.Blocks.Core.BinaryFileTypeList;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays a list of binary file types.
    /// </summary>

    [DisplayName( "Binary File Type List" )]
    [Category( "Core" )]
    [Description( "Displays a list of binary file types." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the binary file type details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.EntityTypeGuid( "94ac60ce-b192-4559-88a0-af0cc143f631" )]
    [Rock.SystemGuid.BlockTypeGuid( "000ca534-6164-485e-b405-ba0fa6ae92f9" )]
    [CustomizedGrid]
    public class BinaryFileTypeList : RockListBlockType<BinaryFileTypeData>
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

        #region Fields

        /// <summary>
        /// The batch attributes that are configured to show on the grid.
        /// </summary>
        private readonly Lazy<List<AttributeCache>> _gridAttributes = new Lazy<List<AttributeCache>>( BuildGridAttributes );

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<BinaryFileTypeListOptionsBag>();
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
        private BinaryFileTypeListOptionsBag GetBoxOptions()
        {
            var options = new BinaryFileTypeListOptionsBag();

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEnabled()
        {
            var entity = new BinaryFileType();

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
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "BinaryFileTypeId", "((Key))" )
            };
        }

        /// <summary>
        /// Get a queryable for binary file type that is properly filtered.
        /// </summary>
        /// <param name="rockContext">The database context.</param>
        /// <returns>A queryable for <see cref="BinaryFileType"/>.</returns>
        private IQueryable<BinaryFileType> GetBinaryFileTypeQueryable( RockContext rockContext )
        {
            var qry = new BinaryFileTypeService( rockContext )
                .Queryable()
                .Include( a => a.StorageEntityType );

            return qry;
        }

        /// <inheritdoc/>
        protected override IQueryable<BinaryFileTypeData> GetListQueryable( RockContext rockContext )
        {
            var binaryFileQry = new BinaryFileService( rockContext ).Queryable();
            return GetBinaryFileTypeQueryable( rockContext ).Select( b => new BinaryFileTypeData
            {
                BinaryFileType = b,
                FileCount = binaryFileQry.Where( a => a.BinaryFileTypeId == b.Id ).Count()
            } );
        }

        /// <inheritdoc/>
        protected override GridBuilder<BinaryFileTypeData> GetGridBuilder()
        {
            var blockOptions = new GridBuilderGridOptions<BinaryFileTypeData>
            {
                LavaObject = row => row.BinaryFileType
            };

            return new GridBuilder<BinaryFileTypeData>()
                .WithBlock( this, blockOptions )
                .AddTextField( "idKey", a => a.BinaryFileType.IdKey )
                .AddTextField( "name", a => a.BinaryFileType.Name )
                .AddTextField( "description", a => a.BinaryFileType.Description )
                .AddTextField( "storageEntityType", a => a.BinaryFileType.StorageEntityType?.FriendlyName )
                .AddField( "fileCount", p => p.FileCount )
                .AddField( "isSystem", a => a.BinaryFileType.IsSystem )
                .AddField( "cacheToServerFileSystem", a => a.BinaryFileType.CacheToServerFileSystem )
                .AddField( "requiresViewSecurity", a => a.BinaryFileType.RequiresViewSecurity )
                .AddField( "isSecurityDisabled", a => !a.BinaryFileType.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
                .AddAttributeFieldsFrom( a => a.BinaryFileType, _gridAttributes.Value );
        }

        /// <summary>
        /// Builds the list of grid attributes that should be included on the Grid.
        /// </summary>
        /// <remarks>
        /// The default implementation returns only attributes that are not qualified.
        /// </remarks>
        /// <returns>A list of <see cref="AttributeCache"/> objects.</returns>
        private static List<AttributeCache> BuildGridAttributes()
        {
            var entityTypeId = EntityTypeCache.Get<BinaryFileType>( false )?.Id;

            if ( entityTypeId.HasValue )
            {
                return AttributeCache.GetOrderedGridAttributes( entityTypeId, string.Empty, string.Empty );
            }

            return new List<AttributeCache>();
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
                var entityService = new BinaryFileTypeService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{BinaryFileType.FriendlyTypeName} not found." );
                }

                if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete ${BinaryFileType.FriendlyTypeName}." );
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

        #region Support Classes

        /// <summary>
        /// The temporary data format to use when building the results for the
        /// grid.
        /// </summary>
        public class BinaryFileTypeData
        {
            /// <summary>
            /// Gets or sets the whole binary file type object from the database.
            /// </summary>
            /// <value>
            /// The whole binary file type object from the database.
            /// </value>
            public BinaryFileType BinaryFileType { get; set; }

            /// <summary>
            /// Gets or sets the number of file in this binary file type.
            /// </summary>
            /// <value>
            /// The number of file in this binary file type.
            /// </value>
            public int FileCount { get; set; }
        }

        #endregion
    }
}

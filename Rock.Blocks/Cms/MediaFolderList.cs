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
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Cms.MediaFolderList;
using Rock.Web.Cache;

using static Rock.Blocks.Cms.MediaAccountList;
using static Rock.Blocks.Cms.MediaFolderList;
using static Rock.Blocks.Finance.FinancialBatchList;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays a list of media folders.
    /// </summary>

    [DisplayName( "Media Folder List" )]
    [Category( "CMS" )]
    [Description( "Displays a list of media folders." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the media folder details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.EntityTypeGuid( "af4fa9d1-c8e7-47a6-a522-d40a7370517c" )]
    [Rock.SystemGuid.BlockTypeGuid( "75133c37-547f-47fa-991c-6d957b2ea92d" )]
    [CustomizedGrid]
    public class MediaFolderList : RockListBlockType<MediaFolderData>
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
            var box = new ListBlockBox<MediaFolderListOptionsBag>();
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
        private MediaFolderListOptionsBag GetBoxOptions()
        {
            var options = new MediaFolderListOptionsBag();

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEnabled()
        {
            var entity = new MediaFolder();

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
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "MediaFolderId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        private IQueryable<MediaFolder> GetMediaFolderListQueryable( RockContext rockContext )
        {
            var qry = new MediaFolderService( rockContext )
               .Queryable()
               .Include( "MediaElements" );

            return qry;
        }

        /// <inheritdoc/>
        protected override IQueryable<MediaFolderData> GetListQueryable( RockContext rockContext )
        {
            return GetMediaFolderListQueryable( rockContext ).Select( b => new MediaFolderData
            {
                MediaFolder = b,
                VideoCount = b.MediaElements.Count(),
            } );
        }

        /// <inheritdoc/>
        protected override List<MediaFolderData> GetListItems( IQueryable<MediaFolderData> queryable, RockContext rockContext )
        {
            // Load all the batches into memory.
            var items = queryable.ToList();

            // Load any attribute column configuration.
            var gridAttributeIds = _gridAttributes.Value.Select( a => a.Id ).ToList();
            Helper.LoadFilteredAttributes( items.Select( d => d.MediaFolder ), rockContext, a => gridAttributeIds.Contains( a.Id ) );
            var interactionChannelId = InteractionChannelCache.GetId( Rock.SystemGuid.InteractionChannel.MEDIA_EVENTS.AsGuid() );
            var mediaElementQry = new MediaElementService( rockContext ).Queryable();
            var interactionComponentQry = new InteractionComponentService( rockContext )
                .Queryable()
                .Where( c => c.InteractionChannelId == interactionChannelId );
            var watchCountQry = new InteractionService( rockContext )
                .Queryable()
                .Where( a => a.Operation == "Watch" );
            foreach ( var item in items )
            {
                var mediaElementIdQry = mediaElementQry.Where( a => a.MediaFolderId == item.MediaFolder.Id ).Select( a => ( int? ) a.Id );
                var interactionComponentSelectQry = interactionComponentQry.Where( c => mediaElementIdQry.Contains( c.EntityId ) )
                .Select( a => a.Id );
                item.WatchCount = watchCountQry.Where( b => interactionComponentSelectQry.Contains( b.InteractionComponentId ) ).Count();
            }

            return items;
        }

        /// <inheritdoc/>
        protected override GridBuilder<MediaFolderData> GetGridBuilder()
        {
            var blockOptions = new GridBuilderGridOptions<MediaFolderData>
            {
                LavaObject = row => row.MediaFolder
            };

            return new GridBuilder<MediaFolderData>()
                .WithBlock( this, blockOptions )
                .AddTextField( "idKey", a => a.MediaFolder.IdKey )
                .AddTextField( "name", a => a.MediaFolder.Name )
                .AddField( "isContentChannelSyncEnabled", a => a.MediaFolder.IsContentChannelSyncEnabled )
                .AddField( "watchCount", a => a.WatchCount )
                .AddField( "videoCount", a => a.VideoCount )
                .AddAttributeFieldsFrom( a => a.MediaFolder, _gridAttributes.Value );
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
            var entityTypeId = EntityTypeCache.Get<MediaFolder>( false )?.Id;

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
                var entityService = new MediaFolderService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{MediaFolder.FriendlyTypeName} not found." );
                }

                if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete ${MediaFolder.FriendlyTypeName}." );
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
        public class MediaFolderData
        {
            /// <summary>
            /// Gets or sets the whole media folder object from the database.
            /// </summary>
            /// <value>
            /// The whole media folder object from the database.
            /// </value>
            public MediaFolder MediaFolder { get; set; }

            /// <summary>
            /// Gets or sets the watch count for this media folder.
            /// </summary>
            /// <value>
            /// The  watch count for this media folder.
            /// </value>
            public int WatchCount { get; set; }

            /// <summary>
            /// Gets or sets the number of video in this media folder.
            /// </summary>
            /// <value>
            /// The number of video in this media folder.
            /// </value>
            public int VideoCount { get; set; }
        }

        #endregion
    }
}

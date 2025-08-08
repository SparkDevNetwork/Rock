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
using Rock.ViewModels.Blocks.Cms.MediaElementList;
using Rock.Web.Cache;
using Rock.Media;
using Rock.Utility;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays a list of media elements.
    /// </summary>

    [DisplayName( "Media Element List" )]
    [Category( "CMS" )]
    [Description( "Displays a list of media elements." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the media element details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.EntityTypeGuid( "9560305d-5ada-4ce4-a67a-2fe2d606cfb8" )]
    [Rock.SystemGuid.BlockTypeGuid( "a713cbd4-549e-4795-9468-828ee2f8c21d" )]
    [CustomizedGrid]
    public class MediaElementList : RockEntityListBlockType<MediaElement>
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
            public const string MediaFolderId = "MediaFolderId";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<MediaElementListOptionsBag>();
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
        private MediaElementListOptionsBag GetBoxOptions()
        {
            var options = new MediaElementListOptionsBag();

            var mediaFolderId = PageParameter( PageParameterKey.MediaFolderId ).AsIntegerOrNull();
            if ( !mediaFolderId.HasValue )
            {
                mediaFolderId = IdHasher.Instance.GetId( PageParameter( PageParameterKey.MediaFolderId ) );
            }

            if ( mediaFolderId.HasValue )
            {
                var mediaFolder = new MediaFolderService( RockContext ).Get( mediaFolderId.Value );
                if ( mediaFolder != null )
                {
                    options.MediaFolderName = mediaFolder.Name;
                }
            }

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEnabled()
        {
            var entity = new MediaElement();

            return entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var mediaFolderId = PageParameter( PageParameterKey.MediaFolderId ).AsIntegerOrNull();
            if ( !mediaFolderId.HasValue )
            {
                mediaFolderId = IdHasher.Instance.GetId( PageParameter( PageParameterKey.MediaFolderId ) );
            }

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, new Dictionary<string, string>
                {
                    { "MediaElementId", "((Key))" },
                    { PageParameterKey.MediaFolderId, mediaFolderId?.ToString() }
                } )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<MediaElement> GetListQueryable( RockContext rockContext )
        {
            var mediaElementService = new MediaElementService( rockContext );
            var mediaFolderId = PageParameter( PageParameterKey.MediaFolderId ).AsIntegerOrNull();

            if ( !mediaFolderId.HasValue )
            {
                mediaFolderId = IdHasher.Instance.GetId( PageParameter( PageParameterKey.MediaFolderId ) );
            }

            var qry = mediaElementService.Queryable().AsNoTracking();

            if ( mediaFolderId.HasValue )
            {
                qry = qry.Where( a => a.MediaFolderId == mediaFolderId.Value );
            }

            // Get interaction channel for media events
            var interactionChannelId = InteractionChannelCache.GetId( Rock.SystemGuid.InteractionChannel.MEDIA_EVENTS.AsGuid() );

            // Create a single efficient subquery for watch counts
            var watchCountsQuery = new InteractionService( rockContext )
                .Queryable()
                .Where( i => i.Operation == "Watch" &&
                            i.InteractionComponent.InteractionChannelId == interactionChannelId )
                .GroupBy( i => i.InteractionComponent.EntityId )
                .Select( g => new { EntityId = g.Key, Count = g.Count() } );

            // Join with the watch counts in a single operation
            qry = qry.GroupJoin(
                watchCountsQuery,
                m => m.Id,
                w => w.EntityId,
                ( m, w ) => new { MediaElement = m, WatchCount = w.Select( x => x.Count ).FirstOrDefault() } )
                .Select( x => x.MediaElement );

            return qry;
        }

        /// <inheritdoc/>
        protected override IQueryable<MediaElement> GetOrderedListQueryable( IQueryable<MediaElement> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( a => a.Name );
        }

        /// <inheritdoc/>
        protected override GridBuilder<MediaElement> GetGridBuilder()
        {
            return new GridBuilder<MediaElement>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.Name )
                .AddField( "durationSeconds", a => a.DurationSeconds )
                .AddField( "watchCount", a => GetWatchCount( a ) )
                .AddField( "transcribed", a => a.TranscriptionText != null && a.TranscriptionText.Trim() != string.Empty )
                .AddField( "isSecurityDisabled", a => !a.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
                .AddAttributeFields( GetGridAttributes() );
        }

        /// <summary>
        /// Gets the watch count for a media element.
        /// </summary>
        /// <param name="mediaElement">The media element.</param>
        /// <returns>The number of times the media element has been watched.</returns>
        private int GetWatchCount( MediaElement mediaElement )
        {
            var interactionChannelId = InteractionChannelCache.GetId( Rock.SystemGuid.InteractionChannel.MEDIA_EVENTS.AsGuid() );
            var rockContext = new RockContext();

            var watchCount = new InteractionService( rockContext )
                .Queryable()
                .Where( a => a.Operation == "Watch" )
                .Join(
                    new InteractionComponentService( rockContext )
                        .Queryable()
                        .Where( c => c.InteractionChannelId == interactionChannelId && c.EntityId == mediaElement.Id ),
                    i => i.InteractionComponentId,
                    c => c.Id,
                    ( i, c ) => i )
                .Count();

            return watchCount;
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
            var entityService = new MediaElementService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{MediaElement.FriendlyTypeName} not found." );
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete {MediaElement.FriendlyTypeName}." );
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

    /// <summary>
    /// View model for media element list items
    /// </summary>
    public class MediaElementListItem
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the duration in seconds.
        /// </summary>
        public int? DurationSeconds { get; set; }

        /// <summary>
        /// Gets or sets the watch count.
        /// </summary>
        public int WatchCount { get; set; }

        /// <summary>
        /// Gets or sets whether or not the element has TranscriptionText
        /// </summary>
        public bool Transcribed { get; set; }

        /// <summary>
        /// Gets or sets the IdKey.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets whether security is disabled.
        /// </summary>
        public bool IsSecurityDisabled { get; set; }
    }
}

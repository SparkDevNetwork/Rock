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
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

using Rock.Attribute;
using Rock.Cms.ContentCollection.IndexDocuments;
using Rock.Cms.ContentCollection.Search;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Cms.ContentCollection.Indexers
{
    /// <summary>
    /// Content Collection Indexer for <see cref="ContentChannelItem"/> entities.
    /// </summary>
    [RockInternal( "1.14" )]
    internal class ContentChannelItemIndexer : IContentCollectionIndexer
    {
        /// <inheritdoc/>
        public Task DeleteAllContentCollectionSourceDocumentsAsync( int sourceId )
        {
            // Delete all content channel item documents with this entity id.
            var query = new SearchQuery
            {
                IsAllMatching = true
            };

            query.Add( new SearchField
            {
                Name = nameof( IndexDocumentBase.SourceId ),
                Value = sourceId.ToString(),
                IsPhrase = true,
                IsWildcard = false
            } );

            return ContentIndexContainer.DeleteMatchingDocumentsAsync<ContentChannelItemDocument>( query );
        }

        /// <inheritdoc/>
        public Task DeleteContentCollectionDocumentAsync( int id )
        {
            // Delete all content channel item documents with this entity id.
            var query = new SearchQuery
            {
                IsAllMatching = true
            };

            query.Add( new SearchField
            {
                Name = nameof( IndexDocumentBase.EntityId ),
                Value = id.ToString(),
                IsPhrase = true,
                IsWildcard = false
            } );

            return ContentIndexContainer.DeleteMatchingDocumentsAsync<ContentChannelItemDocument>( query );
        }

        /// <inheritdoc/>
        public async Task<int> IndexAllContentCollectionSourceDocumentsAsync( int sourceId, IndexDocumentOptions options )
        {
            var contentCollectionSourceCache = ContentCollectionSourceCache.Get( sourceId );
            var contentChannelEntityTypeId = EntityTypeCache.GetId<ContentChannel>() ?? 0;
            var now = RockDateTime.Now;
            List<ContentChannelItem> items;
            Dictionary<int, int> trending;

            // Make sure the source is valid.
            if ( contentCollectionSourceCache == null || contentCollectionSourceCache.EntityTypeId != contentChannelEntityTypeId )
            {
                return 0;
            }

            using ( var rockContext = new RockContext() )
            {
                // Get all the content channel items for this source.
                items = new ContentChannelItemService( rockContext ).Queryable()
                    .AsNoTracking()
                    .Include( cci => cci.ContentChannelItemSlugs )
                    .Where( cci => cci.ContentChannelId == contentCollectionSourceCache.EntityId
                        && cci.StartDateTime <= now
                        && ( !cci.ExpireDateTime.HasValue || cci.ExpireDateTime.Value >= now ) )
                    .ToList();

                if ( !items.Any() )
                {
                    return 0;
                }

                items.LoadAttributes( rockContext );

                // If trending is enabled, get the trending ranks.
                trending = options.IsTrendingEnabled && contentCollectionSourceCache.ContentCollection.TrendingEnabled
                    ? GetTrendingRanksLookup( contentCollectionSourceCache, rockContext )
                    : null;

                // Process all items while the RockContext is still available
                // so that lazy load properties can be used in custom fields.
                // This also requires that we process items sequentially since
                // EF contexts are not thread-safe.
                foreach ( var item in items )
                {
                    try
                    {
                        var document = await ContentChannelItemDocument.LoadByModelAsync( item, contentCollectionSourceCache );

                        if ( trending != null )
                        {
                            document.IsTrending = trending.ContainsKey( document.EntityId );
                            document.TrendingRank = document.IsTrending ? trending[document.EntityId] : 0;
                        }

                        document.IsApproved = GetIsApproved( item );

                        await ContentIndexContainer.IndexDocumentAsync( document );
                    }
                    catch ( Exception ex )
                    {
                        // If a single item fails, log the exception and continue processing.
                        ExceptionLogService.LogException( ex );
                    }
                }

                return items.Count;
            }
        }

        /// <inheritdoc/>
        public async Task<int> IndexContentCollectionDocumentAsync( int id, IndexDocumentOptions options )
        {
            using ( var rockContext = new RockContext() )
            {
                var itemEntity = new ContentChannelItemService( rockContext ).GetInclude( id, ci => ci.ContentChannelItemSlugs );
                var now = RockDateTime.Now;

                // If entity wasn't found or isn't visible yet then don't index.
                if ( itemEntity == null || itemEntity.StartDateTime > now )
                {
                    return 0;
                }

                // If it has already expired, do not index.
                if ( itemEntity.ExpireDateTime.HasValue && itemEntity.ExpireDateTime.Value < now )
                {
                    return 0;
                }

                var isApproved = GetIsApproved( itemEntity );

                itemEntity.LoadAttributes( rockContext );

                // Create or update any indexed documents for content collection sources.
                var contentChannelEntityTypeId = EntityTypeCache.Get<ContentChannel>().Id;
                var sources = ContentCollectionSourceCache.All()
                    .Where( s => s.EntityTypeId == contentChannelEntityTypeId
                        && s.EntityId == itemEntity.ContentChannelId )
                    .ToList();

                foreach ( var source in sources )
                {
                    try
                    {
                        var document = await ContentChannelItemDocument.LoadByModelAsync( itemEntity, source );
                        document.IsApproved = isApproved;

                        await ContentIndexContainer.IndexDocumentAsync( document );
                    }
                    catch ( Exception ex )
                    {
                        // If a single item fails, log the exception and continue processing.
                        ExceptionLogService.LogException( ex );
                    }

                }

                return sources.Count;
            }
        }

        /// <summary>
        /// Determines the approved state of the item. This requires a bit more
        /// complex logic than normal in order to account for the various ways
        /// that an item can be considered approved.
        /// </summary>
        /// <param name="item">The content channel item to check.</param>
        /// <returns><c>true</c> if it is considered approved; otherwise <c>false</c>.</returns>
        private static bool GetIsApproved( ContentChannelItem item )
        {
            // If the item is explicitly approved, then it is approved.
            if ( item.Status == ContentChannelItemStatus.Approved )
            {
                return true;
            }

            // If there is no content channel, assume it is approved.
            if ( item.ContentChannel == null )
            {
                return true;
            }

            // If the channel doesn't require approval, then the item is approved.
            if ( !item.ContentChannel.RequiresApproval )
            {
                return true;
            }

            // If the channel type is configured to disable status, then consider
            // the item to be approved.
            if ( item.ContentChannelType != null && item.ContentChannelType.DisableStatus )
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the trending ranks lookup table for the source.
        /// </summary>
        /// <param name="source">The source whose items should be ranked.</param>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <returns>A dictionary whose key represents the entity identifier and the corresponding value is the rank.</returns>
        private static Dictionary<int, int> GetTrendingRanksLookup( ContentCollectionSourceCache source, RockContext rockContext )
        {
            var daysBack = source.ContentCollection.TrendingWindowDay;
            var gravity = source.ContentCollection.TrendingGravity;
            var maxItems = source.ContentCollection.TrendingMaxItems;
            var cutOffDate = RockDateTime.Now.AddDays( -daysBack );
            var contentChannelMediumValueId = DefinedValueCache.GetId( SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_CONTENTCHANNEL.AsGuid() );

            if ( !contentChannelMediumValueId.HasValue )
            {
                return new Dictionary<int, int>();
            }

            var interactionService = new InteractionService( rockContext );
            var contentChannelItemService = new ContentChannelItemService( rockContext );
            var cciQuery = contentChannelItemService.Queryable();

            var viewCounts = interactionService.Queryable()
                .Where( i => i.InteractionComponent.InteractionChannel.ChannelTypeMediumValueId == contentChannelMediumValueId.Value
                    && i.InteractionDateTime >= cutOffDate )
                // Join to the content channel item table to get the start date time.
                .Join( cciQuery, i => i.InteractionComponent.EntityId, cci => cci.Id, ( i, cci ) => new
                {
                    cci.Id,
                    DateTime = cci.StartDateTime
                } )
                // Group the results by the ContentChannelItem.Id and DateTime
                .GroupBy( a => new
                {
                    a.Id,
                    a.DateTime
                } )
                // Get each entity id, start date and the number of times it appeared.
                // This should result in a single row per Id.
                .Select( grp => new EntityViewCount
                {
                    Id = grp.Key.Id,
                    DateTime = grp.Key.DateTime,
                    Views = grp.Count()
                } )
                .ToList();

            return ContentCollectionSourceService.CalculateTrendingRanksLookup( viewCounts, cutOffDate, gravity, maxItems );
        }
    }
}

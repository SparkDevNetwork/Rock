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
            List<int> unapprovedItems = new List<int>();

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

                foreach( var item in items )
                {
                    // If the content channel item is not approved, set "IsApproved" to false.
                    if ( item.ContentChannel != null )
                    {
                        // If the content channel item is not approved, set "IsApproved" to false.
                        if ( item.ContentChannel.RequiresApproval && item.ApprovedDateTime == null && !item.ContentChannelType.DisableStatus )
                        {
                            unapprovedItems.Add( item.Id );
                        }
                    }
                }
            }

            // The RockContext is now closed. Any navigation properties that are
            // used by the indexer must be included in the query itself and
            // eager loaded. Navigation properties that lazy load are not
            // thread-safe.
            var processor = new ParallelProcessor( options.MaxConcurrency );

            await processor.ExecuteAsync( items, async contentChannelItem =>
            {
                var document = await ContentChannelItemDocument.LoadByModelAsync( contentChannelItem, contentCollectionSourceCache );

                if ( trending != null )
                {
                    document.IsTrending = trending.ContainsKey( document.EntityId );
                    document.TrendingRank = document.IsTrending ? trending[document.EntityId] : 0;
                }

                if ( unapprovedItems.Contains( document.EntityId ) )
                {
                    document.IsApproved = false;
                }

                await ContentIndexContainer.IndexDocumentAsync( document );
            } );

            return items.Count;
        }

        /// <inheritdoc/>
        public async Task<int> IndexContentCollectionDocumentAsync( int id, IndexDocumentOptions options )
        {
            ContentChannelItem itemEntity;

            var isApproved = true;

            using ( var rockContext = new RockContext() )
            {
                itemEntity = new ContentChannelItemService( rockContext ).Get( id );
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

                // If the content channel item is not approved, set "IsApproved" to false.
                if ( itemEntity.ContentChannel != null )
                {
                    // If the content channel item is not approved, set "IsApproved" to false.
                    if ( itemEntity.ContentChannel.RequiresApproval && itemEntity.ApprovedDateTime == null && !itemEntity.ContentChannelType.DisableStatus )
                    {
                        isApproved = false;
                    }
                }

                itemEntity.LoadAttributes( rockContext );
            }

            // The RockContext is now closed. Any navigation properties that are
            // used by the indexer must be included in the query itself and
            // eager loaded. Navigation properties that lazy load are not
            // thread-safe.

            // Create or update any indexed documents for content collection sources.
            var contentChannelEntityTypeId = EntityTypeCache.Get<ContentChannel>().Id;
            var sources = ContentCollectionSourceCache.All()
                .Where( s => s.EntityTypeId == contentChannelEntityTypeId
                    && s.EntityId == itemEntity.ContentChannelId )
                .ToList();

            var processor = new ParallelProcessor( options.MaxConcurrency );

            await processor.ExecuteAsync( sources, async source =>
            {
                var document = await ContentChannelItemDocument.LoadByModelAsync( itemEntity, source );
                document.IsApproved = isApproved;

                await ContentIndexContainer.IndexDocumentAsync( document );
            } );

            return sources.Count;
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

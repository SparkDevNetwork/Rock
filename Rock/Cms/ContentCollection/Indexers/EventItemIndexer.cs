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
    /// Content Collection Indexer for <see cref="EventItem"/> entities.
    /// </summary>
    [RockInternal( "1.14" )]
    internal class EventItemIndexer : IContentCollectionIndexer
    {
        /// <inheritdoc/>
        public Task DeleteAllContentCollectionSourceDocumentsAsync( int sourceId )
        {
            // Delete all event item documents with this entity id.
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

            return ContentIndexContainer.DeleteMatchingDocumentsAsync<EventItemDocument>( query );
        }

        /// <inheritdoc/>
        public Task DeleteContentCollectionDocumentAsync( int id )
        {
            // Delete all event item documents with this entity id.
            var query = new SearchQuery
            {
                IsAllMatching = true
            };

            query.Add( new SearchField
            {
                Name = nameof( IndexDocumentBase.EntityId ),
                Value = id.ToString()
            } );

            return ContentIndexContainer.DeleteMatchingDocumentsAsync<EventItemDocument>( query );
        }

        /// <inheritdoc/>
        public async Task<int> IndexAllContentCollectionSourceDocumentsAsync( int sourceId, IndexDocumentOptions options )
        {
            var contentCollectionSourceCache = ContentCollectionSourceCache.Get( sourceId );
            var eventCalendarEntityTypeId = EntityTypeCache.GetId<EventCalendar>() ?? 0;
            var now = RockDateTime.Now;
            List<EventItem> eventItems;
            List<int> unapprovedItems = new List<int>();

            // Make sure the source is valid.
            if ( contentCollectionSourceCache == null || contentCollectionSourceCache.EntityTypeId != eventCalendarEntityTypeId )
            {
                return 0;
            }

            using ( var rockContext = new RockContext() )
            {
                // Get all the event items for this source.
                eventItems = new EventCalendarItemService( rockContext ).Queryable()
                    .Include( eci => eci.EventItem.EventItemOccurrences )
                    .AsNoTracking()
                    .Where( eci => eci.EventCalendarId == contentCollectionSourceCache.EntityId )
                    .Select( eci => eci.EventItem )
                    .ToList()
                    .Where( ei => ei.NextStartDateTime.HasValue && ei.NextStartDateTime >= now )
                    .ToList();

                if ( !eventItems.Any() )
                {
                    return 0;
                }

                foreach( var eventItem in eventItems )
                {
                    if ( !eventItem.IsApproved )
                    {
                        unapprovedItems.Add( eventItem.Id );
                    }
                }

                eventItems.LoadAttributes( rockContext );
            }

            // The RockContext is now closed. Any navigation properties that are
            // used by the indexer must be included in the query itself and
            // eager loaded. Navigation properties that lazy load are not
            // thread-safe.

            var processor = new ParallelProcessor( options.MaxConcurrency );

            await processor.ExecuteAsync( eventItems, async eventItem =>
            {
                var indexItem = await EventItemDocument.LoadByModelAsync( eventItem, contentCollectionSourceCache );

                if( unapprovedItems.Contains( indexItem.EntityId ) )
                {
                    indexItem.IsApproved = false;
                }

                await ContentIndexContainer.IndexDocumentAsync( indexItem );
            } );

            return eventItems.Count;
        }

        /// <inheritdoc/>
        public async Task<int> IndexContentCollectionDocumentAsync( int id, IndexDocumentOptions options )
        {
            EventItem itemEntity;
            var isApproved = true;

            using ( var rockContext = new RockContext() )
            {
                itemEntity = new EventItemService( rockContext ).Queryable()
                    .Include( ei => ei.EventItemOccurrences )
                    .FirstOrDefault( ei => ei.Id == id );

                if ( itemEntity == null || !itemEntity.NextStartDateTime.HasValue || itemEntity.NextStartDateTime < RockDateTime.Now )
                {
                    return 0;
                }

                isApproved = itemEntity.IsApproved;
                itemEntity.LoadAttributes( rockContext );
            }

            // The RockContext is now closed. Any navigation properties that are
            // used by the indexer must be included in the query itself and
            // eager loaded. Navigation properties that lazy load are not
            // thread-safe.

            // Create or update any indexed documents for content collection sources.
            var eventCalendarEntityTypeId = EntityTypeCache.Get<EventCalendar>().Id;
            var calendarIds = itemEntity.EventCalendarItems
                .Select( eci => eci.EventCalendarId )
                .ToList();

            var sources = ContentCollectionSourceCache.All()
                .Where( s => s.EntityTypeId == eventCalendarEntityTypeId
                    && calendarIds.Contains( s.EntityId ) )
                .ToList();

            var processor = new ParallelProcessor( options.MaxConcurrency );

            await processor.ExecuteAsync( sources, async source =>
            {
                var indexItem = await EventItemDocument.LoadByModelAsync( itemEntity, source );
                indexItem.IsApproved = isApproved;

                await ContentIndexContainer.IndexDocumentAsync( indexItem );
            } );

            return sources.Count;
        }
    }
}

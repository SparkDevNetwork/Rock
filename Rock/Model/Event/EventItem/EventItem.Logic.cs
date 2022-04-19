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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using Rock.Data;
using Rock.UniversalSearch;
using Rock.UniversalSearch.IndexModels;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class EventItem
    {
        #region Properties

        /// <summary>
        /// Gets the next start date time.
        /// </summary>
        /// <value>
        /// The next start date time.
        /// </value>
        [NotMapped]
        public virtual DateTime? NextStartDateTime
        {
            get
            {
                return EventItemOccurrences
                    .Select( s => s.NextStartDateTime )
                    .DefaultIfEmpty()
                    .Min();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the start times.
        /// </summary>
        /// <param name="beginDateTime">The begin date time.</param>
        /// <param name="endDateTime">The end date time.</param>
        /// <returns></returns>
        public virtual List<DateTime> GetStartTimes( DateTime beginDateTime, DateTime endDateTime )
        {
            var result = new List<DateTime>();

            foreach ( var eventItemOccurrence in EventItemOccurrences )
            {
                result.AddRange( eventItemOccurrence.GetStartTimes( beginDateTime, endDateTime ) );
            }

            return result.Distinct().OrderBy( d => d ).ToList();
        }

        /// <summary>
        /// Get a list of all inherited Attributes that should be applied to this entity.
        /// </summary>
        /// <returns>A list of all inherited AttributeCache objects.</returns>
        public override List<AttributeCache> GetInheritedAttributes( Rock.Data.RockContext rockContext )
        {
            var calendarIds = this.EventCalendarItems.Select( c => c.EventCalendarId ).ToList();
            if ( !calendarIds.Any() )
            {
                return null;
            }

            var inheritedAttributes = new Dictionary<int, List<AttributeCache>>();
            calendarIds.ForEach( c => inheritedAttributes.Add( c, new List<AttributeCache>() ) );

            //
            // Check for any calendar item attributes that the event item inherits.
            //
            var calendarItemEntityType = EntityTypeCache.Get( typeof( EventCalendarItem ) );
            if ( calendarItemEntityType != null )
            {
                foreach ( var calendarItemEntityAttribute in AttributeCache
                    .GetByEntityType( calendarItemEntityType.Id )
                    .Where( a =>
                        a.EntityTypeQualifierColumn == "EventCalendarId" &&
                        calendarIds.Contains( a.EntityTypeQualifierValue.AsInteger() ) ) )
                {
                    inheritedAttributes[calendarItemEntityAttribute.EntityTypeQualifierValue.AsInteger()].Add( calendarItemEntityAttribute );
                }
            }

            //
            // Walk the generated list of attribute groups and put them, ordered, into a list
            // of inherited attributes.
            //
            var attributes = new List<AttributeCache>();
            foreach ( var attributeGroup in inheritedAttributes )
            {
                foreach ( var attribute in attributeGroup.Value.OrderBy( a => a.Order ) )
                {
                    attributes.Add( attribute );
                }
            }

            return attributes;
        }

        /// <summary>
        /// Get any alternate Ids that should be used when loading attribute value for this entity.
        /// </summary>
        /// <param name="rockContext"></param>
        /// <returns>
        /// A list of any alternate entity Ids that should be used when loading attribute values.
        /// </returns>
        [Obsolete( "Use GetAlternateEntityIdsByType instead." )]
        [RockObsolete( "1.13" )]
        public override List<int> GetAlternateEntityIds( RockContext rockContext )
        {
            //
            // Find all the calendar Ids this event item is present on.
            //
            return this.EventCalendarItems.Select( c => c.Id ).ToList();
        }

        /// <inheritdoc/>
        public override Dictionary<int, List<int>> GetAlternateEntityIdsByType( RockContext rockContext )
        {
            // Return all of the EventCalendarItems on which this event item occurs.
            var entitiesByType = new Dictionary<int, List<int>>
            {
                { EntityTypeCache.GetId( typeof(EventCalendarItem) ) ?? 0, this.EventCalendarItems.Select( c => c.Id ).ToList() }
            };
            return entitiesByType;
        }

        #region Indexing Methods

        /// <summary>
        /// Bulks the index documents.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public void BulkIndexDocuments()
        {
            var indexableItems = new List<IndexModelBase>();

            var eventItems = new EventItemService( new RockContext() )
                                .GetIndexableActiveItems()
                                .Include( i => i.EventItemAudiences )
                                .Include( i => i.EventItemOccurrences )
                                .Include( i => i.EventItemOccurrences.Select( s => s.Schedule ) )
                                .Include( i => i.EventCalendarItems.Select( c => c.EventCalendar ) )
                                .AsNoTracking()
                                .ToList();

            int recordCounter = 0;
            foreach ( var eventItem in eventItems )
            {
                var indexableEventItem = EventItemIndex.LoadByModel( eventItem );

                if ( indexableEventItem != null )
                {
                    indexableItems.Add( indexableEventItem );
                }

                recordCounter++;

                if ( recordCounter > 100 )
                {
                    IndexContainer.IndexDocuments( indexableItems );
                    indexableItems = new List<IndexModelBase>();
                    recordCounter = 0;
                }
            }

            IndexContainer.IndexDocuments( indexableItems );
        }

        /// <summary>
        /// Indexes the document.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public void IndexDocument( int id )
        {
            var eventItemEntity = new EventItemService( new RockContext() ).Get( id );

            // Check to ensure that the event item is on a calendar that is indexed
            if ( eventItemEntity != null && eventItemEntity.EventCalendarItems.Any( c => c.EventCalendar.IsIndexEnabled ) )
            {
                var indexItem = EventItemIndex.LoadByModel( eventItemEntity );
                IndexContainer.IndexDocument( indexItem );
            }
        }

        /// <summary>
        /// Deletes the indexed document.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public void DeleteIndexedDocument( int id )
        {
            Type indexType = Type.GetType( "Rock.UniversalSearch.IndexModels.EventItemIndex" );
            IndexContainer.DeleteDocumentById( indexType, id );
        }

        /// <summary>
        /// Deletes the indexed documents.
        /// </summary>
        public void DeleteIndexedDocuments()
        {
            IndexContainer.DeleteDocumentsByType<EventItemIndex>();
        }

        /// <summary>
        /// Indexes the name of the model.
        /// </summary>
        /// <returns></returns>
        public Type IndexModelType()
        {
            return typeof( EventItemIndex );
        }

        /// <summary>
        /// Gets the index filter values.
        /// </summary>
        /// <returns></returns>
        public ModelFieldFilterConfig GetIndexFilterConfig()
        {
            return new ModelFieldFilterConfig() { FilterLabel = "", FilterField = "" };

        }

        /// <summary>
        /// Gets the index filter field.
        /// </summary>
        /// <returns></returns>
        public bool SupportsIndexFieldFiltering()
        {
            return true;
        }

        #endregion

        #endregion
    }
}

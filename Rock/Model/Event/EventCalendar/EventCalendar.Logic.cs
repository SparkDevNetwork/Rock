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

using System.Data.Entity;
using System.Linq;
using Rock.Data;
using Rock.Tasks;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class EventCalendar
    {
        #region Index Methods

        /// <summary>
        /// Deletes the indexed documents by calendar.
        /// </summary>
        /// <param name="calendarId">The calendar identifier.</param>
        public void DeleteIndexedDocumentsByCalendarId( int calendarId )
        {
            // Ensure provided calendar is indexable
            var calendar = EventCalendarCache.Get( calendarId );

            if ( calendar.IsNull() || !calendar.IsIndexEnabled )
            {
                return;
            }

            // Get event items for this calendar that are ONLY on this calendar.
            // We don't want to delete items that are also on another calendar.
            var eventItems = new EventItemService( new RockContext() )
                                    .GetActiveItemsByCalendarId( calendarId )
                                    .Where( i => i.EventCalendarItems.Count() == 1 )
                                    .Select( a => a.Id ).ToList();

            int eventItemEntityTypeId = EntityTypeCache.GetId<Rock.Model.EventItem>().Value;

            foreach ( var eventItemId in eventItems )
            {
                var deleteEntityTypeIndexMsg = new DeleteEntityTypeIndex.Message
                {
                    EntityTypeId = eventItemEntityTypeId,
                    EntityId = eventItemId
                };

                deleteEntityTypeIndexMsg.Send();
            }
        }

        /// <summary>
        /// Bulks the index documents by calendar.
        /// </summary>
        /// <param name="calendarId">The calendar identifier.</param>
        public void BulkIndexDocumentsByCalendar( int calendarId )
        {
            // Ensure provided calendar is indexable
            var calendar = EventCalendarCache.Get( calendarId );

            if ( calendar.IsNull() || !calendar.IsIndexEnabled )
            {
                return;
            }

            var eventItems = new EventItemService( new RockContext() )
                                    .GetActiveItemsByCalendarId( calendarId )
                                    .Select( a => a.Id ).ToList();

            int eventItemEntityTypeId = EntityTypeCache.GetId<Rock.Model.EventItem>().Value;

            foreach ( var eventItemId in eventItems )
            {
                var deleteEntityTypeIndexMsg = new DeleteEntityTypeIndex.Message
                {
                    EntityTypeId = eventItemEntityTypeId,
                    EntityId = eventItemId
                };

                deleteEntityTypeIndexMsg.Send();
            }
        }
        #endregion

        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return EventCalendarCache.Get( this.Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            EventCalendarCache.UpdateCachedEntity( this.Id, entityState );
        }

        #endregion
    }
}

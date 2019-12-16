using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

using NewPointe.eSpace;
using ESpace = NewPointe.eSpace.Models;

namespace org.newpointe.eSpace.Utility
{
    public static class QueryExtensions
    {

        /// <summary>
        /// Gets an entity using a foreign key and id.
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="service">The entity service</param>
        /// <param name="include">A comma-delimited list of properties to eagerly load</param>
        /// <param name="foreignKey">The foreign key</param>
        /// <param name="foreignId">The foreign id</param>
        /// <returns>The entity or null if it could not be found</returns>
        public static T GetByForeignId<T>( this Service<T> service, string include, string foreignKey, int foreignId ) where T : Entity<T>, new()
        {
            return service.Queryable( include ).FirstOrDefault( e => e.ForeignKey == foreignKey && e.ForeignId == foreignId );
        }

        /// <summary>
        /// Gets an entity using a foreign key and id.
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="collection">The collection of entities</param>
        /// <param name="foreignKey">The foreign key</param>
        /// <param name="foreignId">The foreign id</param>
        /// <returns>The entity or null if it could not be found</returns>
        public static T GetByForeignId<T>( this ICollection<T> collection, string foreignKey, int foreignId ) where T : Entity<T>, new()
        {
            return collection.FirstOrDefault( e => e.ForeignKey == foreignKey && e.ForeignId == foreignId );
        }

        /// <summary>
        /// Gets or Creates an entity using a foreign key and id.
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="service">The entity service</param>
        /// <param name="foreignKey">The foreign key</param>
        /// <param name="foreignId">The foreign id</param>
        /// <returns>The entity</returns>
        public static T GetOrCreateByForeignId<T>( this Service<T> service, string include, string foreignKey, int foreignId, out bool created ) where T : Entity<T>, new()
        {
            created = false;
            var entity = service.GetByForeignId( include, foreignKey, foreignId );
            if ( entity == null )
            {
                created = true;
                entity = new T { ForeignKey = foreignKey, ForeignId = foreignId };
                service.Add( entity );
            }
            return entity;
        }

        /// <summary>
        /// Gets or Creates an entity using a foreign key and id.
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="collection">The collection of entities</param>
        /// <param name="foreignKey">The foreign key</param>
        /// <param name="foreignId">The foreign id</param>
        /// <returns>The entity</returns>
        public static T GetOrCreateByForeignId<T>( this ICollection<T> collection, string foreignKey, int foreignId, out bool created ) where T : Entity<T>, new()
        {
            created = false;
            var entity = collection.GetByForeignId( foreignKey, foreignId );
            if ( entity == null )
            {
                created = true;
                entity = new T { ForeignKey = foreignKey, ForeignId = foreignId };
                collection.Add( entity );
            }
            return entity;
        }

        /// <summary>
        /// Finds a person that matches the given eSpace contact.
        /// </summary>
        /// <param name="personService">The person service</param>
        /// <param name="contact">The eSpace contact</param>
        /// <returns>The matching Person or null if a mach could not be found</returns>
        public static Person FindPerson( this PersonService personService, ESpace.EventContact contact )
        {
            if ( contact == null ) return null;
            return personService.FindPerson( contact.FirstName, contact.LastName, contact.Email, false );
        }

        /// <summary>
        /// Adds the event to a Calendar.
        /// </summary>
        /// <param name="eventItem">The event</param>
        /// <param name="calendar">The calendar</param>
        /// <param name="created">If a new calendar item was created</param>
        /// <returns>The calendar item for the event</returns>
        public static EventCalendarItem AddToCalendar( this EventItem eventItem, EventCalendarCache calendar, out bool created )
        {
            created = false;
            if ( calendar == null ) return null;
            var calendarItem = eventItem.EventCalendarItems.FirstOrDefault( ci => ci.EventCalendarId == calendar.Id );
            if ( calendarItem == null )
            {
                created = true;
                calendarItem = new EventCalendarItem { EventCalendarId = calendar.Id };
                eventItem.EventCalendarItems.Add( calendarItem );
            }
            return calendarItem;
        }

        /// <summary>
        /// Adds the event to a Calendar.
        /// </summary>
        /// <param name="eventItem">The event</param>
        /// <param name="calendar">The calendar</param>
        /// <param name="created">If a new calendar item was created</param>
        /// <returns>The calendar item for the event</returns>
        public static void RemoveFromCalendar( this EventItem eventItem, EventCalendarCache calendar, out bool removed )
        {
            removed = false;
            if ( calendar == null ) return;
            var calendarItem = eventItem.EventCalendarItems.FirstOrDefault( ci => ci.EventCalendarId == calendar.Id );
            if ( calendarItem != null )
            {
                removed = true;
                eventItem.EventCalendarItems.Remove( calendarItem );
            }
        }

    }
}

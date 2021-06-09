using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Model
{
    public partial class EventItemService
    {

        /// <summary>
        /// Gets the active calendar items.
        /// </summary>
        /// <param name="includes">The includes.</param>
        /// <returns></returns>
        public IQueryable<EventItem> GetActiveItems()
        {
            return Queryable()
                    .Where( e => e.IsActive == true
                       && e.IsApproved == true )
                    .Where( e => e.EventCalendarItems.Any( c =>
                                   c.EventCalendar.IsActive == true ) );
        }

        /// <summary>
        /// Gets the active items by calendar identifier.
        /// </summary>
        /// <param name="calendarId">The calendar identifier.</param>
        /// <param name="includes">The includes.</param>
        /// <returns></returns>
        public IQueryable<EventItem> GetActiveItemsByCalendarId( int calendarId )
        {
            return this.GetActiveItems()
                        .Where( e => e.EventCalendarItems.Any( c =>
                                        c.EventCalendar.Id == calendarId
                                ) );
        }

        /// <summary>
        /// Gets the indexable active items.
        /// </summary>
        /// <param name="includes">The includes.</param>
        /// <returns></returns>
        public IQueryable<EventItem> GetIndexableActiveItems()
        {
            return this.GetActiveItems()
                        .Where( e => e.EventCalendarItems.Any( c =>
                                        c.EventCalendar.IsActive
                                        && c.EventCalendar.IsIndexEnabled ) );
        }
    }
}

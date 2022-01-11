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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using Ical.Net;
using Ical.Net.DataTypes;

namespace Rock.Model
{
    /// <summary>
    /// A helper class for processing iCalendar (RFC 5545) schedules.
    /// </summary>
    /// <remarks>
    /// This class uses the iCal.Net implementation of the iCalendar (RFC 5545) standard.
    /// </remarks>
    public static class InetCalendarHelper
    {
        // using MemoryCache instead RockCacheManager, since Occurrences isn't serializable.
        private static MemoryCache _iCalOccurrencesCache = new MemoryCache( "Rock.InetCalendarHelper._iCalOccurrences" );

        // only keep in memory if unused for 10 minutes. This reduces the chances of this getting too big.
        private static CacheItemPolicy cacheItemPolicy10Minutes = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMinutes( 10 ) };

        /// <summary>
        /// Gets the calendar event.
        /// </summary>
        /// <param name="iCalendarContent">RFC 5545 ICal Content</param>
        /// <returns></returns>
        [RockObsolete( "1.12.4" )]
        [Obsolete( "Use CreateCalendarEvent instead" )]
        public static Ical.Net.Event GetCalendarEvent( string iCalendarContent )
        {
            // changed to obsolete because this used to return a shared object that could be altered or create thread-safety issues
            return CreateCalendarEvent( iCalendarContent );
        }

        /// <summary>
        /// Creates the calendar event.
        /// </summary>
        /// <param name="iCalendarContent">RFC 5545 ICal Content</param>
        /// <returns></returns>
        public static Event CreateCalendarEvent( string iCalendarContent )
        {
            StringReader stringReader = new StringReader( iCalendarContent );
            var calendarList = Calendar.LoadFromStream( stringReader );
            Event calendarEvent = null;

            //// iCal is stored as a list of Calendar's each with a list of Events, etc.  
            //// We just need one Calendar and one Event
            if ( calendarList.Count() > 0 )
            {
                var calendar = calendarList[0] as Calendar;
                if ( calendar != null )
                {
                    calendarEvent = calendar.Events[0] as Event;
                }
            }

            return calendarEvent;
        }

        /// <summary>
        /// Gets the occurrences.
        /// </summary>
        /// <param name="icalEvent">The ical event.</param>
        /// <param name="startTime">The start time.</param>
        /// <returns></returns>
        [Obsolete( "Use the override with the string instead of the Ical.Net.Event." )]
        [RockObsolete( "1.12.4" )]
        public static IList<Occurrence> GetOccurrences( Ical.Net.Event icalEvent, DateTime startTime )
        {
            return icalEvent.GetOccurrences( startTime ).ToList();
        }

        /// <summary>
        /// Gets the occurrences.
        /// </summary>
        /// <param name="icalEvent">The ical event.</param>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        /// <returns></returns>
        [Obsolete( "Use the override with the string instead of the Ical.Net.Event." )]
        [RockObsolete( "1.12.4" )]
        public static IList<Occurrence> GetOccurrences( Ical.Net.Event icalEvent, DateTime startTime, DateTime endTime )
        {
            return icalEvent.GetOccurrences( startTime, endTime ).ToList();
        }

        /// <summary>
        /// Gets the occurrences for the specified iCal
        /// </summary>
        /// <param name="iCalendarContent">RFC 5545 ICal Content</param>
        /// <param name="startDateTime">The start date time.</param>
        /// <returns></returns>
        public static IList<Occurrence> GetOccurrences( string iCalendarContent, DateTime startDateTime )
        {
            return GetOccurrences( iCalendarContent, startDateTime, null );
        }

        /// <summary>
        /// Gets the occurrences.
        /// </summary>
        /// <param name="iCalendarContent">RFC 5545 ICal Content</param>
        /// <param name="startDateTime">The start date time.</param>
        /// <param name="endDateTime">The end date time.</param>
        /// <returns></returns>
        public static IList<Occurrence> GetOccurrences( string iCalendarContent, DateTime startDateTime, DateTime? endDateTime )
        {
            return GetOccurrences( iCalendarContent, startDateTime, endDateTime, null );
        }

        /// <summary>
        /// Gets the occurrences.
        /// </summary>
        /// <param name="iCalendarContent">RFC 5545 ICal Content</param>
        /// <param name="startDateTime">The start date time.</param>
        /// <param name="endDateTime">The end date time.</param>
        /// <param name="scheduleStartDateTimeOverride">The schedule start date time override.</param>
        /// <returns></returns>
        public static IList<Occurrence> GetOccurrences( string iCalendarContent, DateTime startDateTime, DateTime? endDateTime, DateTime? scheduleStartDateTimeOverride )
        {
            string occurrenceLookupKey = $"{startDateTime.ToShortDateTimeString()}__{endDateTime?.ToShortDateTimeString()}__{scheduleStartDateTimeOverride?.ToShortDateTimeString()}__{iCalendarContent.Trim()}".XxHash();

            Occurrence[] occurrenceList = _iCalOccurrencesCache.Get( occurrenceLookupKey ) as Occurrence[];

            if ( occurrenceList == null )
            {
                occurrenceList = LoadOccurrences( iCalendarContent, startDateTime, endDateTime, scheduleStartDateTimeOverride );
                _iCalOccurrencesCache.AddOrGetExisting( occurrenceLookupKey, occurrenceList, cacheItemPolicy10Minutes );
            }

            return occurrenceList;
        }

        /// <summary>
        /// Loads the occurrences.
        /// </summary>
        /// <param name="iCalendarContent">RFC 5545 ICal Content</param>
        /// <param name="startDateTime">The start date time.</param>
        /// <param name="endDateTime">The end date time.</param>
        /// <param name="scheduleStartDateTimeOverride">The schedule start date time override.</param>
        /// <returns></returns>
        private static Occurrence[] LoadOccurrences( string iCalendarContent, DateTime startDateTime, DateTime? endDateTime, DateTime? scheduleStartDateTimeOverride )
        {
            var iCalEvent = CreateCalendarEvent( iCalendarContent );
            if ( iCalEvent == null )
            {
                return new Occurrence[0];
            }

            if ( scheduleStartDateTimeOverride.HasValue )
            {
                iCalEvent.DtStart = new CalDateTime( scheduleStartDateTimeOverride.Value );
            }

            if ( endDateTime.HasValue )
            {
                return iCalEvent.GetOccurrences( startDateTime, endDateTime.Value ).ToArray();
            }
            else
            {
                return iCalEvent.GetOccurrences( startDateTime ).ToArray();
            }
        }
    }
}

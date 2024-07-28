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
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;

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
        public static CalendarEvent GetCalendarEvent( string iCalendarContent )
        {
            // changed to obsolete because this used to return a shared object that could be altered or create thread-safety issues
            return CreateCalendarEvent( iCalendarContent );
        }

        /// <summary>
        /// Creates the calendar event.
        /// </summary>
        /// <param name="iCalendarContent">RFC 5545 ICal Content</param>
        /// <returns></returns>
        public static CalendarEvent CreateCalendarEvent( string iCalendarContent )
        {
            var stringReader = new StringReader( iCalendarContent );
            var calendarList = CalendarCollection.Load( stringReader );
            CalendarEvent calendarEvent = null;

            //// iCal is stored as a list of Calendar's each with a list of Events, etc.  
            //// We just need one Calendar and one Event
            if ( calendarList.Count() > 0 )
            {
                var calendar = calendarList[0] as Calendar;
                if ( calendar != null )
                {
                    calendarEvent = calendar.Events[0] as CalendarEvent;
                }
            }

            return calendarEvent;
        }

        /// <summary>
        /// Serialize a CalendarEvent object to a string.
        /// </summary>
        /// <param name="iCalEvent"></param>
        /// <returns></returns>
        public static string SerializeToCalendarString( CalendarEvent iCalEvent )
        {
            if ( iCalEvent == null )
            {
                return string.Empty;
            }

            var iCalCalendar = new Calendar();
            iCalCalendar.Events.Add( iCalEvent );

            var serializer = new CalendarSerializer();

            var iCalString = serializer.SerializeToString( iCalCalendar );
            return iCalString;
        }

        /// <summary>
        /// Serialize a Calendar object to a string.
        /// </summary>
        /// <param name="iCalCalendar"></param>
        /// <returns></returns>
        public static string SerializeToCalendarString( Calendar iCalCalendar )
        {
            if ( iCalCalendar == null )
            {
                return string.Empty;
            }

            var serializer = new CalendarSerializer( iCalCalendar );

            var iCalString = serializer.SerializeToString();
            return iCalString;
        }

        /// <summary>
        /// Gets the occurrences.
        /// </summary>
        /// <param name="icalEvent">The ical event.</param>
        /// <param name="startTime">The start time.</param>
        /// <returns></returns>
        [Obsolete( "Use the override with the string instead of the Ical.Net.Event." )]
        [RockObsolete( "1.12.4" )]
        public static IList<Occurrence> GetOccurrences( CalendarEvent icalEvent, DateTime startTime )
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
        public static IList<Occurrence> GetOccurrences( CalendarEvent icalEvent, DateTime startTime, DateTime endTime )
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

        /// <summary>
        /// Loads the occurrences for the specified iCalendar content string, excluding the occurrence that represents
        /// the calendar event's start date/time, if it doesn't match the specified recurrence dates or recurrence rules.
        /// </summary>
        /// <remarks>
        /// For example, this is helpful when considering a group member's preferred schedule template, with respect to
        /// group scheduling. If we don't exclude start date/times that don't match the recurrence dates or rules in
        /// such scenarios, this can lead to false positives and people being scheduled when they shouldn't be.
        /// </remarks>
        /// <param name="iCalendarContent">The RFC 5545 iCalendar content string.</param>
        /// <param name="startDateTime">The start date time.</param>
        /// <param name="endDateTime">The end date time.</param>
        /// <returns>The occurrences.</returns>
        internal static Occurrence[] GetOccurrencesExcludingStartDate( string iCalendarContent, DateTime startDateTime, DateTime? endDateTime )
        {
            var occurrenceLookupKey = $"{startDateTime.ToShortDateTimeString()}__{endDateTime?.ToShortDateTimeString()}__ExcludeStartDate__{iCalendarContent.Trim()}".XxHash();

            var cachedOccurences = _iCalOccurrencesCache.Get( occurrenceLookupKey ) as Occurrence[];
            if ( cachedOccurences != null )
            {
                return cachedOccurences;
            }

            var iCalEvent = CreateCalendarEvent( iCalendarContent );
            if ( iCalEvent == null )
            {
                return new Occurrence[0];
            }

            /*
                8/26/2024 - JPH

                Issue #1:
                ---------
                The iCal.NET library we use to manage iCalendar events has a known issue where recurring events can
                incorrectly-generate an extra occurrence for the specified `DtStart` date/time value, even if:

                    1) `DtStart` doesn't qualify as a recurrence (for events with recurrence rules)
                    2) `DtStart` doesn't appear in the list of specific dates (for events with recurrence dates)

                https://github.com/rianjs/ical.net/issues/431

                Furthermore, the iCal.NET library is no longer maintainted, so it's clear we cannot count on this
                issue being fixed on their end any time soon.

                Workaround #1:
                --------------
                The `CalendarEvent` Type is an implementation of the `RecurringComponent` Type, which has a
                `EvaluationIncludesReferenceDate` property that dictates whether to add the `DtStart` date/time
                value as an `Occurrence` in the returned set of occurrences. Simply put, we ultimately need to set
                this property to `false`. But the problem is that it's a readonly property, with the `CalendarEvent`
                Type setting it to `true` upon instantiation. Luckily, the base `RecurringComponent` Type sets this
                property to `false` upon instantiation.

                All we need to do is create a new instance of `RecurringComponent`, copy all of the `CalendarEvent`
                instance's property values over (using their handy `CopyFrom()` method) and get a new, refined set
                of occurrences. We can then compare the two `Occurrence` sets and remove those occurrences from the
                former set that don't appear in the latter. The reason we don't want to simply RETURN the latter
                occurrence set directly, is because the iCal.NET library attaches the underlying Type instance to
                each occurrence's `Source` property, and in local testing, returning an occurrence set with
                `RecurringComponent` source objects can lead to unhandled exceptions being thrown by some downstream
                Rock processes; better to play it safe and return the same object graph structure that's
                historically been returned from this method.

                Issue #2:
                ---------
                Related to Issue #1, events having recurrence rules that include a specified count (i.e. "End after n
                occurrences"), will incorrectly count the `DtStart` occurrence as one of the "n" occurrences, which can
                lead to the final, correct occurrence being excluded from the returned set (because the library thinks
                it has already returned enough occurrences, since it incorrectly included the `DtStart` date/time value
                as an occurrence.

                Workaround #2:
                --------------
                We'll simply increase each recurrence rule count by 1 before getting the first ['CalendarEvent`] set of
                occurrences, then reduce each recurrence rule count back to the original value before getting the second
                [`RecurringComponent`] set of occurrences. This will ensure that the final, correct occurrence doesn't
                get chopped from the set, and will be returned from this method.

                Reason: Recurring Schedules sometimes return incorrect occurrences.
                https://github.com/SparkDevNetwork/Rock/issues/5980
            */

            // Temporarily increase each recurrence rule count by 1, so we don't accidentally exclude the last, correct
            // occurrence from the final set.
            var rulesWithCounts = iCalEvent.RecurrenceRules?.Where( rr => rr.Count > 0 ).ToList();
            if ( rulesWithCounts?.Any() == true )
            {
                rulesWithCounts.ForEach( rr => rr.Count++ );
            }

            // Get the original set of occurrences (which might incorrectly include the `DtStart` date/time value).
            var occurrenceSet = endDateTime.HasValue
                ? iCalEvent.GetOccurrences( startDateTime, endDateTime.Value )
                : iCalEvent.GetOccurrences( startDateTime );

            if ( iCalEvent.RecurrenceRules?.Any() == true || iCalEvent.RecurrenceDates?.Any() == true )
            {
                // Copy the `CalendarEvent` property values into a new `RecurringComponent` instance.
                var recurringComponent = new RecurringComponent();
                recurringComponent.CopyFrom( iCalEvent );

                // Return each recurrence rule count to its original value, since the `RecurringComponent` object won't
                // incorrectly include a non-matching `DtStart` date/time value as an occurrence.
                rulesWithCounts = recurringComponent.RecurrenceRules?.Where( rr => rr.Count > 1 ).ToList();
                if ( rulesWithCounts?.Any() == true )
                {
                    rulesWithCounts.ForEach( rr => rr.Count-- );
                }

                // Get the 2nd set of occurrences (which will not incorrectly include the `DtStart` date/time value).
                var recurringOccurrences = endDateTime.HasValue
                    ? recurringComponent.GetOccurrences( startDateTime, endDateTime.Value )
                    : recurringComponent.GetOccurrences( startDateTime );

                // Refine the final set of occurrences to only those that appear in both sets.
                occurrenceSet = occurrenceSet
                    .Where( o =>
                        o.Period?.StartTime?.Value != null
                        && recurringOccurrences.Any( ro => ro.Period?.StartTime?.Value == o.Period.StartTime.Value )
                    )
                    .OrderBy( o => o.Period.StartTime.Value )
                    .ToHashSet();
            }

            var occurrences = occurrenceSet.ToArray();

            _iCalOccurrencesCache.AddOrGetExisting( occurrenceLookupKey, occurrences, cacheItemPolicy10Minutes );

            return occurrences;
        }
    }
}

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
using System.Text.RegularExpressions;

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

            // iCal is stored as a list of calendars, each with a list of events.
            // Since Rock's schedule entity represents a single event, we just need one calendar and one event.
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

            var cachedOccurrences = _iCalOccurrencesCache.Get( occurrenceLookupKey ) as Occurrence[];
            if ( cachedOccurrences != null )
            {
                return cachedOccurrences;
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

                Furthermore, the iCal.NET library is no longer maintained, so it's clear we cannot count on this
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

        #region Serialization for Export

        /// <summary>
        /// Serializes the provided iCal.NET calendar for export to third-party calendar applications. Minor corrections
        /// will be made as needed, to ensure maximum compatibility with each supported calendar application's nuances.
        /// </summary>
        /// <param name="calendar">The iCal.NET calendar.</param>
        /// <returns>The serialized RFC 5545 iCalendar content.</returns>
        internal static string SerializeCalendarForExport( Calendar calendar )
        {
            /*
                10/25/2024 - JPH

                The following issues are with v4.2.0 of iCal.NET. If we upgrade to a later version, we should carefully
                test our calendar feeds to ensure we don't introduce regressions. The plan is to circle back and add
                thorough unit test coverage around this output, but until then.. be careful!

                Issue #1:
                ---------
                The iCal.NET library incorrectly adds a duration specifier to exception dates within its serialized
                iCalendar string, which causes third-party calendar apps to ignore exception dates when importing
                calendar feeds from Rock. For example:

                    EXDATE:20241220/P1D,20241221/P1D

                Each date should OMIT the "/P1D" duration specifier, like this:

                    EXDATE:20241220,20241221

                Additionally, the Obsidian code responsible for serializing Rock Schedules to an iCalendar string does
                the same, to match the way Rock has always done this.

                    See: https://github.com/SparkDevNetwork/Rock/blob/cd5f181a8a28f65c203b15c74cab350fde6d0263/Rock.JavaScript.Obsidian/Framework/Utility/internetCalendar.ts#L946

                Issue #2:
                ---------
                For events that aren't all day, the time portion of each exclusion date must match that of the calendar
                event. Also, the list that holds these days must specify a time zone that matches that of the calendar
                event (regardless of whether it's an all day event). Otherwise, third-party calendar apps often ignore
                these exclusions altogether.

                Additional Consideration:
                -------------------------
                Even if the iCal.NET library fixes these issues on their end, it's not ideal to load and re-save all
                Schedule records across all Rock instance databases, so we'll instead handle this on the fly, when
                sending calendar feeds out the door.

                Reason: Calendar Exclusions Not Acknowledged Within Calendar Feeds
                https://github.com/SparkDevNetwork/Rock/issues/6024
                https://github.com/ical-org/ical.net/issues/239
            */

            NormalizeExceptionDates( calendar );

            var serializer = new CalendarSerializer();
            var iCalendarContent = serializer.SerializeToString( calendar );

            return RemoveExceptionDateDurationSpecifiers( iCalendarContent );
        }

        /// <summary>
        /// Normalizes exception dates within the provided iCal.NET calendar, to ensure third-party calendar apps will
        /// properly acknowledge these exclusions.
        /// </summary>
        /// <param name="calendar">The calendar whose exclusion dates should be normalized in place.</param>
        private static void NormalizeExceptionDates( Calendar calendar )
        {
            if ( calendar?.Events?.Any() != true )
            {
                return;
            }

            foreach ( var calendarEvent in calendar.Events )
            {
                if ( calendarEvent.DtStart == null || calendarEvent?.ExceptionDates?.Any() != true )
                {
                    continue;
                }

                // Take note of the following metadata about the calendar event, so we can add exception dates with
                // matching metadata below. This is the key to ensuring compatibility with all third-party calendar
                // apps supported by Rock.
                var eventStart = calendarEvent.DtStart;
                var eventIsAllDay = calendarEvent.IsAllDay;

                var eventTimeZoneId = eventStart.TzId;
                var eventStartTime = new TimeSpan( eventStart.Hour, eventStart.Minute, eventStart.Second );

                // Create a new list to hold the normalized exception dates, with a time zone matching that of the
                // calendar event.
                var exceptionDates = new PeriodList()
                {
                    TzId = eventTimeZoneId
                };

                if ( eventIsAllDay )
                {
                    // Even though we've set the time zone on the exception dates list above, it will not be included
                    // in the resulting iCalendar string for all day events unless we manually add it here.
                    exceptionDates.Parameters.Set( "TZID", eventTimeZoneId );

                    // The default for the `EXDATE;VALUE` parameter is `DATE-TIME`, but when we're adding exception
                    // dates for an all day event, we need to override this to be `DATE`. There appears to be no
                    // reliable way to instruct iCal.NET to generate this value, so we'll just set it manually.
                    //
                    // For the RFC 5545 spec, see:
                    // https://datatracker.ietf.org/doc/html/rfc5545#section-3.8.5.1
                    //
                    // For the only place in iCal.NET that explicitly sets a value of `DATE`, see:
                    // https://github.com/ical-org/ical.net/blob/efa41fb8ce950b4a771bba3d544c521138c86e17/src/Ical.Net/Serialization/DataTypes/DateTimeSerializer.cs#L71-L78
                    exceptionDates.Parameters.Set( "VALUE", "DATE" );
                }

                // Set aside the existing exception dates; we'll re-add them later.
                var exceptionDatePeriods = calendarEvent.ExceptionDates
                    .SelectMany( periodList => periodList.Select( period => period ) )
                    .Where( period => period?.StartTime?.Value != null )
                    .OrderBy( period => period.StartTime.Value )
                    .ToList();

                // Clear out the existing exception dates.
                calendarEvent.ExceptionDates.Clear();

                foreach ( var exceptionDatePeriod in exceptionDatePeriods )
                {
                    // For each exception date, the iCal.NET `PeriodListSerializer` chooses between either the
                    // `PeriodSerializer` or `DateTimeSerializer` based on whether a given `Period` has an `EndTime`.
                    // Unfortunately, they've got some crazy "gotcha" logic that makes it seemingly impossible to clear
                    // out a given `EndTime` value.
                    //
                    // See:
                    //  (choosing serializer) https://github.com/ical-org/ical.net/blob/efa41fb8ce950b4a771bba3d544c521138c86e17/src/Ical.Net/Serialization/DataTypes/PeriodListSerializer.cs#L34-L44
                    //  (property setters with side effects) https://github.com/ical-org/ical.net/blob/efa41fb8ce950b4a771bba3d544c521138c86e17/src/Ical.Net/DataTypes/Period.cs#L87-L157
                    //
                    // This means we need to ultimately use Regex to produce our final output, but we'll get as close as
                    // we can using their serializers first.

                    // Start with a raw date time value.
                    var exceptionDateTime = exceptionDatePeriod.StartTime.Value.Date;

                    Period period;

                    if ( !eventIsAllDay )
                    {
                        // Only add a start time if it's not an all date event.
                        exceptionDateTime = exceptionDateTime.Add( eventStartTime );

                        period = new Period( new CalDateTime( exceptionDateTime )
                        {
                            TzId = eventTimeZoneId,
                            HasTime = true
                        } );
                    }
                    else
                    {
                        // For all day events, add only the start date with no time zone.
                        period = new Period( new CalDateTime( exceptionDateTime ) );
                    }

                    exceptionDates.Add( period );
                }

                // Re-add the normalized exception dates.
                calendarEvent.ExceptionDates.Add( exceptionDates );
            }
        }

        /// <summary>
        /// Removes any "/P1D" duration specifiers from exception dates within the provided iCalendar content string.
        /// </summary>
        /// <param name="iCalendarContent">The iCalendar content string to search for invalid duration specifiers.</param>
        /// <returns>The iCalendar content string with the invalid duration specifiers removed.</returns>
        private static string RemoveExceptionDateDurationSpecifiers( string iCalendarContent )
        {
            // Match only EXDATE lines that contain "/P1D", including any folded lines.
            //
            // EXDATE                     # Matches the literal string "EXDATE"
            // (?:;[^:=]+=[^:;]+)*        # Non-capturing group to match zero or more parameters, e.g., ";TZID=America/Phoenix;VALUE=DATE"
            // :                          # Matches the colon that separates parameters from the value
            // (?:                        # Start non-capturing group to match the property value and folded lines
            //     [^\r\n]*               # Matches any characters except CR and LF up to the end of the line
            //     (?:/P1D)               # Ensures at least one instance of "/P1D" is present in the line
            //     [^\r\n]*               # Matches any remaining characters on the line
            //     (?:\r?\n\s+)?          # Matches line breaks and whitespace for folded lines, if needed
            // )*                         # Allows zero or more folded lines by repeating the non-capturing group
            var propertyRegexPattern = @"EXDATE(?:;[^:=]+=[^:;]+)*:(?:[^\r\n]*(?:/P1D)[^\r\n]*(?:\r?\n\s+)?)*";

            // Perform a global replace to correct all offending EXDATE lines.
            var newICalendarContent = Regex.Replace( iCalendarContent, propertyRegexPattern, match =>
            {
                // Unfold the lines: Remove line breaks followed by any whitespace.
                var fullPropertyLine = Regex.Replace( match.Value, @"\r?\n\s+", "" );

                // Remove any instances of "/P1D" from the consolidated EXDATE line.
                fullPropertyLine = fullPropertyLine.Replace( "/P1D", "" );

                // Refold the shortened line if needed.
                return FoldLine( fullPropertyLine );
            } );

            return newICalendarContent;
        }

        /// <summary>
        /// Folds the line at 75 characters, and prepends the next line with a space per RFC https://tools.ietf.org/html/rfc5545#section-3.1.
        /// </summary>
        /// <remarks>
        /// This is a copy of an iCal.NET internal method (with minor modifications).
        /// See: https://github.com/ical-org/ical.net/blob/efa41fb8ce950b4a771bba3d544c521138c86e17/src/Ical.Net/Utility/TextUtil.cs#L11-L28
        /// </remarks>
        /// <param name="line">The line to fold.</param>
        /// <returns>The folded line.</returns>
        private static string FoldLine( string line )
        {
            // The spec says nothing about trimming, but it seems reasonable...
            var trimmed = line.Trim();
            if ( trimmed.Length <= 75 )
            {
                return trimmed;
            }

            const int takeLimit = 74;

            var firstLine = trimmed.Substring( 0, takeLimit );
            var remainder = trimmed.Substring( takeLimit, trimmed.Length - takeLimit );

            var chunkedRemainder = string.Join( SerializationConstants.LineBreak + " ", Chunk( remainder ) );
            return firstLine + SerializationConstants.LineBreak + " " + chunkedRemainder;
        }

        /// <summary>
        /// Breaks the provided string into chunks of the specified character count.
        /// </summary>
        /// <remarks>
        /// This is a copy of an iCal.NET internal method.
        /// See: https://github.com/ical-org/ical.net/blob/efa41fb8ce950b4a771bba3d544c521138c86e17/src/Ical.Net/Utility/TextUtil.cs#L30-L36
        /// </remarks>
        /// <param name="str">The string to be chunked.</param>
        /// <param name="characterCount">The count of characters each chunk should contain.</param>
        /// <returns>Chunks of the specified character count.</returns>
        private static IEnumerable<string> Chunk( string str, int characterCount = 73 )
        {
            for ( var index = 0; index < str.Length; index += characterCount )
            {
                yield return str.Substring( index, Math.Min( characterCount, str.Length - index ) );
            }
        }

        #endregion Serialization for Export
    }
}

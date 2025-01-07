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
using System.Linq;

using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;

using Rock;
using Rock.Data;

using TimeZoneConverter;

using Calendar = Ical.Net.Calendar;

namespace Rock.Model
{
    public partial class EventCalendarService
    {
        #region GetEventCalendarFeed

        /// <summary>
        /// Creates an iCalendar string for the specified events.
        /// </summary>
        /// <param name="args">The settings used to create the iCalendar.</param>
        /// <returns>An iCalendar string for the specified events.</returns>
        public string CreateICalendar( GetCalendarEventFeedArgs args )
        {
            /*
                1/16/2024 - DJL

                Extreme caution should be exercised when modifying the sequence or format of any output rendered in this section.
                Most third-party applications have strict requirements for imported calendars, and only partially implement the iCalendar standard.
                Changes that affect the format of the output should be tested against these major applications, in order of known difficulties:

                    1. Outlook Web
                    2. Apple Calendar (macOS, iPadOS, iOS)
                    3. Google Calendar
                    4. Outlook Desktop 365

                Reason: Calendar apps only partially implement the iCalendar standard.
             */

            // Get a list of Rock Calendar Events that match the specified filter.
            var eventItems = GetEventItems( args );

            // Create the iCalendar.
            var iCalendar = new Calendar();

            // Specify the calendar time zone using the Internet Assigned Numbers Authority (IANA) identifier, because
            // most third-party applications require this to interpret event times correctly.
            var timeZoneId = TZConvert.WindowsToIana( RockDateTime.OrgTimeZoneInfo.Id );

            // If the client is Outlook, do not set the basic event description property.
            var setEventDescription = ( args.ClientDeviceType != "Outlook" );
            var lavaTemplate = args.EventCalendarLavaTemplate;

            // Keep track of the earliest event date/time, so we can use it to set the calendar's time zone info below.
            var earliestEventDateTime = RockDateTime.Now;

            foreach ( EventItem eventItem in eventItems )
            {
                // Calculate a sequence number for the event item.
                var eventItemSequenceNo = GetSequenceNumber( eventItem.CreatedDateTime, eventItem.ModifiedDateTime );

                foreach ( EventItemOccurrence eventItemOccurrence in eventItem.EventItemOccurrences )
                {
                    if ( eventItemOccurrence.Schedule == null )
                    {
                        continue;
                    }

                    // Calculate a sequence number for the event item occurrence.
                    //
                    // The `CalendarEvent.Sequence` represents the revision number for a specific event occurrence. Many
                    // calendaring applications will not update an existing event with the same `CalendarEvent.Uid`
                    // unless the sequence number is greater than the last time an event with the same unique ID was
                    // sent. We assign a sequence number based on the number of seconds difference between the dates on
                    // which the Rock event component instances were first created and last modified. Furthermore, there
                    // are multiple Rock components within a given event object graph to consider; the final sequence
                    // number will be the highest of all sequence numbers calculated within a given event object graph.
                    // For more information, refer to https://icalendar.org/iCalendar-RFC-5545/3-8-7-4-sequence-number.html.
                    var sequenceNo = eventItemSequenceNo;

                    var occurrenceSequenceNo = GetSequenceNumber( eventItemOccurrence.CreatedDateTime, eventItemOccurrence.ModifiedDateTime );
                    if ( sequenceNo < occurrenceSequenceNo )
                    {
                        sequenceNo = occurrenceSequenceNo;
                    }

                    var scheduleSequenceNo = GetSequenceNumber( eventItemOccurrence.Schedule.CreatedDateTime, eventItemOccurrence.Schedule.ModifiedDateTime );
                    if ( sequenceNo < scheduleSequenceNo )
                    {
                        sequenceNo = scheduleSequenceNo;
                    }

                    var startDateTimesAccordingToRock = eventItemOccurrence.GetStartTimes( args.StartDate, args.EndDate );
                    if ( !startDateTimesAccordingToRock.Any() )
                    {
                        continue;
                    }

                    var firstStartDateTime = startDateTimesAccordingToRock.OrderBy( dt => dt ).First();
                    if ( firstStartDateTime < earliestEventDateTime )
                    {
                        earliestEventDateTime = firstStartDateTime;
                    }

                    var calendarEvent = InetCalendarHelper.CreateCalendarEvent( eventItemOccurrence.Schedule.iCalendarContent );
                    if ( calendarEvent?.Start == null )
                    {
                        continue;
                    }

                    calendarEvent.Sequence = sequenceNo;

                    // Create a new calendar event copy to prevent thread-safety issues. This might not be a legitimate
                    // concern, but we've historically done this, so it doesn't hurt to leave this behavior in place.
                    calendarEvent = CopyCalendarEvent( calendarEvent );

                    // Fill out the calendar event's details from this event item occurrence.
                    SetCalendarEventDetailsFromRockEvent( calendarEvent, eventItemOccurrence, lavaTemplate, setEventDescription );

                    // In many cases, we can simply use a schedule's iCalendar (RFC 5545) definition to export a given
                    // event to the external calendar apps. Testing has proven, however, that some recurring event
                    // definitions are not handled consistently among the calendar apps supported by Rock. In these
                    // cases, we need to take more of a manual approach to ensure each all calendar apps ultimately
                    // reflects Rock's internal event calendar behavior.

                    if ( calendarEvent.RecurrenceDates?.Any() == true )
                    {
                        // Outlook (Web + Desktop) & Apple calendars don't properly add iCalendar events having specific
                        // recurrence dates; we'll add these recurrences manually to play it safe.
                        calendarEvent.RecurrenceDates.Clear();

                        // Rock's schedule builder only allows EITHER recurrence dates OR recurrence rules to be set;
                        // Let's clear out the recurrence rules just to be sure we know what we're working with.
                        calendarEvent.RecurrenceRules.Clear();

                        foreach ( var startDateTime in startDateTimesAccordingToRock )
                        {
                            var recurrenceCalendarEvent = CopyCalendarEvent( calendarEvent );
                            recurrenceCalendarEvent.Uid = $"{calendarEvent.Uid}_{startDateTime:s}";
                            recurrenceCalendarEvent.DtStart = ConvertToCalDateTime( startDateTime, timeZoneId );

                            SetCalendarEventDateTimeInfo( recurrenceCalendarEvent, timeZoneId );
                            iCalendar.Events.Add( recurrenceCalendarEvent );
                        }

                        // This event item occurrence has been manually added; move on to the next one.
                        continue;
                    }

                    if ( calendarEvent.RecurrenceRules?.Any() == true )
                    {
                        // The various calendar apps supported by Rock all handle recurrence rule-based iCalendar events
                        // slightly differently, when the event's start date itself doesn't follow the recurrence rules
                        // (i.e. a Rock schedule whose start date is on a Friday, but is scheduled to repeat every
                        // Saturday thereafter).
                        //
                        // To determine if this particular event is one of these scenarios, check to see if the schedule's
                        // start date follows the recurrence rules (by checking for an occurrence on the start date).
                        var startDateTime = calendarEvent.Start.Value;
                        var startDateOccurrences = InetCalendarHelper.GetOccurrencesExcludingStartDate(
                            eventItemOccurrence.Schedule.iCalendarContent,
                            startDateTime.StartOfDay(),
                            startDateTime.EndOfDay()
                        );

                        if ( !startDateOccurrences.Any() )
                        {
                            // Add the start date as a one-time event (it will be disconnected from the rest of the series).
                            var startDateCalendarEvent = CopyCalendarEvent( calendarEvent );
                            startDateCalendarEvent.Uid = $"{calendarEvent.Uid}_{startDateTime:s}";
                            startDateCalendarEvent.RecurrenceRules.Clear();

                            SetCalendarEventDateTimeInfo( startDateCalendarEvent, timeZoneId );
                            iCalendar.Events.Add( startDateCalendarEvent );

                            // If - for some reason - the start date was the only recurrence (should never happen),
                            // there are no more recurrences to add, so move on to the next event item occurrence.
                            if ( startDateTimesAccordingToRock.Count < 2 )
                            {
                                continue;
                            }

                            // Reassign the calendar event's start date to that of the first recurrence that matches the
                            // recurrence rules (bypass the original start date).
                            calendarEvent.DtStart = ConvertToCalDateTime( startDateTimesAccordingToRock[1], timeZoneId );

                            // Reduce any recurrence rule counts by 1, to account for the start date recurrence we already
                            // added manually, above.
                            var rulesWithCounts = calendarEvent.RecurrenceRules.Where( rr => rr.Count > 0 ).ToList();
                            foreach ( var rule in rulesWithCounts )
                            {
                                rule.Count--;

                                // If this rule's count is now zero, continue to the next event item occurrence.
                                // This would be indicative of a poorly-written recurring schedule, with a
                                // "End after 1 occurrences" rule.
                                if ( rule.Count == 0 )
                                {
                                    continue;
                                }
                            }

                            SetCalendarEventDateTimeInfo( calendarEvent, timeZoneId );
                            iCalendar.Events.Add( calendarEvent );

                            // This event item occurrence has been manually added; move on to the next one.
                            continue;
                        }

                        // Else, this event is safe to add as a standard iCalendar event below, since its start date
                        // follows the recurrence rules.
                    }

                    // One-time events and recurrence rule-based events whose start date follows the recurrence rules
                    // can be added as a standard iCalendar event, as all supported calendar apps handle such events in
                    // a manner that matches Rock's internal event calendar behavior.
                    SetCalendarEventDateTimeInfo( calendarEvent, timeZoneId );
                    iCalendar.Events.Add( calendarEvent );
                }
            }

            // Find a non-DST date to use as the earliest supported time zone date, also ensuring that it is not a leap-day.
            // This is necessary to work around a bug in the iCal.Net framework (v4.2.0).
            // See https://github.com/rianjs/ical.net/issues/439.
            var tzInfo = TZConvert.GetTimeZoneInfo( timeZoneId );
            if ( tzInfo.SupportsDaylightSavingTime )
            {
                for ( var i = 0; i < 365; i++ )
                {
                    if ( !tzInfo.IsDaylightSavingTime( earliestEventDateTime ) )
                    {
                        break;
                    }
                    earliestEventDateTime = earliestEventDateTime.AddDays( -1 );
                };
            }

            // Ensure that the target date is not a leap-day.
            // This is necessary to work around a bug in the iCal.Net framework (v4.2.0).
            if ( earliestEventDateTime.Month == 2 && earliestEventDateTime.Day == 29 )
            {
                earliestEventDateTime = earliestEventDateTime.AddDays( -1 );
            }

            iCalendar.AddTimeZone( VTimeZone.FromDateTimeZone( timeZoneId, earliestEventDateTime, includeHistoricalData: true ) );

            // Return a serialized iCalendar.
            var iCalendarString = InetCalendarHelper.SerializeCalendarForExport( iCalendar );

            return iCalendarString;
        }

        /// <summary>
        /// Uses the filter information in the calendar props object to get a list of event items.
        /// </summary>
        /// <param name="calendarProps">The calendar props.</param>
        /// <returns>The matching event items.</returns>
        private List<EventItem> GetEventItems( GetCalendarEventFeedArgs calendarProps )
        {
            var rockContext = new RockContext();

            var eventCalendar = new EventCalendarService( rockContext ).Get( calendarProps.CalendarId );
            if ( eventCalendar == null )
            {
                throw new Exception( $"Invalid Calendar reference. [CalendarId={calendarProps.CalendarId}]" );
            }

            if ( calendarProps.StartDate > calendarProps.EndDate )
            {
                throw new Exception( $"Invalid Date Range. Start Date must be prior to End Date [StartDate={calendarProps.StartDate}, EndDate={calendarProps.EndDate}]" );
            }

            var eventCalendarItemQuery = new EventCalendarItemService( rockContext )
                .Queryable()
                .Where( eventCalendarItem => eventCalendarItem.EventCalendarId == calendarProps.CalendarId );

            // Filter by event item(s).
            if ( calendarProps.EventItemIds?.Any() == true )
            {
                eventCalendarItemQuery = eventCalendarItemQuery.Where( eventCalendarItem =>
                    calendarProps.EventItemIds.Contains( eventCalendarItem.EventItemId )
                );
            }

            var eventItemIdsQuery = eventCalendarItemQuery.Select( eventCalendarItem => eventCalendarItem.EventItemId );

            var eventItemQuery = new EventItemService( rockContext )
                .Queryable( "EventItemAudiences, EventItemOccurrences.Schedule, EventItemOccurrences.ContactPersonAlias.Person" )
                .Where( eventItem =>
                    eventItem.IsActive
                    && eventItem.IsApproved
                    && eventItemIdsQuery.Contains( eventItem.Id )
                    && eventItem.EventItemOccurrences.Any( occurrence =>
                        (
                            occurrence.Schedule.EffectiveStartDate == null
                            || occurrence.Schedule.EffectiveStartDate <= calendarProps.EndDate
                        )
                        && (
                            occurrence.Schedule.EffectiveEndDate == null
                            || occurrence.Schedule.EffectiveEndDate >= calendarProps.StartDate
                        )
                    )
                );

            // Filter by campus(es).
            if ( calendarProps.CampusIds?.Any() == true )
            {
                eventItemQuery = eventItemQuery.Where( eventItem =>
                    eventItem.EventItemOccurrences.Any(
                        occurrence => !occurrence.CampusId.HasValue
                        || calendarProps.CampusIds.Contains( occurrence.CampusId.Value )
                    )
                );
            }

            // Filter by audience(s).
            if ( calendarProps.AudienceIds?.Any() == true )
            {
                eventItemQuery = eventItemQuery.Where( eventItem =>
                    eventItem.EventItemAudiences.Any( audience =>
                        calendarProps.AudienceIds.Contains( audience.DefinedValueId )
                    )
                );
            }

            return eventItemQuery.ToList();
        }

        /// <summary>
        /// Create a sequence number that indicates the age of an item based on when it was created and last modified.
        /// </summary>
        /// <param name="createdDateTime">The item's created date/time.</param>
        /// <param name="modifiedDateTime">The items last modified date/time.</param>
        /// <returns>A sequence number that indicates the age of an item.</returns>
        private int GetSequenceNumber( DateTime? createdDateTime, DateTime? modifiedDateTime )
        {
            var minCreatedDateTime = RockDateTime.New( 2020, 1, 1 ).Value;

            createdDateTime = createdDateTime ?? minCreatedDateTime;
            if ( createdDateTime < minCreatedDateTime )
            {
                createdDateTime = minCreatedDateTime;
            }

            modifiedDateTime = modifiedDateTime ?? createdDateTime;
            if ( modifiedDateTime < createdDateTime )
            {
                modifiedDateTime = createdDateTime;
            }

            var sequenceNo = ( int ) modifiedDateTime.Value.Subtract( createdDateTime.Value ).TotalSeconds;
            return sequenceNo;
        }

        /// <summary>
        /// Copies a calendar event to a new instance to prevent thread-safety issues.
        /// </summary>
        /// <param name="iCalEvent">The iCal.NET calendar event.</param>
        /// <returns>A copy of the provided calendar event.</returns>
        private CalendarEvent CopyCalendarEvent( CalendarEvent iCalEvent )
        {
            // The iCal.Net serializer is not thread-safe, so we need to create a new instance for each serialization.
            // See https://github.com/rianjs/ical.net/issues/553.
            var serializer = new CalendarSerializer();
            var iCalString = serializer.SerializeToString( iCalEvent );

            var eventCopy = Calendar.Load<CalendarEvent>( iCalString )
                .FirstOrDefault();

            return eventCopy;
        }

        /// <summary>
        /// Sets additional event details on the calendar event, from the corresponding Rock event item occurrence.
        /// </summary>
        /// <param name="iCalEvent">The iCal.NET calendar event.</param>
        /// <param name="eventItemOccurrence">The corresponding event item occurrence.</param>
        /// <param name="lavaTemplate">The lava template to be used to create the event description.</param>
        /// <param name="setEventDescription">Whether to set the description directly on the calendar event object.</param>
        private void SetCalendarEventDetailsFromRockEvent( CalendarEvent iCalEvent, EventItemOccurrence eventItemOccurrence, string lavaTemplate, bool setEventDescription )
        {
            var eventItem = eventItemOccurrence.EventItem;

            iCalEvent.Summary = !string.IsNullOrEmpty( eventItem.Name ) ? eventItem.Name : string.Empty;
            iCalEvent.Location = !string.IsNullOrEmpty( eventItemOccurrence.Location ) ? eventItemOccurrence.Location : string.Empty;
            iCalEvent.Uid = eventItemOccurrence.Guid.ToString();

            // Rock has more descriptions than iCal, so lets concatenate them.
            var description = CreateEventDescription( lavaTemplate, eventItem, eventItemOccurrence );
            if ( description.IsNullOrWhiteSpace() )
            {
                description = string.Empty;
            }

            // Don't set the description prop for outlook, to force it to use the X-ALT-DESC property which can have markup.
            if ( setEventDescription )
            {
                iCalEvent.Description = description.ConvertBrToCrLf()
                                                    .Replace( "</P>", "" )
                                                    .Replace( "</p>", "" )
                                                    .Replace( "<P>", Environment.NewLine )
                                                    .Replace( "<p>", Environment.NewLine )
                                                    .Replace( "&nbsp;", " " )
                                                    .SanitizeHtml();
            }

            // HTML version of the description for outlook.
            iCalEvent.AddProperty( "X-ALT-DESC;FMTTYPE=text/html", $"<html>{description.RemoveCrLf()}</html>" );

            // Classification: "PUBLIC", "PRIVATE", "CONFIDENTIAL".
            iCalEvent.Class = "PUBLIC";

            if ( !string.IsNullOrEmpty( eventItem.DetailsUrl ) )
            {
                if ( Uri.TryCreate( eventItem.DetailsUrl, UriKind.Absolute, out Uri result ) )
                {
                    iCalEvent.Url = result;
                }
                else if ( Uri.TryCreate( $"http://{eventItem.DetailsUrl}", UriKind.Absolute, out result ) )
                {
                    iCalEvent.Url = result;
                }
            }

            // Add contact info if it exists.
            if ( eventItemOccurrence.ContactPersonAlias != null )
            {
                iCalEvent.Organizer = new Organizer( $"MAILTO:{eventItemOccurrence.ContactPersonAlias.Person.Email}" )
                {
                    CommonName = eventItemOccurrence.ContactPersonAlias.Person.FullName
                };

                // Outlook doesn't seem to use Contacts or Comments.
                var contactName = !string.IsNullOrEmpty( eventItemOccurrence.ContactPersonAlias.Person.FullName ) ? $"Name: {eventItemOccurrence.ContactPersonAlias.Person.FullName}" : string.Empty;
                var contactEmail = !string.IsNullOrEmpty( eventItemOccurrence.ContactEmail ) ? $", Email: {eventItemOccurrence.ContactEmail}" : string.Empty;
                var contactPhone = !string.IsNullOrEmpty( eventItemOccurrence.ContactPhone ) ? $", Phone: {eventItemOccurrence.ContactPhone}" : string.Empty;
                var contactInfo = $"{contactName}{contactEmail}{contactPhone}";

                iCalEvent.Contacts.Add( contactInfo );
                iCalEvent.Comments.Add( contactInfo );
            }

            // Add audiences as categories.
            foreach ( var a in eventItem.EventItemAudiences )
            {
                iCalEvent.Categories.Add( a.DefinedValue.Value );
            }
        }

        /// <summary>
        /// Creates the event description from the lava template.
        /// </summary>
        /// <param name="lavaTemplate">The lava template to be used when creating the event description.</param>
        /// <param name="eventItem">The event item.</param>
        /// <param name="occurrence">The event item occurrence.</param>
        /// <returns>The event description.</returns>
        private string CreateEventDescription( string lavaTemplate, EventItem eventItem, EventItemOccurrence occurrence )
        {
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
            mergeFields.Add( "EventItem", eventItem );
            mergeFields.Add( "EventItemOccurrence", occurrence );

            return lavaTemplate.ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Adjust the date and time information for this event to ensure that the serialized iCalendar data can be
        /// processed by calendaring applications such as Microsoft Outlook Web, Google Calendar and Apple Calendar.
        /// These applications require specific date/time formats and value combinations for a valid import format.
        /// </summary>
        /// <param name="iCalEvent">The iCal.NET calendar event.</param>
        /// <param name="timeZoneId">The IANA time zone identifier.</param>
        private void SetCalendarEventDateTimeInfo( CalendarEvent iCalEvent, string timeZoneId = null )
        {
            // Determine the start and end time for the event.
            // For an all-day event, omit the End date.
            // See: https://stackoverflow.com/questions/1716237/single-day-all-day-appointments-in-ics-files
            var start = iCalEvent.Start;

            timeZoneId = timeZoneId ?? iCalEvent.Start.TzId;

            iCalEvent.Start = ConvertToCalDateTime( start, timeZoneId );

            // Determine if this is an all-day event. The Rock ScheduleBuilder component adopts a convention of
            // assigning a 1 second duration to an event if the duration was not specified as part of the input.
            // Therefore, if the event starts at midnight and has a duration of <= 1s, assume it is an all day event.
            var startTime = new TimeSpan( start.Hour, start.Minute, start.Second );
            if ( startTime.TotalSeconds == 0 && ( iCalEvent.Duration == null || iCalEvent.Duration.TotalSeconds <= 1 ) )
            {
                iCalEvent.IsAllDay = true;
            }

            if ( iCalEvent.IsAllDay )
            {
                iCalEvent.End = null;
            }
            else
            {
                iCalEvent.End = ConvertToCalDateTime( iCalEvent.Start.Add( iCalEvent.Duration ), timeZoneId );
            }
        }

        /// <summary>
        /// Converts the provided <see cref="IDateTime"/> instance to an iCal.NET date/time object.
        /// </summary>
        /// <param name="sourceDateTime">The source <see cref="IDateTime"/> to convert.</param>
        /// <param name="timeZoneId">The IANA time zone identifier.</param>
        /// <returns>An iCal.NET date/time equivalent of the provided <see cref="IDateTime"/>.</returns>
        private CalDateTime ConvertToCalDateTime( IDateTime sourceDateTime, string timeZoneId )
        {
            if ( sourceDateTime == null )
            {
                return null;
            }

            if ( sourceDateTime is CalDateTime cdt )
            {
                if ( timeZoneId != null )
                {
                    cdt.TzId = timeZoneId;
                }

                return cdt;
            }

            var dateTime = new DateTime(
                sourceDateTime.Year,
                sourceDateTime.Month,
                sourceDateTime.Day,
                sourceDateTime.Hour,
                sourceDateTime.Minute,
                sourceDateTime.Second,
                sourceDateTime.Millisecond,
                DateTimeKind.Local
            );

            var calDateTime = ConvertToCalDateTime( dateTime, timeZoneId );

            return calDateTime;
        }

        /// <summary>
        /// Converts the provided <see cref="DateTime"/> to an iCal.NET date/time object.
        /// </summary>
        /// <param name="sourceDateTime">The source <see cref="DateTime"/> to convert.</param>
        /// <param name="timeZoneId">The IANA time zone identifier.</param>
        /// <returns>An iCal.NET date/time equivalent of the provided <see cref="DateTime"/>.</returns>
        private CalDateTime ConvertToCalDateTime( DateTime sourceDateTime, string timeZoneId )
        {
            var calDateTime = new CalDateTime( sourceDateTime );
            if ( timeZoneId != null )
            {
                calDateTime.TzId = timeZoneId;
            }

            // Set the HasTime property to ensure that iCal.Net serializes the date value in the form "TZID={timeZoneId}:YYYYMMDDTHHMMSS".
            // Microsoft Outlook Web ignores date values that are expressed using the iCalendar "PERIOD" or "DATE" type.
            // (see: MS-STANOICAL - v20210817 - 2.2.86)
            calDateTime.HasTime = true;

            return calDateTime;
        }
    }

    /// <summary>
    /// Arguments for the GetCalendarEventFeed request.
    /// </summary>
    public class GetCalendarEventFeedArgs
    {
        private DateTime? _startDate;
        private DateTime? _endDate;

        /// <summary>
        /// Gets or sets the calendar id. (Required)
        /// </summary>
        /// <value>
        /// The calendar identifier.
        /// </value>
        public int CalendarId { get; set; }

        /// <summary>
        /// Gets or sets the Campus Id list. If not set, this filter is ignored.
        /// </summary>
        /// <value>
        /// A collection of Campus identifiers.
        /// </value>
        public List<int> CampusIds { get; set; }

        /// <summary>
        /// Gets or sets the Audience Id list. If not set, this filter is ignored.
        /// </summary>
        /// <value>
        /// A collection of Audience identifiers.
        /// </value>
        public List<int> AudienceIds { get; set; }

        /// <summary>
        /// Gets or sets the start date. if not explicitly set returns 3 months prior to the current date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        public DateTime StartDate
        {
            get
            {
                return _startDate ?? RockDateTime.Now.AddMonths( -3 ).Date;
            }

            set
            {
                _startDate = value;
            }
        }

        /// <summary>
        /// Gets or sets the end date. If not explicitly set returns 12 months from current date.
        /// </summary>
        /// <value>
        /// The end date.
        /// </value>
        public DateTime EndDate
        {
            get
            {
                return _endDate ?? RockDateTime.Now.AddMonths( 12 ).Date;
            }

            set
            {
                _endDate = value;
            }
        }

        /// <summary>
        /// The name of the target device type for the calendar feed.
        /// If set to "Outlook", the feed will be formatted so that it can be processed correctly by Microsoft Outlook.
        /// </summary>
        public string ClientDeviceType { get; set; } = "";

        /// <summary>
        /// The Lava template to be used for the Event Item summaries.
        /// </summary>
        public string EventCalendarLavaTemplate { get; set; } = "";

        /// <summary>
        /// Gets or sets the Event Item identifiers to be included. If not set, this filter is ignored.
        /// </summary>
        /// <value>
        /// A collection of Event Item identifiers.
        /// </value>
        public List<int> EventItemIds { get; set; }
    }

    #endregion
}

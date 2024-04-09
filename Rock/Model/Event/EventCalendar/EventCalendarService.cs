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
using Ical.Net;
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
        /// <returns></returns>
        public string CreateICalendar( GetCalendarEventFeedArgs args )
        {
            /*
             * [2024-01-16] DJL
             * Extreme caution should be exercised when modifying the sequence or format of the elements in this section.
             * Any changes may break the compatibility of the iCal file with third-party applications, and should be tested
             * against Google Calendar, Outlook Web, Outlook Desktop 365 and Apple Calendar.
             */

            // Get a list of Rock Calendar Events that match the specified filter.
            var eventItems = GetEventItems( args );

            // Create the iCalendar.
            var iCalendar = new Calendar();

            // Specify the calendar timezone using the Internet Assigned Numbers Authority (IANA) identifier, because most third-party applications
            // require this to interpret event times correctly.
            var timeZoneId = TZConvert.WindowsToIana( RockDateTime.OrgTimeZoneInfo.Id );

            // If the client is Outlook, do not set the basic Event Description property.
            var setEventDescription = ( args.ClientDeviceType != "Outlook" );
            var lavaTemplate = args.EventCalendarLavaTemplate;

            var startDate = new CalDateTime( args.StartDate, timeZoneId );
            var endDate = new CalDateTime( args.EndDate, timeZoneId );

            // Create each of the events for the calendar(s)
            var earliestEventDateTime = RockDateTime.Now;

            foreach ( EventItem eventItem in eventItems )
            {
                // Calculate a sequence number for the Event Item.
                var eventItemSequenceNo = GetSequenceNumber( eventItem.CreatedDateTime, eventItem.ModifiedDateTime );

                foreach ( EventItemOccurrence occurrence in eventItem.EventItemOccurrences )
                {
                    if ( occurrence.Schedule == null )
                    {
                        continue;
                    }

                    // Calculate a Sequence Number for the calendar event.
                    // The iCalendar.SEQUENCE represents the revision number of a specific event occurrence.
                    // Many calendaring applications will not update an existing event with the same iCalendar.Uid
                    // unless the sequence number is greater.
                    // We assign a Sequence Number based on the number of seconds difference between the dates on which the Rock
                    // event components were first created and last modified. The sequence number is an Int32, which allows a valid
                    // range of sequences for 60+ years from the date the event components were created.
                    // There are multiple Rock components that affect the final calendar entries, and the sequence number assigned
                    // to the iCalendar event should be the highest of the sequence numbers calculated for each of these components.
                    // For more information, refer https://icalendar.org/iCalendar-RFC-5545/3-8-7-4-sequence-number.html
                    //
                    var sequenceNo = eventItemSequenceNo;

                    var occurrenceSequenceNo = GetSequenceNumber( occurrence.CreatedDateTime, occurrence.ModifiedDateTime );
                    if ( sequenceNo < occurrenceSequenceNo )
                    {
                        sequenceNo = occurrenceSequenceNo;
                    }

                    var scheduleSequenceNo = GetSequenceNumber( occurrence.Schedule.CreatedDateTime, occurrence.Schedule.ModifiedDateTime );
                    if ( sequenceNo < scheduleSequenceNo )
                    {
                        sequenceNo = scheduleSequenceNo;
                    }

                    var firstDateTime = occurrence.GetFirstStartDateTime();
                    if ( earliestEventDateTime < firstDateTime )
                    {
                        earliestEventDateTime = firstDateTime.Value;
                    }

                    var iCalOccurrence = CalendarCollection.Load( occurrence.Schedule.iCalendarContent.ToStreamReader() );
                    foreach ( var iCalEvent in iCalOccurrence[0].Events )
                    {
                        var hasSpecificDates = iCalEvent.RecurrenceRules.Count == 0
                            && iCalEvent.RecurrenceDates.Count > 0;

                        // If the event is not within the requested date range, discard it.
                        // This may occur if the template event has date values that are not aligned with the recurrence schedule.
                        if ( iCalEvent.Start.LessThan( startDate ) || iCalEvent.Start.GreaterThan( endDate ) )
                        {
                            continue;
                        }

                        iCalEvent.Sequence = sequenceNo;

                        if ( hasSpecificDates )
                        {
                            var iCalSpecificDateEvents = GetCalendarEventsForSpecificDates_RecurrenceId( iCalEvent, timeZoneId, occurrence, lavaTemplate, setEventDescription );

                            foreach ( var iCalSpecificDateEvent in iCalSpecificDateEvents )
                            {
                                iCalendar.Events.Add( iCalSpecificDateEvent );
                            }
                        }
                        else
                        {
                            var iCalEventNew = CopyCalendarEvent( iCalEvent );
                            SetCalendarEventDetailsFromRockEvent( iCalEventNew, timeZoneId, occurrence, lavaTemplate, setEventDescription );

                            iCalendar.Events.Add( iCalEventNew );
                        }
                    }
                }
            }

            // Find a non-DST date to use as the earliest supported timezone date, also ensuring that it is not a leap-day.
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
            var serializer = new CalendarSerializer();
            var calendarString = serializer.SerializeToString( iCalendar );

            return calendarString;
        }

        private List<CalendarEvent> GetCalendarEventsForSpecificDates_RecurrenceId( CalendarEvent iCalEvent, string timeZoneId, EventItemOccurrence occurrence, string lavaTemplate, bool setEventDescription )
        {
            var events = new List<CalendarEvent>();

            var eventStartTime = new TimeSpan( iCalEvent.DtStart.Hour, iCalEvent.DtStart.Minute, iCalEvent.DtStart.Second );
            var hasDuration = iCalEvent.Duration.TotalMilliseconds > 0;

            var specificDatePeriodList = iCalEvent.RecurrenceDates.FirstOrDefault();
            if ( specificDatePeriodList.Count == 0 )
            {
                return events;
            }

            var firstDateTime = ConvertToCalDateTime( specificDatePeriodList?.First(), eventStartTime );
            var recurrenceDates = ConvertPeriodListElementsToDateType( specificDatePeriodList, timeZoneId, eventStartTime, iCalEvent.Duration );
            var totalDateCount = recurrenceDates.Count;

            int dateNo = 0;
            int sequenceNo = iCalEvent.Sequence;

            CalDateTime recurrenceId;

            // Add the first calendar event to create a recurring daily schedule for the number of specific dates.
            var iEvent = CopyCalendarEvent( iCalEvent );

            iEvent.DtStart = ConvertToCalDateTime( firstDateTime, timeZoneId );

            SetCalendarEventDetailsFromRockEvent( iEvent, timeZoneId, occurrence, lavaTemplate, setEventDescription );

            iEvent.RecurrenceDates = null;
            iEvent.RecurrenceRules.Add( new RecurrencePattern( $"FREQ=DAILY;COUNT={totalDateCount}" ) );
            iEvent.Sequence = sequenceNo;

            events.Add( iEvent );

            // Add subsequent calendar events to reschedule the daily recurrences to the specific dates.
            // Order these variations to the recurrence schedule from latest to earliest.
            // If they are not specified in this order, Outlook Web and Google Calendar fail to maintain the entries
            // as a single recurring series.
            recurrenceDates = SortPeriodListByStartDateDescending( recurrenceDates );

            // Each of the rescheduled events must have a higher sequence number than the first calendar event.
            sequenceNo++;

            foreach ( var recurrenceDate in recurrenceDates )
            {
                iEvent = CopyCalendarEvent( iCalEvent );

                SetCalendarEventDetailsFromRockEvent( iEvent, timeZoneId, occurrence, lavaTemplate, setEventDescription );

                iEvent.DtStart = ConvertToCalDateTime( recurrenceDate.StartTime, timeZoneId );

                // Reset the All Day event flag, because it is reset by iCal.Net when the DtStart property is assigned.
                iEvent.IsAllDay = iCalEvent.IsAllDay;

                // Set the EndDateTime, unless this is flagged as an All Day event.
                if ( !iEvent.IsAllDay && recurrenceDate.EndTime != null )
                {
                    iEvent.DtEnd = ConvertToCalDateTime( recurrenceDate.EndTime, timeZoneId );
                }
                iEvent.RecurrenceDates = null;

                iEvent.Sequence = sequenceNo;

                // The RecurrenceId should match the date in the daily recurrence pattern that is being rescheduled.
                // The time component must be omitted, or Google Calendar will fail to match the rescheduled event correctly.
                var recurrencePatternDate = firstDateTime.AddDays( totalDateCount - 1 ).AddDays( -1 * dateNo );
                recurrenceId = ConvertToCalDateTime( recurrencePatternDate, null );

                recurrenceId.HasTime = hasDuration;

                iEvent.RecurrenceId = recurrenceId;

                dateNo++;

                events.Add( iEvent );
            }

            return events;
        }

        private CalendarEvent SetCalendarEventDetailsFromRockEvent( CalendarEvent iCalEvent, string timeZoneId, EventItemOccurrence occurrence, string eventCalendarLavaTemplate, bool setEventDescription )
        {
            var eventItem = occurrence.EventItem;

            // We get all of the schedule info from Schedule.iCalendarContent
            iCalEvent.Summary = !string.IsNullOrEmpty( eventItem.Name ) ? eventItem.Name : string.Empty;
            iCalEvent.Location = !string.IsNullOrEmpty( occurrence.Location ) ? occurrence.Location : string.Empty;
            iCalEvent.Uid = occurrence.Guid.ToString();

            // Determine the start and end time for the event.
            // For an all-day event, omit the End date.
            // see https://stackoverflow.com/questions/1716237/single-day-all-day-appointments-in-ics-files
            iCalEvent.Start = new CalDateTime( iCalEvent.Start.Value, timeZoneId );

            if ( !iCalEvent.Start.HasTime
                && ( iCalEvent.End != null && !iCalEvent.End.HasTime )
                && iCalEvent.Duration == null || iCalEvent.Duration.Ticks == 0 )
            {
                iCalEvent.End = null;
            }
            else
            {
                iCalEvent.End = new CalDateTime( iCalEvent.End.Value, timeZoneId );
            }

            // Rock has more descriptions than iCal so lets concatenate them
            var description = CreateEventDescription( eventCalendarLavaTemplate, eventItem, occurrence );


            // Don't set the description prop for outlook to force it to use the X-ALT-DESC property which can have markup.
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

            // HTML version of the description for outlook
            iCalEvent.AddProperty( "X-ALT-DESC;FMTTYPE=text/html", "<html>" + description + "</html>" );

            // classification: "PUBLIC", "PRIVATE", "CONFIDENTIAL"
            iCalEvent.Class = "PUBLIC";

            if ( !string.IsNullOrEmpty( eventItem.DetailsUrl ) )
            {
                if ( Uri.TryCreate( eventItem.DetailsUrl, UriKind.Absolute, out Uri result ) )
                {
                    iCalEvent.Url = result;
                }
                else if ( Uri.TryCreate( "http://" + eventItem.DetailsUrl, UriKind.Absolute, out result ) )
                {
                    iCalEvent.Url = result;
                }
            }

            // add contact info if it exists
            if ( occurrence.ContactPersonAlias != null )
            {
                iCalEvent.Organizer = new Organizer( string.Format( "MAILTO:{0}", occurrence.ContactPersonAlias.Person.Email ) )
                {
                    CommonName = occurrence.ContactPersonAlias.Person.FullName
                };

                // Outlook doesn't seem to use Contacts or Comments
                var contactName = !string.IsNullOrEmpty( occurrence.ContactPersonAlias.Person.FullName ) ? "Name: " + occurrence.ContactPersonAlias.Person.FullName : string.Empty;
                var contactEmail = !string.IsNullOrEmpty( occurrence.ContactEmail ) ? ", Email: " + occurrence.ContactEmail : string.Empty;
                var contactPhone = !string.IsNullOrEmpty( occurrence.ContactPhone ) ? ", Phone: " + occurrence.ContactPhone : string.Empty;
                var contactInfo = contactName + contactEmail + contactPhone;

                iCalEvent.Contacts.Add( contactInfo );
                iCalEvent.Comments.Add( contactInfo );
            }

            // Add Audience as Categories.
            foreach ( var a in eventItem.EventItemAudiences )
            {
                iCalEvent.Categories.Add( a.DefinedValue.Value );
            }

            return iCalEvent;
        }

        /// <summary>
        /// Convert the elements of an iCal.PeriodList to ensure they use the CalDateTime type.
        /// </summary>
        /// <param name="periodList"></param>
        /// <param name="tzId"></param>
        /// <param name="eventStartTime"></param>
        /// <param name="eventDuration"></param>
        /// <returns></returns>
        private PeriodList ConvertPeriodListElementsToDateType( PeriodList periodList, string tzId, TimeSpan eventStartTime, TimeSpan? eventDuration )
        {
            if ( eventDuration?.TotalMilliseconds < 1 )
            {
                eventDuration = null;
            }

            // It's important to create and return a new PeriodList object here rather than simply removing elements of the existing collection,
            // because iCal.Net has some issues with synchronising changes to PeriodList elements that cause problems downstream.
            var newPeriodList = new PeriodList() { TzId = tzId };
            foreach ( var period in periodList )
            {
                var newDateTime = ConvertToCalDateTime( period, eventStartTime );

                Period newPeriod;
                if ( eventDuration != null )
                {
                    newPeriod = new Period( newDateTime, eventDuration.Value );
                }
                else
                {
                    newPeriod = new Period( newDateTime );
                }

                newPeriodList.Add( newPeriod );
            }

            return newPeriodList;
        }

        private PeriodList SortPeriodListByStartDateDescending( PeriodList periodList )
        {
            var sortedPeriods = periodList.OrderByDescending( p => p.StartTime ).ToList();

            var newPeriodList = new PeriodList() { TzId = periodList.TzId };
            foreach ( var period in sortedPeriods )
            {
                newPeriodList.Add( period );
            }

            return newPeriodList;
        }

        private CalDateTime ConvertToCalDateTime( Period period, TimeSpan time )
        {
            var newDateTime = period.StartTime.HasTime
                                ? period.StartTime.Value
                                : period.StartTime.Value.Add( time );
            newDateTime = new DateTime( newDateTime.Year, newDateTime.Month, newDateTime.Day, newDateTime.Hour, newDateTime.Minute, newDateTime.Second, newDateTime.Millisecond, DateTimeKind.Local );

            var newDate = ConvertToCalDateTime( newDateTime, period.StartTime.TzId );

            return newDate;
        }

        private CalDateTime ConvertToCalDateTime( IDateTime newDateTime, string tzId )
        {
            if ( newDateTime is CalDateTime cdt )
            {
                if ( tzId != null )
                {
                    cdt.TzId = tzId;
                }
                return cdt;
            }

            var dateTime = new DateTime( newDateTime.Year, newDateTime.Month, newDateTime.Day, newDateTime.Hour, newDateTime.Minute, newDateTime.Second, newDateTime.Millisecond, DateTimeKind.Local );

            var newDate = ConvertToCalDateTime( dateTime, tzId );

            return newDate;
        }

        private CalDateTime ConvertToCalDateTime( DateTime newDateTime, string tzId )
        {
            var newDate = new CalDateTime( newDateTime );
            if ( tzId != null )
            {
                newDate.TzId = tzId;
            }

            // Set the HasTime property to ensure that iCal.Net serializes the date value as an iCalendar "DATE" rather than a "PERIOD".
            // Microsoft Outlook ignores date values that are expressed using the iCalendar "PERIOD" type.
            // (see: MS-STANOICAL - v20210817 - 2.2.86)
            newDate.HasTime = true;

            return newDate;
        }

        /// <summary>
        /// Creates the event description from the lava template. Default is used if one is not specified in the request.
        /// </summary>
        /// <param name="eventCalendarLavaTemplate"></param>
        /// <param name="eventItem">The event item.</param>
        /// <param name="occurrence">The occurrence.</param>
        /// <returns></returns>
        private string CreateEventDescription( string eventCalendarLavaTemplate, EventItem eventItem, EventItemOccurrence occurrence )
        {
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
            mergeFields.Add( "EventItem", eventItem );
            mergeFields.Add( "EventItemOccurrence", occurrence );

            return eventCalendarLavaTemplate.ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Uses the filter information in the CalendarProps object to get a list of events
        /// </summary>
        /// <param name="calendarProps">The calendar props.</param>
        /// <returns></returns>
        private List<EventItem> GetEventItems( GetCalendarEventFeedArgs calendarProps )
        {
            var rockContext = new RockContext();

            var eventCalendarService = new EventCalendarService( rockContext );
            var eventCalendar = eventCalendarService.Get( calendarProps.CalendarId );
            if ( eventCalendar == null )
            {
                throw new Exception( $"Invalid Calendar reference. [CalendarId={calendarProps.CalendarId}]" );
            }

            if ( calendarProps.StartDate > calendarProps.EndDate )
            {
                throw new Exception( $"Invalid Date Range. Start Date must be prior to End Date [StartDate={calendarProps.StartDate}, EndDate={calendarProps.EndDate}]" );
            }

            var eventCalendarItemService = new EventCalendarItemService( rockContext );
            var eventItemQuery = eventCalendarItemService
                .Queryable()
                .Where( i => i.EventCalendarId == calendarProps.CalendarId );

            // Filter by Event Item.
            if ( calendarProps.EventItemIds != null && calendarProps.EventItemIds.Any() )
            {
                eventItemQuery = eventItemQuery.Where( eci => calendarProps.EventItemIds.Contains( eci.EventItemId ) );
            }

            var eventIdsForCalendar = eventItemQuery
                .Select( i => i.EventItemId )
                .ToList();

            var eventItemService = new EventItemService( rockContext );
            var eventQueryable = eventItemService
                .Queryable( "EventItemAudiences, EventItemOccurrences.Schedule" )
                .Where( e => eventIdsForCalendar.Contains( e.Id ) )
                .Where( e => e.EventItemOccurrences.Any( o => ( o.Schedule.EffectiveStartDate == null || o.Schedule.EffectiveStartDate <= calendarProps.EndDate ) && ( o.Schedule.EffectiveEndDate == null || calendarProps.StartDate <= o.Schedule.EffectiveEndDate ) ) )
                .Where( e => e.IsActive == true )
                .Where( e => e.IsApproved );

            // For Campus
            if ( calendarProps.CampusIds != null
                 && calendarProps.CampusIds.Any() )
            {
                eventQueryable = eventQueryable.Where( e => e.EventItemOccurrences.Any( c => !c.CampusId.HasValue || calendarProps.CampusIds.Contains( c.CampusId.Value ) ) );
            }

            // For Audience
            if ( calendarProps.AudienceIds != null
                 && calendarProps.AudienceIds.Any() )
            {
                eventQueryable = eventQueryable.Where( e => e.EventItemAudiences.Any( c => calendarProps.AudienceIds.Contains( c.DefinedValueId ) ) );
            }

            return eventQueryable.ToList();
        }

        /// <summary>
        /// Create a sequence number that indicates the age of an item based on when it was created and last modified.
        /// </summary>
        /// <param name="createdDateTime"></param>
        /// <param name="modifiedDateTime"></param>
        /// <returns></returns>
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
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
            // Get a list of Rock Calendar Events that match the specified filter.
            var eventItems = GetEventItems( args );

            // Create the iCalendar.
            var icalendar = new Calendar();

            // Specify the calendar timezone using the Internet Assigned Numbers Authority (IANA) identifier, because most third-party applications
            // require this to interpret event times correctly.
            var timeZoneId = TZConvert.WindowsToIana( RockDateTime.OrgTimeZoneInfo.Id );
            icalendar.AddTimeZone( VTimeZone.FromDateTimeZone( timeZoneId ) );

            var startDate = new CalDateTime( args.StartDate, timeZoneId );
            var endDate = new CalDateTime( args.EndDate, timeZoneId );

            // Add the ICalendar events.
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

                    //
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

                    // Read the scheduling information from the Event Occurrence Schedule iCalendar data.
                    var ical = CalendarCollection.Load( occurrence.Schedule.iCalendarContent.ToStreamReader() );

                    foreach ( var icalEvent in ical[0].Events )
                    {
                        // If the event is not within the requested date range, discard it.
                        // This may occur if the template event has date values that are not aligned with the recurrence schedule.
                        if ( icalEvent.Start.LessThan( startDate ) || icalEvent.Start.GreaterThan( endDate ) )
                        {
                            continue;
                        }

                        var ievent = icalEvent.Copy<CalendarEvent>();
                        ievent.Uid = occurrence.Guid.ToString();
                        ievent.Sequence = sequenceNo;

                        ievent.Summary = !string.IsNullOrEmpty( eventItem.Name ) ? eventItem.Name : string.Empty;
                        ievent.Location = !string.IsNullOrEmpty( occurrence.Location ) ? occurrence.Location : string.Empty;

                        // Determine the start and end time for the event.
                        // For an all-day event, omit the End date.
                        // see https://stackoverflow.com/questions/1716237/single-day-all-day-appointments-in-ics-files
                        ievent.Start = new CalDateTime( icalEvent.Start.Value, timeZoneId );

                        if ( ievent.Start.LessThan( startDate ) || ievent.Start.GreaterThan( endDate ) )
                        {
                            continue;
                        }

                        if ( !ievent.Start.HasTime
                            && ( ievent.End != null && !ievent.End.HasTime )
                            && ievent.Duration == null || ievent.Duration.Ticks == 0 )
                        {
                            ievent.End = null;
                        }
                        else
                        {
                            ievent.End = new CalDateTime( icalEvent.End.Value, timeZoneId );
                        }

                        /*
                            2022-10-19 - DL

                            This code contains a number of workarounds for exporting recurring events in a format that can be processed by
                            external calendar applications such as Microsoft Outlook, namely:
                            1. The iCalendar PERIOD type is not recognized by some applications.
                               We need to ensure that recurrence settings are always specified using the DATE type.
                            2. Exception dates must have exactly the same start time and time zone as the template event, and the time zone
                               must be expressed as an IANA name.
                            3. Duplicate events may be imported if the template event date is also included in the list of recurrence dates.
                               We need to remove the template event date (DTSTART) from the list of recurrences (RDATE).
                            4. If a set of ad-hoc recurrence dates exist, events for these dates may not be created unless
                               a recurrence rule also exists.

                             Reason: To allow recurring events to be imported correctly to third-party calendar applications.
                        */

                        // Create the list of exceptions.
                        // Exceptions must meet RFC 5545 iCalendar specifications to be correctly processed by third-party calendar applications
                        // such as Microsoft Outlook and Google Calendar. Specifically, an exception must have exactly the same start time
                        // and time zone as the template event, and the time zone must be expressed as an IANA name.
                        // The most recent version of iCal.Net (v2.3.5) that supports .NET framework v4.5.2 has some inconsistencies in the
                        // iCalendar serialization process, so we need to force the Start, End and Exception dates to render in exactly the same format.

                        var eventStartTime = new TimeSpan( ievent.DtStart.Hour, ievent.DtStart.Minute, ievent.DtStart.Second );

                        ievent.ExceptionDates = ConvertPeriodListElementsToDateType( ievent.ExceptionDates, timeZoneId, eventStartTime );

                        // Microsoft Outlook does not import a recurrence date of type PERIOD, only DATE or DATETIME.
                        // If the Recurrence Dates do not specify a Start Time, set the start time to the same as the event.
                        // If this is an all day event, set the Start Time to 12:00am.
                        ievent.RecurrenceDates = ConvertPeriodListElementsToDateType( ievent.RecurrenceDates, timeZoneId, eventStartTime );

                        // If the recurrence dates include the calendar event start date, remove it.
                        // If we don't, Microsoft Outlook will create a duplicate entry for that date.
                        ievent.RecurrenceDates = RemoveDateFromPeriodList( ievent.RecurrenceDates, ievent.DtStart );

                        // If one-time recurrence dates exist, create a placeholder recurrence rule to ensure that the iCalendar file
                        // can be correctly imported by Outlook.
                        // Fixes Issue #4112. Refer https://github.com/SparkDevNetwork/Rock/issues/4112
                        if ( ievent.RecurrenceRules.Count == 0
                            && ievent.RecurrenceDates.Count > 0 )
                        {
                            ievent.RecurrenceRules.Add( new RecurrencePattern( "FREQ=DAILY;COUNT=1" ) );
                        }

                        // Rock has more descriptions than iCal so lets concatenate them
                        var description = CreateEventDescription( args, eventItem, occurrence );

                        // Don't set the description prop for outlook to force it to use the X-ALT-DESC property which can have markup.
                        if ( args.ClientDeviceType != "Outlook" )
                        {
                            ievent.Description = description.ConvertBrToCrLf()
                                                                .Replace( "</P>", "" )
                                                                .Replace( "</p>", "" )
                                                                .Replace( "<P>", Environment.NewLine )
                                                                .Replace( "<p>", Environment.NewLine )
                                                                .Replace( "&nbsp;", " " )
                                                                .SanitizeHtml();
                        }

                        // HTML version of the description for outlook
                        ievent.AddProperty( "X-ALT-DESC;FMTTYPE=text/html", "<html>" + description + "</html>" );

                        // classification: "PUBLIC", "PRIVATE", "CONFIDENTIAL"
                        ievent.Class = "PUBLIC";

                        if ( !string.IsNullOrEmpty( eventItem.DetailsUrl ) )
                        {
                            if ( Uri.TryCreate( eventItem.DetailsUrl, UriKind.Absolute, out Uri result ) )
                            {
                                ievent.Url = result;
                            }
                            else if ( Uri.TryCreate( "http://" + eventItem.DetailsUrl, UriKind.Absolute, out result ) )
                            {
                                ievent.Url = result;
                            }
                        }

                        // add contact info if it exists
                        if ( occurrence.ContactPersonAlias != null )
                        {
                            ievent.Organizer = new Organizer( string.Format( "MAILTO:{0}", occurrence.ContactPersonAlias.Person.Email ) )
                            {
                                CommonName = occurrence.ContactPersonAlias.Person.FullName
                            };

                            // Outlook doesn't seem to use Contacts or Comments
                            var contactName = !string.IsNullOrEmpty( occurrence.ContactPersonAlias.Person.FullName ) ? "Name: " + occurrence.ContactPersonAlias.Person.FullName : string.Empty;
                            var contactEmail = !string.IsNullOrEmpty( occurrence.ContactEmail ) ? ", Email: " + occurrence.ContactEmail : string.Empty;
                            var contactPhone = !string.IsNullOrEmpty( occurrence.ContactPhone ) ? ", Phone: " + occurrence.ContactPhone : string.Empty;
                            var contactInfo = contactName + contactEmail + contactPhone;

                            ievent.Contacts.Add( contactInfo );
                            ievent.Comments.Add( contactInfo );
                        }

                        // Add Audience as Categories.
                        foreach ( var a in eventItem.EventItemAudiences )
                        {
                            ievent.Categories.Add( a.DefinedValue.Value );
                        }

                        icalendar.Events.Add( ievent );
                    }
                }
            }

            // Return a serialized iCalendar.
            var serializer = new CalendarSerializer();
            var calendarString = serializer.SerializeToString( icalendar );

            return calendarString;
        }

        /// <summary>
        /// Convert the elements of a PeriodList from the iCalendar PERIOD type to the DATE type.
        /// </summary>
        /// <param name="periodLists"></param>
        /// <param name="tzId"></param>
        /// <param name="eventStartTime"></param>
        /// <returns></returns>
        private IList<PeriodList> ConvertPeriodListElementsToDateType( IList<PeriodList> periodLists, string tzId, TimeSpan eventStartTime )
        {
            // It's important to create and return a new PeriodList object here rather than simply removing elements of the existing collection,
            // because iCal.Net has some issues with synchronising changes to PeriodList elements that cause problems downstream.
            var newDatesList = new List<PeriodList>();

            foreach ( var periodList in periodLists )
            {
                var newPeriodList = new PeriodList() { TzId = tzId };
                foreach ( var period in periodList )
                {
                    var newDateTime = period.StartTime.HasTime
                        ? period.StartTime.Value
                        : period.StartTime.Value.Add( eventStartTime );
                    newDateTime = new DateTime( newDateTime.Year, newDateTime.Month, newDateTime.Day, newDateTime.Hour, newDateTime.Minute, newDateTime.Second, newDateTime.Millisecond, DateTimeKind.Local );

                    var newDate = new CalDateTime( newDateTime );

                    // Set the HasTime property to ensure that iCal.Net serializes the date value as an iCalendar "DATE" rather than a "PERIOD".
                    // Microsoft Outlook ignores date values that are expressed using the iCalendar "PERIOD" type.
                    // (see: MS-STANOICAL - v20210817 - 2.2.86)
                    newDate.HasTime = true;
                    var newPeriod = new Period( newDate );

                    newPeriodList.Add( newPeriod );
                }

                newDatesList.Add( newPeriodList );
            }

            return newDatesList;
        }

        /// <summary>
        /// Removes instances of the specified date from a collection of PeriodList objects.
        /// </summary>
        /// <param name="periodLists"></param>
        /// <param name="removeDate"></param>
        /// <returns></returns>
        private IList<PeriodList> RemoveDateFromPeriodList( IList<PeriodList> periodLists, IDateTime removeDate )
        {
            // It's important to create and return a new PeriodList object here rather than simply removing elements of the existing collection,
            // because iCal.Net has some issues with synchronising changes to PeriodList elements that cause problems downstream.
            var newPeriodLists = new List<PeriodList>();

            foreach ( var periodList in periodLists )
            {
                var newPeriodList = new PeriodList() { TzId = periodList.TzId };
                foreach ( var period in periodList )
                {
                    if ( period.StartTime.Ticks == removeDate.Ticks )
                    {
                        continue;
                    }
                    newPeriodList.Add( period );
                }

                newPeriodLists.Add( newPeriodList );
            }

            return newPeriodLists;
        }

        /// <summary>
        /// Creates the event description from the lava template. Default is used if one is not specified in the request.
        /// </summary>
        /// <param name="calendarProps"></param>
        /// <param name="eventItem">The event item.</param>
        /// <param name="occurrence">The occurrence.</param>
        /// <returns></returns>
        private string CreateEventDescription( GetCalendarEventFeedArgs calendarProps, EventItem eventItem, EventItemOccurrence occurrence )
        {
            var template = calendarProps.EventCalendarLavaTemplate;

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
            mergeFields.Add( "EventItem", eventItem );
            mergeFields.Add( "EventItemOccurrence", occurrence );

            return template.ResolveMergeFields( mergeFields );
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
                throw new Exception( $"Invalid Calendar reference. [CalendarId={ calendarProps.CalendarId }]" );
            }

            if ( calendarProps.StartDate > calendarProps.EndDate )
            {
                throw new Exception( $"Invalid Date Range. Start Date must be prior to End Date [StartDate={ calendarProps.StartDate }, EndDate={ calendarProps.EndDate }]" );
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
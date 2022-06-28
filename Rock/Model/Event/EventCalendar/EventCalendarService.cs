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
            // Get a list of Rock Calendar Events filtered by calendarProps
            var eventItems = GetEventItems( args );

            // Create the iCalendar.
            // Allow ICal to create the VTimeZone object from the local time zone to ensure that we get the correct name and daylight saving offset.
            var vtz = new VTimeZone( RockDateTime.OrgTimeZoneInfo.Id );
            var icalendar = new Calendar();
            icalendar.AddTimeZone( vtz );

            var timeZoneId = vtz.TzId;

            // Create each of the events for the calendar(s)
            foreach ( EventItem eventItem in eventItems )
            {
                foreach ( EventItemOccurrence occurrence in eventItem.EventItemOccurrences )
                {
                    if ( occurrence.Schedule == null )
                    {
                        continue;
                    }

                    var ical = CalendarCollection.Load( occurrence.Schedule.iCalendarContent.ToStreamReader() );
                    foreach ( var icalEvent in ical[0].Events )
                    {
                        // We get all of the schedule info from Schedule.iCalendarContent
                        var ievent = icalEvent.Copy<CalendarEvent>();
                        ievent.Summary = !string.IsNullOrEmpty( eventItem.Name ) ? eventItem.Name : string.Empty;
                        ievent.Location = !string.IsNullOrEmpty( occurrence.Location ) ? occurrence.Location : string.Empty;

                        // Create the list of exceptions.
                        // Exceptions must meet RFC 5545 iCalendar specifications to be correctly processed by third-party calendar applications
                        // such as Microsoft Outlook and Google Calendar. Specifically, an exception must have exactly the same start time
                        // and time zone as the template event, and the time zone must be expressed as an IANA name.
                        // The most recent version of iCal.Net (v2.3.5) that supports .NET framework v4.5.2 has some inconsistencies in the
                        // iCalendar serialization process, so we need to force the Start, End and Exception dates to render in exactly the same format.
                        ievent.Start = new CalDateTime( icalEvent.Start.Value, timeZoneId );
                        ievent.End = new CalDateTime( icalEvent.End.Value, timeZoneId );

                        var eventStartTime = new TimeSpan( ievent.DtStart.Hour, ievent.DtStart.Minute, ievent.DtStart.Second );
                        var newExceptionDatesList = new List<PeriodList>();

                        foreach ( var exceptionDateList in ievent.ExceptionDates )
                        {
                            var newDateList = new PeriodList() { TzId = timeZoneId };
                            foreach ( var exceptionDate in exceptionDateList )
                            {
                                var newDateTime = exceptionDate.StartTime.HasTime ? exceptionDate.StartTime.Value : exceptionDate.StartTime.Value.Add( eventStartTime );
                                newDateTime = new DateTime( newDateTime.Year, newDateTime.Month, newDateTime.Day, newDateTime.Hour, newDateTime.Minute, newDateTime.Second, newDateTime.Millisecond, DateTimeKind.Local );

                                var newDate = new CalDateTime( newDateTime );
                                newDateList.Add( newDate );
                            }

                            newExceptionDatesList.Add( newDateList );
                        }

                        ievent.ExceptionDates = newExceptionDatesList;

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

            var eventCalendarItemService = new EventCalendarItemService( rockContext );
            var eventIdsForCalendar = eventCalendarItemService
                .Queryable()
                .Where( i => i.EventCalendarId == calendarProps.CalendarId )
                .Select( i => i.EventItemId )
                .ToList();

            var eventItemService = new EventItemService( rockContext );
            var eventQueryable = eventItemService
                .Queryable( "EventItemAudiences, EventItemOccurrences.Schedule" )
                .Where( e => eventIdsForCalendar.Contains( e.Id ) )
                .Where( e => e.EventItemOccurrences.Any( o => o.Schedule.EffectiveStartDate <= calendarProps.EndDate && calendarProps.StartDate <= ( o.Schedule.EffectiveEndDate == null ? o.Schedule.EffectiveStartDate : o.Schedule.EffectiveEndDate ) ) )
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
    }

    #endregion
}
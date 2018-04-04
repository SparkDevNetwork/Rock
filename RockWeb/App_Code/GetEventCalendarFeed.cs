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
using System.Linq;
using System.Web;
using System.Net;

using Rock;
using Rock.Data;
using Rock.Model;

using DDay.iCal;
using DDay.iCal.Serialization.iCalendar;

using System.Globalization;

namespace RockWeb
{
    /// <summary>
    /// Summary description for GetEventCalendarFeed
    /// </summary>
    public class GetEventCalendarFeed : IHttpHandler
    {
        private HttpRequest request;
        private HttpResponse response;

        /// <summary>
        /// Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler" /> instance.
        /// </summary>
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Processes the request.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        public void ProcessRequest( HttpContext httpContext )
        {
            request = httpContext.Request;
            response = httpContext.Response;

            // TODO: if querystring value is missing then check the request body
            if ( !ValidateSecurity( httpContext ) )
            {
                return;
            }

            RockContext rockContext = new RockContext();
            CalendarProps calendarProps = ValidateRequestData( httpContext );

            if ( calendarProps == null )
            {
                return;
            }

            iCalendar icalendar = CreateICalendar( calendarProps );

            iCalendarSerializer serializer = new iCalendarSerializer();
            string s = serializer.SerializeToString( icalendar );

            response.Clear();
            response.ClearHeaders();
            response.ClearContent();
            response.AddHeader( "content-disposition", string.Format( "attachment; filename={0}_ical.ics", calendarProps.StartDate.ToString( "yyyy-MM-dd" ) ) );
            response.ContentType = "text/calendar";
            response.Write( s );
        }

        /// <summary>
        /// Creates the iCalendar object and populates it with events
        /// </summary>
        /// <param name="calendarProps">The calendar props.</param>
        /// <returns></returns>
        private iCalendar CreateICalendar( CalendarProps calendarProps )
        {
            // Get a list of Rock Calendar Events filtered by calendarProps
            List<EventItem> eventItems = GetEventItems( calendarProps );

            // Create the iCalendar
            iCalendar icalendar = new iCalendar();
            icalendar.AddLocalTimeZone();

            // Create each of the events for the calendar(s)
            foreach ( EventItem eventItem in eventItems )
            {
                foreach ( EventItemOccurrence occurrence in eventItem.EventItemOccurrences )
                {
                    iCalendarSerializer serializer = new iCalendarSerializer();
                    iCalendarCollection ical = ( iCalendarCollection ) serializer.Deserialize( occurrence.Schedule.iCalendarContent.ToStreamReader() );

                    foreach ( var icalEvent in ical[0].Events )
                    {
                        // We get all of the schedule info from Schedule.iCalendarContent
                        Event ievent = icalEvent.Copy<Event>();

                        // This is the subject in outlook
                        ievent.Summary = !string.IsNullOrEmpty( eventItem.Name ) ? eventItem.Name : string.Empty;

                        // body
                        ievent.Description = !string.IsNullOrEmpty( eventItem.Description ) ? eventItem.Description : string.Empty;

                        // Add occurrence note to the description
                        if ( !string.IsNullOrEmpty( occurrence.Note ) )
                        {
                            ievent.Description += Environment.NewLine;
                            ievent.Description += occurrence.Note;
                        }

                        // classification: "PUBLIC", "PRIVATE", "CONFIDENTIAL"
                        ievent.Class = "PUBLIC";

                        if ( !string.IsNullOrEmpty( eventItem.DetailsUrl ) )
                        {
                            ievent.Url = new Uri( eventItem.DetailsUrl );
                        }

                        ievent.Location = !string.IsNullOrEmpty( occurrence.Location ) ? occurrence.Location : string.Empty;

                        // add the photo if it exists. Might want to add content channels too.
                        if ( eventItem.PhotoId != null )
                        {
                            ievent.Attachments.Add( new Attachment( eventItem.Photo.ContentsToString() ) );// this is not working.
                        }

                        // organizer  - ORGANIZER;CN=John Smith:mailto:jsmith@example.com
                        // contact - textual contact info, my include name, phone, emai;

                        // optional and can occur more than once
                        // categories - comma delmited list of whatever, might use audience
                        // comment - Any text

                        icalendar.Events.Add( ievent );
                    }
                }
            }

            return icalendar;
        }

        /// <summary>
        /// Uses the filter information in the CalendarProps object to get a list of events
        /// </summary>
        /// <param name="calendarProps">The calendar props.</param>
        /// <returns></returns>
        private List<EventItem> GetEventItems( CalendarProps calendarProps )
        {
            RockContext rockContext = new RockContext();

            EventCalendarItemService eventCalendarItemService = new EventCalendarItemService( rockContext );
            var eventIdsForCalendar = eventCalendarItemService
                .Queryable()
                .Where( i => i.EventCalendarId == calendarProps.CalendarId )
                .Select( i => i.EventItemId )
                .ToList();

            EventItemService eventItemService = new EventItemService( rockContext );
            var eventQueryable = eventItemService
                .Queryable( "EventItemAudiences, EventItemOccurrences.Schedule" )
                .Where( e => eventIdsForCalendar.Contains( e.Id ) )
                .Where( e => e.EventItemOccurrences.Any( o => o.Schedule.EffectiveStartDate >= calendarProps.StartDate && o.Schedule.EffectiveEndDate <= calendarProps.EndDate ) )
                .Where( e => e.IsActive == true )
                .Where( e => e.IsApproved );

            // For Campus
            if ( calendarProps.CampusIds.Any() )
            {
                eventQueryable = eventQueryable.Where( e => e.EventItemOccurrences.Any( c => !c.CampusId.HasValue || calendarProps.CampusIds.Contains( c.CampusId.Value ) ) );
            }

            // For Audience
            if ( calendarProps.AudienceIds.Any() )
            {
                eventQueryable = eventQueryable.Where( e => e.EventItemAudiences.Any( c => calendarProps.AudienceIds.Contains( c.DefinedValueId ) ) );
            }

            return eventQueryable.ToList();
        }

        /// <summary>
        /// Sends the not authorized response
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        private void SendNotAuthorized( HttpContext httpContext )
        {
            httpContext.Response.StatusCode = HttpStatusCode.Forbidden.ConvertToInt();
            httpContext.Response.StatusDescription = "Not authorized to view calendar.";
            httpContext.ApplicationInstance.CompleteRequest();
        }

        /// <summary>
        /// Sends the bad request response
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <param name="addlInfo">The addl information.</param>
        private void SendBadRequest( HttpContext httpContext, string addlInfo = "" )
        {
            httpContext.Response.StatusCode = HttpStatusCode.BadRequest.ConvertToInt();
            httpContext.Response.StatusDescription = "Request is inavalid or malformed. " + addlInfo;
        }

        /// <summary>
        /// Ensure the current user is authorized to view the calendar. If all are allowed then current user is not evaluated.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private bool ValidateSecurity( HttpContext context )
        {
            int calendarId;
            if ( request.QueryString["calendarid"] == null || !int.TryParse( request.QueryString["calendarId"], out calendarId ) )
            {
                SendNotAuthorized( context );
                return false;
            }

            RockContext rockContext = new RockContext();
            EventCalendarService eventCalendarService = new EventCalendarService( rockContext );
            EventCalendar eventCalendar = eventCalendarService.Get( calendarId );

            if ( eventCalendar == null )
            {
                SendNotAuthorized( context );
                return false;
            }

            // If this is a public calendar then just return true
            if ( eventCalendar.IsAllowedByDefault( "View" ) )
            {
                return true;
            }

            UserLogin currentUser = new UserLoginService( rockContext ).GetByUserName( UserLogin.GetCurrentUserName() );
            Person currentPerson = currentUser != null ? currentUser.Person : null;

            if ( currentPerson != null && eventCalendar.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) )
            {
                return true;
            }

            SendNotAuthorized( context );
            return false;
        }

        /// <summary>
        /// Validates the request data.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private CalendarProps ValidateRequestData( HttpContext context )
        {
            CalendarProps calendarProps = new CalendarProps();

            // Security check made sure the calendar ID is good so no need to check it again.
            calendarProps.CalendarId = int.Parse( request.QueryString["calendarid"] );

            string campusIdQueryString = request.QueryString["campusids"] != null ? request.QueryString["campusids"] : string.Empty;
            calendarProps.CampusIds = ParseIds( campusIdQueryString );

            string audienceIdQueryString = request.QueryString["audienceids"] != null ? request.QueryString["audienceids"] : string.Empty;
            calendarProps.AudienceIds = ParseIds( audienceIdQueryString );

            string startDate = request.QueryString["startDate"];
            if ( !string.IsNullOrWhiteSpace( startDate ) )
            {
                calendarProps.StartDate = DateTime.ParseExact( startDate, "yyyyMMDD", CultureInfo.InvariantCulture );
            }

            string endDate = request.QueryString["endDate"];
            if ( !string.IsNullOrWhiteSpace( endDate ) )
            {
                calendarProps.EndDate = DateTime.ParseExact( endDate, "yyyyMMDD", CultureInfo.InvariantCulture );
            }

            return calendarProps;
        }

        /// <summary>
        /// Parses a query string for a list of Ids
        /// </summary>
        /// <returns></returns>
        private List<int> ParseIds( string queryParamemter )
        {
            List<string> stringIdList = new List<string>();
            List<int> intIdList = new List<int>();

            if ( queryParamemter.IsNotNullOrWhitespace() )
            {
                stringIdList = queryParamemter.Split( ',' ).ToList();

                foreach ( string stringId in stringIdList )
                {
                    int intId;
                    if ( int.TryParse( stringId, out intId ) )
                    {
                        intIdList.Add( intId );
                    }
                }
            }

            return intIdList;
        }

        /// <summary>
        /// CalendarId is required. CampusIds, AudienceIds, Startdate, and Enddate are optional.
        /// StartDate defaults to the current date, EndDate defaults to the currentDate + 2 months.
        /// </summary>
        private class CalendarProps
        {
            private DateTime? _startDate;
            private DateTime? _endDate;

            /// <summary>
            /// Gets or sets the calendar id.
            /// </summary>
            /// <value>
            /// The calendar identifier.
            /// </value>
            public int CalendarId { get; set; }

            /// <summary>
            /// Gets or sets the campus ids. Leave empty to return all campuses
            /// </summary>
            /// <value>
            /// The campus ids.
            /// </value>
            public List<int> CampusIds { get; set; }

            /// <summary>
            /// Gets or sets the audience ids list. leave empty to return all audiences
            /// </summary>
            /// <value>
            /// The audience ids.
            /// </value>
            public List<int> AudienceIds { get; set; }

            /// <summary>
            /// Gets or sets the start date. if not explictly set returns current date
            /// </summary>
            /// <value>
            /// The start date.
            /// </value>
            public DateTime StartDate
            {
                get
                {
                    return _startDate != null ? (DateTime) _startDate : DateTime.Now.Date;
                }

                set
                {
                    _startDate = value;
                }
            }

            /// <summary>
            /// Gets or sets the end date. If not explictly set returns two months from current date.
            /// </summary>
            /// <value>
            /// The end date.
            /// </value>
            public DateTime EndDate
            {
                get
                {
                    return _endDate != null ? (DateTime) _endDate : DateTime.Now.AddMonths( 2 ).Date;
                }

                set
                {
                    _endDate = value;
                }
            }
        }
    }
}
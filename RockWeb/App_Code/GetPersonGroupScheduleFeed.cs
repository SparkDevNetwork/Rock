﻿// <copyright>
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
using Ical.Net.DataTypes;
using Calendar = Ical.Net.Calendar;

using System.Globalization;
using System.Data.Entity;
using Ical.Net.Serialization;
using Ical.Net.CalendarComponents;
using Rock.Web.Cache;

namespace RockWeb
{
    /// <summary>
    /// Http Handler to get the volunteer Group Scheduler calendar feed for a person.
    /// </summary>
    public class GetPersonGroupScheduleFeed : IHttpHandler
    {
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
            string interactionDeviceType = InteractionDeviceType.GetClientType( httpContext.Request.UserAgent );

            try
            {
                CalendarProps calendarProps = ValidateRequestData( httpContext );

                if ( calendarProps == null )
                {
                    return;
                }

                var icalendar = CreateICalendar( calendarProps, interactionDeviceType );

                var serializer = new CalendarSerializer();
                string s = serializer.SerializeToString( icalendar );

                httpContext.Response.Clear();
                httpContext.Response.ClearHeaders();
                httpContext.Response.ClearContent();
                httpContext.Response.AddHeader( "content-disposition", string.Format( "attachment; filename={0}_ical.ics", RockDateTime.Now.ToString( "yyyy-MM-dd_hhmmss" ) ) );
                httpContext.Response.ContentType = "text/calendar";
                httpContext.Response.Write( s );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, httpContext );
                SendBadRequest( httpContext );
            }
        }

        /// <summary>
        /// Creates the iCalendar object and populates it with events
        /// </summary>
        /// <param name="calendarProps">The calendar props.</param>
        /// <returns></returns>
        private Calendar CreateICalendar( CalendarProps calendarProps, string interactionDeviceType )
        {
            // Get a list of confirmed attendances filtered by calendarProps
            List<Attendance> attendances = GetAttendances( calendarProps );

            // Create the iCalendar
            var icalendar = new Calendar();
            icalendar.AddTimeZone( TimeZoneInfo.Local );
            TimeSpan duration = TimeSpan.MinValue;
            int currentScheduleId = -1;

            // Create each of the attendances
            foreach ( var attendance in attendances )
            {
                using ( var rockContext = new RockContext() )
                {
                    var attendanceOccurrenceService = new AttendanceOccurrenceService( rockContext );

                    var schedule = attendanceOccurrenceService
                        .Queryable()
                        .AsNoTracking()
                        .Where( a => a.Id == attendance.OccurrenceId )
                        .Select( a => a.Schedule )
                        .FirstOrDefault();

                    string scheduleName = schedule.Name;

                    // TODO: Construct a description that includes the group leader contact info and the URL to the schedule toolbox.
                    string description = schedule.Description;

                    string locationName = attendanceOccurrenceService
                        .Queryable()
                        .AsNoTracking()
                        .Where( a => a.Id == attendance.OccurrenceId )
                        .Select( a => a.Location.Name )
                        .FirstOrDefault() ?? string.Empty;

                    if ( schedule.Id != currentScheduleId )
                    {
                        // We have to get the duration from Schedule.iCal for this attendance.
                        // Attendances are ordered by scheduleId so this only happens once for each unique schedule.
                        var calendar = Calendar.Load( schedule.iCalendarContent );
                        var scheduleEvent = calendar?.Events[0];
                        if ( scheduleEvent != null )
                        {
                            duration = scheduleEvent.Duration;
                        }

                        currentScheduleId = schedule.Id;
                    }

                    int groupId = attendanceOccurrenceService
                        .Queryable()
                        .AsNoTracking()
                        .Where( a => a.Id == attendance.OccurrenceId )
                        .Select( a => a.GroupId )
                        .FirstOrDefault() ?? -1;

                    var group = GroupCache.Get( groupId );

                    var iCalEvent = new CalendarEvent();
                    iCalEvent.Summary = $"{group.Name} - {locationName} - {scheduleName}";
                    iCalEvent.Location = locationName;
                    iCalEvent.DtStart = new CalDateTime( attendance.StartDateTime, icalendar.TimeZones[0].TzId );
                    iCalEvent.Duration = duration;

                    // Set a unique identifier for this occurrence. Google Calendar does not import events that have duplicate identifiers.
                    iCalEvent.Uid = attendance.Occurrence?.Guid.ToString();

                    // Don't set the description prop for outlook to force it to use the X-ALT-DESC property which can have markup.
                    if ( interactionDeviceType != "Outlook" )
                    {
                        iCalEvent.Description = description.ConvertBrToCrLf().SanitizeHtml();
                    }

                    // HTML version of the description for outlook
                    iCalEvent.AddProperty( "X-ALT-DESC;FMTTYPE=text/html", "<html>" + description + "</html>" );

                    // classification: "PUBLIC", "PRIVATE", "CONFIDENTIAL"
                    iCalEvent.Class = "PUBLIC";

                    icalendar.Events.Add( iCalEvent );
                }
            }

            return icalendar;
        }

        /// <summary>
        /// Uses the filter information in the CalendarProps object to get a list of attendance records and orders them by schedule ID.
        /// </summary>
        /// <param name="calendarProps">The calendar props.</param>
        /// <returns></returns>
        private List<Attendance> GetAttendances( CalendarProps calendarProps )
        {
            using ( var rockContext = new RockContext() )
            {
                var attendanceService = new AttendanceService( rockContext );
                var attendances = attendanceService
                    .GetConfirmedScheduled()
                    .Include( a => a.Occurrence )
                    .AsNoTracking()
                    .Where( a => a.PersonAlias.PersonId == calendarProps.PersonId )
                    .Where( a => a.Occurrence.OccurrenceDate >= calendarProps.StartDate )
                    .Where( a => a.Occurrence.OccurrenceDate <= calendarProps.EndDate )
                    .OrderBy( a => a.Occurrence.ScheduleId );

                return attendances.ToList();
            }
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
            httpContext.Response.StatusDescription = "Request is invalid or malformed. " + addlInfo;
            httpContext.ApplicationInstance.CompleteRequest();
        }

        /// <summary>
        /// Validates the request data.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private CalendarProps ValidateRequestData( HttpContext httpContext )
        {
            CalendarProps calendarProps = new CalendarProps();

            Guid? personAliasGuid = httpContext.Request.QueryString["paguid"].AsGuidOrNull();

            if ( personAliasGuid == null )
            {
                SendBadRequest( httpContext );
                return null;
            }

            using ( var rockContext = new RockContext() )
            {
                var personAliasService = new PersonAliasService( rockContext );
                int? personId = personAliasService.Queryable().AsNoTracking().Where( pa => pa.Guid == personAliasGuid ).Select( pa => pa.PersonId ).Cast<int?>().FirstOrDefault();

                if ( personId == null )
                {
                    SendBadRequest( httpContext );
                    return null;
                }

                calendarProps.PersonId = personId.Value;
            }

            string startDate = httpContext.Request.QueryString["startdate"];
            if ( !string.IsNullOrWhiteSpace( startDate ) )
            {
                calendarProps.StartDate = DateTime.ParseExact( startDate, "yyyyMMdd", CultureInfo.InvariantCulture );
            }

            string endDate = httpContext.Request.QueryString["enddate"];
            if ( !string.IsNullOrWhiteSpace( endDate ) )
            {
                calendarProps.EndDate = DateTime.ParseExact( endDate, "yyyyMMdd", CultureInfo.InvariantCulture );
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

            if ( queryParamemter.IsNotNullOrWhiteSpace() )
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
            public int PersonId { get; set; }

            /// <summary>
            /// Gets or sets the start date. if not explicitly set returns a date minus 1 year
            /// </summary>
            /// <value>
            /// The start date.
            /// </value>
            public DateTime StartDate
            {
                get
                {
                    return _startDate != null ? ( DateTime ) _startDate : RockDateTime.Now.AddYears( -1 ).Date;
                }

                set
                {
                    _startDate = value;
                }
            }

            /// <summary>
            /// Gets or sets the end date. If not explicitly set returns max value
            /// </summary>
            /// <value>
            /// The end date.
            /// </value>
            public DateTime EndDate
            {
                get
                {
                    return _endDate != null ? ( DateTime ) _endDate : DateTime.MaxValue;
                }

                set
                {
                    _endDate = value;
                }
            }
        }
    }
}
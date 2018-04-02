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
using System.Web;
using System.Xml;
using System.Text;
using System.Net;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

using DDay.iCal;
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

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        public void ProcessRequest( HttpContext httpContext )
        {
            request = httpContext.Request;
            response = httpContext.Response;

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

            


            iCalendar icalendar = new iCalendar();
            


        }


        private void SendNotAuthorized( HttpContext httpContext )
        {
            httpContext.Response.StatusCode = HttpStatusCode.Forbidden.ConvertToInt();
            httpContext.Response.StatusDescription = "Not authorized to view calendar.";
            httpContext.ApplicationInstance.CompleteRequest();
        }

        private void SendBadRequest( HttpContext httpContext, string addlInfo = "" )
        {
            httpContext.Response.StatusCode = HttpStatusCode.BadRequest.ConvertToInt();
            httpContext.Response.StatusDescription = "Request is inavalid or malformed. " + addlInfo;
        }

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

            string campusIdQueryString = request.QueryString["campusIds"] != null ? request.QueryString["campusIds"] : string.Empty;
            calendarProps.CampusIds = ParseIds( campusIdQueryString );

            string audienceIdQueryString = request.QueryString["audienceIds"] != null ? request.QueryString["audienceIds"] : string.Empty;
            calendarProps.AudienceIds = ParseIds( audienceIdQueryString );

            string startDate = request.QueryString["startDate"] != null ? request.QueryString["startDate"] : string.Empty;
            calendarProps.StartDate = DateTime.ParseExact( startDate, "yyyyMMDD", CultureInfo.InvariantCulture );

            string endDate = request.QueryString["endDate"] != null ? request.QueryString["endDate"] : string.Empty;
            calendarProps.EndDate = DateTime.ParseExact( endDate, "yyyyMMDD", CultureInfo.InvariantCulture );

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
            DateTime _startDate;
            DateTime _endDate;

            public int CalendarId { get; set; }
            public List<int> CampusIds { get; set; }
            public List<int> AudienceIds { get; set; }
            public DateTime StartDate
            {
                get
                {
                    return _startDate != null ? _startDate : DateTime.Now.Date;
                }

                set
                {
                    _startDate = value;
                }
            }

            public DateTime EndDate
            {
                get
                {
                    return _endDate != null ? _endDate : DateTime.Now.AddMonths( 2 ).Date;
                }

                set
                {
                    _endDate = value;
                }
            }


        }

    }
}
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
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

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
            try
            {
                request = httpContext.Request;
                response = httpContext.Response;

                if ( !ValidateSecurity( httpContext ) )
                {
                    return;
                }

                RockContext rockContext = new RockContext();
                GetCalendarEventFeedArgs calendarProps = ValidateRequestData( httpContext );

                if ( calendarProps == null )
                {
                    return;
                }

                calendarProps.ClientDeviceType = InteractionDeviceType.GetClientType( request.UserAgent );

                // get the lava template
                int templateDefinedValueId = 0;
                var iCalTemplateDefinedValue = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.DEFAULT_ICAL_DESCRIPTION );
                if ( request.QueryString["templateid"] != null )
                {
                    int.TryParse( request.QueryString["templateid"], out templateDefinedValueId );
                    if ( templateDefinedValueId > 0 )
                    {
                        iCalTemplateDefinedValue = DefinedValueCache.Get( templateDefinedValueId );
                    }
                }
                calendarProps.EventCalendarLavaTemplate = iCalTemplateDefinedValue.GetAttributeValue( "Template" );

                var eventCalendarService = new EventCalendarService( rockContext );
                var icalendarString = eventCalendarService.CreateICalendar( calendarProps );

                response.Clear();
                response.ClearHeaders();
                response.ClearContent();
                response.AddHeader( "content-disposition", string.Format( "attachment; filename={0}_ical.ics", RockDateTime.Now.ToString( "yyyy-MM-dd_hhmmss" ) ) );
                response.ContentType = "text/calendar";
                response.Write( icalendarString );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, httpContext );
                SendBadRequest( httpContext );
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
                SendBadRequest( context );
                return false;
            }


            // Need to replace CurrentUser with the result of a person token, in the meantime this will always create a null person unless directly downloadng the ical when logged into the site
            UserLogin currentUser = new UserLoginService( rockContext ).GetByUserName( UserLogin.GetCurrentUserName() );
            Person currentPerson = currentUser != null ? currentUser.Person : null;
            var isAuthorized = eventCalendar.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson );

            if ( isAuthorized )
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
        private GetCalendarEventFeedArgs ValidateRequestData( HttpContext context )
        {
            var calendarProps = new GetCalendarEventFeedArgs();

            // Security check made sure the calendar ID is good so no need to check it again.
            calendarProps.CalendarId = int.Parse( request.QueryString["calendarid"] );

            string campusIdQueryString = request.QueryString["campusids"] != null ? request.QueryString["campusids"] : string.Empty;
            calendarProps.CampusIds = ParseIds( campusIdQueryString );

            string audienceIdQueryString = request.QueryString["audienceids"] != null ? request.QueryString["audienceids"] : string.Empty;
            calendarProps.AudienceIds = ParseIds( audienceIdQueryString );

            string startDate = request.QueryString["startdate"];
            if ( !string.IsNullOrWhiteSpace( startDate ) )
            {
                calendarProps.StartDate = DateTime.ParseExact( startDate, "yyyyMMdd", CultureInfo.InvariantCulture );
            }

            string endDate = request.QueryString["enddate"];
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

    }
}
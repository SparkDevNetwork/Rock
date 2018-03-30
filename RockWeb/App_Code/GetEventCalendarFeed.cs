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

        public void ProcessRequest( HttpContext context )
        {
            request = context.Request;
            response = context.Response;

            RockContext rockContext = new RockContext();
            CalendarProps calendarProps = ValidateRequestData( context );

            if ( calendarProps == null )
            {
                return;
            }

            if ( !ValidateSecurity( context ) )
            {
                return;
            }


            iCalendar icalendar = new iCalendar();
            


        }


        private void SendNotAuthorized( HttpContext context )
        {
            context.Response.StatusCode = HttpStatusCode.Forbidden.ConvertToInt();
            context.Response.StatusDescription = "Not authorized to view calendar.";
            context.ApplicationInstance.CompleteRequest();
        }

        private void SendBadRequest( HttpContext context, string addlInfo = "" )
        {
            context.Response.StatusCode = HttpStatusCode.BadRequest.ConvertToInt();
            context.Response.StatusDescription = "Request is inavalid or malformed. " + addlInfo;
        }

        private bool ValidateSecurity( HttpContext context )
        {
            RockContext rockContext = new RockContext();

            UserLogin currentUser = new UserLoginService( rockContext ).GetByUserName( UserLogin.GetCurrentUserName() );
            Person currentPerson = currentUser != null ? currentUser.Person : null;

            EventCalendarService eventCalendarService = new EventCalendarService( rockContext );

            EventCalendar eventCalendar = eventCalendarService.Get( 1 );

            //eventCalendar.IsAuthorized( Authorization.VIEW, currentPerson )
            
            return true;
        }

        private CalendarProps ValidateRequestData( HttpContext context )
        {
            CalendarProps calendarProps = new CalendarProps();

            // CalendarID is required
            int calendarId;
            if ( request.QueryString["calendarid"] == null || !int.TryParse(request.QueryString["calendarid"], out calendarId ) )
            {
                SendBadRequest( context, "Calendar ID is missing or not valid." );
                return null;
            }
            
            // Campus, audiences, startdate, enddate are optional



            return calendarProps;
        }

        private class CalendarProps
        {
            public int CalendarId { get; set; }
            public List<int> CampusIds { get; set; }
            public List<int> AudienceIds { get; set; }
            public DateTime startDate { get; set; }
            public DateTime endDate { get; set; }

        }

    }
}
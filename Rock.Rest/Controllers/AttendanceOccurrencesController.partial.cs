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
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using Ical.Net;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public partial class AttendanceOccurrencesController
    {
        /// <summary>
        /// Gets all the occurrences for a group for the selected dates, location and schedule.
        /// </summary>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/AttendanceOccurrences/GetFutureGroupOccurrences" )]
        public List<GroupOccurrenceResponse> GetFutureGroupOccurrences( int groupId, DateTime? toDateTime = null, string locationIds = null, string scheduleIds = null )
        {
            using ( var rockContext = new RockContext() )
            {
                rockContext.Configuration.ProxyCreationEnabled = false;
                var group = new GroupService( rockContext ).Get( groupId );

                var occurrences = new AttendanceOccurrenceService( rockContext )
                    .GetFutureGroupOccurrences( group, toDateTime, locationIds, scheduleIds);

                var response = new List<GroupOccurrenceResponse>(); 
                foreach ( var occurrence in occurrences )
                {
                    response.Add( new GroupOccurrenceResponse( occurrence ) );
                }
                return response;
            }
        }

        /// <summary>
        /// Creates a new attendance occurrence for a group.
        /// </summary>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route("api/AttendanceOccurrences/CreateGroupOccurrence")]
        public AttendanceOccurrence CreateGroupOccurrence( int groupId, DateTime occurrenceDate, int? scheduleId = null, int? locationId = null )
        {
            return new AttendanceOccurrenceService( new RockContext () ).GetOrAdd( occurrenceDate, groupId, locationId, scheduleId );
        }

        /// <summary>
        /// Object used by GetFutureGroupOccurrences to return a formatted title along with the AttendanceOccurrence record.
        /// </summary>
        public class GroupOccurrenceResponse
        {
            /// <summary>
            /// The AttendaneOccurrence.
            /// </summary>
            public AttendanceOccurrence Occurrence;

            /// <summary>
            /// A formatted title for public display.
            /// </summary>
            public string DisplayTitle;

            /// <summary>
            /// Sets the DisplayTitle.
            /// </summary>
            private void GetOccurrenceTitle()
            {
                bool hasSchedule = ( Occurrence.Schedule != null );
                
                if ( hasSchedule )
                {
                    var calendarEvent = Occurrence.Schedule.GetICalEvent();
                    if ( calendarEvent == null )
                    {
                        hasSchedule = false;
                    }
                }

                // Format date and time.
                if ( hasSchedule )
                {
                    DisplayTitle = string.Format(
                        "{0} - {1}, {2}",
                        Occurrence.Group.Name,
                        Occurrence.OccurrenceDate.ToString( "dddd, MMMM d, yyyy" ),
                        Occurrence.Schedule.GetICalEvent().DtStart.Value.TimeOfDay.ToTimeString() );
                }
                else
                {
                    DisplayTitle = string.Format(
                        "{0} - {1}",
                        Occurrence.Group.Name,
                        Occurrence.OccurrenceDate.ToString( "dddd, MMMM d, yyyy" ) );
                }

                // Add Location title.
                if ( Occurrence.Location != null )
                {
                    DisplayTitle = string.Format(
                        "{0} - {1}",
                        DisplayTitle,
                        Occurrence.Location.EntityStringValue );
                }
            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="occurrence"></param>
            public GroupOccurrenceResponse( AttendanceOccurrence occurrence )
            {
                Occurrence = occurrence;
                GetOccurrenceTitle();
            }

        }

    }
}

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
using Rock.Web.Cache;

namespace Rock.Rest.Controllers
{
    /// <summary>
    ///
    /// </summary>
    public partial class AttendanceOccurrencesController
    {
        /// <summary>
        /// Gets all the occurrences for a group for the selected dates, location and schedule, sorted by occurrence data in ascending order.
        /// </summary>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/AttendanceOccurrences/GetFutureGroupOccurrences" )]
        [Rock.SystemGuid.RestActionGuid( "D5B342D0-CDFF-4895-9716-B1EEEF19C38C" )]
        public List<GroupOccurrenceResponse> GetFutureGroupOccurrences( int groupId, DateTime? toDateTime = null, string locationIds = null, string scheduleIds = null )
        {
            using ( var rockContext = new RockContext() )
            {
                rockContext.Configuration.ProxyCreationEnabled = false;
                var group = new GroupService( rockContext ).Get( groupId );

                var occurrences = new AttendanceOccurrenceService( rockContext )
                    .GetFutureGroupOccurrences( group, toDateTime, locationIds, scheduleIds );

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
        [System.Web.Http.Route( "api/AttendanceOccurrences/CreateGroupOccurrence" )]
        [Rock.SystemGuid.RestActionGuid( "07AE6B44-790D-42C5-AA02-BBEFA63E97ED" )]
        public AttendanceOccurrence CreateGroupOccurrence( int groupId, DateTime occurrenceDate, int? scheduleId = null, int? locationId = null )
        {
            return new AttendanceOccurrenceService( new RockContext() ).GetOrAdd( occurrenceDate, groupId, locationId, scheduleId );
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
                if ( Occurrence.LocationId.HasValue )
                {
                    var location = NamedLocationCache.Get( Occurrence.LocationId.Value );
                    if ( location != null )
                    {
                        DisplayTitle = string.Format(
                            "{0} - {1}",
                            DisplayTitle,
                            location.ToString() );
                    }
                }

                // Add Name if it has one
                if ( ! string.IsNullOrEmpty( Occurrence.Name ) )
                {
                    DisplayTitle = string.Format(
                        "{0} ({1})",
                        DisplayTitle,
                        Occurrence.Name );
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
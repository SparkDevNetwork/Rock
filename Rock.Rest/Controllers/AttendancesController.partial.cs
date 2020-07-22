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
using System.Net;
using System.Net.Http;
using System.Web.Http;

using Rock.Chart;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public partial class AttendancesController
    {
        /// <summary>
        /// Gets the chart data.
        /// </summary>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/Attendances/GetChartData" )]
        public IEnumerable<IChartData> GetChartData( ChartGroupBy groupBy = ChartGroupBy.Week, AttendanceGraphBy graphBy = AttendanceGraphBy.Total, DateTime? startDate = null, DateTime? endDate = null, string groupIds = null, string campusIds = null, string scheduleIds = null, int? dataViewId = null )
        {
            return new AttendanceService( new RockContext() ).GetChartData( groupBy, graphBy, startDate, endDate, groupIds, campusIds, dataViewId, scheduleIds );
        }

        /// <summary>
        /// Adds an attendance. If the AttendanceOccurrence record does not exist it is created. If the Attendance record already exists then it is updated.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="scheduleId">The schedule identifier.</param>
        /// <param name="occurrenceDate">The occurrence date.</param>
        /// <param name="personId">The person identifier. If provided it is used to get the primary PersonAliasId and takes presidence over "personAliasId"</param>
        /// <param name="personAliasId">The person alias identifier. Is not used if a "personId" is provided.</param>
        /// <returns>Attendance</returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/Attendances/AddAttendance" )]
        [HttpPut]
        public Attendance AddAttendance( int groupId, int locationId, int scheduleId, DateTime occurrenceDate, int? personId = null, int? personAliasId = null )
        {
            using ( var rockContext = new RockContext() )
            {
                // If personId is provided set the personAliasId to the primary alias of the person.
                if ( personId != null )
                {
                    personAliasId = new PersonService( rockContext ).Get( personId.Value ).PrimaryAliasId;
                }

                if ( personAliasId == null )
                {
                    var response = new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        Content = new StringContent( "PersonId or PersonAliasId is required." )
                    };

                    throw new HttpResponseException( response );
                }

                var attendance = new AttendanceService( rockContext ).AddOrUpdate( personAliasId.Value, occurrenceDate, groupId, locationId, scheduleId, null );
                rockContext.SaveChanges();
                return attendance;
            }
        }

        #region Group Scheduler Related

        /// <summary>
        /// Gets a list of available the scheduler resources (people) based on the options specified in schedulerResourceParameters 
        /// </summary>
        /// <param name="schedulerResourceParameters">The scheduler resource parameters.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/Attendances/GetSchedulerResources" )]
        [HttpPost]
        public IEnumerable<SchedulerResource> GetSchedulerResources( [FromBody]SchedulerResourceParameters schedulerResourceParameters )
        {
            var rockContext = new RockContext();
            var attendanceService = new AttendanceService( rockContext );
            return attendanceService.GetSchedulerResources( schedulerResourceParameters );
        }

        /// <summary>
        /// Gets a list of scheduled attendances ( people that are scheduled ) for an attendance occurrence
        /// </summary>
        /// <param name="attendanceOccurrenceId">The attendance occurrence identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/Attendances/GetAttendingSchedulerResources" )]
        [HttpGet]
        public IEnumerable<SchedulerResourceAttend> GetAttendingSchedulerResources( int attendanceOccurrenceId )
        {
            var rockContext = new RockContext();
            var attendanceService = new AttendanceService( rockContext );

            return attendanceService.GetAttendingSchedulerResources( attendanceOccurrenceId );
        }

        /// <summary>
        /// Updates attendance record to indicate person is not pending, or confirmed, or declined
        /// </summary>
        /// <param name="attendanceId">The attendance identifier.</param>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/Attendances/ScheduledPersonRemove" )]
        [HttpPut]
        public void ScheduledPersonRemove( int attendanceId )
        {
            var rockContext = new RockContext();
            var attendanceService = new AttendanceService( rockContext );

            attendanceService.ScheduledPersonRemove( attendanceId );
            rockContext.SaveChanges();
        }

        /// <summary>
        /// Schedules a person to an attendance and marks them as pending
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="attendanceOccurrenceId">The attendance occurrence identifier.</param>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/Attendances/ScheduledPersonAddPending" )]
        [HttpPut]
        public Attendance ScheduledPersonAddPending( int personId, int attendanceOccurrenceId )
        {
            var rockContext = new RockContext();
            var attendanceService = new AttendanceService( rockContext );

            var currentPersonAlias = this.GetPersonAlias();

            var result = attendanceService.ScheduledPersonAddPending( personId, attendanceOccurrenceId, currentPersonAlias );
            rockContext.SaveChanges();

            return result;
        }

        /// <summary>
        /// Schedules a person to an attendance and immediately marks them as confirmed
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="attendanceOccurrenceId">The attendance occurrence identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/Attendances/ScheduledPersonAddConfirmed" )]
        [HttpPut]
        public Attendance ScheduledPersonAddConfirmed( int personId, int attendanceOccurrenceId )
        {
            var rockContext = new RockContext();
            var attendanceService = new AttendanceService( rockContext );

            var currentPersonAlias = this.GetPersonAlias();

            var result = attendanceService.ScheduledPersonAddPending( personId, attendanceOccurrenceId, currentPersonAlias );
            rockContext.SaveChanges();
            var attendanceId = result.Id;

            attendanceService.ScheduledPersonConfirm( attendanceId );

            rockContext.SaveChanges();

            return result;
        }

        /// <summary>
        /// Sets a person's status to pending for the scheduled attendance
        /// </summary>
        /// <param name="attendanceId">The attendance identifier.</param>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/Attendances/ScheduledPersonPending" )]
        [HttpPut]
        public void ScheduledPersonPending( int attendanceId )
        {
            var rockContext = new RockContext();
            var attendanceService = new AttendanceService( rockContext );
            attendanceService.ScheduledPersonPending( attendanceId );

            rockContext.SaveChanges();
        }

        /// <summary>
        /// Confirms a person for a scheduled attendance
        /// </summary>
        /// <param name="attendanceId">The attendance identifier.</param>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/Attendances/ScheduledPersonConfirm" )]
        [HttpPut]
        public void ScheduledPersonConfirm( int attendanceId )
        {
            var rockContext = new RockContext();
            var attendanceService = new AttendanceService( rockContext );
            attendanceService.ScheduledPersonConfirm( attendanceId );

            rockContext.SaveChanges();
        }

        /// <summary>
        /// Set person as declined for a scheduled attendance
        /// </summary>
        /// <param name="attendanceId">The attendance identifier.</param>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/Attendances/ScheduledPersonDecline" )]
        [HttpPut]
        public void ScheduledPersonDecline( int attendanceId )
        {
            var rockContext = new RockContext();
            var attendanceService = new AttendanceService( rockContext );
            attendanceService.ScheduledPersonDecline( attendanceId, null );

            rockContext.SaveChanges();
        }

        /// <summary>
        /// Sends (or Re-sends) a confirmation email to the person in the specified scheduled attendance record
        /// </summary>
        /// <param name="attendanceId">The attendance identifier.</param>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/Attendances/ScheduledPersonSendConfirmationEmail" )]
        [HttpPut]
        public void ScheduledPersonSendConfirmationEmail( int attendanceId )
        {
            var rockContext = new RockContext();
            var attendanceService = new AttendanceService( rockContext );
            var sendConfirmationAttendancesQuery = attendanceService.Queryable().Where( a => a.Id == attendanceId );
            List<string> errorMessages;
            attendanceService.SendScheduleConfirmationSystemEmails( sendConfirmationAttendancesQuery, out errorMessages );
            rockContext.SaveChanges();
        }

        #endregion Group Scheduler Related

        #region RSVP Related

        /// <summary>
        /// This method is deprecated and should not be used as it is subject to the limits of the maximum URL length of the browser.
        /// Use the method which accepts the list of Person Ids in the body of the request, instead.
        /// </summary>
        /// <param name="occurrenceId">The ID of the AttendanceOccurrence record.</param>
        /// <param name="personIds">A comma-delimited list of Person IDs.</param>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/Attendances/RegisterRSVPRecipients" )]
        [HttpPost]
        [Obsolete( "Use the method which accepts a List<string> parameter instead." )]
        [RockObsolete( "1.10.4" )]
        public void RegisterRSVPRecipients( int occurrenceId, string personIds )
        {
            new AttendanceService( new RockContext() )
                .RegisterRSVPRecipients( occurrenceId, personIds );
        }

        /// <summary>
        /// Creates attendance records if they don't exist for a designated occurrence and list of person IDs.
        /// </summary>
        /// <param name="occurrenceId">The ID of the AttendanceOccurrence record.</param>
        /// <param name="personIds">A list of Person IDs.</param>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/Attendances/RegisterRSVPRecipients" )]
        [HttpPost]
        public void RegisterRSVPRecipients( int occurrenceId, [FromBody] List<string> personIds )
        {
            var personIdList = personIds.Select( int.Parse ).ToList();
            new AttendanceService( new RockContext() )
                .RegisterRSVPRecipients( occurrenceId, personIdList );
        }


        #endregion RSVP Related

        #region Import related

        /// <summary>
        /// Import Attendance Records using BulkInsert
        /// </summary>
        /// <remarks>
        /// For best performance, limit to 1000 records at a time.
        /// Either the PersonId or PersonAliasId value can be specified, but at least one is required.
        /// </remarks>
        /// <param name="attendancesImport">The Attendances to bulk import.</param>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "api/Attendances/Import" )]
        public void AttendanceImport( Rock.BulkImport.AttendancesImport attendancesImport )
        {
            if ( attendancesImport == null )
            {
                var response = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent( "AttendancesImport data is required" )
                };

                throw new HttpResponseException( response );
            }

            AttendanceService.BulkAttendanceImport( attendancesImport );
        }

        #endregion Import related

    }
}

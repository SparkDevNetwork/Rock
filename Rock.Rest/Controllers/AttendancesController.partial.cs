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
        [Rock.SystemGuid.RestActionGuid( "7611D15B-55CD-4D84-96B5-1A11340D8D8A" )]
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
        /// <param name="personId">The person identifier. If provided it is used to get the primary PersonAliasId and takes precedence over "personAliasId"</param>
        /// <param name="personAliasId">The person alias identifier. Is not used if a "personId" is provided.</param>
        /// <returns>Attendance</returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/Attendances/AddAttendance" )]
        [HttpPut]
        [Rock.SystemGuid.RestActionGuid( "68FF77EB-7A7C-4763-A090-0F917CC2E033" )]
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
        [Rock.SystemGuid.RestActionGuid( "DDBDDBE0-7032-49B2-82E7-08DBA28F1FC1" )]
        public IEnumerable<SchedulerResource> GetSchedulerResources( [FromBody] SchedulerResourceParameters schedulerResourceParameters )
        {
            var rockContext = new RockContext();
            var attendanceService = new AttendanceService( rockContext );
            return attendanceService.GetSchedulerResources( schedulerResourceParameters );
        }

        /// <summary>
        /// Gets an individual scheduler resource.
        /// </summary>
        /// <param name="schedulerResourceParameters">The scheduler resource parameters.</param>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/Attendances/GetSchedulerResource" )]
        [HttpPost]
        [Rock.SystemGuid.RestActionGuid( "082BC954-89CA-4D73-A971-72446B84E92C" )]
        public SchedulerResource GetSchedulerResource( [FromBody] SchedulerResourceParameters schedulerResourceParameters, int personId )
        {
            var rockContext = new RockContext();
            var attendanceService = new AttendanceService( rockContext );
            schedulerResourceParameters.LimitToPersonId = personId;
            var result = attendanceService.GetSchedulerResources( schedulerResourceParameters ).FirstOrDefault();
            return result;
        }

        /// <summary>
        /// Gets a list of scheduled attendances ( people that are scheduled ) for an attendance occurrence
        /// </summary>
        /// <param name="attendanceOccurrenceId">The attendance occurrence identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/Attendances/GetAttendingSchedulerResources" )]
        [HttpGet]
        [Rock.SystemGuid.RestActionGuid( "CDE17F35-DA0A-47D9-80EF-994DEF9FB946" )]
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
        [Rock.SystemGuid.RestActionGuid( "5621A903-EF18-49D7-91DE-73E27E5D2B5A" )]
        public void ScheduledPersonRemove( int attendanceId )
        {
            var rockContext = new RockContext();
            var attendanceService = new AttendanceService( rockContext );

            attendanceService.ScheduledPersonClear( attendanceId );
            rockContext.SaveChanges();
        }

        /// <summary>
        /// Determines whether this the person can be scheduled for the specified occurrence
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="attendanceOccurrenceId">The attendance occurrence identifier.</param>
        /// <param name="fromAttendanceOccurrenceId">From attendance occurrence identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/Attendances/CanSchedulePerson" )]
        [Rock.SystemGuid.RestActionGuid( "3B23B14A-9D00-4D73-95BF-C6C22C1D07F8" )]
        public virtual HttpResponseMessage CanSchedulePerson( int personId, int attendanceOccurrenceId, int? fromAttendanceOccurrenceId = null )
        {
            var rockContext = new RockContext();
            var attendanceService = new AttendanceService( rockContext );

            var attendanceOccurrenceInfo = new AttendanceOccurrenceService( rockContext ).GetSelect( attendanceOccurrenceId, s => new
            {
                s.ScheduleId,
                s.OccurrenceDate
            } );

            if ( attendanceOccurrenceInfo == null )
            {
                return new HttpResponseMessage( HttpStatusCode.NotFound );
            }

            var scheduleId = attendanceOccurrenceInfo.ScheduleId;
            var occurrenceDate = attendanceOccurrenceInfo.OccurrenceDate;

            // Only count occurrences with active locations and active schedules as conflicting.
            var conflictingScheduledAttendancesQuery = attendanceService.Queryable().WhereDeducedIsActive();

            if ( fromAttendanceOccurrenceId.HasValue )
            {
                // if person was dragged from one occurrence to another, we can exclude both source and target occurrences when detecting conflicts
                conflictingScheduledAttendancesQuery = conflictingScheduledAttendancesQuery
                .Where( c => ( c.OccurrenceId != attendanceOccurrenceId ) && ( c.OccurrenceId != fromAttendanceOccurrenceId.Value ) );
            }
            else
            {
                // if person was dragged from resources to an occurrence, we can exclude the target occurrences when detecting conflicts
                conflictingScheduledAttendancesQuery = conflictingScheduledAttendancesQuery
                .Where( c => c.OccurrenceId != attendanceOccurrenceId );
            }

            // a conflict would be if the same person is requested/scheduled for another attendance within the same ScheduleId/Date
            conflictingScheduledAttendancesQuery = conflictingScheduledAttendancesQuery.Where( c =>
                c.PersonAlias.PersonId == personId
                && ( c.RequestedToAttend == true || c.ScheduledToAttend == true )
                && c.Occurrence.ScheduleId == scheduleId
                && c.Occurrence.OccurrenceDate == occurrenceDate );

            if ( conflictingScheduledAttendancesQuery.Any() )
            {
                var person = new PersonService( rockContext ).Get( personId );
                var firstConflict = conflictingScheduledAttendancesQuery.Select( s => new
                {
                    GroupName = s.Occurrence.Group.Name,
                    LocationName = s.Occurrence.Location.Name
                } ).FirstOrDefault();

                return ControllerContext.Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest,
                        $"{person} cannot be scheduled due a scheduling conflict with {firstConflict?.GroupName} in the {firstConflict?.LocationName}." );
            }

            return new HttpResponseMessage( HttpStatusCode.OK );
        }

        /// <summary>
        /// Schedules a person to an attendance and marks them as pending
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="attendanceOccurrenceId">The attendance occurrence identifier.</param>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/Attendances/ScheduledPersonAddPending" )]
        [HttpPut]
        [Rock.SystemGuid.RestActionGuid( "12AB7295-CAEA-4308-A63C-934E7AFD8605" )]
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
        [Rock.SystemGuid.RestActionGuid( "5517065D-0C65-4FD4-95CC-C676AABA64FE" )]
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
        [Rock.SystemGuid.RestActionGuid( "27304D4E-58F7-4B27-861D-E505FF010579" )]
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
        [Rock.SystemGuid.RestActionGuid( "35E8D73C-5783-40AB-B874-CD4A63D14CAA" )]
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
        [Rock.SystemGuid.RestActionGuid( "F9C2E4CB-A3B5-4CAF-A678-0C543774F736" )]
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
        [Obsolete( "Use ScheduledPersonSendConfirmationCommunication instead." )]
        [RockObsolete( "1.13" )]
        [Rock.SystemGuid.RestActionGuid( "8531ED24-D82A-4FFB-8F5C-3848FCC04A44" )]
        public void ScheduledPersonSendConfirmationEmail( int attendanceId )
        {
            var rockContext = new RockContext();
            var attendanceService = new AttendanceService( rockContext );
            var sendConfirmationAttendancesQuery = attendanceService.Queryable().Where( a => a.Id == attendanceId );
            List<string> errorMessages;
            attendanceService.SendScheduleConfirmationSystemEmails( sendConfirmationAttendancesQuery, out errorMessages );
            rockContext.SaveChanges();
        }

        /// <summary>
        /// Sends (or Re-sends) a confirmation email to the person in the specified scheduled attendance record
        /// </summary>
        /// <param name="attendanceId">The attendance identifier.</param>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/Attendances/ScheduledPersonSendConfirmationCommunication" )]
        [HttpPut]
        [Rock.SystemGuid.RestActionGuid( "B126E3E7-5B54-4A91-8214-2006BB4D3DEB" )]
        public void ScheduledPersonSendConfirmationCommunication( int attendanceId )
        {
            var rockContext = new RockContext();
            var attendanceService = new AttendanceService( rockContext );
            var sendConfirmationAttendancesQuery = attendanceService.Queryable().Where( a => a.Id == attendanceId );

            attendanceService.SendScheduleConfirmationCommunication( sendConfirmationAttendancesQuery );

            rockContext.SaveChanges();
        }

        #endregion Group Scheduler Related

        #region RSVP Related

        /// <summary>
        /// Creates attendance records if they don't exist for a designated occurrence and list of person IDs.
        /// </summary>
        /// <param name="occurrenceId">The ID of the AttendanceOccurrence record.</param>
        /// <param name="personIds">A list of Person IDs.</param>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/Attendances/RegisterRSVPRecipients" )]
        [HttpPost]
        [Rock.SystemGuid.RestActionGuid( "E3C50A1C-DF4E-4828-A0D0-A0BED27EDBE1" )]
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
        [Rock.SystemGuid.RestActionGuid( "19FB3AA7-5E40-4186-AA25-E4978FFCEFC4" )]
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
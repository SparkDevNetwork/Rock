// <copyright>
// Copyright by Central Christian Church
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
using System.Linq.Expressions;
using System.Reflection;

using com.centralaz.RoomManagement.Model;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Security;
using Rock.Web.Cache;
using static com.centralaz.RoomManagement.Model.ReservationService;

namespace Rock.Rest.Controllers
{
    public partial class ReservationsController : Rock.Rest.ApiController<com.centralaz.RoomManagement.Model.Reservation>
    {
        public ReservationsController() : base( new com.centralaz.RoomManagement.Model.ReservationService( new Rock.Data.RockContext() ) ) { }
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class ReservationsController
    {
        /// <summary>
        /// Gets the reservation occurrences
        /// </summary>
        /// <param name="startDateTime">The start date time. Defaults to current datetime.</param>
        /// <param name="endDateTime">The end date time. Defaults to current datetime plus one month.</param>
        /// <param name="reservationIds">An optional parameter to filter occurrences by reservations. Should be a list of integers separated by commas.</param>
        /// <param name="locationIds">An optional parameter to filter occurrences by locations. Should be a list of integers separated by commas.</param>
        /// <param name="resourceIds">An optional parameter to filter occurrences by resources. Should be a list of integers separated by commas.</param>
        /// <param name="approvalStates">An optional parameter to filter occurrences by approval state. Should be a list of strings separated by commas. If this value is null, the method will only return approved reservations.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/Reservations/GetReservationOccurrences" )]
        public IQueryable<ReservationOccurrence> GetReservationOccurrences(
            DateTime? startDateTime = null,
            DateTime? endDateTime = null,
            string reservationIds = null,
            string locationIds = null,
            string resourceIds = null,
            string approvalStates = null
            )
        {

            RockContext rockContext = new RockContext();
            ReservationService reservationService = new ReservationService( rockContext );
            var reservationQry = reservationService.Queryable();

            List<int> reservationIdList = reservationIds.SplitDelimitedValues().AsIntegerList();
            if ( reservationIdList.Any() )
            {
                reservationQry = reservationQry.Where( r => reservationIdList.Contains( r.Id ) );
            }

            List<int> locationIdList = locationIds.SplitDelimitedValues().AsIntegerList();
            if ( locationIdList.Any() )
            {
                reservationQry = reservationQry.Where( r => r.ReservationLocations.Any( rl => locationIdList.Contains( rl.LocationId ) ) );
            }

            List<int> resourceIdList = resourceIds.SplitDelimitedValues().AsIntegerList();
            if ( resourceIdList.Any() )
            {
                reservationQry = reservationQry.Where( r => r.ReservationResources.Any( rr => resourceIdList.Contains( rr.ResourceId ) ) );
            }

            List<ReservationApprovalState> approvalStateList = new List<ReservationApprovalState>();

            foreach ( var approvalString in approvalStates.SplitDelimitedValues() )
            {
                try
                {
                    approvalStateList.Add( approvalString.ConvertToEnum<ReservationApprovalState>() );
                }
                catch
                {

                }
            }

            if ( approvalStateList.Any() )
            {
                reservationQry = reservationQry.Where( r => approvalStateList.Contains( r.ApprovalState ) );
            }
            else
            {
                reservationQry = reservationQry.Where( r => r.ApprovalState == ReservationApprovalState.Approved );
            }

            if ( startDateTime == null )
            {
                startDateTime = DateTime.Now;
            }

            if ( endDateTime == null )
            {
                endDateTime = DateTime.Now.AddMonths( 1 );
            }

            var qryStartDateTime = startDateTime.Value.AddMonths( -1 );
            var qryEndDateTime = endDateTime.Value.AddMonths( 1 );

            var reservations = reservationQry.ToList();
            var reservationsWithDates = reservations
                .Select( r => new ReservationDate
                {
                    Reservation = r,
                    ReservationDateTimes = r.GetReservationTimes( qryStartDateTime, qryEndDateTime )
                } )
                .Where( r => r.ReservationDateTimes.Any() )
                .ToList();

            var reservationOccurrenceList = new List<ReservationOccurrence>();
            foreach ( var reservationWithDates in reservationsWithDates )
            {
                var reservation = reservationWithDates.Reservation;
                foreach ( var reservationDateTime in reservationWithDates.ReservationDateTimes )
                {
                    var reservationStartDateTime = reservationDateTime.StartDateTime.AddMinutes( -reservation.SetupTime ?? 0 );
                    var reservationEndDateTime = reservationDateTime.EndDateTime.AddMinutes( reservation.CleanupTime ?? 0 );

                    if (
                        ( ( reservationStartDateTime >= startDateTime ) || ( reservationEndDateTime >= startDateTime ) ) &&
                        ( ( reservationStartDateTime < endDateTime ) || ( reservationEndDateTime < endDateTime ) ) )
                    {
                        reservationOccurrenceList.Add( new ReservationOccurrence
                        {
                            ReservationId = reservation.Id,
                            ApprovalState = reservation.ApprovalState,
                            ReservationName = reservation.Name,
                            ReservationLocations = reservation.ReservationLocations.ToList(),
                            ReservationResources = reservation.ReservationResources.ToList(),
                            EventStartDateTime = reservationDateTime.StartDateTime,
                            EventEndDateTime = reservationDateTime.EndDateTime,
                            ReservationStartDateTime = reservationStartDateTime,
                            ReservationEndDateTime = reservationEndDateTime,
                            SetupPhotoId = reservation.SetupPhotoId,
                            Note = reservation.Note,
                            NumberAttending = reservation.NumberAttending,
                            ModifiedDateTime = reservation.ModifiedDateTime,
                            ScheduleId = reservation.ScheduleId
                        } );
                    }
                }
            }

            return reservationOccurrenceList.AsQueryable();
        }

    }

    /// <summary>
    /// A class to store occurrence data to be returned by the API
    /// </summary>
    public class ReservationOccurrence
    {
        public int ReservationId { get; set; }
        public ReservationApprovalState ApprovalState { get; set; }
        public String ReservationName { get; set; }
        public List<ReservationLocation> ReservationLocations { get; set; }
        public List<ReservationResource> ReservationResources { get; set; }
        public DateTime ReservationStartDateTime { get; set; }// EventStartDateTime - Setup Time
        public DateTime ReservationEndDateTime { get; set; }// EventEndDateTime + Cleanup Time
        public DateTime EventStartDateTime { get; set; }
        public DateTime EventEndDateTime { get; set; }
        public int? SetupPhotoId { get; set; }
        public string Note { get; set; }
        public int? NumberAttending { get; set; }
        public DateTime? ModifiedDateTime { get; set; }
        public int? ScheduleId { get; set; }

    }
}


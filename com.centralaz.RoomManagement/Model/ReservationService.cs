// <copyright>
// Copyright by the Central Christian Church
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
using System.Text;
using System.Threading.Tasks;
using Rock;
using Rock.Data;
using Rock.Model;

namespace com.centralaz.RoomManagement.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class ReservationService : Service<Reservation>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReservationService"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public ReservationService( RockContext context ) : base( context ) { }

        /// <summary>
        /// Gets the reservation summaries.
        /// </summary>
        /// <param name="qry">The qry.</param>
        /// <param name="filterStartDateTime">The filter start date time.</param>
        /// <param name="filterEndDateTime">The filter end date time.</param>
        /// <returns></returns>
        public List<ReservationSummary> GetReservationSummaries( IQueryable<Reservation> qry, DateTime filterStartDateTime, DateTime filterEndDateTime )
        {
            var qryStartDateTime = filterStartDateTime.AddMonths( -1 );
            var qryEndDateTime = filterEndDateTime.AddMonths( 1 );
            filterEndDateTime = filterEndDateTime.AddDays( 1 ).AddMilliseconds( -1 );

            var reservations = qry.ToList();
            var reservationsWithDates = reservations
                .Select( r => new ReservationDate
                {
                    Reservation = r,
                    ReservationDateTimes = r.GetReservationTimes( qryStartDateTime, qryEndDateTime )
                } )
                .Where( r => r.ReservationDateTimes.Any() )
                .ToList();

            var reservationSummaryList = new List<ReservationSummary>();
            foreach ( var reservationWithDates in reservationsWithDates )
            {
                var reservation = reservationWithDates.Reservation;
                foreach ( var reservationDateTime in reservationWithDates.ReservationDateTimes )
                {
                    if (
                        ( ( reservationDateTime.StartDateTime > filterStartDateTime ) || ( reservationDateTime.EndDateTime > filterStartDateTime ) ) &&
                        ( ( reservationDateTime.StartDateTime < filterEndDateTime ) || ( reservationDateTime.EndDateTime < filterEndDateTime ) ) )
                    {
                        reservationSummaryList.Add( new ReservationSummary
                        {
                            Id = reservation.Id,
                            Status = reservation.ReservationStatus!= null ? reservation.ReservationStatus.Name : reservation.IsApproved ? "Approved" : "Needs Approval",
                            ReservationName = reservation.Name,
                            ReservationLocations = reservation.ReservationLocations.ToList(),
                            ReservationResources = reservation.ReservationResources.ToList(),
                            EventStartDateTime = reservationDateTime.StartDateTime,
                            EventEndDateTime = reservationDateTime.EndDateTime,
                            ReservationStartDateTime = reservationDateTime.StartDateTime.AddMinutes( -reservation.SetupTime ?? 0 ),
                            ReservationEndDateTime = reservationDateTime.EndDateTime.AddMinutes( reservation.CleanupTime ?? 0 ),
                            EventDateTimeDescription = GetFriendlyScheduleDescription( reservationDateTime.StartDateTime, reservationDateTime.EndDateTime ),
                            EventTimeDescription = GetFriendlyScheduleDescription( reservationDateTime.StartDateTime, reservationDateTime.EndDateTime, false ),
                            ReservationDateTimeDescription = GetFriendlyScheduleDescription( reservationDateTime.StartDateTime.AddMinutes( -reservation.SetupTime ?? 0 ), reservationDateTime.EndDateTime.AddMinutes( reservation.CleanupTime ?? 0 ) ),
                            ReservationTimeDescription = GetFriendlyScheduleDescription( reservationDateTime.StartDateTime.AddMinutes( -reservation.SetupTime ?? 0 ), reservationDateTime.EndDateTime.AddMinutes( reservation.CleanupTime ?? 0 ), false )
                        } );
                    }
                }
            }
            return reservationSummaryList;
        }

        public List<int> GetReservedLocationIds( Reservation newReservation )
        {
            var deniedGuid = SystemGuid.ReservationStatus.DENIED.AsGuid();
            var newReservationSummaries = GetReservationSummaries( new List<Reservation>() { newReservation }.AsQueryable(), RockDateTime.Now.AddMonths(-1), RockDateTime.Now.AddYears( 1 ) );
            var reservedLocationIds = GetReservationSummaries( Queryable().Where( r => r.Id != newReservation.Id && r.ReservationStatus.Guid != deniedGuid ), RockDateTime.Now.AddMonths( -1 ), RockDateTime.Now.AddYears( 1 ) )
                .Where( currentReservationSummary => newReservationSummaries.Any( newReservationSummary =>
                 ( currentReservationSummary.ReservationStartDateTime > newReservationSummary.ReservationStartDateTime || currentReservationSummary.ReservationEndDateTime > newReservationSummary.ReservationStartDateTime ) &&
                 ( currentReservationSummary.ReservationStartDateTime < newReservationSummary.ReservationEndDateTime || currentReservationSummary.ReservationEndDateTime < newReservationSummary.ReservationEndDateTime )
                 ) ).SelectMany( currentReservationSummary => currentReservationSummary.ReservationLocations.Select( rl => rl.LocationId ) )
                 .Distinct()
                 .ToList();
            return reservedLocationIds;
        }

        private string GetFriendlyScheduleDescription( DateTime startDateTime, DateTime endDateTime, bool showDate = true )
        {
            if ( startDateTime.Date == endDateTime.Date )
            {
                if ( showDate )
                {
                    return String.Format( "{0} {1} - {2}", startDateTime.ToShortDateString(), startDateTime.ToShortTimeString(), endDateTime.ToShortTimeString() );
                }
                else
                {
                    return String.Format( "{0} - {1}", startDateTime.ToShortTimeString(), endDateTime.ToShortTimeString() );
                }
            }
            else
            {
                return String.Format( "{0} {1} - {2} {3}", startDateTime.ToShortDateString(), startDateTime.ToShortTimeString(), endDateTime.ToShortDateString(), endDateTime.ToShortTimeString() );
            }
        }

        public class ReservationSummary
        {
            public int Id { get; set; }
            public String Status { get; set; }
            public String ReservationName { get; set; }
            public String EventDateTimeDescription { get; set; }
            public String EventTimeDescription { get; set; }
            public String ReservationDateTimeDescription { get; set; }
            public String ReservationTimeDescription { get; set; }
            public List<ReservationLocation> ReservationLocations { get; set; }
            public List<ReservationResource> ReservationResources { get; set; }
            public DateTime ReservationStartDateTime { get; set; }
            public DateTime ReservationEndDateTime { get; set; }
            public DateTime EventStartDateTime { get; set; }
            public DateTime EventEndDateTime { get; set; }
        }

        public class ReservationDate
        {
            public Reservation Reservation { get; set; }
            public List<ReservationDateTime> ReservationDateTimes { get; set; }
        }
    }
}

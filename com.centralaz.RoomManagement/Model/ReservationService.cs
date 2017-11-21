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

        #region Reservation Methods
        /// <summary>
        /// Gets the reservation summaries.
        /// </summary>
        /// <param name="qry">The qry.</param>
        /// <param name="filterStartDateTime">The filter start date time.</param>
        /// <param name="filterEndDateTime">The filter end date time.</param>
        /// <returns></returns>
        public List<ReservationSummary> GetReservationSummaries( IQueryable<Reservation> qry, DateTime filterStartDateTime, DateTime filterEndDateTime, bool roundToDay = false )
        {
            var qryStartDateTime = filterStartDateTime.AddMonths( -1 );
            var qryEndDateTime = filterEndDateTime.AddMonths( 1 );
            if ( roundToDay )
            {
                filterEndDateTime = filterEndDateTime.AddDays( 1 ).AddMilliseconds( -1 );
            }

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
                    var reservationStartDateTime = reservationDateTime.StartDateTime.AddMinutes( -reservation.SetupTime ?? 0 );
                    var reservationEndDateTime = reservationDateTime.EndDateTime.AddMinutes( reservation.CleanupTime ?? 0 );

                    if (
                        ( ( reservationStartDateTime >= filterStartDateTime ) || ( reservationEndDateTime >= filterStartDateTime ) ) &&
                        ( ( reservationStartDateTime < filterEndDateTime ) || ( reservationEndDateTime < filterEndDateTime ) ) )
                    {
                        reservationSummaryList.Add( new ReservationSummary
                        {
                            Id = reservation.Id,
                            //Status = reservation.ReservationStatus!= null ? reservation.ReservationStatus.Name : reservation.IsApproved ? "Approved" : "Needs Approval",
                            ApprovalState = reservation.ApprovalState,
                            ReservationName = reservation.Name,
                            ReservationLocations = reservation.ReservationLocations.ToList(),
                            ReservationResources = reservation.ReservationResources.ToList(),
                            EventStartDateTime = reservationDateTime.StartDateTime,
                            EventEndDateTime = reservationDateTime.EndDateTime,
                            ReservationStartDateTime = reservationStartDateTime,
                            ReservationEndDateTime = reservationEndDateTime,
                            EventDateTimeDescription = GetFriendlyScheduleDescription( reservationDateTime.StartDateTime, reservationDateTime.EndDateTime ),
                            EventTimeDescription = GetFriendlyScheduleDescription( reservationDateTime.StartDateTime, reservationDateTime.EndDateTime, false ),
                            ReservationDateTimeDescription = GetFriendlyScheduleDescription( reservationDateTime.StartDateTime.AddMinutes( -reservation.SetupTime ?? 0 ), reservationDateTime.EndDateTime.AddMinutes( reservation.CleanupTime ?? 0 ) ),
                            ReservationTimeDescription = GetFriendlyScheduleDescription( reservationDateTime.StartDateTime.AddMinutes( -reservation.SetupTime ?? 0 ), reservationDateTime.EndDateTime.AddMinutes( reservation.CleanupTime ?? 0 ), false ),
                            SetupPhotoId = reservation.SetupPhotoId,
                            Note = reservation.Note
                        } );
                    }
                }
            }
            return reservationSummaryList;
        }

        private IEnumerable<ReservationSummary> GetConflictingReservationSummaries( Reservation newReservation )
        {
            var newReservationSummaries = GetReservationSummaries( new List<Reservation>() { newReservation }.AsQueryable(), RockDateTime.Now.AddMonths( -1 ), RockDateTime.Now.AddYears( 1 ) );
            var reservedLocationIds = GetReservationSummaries( Queryable().Where( r => r.Id != newReservation.Id && r.ApprovalState != ReservationApprovalState.Denied ), RockDateTime.Now.AddMonths( -1 ), RockDateTime.Now.AddYears( 1 ) )
                .Where( currentReservationSummary => newReservationSummaries.Any( newReservationSummary =>
                 ( currentReservationSummary.ReservationStartDateTime > newReservationSummary.ReservationStartDateTime || currentReservationSummary.ReservationEndDateTime > newReservationSummary.ReservationStartDateTime ) &&
                 ( currentReservationSummary.ReservationStartDateTime < newReservationSummary.ReservationEndDateTime || currentReservationSummary.ReservationEndDateTime < newReservationSummary.ReservationEndDateTime )
                 ) );
            return reservedLocationIds;
        }

        private string GetFriendlyScheduleDescription( DateTime startDateTime, DateTime endDateTime, bool showDate = true )
        {
            if ( startDateTime.Date == endDateTime.Date )
            {
                if ( showDate )
                {
                    return String.Format( "{0} {1} - {2}", startDateTime.ToString( "MM/dd" ), startDateTime.ToString( "hh:mmt" ).ToLower(), endDateTime.ToString( "hh:mmt" ).ToLower() );
                }
                else
                {
                    return String.Format( "{0} - {1}", startDateTime.ToString( "hh:mmt" ).ToLower(), endDateTime.ToString( "hh:mmt" ).ToLower() );
                }
            }
            else
            {
                return String.Format( "{0} {1} - {2} {3}", startDateTime.ToString( "MM/dd/yy" ), startDateTime.ToString( "hh:mmt" ).ToLower(), endDateTime.ToString( "MM/dd/yy" ), endDateTime.ToString( "hh:mmt" ).ToLower() );
            }
        }
        #endregion

        #region Location Conflict Methods

        /// <summary>
        /// Gets the  location ids for any existing non-denied reservations that have the a location as the ones in the given newReservation object.
        /// </summary>
        /// <param name="newReservation">The new reservation.</param>
        /// <returns></returns>
        public List<int> GetReservedLocationIds( Reservation newReservation )
        {
            var locationService = new LocationService( new RockContext() );
            IEnumerable<ReservationSummary> conflictingReservationSummaries = GetConflictingReservationSummaries( newReservation );

            var reservedLocationIds = conflictingReservationSummaries.SelectMany( currentReservationSummary =>
                    currentReservationSummary.ReservationLocations.Where( rl =>
                        rl.ApprovalState != ReservationLocationApprovalState.Denied )
                        .Select( rl => rl.LocationId )
                        )
                  .Distinct();

            var reservedLocationAndChildIds = new List<int>();
            reservedLocationAndChildIds.AddRange( reservedLocationIds );

            reservedLocationAndChildIds.AddRange( reservedLocationIds
                  .SelectMany( l => locationService.GetAllDescendentIds( l ) ) );

            return reservedLocationAndChildIds;
        }

        public List<ReservationConflict> GetConflictsForLocationId( int locationId, Reservation newReservation)
        {
            var locationService = new LocationService( new RockContext() );

            var locationAndAncestorIds = new List<int>();
            locationAndAncestorIds.Add( locationId );
            locationAndAncestorIds.AddRange( locationService.GetAllAncestorIds( locationId ) );
            locationAndAncestorIds.AddRange( locationService.GetAllDescendentIds( locationId ) );

            IEnumerable<ReservationSummary> conflictingReservationSummaries = GetConflictingReservationSummaries( newReservation );
            var locationConflicts = conflictingReservationSummaries.SelectMany( currentReservationSummary =>
                    currentReservationSummary.ReservationLocations.Where( rl =>
                        rl.ApprovalState != ReservationLocationApprovalState.Denied &&
                        locationAndAncestorIds.Contains( rl.LocationId ) )
                     .Select( rl => new ReservationConflict
                     {
                         LocationId = rl.LocationId,
                         Location = rl.Location,
                         ReservationId = rl.ReservationId,
                         Reservation = rl.Reservation
                     } ) )
                 .Distinct()
                 .ToList();
            return locationConflicts;
        }

        #endregion

        #region Resource Conflict Methods

        /// <summary>
        /// Gets the available resource quantity.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="reservation">The reservation.</param>
        /// <returns></returns>
        public int GetAvailableResourceQuantity( Resource resource, Reservation reservation )
        {
            // For each new reservation summary, make sure that the quantities of existing summaries that come into contact with it
            // do not exceed the resource's quantity

            var currentReservationSummaries = GetReservationSummaries( Queryable().Where( r => r.Id != reservation.Id && r.ApprovalState != ReservationApprovalState.Denied ), RockDateTime.Now.AddMonths( -1 ), RockDateTime.Now.AddYears( 1 ) );

            var reservedQuantities = GetReservationSummaries( new List<Reservation>() { reservation }.AsQueryable(), RockDateTime.Now.AddMonths( -1 ), RockDateTime.Now.AddYears( 1 ) )
                .Select( newReservationSummary =>
                    currentReservationSummaries.Where( currentReservationSummary =>
                     ( currentReservationSummary.ReservationStartDateTime > newReservationSummary.ReservationStartDateTime || currentReservationSummary.ReservationEndDateTime > newReservationSummary.ReservationStartDateTime ) &&
                     ( currentReservationSummary.ReservationStartDateTime < newReservationSummary.ReservationEndDateTime || currentReservationSummary.ReservationEndDateTime < newReservationSummary.ReservationEndDateTime )
                    )
                    .DistinctBy( reservationSummary => reservationSummary.Id )
                    .Sum( currentReservationSummary => currentReservationSummary.ReservationResources.Where( rr => rr.ApprovalState != ReservationResourceApprovalState.Denied && rr.ResourceId == resource.Id ).Sum( rr => rr.Quantity ) )
               );

            var maxReservedQuantity = reservedQuantities.Count() > 0 ? reservedQuantities.Max() : 0;
            return resource.Quantity - maxReservedQuantity;
        }

        public List<ReservationConflict> GetConflictsForResourceId( int resourceId, Reservation newReservation )
        {
            IEnumerable<ReservationSummary> conflictingReservationSummaries = GetConflictingReservationSummaries( newReservation );
            var locationConflicts = conflictingReservationSummaries.SelectMany( currentReservationSummary =>
                    currentReservationSummary.ReservationResources.Where( rr =>
                        rr.ApprovalState != ReservationResourceApprovalState.Denied &&
                        rr.ResourceId == resourceId )
                     .Select( rr => new ReservationConflict
                     {
                         ResourceId = rr.ResourceId,
                         Resource = rr.Resource,
                         ResourceQuantity = rr.Quantity,
                         ReservationId = rr.ReservationId,
                         Reservation = rr.Reservation
                     } ) )
                 .Distinct()
                 .ToList();
            return locationConflicts;
        }

        #endregion

        #region Helper Classes

        public class ReservationSummary
        {
            public int Id { get; set; }
            public ReservationApprovalState ApprovalState { get; set; }
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
            public int? SetupPhotoId { get; set; }
            public string Note { get; set; }
        }

        public class ReservationDate
        {
            public Reservation Reservation { get; set; }
            public List<ReservationDateTime> ReservationDateTimes { get; set; }
        }

        public class ReservationConflict
        {
            public int LocationId { get; set; }

            public Location Location { get; set; }

            public int ResourceId { get; set; }

            public Resource Resource { get; set; }

            public int ResourceQuantity { get; set; }

            public int ReservationId { get; set; }

            public Reservation Reservation { get; set; }
        }

        #endregion
    }
}

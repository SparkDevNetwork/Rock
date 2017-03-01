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
    public class ReservationResourceService : Service<ReservationResource>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReservationResourceService"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public ReservationResourceService( RockContext context ) : base( context ) { }

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

            var rockContext = new RockContext();
            var reservationService = new ReservationService( rockContext );
            var deniedGuid = SystemGuid.ReservationStatus.DENIED.AsGuid();

            List<Reservation> newReservationList = new List<Reservation>() { reservation };
            var currentReservationSummaries = reservationService.GetReservationSummaries( reservationService.Queryable().Where( r => r.Id != reservation.Id && r.ReservationStatus.Guid != deniedGuid ), RockDateTime.Now.AddMonths( -1 ), RockDateTime.Now.AddYears( 1 ) );

            var reservedQuantities = reservationService.GetReservationSummaries( newReservationList.AsQueryable(), RockDateTime.Now.AddMonths( -1 ), RockDateTime.Now.AddYears( 1 ) )
                .Select( newReservationSummary =>
                    currentReservationSummaries.Where( currentReservationSummary =>
                     ( currentReservationSummary.ReservationStartDateTime > newReservationSummary.ReservationStartDateTime || currentReservationSummary.ReservationEndDateTime > newReservationSummary.ReservationStartDateTime ) &&
                     ( currentReservationSummary.ReservationStartDateTime < newReservationSummary.ReservationEndDateTime || currentReservationSummary.ReservationEndDateTime < newReservationSummary.ReservationEndDateTime )
                    ).DistinctBy( reservationSummary => reservationSummary.Id ).Sum( currentReservationSummary => currentReservationSummary.ReservationResources.Where( rr => rr.ResourceId == resource.Id ).Sum( rr => rr.Quantity ) )
               );

            var maxReservedQuantity = reservedQuantities.Count() > 0 ? reservedQuantities.Max() : 0;
            return resource.Quantity - maxReservedQuantity;
        }
    }
}

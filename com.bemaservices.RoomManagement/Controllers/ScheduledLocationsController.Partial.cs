// <copyright>
// Copyright by BEMA Software Services
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
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using com.bemaservices.RoomManagement.Model;

using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web.UI.Controls;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// The controller class for the ScheduledLocations
    /// </summary>
    public partial class ScheduledLocationsController : Rock.Rest.ApiController<Rock.Model.Category>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduledLocationsController"/> class.
        /// </summary>
        public ScheduledLocationsController() : base( new Rock.Model.CategoryService( new Rock.Data.RockContext() ) ) { }
    }

    /// <summary>
    /// The controller class for the ScheduledLocations
    /// </summary>
    public partial class ScheduledLocationsController
    {
        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rootLocationId">The root location identifier.</param>
        /// <param name="reservationId">The reservation identifier.</param>
        /// <param name="iCalendarContent">Content of the i calendar.</param>
        /// <param name="setupTime">The setup time.</param>
        /// <param name="cleanupTime">The cleanup time.</param>
        /// <param name="attendeeCount">The attendee count.</param>
        /// <param name="reservationTypeId">The reservation type id.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/com_bemaservices/ScheduledLocations/GetChildren/{id}/{rootLocationId}" )]
        public IQueryable<TreeViewItem> GetChildren(
            int id,
            int rootLocationId = 0,
            int? reservationId = null,
            string iCalendarContent = "",
            int? setupTime = null,
            int? cleanupTime = null,
            int? attendeeCount = null,
            int? reservationTypeId = null )
        {
            var rockContext = new RockContext();
            var locationService = new LocationService( rockContext );
            var reservationLocationTypeList = new List<int>();

            IQueryable<Location> qry;
            if ( id == 0 )
            {
                qry = locationService.Queryable().AsNoTracking().Where( a => a.ParentLocationId == null );
                if ( rootLocationId != 0 )
                {
                    qry = qry.Where( a => a.Id == rootLocationId );
                }
            }
            else
            {
                qry = locationService.Queryable().AsNoTracking().Where( a => a.ParentLocationId == id );
            }

            // limit to only active, Named Locations (don't show home addresses, etc)
            qry = qry.Where( a => a.Name != null && a.Name != string.Empty && a.IsActive == true );

            if ( reservationTypeId.HasValue )
            {
                var reservationType = new ReservationTypeService( rockContext ).Get( reservationTypeId.Value );
                reservationLocationTypeList = reservationType.ReservationLocationTypes.Select( rlt => rlt.LocationTypeValueId ).ToList();
            }

            List<Location> locationList = new List<Location>();
            List<TreeViewItem> locationNameList = new List<TreeViewItem>();

            var person = GetPerson();

            var newReservation = new Reservation() { Id = reservationId ?? 0, Schedule = ReservationService.BuildScheduleFromICalContent( iCalendarContent ), SetupTime = setupTime, CleanupTime = cleanupTime };

            var reservationService = new ReservationService( rockContext );
            List<int> reservedLocationIds = reservationService.GetReservedLocationIds( newReservation, false, false, false );

            foreach ( var location in qry.OrderBy( l => l.Name ) )
            {
                if ( location.IsAuthorized( Rock.Security.Authorization.VIEW, person ) )
                {
                    var descendantLocations = locationService.GetAllDescendents( location.Id );
                    bool isValidLocationType = ( !reservationLocationTypeList.Any() || !location.LocationTypeValueId.HasValue || reservationLocationTypeList.Contains( location.LocationTypeValueId.Value ) );
                    bool hasValidDescendant = ( !reservationLocationTypeList.Any() || descendantLocations.Any( dl => !dl.LocationTypeValueId.HasValue || reservationLocationTypeList.Contains( dl.LocationTypeValueId.Value ) ) );
                    if ( isValidLocationType || hasValidDescendant )
                    {
                        locationList.Add( location );
                        var treeViewItem = new TreeViewItem();
                        treeViewItem.Id = location.Id.ToString();
                        treeViewItem.Name = string.Format( "{0}<small style='color:grey;'>{1}{2}</small>",
                            System.Web.HttpUtility.HtmlEncode( location.Name ),
                            location.FirmRoomThreshold != null ? "\t(" + location.FirmRoomThreshold + ")" : "",
                             !isValidLocationType ? "\t(Non-reservable)" : "" );
                        treeViewItem.IsActive =
                            // location isnt' reserved
                            !( reservedLocationIds.Contains( location.Id ) )
                            // and the attendee count is less than or equal to the room's capacity
                            && ( attendeeCount == null || location.FirmRoomThreshold == null || attendeeCount.Value <= location.FirmRoomThreshold.Value );
                        locationNameList.Add( treeViewItem );
                    }
                }
            }

            // try to quickly figure out which items have Children
            List<int> resultIds = locationList.Select( a => a.Id ).ToList();

            var qryHasChildren = locationService.Queryable().AsNoTracking()
                .Where( l =>
                    l.ParentLocationId.HasValue &&
                    resultIds.Contains( l.ParentLocationId.Value ))
                .Select( l => l.ParentLocationId.Value )
                .Distinct()
                .ToList();

            var qryHasChildrenList = qryHasChildren.ToList();

            foreach ( var item in locationNameList )
            {
                int locationId = int.Parse( item.Id );
                item.HasChildren = qryHasChildrenList.Any( a => a == locationId );
            }

            return locationNameList.AsQueryable();
        }

    }
}

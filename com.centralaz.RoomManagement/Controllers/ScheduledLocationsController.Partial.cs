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
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using com.centralaz.RoomManagement.Model;

using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web.UI.Controls;

namespace Rock.Rest.Controllers
{
    public partial class ScheduledLocationsController : Rock.Rest.ApiController<Rock.Model.Category>
    {
        public ScheduledLocationsController() : base( new Rock.Model.CategoryService( new Rock.Data.RockContext() ) ) { }
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class ScheduledLocationsController
    {
        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rootCategoryId">The root category identifier.</param>
        /// <param name="getCategorizedItems">if set to <c>true</c> [get categorized items].</param>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityQualifier">The entity qualifier.</param>
        /// <param name="entityQualifierValue">The entity qualifier value.</param>
        /// <param name="showUnnamedEntityItems">if set to <c>true</c> [show unnamed entity items].</param>
        /// <param name="showCategoriesThatHaveNoChildren">if set to <c>true</c> [show categories that have no children].</param>
        /// <param name="includedCategoryIds">The included category ids.</param>
        /// <param name="excludedCategoryIds">The excluded category ids.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/com_centralaz/ScheduledLocations/GetChildren/{id}/{rootLocationId}" )]
        public IQueryable<TreeViewItem> GetChildren(
            int id,
            int rootLocationId = 0,
            int? reservationId = null,
            string iCalendarContent = "",
            int? setupTime = null,
            int? cleanupTime = null,
            int? attendeeCount = null )
        {
            var rockContext = new RockContext();
            var locationService = new LocationService( rockContext );

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

            List<Location> locationList = new List<Location>();
            List<TreeViewItem> locationNameList = new List<TreeViewItem>();

            var person = GetPerson();

            var newReservation = new Reservation() { Id = reservationId ?? 0, Schedule = new Schedule() { iCalendarContent = iCalendarContent }, SetupTime = setupTime, CleanupTime = cleanupTime };

            var reservationService = new ReservationService( rockContext );
            List<int> reservedLocationIds = reservationService.GetReservedLocationIds( newReservation );

            foreach ( var location in qry.OrderBy( l => l.Name ) )
            {
                if ( location.IsAuthorized( Rock.Security.Authorization.VIEW, person ) )
                {
                    var ancestorIds = locationService.GetAllAncestorIds( location.Id );
                    locationList.Add( location );
                    var treeViewItem = new TreeViewItem();
                    treeViewItem.Id = location.Id.ToString();
                    treeViewItem.Name = string.Format( "{0}<small style='color:grey;'>{1}</small>", System.Web.HttpUtility.HtmlEncode( location.Name ), location.FirmRoomThreshold != null ? "\t(" + location.FirmRoomThreshold + ")" : "" );
                    treeViewItem.IsActive = 
                        // location isnt' reserved or it's parent (or grandparent) isn't reserved
                        ! ( reservedLocationIds.Contains( location.Id ) || ( location.ParentLocationId.HasValue && reservedLocationIds.Contains( location.ParentLocationId.Value ) )
                            || location.ParentLocation != null && location.ParentLocation.ParentLocation != null && reservedLocationIds.Contains( location.ParentLocation.ParentLocationId.Value ) )
                        // and the attendee count is less than or equal to the room's capacity
                        && ( attendeeCount == null || location.FirmRoomThreshold == null || attendeeCount.Value <= location.FirmRoomThreshold.Value );
                    locationNameList.Add( treeViewItem );
                }
            }

            // try to quickly figure out which items have Children
            List<int> resultIds = locationList.Select( a => a.Id ).ToList();

            var qryHasChildren = locationService.Queryable().AsNoTracking()
                .Where( l =>
                    l.ParentLocationId.HasValue &&
                    resultIds.Contains( l.ParentLocationId.Value ) )
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

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
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;

namespace Rock.Model
{
    /// <summary>
    /// Data access/service class for <see cref="Rock.Model.GroupLocation"/> objects.
    /// </summary>
    public partial class GroupLocationService
    {
        /// <summary>
        /// Deletes the specified GroupLocation and sets GroupLocationHistorical.GroupLocationId to NULL.
        /// Will not delete the GroupLocation and return false if the GroupLocationHistorical.GroupLocationId fails to update.
        /// Will try to determine current person alias from HttpContext.
        /// Caller is responsible to save changes.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public override bool Delete( GroupLocation item )
        {
            // Remove the ID from GroupLocationHistorical before deleting
            var rockContext = this.Context as Rock.Data.RockContext;
            bool isNulled = new GroupLocationHistoricalService( rockContext ).SetGroupLocationIdToNullForGroupLocationId( item.Id );
            if (!isNulled)
            {
                return false;
            }

            // Delete the GroupLocation
            return base.Delete( item );
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.GroupLocation">GroupLocations</see> by their LocationId.
        /// </summary>
        /// <param name="locationId">A <see cref="System.Int32"/> representing the Id of a <see cref="Rock.Model.Location"/> to search by.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.GroupLocation"/> which are associated with the provided <see cref="Rock.Model.Location"/> </returns>
        public IQueryable<GroupLocation> GetByLocation( int locationId )
        {
            return Queryable().Where( g => g.LocationId == locationId );
        }

        /// <summary>
        /// Returns an enumerable collection of  active <see cref="Rock.Model.GroupLocation">GroupLocations</see> by their <see cref="Rock.Model.Location"/> Id
        /// </summary>
        /// <param name="locationId">A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Location"/> to search by.</param>
        /// <returns></returns>
        public IQueryable<GroupLocation> GetActiveByLocation( int locationId )
        {
            return Queryable()
                .Where( g =>
                    g.LocationId == locationId &&
                    g.Group.IsActive && !g.Group.IsArchived );
        }

        /// <summary>
        /// Returns an enumerable collection of  active <see cref="Rock.Model.GroupLocation">GroupLocations</see> by their <see cref="Rock.Model.Location"/> Ids
        /// </summary>
        /// <param name="locationIds">The location ids.</param>
        /// <returns></returns>
        public IQueryable<GroupLocation> GetActiveByLocations( List<int> locationIds )
        {
            return Queryable().Where( g =>
                    locationIds.Contains( g.LocationId ) &&
                    g.Group.IsActive && !g.Group.IsArchived );
        }

        /// <summary>
        /// Gets the 'IsMappedLocation' locations that are within any of the selected geofences
        /// </summary>
        /// <param name="geofences">The geofences.</param>
        /// <returns></returns>
        public IQueryable<GroupLocation> GetMappedLocationsByGeofences( List<DbGeography> geofences )
        {
            List<int> locationIds = new List<int>();

            foreach ( var geofence in geofences )
            {
                locationIds.AddRange( Queryable()
               .Where( l =>
                   l.IsMappedLocation &&
                   l.Location != null &&
                   l.Location.GeoPoint != null
                   && l.Location.GeoPoint.Intersects( geofence )
               )
               .Select( l => l.Id ) );
            }
            if ( locationIds.Count() < 10000 )
            {
                return Queryable().Where( l => locationIds.Contains( l.Id ) );
            }
            else
            {
                return Queryable()
                    .Where( l =>
                        l.IsMappedLocation &&
                        l.Location != null &&
                        l.Location.GeoPoint != null
                        && geofences.Any( f =>
                            l.Location.GeoPoint.Intersects( f )
                        )
                    );
            }
        }
    }

    /// <summary>
    /// Extension methods for GroupLocation
    /// </summary>
    public static class GroupLocationExtensions
    {
        /// <summary>
        /// Where the group is active.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public static IQueryable<GroupLocation> WhereHasActiveGroup( this IQueryable<GroupLocation> query )
        {
            return query.Where( gl => gl.Group.IsActive );
        }

        /// <summary>
        /// Where the location is active.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public static IQueryable<GroupLocation> WhereHasActiveLocation( this IQueryable<GroupLocation> query )
        {
            return query.Where( gl => gl.Location.IsActive );
        }

        /// <summary>
        /// Where the entities are active (deduced from the group and location both being active).
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public static IQueryable<GroupLocation> WhereDeducedIsActive( this IQueryable<GroupLocation> query )
        {
            return query
                .WhereHasActiveLocation()
                .WhereHasActiveGroup();
        }
    }
}

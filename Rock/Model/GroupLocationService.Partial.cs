// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
            return Queryable( "Schedules,Group.GroupType" )
                .Where( g =>
                    g.LocationId == locationId &&
                    g.Group.IsActive );

        }

        /// <summary>
        /// Returns an enumerable collection of  active <see cref="Rock.Model.GroupLocation">GroupLocations</see> by their <see cref="Rock.Model.Location"/> Ids
        /// </summary>
        /// <param name="locationIds">The location ids.</param>
        /// <returns></returns>
        public IQueryable<GroupLocation> GetActiveByLocations( List<int> locationIds )
        {
            return Queryable( "Schedules,Group.GroupType,Location" )
                .Where( g =>
                    locationIds.Contains(g.LocationId) &&
                    g.Group.IsActive );
        }

        /// <summary>
        /// Gets the 'IsMappedLocation' locations that are within and of the selected geofences
        /// </summary>
        /// <param name="geofences">The geofences.</param>
        /// <returns></returns>
        public IQueryable<GroupLocation> GetMappedLocationsByGeofences( List<DbGeography> geofences )
        {
            return Queryable()
                .Where( l =>
                    l.IsMappedLocation &&
                    l.Location != null &&
                    l.Location.GeoPoint != null &&
                    geofences.Any( f => l.Location.GeoPoint.Intersects( f ) )
                );
        }
    }
}

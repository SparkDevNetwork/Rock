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
using System.Data.Entity.Spatial;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a named location that is required by the rendering engine.
    /// This information will be cached by the engine.
    /// </summary>
    [Serializable]
    [DataContract]
    public class NamedLocationCache : ModelCache<NamedLocationCache, Rock.Model.Location>
    {
        #region Properties

        /// <inheritdoc cref="Rock.Model.Location.Name" />
        [DataMember]
        public string Name { get; private set; }

        /// <inheritdoc cref="Rock.Model.Location.CampusId" />
        [DataMember]
        public int? CampusId
        {
            get
            {
                var campuses = CampusCache.All();

                int? campusId = null;

                var loc = this;

                while ( !campusId.HasValue && loc != null )
                {
                    var campus = campuses.Where( c => c.LocationId != null && c.LocationId == loc.Id ).FirstOrDefault();
                    if ( campus != null )
                    {
                        campusId = campus.Id;
                    }
                    else
                    {
                        loc = loc.ParentLocation;
                    }
                }

                return campusId;
            }
        }

        /// <inheritdoc cref="Rock.Model.Location.ParentLocationId" />
        [DataMember]
        public int? ParentLocationId { get; private set; }

        /// <summary>
        /// Gets a collection of child location identifiers. This property
        /// will only return the immediate descendants of this location.
        /// </summary>
        /// <value>
        /// A collection of location identifiers.
        /// </value>
        [DataMember]
        public List<int> ChildLocationIds { get; private set; }

        /// <inheritdoc cref="Rock.Model.Location.IsActive" />
        [DataMember]
        public bool IsActive { get; private set; }

        /// <inheritdoc cref="Rock.Model.Location.FirmRoomThreshold"/>
        [DataMember]
        public int? FirmRoomThreshold { get; private set; }

        /// <inheritdoc cref="Rock.Model.Location.SoftRoomThreshold"/>
        [DataMember]
        public int? SoftRoomThreshold { get; private set; }

        /// <inheritdoc cref="Rock.Model.Location.PrinterDeviceId "/>
        [DataMember]
        public int? PrinterDeviceId { get; internal set; }

        /// <inheritdoc cref="Rock.Model.Location.GeoPoint "/>
        public DbGeography GeoPoint { get; private set; }

        /// <inheritdoc cref="Rock.Model.Location.GeoFence "/>
        public DbGeography GeoFence { get; private set; }

        /// <inheritdoc cref="Rock.Model.Location.ParentLocation" />
        public NamedLocationCache ParentLocation => this.ParentLocationId.HasValue ? NamedLocationCache.Get( ParentLocationId.Value ) : null;

        /// <summary>
        /// Gets a collection of cached child locations. This property will
        /// only return the immediate descendants of this location.
        /// </summary>
        /// <value>
        /// A collection of cached locations.
        /// </value>
        public List<NamedLocationCache> ChildLocations => ChildLocationIds.Select( Get ).Where( l => l != null ).ToList();

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Gets the CampusID associated with the Location from the location or from the location's parent path
        /// </summary>
        /// <returns></returns>
        public int? GetCampusIdForLocation()
        {
            var locationId = this.Id;

            var location = Get( locationId );
            int? campusId = location.CampusId;
            if ( campusId.HasValue )
            {
                return campusId;
            }

            // If location is not a campus, check the location's parent locations to see if any of them are a campus
            var campusLocations = new Dictionary<int, int>();
            CampusCache.All()
                .Where( c => c.LocationId.HasValue )
                .Select( c => new
                {
                    CampusId = c.Id,
                    LocationId = c.LocationId.Value
                } )
                .ToList()
                .ForEach( c => campusLocations.Add( c.CampusId, c.LocationId ) );

            foreach ( var parentLocationId in GetAllAncestorIds() )
            {
                campusId = campusLocations
                    .Where( c => c.Value == parentLocationId )
                    .Select( c => c.Key )
                    .FirstOrDefault();

                if ( campusId != 0 )
                {
                    return campusId;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets all ancestor ids.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<int> GetAllAncestorIds()
        {
            var ancestorIds = new List<int>();
            var parentLocation = this.ParentLocation;
            while ( parentLocation != null )
            {
                ancestorIds.Add( parentLocation.Id );
                parentLocation = parentLocation.ParentLocation;
            }

            return ancestorIds;
        }

        /// <summary>
        /// Gets all descendant location identifers. This includes direct child
        /// locations, grand-child locations, etc. It does not include itself.
        /// </summary>
        /// <returns>An enumeration of location identifiers.</returns>
        public IEnumerable<int> GetAllDescendantLocationIds()
        {
            var locationIds = new HashSet<int>();

            GetAllDescendantLocationIds( locationIds );

            return locationIds;
        }

        /// <summary>
        /// Gets all descendant location identifiers by adding them to the
        /// HashSet.
        /// </summary>
        /// <param name="locationIds">The set of location identifiers.</param>
        internal void GetAllDescendantLocationIds( HashSet<int> locationIds )
        {
            // Don't use ChildLocations property since it returns a List<>
            // which causes an unnecessary allocation.
            var childLocations = ChildLocationIds.Select( Get ).Where( l => l != null ).ToList();

            foreach ( var location in childLocations )
            {
                // Only descend if we haven't already added this location.
                if ( locationIds.Add( location.Id ) )
                {
                    location.GetAllDescendantLocationIds( locationIds );
                }
            }
        }

        /// <summary>
        /// Gets all descendant location identifiers by adding them to the
        /// HashSet.
        /// </summary>
        /// <param name="locationSet">The set of location identifiers.</param>
        internal void GetAllDescendantLocationIds( Dictionary<int, NamedLocationCache> locationSet )
        {
            // Don't use ChildLocations property since it returns a List<>
            // which causes an unnecessary allocation.
            var childLocations = ChildLocationIds.Select( Get ).Where( l => l != null ).ToList();

            foreach ( var location in childLocations )
            {
                // Only descend if we haven't already added this location.
                if ( !locationSet.ContainsKey( location.Id ) )
                {
                    locationSet.Add( location.Id, location );
                    location.GetAllDescendantLocationIds( locationSet );
                }
            }
        }

        /// <summary>
        /// The amount of time that this item will live in the cache before expiring. If null, then the
        /// default lifespan is used.
        /// </summary>
        public override TimeSpan? Lifespan
        {
            get
            {
                if ( Name.IsNullOrWhiteSpace() )
                {
                    // just in case this isn't a named location, expire after 10 minutes
                    return new TimeSpan( 0, 10, 0 );
                }

                return base.Lifespan;
            }
        }

        /// <summary>
        /// Not Supported on NamedLocationCache
        /// </summary>
        /// <returns></returns>
        public static new List<NamedLocationCache> All()
        {
            return All( null );
        }

        /// <summary>
        /// Not Supported on NamedLocationCache
        /// </summary>
        /// <returns></returns>
        public static new List<NamedLocationCache> All( RockContext rockContext )
        {
            // since there could be a very large number of Locations in the database,
            // and we really only want to support Named locations, don't support All()
            throw new NotSupportedException( "NameLocationCache does not support All()" );
        }

        /// <summary>
        /// Set's the cached objects properties from the model/entities properties.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            Rock.Model.Location location = entity as Rock.Model.Location;
            if ( location == null )
            {
                return;
            }

            this.Name = location.Name;
            this.ParentLocationId = location.ParentLocationId;
            this.IsActive = location.IsActive;
            this.FirmRoomThreshold = location.FirmRoomThreshold;
            this.SoftRoomThreshold = location.SoftRoomThreshold;
            this.PrinterDeviceId = location.PrinterDeviceId;
            this.GeoPoint = location.GeoPoint;
            this.GeoFence = location.GeoFence;
            this.ChildLocationIds = location.ChildLocations.Select( l => l.Id ).ToList();
        }

        /// <summary>
        /// returns <see cref="Name"/>
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion Public Methods
    }
}

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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class Location
    {
        #region Navigation Properties

        /// <summary>
        /// Gets or sets a value indicating whether this instance is a named location.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if this instance is a named location; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        [NotMapped]
        public virtual bool IsNamedLocation
        {
            get
            {
                return !string.IsNullOrWhiteSpace( Name );
            }

            private set
            {
            }
        }

        /// <summary>
        /// Gets the <see cref="System.Object"/> with the specified key.
        /// </summary>
        /// <value>
        /// The <see cref="System.Object"/>.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public override object this[object key]
        {
            get
            {
                string keyName = key.ToStringSafe();
                switch ( keyName )
                {
                    case "GeoPoint":
                        {
                            if ( GeoPoint != null && GeoPoint.Latitude.HasValue && GeoPoint.Longitude.HasValue )
                            {
                                return string.Format( "{0},{1}", GeoPoint.Latitude.Value, GeoPoint.Longitude.Value );
                            }

                            break;
                        }

                    case "GeoFence":
                        {
                            if ( GeoFence != null )
                            {
                                return GeoFence.Coordinates()
                                    .Select( c => c.Latitude.ToString() + "," + c.Longitude.ToString() )
                                    .ToList()
                                    .AsDelimited( "|" );
                            }

                            break;
                        }

                    default:
                        {
                            return base[key];
                        }
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the campus that is at this location, or one of this location's parent location
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private int? GetCampusId( RockContext rockContext )
        {
            var campuses = CampusCache.All( rockContext );

            int? campusId = null;
            Location loc = this;

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

        /// <summary>
        /// Gets the GeoFence coordinates.
        /// </summary>
        /// <value>
        /// The GeoFence coordinates.
        /// </value>
        [DataMember]
        public virtual List<Double[]> GeoFenceCoordinates
        {
            get
            {
                if ( GeoFence != null )
                {
                    return GeoFence.Coordinates()
                        .Where( c => c.Latitude.HasValue && c.Longitude.HasValue )
                        .Select( c => new Double[] { c.Latitude.Value, c.Longitude.Value } )
                        .ToList();
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the latitude ( use GeoPoint to set a latitude/longitude values ).
        /// </summary>
        /// <value>
        /// The latitude.
        /// </value>
        [DataMember]
        public virtual double? Latitude
        {
            get
            {
                return GeoPoint != null ? GeoPoint.Latitude : null;
            }
        }

        /// <summary>
        /// Gets the longitude ( use GeoPoint to set a latitude/longitude values ).
        /// </summary>
        /// <value>
        /// The longitude.
        /// </value>
        [DataMember]
        public virtual double? Longitude
        {
            get
            {
                return GeoPoint != null ? GeoPoint.Longitude : null;
            }
        }

        #endregion Navigation Properties

        #region Override IsValid

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public override bool IsValid
        {
            get
            {
                var result = base.IsValid;
                if ( result )
                {
                    // make sure it isn't getting saved with a recursive parent hierarchy
                    var parentIds = new List<int>();
                    parentIds.Add( this.Id );
                    var parent = this.ParentLocationId.HasValue ? ( this.ParentLocation ?? new LocationService( new RockContext() ).Get( this.ParentLocationId.Value ) ) : null;
                    while ( parent != null )
                    {
                        if ( parentIds.Contains( parent.Id ) )
                        {
                            this.ValidationResults.Add( new ValidationResult( "Parent Location cannot be a child of this Location (recursion)" ) );
                            return false;
                        }
                        else
                        {
                            parentIds.Add( parent.Id );
                            parent = parent.ParentLocation;
                        }
                    }
                }

                return result;
            }
        }

        #endregion Override IsValid

        #region Public Methods
        /// <summary>
        /// Sets the location's GeoPoint from a latitude and longitude.
        /// </summary>
        /// <param name="latitude">A <see cref="System.Double"/> representing the latitude for this location.</param>
        /// <param name="longitude">A <see cref="System.Double"/>representing the longitude for this location.</param>
        public bool SetLocationPointFromLatLong( double latitude, double longitude )
        {
            try
            {
                this.GeoPoint = DbGeography.FromText( string.Format( "POINT({0} {1})", longitude, latitude ) );
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Return this location as a string. Set preferName to true
        /// to return this location.Name (if it has one). Otherwise,
        /// it will try to show the Full Address first.
        /// </summary>
        /// <param name="preferName">if set to <c>true</c> [prefer name].</param>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public string ToString( bool preferName )
        {
            if ( preferName && this.Name.IsNotNullOrWhiteSpace() )
            {
                return this.Name;
            }

            return this.ToString();
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> containing the Location's address that represents this instance.
        /// If this location has a street address, that will be returned, otherwise the Name will be returned.
        /// Use <see cref="ToString(bool)"/> to prefer returning the location's name vs address
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> containing the Location's address that represents this instance.
        /// </returns>
        public override string ToString()
        {
            string fullAddress = GetFullStreetAddress();

            if ( fullAddress.IsNotNullOrWhiteSpace() )
            {
                /* 
                    02/05/2021 MDP 

                    Even if Location.Name has a value, return the Full Street Address
                    for ToString() if there is a full address. This way we don't change
                    the behavior of how this has worked before.

                    UIs, etc, that should be showing Location.Name should use Location.Name instead
                    of relying on ToString().
                 */

                return fullAddress;
            }

            if ( this.Name.IsNotNullOrWhiteSpace() )
            {
                return this.Name;
            }

            if ( this.GeoPoint != null )
            {
                return string.Format( "A point at {0}, {1}", this.GeoPoint.Latitude, this.GeoPoint.Longitude );
            }

            if ( this.GeoFence != null )
            {
                int pointCount = this.GeoFence.PointCount ?? 0;
                return string.Format( "An area with {0} points", pointCount > 0 ? pointCount - 1 : 0 );
            }

            // this would only happen if Location didn't have a Name, Address, GeoPoint or GoeFence
            return this.Name;
        }

        /// <summary>
        /// Returns true if the Location has one of the following: Street1, Street2, City. Otherwise returns false.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is minimum viable address]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsMinimumViableAddress()
        {
            if ( this.Street1.IsNullOrWhiteSpace() &&
                this.Street2.IsNullOrWhiteSpace() &&
                this.City.IsNullOrWhiteSpace() )
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets <seealso cref="DbGeography">GeoPoint</seealso> from the specified latitude and longitude
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <returns></returns>
        public static DbGeography GetGeoPoint( double latitude, double longitude )
        {
            return DbGeography.FromText( $"POINT({longitude} {latitude})" );
        }

        /// <summary>
        /// Gets the full street address.
        /// </summary>
        /// <returns></returns>
        public string GetFullStreetAddress()
        {
            if ( !IsMinimumViableAddress() )
            {
                return string.Empty;
            }

            string result = string.Format( "{0} {1} {2}, {3} {4}", this.Street1, this.Street2, this.City, this.State, this.PostalCode ).ReplaceWhileExists( "  ", " " );
            var countryValue = DefinedTypeCache.Get( new Guid( SystemGuid.DefinedType.LOCATION_COUNTRIES ) ).GetDefinedValueFromValue( this.Country );

            if ( countryValue != null )
            {
                string format = countryValue.GetAttributeValue( "AddressFormat" );
                if ( !string.IsNullOrWhiteSpace( format ) )
                {
                    var dict = this.ToDictionary();
                    dict["Country"] = countryValue.Description;
                    result = format.ResolveMergeFields( dict );
                }
            }

            // Remove blank lines
            while ( result.Contains( Environment.NewLine + Environment.NewLine ) )
            {
                result = result.Replace( Environment.NewLine + Environment.NewLine, Environment.NewLine );
            }

            while ( result.Contains( "\x0A\x0A" ) )
            {
                result = result.Replace( "\x0A\x0A", "\x0A" );
            }

            if ( string.IsNullOrWhiteSpace( result.Replace( ",", string.Empty ) ) )
            {
                return string.Empty;
            }

            return result;
        }

        /// <summary>
        /// Encodes the polygon for Google maps
        /// from http://stackoverflow.com/a/3852420
        /// </summary>
        /// <returns></returns>
        public virtual string EncodeGooglePolygon()
        {
            var str = new StringBuilder();

            if ( this.GeoFence != null )
            {
                var encodeDiff = ( Action<int> ) ( diff =>
                   {
                       int shifted = diff << 1;
                       if ( diff < 0 )
                       {
                           shifted = ~shifted;
                       }

                       int rem = shifted;
                       while ( rem >= 0x20 )
                       {
                           str.Append( ( char ) ( ( 0x20 | ( rem & 0x1f ) ) + 63 ) );
                           rem >>= 5;
                       }

                       str.Append( ( char ) ( rem + 63 ) );
                   } );

                int lastLat = 0;
                int lastLng = 0;

                foreach ( var coordinate in this.GeoFence.Coordinates() )
                {
                    if ( coordinate.Longitude.HasValue && coordinate.Latitude.HasValue )
                    {
                        int lat = ( int ) Math.Round( coordinate.Latitude.Value * 1E5 );
                        int lng = ( int ) Math.Round( coordinate.Longitude.Value * 1E5 );
                        encodeDiff( lat - lastLat );
                        encodeDiff( lng - lastLng );
                        lastLat = lat;
                        lastLng = lng;
                    }
                }
            }

            return str.ToString();
        }
        #endregion Public Methods

        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            if ( this.Name.IsNotNullOrWhiteSpace() )
            {
                return NamedLocationCache.Get( this.Id );
            }

            return null;
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            // Make sure CampusCache.All is cached using the dbContext (to avoid deadlock if snapshot isolation is disabled)
            var campusId = this.GetCampusId( dbContext as RockContext );

            NamedLocationCache.FlushItem( this.Id );

            // CampusCache has a CampusLocation that could get stale when Location changes, so refresh the CampusCache for this location's Campus
            if ( this.CampusId.HasValue )
            {
                CampusCache.UpdateCachedEntity( this.CampusId.Value, EntityState.Detached );
            }

            // and also refresh the CampusCache for any Campus that uses this location
            foreach ( var campus in CampusCache.All( dbContext as RockContext ).Where( c => c.LocationId == this.Id ) )
            {
                CampusCache.UpdateCachedEntity( campus.Id, EntityState.Detached );
            }
        }

        #endregion ICacheable

        #region ISecured

        /// <summary>
        /// Gets the parent authority for the location. Location security is automatically inherited from the parent location,
        /// unless explicitly overridden.  If there is no parent location, it is inherited from the EntityType
        /// </summary>
        /// <value>
        /// The parent authority.
        /// </value>
        public override Security.ISecured ParentAuthority
        {
            get
            {
                return this.IsNamedLocation ? ( this.ParentLocation ?? NamedLocationCache.Get( this.ParentLocationId ?? 0 ) ?? base.ParentAuthority ) : base.ParentAuthority;
            }
        }

        #endregion
    }
}

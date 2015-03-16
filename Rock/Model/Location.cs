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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Location Entity class. A location in Rock is any physical place. It could be a street address, building, floor, room, kiosk location, etc. A location 
    /// is also stackable/hierarchical. For example for a church's campus <seealso cref="Campus"/> can have multiple buildings or facilities, 
    /// each building can be multi story and a story can have multiple rooms.
    /// </summary>
    [Table( "Location" )]
    [DataContract]
    public partial class Location : Model<Location>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the if the location's parent Location. 
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32 "/> representing the Id of this Location's parent Location. If this Location does not have a parent Location, this value will be null.
        /// </value>
        [DataMember]
        public int? ParentLocationId { get; set; }

        /// <summary>
        /// Gets or sets the Location's Name.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Name of the Location.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is  <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; }
        }
        private bool _isActive = true;

        /// <summary>
        /// Gets or sets the Id of the LocationType <see cref="Rock.Model.DefinedValue"/> that is used to identify the type of <see cref="Rock.Model.Location"/>
        /// that this is.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> referencing the Id of the LocationType <see cref="Rock.Model.DefinedValue"/> that identifies the type of group location that this is.
        /// If a LocationType <see cref="Rock.Model.DefinedValue"/> is not associated with this GroupLocation this value will be null.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.LOCATION_TYPE )]
        public int? LocationTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the GeoPoint (geolocation) for the location
        /// </summary>
        /// <value>
        /// A <see cref="System.Data.Entity.Spatial.DbGeography"/> object that represents the geolocation of the Location.
        /// </value>
        [DataMember]
        [Newtonsoft.Json.JsonConverter( typeof( DbGeographyConverter ) )]
        public DbGeography GeoPoint { get; set; }

        /// <summary>
        /// Gets or sets the geographic parameter around the a Location's Geopoint. This can also be used to define a large area
        /// like a neighborhood.  
        /// </summary>
        /// <remarks>
        /// Examples of this could be  a radius around a church campus to allow mobile check in if a person is located within a certain radius of 
        /// the campus, or it could be used to define the parameter of an area (i.e. neighborhood, park, etc.)
        /// </remarks>
        /// <value>
        /// A <see cref="System.Data.Entity.Spatial.DbGeography"/> object representing the parameter of a location.
        /// </value>
        [DataMember]
        [Newtonsoft.Json.JsonConverter( typeof( DbGeographyConverter ) )]
        public DbGeography GeoFence { get; set; }

        /// <summary>
        /// Gets or sets the first line of the Location's Street/Mailing Address.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the First line of the Location's Street/Mailing Address. If the Location does not have
        /// a Street/Mailing address, this value is null.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string Street1 { get; set; }

        /// <summary>
        /// Gets or sets the second line of the Location's Street/Mailing Address. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the second line of the Location's Street/Mailing Address. if this Location does not have 
        /// Street/Mailing Address or if the address does not have a 2nd line, this value is null.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string Street2 { get; set; }

        /// <summary>
        /// Gets or sets the city component of the Location's Street/Mailing Address.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the city component of the Location's Street/Mailing Address. If this Location does not have
        /// a Street/Mailing Address this value will be null.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the State component of the Location's Street/Mailing Address.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the state component of the Location's Street/Mailing Address. If this Location does not have 
        /// a Street/Mailing Address, this value will be null.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the country component of the Location's Street/Mailing Address. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> representing the country component of the Location's Street/Mailing Address. If this Location does not have a 
        /// Street/Mailing Address, this value will be null.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets the Zip/Postal Code component of the Location's Street/Mailing Address.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Zip/Postal Code component of the Location's Street/Mailing Address. If this Location does not have 
        /// Street/Mailing Address, this value will be null.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string PostalCode { get; set; }

        /// <summary>
        /// Gets or sets the Local Assessor's parcel identification value that is linked to the location.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> represents the local assessor's parcel Id for the location (if applicable). If this is not applicable to this location,
        /// the value will be null.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string AssessorParcelId { get; set; }

        /// <summary>
        /// Gets or sets the date and time of the last address standardization attempt.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the when the most recent address standardization attempt was made. If this is not applicable to this location,
        /// or if the address has not been standardized, this value will be null.
        /// </value>
        [DataMember]
        public DateTime? StandardizeAttemptedDateTime { get; set; }

        /// <summary>
        /// Gets or set the component name of the service that attempted the most recent address standardization attempt.
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> representing the component name of the service that last attempted to standardize this Location's address.
        /// If this is not applicable to the location or a standardization attempt has not been made, this value will be null.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string StandardizeAttemptedServiceType { get; set; }

        /// <summary>
        /// Gets or sets the result code returned from the address standardization service.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the result code that was returned by the address standardization service. If an address standardization has not been attempted for this location, 
        /// this value will be null.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string StandardizeAttemptedResult { get; set; }

        /// <summary>
        /// Gets or sets the date and time that the Location's address was successfully standardized.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date and time that the Location's address was successfully standardized. If address standardization has not been attempted for this location,
        /// This value will be null.
        /// </value>
        [DataMember]
        public DateTime? StandardizedDateTime { get; set; }

        /// <summary>
        /// Gets and sets the date and time that an attempt was made to geocode the Location's address.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime" /> representing the date and time that an attempt was made to geocode the Location's address. If a geocoding has not been attempted for this location, 
        /// the value will be null.
        /// </value>
        [DataMember]
        public DateTime? GeocodeAttemptedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the component name of the Geocoding service that attempted the most recent address Geocode attempt.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the component name of the Geocoding service that attempted the most recent address Geocode attempt. If geocoding has not been attempted 
        /// for this location, the value will be null.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string GeocodeAttemptedServiceType { get; set; }

        /// <summary>
        /// Gets or sets the result code returned by geocoding service during the last geocode attempt.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the result code returned by the geocoding service from the most recent geocoding attempt. If geocoding has not been attempted for this location,
        /// the value will be null.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string GeocodeAttemptedResult { get; set; }

        /// <summary>
        /// Gets or sets date and time that this Location's  address has been successfully geocoded. 
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date and time that the address of this location was successfully geocoded. If geocoding has not been attepted for this location or 
        /// the location had not been successfully geocoded this value will be null.
        /// </value>
        [DataMember]
        public DateTime? GeocodedDateTime { get; set; }

        /// <summary>
        /// Gets or sets flag indicating if geopoint is locked (shouldn't be geocoded again)
        /// </summary>
        /// <value>
        /// is geo point locked.
        /// </value>
        [DataMember]
        public bool? IsGeoPointLocked { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Device"/> Id of the printer (if any) associated with the location.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Device"/> Id of the printer that is associated with this Location. If no printer is associated with this location, this value will be null.
        /// </value>
        [DataMember]
        public int? PrinterDeviceId { get; set; }

        /// <summary>
        /// Gets or sets the image identifier.
        /// </summary>
        /// <value>
        /// The image identifier.
        /// </value>
        [DataMember]
        public int? ImageId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or set this Location's parent Location.
        /// </summary>
        /// <value>
        /// A Location object representing the parent location of the current location. If this Location does not have a parent Location, this value will be null.
        /// </value>
        public virtual Location ParentLocation { get; set; }

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
            private set { }
        }

        /// <summary>
        /// Gets or sets the location type value.
        /// </summary>
        /// <value>
        /// The location type value.
        /// </value>
        [DataMember]
        public virtual DefinedValue LocationTypeValue { get; set; }

        /// <summary>
        /// Gets or sets a collection of child Locations associated that inherit for this location. This property will only return the immediate descendants of this locations.
        /// </summary>
        /// <value>
        /// A collection of the child Locations that are immediate descendants of this Location.  If this Location does not have any descendants, this value will be null.
        /// </value>
        [DataMember]
        public virtual ICollection<Location> ChildLocations
        {
            get { return _childLocations ?? ( _childLocations = new Collection<Location>() ); }
            set { _childLocations = value; }
        }
        private ICollection<Location> _childLocations;

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.GroupLocation">GroupLocations</see> that reference this Location.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.GroupLocation"/> entities that reference this Location.
        /// </value>
        public virtual ICollection<GroupLocation> GroupLocations
        {
            get { return _groupLocations ?? ( _groupLocations = new Collection<GroupLocation>() ); }
            set { _groupLocations = value; }
        }
        private ICollection<GroupLocation> _groupLocations;

        /// <summary>
        /// Gets or sets the Attendance Printer <see cref="Rock.Model.Device"/> that is used at this Location.
        /// </summary>
        /// <value>
        /// The attendance printer that is used at this Location.
        /// </value>
        [DataMember]
        public virtual Device PrinterDevice { get; set; }

        /// <summary>
        /// Gets or sets the image.
        /// </summary>
        /// <value>
        /// The image.
        /// </value>
        [DataMember]
        public virtual BinaryFile Image { get; set; }

        /// <summary>
        /// Gets the formatted address.
        /// </summary>
        /// <value>
        /// The formatted address.
        /// </value>
        [LavaInclude]
        public virtual string FormattedAddress
        {
            get { return GetFullStreetAddress(); }
        }

        /// <summary>
        /// Gets the formatted HTML address.
        /// </summary>
        /// <value>
        /// The formatted HTML address.
        /// </value>
        [LavaInclude]
        public virtual string FormattedHtmlAddress
        {
            get { return FormattedAddress.ConvertCrLfToHtmlBr(); }
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

        /// <summary>
        /// Gets the GeoFence coordinates.
        /// </summary>
        /// <value>
        /// The geo fence coordinates.
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
        /// Gets the campus that is at this location, or one of this location's parent location
        /// </summary>
        /// <value>
        /// The campus identifier.
        /// </value>
        [LavaInclude]
        public virtual int? CampusId
        {
            get
            {
                var campuses = CampusCache.All();

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
        }

        /// <summary>
        /// Gets the distance.
        /// </summary>
        /// <value>
        /// The distance.
        /// </value>
        [DataMember]
        public virtual double Distance
        {
            get { return _distance; }
        }
        private double _distance = 0.0D;

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the location's GeoPoint from a latitude and longitude.
        /// </summary>
        /// <param name="latitude">A <see cref="System.Double"/> representing the latitude for this location.</param>
        /// <param name="longitude">A <see cref="System.Double"/>representing the longitude for this location.</param>
        public void SetLocationPointFromLatLong( double latitude, double longitude )
        {
            this.GeoPoint = DbGeography.FromText( string.Format( "POINT({0} {1})", longitude, latitude ) );
        }

        /// <summary>
        /// Returns a Google Maps link to use for this Location
        /// </summary>
        /// <param name="title">A <see cref="System.String"/> containing the parameters needed by Google Maps to display this location.</param>
        /// <returns>A <see cref="System.String"/> containing the link to Google Maps for this location.</returns>
        public virtual string GoogleMapLink( string title )
        {
            string qParm = this.ToString();
            if ( !string.IsNullOrWhiteSpace( title ) )
            {
                qParm += " (" + title + ")";
            }

            return "http://maps.google.com/maps?q=" +
                System.Web.HttpUtility.UrlEncode( qParm );
        }

        /// <summary>
        /// Pres the save changes.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="state">The state.</param>
        public override void PreSaveChanges( Rock.Data.DbContext dbContext, System.Data.Entity.EntityState state )
        {
            if ( ImageId.HasValue )
            {
                BinaryFileService binaryFileService = new BinaryFileService( (RockContext)dbContext );
                var binaryFile = binaryFileService.Get( ImageId.Value );
                if ( binaryFile != null && binaryFile.IsTemporary )
                {
                    binaryFile.IsTemporary = false;
                }
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> containing the Location's address that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> containing the Location's address that represents this instance.
        /// </returns>
        public override string ToString()
        {
            string result = this.Name;

            if ( string.IsNullOrEmpty( result ) )
            {
                result = GetFullStreetAddress();
            }

            if ( string.IsNullOrWhiteSpace( result ) )
            {
                if ( this.GeoPoint != null )
                {
                    return string.Format( "A point at {0}, {1}", this.GeoPoint.Latitude, this.GeoPoint.Longitude );
                }

                if ( this.GeoFence != null )
                {
                    int pointCount = this.GeoFence.PointCount ?? 0;
                    return string.Format( "An area with {0} points", ( pointCount > 0 ? pointCount - 1 : 0 ) );
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the full street address.
        /// </summary>
        /// <returns></returns>
        public string GetFullStreetAddress()
        {
            if (string.IsNullOrWhiteSpace(this.Street1) &&
                string.IsNullOrWhiteSpace(this.Street2) &&
                string.IsNullOrWhiteSpace(this.City))
            {
                return string.Empty;
            }

            string result = string.Format( "{0} {1} {2}, {3} {4}",
                this.Street1, this.Street2, this.City, this.State, this.PostalCode ).ReplaceWhileExists( "  ", " " );

            var countryValue = Rock.Web.Cache.DefinedTypeCache.Read( new Guid( SystemGuid.DefinedType.LOCATION_COUNTRIES ) )
                .DefinedValues
                .Where( v => v.Value.Equals( this.Country, StringComparison.OrdinalIgnoreCase ) )
                .FirstOrDefault();
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
            while (result.Contains( Environment.NewLine + Environment.NewLine))
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
        /// </summary>
        /// <returns></returns>
        public virtual string EncodeGooglePolygon()
        {
            var str = new StringBuilder();

            if ( this.GeoFence != null )
            {
                var encodeDiff = (Action<int>)( diff =>
                {
                    int shifted = diff << 1;
                    if ( diff < 0 )
                        shifted = ~shifted;
                    int rem = shifted;
                    while ( rem >= 0x20 )
                    {
                        str.Append( (char)( ( 0x20 | ( rem & 0x1f ) ) + 63 ) );
                        rem >>= 5;
                    }
                    str.Append( (char)( rem + 63 ) );
                } );

                int lastLat = 0;
                int lastLng = 0;

                // AsText() returns coordinates as Well-Known-Text format (WKT).  Strip leading and closing text
                string coordinates = this.GeoFence.AsText().Replace( "POLYGON ((", "" ).Replace( "))", "" );
                string[] longSpaceLat = coordinates.Split( ',' );

                for ( int i = 0; i < longSpaceLat.Length; i++ )
                {
                    string[] longLat = longSpaceLat[i].Trim().Split( ' ' );
                    int lat = (int)Math.Round( double.Parse( longLat[1] ) * 1E5 );
                    int lng = (int)Math.Round( double.Parse( longLat[0] ) * 1E5 );
                    encodeDiff( lat - lastLat );
                    encodeDiff( lng - lastLng );
                    lastLat = lat;
                    lastLng = lng;
                }
            }

            return str.ToString();
        }

        /// <summary>
        /// Sets the distance.
        /// </summary>
        /// <param name="distance">The distance.</param>
        public void SetDistance( double distance )
        {
            _distance = distance;
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
                                    .AsDelimited("|");
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
        
        #endregion

        #region constants

        /// <summary>
        /// Meters per mile (1609.344)
        /// NOTE: Geo Spatial distances are in meters
        /// </summary>
        public const double MetersPerMile = 1609.344;


        /// <summary>
        /// Miles per meter 1/1609.344 (0.00062137505)
        /// NOTE: Geo Spatial distances are in meters
        /// </summary>
        public const double MilesPerMeter = 1 / MetersPerMile;

        #endregion


    }

    #region Entity Configuration

    /// <summary>
    /// Location Configuration class.
    /// </summary>
    public partial class LocationConfiguration : EntityTypeConfiguration<Location>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LocationConfiguration"/> class.
        /// </summary>
        public LocationConfiguration()
        {
            this.HasOptional( l => l.ParentLocation ).WithMany( l => l.ChildLocations ).HasForeignKey( l => l.ParentLocationId ).WillCascadeOnDelete( false );
            this.HasOptional( l => l.PrinterDevice ).WithMany().HasForeignKey( l => l.PrinterDeviceId ).WillCascadeOnDelete( false );
            this.HasOptional( l => l.LocationTypeValue ).WithMany().HasForeignKey( l => l.LocationTypeValueId ).WillCascadeOnDelete( false );
            this.HasOptional( l => l.Image ).WithMany().HasForeignKey( p => p.ImageId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

    #region Map Helper Classes

    /// <summary>
    /// Helper class to store map coordinates
    /// </summary>
    public class MapCoordinate
    {
        /// <summary>
        /// Gets or sets the latitude.
        /// </summary>
        /// <value>
        /// The latitude.
        /// </value>
        public double? Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude.
        /// </summary>
        /// <value>
        /// The longitude.
        /// </value>
        public double? Longitude { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MapCoordinate"/> class.
        /// </summary>
        public MapCoordinate()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MapCoordinate"/> class.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        public MapCoordinate( double? latitude, double? longitude )
        {
            Latitude = latitude;
            Longitude = longitude;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class MapItem
    {
        /// <summary>
        /// Gets or sets the entity type identifier.
        /// </summary>
        /// <value>
        /// The entity type identifier.
        /// </value>
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        /// <value>
        /// The entity identifier.
        /// </value>
        public int EntityId { get; set; }

        /// <summary>
        /// Gets or sets the location identifier.
        /// </summary>
        /// <value>
        /// The location identifier.
        /// </value>
        public int LocationId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the point.
        /// </summary>
        /// <value>
        /// The point.
        /// </value>
        public MapCoordinate Point { get; set; }

        /// <summary>
        /// Gets or sets the polygon points.
        /// </summary>
        /// <value>
        /// The polygon points.
        /// </value>
        public List<MapCoordinate> PolygonPoints { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MapItem"/> class.
        /// </summary>
        public MapItem()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MapItem"/> class.
        /// </summary>
        /// <param name="location">The location.</param>
        public MapItem( Location location )
        {
            PolygonPoints = new List<MapCoordinate>();

            if ( location != null )
            {
                LocationId = location.Id;
                if ( location.GeoPoint != null )
                {
                    Point = new MapCoordinate( location.GeoPoint.Latitude, location.GeoPoint.Longitude );
                }

                if ( location.GeoFence != null )
                {
                    PolygonPoints = location.GeoFence.Coordinates();
                }
            }
        }
    }


    #endregion
}

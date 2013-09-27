//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// CheckInDevice EF Model.
    /// </summary>
    [Table("Device")]
    [DataContract]
    public partial class Device : Model<Device>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the device name.
        /// </summary>
        /// <value>
        /// File Name.
        /// </value>
        [Required]
        [AlternateKey]
        [MaxLength( 50 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the Description.
        /// </summary>
        /// <value>
        /// Description.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the type of the device.
        /// </summary>
        /// <value>
        /// The type of the device.
        /// </value>
        [DataMember]
        public int DeviceTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the location id.
        /// </summary>
        /// <value>
        /// The location id.
        /// </value>
        [DataMember]
        public int? LocationId { get; set; }

        /// <summary>
        /// Gets or sets the IP address.
        /// </summary>
        /// <value>
        /// The IP address.
        /// </value>
        [MaxLength(45)]
        [DataMember]
        public string IPAddress { get; set; }

        /// <summary>
        /// Gets or sets the printer id.
        /// </summary>
        /// <value>
        /// The printer id.
        /// </value>
        [DataMember]
        public int? PrinterDeviceId { get; set; }

        /// <summary>
        /// Gets or sets the print from.
        /// </summary>
        /// <value>
        /// The print from.
        /// </value>
        [DataMember]
        public PrintFrom PrintFrom { get; set; }

        /// <summary>
        /// Gets or sets the print to override.
        /// </summary>
        /// <value>
        /// The print to override.
        /// </value>
        [DataMember]
        public PrintTo PrintToOverride { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the physical location of the device, or the geographic fence 
        /// that this device is active for
        /// </summary>
        /// <value>
        /// The location.
        /// </value>
        public virtual Location Location { get; set; }

        /// <summary>
        /// Gets or sets the locations that this device is used for
        /// </summary>
        /// <value>
        /// The locations using this device.
        /// </value>
        [DataMember]
        public virtual ICollection<Location> Locations
        {
            get { return _locations ?? ( _locations = new Collection<Location>() ); }
            set { _locations = value; }
        }
        private ICollection<Location> _locations;

        /// <summary>
        /// Gets or sets the printer.
        /// </summary>
        /// <value>
        /// The printer.
        /// </value>
        public virtual Device PrinterDevice { get; set; }

        /// <summary>
        /// Gets or sets the type of the device.
        /// </summary>
        /// <value>
        /// The type of the device.
        /// </value>
        [DataMember]
        public virtual DefinedValue DeviceType { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the group types that are configured for the locations that this device is
        /// configured for.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<GroupType> GetLocationGroupTypes()
        {
            var groupTypes = new Dictionary<int, GroupType>();
            foreach ( var groupLocations in this.Locations
                .Select( l => l.GroupLocations ) )
            {
                foreach(var groupType in groupLocations.Select( gl => gl.Group.GroupType))
                {
                    if (!groupTypes.ContainsKey(groupType.Id))
                    {
                        groupTypes.Add(groupType.Id, groupType);
                    }
                }
            }

            return groupTypes.Select( g => g.Value );
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion

    }

    #region Entity Configuration
    
    /// <summary>
    /// File Configuration class.
    /// </summary>
    public partial class DeviceConfiguration : EntityTypeConfiguration<Device>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceConfiguration"/> class.
        /// </summary>
        public DeviceConfiguration()
        {
            this.HasOptional( d => d.Location ).WithMany().HasForeignKey( d => d.LocationId ).WillCascadeOnDelete( false );
            this.HasMany( d => d.Locations ).WithMany().Map( d => { d.MapLeftKey( "DeviceId" ); d.MapRightKey( "LocationId" ); d.ToTable( "DeviceLocation" ); } );
            this.HasOptional( d => d.PrinterDevice ).WithMany().HasForeignKey( d => d.PrinterDeviceId ).WillCascadeOnDelete( false );
            this.HasRequired( d => d.DeviceType ).WithMany().HasForeignKey( d => d.DeviceTypeValueId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

    #region Enumerations

    /// <summary>
    /// Where a label should be printed
    /// </summary>
    public enum PrintTo
    {
        /// <summary>
        /// Print to the default printer
        /// </summary>
        Default = 0,

        /// <summary>
        /// Print to the printer associated with the selected kiosk
        /// </summary>
        Kiosk = 1,

        /// <summary>
        /// Print to the printer associated with the selected location
        /// </summary>
        Location = 2
    }

    /// <summary>
    /// The application responsible for printing a label
    /// </summary>
    public enum PrintFrom
    {
        /// <summary>
        /// The kiosk will print the label
        /// </summary>
        Client = 0,

        /// <summary>
        /// The server 
        /// </summary>
        Server = 1
    }

    #endregion

    # region Custom JSON Converter
    /// The JSON serializer normally would create the following...
    /// 
    /// {
    ///  ...
    ///  "GeoPoint": {
    ///    "Geography": {
    ///      "CoordinateSystemId": 4326,
    ///      "WellKnownText": "POINT (-112.20884 33.7106)"
    ///    }
    ///  },
    ///  "GeoFence": null,
    ///  ...
    ///}
    ///
    /// Which almost works except for the null GeoFence output, however the 
    /// JsonConvert.DerserializeObject() fails with:
    /// 
    ///    Unable to find a constructor to use for type System.Data.Spatial.DbGeography. 
    ///    A class should either have a default constructor, one constructor with arguments
    ///    or a constructor marked with the JsonConstructor attribute. Path 'GeoPoint.Geography',
    ///    line 5, position 17.
    /// 
    public class DbGeographyConverter : JsonConverter
    {
        private const string LATITUDE_KEY = "latitude";
        private const string LONGITUDE_KEY = "longitude";

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        ///   <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert( Type objectType )
        {
            return objectType.Equals( typeof( DbGeography ) );
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>
        /// The object value.
        /// </returns>
        public override object ReadJson( JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer )
        {
            if ( reader.TokenType == JsonToken.Null )
            {
                return default( DbGeography );
            }

            var jObject = JObject.Load( reader );

            if ( !jObject.HasValues || ( jObject.Property( LATITUDE_KEY ) == null || jObject.Property( LONGITUDE_KEY ) == null ) )
            {
                return default( DbGeography );
            }

            string wkt = string.Format( "POINT({0} {1})", jObject[LONGITUDE_KEY], jObject[LATITUDE_KEY] );  // note: long, lat
            return DbGeography.FromText( wkt, DbGeography.DefaultCoordinateSystemId );
        }

        /// <summary>
        /// This serializer produces the following (which is slightly different than the default one):
        /// 
        ///{
        /// ...
        ///  "GeoPoint": {
        ///    "latitude": 33.7106,
        ///    "longitude": -112.20884
        ///  },
        ///  "GeoFence": null,
        ///  ...
        ///}
        /// 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer )
        {
            var dbGeography = value as DbGeography;

            serializer.Serialize( writer, dbGeography == null
                || dbGeography.IsEmpty ? null 
                : new { latitude = dbGeography.Latitude.Value, longitude = dbGeography.Longitude.Value } );
        }
    }
    #endregion

}

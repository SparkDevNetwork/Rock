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
    /// Represents a device or component that interacts with and is manageable through RockChMS.  Examples of these can be check-in kiosks, giving kiosks, label printers, badge printers,
    /// displays, etc.
    /// </summary>
    [Table("Device")]
    [DataContract]
    public partial class Device : Model<Device>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the device name. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> representing the Name of the device.
        /// </value>
        [Required]
        [AlternateKey]
        [MaxLength( 50 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets a description of the device.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the description of the device.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the Id of the DeviceType <see cref="Rock.Model.DefinedValue"/> that identifies
        /// what type of device this is.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the Device Type <see cref="Rock.Model.DefinedValue"/>
        /// </value>
        [DataMember]
        public int DeviceTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Location"/> where this device is located at.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Location"/> where this device is located at.
        /// </value>
        [DataMember]
        public int? LocationId { get; set; }

        /// <summary>
        /// Gets or sets the IP address of the device.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the IP address of the device.
        /// </value>
        [MaxLength(45)]
        [DataMember]
        public string IPAddress { get; set; }

        /// <summary>
        /// Gets or sets the DeviceId of the printer that is associated with this device. This is mostly used if this device is a kiosk.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the DeviceId of the printer that is associated with this device. If there is not a printer 
        /// associated with this Device, this value will be null.
        /// </value>
        [DataMember]
        public int? PrinterDeviceId { get; set; }

        /// <summary>
        /// Gets or sets where print jobs for this device originates from.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.PrintFrom"/> to indicate how print jobs should be handled from this device. If <c>PrintFrom.Client</c> the print job will
        /// be handled from the client, otherwise <c>PrintFrom.Server</c> and the print job will be handled from the server.
        /// </value>
        [DataMember]
        public PrintFrom PrintFrom { get; set; }

        /// <summary>
        /// Gets or sets a flag that overrides which printer the print job is set to.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.PrintTo"/> that indicates overrides where the print job is set to.  If <c>PrintTo.Default</c> the print job will be sent to the default
        /// printer, if <c>PrintTo.Kiosk</c> the print job will be sent to the printer associated with the kiosk, if <c>PrintTo.Location</c> the print job will be sent to the 
        /// printer at the check in location.
        /// </value>
        [DataMember]
        public PrintTo PrintToOverride { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the physical location or geographic fence for the device.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.Location"/> entity that represents the physical location of or the geographic fence for the device.
        /// </value>
        /// <remarks>
        /// A physical location would signify where the device is at. A situation where a geographic fence could be used would be for mobile checkin, 
        /// where if the device is within the fence, a user would be able to check in from their mobile device.
        /// </remarks>
        public virtual Location Location { get; set; }


        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.Locaton">Locations</see> that use this device.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.Location">Locations</see> that use this device.
        /// </value>
        [DataMember]
        public virtual ICollection<Location> Locations
        {
            get { return _locations ?? ( _locations = new Collection<Location>() ); }
            set { _locations = value; }
        }
        private ICollection<Location> _locations;

        /// <summary>
        /// Gets or sets the printer that is associated with this device. 
        /// </summary>
        /// <value>
        /// The printer that is associated with the device.
        /// </value>
        public virtual Device PrinterDevice { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.DefinedValue"/> that represents the type of the device.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.DefinedValue"/> that represents the type of the device.
        /// </value>
        [DataMember]
        public virtual DefinedValue DeviceType { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.GroupType">GroupTypes</see> that use the <see cref="Rock.Model.Location">Locations</see> that this
        /// device is configured for.
        /// </summary>
        /// <returns>A enumerable collection of <see cref="Rock.Model.GroupType"/> entities that use the <see cref="Rock.Model.Location">Locations that this device is configured for.</returns>
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
        /// The label will be printed by the kiosk
        /// </summary>
        Client = 0,

        /// <summary>
        /// The label will be printed by the server.
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

        public override bool CanConvert( Type objectType )
        {
            return objectType.Equals( typeof( DbGeography ) );
        }

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

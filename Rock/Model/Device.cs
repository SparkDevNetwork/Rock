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
using System.Data.Spatial;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// CheckInDevice EF Model.
    /// </summary>
    [Table("Device")]
    [DataContract( IsReference = true )]
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
        /// Gets or sets the geographic point where the device is located.
        /// </summary>
        /// <value>
        /// The geo point.
        /// </value>
        [DataMember]
        public DbGeography GeoPoint { get; set; }

        /// <summary>
        /// Gets or sets the geographic boundry for the device
        /// </summary>
        /// <value>
        /// The geo fence.
        /// </value>
        [DataMember]
        public DbGeography GeoFence { get; set; }

        /// <summary>
        /// Gets or sets the type of the device.
        /// </summary>
        /// <value>
        /// The type of the device.
        /// </value>
        [DataMember]
        public int DeviceTypeValueId { get; set; }

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
        /// Gets or sets the locations.
        /// </summary>
        /// <value>
        /// The locations.
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
        [DataMember]
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


}

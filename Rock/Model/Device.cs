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

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// CheckInDevice EF Model.
    /// </summary>
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
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the Description.
        /// </summary>
        /// <value>
        /// Description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the geographic point where the device is located.
        /// </summary>
        /// <value>
        /// The geo point.
        /// </value>
        public DbGeography GeoPoint { get; set; }

        /// <summary>
        /// Gets or sets the geographic boundry for the device
        /// </summary>
        /// <value>
        /// The geo fence.
        /// </value>
        public DbGeography GeoFence { get; set; }

        /// <summary>
        /// Gets or sets the type of the device.
        /// </summary>
        /// <value>
        /// The type of the device.
        /// </value>
        public DeviceType DeviceType { get; set; }

        /// <summary>
        /// Gets or sets the IP address.
        /// </summary>
        /// <value>
        /// The IP address.
        /// </value>
        public string IPAddress { get; set; }

        /// <summary>
        /// Gets or sets the printer id.
        /// </summary>
        /// <value>
        /// The printer id.
        /// </value>
        public int? PrinterId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the locations.
        /// </summary>
        /// <value>
        /// The locations.
        /// </value>
        public virtual ICollection<Crm.Location> Locations
        {
            get { return _locations ?? ( _locations = new Collection<Crm.Location>() ); }
            set { _locations = value; }
        }
        private ICollection<Crm.Location> _locations;

        /// <summary>
        /// Gets or sets the printer.
        /// </summary>
        /// <value>
        /// The printer.
        /// </value>
        public virtual Device Printer { get; set; }

        /// <summary>
        /// Gets the dto.
        /// </summary>
        /// <returns></returns>
        public override IDto Dto
        {
            get { return null; } // return this.ToDto(); }
        }

        #endregion

        #region Public Methods

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

        #region Private Methods

        #endregion

        #region Static Methods

        /// <summary>
        /// Static Method to return an object based on the id
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static Device Read( int id )
        {
            return Read<Device>( id );
        }

        /// <summary>
        /// Static method to return an object based on the GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        public static Device Read( Guid guid )
        {
            return Read<Device>( guid );
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
            this.HasMany( m => m.Locations ).WithMany().Map( m => { m.MapLeftKey( "DeviceId" ); m.MapRightKey( "LocationId" ); m.ToTable( "DeviceLocation" ); } );
            this.HasOptional( d => d.Printer ).WithMany().HasForeignKey( d => d.PrinterId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

    #region Enumerations

    /// <summary>
    /// The type of device
    /// </summary>
    public enum DeviceType
    {
        /// <summary>
        /// Check-in kiosk device
        /// </summary>
        CheckInKiosk = 0,

        /// <summary>
        /// Giving kiosk device
        /// </summary>
        GivingKiosk = 1,

        /// <summary>
        /// Printer device
        /// </summary>
        Printer = 2,

        /// <summary>
        /// Digital Signage device
        /// </summary>
        DigitalSignage = 3
    }

    #endregion


}

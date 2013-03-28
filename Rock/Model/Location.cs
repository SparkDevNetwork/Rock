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
using System.Runtime.Serialization;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Location POCO Entity.
    /// </summary>
    [Table( "Location" )]
    [DataContract]
    public partial class Location : Model<Location>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the parent location id.
        /// </summary>
        /// <value>
        /// The parent location id.
        /// </value>
        [DataMember]
        public int? ParentLocationId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the location point.
        /// </summary>
        /// <value>
        /// The location point.
        /// </value>
        [DataMember]
        public DbGeography LocationPoint { get; set; }

        /// <summary>
        /// Gets or sets the perimeter.
        /// </summary>
        /// <value>
        /// The perimeter.
        /// </value>
        [DataMember]
        public DbGeography Perimeter { get; set; }

        /// <summary>
        /// Gets or sets the location type value id. (i.e. Campus, Building, Room, Neighborhood, Region, etc)
        /// </summary>
        /// <value>
        /// The location type value id.
        /// </value>
        [DataMember]
        public int? LocationTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is a named location.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is a named location; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsLocation { get; set; }

        /// <summary>
        /// Gets or sets the Street 1.
        /// </summary>
        /// <value>
        /// Street 1.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string Street1 { get; set; }
        
        /// <summary>
        /// Gets or sets the Street 2.
        /// </summary>
        /// <value>
        /// Street 2.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string Street2 { get; set; }
        
        /// <summary>
        /// Gets or sets the City.
        /// </summary>
        /// <value>
        /// City.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string City { get; set; }
        
        /// <summary>
        /// Gets or sets the State.
        /// </summary>
        /// <value>
        /// State.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string State { get; set; }
        
        /// <summary>
        /// Gets or sets the Country.
        /// </summary>
        /// <value>
        /// Country.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string Country { get; set; }
        
        /// <summary>
        /// Gets or sets the Zip.
        /// </summary>
        /// <value>
        /// Zip.
        /// </value>
        [MaxLength( 10 )]
        [DataMember]
        public string Zip { get; set; }

        /// <summary>
        /// Gets or sets the Raw.
        /// </summary>
        /// <value>
        /// Raw.
        /// </value>
        [MaxLength( 400 )]
        [DataMember]
        public string FullAddress { get; set; }

        /// <summary>
        /// Gets or sets the Parcel Id.
        /// </summary>
        /// <value>
        /// Parcel Id.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string AssessorParcelId { get; set; }

        /// <summary>
        /// Gets or sets the Standardize Attempt.
        /// </summary>
        /// <value>
        /// Standardize Attempt.
        /// </value>
        [DataMember]
        public DateTime? StandardizeAttemptedDateTime { get; set; }
        
        /// <summary>
        /// Gets or sets the Standardize Service.
        /// </summary>
        /// <value>
        /// Standardize Service.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string StandardizeAttemptedServiceType { get; set; }
        
        /// <summary>
        /// Gets or sets the Standardize Result.
        /// </summary>
        /// <value>
        /// .
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string StandardizeAttemptedResult { get; set; }
        
        /// <summary>
        /// Gets or sets the Standardize Date.
        /// </summary>
        /// <value>
        /// Standardize Date.
        /// </value>
		[DataMember]
        public DateTime? StandardizedDateTime { get; set; }
        
        /// <summary>
        /// Gets or sets the Geocode Attempt.
        /// </summary>
        /// <value>
        /// Geocode Attempt.
        /// </value>
        [DataMember]
        public DateTime? GeocodeAttemptedDateTime { get; set; }
        
        /// <summary>
        /// Gets or sets the Geocode Service.
        /// </summary>
        /// <value>
        /// Geocode Service.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string GeocodeAttemptedServiceType { get; set; }
        
        /// <summary>
        /// Gets or sets the Geocode Result.
        /// </summary>
        /// <value>
        /// .
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string GeocodeAttemptedResult { get; set; }
        
        /// <summary>
        /// Gets or sets the Geocode Date.
        /// </summary>
        /// <value>
        /// Geocode Date.
        /// </value>
        [DataMember]
        public DateTime? GeocodedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the attendance printer id.
        /// </summary>
        /// <value>
        /// The attendance printer id.
        /// </value>
        [DataMember]
        public int? PrinterDeviceId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the parent location.
        /// </summary>
        /// <value>
        /// The parent location.
        /// </value>
        public virtual Location ParentLocation { get; set; }

        /// <summary>
        /// Gets or sets the child locations.
        /// </summary>
        /// <value>
        /// The child locations.
        /// </value>
        [DataMember]
        public virtual ICollection<Location> ChildLocations
        {
            get { return _childLocations ?? ( _childLocations = new Collection<Location>() ); }
            set { _childLocations = value; }
        }
        private ICollection<Location> _childLocations;

        /// <summary>
        /// Gets or sets the group locations.
        /// </summary>
        /// <value>
        /// The group locations.
        /// </value>
        [DataMember]
        public virtual ICollection<GroupLocation> GroupLocations
        {
            get { return _groupLocations ?? ( _groupLocations = new Collection<GroupLocation>() ); }
            set { _groupLocations = value; }
        }
        private ICollection<GroupLocation> _groupLocations;

        /// <summary>
        /// Gets or sets the type of the location
        /// </summary>
        /// <value>
        /// The type of the location.
        /// </value>
        [DataMember]
        public virtual DefinedValue LocationType { get; set; }

        /// <summary>
        /// Gets or sets the attendance printer.
        /// </summary>
        /// <value>
        /// The attendance printer.
        /// </value>
        [DataMember]
        public virtual Device PrinterDevice { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the location point from a latitude and longitude.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        public void SetLocationPointFromLatLong( double latitude, double longitude )
        {
            this.LocationPoint = DbGeography.FromText( string.Format( "POINT({0} {1})", longitude, latitude ) );
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if ( string.IsNullOrEmpty( this.Name ) )
            {
                return string.Format( "{0} {1} {2}, {3} {4}",
                    this.Street1, this.Street2, this.City, this.State, this.Zip );
            }
            else
            {
                return this.Name;
            }
        }

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
            this.HasOptional( l => l.LocationType ).WithMany().HasForeignKey( l => l.LocationTypeValueId ).WillCascadeOnDelete( false );
            this.HasOptional( l => l.PrinterDevice ).WithMany().HasForeignKey( l => l.PrinterDeviceId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

}

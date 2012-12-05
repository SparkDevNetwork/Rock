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
    /// Location POCO Entity.
    /// </summary>
    [Table( "Location" )]
    public partial class Location : Model<Location>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the parent location id.
        /// </summary>
        /// <value>
        /// The parent location id.
        /// </value>
        public int? ParentLocationId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [MaxLength( 100 )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the location point.
        /// </summary>
        /// <value>
        /// The location point.
        /// </value>
        public DbGeography LocationPoint { get; set; }

        /// <summary>
        /// Gets or sets the perimeter.
        /// </summary>
        /// <value>
        /// The perimeter.
        /// </value>
        public DbGeography Perimeter { get; set; }

        /// <summary>
        /// Gets or sets the location type value id.
        /// </summary>
        /// <value>
        /// The location type value id.
        /// </value>
        public int? LocationTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the Street 1.
        /// </summary>
        /// <value>
        /// Street 1.
        /// </value>
        [MaxLength( 100 )]
        public string Street1 { get; set; }
        
        /// <summary>
        /// Gets or sets the Street 2.
        /// </summary>
        /// <value>
        /// Street 2.
        /// </value>
        [MaxLength( 100 )]
        public string Street2 { get; set; }
        
        /// <summary>
        /// Gets or sets the City.
        /// </summary>
        /// <value>
        /// City.
        /// </value>
        [MaxLength( 50 )]
        public string City { get; set; }
        
        /// <summary>
        /// Gets or sets the State.
        /// </summary>
        /// <value>
        /// State.
        /// </value>
        [MaxLength( 50 )]
        public string State { get; set; }
        
        /// <summary>
        /// Gets or sets the Country.
        /// </summary>
        /// <value>
        /// Country.
        /// </value>
        [MaxLength( 50 )]
        public string Country { get; set; }
        
        /// <summary>
        /// Gets or sets the Zip.
        /// </summary>
        /// <value>
        /// Zip.
        /// </value>
        [MaxLength( 10 )]
        public string Zip { get; set; }

        /// <summary>
        /// Gets or sets the Raw.
        /// </summary>
        /// <value>
        /// Raw.
        /// </value>
        [MaxLength( 400 )]
        public string FullAddress { get; set; }

        /// <summary>
        /// Gets or sets the Latitude.
        /// </summary>
        /// <value>
        /// Latitude.
        /// </value>
        public double? Latitude { get; set; }

        /// <summary>
        /// Gets or sets the Longitude.
        /// </summary>
        /// <value>
        /// Longitude.
        /// </value>
        public double? Longitude { get; set; }

        /// <summary>
        /// Gets or sets the Parcel Id.
        /// </summary>
        /// <value>
        /// Parcel Id.
        /// </value>
        [MaxLength( 50 )]
        public string AssessorParcelId { get; set; }

        /// <summary>
        /// Gets or sets the Standardize Attempt.
        /// </summary>
        /// <value>
        /// Standardize Attempt.
        /// </value>
        public DateTime? StandardizeAttemptedDateTime { get; set; }
        
        /// <summary>
        /// Gets or sets the Standardize Service.
        /// </summary>
        /// <value>
        /// Standardize Service.
        /// </value>
        [MaxLength( 50 )]
        public string StandardizeAttemptedServiceType { get; set; }
        
        /// <summary>
        /// Gets or sets the Standardize Result.
        /// </summary>
        /// <value>
        /// .
        /// </value>
        [MaxLength( 50 )]
        public string StandardizeAttemptedResult { get; set; }
        
        /// <summary>
        /// Gets or sets the Standardize Date.
        /// </summary>
        /// <value>
        /// Standardize Date.
        /// </value>
        public DateTime? StandardizedDateTime { get; set; }
        
        /// <summary>
        /// Gets or sets the Geocode Attempt.
        /// </summary>
        /// <value>
        /// Geocode Attempt.
        /// </value>
        public DateTime? GeocodeAttemptedDateTime { get; set; }
        
        /// <summary>
        /// Gets or sets the Geocode Service.
        /// </summary>
        /// <value>
        /// Geocode Service.
        /// </value>
        [MaxLength( 50 )]
        public string GeocodeAttemptedServiceType { get; set; }
        
        /// <summary>
        /// Gets or sets the Geocode Result.
        /// </summary>
        /// <value>
        /// .
        /// </value>
        [MaxLength( 50 )]
        public string GeocodeAttemptedResult { get; set; }
        
        /// <summary>
        /// Gets or sets the Geocode Date.
        /// </summary>
        /// <value>
        /// Geocode Date.
        /// </value>
        public DateTime? GeocodedDateTime { get; set; }

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
        public virtual ICollection<Location> ChildLocations
        {
            get { return _childLocations ?? ( _childLocations = new Collection<Location>() ); }
            set { _childLocations = value; }
        }
        private ICollection<Location> _childLocations;

        public virtual DefinedValue LocationType { get; set; }
        /// <summary>
        /// Gets the dto.
        /// </summary>
        /// <returns></returns>
        public override IDto Dto
        {
            get { return this.ToDto(); }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format( "{0} {1} {2}, {3} {4}",
                this.Street1, this.Street2, this.City, this.State, this.Zip );
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
        public static Location Read( int id )
        {
            return Read<Location>( id );
        }

        /// <summary>
        /// Static method to return an object based on the GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        public static Location Read( Guid guid )
        {
            return Read<Location>( guid );
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
        }
    }

    #endregion

}

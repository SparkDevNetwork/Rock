//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.CRM
{
    /// <summary>
    /// Address POCO Entity.
    /// </summary>
    [Table( "crmAddress" )]
    public partial class Address : ModelWithAttributes<Address>, IAuditable
    {
		/// <summary>
		/// Gets or sets the Raw.
		/// </summary>
		/// <value>
		/// Raw.
		/// </value>
		[MaxLength( 400 )]
		[DataMember]
		public string Raw { get; set; }
		
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
		/// Gets or sets the Latitude.
		/// </summary>
		/// <value>
		/// Latitude.
		/// </value>
		[DataMember]
		public double Latitude { get; set; }
		
		/// <summary>
		/// Gets or sets the Longitude.
		/// </summary>
		/// <value>
		/// Longitude.
		/// </value>
		[DataMember]
		public double Longitude { get; set; }
		
		/// <summary>
		/// Gets or sets the Standardize Attempt.
		/// </summary>
		/// <value>
		/// Standardize Attempt.
		/// </value>
		[DataMember]
		public DateTime? StandardizeAttempt { get; set; }
		
		/// <summary>
		/// Gets or sets the Standardize Service.
		/// </summary>
		/// <value>
		/// Standardize Service.
		/// </value>
		[MaxLength( 50 )]
		[DataMember]
		public string StandardizeService { get; set; }
		
		/// <summary>
		/// Gets or sets the Standardize Result.
		/// </summary>
		/// <value>
		/// .
		/// </value>
		[MaxLength( 50 )]
		[DataMember]
		public string StandardizeResult { get; set; }
		
		/// <summary>
		/// Gets or sets the Standardize Date.
		/// </summary>
		/// <value>
		/// Standardize Date.
		/// </value>
		[DataMember]
		public DateTime? StandardizeDate { get; set; }
		
		/// <summary>
		/// Gets or sets the Geocode Attempt.
		/// </summary>
		/// <value>
		/// Geocode Attempt.
		/// </value>
		[DataMember]
		public DateTime? GeocodeAttempt { get; set; }
		
		/// <summary>
		/// Gets or sets the Geocode Service.
		/// </summary>
		/// <value>
		/// Geocode Service.
		/// </value>
		[MaxLength( 50 )]
		[DataMember]
		public string GeocodeService { get; set; }
		
		/// <summary>
		/// Gets or sets the Geocode Result.
		/// </summary>
		/// <value>
		/// .
		/// </value>
		[MaxLength( 50 )]
		[DataMember]
		public string GeocodeResult { get; set; }
		
		/// <summary>
		/// Gets or sets the Geocode Date.
		/// </summary>
		/// <value>
		/// Geocode Date.
		/// </value>
		[DataMember]
		public DateTime? GeocodeDate { get; set; }
		
		/// <summary>
		/// Gets or sets the Created Date Time.
		/// </summary>
		/// <value>
		/// Created Date Time.
		/// </value>
		[DataMember]
		public DateTime? CreatedDateTime { get; set; }
		
		/// <summary>
		/// Gets or sets the Modified Date Time.
		/// </summary>
		/// <value>
		/// Modified Date Time.
		/// </value>
		[DataMember]
		public DateTime? ModifiedDateTime { get; set; }
		
		/// <summary>
		/// Gets or sets the Created By Person Id.
		/// </summary>
		/// <value>
		/// Created By Person Id.
		/// </value>
		[DataMember]
		public int? CreatedByPersonId { get; set; }
		
		/// <summary>
		/// Gets or sets the Modified By Person Id.
		/// </summary>
		/// <value>
		/// Modified By Person Id.
		/// </value>
		[DataMember]
		public int? ModifiedByPersonId { get; set; }
		
        /// <summary>
        /// Gets the auth entity.
        /// </summary>
		[NotMapped]
		public override string AuthEntity { get { return "CRM.Address"; } }
        
		/// <summary>
        /// Gets or sets the Created By Person.
        /// </summary>
        /// <value>
        /// A <see cref="Person"/> object.
        /// </value>
		public virtual Person CreatedByPerson { get; set; }
        
		/// <summary>
        /// Gets or sets the Modified By Person.
        /// </summary>
        /// <value>
        /// A <see cref="Person"/> object.
        /// </value>
		public virtual Person ModifiedByPerson { get; set; }

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
    }

    /// <summary>
    /// Address Configuration class.
    /// </summary>
    public partial class AddressConfiguration : EntityTypeConfiguration<Address>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddressConfiguration"/> class.
        /// </summary>
        public AddressConfiguration()
        {
			this.HasOptional( p => p.CreatedByPerson ).WithMany().HasForeignKey( p => p.CreatedByPersonId ).WillCascadeOnDelete(false);
			this.HasOptional( p => p.ModifiedByPerson ).WithMany().HasForeignKey( p => p.ModifiedByPersonId ).WillCascadeOnDelete(false);
		}
    }

    /// <summary>
    /// Data Transformation Object
    /// </summary>
    public partial class AddressDTO : DTO<Address>
    {
        /// <summary>
        /// Gets or sets the Raw.
        /// </summary>
        /// <value>
        /// Raw.
        /// </value>
        public string Raw { get; set; }

        /// <summary>
        /// Gets or sets the Street 1.
        /// </summary>
        /// <value>
        /// Street 1.
        /// </value>
        public string Street1 { get; set; }

        /// <summary>
        /// Gets or sets the Street 2.
        /// </summary>
        /// <value>
        /// Street 2.
        /// </value>
        public string Street2 { get; set; }

        /// <summary>
        /// Gets or sets the City.
        /// </summary>
        /// <value>
        /// City.
        /// </value>
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the State.
        /// </summary>
        /// <value>
        /// State.
        /// </value>
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the Country.
        /// </summary>
        /// <value>
        /// Country.
        /// </value>
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets the Zip.
        /// </summary>
        /// <value>
        /// Zip.
        /// </value>
        public string Zip { get; set; }

        /// <summary>
        /// Gets or sets the Latitude.
        /// </summary>
        /// <value>
        /// Latitude.
        /// </value>
        public double Latitude { get; set; }

        /// <summary>
        /// Gets or sets the Longitude.
        /// </summary>
        /// <value>
        /// Longitude.
        /// </value>
        public double Longitude { get; set; }

        /// <summary>
        /// Gets or sets the Standardize Attempt.
        /// </summary>
        /// <value>
        /// Standardize Attempt.
        /// </value>
        public DateTime? StandardizeAttempt { get; set; }

        /// <summary>
        /// Gets or sets the Standardize Service.
        /// </summary>
        /// <value>
        /// Standardize Service.
        /// </value>
        public string StandardizeService { get; set; }

        /// <summary>
        /// Gets or sets the Standardize Result.
        /// </summary>
        /// <value>
        /// Standardize Result.
        /// </value>
        public string StandardizeResult { get; set; }

        /// <summary>
        /// Gets or sets the Standardize Date.
        /// </summary>
        /// <value>
        /// Standardize Date.
        /// </value>
        public DateTime? StandardizeDate { get; set; }

        /// <summary>
        /// Gets or sets the Geocode Attempt.
        /// </summary>
        /// <value>
        /// Geocode Attempt.
        /// </value>
        public DateTime? GeocodeAttempt { get; set; }

        /// <summary>
        /// Gets or sets the Geocode Service.
        /// </summary>
        /// <value>
        /// Geocode Service.
        /// </value>
        public string GeocodeService { get; set; }

        /// <summary>
        /// Gets or sets the Geocode Result.
        /// </summary>
        /// <value>
        /// Geocode Result.
        /// </value>
        public string GeocodeResult { get; set; }

        /// <summary>
        /// Gets or sets the Geocode Date.
        /// </summary>
        /// <value>
        /// Geocode Date.
        /// </value>
        public DateTime? GeocodeDate { get; set; }

        /// <summary>
        /// Instantiate new DTO object
        /// </summary>
        public AddressDTO()
        {
        }

        /// <summary>
        /// Instantiate new DTO object from Model
        /// </summary>
        /// <param name="auth"></param>
        public AddressDTO( Address address )
        {
            CopyFromModel( address );
        }

        /// <summary>
        /// Copy DTO to Model
        /// </summary>
        /// <param name="address"></param>
        public override void CopyFromModel( Address address )
        {
            this.Id = address.Id;
            this.Guid = address.Guid;
            this.Raw = address.Raw;
            this.Street1 = address.Street1;
            this.Street2 = address.Street2;
            this.City = address.City;
            this.State = address.State;
            this.Country = address.Country;
            this.Zip = address.Zip;
            this.Latitude = address.Latitude;
            this.Longitude = address.Longitude;
            this.StandardizeAttempt = address.StandardizeAttempt;
            this.StandardizeService = address.StandardizeService;
            this.StandardizeResult = address.StandardizeResult;
            this.StandardizeDate = address.StandardizeDate;
            this.GeocodeAttempt = address.GeocodeAttempt;
            this.GeocodeService = address.GeocodeService;
            this.GeocodeResult = address.GeocodeResult;
            this.GeocodeDate = address.GeocodeDate;
            this.CreatedDateTime = address.CreatedDateTime;
            this.ModifiedDateTime = address.ModifiedDateTime;
            this.CreatedByPersonId = address.CreatedByPersonId;
            this.ModifiedByPersonId = address.ModifiedByPersonId;
        }

        /// <summary>
        /// Copy Model to DTO
        /// </summary>
        /// <param name="address"></param>
        public override void CopyToModel( Address address )
        {
            address.Id = this.Id;
            address.Guid = this.Guid;
            address.Raw = this.Raw;
            address.Street1 = this.Street1;
            address.Street2 = this.Street2;
            address.City = this.City;
            address.State = this.State;
            address.Country = this.Country;
            address.Zip = this.Zip;
            address.Latitude = this.Latitude;
            address.Longitude = this.Longitude;
            address.StandardizeAttempt = this.StandardizeAttempt;
            address.StandardizeService = this.StandardizeService;
            address.StandardizeResult = this.StandardizeResult;
            address.StandardizeDate = this.StandardizeDate;
            address.GeocodeAttempt = this.GeocodeAttempt;
            address.GeocodeService = this.GeocodeService;
            address.GeocodeResult = this.GeocodeResult;
            address.GeocodeDate = this.GeocodeDate;
            address.CreatedDateTime = this.CreatedDateTime;
            address.ModifiedDateTime = this.ModifiedDateTime;
            address.CreatedByPersonId = this.CreatedByPersonId;
            address.ModifiedByPersonId = this.ModifiedByPersonId;
        }
    }
}

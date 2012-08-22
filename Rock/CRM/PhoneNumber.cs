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
    /// Phone Number POCO Entity.
    /// </summary>
    [Table( "crmPhoneNumber" )]
    public partial class PhoneNumber : ModelWithAttributes<PhoneNumber>, IAuditable
    {
		/// <summary>
		/// Gets or sets the System.
		/// </summary>
		/// <value>
		/// System.
		/// </value>
		[Required]
		[DataMember]
		public bool IsSystem { get; set; }
		
		/// <summary>
		/// Gets or sets the Person Id.
		/// </summary>
		/// <value>
		/// Person Id.
		/// </value>
		[Required]
		[DataMember]
		public int PersonId { get; set; }
		
		/// <summary>
		/// Gets or sets the Number.
		/// </summary>
		/// <value>
		/// Number.
		/// </value>
		[Required]
		[MaxLength( 100 )]
		[DataMember]
		public string Number { get; set; }
		
		/// <summary>
		/// Gets or sets the Description.
		/// </summary>
		/// <value>
		/// Description.
		/// </value>
		[DataMember]
		public string Description { get; set; }
		
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
		public override string AuthEntity { get { return "CRM.PhoneNumber"; } }
        
		/// <summary>
        /// Gets or sets the Person.
        /// </summary>
        /// <value>
        /// A <see cref="Person"/> object.
        /// </value>
		public virtual Person Person { get; set; }
        
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

    }

    /// <summary>
    /// Phone Number Configuration class.
    /// </summary>
    public partial class PhoneNumberConfiguration : EntityTypeConfiguration<PhoneNumber>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PhoneNumberConfiguration"/> class.
        /// </summary>
        public PhoneNumberConfiguration()
        {
			this.HasRequired( p => p.Person ).WithMany( p => p.PhoneNumbers ).HasForeignKey( p => p.PersonId ).WillCascadeOnDelete(false);
			this.HasOptional( p => p.CreatedByPerson ).WithMany().HasForeignKey( p => p.CreatedByPersonId ).WillCascadeOnDelete(false);
			this.HasOptional( p => p.ModifiedByPerson ).WithMany().HasForeignKey( p => p.ModifiedByPersonId ).WillCascadeOnDelete(false);
		}
    }

    /// <summary>
    /// Data Transformation Object
    /// </summary>
    public partial class PhoneNumberDTO : DTO<PhoneNumber>
    {
        /// <summary>
        /// Gets or sets the System.
        /// </summary>
        /// <value>
        /// System.
        /// </value>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Person Id.
        /// </summary>
        /// <value>
        /// Person Id.
        /// </value>
        public int PersonId { get; set; }

        /// <summary>
        /// Gets or sets the Number.
        /// </summary>
        /// <value>
        /// Number.
        /// </value>
        public string Number { get; set; }

        /// <summary>
        /// Gets or sets the Description.
        /// </summary>
        /// <value>
        /// Description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Instantiate new DTO object
        /// </summary>
        public PhoneNumberDTO()
        {
        }

        /// <summary>
        /// Instantiate new DTO object from Model
        /// </summary>
        /// <param name="auth"></param>
        public PhoneNumberDTO( PhoneNumber phoneNumber )
        {
            CopyFromModel( phoneNumber );
        }

        /// <summary>
        /// Copy DTO to Model
        /// </summary>
        /// <param name="phoneNumber"></param>
        public override void CopyFromModel( PhoneNumber phoneNumber )
        {
            this.Id = phoneNumber.Id;
            this.Guid = phoneNumber.Guid;
            this.IsSystem = phoneNumber.IsSystem;
            this.PersonId = phoneNumber.PersonId;
            this.Number = phoneNumber.Number;
            this.Description = phoneNumber.Description;
            this.CreatedDateTime = phoneNumber.CreatedDateTime;
            this.ModifiedDateTime = phoneNumber.ModifiedDateTime;
            this.CreatedByPersonId = phoneNumber.CreatedByPersonId;
            this.ModifiedByPersonId = phoneNumber.ModifiedByPersonId;
        }

        /// <summary>
        /// Copy Model to DTO
        /// </summary>
        /// <param name="phoneNumber"></param>
        public override void CopyToModel( PhoneNumber phoneNumber )
        {
            phoneNumber.Id = this.Id;
            phoneNumber.Guid = this.Guid;
            phoneNumber.IsSystem = this.IsSystem;
            phoneNumber.PersonId = this.PersonId;
            phoneNumber.Number = this.Number;
            phoneNumber.Description = this.Description;
            phoneNumber.CreatedDateTime = this.CreatedDateTime;
            phoneNumber.ModifiedDateTime = this.ModifiedDateTime;
            phoneNumber.CreatedByPersonId = this.CreatedByPersonId;
            phoneNumber.ModifiedByPersonId = this.ModifiedByPersonId;
        }
    }
}

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

namespace Rock.Crm
{
    /// <summary>
    /// Phone Number POCO Entity.
    /// </summary>
    [Table( "crmPhoneNumber" )]
    public partial class PhoneNumber : Model<PhoneNumber>, IAuditable
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
		[MaxLength( 20 )]
		[DataMember]
		public string Number { get; set; }

		/// <summary>
		/// Gets or sets the extension
		/// </summary>
		[MaxLength( 20 )]
		[DataMember]
		public string Extension { get; set; }

		/// <summary>
		/// Type of phone number
		/// </summary>
		[DataMember]
		public int? NumberTypeId { get; set; }

		/// <summary>
		/// Gets or sets whether the number has been opted in for SMS
		/// </summary>
		[Required]
		[DataMember]
		public bool IsMessagingEnabled { get; set; }

		/// <summary>
		/// The phone number type
		/// </summary>
		public virtual Core.DefinedValue NumberType { get; set; }

		/// <summary>
		/// Gets or sets the whether the number is unlisted or not.
		/// </summary>
		/// <value>
		/// IsUnlisted.
		/// </value>
		[DataMember]
		public bool IsUnlisted { get; set; }

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
		public override string EntityTypeName { get { return "Crm.PhoneNumber"; } }
        
		/// <summary>
        /// Gets or sets the Person.
        /// </summary>
        /// <value>
        /// A <see cref="Person"/> object.
        /// </value>
		public virtual Person Person { get; set; }
        
		/// <summary>
		/// Static Method to return an object based on the id
		/// </summary>
		/// <param name="id">The id.</param>
		/// <returns></returns>
		public static PhoneNumber Read( int id )
		{
			return Read<PhoneNumber>( id );
		}

		/// <summary>
		/// Formats a phone number
		/// </summary>
		/// <param name="number"></param>
		/// <returns></returns>
		public static string FormattedNumber( string number )
		{
			number = new System.Text.RegularExpressions.Regex( @"\D" ).Replace( number, string.Empty );
			number = number.TrimStart( '1' );
			if ( number.Length == 7 )
				return Convert.ToInt64( number ).ToString( "###\\.####" );
			if ( number.Length == 10 )
				return Convert.ToInt64( number ).ToString( "###\\.###\\.####" );
			if ( number.Length > 10 )
				return Convert.ToInt64( number )
					.ToString( "###\\.###\\.#### " + new String( '#', ( number.Length - 10 ) ) );
			return number;
		}


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
			this.HasOptional( p => p.NumberType ).WithMany().HasForeignKey( p => p.NumberTypeId ).WillCascadeOnDelete( false );
		}
    }
}

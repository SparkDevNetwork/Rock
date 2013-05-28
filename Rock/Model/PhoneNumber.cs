//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Phone Number POCO Entity.
    /// </summary>
    [Table( "PhoneNumber" )]
    [DataContract]
    public partial class PhoneNumber : Model<PhoneNumber>
    {
        /// <summary>
        /// Gets or sets the System.
        /// </summary>
        /// <value>
        /// System.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }
        
        /// <summary>
        /// Gets or sets the Person Id.
        /// </summary>
        /// <value>
        /// Person Id.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int PersonId { get; set; }
        
        /// <summary>
        /// Gets or sets the Number.
        /// </summary>
        /// <value>
        /// Number.
        /// </value>
        [Required]
        [MaxLength( 20 )]
        [DataMember( IsRequired = true )]
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
        public int? NumberTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets whether the number has been opted in for SMS
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsMessagingEnabled { get; set; }

        /// <summary>
        /// The phone number type
        /// </summary>
        [DataMember]
        public virtual Model.DefinedValue NumberTypeValue { get; set; }

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
        /// Gets or sets the Person.
        /// </summary>
        /// <value>
        /// A <see cref="Person"/> object.
        /// </value>
        [DataMember]
        public virtual Person Person { get; set; }

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
                return Convert.ToInt64( number ).ToString( "###-####" );
            if ( number.Length == 10 )
                return Convert.ToInt64( number ).ToString( "(###) ###-####" );
            if ( number.Length > 10 )
                return Convert.ToInt64( number )
                    .ToString( "(###) ###-#### " + new String( '#', ( number.Length - 10 ) ) );
            return number;
        }

        /// <summary>
        /// Gets the formatted number.
        /// </summary>
        /// <value>
        /// The formatted number.
        /// </value>
        public virtual string NumberFormatted
        {
            get { return PhoneNumber.FormattedNumber( Number ); }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return FormattedNumber( this.Number );
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
            this.HasOptional( p => p.NumberTypeValue ).WithMany().HasForeignKey( p => p.NumberTypeValueId ).WillCascadeOnDelete( false );
        }
    }
}

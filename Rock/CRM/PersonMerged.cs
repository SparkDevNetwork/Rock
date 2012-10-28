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
    /// Person Trail POCO Entity.
    /// </summary>
    [Table( "crmPersonMerged" )]
    public partial class PersonMerged : Model<PersonMerged>
    {
        /// <summary>
        /// Gets or sets the Current Id.
        /// </summary>
        /// <value>
        /// Current Id.
        /// </value>
        [Required]
        [DataMember]
        public int CurrentId { get; set; }
        
        /// <summary>
        /// Gets or sets the Current Guid.
        /// </summary>
        /// <value>
        /// Current Guid.
        /// </value>
        [Required]
        [DataMember]
        public Guid CurrentGuid { get; set; }
        
        /// <summary>
        /// Gets the auth entity.
        /// </summary>
        [NotMapped]
        public string DeprecatedEntityTypeName { get { return "Crm.PersonMerged"; } }

        /// <summary>
        /// Gets a publicly viewable unique key for the model.
        /// </summary>
        [NotMapped]
        public string CurrentPublicKey
        {
            get
            {
                string identifier = this.CurrentId.ToString() + ">" + this.CurrentGuid.ToString();
                return Rock.Security.Encryption.EncryptString( identifier );
            }
        }

        /// <summary>
        /// Static Method to return an object based on the id
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static PersonMerged Read( int id )
        {
            return Read<PersonMerged>( id );
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format( "{0}->{1}", this.Id, this.CurrentId );
        }
    }

    /// <summary>
    /// Person Trail Configuration class.
    /// </summary>
    public partial class PersonMergedConfiguration : EntityTypeConfiguration<PersonMerged>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonMergedConfiguration"/> class.
        /// </summary>
        public PersonMergedConfiguration()
        {
        }
    }
}

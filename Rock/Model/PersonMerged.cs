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
    /// Person Merged POCO Entity.
    /// </summary>
    [Table( "PersonMerged" )]
    [DataContract]
    public partial class PersonMerged : Entity<PersonMerged>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the previous person id.
        /// </summary>
        /// <value>
        /// The previous person id.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [AlternateKey]
        public int PreviousPersonId { get; set; }

        /// <summary>
        /// Gets or sets the previous person GUID.
        /// </summary>
        /// <value>
        /// The previous person GUID.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public Guid PreviousPersonGuid { get; set; }

        /// <summary>
        /// Gets or sets the new person id.
        /// </summary>
        /// <value>
        /// The new person id.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int NewPersonId { get; set; }

        /// <summary>
        /// Gets or sets the new person GUID.
        /// </summary>
        /// <value>
        /// The new person GUID.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public Guid NewPersonGuid { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets the previous encrypted key.
        /// </summary>
        /// <value>
        /// The previous encrypted key.
        /// </value>
        [NotMapped]
        public virtual string PreviousEncryptedKey
        {
            get
            {
                string identifier = this.PreviousPersonId.ToString() + ">" + this.PreviousPersonGuid.ToString();
                return Rock.Security.Encryption.EncryptString( identifier );
            }
        }
        /// <summary>
        /// Gets the new encrypted key.
        /// </summary>
        /// <value>
        /// The new encrypted key.
        /// </value>
        [NotMapped]
        public virtual string NewEncryptedKey
        {
            get
            {
                string identifier = this.NewPersonId.ToString() + ">" + this.NewPersonGuid.ToString();
                return Rock.Security.Encryption.EncryptString( identifier );
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format( "{0}->{1}", this.PreviousPersonId, this.NewPersonId);
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Person Merged Configuration class.
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

    #endregion
}

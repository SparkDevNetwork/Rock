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
    /// Represents the merge history for people in RockChMS. When a person is found to have a duplicate <see cref="Rock.Model.Person"/> entity in the database
    /// the duplicate entities need to be merged together into one record to avoid confusion and to ensure that we have accurate contact, involvement, and 
    /// contribution data. It also helps to avoid situations where an individual is counted or contacted multiple times.
    /// 
    /// The PersonMerged entity is a log containing the merge history (previous Person identifiers) and a pointer to the Person's current Id.
    /// </summary>
    [Table( "PersonMerged" )]
    [DataContract]
    public partial class PersonMerged : Entity<PersonMerged>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the previous (merged) Id of the <see cref="Rock.Model.Person"/>. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the previous (merged) Id of the <see cref="Rock.Model.Person"/>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [AlternateKey]
        public int PreviousPersonId { get; set; }

        /// <summary>
        /// Gets or sets the previous (merged) <see cref="System.Guid"/> for the <see cref="Rock.Model.Person"/>. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Guid"/> representing the previous <see cref="System.Guid"/> identifier for the <see cref="Rock.Model.Person"/>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public Guid PreviousPersonGuid { get; set; }

        /// <summary>
        /// Gets or sets the new/current Id of the <see cref="Rock.Model.Person"/>. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the new/current Id of the <see cref="Rock.Model.Person"/>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int NewPersonId { get; set; }

        /// <summary>
        /// Gets or sets the new <see cref="System.Guid"/> identifier of the <see cref="Rock.Model.Person"/>. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Guid"/> representing the new/current Guid identifier of the <see cref="Rock.Model.Person"/>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public Guid NewPersonGuid { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets the previous encrypted key for the <see cref="Rock.Model.Person"/>.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the previous encrypted key for the <see cref="Rock.Model.Person"/>.
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
        /// Gets the new encrypted key for the <see cref="Rock.Model.Person"/>
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing the new encrypted key for the <see cref="Rock.Model.Person"/>
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

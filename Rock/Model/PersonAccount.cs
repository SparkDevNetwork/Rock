//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// PersonAccountLookup POCO class.
    /// </summary>
    [Table("PersonAccount")]
    [DataContract]
    public partial class PersonAccount : Model<PersonAccount>
    {
        /// <summary>
        /// Gets or sets the person id.
        /// </summary>
        /// <value>
        /// The person id.
        /// </value>
        [DataMember]
        public int? PersonId { get; set; }

        /// <summary>
        /// Gets or sets the account.
        /// </summary>
        /// <value>
        /// The account.
        /// </value>
        [MaxLength(50)]
        [DataMember]
        public string Account { get; set; }

        /// <summary>
        /// Gets or sets the person.
        /// </summary>
        /// <value>
        /// The person.
        /// </value>
        [DataMember]
        public virtual Person Person { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Account;
        }
    }

    /// <summary>
    /// PersonAccountLookup Configuration class.
    /// </summary>
    public partial class PersonAccountLookupConfiguration : EntityTypeConfiguration<PersonAccount>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonAccountLookupConfiguration"/> class.
        /// </summary>
        public PersonAccountLookupConfiguration()
        {
            this.HasOptional(p => p.Person).WithMany(p => p.PersonAccountLookups).HasForeignKey(p => p.PersonId).WillCascadeOnDelete(false);
        }
    }
}
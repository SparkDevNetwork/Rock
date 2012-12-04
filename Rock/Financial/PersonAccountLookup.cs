//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Rock.Model;
using Rock.Data;

namespace Rock.Financial
{
    /// <summary>
    /// PersonAccountLookup POCO class.
    /// </summary>
    [Table("PersonAccount")]
    public partial class PersonAccount : Model<PersonAccount>
    {
        /// <summary>
        /// Gets or sets the person id.
        /// </summary>
        /// <value>
        /// The person id.
        /// </value>
        public int? PersonId { get; set; }

        /// <summary>
        /// Gets or sets the account.
        /// </summary>
        /// <value>
        /// The account.
        /// </value>
        [MaxLength(50)]
        public string Account { get; set; }

        /// <summary>
        /// Gets or sets the person.
        /// </summary>
        /// <value>
        /// The person.
        /// </value>
        public virtual Person Person { get; set; }

        /// <summary>
        /// Gets the dto.
        /// </summary>
        /// <returns></returns>
        public override IDto Dto
        {
            get { return this.ToDto(); }
        }

        /// <summary>
        /// Static Method to return an object based on the id
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static PersonAccount Read( int id )
        {
            return Read<PersonAccount>( id );
        }

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
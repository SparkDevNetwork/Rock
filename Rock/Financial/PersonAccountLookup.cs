//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.CRM;
using Rock.Data;

namespace Rock.Financial
{
    /// <summary>
    /// PersonAccountLookup POCO class.
    /// </summary>
    [Table("financialPersonAccountLookup")]
    public partial class PersonAccountLookup : ModelWithAttributes<PersonAccountLookup>
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
        [DataMember]
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
        /// Gets the auth entity.
        /// </summary>
        public override string AuthEntity { get { return "Financial.PersonAccountLookup"; } }
    }

    /// <summary>
    /// PersonAccountLookup Configuration class.
    /// </summary>
    public partial class PersonAccountLookupConfiguration : EntityTypeConfiguration<PersonAccountLookup>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonAccountLookupConfiguration"/> class.
        /// </summary>
        public PersonAccountLookupConfiguration()
        {
            this.HasOptional(p => p.Person).WithMany(p => p.PersonAccountLookups).HasForeignKey(p => p.PersonId).WillCascadeOnDelete(false);
        }
    }

    /// <summary>
    /// Data Transformation Object
    /// </summary>
    public partial class PersonAccountLookupDTO : DTO<PersonAccountLookup>
    {
        /// <summary>
        /// Instantiate new DTO object
        /// </summary>
        public PersonAccountLookupDTO()
        {
        }

        /// <summary>
        /// Instantiate new DTO object from Model
        /// </summary>
        /// <param name="auth"></param>
        public PersonAccountLookupDTO( PersonAccountLookup personAccountLookup )
        {
            CopyFromModel( personAccountLookup );
        }

        /// <summary>
        /// Copy DTO to Model
        /// </summary>
        /// <param name="personAccountLookup"></param>
        public override void CopyFromModel( PersonAccountLookup personAccountLookup )
        {
        }

        /// <summary>
        /// Copy Model to DTO
        /// </summary>
        /// <param name="personAccountLookup"></param>
        public override void CopyToModel( PersonAccountLookup personAccountLookup )
        {
        }
    }
}
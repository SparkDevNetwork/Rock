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
    /// Site Domain POCO Entity.
    /// </summary>
    [Table( "SiteDomain" )]
    [DataContract( IsReference = true )]
    public partial class SiteDomain : Model<SiteDomain>
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
        /// Gets or sets the Site Id.
        /// </summary>
        /// <value>
        /// Site Id.
        /// </value>
        [Required]
        [DataMember]
        public int SiteId { get; set; }
        
        /// <summary>
        /// Gets or sets the Domain.
        /// </summary>
        /// <value>
        /// Domain.
        /// </value>
        [Required]
        [MaxLength( 200 )]
        [DataMember]
        public string Domain { get; set; }
        
        /// <summary>
        /// Gets or sets the Site.
        /// </summary>
        /// <value>
        /// A <see cref="Site"/> object.
        /// </value>
        [DataMember]
        public virtual Site Site { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Domain;
        }
    }

    /// <summary>
    /// Site Domain Configuration class.
    /// </summary>
    public partial class SiteDomainConfiguration : EntityTypeConfiguration<SiteDomain>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SiteDomainConfiguration"/> class.
        /// </summary>
        public SiteDomainConfiguration()
        {
            this.HasRequired( p => p.Site ).WithMany( p => p.SiteDomains ).HasForeignKey( p => p.SiteId ).WillCascadeOnDelete(true);
        }
    }
}

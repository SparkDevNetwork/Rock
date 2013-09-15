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
    /// Represents a SiteDomain or URL that is associated with a <see cref="Rock.Model.Site"/> in RockChMS.  A domain (i.e. yourchurch.org) and a subdomain (sub.yourchurch.org) are considered
    /// two different SiteDomains in RockChMS.
    /// </summary>
    /// <remarks>
    /// A SiteDomain must have a matching Binding setup in IIS otherwise it will not resolve.
    /// </remarks>
    [Table( "SiteDomain" )]
    [DataContract]
    public partial class SiteDomain : Model<SiteDomain>
    {
        /// <summary>
        /// Gets or sets a flag indicating if this SiteDomain was created by and is part of the RockChMS core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if the SiteDomain is part of the core system/framework.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }
        
        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Site"/> that this SiteDomain references. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> containing the Id of the <see cref="Rock.Model.Site"/> that this SiteDomain references.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int SiteId { get; set; }
        
        /// <summary>
        /// Gets or sets the the Url/Domain Name of this SiteDomain. This property is required.
        /// </summary>
        /// <remarks>
        /// Examples include: localhost, mysite.com (or www.mysite.com), subdomain.mysite.com.
        /// </remarks>
        /// <value>
        /// A <see cref="System.String"/> containing the Domain Name for this SiteDomain.
        /// </value>
        [Required]
        [MaxLength( 200 )]
        [DataMember( IsRequired = true )]
        public string Domain { get; set; }
        
        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Site"/> that is associated with this SiteDomain.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Site"/> that this SiteDomain is associated with.
        /// </value>
        [DataMember]
        public virtual Site Site { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> containing the domain name and represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> containing the domain name  that represents this instance.
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

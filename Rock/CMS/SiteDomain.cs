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
using Rock.Data;

namespace Rock.Cms
{
    /// <summary>
    /// Site Domain POCO Entity.
    /// </summary>
    [Table( "cmsSiteDomain" )]
    public partial class SiteDomain : Model<SiteDomain>
    {
        /// <summary>
        /// Gets or sets the System.
        /// </summary>
        /// <value>
        /// System.
        /// </value>
        [Required]
        public bool IsSystem { get; set; }
        
        /// <summary>
        /// Gets or sets the Site Id.
        /// </summary>
        /// <value>
        /// Site Id.
        /// </value>
        [Required]
        public int SiteId { get; set; }
        
        /// <summary>
        /// Gets or sets the Domain.
        /// </summary>
        /// <value>
        /// Domain.
        /// </value>
        [Required]
        [MaxLength( 200 )]
        public string Domain { get; set; }
        
        /// <summary>
        /// Gets or sets the Site.
        /// </summary>
        /// <value>
        /// A <see cref="Site"/> object.
        /// </value>
        public virtual Site Site { get; set; }
        
        /// <summary>
        /// Static Method to return an object based on the id
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static SiteDomain Read( int id )
        {
            return Read<SiteDomain>( id );
        }

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

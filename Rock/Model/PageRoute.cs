//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Newtonsoft.Json;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Page Route POCO Entity.
    /// </summary>
    [Table( "PageRoute" )]
    [DataContract( IsReference = true )]
    public partial class PageRoute : Model<PageRoute>
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
        /// Gets or sets the Page Id.
        /// </summary>
        /// <value>
        /// Page Id.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int PageId { get; set; }
        
        /// <summary>
        /// Gets or sets the Route.
        /// </summary>
        /// <value>
        /// Route.
        /// </value>
        [Route]
        [Required]
        [MaxLength( 200 )]
        [DataMember( IsRequired = true )]
        public string Route { get; set; }
        
        /// <summary>
        /// Gets or sets the Page.
        /// </summary>
        /// <value>
        /// A <see cref="Page"/> object.
        /// </value>
        [DataMember]
        public virtual Page Page { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Route;
        }

    }

    /// <summary>
    /// Page Route Configuration class.
    /// </summary>
    public partial class PageRouteConfiguration : EntityTypeConfiguration<PageRoute>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PageRouteConfiguration"/> class.
        /// </summary>
        public PageRouteConfiguration()
        {
            this.HasRequired( p => p.Page ).WithMany( p => p.PageRoutes ).HasForeignKey( p => p.PageId ).WillCascadeOnDelete( true );
        }
    }
}

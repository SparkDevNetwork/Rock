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
    /// Represents a PageRoute object in RockChMS. All pages in RockChMS are accessed via a PageRoute. The default route that is used in RockChMS
    /// is /page/{pageId} (for example /page/113 tells Rock to load the <see cref="Rock.Model.Page"/> associated with PageId 113). This model allows
    /// for custom page routes to be created, which in turn allows us to use Friendlier Urls (for example the default New Account page can be accessed by /NewAccount 
    /// as well as /page/4).
    /// </summary>
    [Table( "PageRoute" )]
    [DataContract]
    public partial class PageRoute : Model<PageRoute>
    {
        /// <summary>
        /// Gets or sets a flag indicating if the PageRoute is part of of the RockChMS core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if the PageRoute is part of the RockChMS core system/framework, otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }
        
        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Page"/> that the PageRoute is linked to. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> containing the Id of the <see cref="Rock.Model.Page"/> that the PageRoute is linked to.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int PageId { get; set; }

        /// <summary>
        /// Gets or sets the format of the route path. Route examples include: Page <example>NewAccount</example> or <example>Checkin/Welcome</example>. 
        /// A specific group <example>Group/{GroupId} (i.e. Group/16)</example>. A person's history <example>Person/{PersonId}/History (i.e. Person/12/History)</example>.
        /// This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing the format of the RoutePath.
        /// </value>
        [Route]
        [Required]
        [MaxLength( 200 )]
        [DataMember( IsRequired = true )]
        public string Route { get; set; }
        
        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Page"/> associated with the RoutePath.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Page"/> that is associated with the RoutePath.
        /// </value>
        [DataMember]
        public virtual Page Page { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> containing the Route and represents this PageRoute
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> containing the Route and represents this instance.
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

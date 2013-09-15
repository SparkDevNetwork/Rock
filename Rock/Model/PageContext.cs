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

using Newtonsoft.Json;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a PageContext object in RockChMS.  A PageContext entity is an entity object that can be shared amongst all of the <see cref="Rock.Model.Block">Blocks</see> on a page.
    /// </summary>
    /// <remarks>
    /// A good example of this is a <see cref="Rock.Model.Person"/> that is shared amongst all of the <see cref="Rock.Model.Block">Blocks</see> on the Person Detail Page.
    /// </remarks>
    [Table( "PageContext" )]
    [DataContract]
    public partial class PageContext : Model<PageContext>
    {
        /// <summary>
        /// Gets or sets a flag indicating if this PageContext is a part of the RockChMS core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if the PageContext is part of the core system/framework, otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }
        
        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Page"/> that this PageContext is used on. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> that represents the Id of the <see cref="Rock.Model.Page"/> that this PageContext is used on.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int PageId { get; set; }
        
        /// <summary>
        /// Gets or sets the object type name of the entity object that is being shared through this PageContext. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the object type name of the entity object that is being shared through the PageContext.
        /// </value>
        [Required]
        [MaxLength( 200 )]
        [DataMember( IsRequired = true )]
        public string Entity { get; set; }

        /// <summary>
        /// Gets or sets the name of the Page Attribute/Parameter that stores the Id of the shared entity object. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing the name of the Page Attribute/Parameter storing the Id of the entity object. 
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string IdParameter { get; set; }

        /// <summary>
        /// Gets or sets the date and time that the PageContext was created.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> containing the date and time that the PageContext was created.
        /// </value>
        [DataMember]
        public DateTime? CreatedDateTime { get; set; }
        
        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Page"/> that this PageContext is used on.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Page"/> that uses this PageContext.
        /// </value>
        [DataMember]
        public virtual Page Page { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> containing the Entity (type name) and IdParamenter that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" />  containing the Entity (type name) and IdParamenter that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format( "{0}:{1}", this.Entity, this.IdParameter );
        }

    }

    /// <summary>
    /// Page Route Configuration class.
    /// </summary>
    public partial class PageContextConfiguration : EntityTypeConfiguration<PageContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PageContextConfiguration"/> class.
        /// </summary>
        public PageContextConfiguration()
        {
            this.HasRequired( p => p.Page ).WithMany( p => p.PageContexts ).HasForeignKey( p => p.PageId ).WillCascadeOnDelete(true);
        }
    }
}

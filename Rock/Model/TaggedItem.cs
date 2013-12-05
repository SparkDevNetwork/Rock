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
    /// Represents an entity object that belongs to a Tag. The same entity object can belong to multiple tags.
    /// </summary>
    [Table( "TaggedItem" )]
    [DataContract]
    public partial class TaggedItem : Model<TaggedItem>
    {
        /// <summary>
        /// Gets or sets a flag indicating if this TaggedItem is part of the RockChMS core system/framework.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if this TaggedItem is part of the RockChMS core system/framework; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }
        
        /// <summary>
        /// Gets or sets the TagId of the <see cref="Rock.Model.Tag"/> that this TaggedItem is tagged with.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the TagId of the <see cref="Rock.Model.Tag"/> that this TaggedItem is tagged with.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int TagId { get; set; }
        
        /// <summary>
        /// Gets or sets the GUID identifier of the tagged entity.
        /// </summary>
        /// <value>
        /// A <see cref="System.Guid"/> representing the GUID identifier of the tagged entity.
        /// </value>
        [DataMember]
        public Guid EntityGuid { get; set; }
        
        /// <summary>
        /// Gets or sets the Tag that this TaggedItem belongs to.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Tag"/> that this TaggedItem belongs to.
        /// </value>
        [DataMember]
        public virtual Tag Tag { get; set; }

        /// <summary>
        /// Gets the parent security authority for this TagItem
        /// </summary>
        /// <value>
        /// An entity that implements the <see cref="Rock.Security.ISecured"/> interface that this TagItem inherits security authority from.
        /// </value>
        public override Security.ISecured ParentAuthority
        {
            get { return this.Tag; }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this TagItem.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this TagItem.
        /// </returns>
        public override string ToString()
        {
            return this.Tag != null ? this.Tag.Name : string.Empty;
        }
    }

    /// <summary>
    /// Attribute Value Configuration class.
    /// </summary>
    public partial class TaggedItemConfiguration : EntityTypeConfiguration<TaggedItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeValueConfiguration"/> class.
        /// </summary>
        public TaggedItemConfiguration()
        {
            this.HasRequired( p => p.Tag ).WithMany( p => p.TaggedItems ).HasForeignKey( p => p.TagId ).WillCascadeOnDelete(true);
        }
    }
}

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
using System.Runtime.Serialization;

using Newtonsoft.Json;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Block POCO Entity.
    /// </summary>
    [Table( "Block" )]
    [DataContract( IsReference = true )]
    public partial class Block : Model<Block>, IOrdered
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
        /// Gets or sets the Page Id.
        /// </summary>
        /// <value>
        /// Page Id.
        /// </value>
        [DataMember]
        public int? PageId { get; set; }
        
        /// <summary>
        /// Gets or sets the Layout.
        /// </summary>
        /// <value>
        /// Layout.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string Layout { get; set; }
        
        /// <summary>
        /// Gets or sets the Block Type Id.
        /// </summary>
        /// <value>
        /// Block Type Id.
        /// </value>
        [Required]
        [DataMember]
        public int BlockTypeId { get; set; }
        
        /// <summary>
        /// Gets or sets the Zone.
        /// </summary>
        /// <value>
        /// Zone.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember]
        public string Zone { get; set; }
        
        /// <summary>
        /// Gets or sets the Order.
        /// </summary>
        /// <value>
        /// Order.
        /// </value>
        [Required]
        [DataMember]
        public int Order { get; set; }
        
        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        /// <value>
        /// Name.
        /// </value>
        [MaxLength( 100 )]
        [TrackChanges]
        [Required( ErrorMessage = "Name is required" )]
        [DataMember]
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the Output Cache Duration.
        /// </summary>
        /// <value>
        /// Output Cache Duration.
        /// </value>
        [Required]
        [DataMember]
        public int OutputCacheDuration { get; set; }
        
        /// <summary>
        /// Gets or sets the Block Type.
        /// </summary>
        /// <value>
        /// A <see cref="BlockType"/> object.
        /// </value>
        [DataMember]
        public virtual BlockType BlockType { get; set; }

        /// <summary>
        /// Gets or sets the Page.
        /// </summary>
        /// <value>
        /// A <see cref="Page"/> object.
        /// </value>
        [DataMember]
        public virtual Page Page { get; set; }

        /// <summary>
        /// Gets the block location.
        /// </summary>
        /// <value>
        /// The block location.
        /// </value>
        public virtual BlockLocation BlockLocation
        {
            get { return this.PageId.HasValue ? BlockLocation.Page : BlockLocation.Layout; }
        }

        /// <summary>
        /// Gets the supported actions.
        /// </summary>
        public override List<string> SupportedActions
        {
            get { return new List<string>() { "View", "Edit", "Administrate" }; }
        }

        /// <summary>
        /// Gets the parent authority.
        /// </summary>
        /// <value>
        /// The parent authority.
        /// </value>
        public override Security.ISecured ParentAuthority
        {
            get
            {
                return this.Page;
            }
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

    }

    /// <summary>
    /// Block Instance Configuration class.
    /// </summary>
    public partial class BlockConfiguration : EntityTypeConfiguration<Block>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlockConfiguration"/> class.
        /// </summary>
        public BlockConfiguration()
        {
            this.HasRequired( p => p.BlockType ).WithMany( p => p.Blocks ).HasForeignKey( p => p.BlockTypeId ).WillCascadeOnDelete( true );
            this.HasOptional( p => p.Page ).WithMany( p => p.Blocks ).HasForeignKey( p => p.PageId ).WillCascadeOnDelete( true );
        }
    }

    /// <summary>
    /// The location of the block 
    /// </summary>
    [Serializable]
    public enum BlockLocation
    {
        /// <summary>
        /// Block is located in the layout (will be rendered for every page using the layout)
        /// </summary>
        Layout,

        /// <summary>
        /// Block is located on the page
        /// </summary>
        Page,
    }

}

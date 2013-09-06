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
    /// Represents an implementation of a <see cref="BlockType"/> in RockChMS. A block can be implemented on a Site's template layout and appear on 
    /// all pages on the site that uses that template or on an individual page.  
    /// 
    /// An example of a Block being implemented on a layout template would be an implementation of a HTML Content Block Type in the footer zone of a layout that contains the site's copyright notice.  
    /// This Block will show on all <see cref="Page">pages</see> of the <see cref="Site"/> that uses the layout.
    /// 
    /// An example of a Block being implemented on a page would be the New Account <see cref="BlockType"/> being implemented on the New Account page.
    /// </summary>
    [Table( "Block" )]
    [DataContract]
    public partial class Block : Model<Block>, IOrdered
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if this Block was created by and is a part of the RockChMS core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if this Block is part of the RockChMS core system/framework, otherwise is <c>false</c>.
        /// </value>
        /// <example>
        /// True
        /// </example>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Id <see cref="Page"/> that this Block belongs to. This property is nullable and will only be populated
        /// if the Block is implemented on a <see cref="Page"/>.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int"/> that represents the Id of the <see cref="Page"/> that this Block is implemented on.  This value will be null if this Block is implemented 
        /// as part of a <see cref="Site"/> layout template.
        /// </value>
        /// <example>
        /// 1
        /// </example>
        [DataMember]
        public int? PageId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Site"/> that this Block is implemented on. This property is nullable and will only be populated
        /// if the Block is implmeneted as part of a <see cref="Site"/> layout template, and is used in conjunction with the Layout name property.
        /// </summary>
        /// <value>
        /// An <see cref="Integer"/> that represents the Id of the <see cref="Site"/> that this Block is implmeneted on. This value will be null if this Block is implemented on a page.
        /// </value>
        /// <example>
        ///  1 
        /// </example>
        [DataMember]
        public int? SiteId { get; set; }

        /// <summary>
        /// Gets or sets the name of the layout template that this Block is implemented on, when a block is a part of a site/layout template. This property
        /// is used in conjunction with the SiteId, and will only be populated when a Block is implemented as part of a site/layout template.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the name of the Layout that this Block is implmented on. This value will be null if the Block is implmented as part of a page.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string Layout { get; set; }
        
        /// <summary>
        /// Gets or sets the Id of the <see cref="BlockType"/> that this Block is implementing. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int"/> that represents the <see cref="BlockType"/> that this Block is implementing.
        /// </value>
        /// <example>
        /// 4
        /// </example>
        [Required]
        [DataMember( IsRequired = true )]
        public int BlockTypeId { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the layout zone/section that this Block is being implemented on. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the name of the Zone that this Block is implmeneted on.
        /// </value>
        /// <example>
        /// Content
        /// </example>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Zone { get; set; }
        
        /// <summary>
        /// Gets or sets the order that this Block appears in the <see cref="Page"/>/Layout zone that the Block is implmented in.  Blocks are 
        /// displayed/rendered in Ascending (1,2,3,...) order. The lower the number the higher in the Zone the Block will appear.  <see cref="Page"/> Blocks have
        /// priority over <see cref="Site"/> layout Blocks, so they will appear higher in the Zone than <see cref="Site"/>/Layout Blocks. This property is required
        /// </summary>
        /// <value>
        /// A <see cref="System.Int"/> that represents the display order of the Block in a <see cref="Page"/>/Layout Zone.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int Order { get; set; }
        
        /// <summary>
        /// Gets or sets a user defined name of the block implementation. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents a user defined name for the Block.
        /// </value>
        [MaxLength( 100 )]
        [Required( ErrorMessage = "Name is required" )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the length of time (in minutes) that the Block's data is cached. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int"/> that represents the amout of time that the Block's data is cached.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int OutputCacheDuration { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="BlockType"/> entity that this Block is implementing.
        /// </summary>
        /// <value>
        /// The <see cref="BlockType"/> that that is being implmeneted by this Block.
        /// </value>
        [DataMember]
        public virtual BlockType BlockType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Page"/> entity that this Block is implemented on. This property will be null if this Block is being implmeneted on as part of a Site Layout.
        /// </summary>
        /// <value>
        /// The <see cref="Page"/> entity that this Blcok is being implmeneted on. This value will be null if the Block is implemented as part of a <see cref="Site"/> Layout.
        /// </value>
        [DataMember]
        public virtual Page Page { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Site"/> that this Layout Block is being implemented on. This property will be null if this Block is implemented as part of a <see cref="Page"/>.
        /// </summary>
        /// <value>The <see cref="Site"/> that this Layout Block is being implemented on. This property will be null if this Block is being implmented as part a <see cref="Page"/> </value>
        [DataMember]
        public virtual Site Site { get; set; }

        /// <summary>
        /// Gets the location where this Block is being implmeneted on.  
        /// </summary>
        /// <value>
        /// The location where this Block is being implemented on. <c>BlockLocation.Page</c> if the PageId is not null, otherwise <c>BlockLocation.Layout</c>
        /// </value>
        /// <example>
        /// <c>BlockLocation.Page</c>
        /// </example>
        public virtual BlockLocation BlockLocation
        {
            get { return this.PageId.HasValue ? BlockLocation.Page : BlockLocation.Layout; }
        }

        /// <summary>
        /// Gets the securiable object that security permissions should be inherited from.  If block is located on a page
        /// security will be inheritied from the page, otherwise it will be inherited from the site.
        /// </summary>
        /// <value>
        /// The parent authority. If the block is located on the page, security will be
        /// inherited from the page, otherwise it will be inherited from the site.
        /// </value>
        public override Security.ISecured ParentAuthority
        {
            get
            {
                if ( this.BlockLocation == Model.BlockLocation.Page )
                {
                    return this.Page;
                }
                else
                {
                    return this.Site;
                }
            }
        }

        #endregion

        #region Public Methods
        
        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents this instance. Returns the Name of the Block
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion

    }

    #region Entity Configuration

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
            this.HasOptional( p => p.Site ).WithMany( ).HasForeignKey( p => p.SiteId ).WillCascadeOnDelete( true );
        }
    }

    #endregion

    #region Enumerations

    /// <summary>
    /// The location where the Block is implmeneted
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

    #endregion

}

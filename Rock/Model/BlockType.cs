//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Newtonsoft.Json;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a a configurable and functional component or module that extends the base functionality of the RockChMS system/framework. A
    /// BlockType can be implemented one or more <see cref="Page">Pages</see> or <see cref="Site"/> Layouts.
    /// </summary>
    [Table( "BlockType" )]
    [DataContract]
    public partial class BlockType : Model<BlockType>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if this BlockType was created by and is a part of the RockChMS core system/framework. This property is required.
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
        /// Gets or sets relative path to the .Net ASCX UserControl that provides the HTML Markup and code for the BlockType. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the relative path to the supporting UserControl for the BlockType.
        /// </value>
        /// <example>
        /// ~/Blocks/Security/Login.ascx
        /// </example>
        [Required]
        [MaxLength( 260 )]
        [DataMember( IsRequired = true )]
        public string Path { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the BlockType.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the Name of the BlockType. This property is required.
        /// </value>
        /// <example>
        /// Login
        /// </example>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the user defined description of the BlockType. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the Description of the BlockType
        /// </value>
        /// <example>
        /// Provides ability to login to site.
        /// </example>
        [DataMember]
        public string Description { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets a collection of  <see cref="Rock.Model.Block">Blocks</see> that are implementations of this BlockType.
        /// </summary>
        /// <value>
        /// Collection of <see cref="Rock.Model.Block">Blocks</see> that implements this BlockType.
        /// </value>
        [DataMember]
        public virtual ICollection<Block> Blocks
        {
            get { return _blocks ?? ( _blocks = new Collection<Block>() ); }
            set { _blocks = value; }
        }
        private ICollection<Block> _blocks;

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance. Returns the name of the BlockType
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// Block Type Configuration class.
    /// </summary>
    public partial class BlockTypeConfiguration : EntityTypeConfiguration<BlockType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlockTypeConfiguration"/> class.
        /// </summary>
        public BlockTypeConfiguration()
        {
        }
    }

    #endregion

}

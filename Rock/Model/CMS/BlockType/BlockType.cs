// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Newtonsoft.Json;

using Rock.Attribute;
using Rock.Data;
using Rock.Web.Cache;
using Rock.Enums.Cms;

namespace Rock.Model
{
    /// <summary>
    /// Represents a configurable and functional component or module that extends the base functionality of the Rock system/framework. A
    /// BlockType can be implemented one or more <see cref="Page">Pages</see> or <see cref="Layout">Layouts</see>.
    /// </summary>
    [RockDomain( "CMS" )]
    [Table( "BlockType" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "04768EDF-C0CD-4950-B629-4D2370B57C99" )]
    public partial class BlockType : Model<BlockType>, ICacheable
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if this BlockType was created by and is a part of the Rock core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if this Block is part of the Rock core system/framework, otherwise is <c>false</c>.
        /// </value>
        /// <example>
        /// True
        /// </example>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this blocktype is commonly used
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is common; otherwise, <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsCommon { get; set; }

        /// <summary>
        /// Gets or sets relative path to the .Net ASCX UserControl that provides the HTML Markup and code for the BlockType.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the relative path to the supporting UserControl for the BlockType.
        /// </value>
        /// <example>
        /// ~/Blocks/Security/Login.ascx
        /// </example>
        [MaxLength( 260 )]
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the entity type identifier for the pre-compiled class that provides the logic for this block type.
        /// </summary>
        /// <value>
        /// The entity type identifier for the pre-compiled class that provides the logic for this block type.
        /// </value>
        public int? EntityTypeId { get; set; }

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
        /// Gets or sets the category of the BlockType.  Blocks will be grouped by category when displayed to user
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the category of the BlockType.
        /// </value>
        /// <example>
        /// Security
        /// </example>
        [MaxLength( 100 )]
        [DataMember]
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the user defined description of the BlockType.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the Description of the BlockType
        /// </value>
        /// <example>
        /// Provides ability to log into the site.
        /// </example>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// The list of <see cref="SiteType" /> this block type could be a part of.
        /// </summary>
        [RockInternal( "1.16.6" )]
        public SiteTypeFlags SiteTypeFlags { get; set; } = SiteTypeFlags.None;

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets a collection of  <see cref="Rock.Model.Block">Blocks</see> that are implementations of this BlockType.
        /// </summary>
        /// <value>
        /// Collection of <see cref="Rock.Model.Block">Blocks</see> that implements this BlockType.
        /// </value>
        public virtual ICollection<Block> Blocks
        {
            get { return _blocks ?? ( _blocks = new Collection<Block>() ); }
            set { _blocks = value; }
        }

        private ICollection<Block> _blocks;

        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        public virtual EntityType EntityType { get; set; }

        #endregion Navigation Properties

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

        #endregion Public Methods
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
            this.HasOptional( b => b.EntityType ).WithMany().HasForeignKey( b => b.EntityTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion Entity Configuration
}
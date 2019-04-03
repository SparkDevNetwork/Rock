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
//
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Used to map a site and token to a specific url 
    /// </summary>
    [RockDomain( "CMS" )]
    [Table( "LavaShortcode" )]
    [DataContract]
    public partial class LavaShortcode : Model<LavaShortcode>, ICacheable
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the public name of the shortcode.
        /// </summary>
        /// <value>
        /// The public name of the shortcode.
        /// </value>
        [Required]
        [MaxLength( 50 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Description of the Lava Shortcode.
        /// </summary>
        /// <value>
        /// The description of the shortcode. This is used as a public description.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the documentation. This serves as the technical description of the internals of the shortcode.
        /// </summary>
        /// <value>
        /// The technical description that serves as documentation.
        /// </value>
        [DataMember]
        public string Documentation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is system; otherwise, <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; }
        }
        private bool _isActive = true;

        /// <summary>
        /// Gets or sets the name of the tag.
        /// </summary>
        /// <value>
        /// The name of the tag.
        /// </value>
        [Required]
        [MaxLength( 50 )]
        [DataMember( IsRequired = true )]
        public string TagName { get; set; }

        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        /// <value>
        /// The token.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public string Markup { get; set; }

        /// <summary>
        /// Gets or sets the type of the tag (inline or block). A tag type of block requires an end tag.
        /// </summary>
        /// <value>
        /// The type of the tag.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public TagType TagType { get; set; }

        /// <summary>
        /// Gets or sets the enabled lava commands.
        /// </summary>
        /// <value>
        /// The enabled lava commands.
        /// </value>
        [MaxLength( 500 )]
        public string EnabledLavaCommands{ get; set; }

        /// <summary>
        /// Gets or sets the parameters.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        [MaxLength( 2500 )]
        public string Parameters { get; set; }
        #endregion

        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return LavaShortcodeCache.Get( this.Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            LavaShortcodeCache.UpdateCachedEntity( this.Id, entityState );
        }

        #endregion
    }

    /// <summary>
    /// Determines the type of tag (inline or block). Block type requires an end tag.
    /// </summary>
    public enum TagType
    {
        /// <summary>
        /// The inline
        /// </summary>
        Inline = 1,
        
        /// <summary>
        /// The block
        /// </summary>
        Block = 2
    }

    #region Entity Configuration

    /// <summary>
    /// Lava Shortcode Configuration class.
    /// </summary>
    public partial class LavaShortcodeConfiguration : EntityTypeConfiguration<LavaShortcode>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LavaShortcodeConfiguration"/> class.
        /// </summary>
        public LavaShortcodeConfiguration()
        {
            
        }
    }

    #endregion
}

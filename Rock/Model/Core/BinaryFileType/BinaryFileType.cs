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

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a type or category of binary files in Rock, and configures how binary files of this type are stored and accessed.
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "BinaryFileType" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "62AF597F-F193-412B-94EA-291CF713327D")]
    public partial class BinaryFileType : Model<BinaryFileType>, ICacheable
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if this BinaryFileType is part of the Rock core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this is part of the core system/framework; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the given Name of the BinaryFileType. This value is an alternate key and is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the given Name of the BinaryFileType. 
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a description of the BinaryFileType.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the description of the BinaryFileType.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the CSS class that is used for a vector/CSS icon.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the CSS class that is used for a vector/CSS based icon.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the Id of the storage service <see cref="Rock.Model.EntityType"/> that is used to store files of this type.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32" /> representing the Id of the storage service <see cref="Rock.Model.EntityType"/>.
        /// </value>
        [DataMember]
        public int? StorageEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether the file on any <see cref="Rock.Model.BinaryFile"/> child entities should be cached to the server.
        /// </summary>
        /// <value>
        ///  A <see cref="System.Boolean"/> value that is <c>true</c> if caching to the server is allowed; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool CacheToServerFileSystem { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether security should be checked when displaying files of this type
        /// </summary>
        /// <value>
        /// <c>true</c> if [requires view security]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool RequiresViewSecurity { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the maximum width of a file type.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the maximum width in pixels of a file type.
        /// </value>
        [DataMember]
        public int? MaxWidth { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the maximum height of a file type.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the maximum height in pixels of a file type.
        /// </value>
        [DataMember]
        public int? MaxHeight { get; set; }

        /// <summary>
        /// Gets or sets the preferred format of the file type.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.Format"/> enum value that represents the preferred format of the file type.
        /// </value>
        [DataMember]
        public Format PreferredFormat { get; set; }

        /// <summary>
        /// Gets or sets the preferred resolution of the file type.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.Resolution"/> enum value that represents the preferred resolution of the file type.
        /// </value>
        [DataMember]
        public Resolution PreferredResolution { get; set; }

        /// <summary>
        /// Gets or sets the preferred color depth of the file type.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.ColorDepth"/> enum value that represents the preferred color depth of the file type.
        /// </value>
        [DataMember]
        public ColorDepth PreferredColorDepth { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the preferred attributes of the file type are required
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if the "preferred" attributes are required; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool PreferredRequired { get; set; }

        private string _cacheControlHeaderSettings;
        /// <summary>
        /// Gets or sets the cache control header settings.
        /// </summary>
        /// <value>
        /// The cache control header settings.
        /// </value>
        [MaxLength( 500 )]
        [DataMember]
        public string CacheControlHeaderSettings
        {
            get => _cacheControlHeaderSettings;
            set
            {
                if ( _cacheControlHeaderSettings != value )
                {
                    _cacheControlHeader = null;
                }
                _cacheControlHeaderSettings = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum file size bytes.
        /// </summary>
        /// <value>The maximum file size bytes.</value>
        [DataMember]
        public int? MaxFileSizeBytes { get; set; }

        /// <summary>
        /// If true then the file type allows anonymous uploads.
        /// </summary>
        [DataMember]
        public bool AllowAnonymous { get; set; }

        private RockCacheability _cacheControlHeader;
       
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Person"/> class.
        /// </summary>
        public BinaryFileType()
            : base()
        {
            PreferredFormat = Format.Undefined;
            PreferredResolution = Resolution.Undefined;
            PreferredColorDepth = ColorDepth.Undefined;
        }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the storage mode <see cref="Rock.Model.EntityType"/>.
        /// </summary>
        /// <value>
        /// The storage mode <see cref="Rock.Model.EntityType"/>.
        /// </value>
        [DataMember]
        public virtual EntityType StorageEntityType { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// File Configuration class.
    /// </summary>
    public partial class BinaryFileTypeConfiguration : EntityTypeConfiguration<BinaryFileType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryFileTypeConfiguration"/> class.
        /// </summary>
        public BinaryFileTypeConfiguration()
        {
        }
    }

    #endregion
}
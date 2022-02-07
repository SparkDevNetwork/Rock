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
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;
using Rock.Lava;
using Rock.Storage;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents any file that has either been uploaded to or generated and saved to Rock.  
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "BinaryFile" )]
    [DataContract]
    public partial class BinaryFile : Model<BinaryFile>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if this is a temporary file. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this is a temporary file, otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsTemporary { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this file is part of the Rock core system/framework.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean" /> value that is <c>true</c> if this file is part of the core system/framework; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the id of the <see cref="Rock.Model.BinaryFileType"/> that this file belongs to.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the <see cref="Rock.Model.BinaryFileType"/>.
        /// </value>
        [DataMember]
        public int? BinaryFileTypeId { get; set; }

        /// <summary>
        /// Gets or sets the name of the file, including any extensions. This name is usually captured when the file is uploaded to Rock and this same name will be used when the file is downloaded. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the name of the file, including the extension.
        /// </value>
        [Required]
        [MaxLength( 255 )]
        [DataMember( IsRequired = true )]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the size of the file (in bytes)
        /// </summary>
        /// <value>
        /// The size of the file in bytes.
        /// </value>
        [DataMember]
        public long? FileSize { get; set; }

        /// <summary>
        /// Gets or sets the Mime Type for the file. This property is required
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Mime Type for the file.
        /// </value>
        [Required]
        [MaxLength( 255 )]
        [DataMember( IsRequired = true )]
        public string MimeType { get; set; }

        /// <summary>
        /// Gets or sets a user defined description of the file.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the user defined description of the file.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets the Id of the Storage Service <see cref="Rock.Model.EntityType"/> that is used to store this file.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the ID of the Storage Service <see cref="Rock.Model.EntityType"/> that is being used to store this file.
        /// </value>
        [DataMember]
        public int? StorageEntityTypeId
        {
            get
            {
                return _storageEntityTypeId;
            }

            private set
            {
                _storageEntityTypeId = value;

                StorageProvider = null;
                if ( value.HasValue )
                {
                    var entityType = EntityTypeCache.Get( value.Value );
                    if ( entityType != null )
                    {
                        StorageProvider = ProviderContainer.GetComponent( entityType.Name );
                    }
                }


            }
        }
        private int? _storageEntityTypeId = null;

        /// <summary>
        /// Gets or sets the storage entity settings.
        /// </summary>
        /// <value>
        /// The storage entity settings.
        /// </value>
        /// <remarks>
        /// Because a Storage provider's settings are stored with the binary file type, and that binary file 
        /// type may change the settings or storage provider, the setting values at the time this file was 
        /// saved need to be stored with the binary file so that the storage provider is still able to 
        /// retrieve the file using these settings
        /// </remarks>
        public string StorageEntitySettings { get; set; }

        /// <summary>
        /// Gets or sets a path to the file that is understandable by the storage provider.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        /// <remarks>
        /// It us up to the storage provider to save the path value when creating the file
        /// </remarks>
        [MaxLength( 2083 )]
        [DataMember]
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the width of a file type.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the width in pixels of a file type.
        /// </value>
        [DataMember]
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the height of a file type.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the height in pixels of a file type.
        /// </value>
        [DataMember]
        public int? Height { get; set; }

        /// <summary>
        /// Gets or sets the content last modified.
        /// </summary>
        /// <value>
        /// The content last modified.
        /// </value>
        /// <remarks>
        /// Because the Content property is not part of the EF model, changes to just the content
        /// would not mark the entity as modified when it is saved. Because the storage providers will 
        /// only get notified of new content when this model is added or changed, this property is 
        /// required in order to flag the entity as being modified whenever the content is updated.
        /// </remarks>
        [DataMember]
        public DateTime? ContentLastModified { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.BinaryFileType"/> of the file.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.BinaryFileType"/> of the file.
        /// </value>
        [DataMember]
        public virtual BinaryFileType BinaryFileType { get; set; }

        /// <summary>
        /// Gets or sets a <see cref="Rock.Model.BinaryFileData"/> that contains the content of the file. This object can be used for temporary storage or be persisted to the database.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.BinaryFileData"/> that contains the content of the file. 
        /// </value>
        [LavaVisible]
        public virtual BinaryFileData DatabaseData { get; set; }

        /// <summary>
        /// Gets or sets the document.
        /// </summary>
        /// <value>
        /// The document.
        /// </value>
        [DataMember]
        public virtual Document Document { get; set; }

        #endregion

        /// <summary>
        /// Returns a <see cref="System.String" /> containing the name of the file and  represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> containing the name of the file and represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.FileName;
        }
    }

    #region Entity Configuration

    /// <summary>
    /// File Configuration class.
    /// </summary>
    public partial class BinaryFileConfiguration : EntityTypeConfiguration<BinaryFile>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryFileConfiguration"/> class.
        /// </summary>
        public BinaryFileConfiguration()
        {
            this.HasRequired( f => f.BinaryFileType ).WithMany().HasForeignKey( f => f.BinaryFileTypeId ).WillCascadeOnDelete( false );
            this.HasOptional( f => f.DatabaseData ).WithRequired().WillCascadeOnDelete();
        }
    }

    #endregion
}

// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.IO;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Storage;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents any file that has either been uploaded to or generated and saved to Rock.  
    /// </summary>
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
                    var entityType = EntityTypeCache.Read( value.Value );
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

        #region Virtual Properties

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
        public virtual BinaryFileData DatabaseData { get; set; }

        /// <summary>
        /// Gets the storage provider.
        /// </summary>
        /// <value>
        /// The storage provider.
        /// </value>
        [NotMapped]
        public virtual Storage.ProviderComponent StorageProvider { get; private set; }

        /// <summary>
        /// Gets the URL.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        [NotMapped]
        [DataMember]
        public virtual string Url
        {
            get
            {
                if ( StorageProvider != null )
                {
                    return StorageProvider.GetUrl( this );
                }
                else
                {
                    return Path;
                }
            }
            private set { }
        }

        /// <summary>
        /// Gets or sets the content stream.
        /// </summary>
        /// <value>
        /// The content stream.
        /// </value>
        [NotMapped]
        [HideFromReporting]
        public virtual Stream ContentStream
        {
            get
            {
                if ( _stream == null )
                {
                    if ( StorageProvider != null )
                    {
                        _stream = StorageProvider.GetContentStream( this );
                    }
                }
                else
                {
                    if ( _stream.CanSeek )
                    {
                        _stream.Position = 0;
                    }
                }

                return _stream;
            }
            set
            {
                _stream = value;
                _contentIsDirty = true;
                ContentLastModified = RockDateTime.Now;
            }
        }
        private Stream _stream;
        private bool _contentIsDirty = false;

        /// <summary>
        /// Gets the storage settings.
        /// </summary>
        /// <value>
        /// The storage settings.
        /// </value>
        [NotMapped]
        [HideFromReporting]
        public virtual Dictionary<string,string> StorageSettings
        {
            get
            {
                return StorageEntitySettings.FromJsonOrNull<Dictionary<string, string>>() ?? new Dictionary<string, string>();
            }
        }

        #endregion

        #region Methods


        /// <summary>
        /// Sets the type of the storage entity.
        /// Should only be set by the BinaryFileService
        /// </summary>
        /// <param name="storageEntityTypeId">The storage entity type identifier.</param>
        public void SetStorageEntityTypeId( int? storageEntityTypeId )
        {
            StorageEntityTypeId = storageEntityTypeId;
        }

        /// <summary>
        /// Pres the save.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entry">The entry.</param>
        public override void PreSaveChanges( DbContext dbContext, System.Data.Entity.Infrastructure.DbEntityEntry entry )
        {
            if ( entry.State == System.Data.Entity.EntityState.Deleted )
            {
                if ( StorageProvider != null )
                {
                    this.BinaryFileTypeId = entry.OriginalValues["BinaryFileTypeId"].ToString().AsInteger();
                    StorageProvider.DeleteContent( this );
                    this.BinaryFileTypeId = null;
                }
            }
            else
            {
                if ( BinaryFileType == null && BinaryFileTypeId.HasValue )
                {
                    BinaryFileType = new BinaryFileTypeService( (RockContext)dbContext ).Get( BinaryFileTypeId.Value );
                }

                if ( entry.State == System.Data.Entity.EntityState.Added )
                {
                    // when a file is saved (unless it is getting Deleted/Saved), it should use the StoredEntityType that is associated with the BinaryFileType
                    if ( BinaryFileType != null )
                    {
                        // Persist the storage type
                        StorageEntityTypeId = BinaryFileType.StorageEntityTypeId;

                        // Persist the storage type's settings specific to this binary file type
                        var settings = new Dictionary<string, string>();
                        if ( BinaryFileType.Attributes == null )
                        {
                            BinaryFileType.LoadAttributes();
                        }
                        foreach ( var attributeValue in BinaryFileType.AttributeValues )
                        {
                            settings.Add( attributeValue.Key, attributeValue.Value.Value );
                        }
                        StorageEntitySettings = settings.ToJson();

                        if ( StorageProvider != null )
                        {
                            // save the file to the provider's new storage medium
                            StorageProvider.SaveContent( this );
                            Path = StorageProvider.GetPath( this );
                        }
                    }
                }

                else if ( entry.State == System.Data.Entity.EntityState.Modified )
                {
                    // when a file is saved (unless it is getting Deleted/Added), 
                    // it should use the StorageEntityType that is associated with the BinaryFileType
                    if ( BinaryFileType != null )
                    {
                        // if the storage provider changed, or any of its settings specific 
                        // to the binary file type changed, delete the original provider's content
                        if ( StorageEntityTypeId.HasValue && BinaryFileType.StorageEntityTypeId.HasValue )
                        {
                            var settings = new Dictionary<string, string>();
                            if ( BinaryFileType.Attributes == null )
                            {
                                BinaryFileType.LoadAttributes();
                            }
                            foreach ( var attributeValue in BinaryFileType.AttributeValues )
                            {
                                settings.Add( attributeValue.Key, attributeValue.Value.Value );
                            }
                            string settingsJson = settings.ToJson();

                            if ( StorageProvider != null && (
                                StorageEntityTypeId.Value != BinaryFileType.StorageEntityTypeId.Value ||
                                StorageEntitySettings != settingsJson ) )
                            {
                                // Delete the current provider's storage
                                StorageProvider.DeleteContent( this );

                                // Set the new storage provider with its settings
                                StorageEntityTypeId = BinaryFileType.StorageEntityTypeId;
                                StorageEntitySettings = settingsJson;
                            }
                        }
                    }

                    if ( _contentIsDirty && StorageProvider != null )
                    {
                        StorageProvider.SaveContent( this );
                        Path = StorageProvider.GetPath( this );
                    }
                }
            }
        }

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

        /// <summary>
        /// Reads the file's content stream and converts to a string.
        /// </summary>
        /// <returns></returns>
        public string ContentsToString()
        {
            string contents = string.Empty;

            using ( var stream = this.ContentStream )
            {
                using ( var reader = new StreamReader( stream ) )
                {
                    contents = reader.ReadToEnd();
                }
            }

            return contents;
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
                return this.BinaryFileType != null ? this.BinaryFileType : base.ParentAuthority;
            }
        }

        #endregion
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
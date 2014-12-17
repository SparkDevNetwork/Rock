﻿// <copyright>
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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;

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
        public int? StorageEntityTypeId { get; private set; }

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
        public virtual BinaryFileData Data { get; set; }

        /// <summary>
        /// Gets or sets the Storage Service <see cref="Rock.Model.EntityType"/>
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.EntityType"/> representing the Storage Service that is being used.
        /// </value>
        [DataMember]
        public virtual EntityType StorageEntityType { get; set; }

        /// <summary>
        /// Gets the URL that can be used to retrieve the file. A prefix of "~" represents the Application Root path
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        [DataMember]
        public virtual string Url
        {
            get
            {
                Rock.Storage.ProviderComponent storageProvider = BinaryFileService.DetermineBinaryFileStorageProvider( new RockContext(), this );
                if (storageProvider != null)
                {
                    return storageProvider.GenerateUrl( this );
                }

                return null;
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
        /// <param name="state">The state.</param>
        public override void PreSaveChanges( DbContext dbContext, System.Data.Entity.EntityState state )
        {
            if ( state == System.Data.Entity.EntityState.Deleted )
            {
                Rock.Storage.ProviderComponent storageProvider = BinaryFileService.DetermineBinaryFileStorageProvider( (Rock.Data.RockContext)dbContext, this );
                if ( storageProvider != null )
                {
                    storageProvider.RemoveFile( this, System.Web.HttpContext.Current );
                }
            }
            else if ( state == System.Data.Entity.EntityState.Added )
            {
                // when a file is saved (unless it is getting Deleted/Saved), it should use the StoredEntityType that is associated with the BinaryFileType
                if ( BinaryFileType != null )
                {
                    SetStorageEntityTypeId( BinaryFileType.StorageEntityTypeId );

                    var storageProvider = BinaryFileService.DetermineBinaryFileStorageProvider( (Rock.Data.RockContext)dbContext, this );

                    if ( storageProvider != null )
                    {
                        // save the file to the provider's new storage medium
                        storageProvider.SaveFile( this, System.Web.HttpContext.Current );
                    }
                }
            }
            else if ( state == System.Data.Entity.EntityState.Modified )
            {
                // when a file is saved (unless it is getting Deleted/Saved), it should use the StorageEntityType that is associated with the BinaryFileType
                if ( BinaryFileType != null )
                {
                    if ( !this.StorageEntityTypeId.HasValue )
                    {
                        SetStorageEntityTypeId( BinaryFileType.StorageEntityTypeId );
                    }

                    // if the storage provider changed, read it from the old provider and write it to the new one, then remove it from the old provider
                    if ( this.StorageEntityTypeId != BinaryFileType.StorageEntityTypeId )
                    {
                        Rock.Storage.ProviderComponent origStorageProvider = BinaryFileService.DetermineBinaryFileStorageProvider( (Rock.Data.RockContext)dbContext, this );
                        SetStorageEntityTypeId( BinaryFileType.StorageEntityTypeId );
                        Rock.Storage.ProviderComponent newStorageProvider = BinaryFileService.DetermineBinaryFileStorageProvider( (Rock.Data.RockContext)dbContext, this );
                        using ( var origStream = origStorageProvider.GetFileContentStream( this, System.Web.HttpContext.Current ) )
                        {
                            newStorageProvider.SaveFile( this, System.Web.HttpContext.Current );
                            try
                            {
                                // try to delete it from the old provider
                                origStorageProvider.RemoveFile( this, System.Web.HttpContext.Current );
                            }
                            catch ( Exception ex )
                            {
                                ExceptionLogService.LogException( ex, System.Web.HttpContext.Current );
                            }
                        }
                    }
                    else
                    {
                        // storage provider didn't change, so just save the file
                        Rock.Storage.ProviderComponent storageProvider = BinaryFileService.DetermineBinaryFileStorageProvider( (Rock.Data.RockContext)dbContext, this );
                        storageProvider.SaveFile( this, System.Web.HttpContext.Current );
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
        /// Gets the parent authority.
        /// </summary>
        /// <value>
        /// The parent authority.
        /// </value>
        public override Security.ISecured ParentAuthority
        {
            get
            {
                return this.BinaryFileType;
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
            this.HasOptional( f => f.Data ).WithRequired().WillCascadeOnDelete();
        }
    }

    #endregion
}
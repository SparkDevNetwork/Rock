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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Storage;
using Rock.Data;
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
        /// Gets or sets the Url to access the file.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Url to the file.
        /// </value>
        [MaxLength( 255 )]
        [DataMember]
        public string Url { get; set; }
        
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

        /// <summary>
        /// Sets the type of the storage entity.
        /// Should only be set by the BinaryFileService
        /// </summary>
        /// <param name="storageEntityTypeId">The storage entity type identifier.</param>
        public void SetStorageEntityTypeId( int? storageEntityTypeId)
        {
            StorageEntityTypeId = storageEntityTypeId;
        }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.BinaryFileType"/> of the file.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.BinaryFileType"/> of the file.
        /// </value>
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
        public virtual EntityType StorageEntityType { get; set; }

        #endregion

        #region Methods

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
    
        #endregion

        #region StaticMethods

        /// <summary>
        /// Makes the <see cref="Rock.Model.BinaryFile">BinaryFiles</see> represented in the comma delimited list of BinaryFileIds permanent.
        /// </summary>
        /// <param name="commaDelimitedIds">A <see cref="System.String"/> containing a comma delimited list of BinaryFileIDs representing the <see cref="Rock.Model.BinaryFile">BinaryFiles</see> that are to be marked as permanent.</param>
        public static void MakePermanent( string commaDelimitedIds )
        {
            string query = string.Format( "UPDATE BinaryFile SET IsTemporary = 0 WHERE Id IN ({0})", commaDelimitedIds );
            var service = new Service();
            service.ExecuteCommand( query );
        }

        /// <summary>
        /// Makes the <see cref="Rock.Model.BinaryFile"/> represented by the provided BinaryFileId permanent.
        /// </summary>
        /// <param name="id">A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.BinaryFile"/> to be made permanent.</param>
        public static void MakePermanent( int id)
        {
            string query = string.Format("UPDATE BinaryFile SET IsTemporary = 0 WHERE Id = {0}", id);
            var service = new Service();
            service.ExecuteCommand( query );
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

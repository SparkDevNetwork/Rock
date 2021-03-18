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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a type or category of docuemnt in Rock.
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "DocumentType" )]
    [DataContract]
    public partial class DocumentType : Model<DocumentType>, IOrdered, ICacheable
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if this DocumentType is part of the Rock core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this is part of the core system/framework; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the given Name of the DocumentType.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the given Name of the BinaryFileType. 
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.EntityType"/> that this DocumentType is used for.  A DocumentType can only be associated with a single <see cref="Rock.Model.EntityType"/> and will 
        /// only contain notes for entities of this type. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.EntityType"/>
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the name of the qualifier column/property on the <see cref="Rock.Model.EntityType"/> that this Docuement Type applies to. If this is not 
        /// provided, the document type can be used on all entities of the provided <see cref="Rock.Model.EntityType"/>.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the name of the qualifier column that this DocumentType applies to.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string EntityTypeQualifierColumn { get; set; }

        /// <summary>
        /// Gets or sets the qualifier value in the qualifier column that this document type applies to.  For instance this note type and related notes will only be applicable to entity 
        /// if the value in the EntityTypeQualiferColumn matches this value. This property should not be populated without also populating the EntityTypeQualifierColumn property.
        /// </summary>
        /// <value>
        /// Entity Type Qualifier Value.
        /// </value>
        [MaxLength( 200 )]
        [DataMember]
        public string EntityTypeQualifierValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the type is user selectable.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [user selectable]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool UserSelectable { get; set; }

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
        /// Gets or sets the display order of this DocumentType.  The lower the number the higher the display priority.  This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> that represents the display order of this DocumentType.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the id of the <see cref="Rock.Model.BinaryFileType"/> that this document type belongs to.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the <see cref="Rock.Model.BinaryFileType"/>.
        /// </value>
        [DataMember]
        public int BinaryFileTypeId { get; set; }

        /// <summary>
        /// Gets or sets the default document name template.
        /// </summary>
        /// <value>
        /// The default document name template.
        /// </value>
        [DataMember]
        public string DefaultDocumentNameTemplate { get; set; }

        /// <summary>
        /// Gets or sets the maximum documents per entity.  This would limit the documents of that type per entity. A blank value means no limit.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> that represents the maximum documents per entity.
        /// </value>
        [DataMember]
        public int? MaxDocumentsPerEntity { get; set; }

        /// <summary>
        /// Gets or sets the IsImage flag for the <see cref="Rock.Model.DocumentType"/>.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> for the IsImage flag.
        /// </value>
        [Required]
        [DataMember]
        public bool IsImage { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentType"/> class.
        /// </summary>
        public DocumentType()
            : base()
        {
        }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.EntityType"/> of the entities that <see cref="Rock.Model.Document">Notes</see> of this DocumentType 
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.EntityType"/> that this DocumentType is associated with.
        /// </value>
        [DataMember]
        public virtual EntityType EntityType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.BinaryFileType"/> of the document type.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.BinaryFileType"/> of the document type.
        /// </value>
        [DataMember]
        public virtual BinaryFileType BinaryFileType { get; set; }

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

        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return DocumentTypeCache.Get( this.Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            DocumentTypeCache.UpdateCachedEntity( this.Id, entityState );
            DocumentTypeCache.RemoveEntityDocumentTypes();
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// DocumentType Configuration class.
    /// </summary>
    public partial class DocumentTypeConfiguration : EntityTypeConfiguration<DocumentType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentTypeConfiguration"/> class.
        /// </summary>
        public DocumentTypeConfiguration()
        {
            this.HasRequired( p => p.EntityType ).WithMany().HasForeignKey( p => p.EntityTypeId ).WillCascadeOnDelete( false );
            this.HasRequired( r => r.BinaryFileType ).WithMany().HasForeignKey( r => r.BinaryFileTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}

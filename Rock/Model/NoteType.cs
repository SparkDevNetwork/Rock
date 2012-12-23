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

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Note Type POCO Entity.
    /// </summary>
    [Table( "NoteType" )]
    [DataContract( IsReference = true )]
    public partial class NoteType : Model<NoteType>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the System.
        /// </summary>
        /// <value>
        /// System.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Entity Type Id.
        /// </summary>
        /// <value>
        /// Entity Type Id.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        /// <value>
        /// Name.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the sources defined type id.
        /// </summary>
        /// <value>
        /// The sources defined type id.
        /// </value>
        [DataMember]
        public int? SourcesTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Entity Type Qualifier Column.
        /// </summary>
        /// <value>
        /// Entity Qualifier Column.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string EntityTypeQualifierColumn { get; set; }

        /// <summary>
        /// Gets or sets the Entity Type Qualifier Value.
        /// </summary>
        /// <value>
        /// Entity Type Qualifier Value.
        /// </value>
        [MaxLength( 200 )]
        [DataMember]
        public string EntityTypeQualifierValue { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the sources that can be used for this note type.
        /// </summary>
        /// <value>
        /// The sources.
        /// </value>
        [DataMember]
        public virtual DefinedType Sources { get; set; }

        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        [DataMember]
        public virtual EntityType EntityType { get; set; }

        /// <summary>
        /// Gets or sets the Attribute Qualifiers.
        /// </summary>
        /// <value>
        /// Collection of Attribute Qualifiers.
        /// </value>
        public virtual ICollection<Note> Notes { get; set; }

        #endregion

        #region Public Methods

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
    /// Note Type Configuration class.
    /// </summary>
    public partial class NoteTypeConfiguration : EntityTypeConfiguration<NoteType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NoteTypeConfiguration"/> class.
        /// </summary>
        public NoteTypeConfiguration()
        {
            this.HasRequired( p => p.EntityType ).WithMany().HasForeignKey( p => p.EntityTypeId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.Sources ).WithMany().HasForeignKey( p => p.SourcesTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

}

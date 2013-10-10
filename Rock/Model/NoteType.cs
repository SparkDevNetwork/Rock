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
    /// Represents a type or category of <see cref="Rock.Model.Note">Notes</see> in RockChMS, and configures the type of entities that notes of this type apply to other settings
    /// specific to the type of note.
    /// </summary>
    [Table( "NoteType" )]
    [DataContract]
    public partial class NoteType : Model<NoteType>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating that this NoteType is part of the RockChMS core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this NoteType is part of the core system/framework, otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.EntityType"/> that this NoteType is used for.  A NoteType can only be associated with a single <see cref="Rock.Model.EntityType"/> and will 
        /// only contain notes for entities of this type. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.MOdel.EntityType"/>
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Name of the NoteType. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Name of the NoteType.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Sources Type <see cref="System.Rock.DefinedType" /> that defines the sources that can be used
        /// for <see cref="Rock.Model.Note">Notes</see> of this NoteType.
        /// </summary>
        /// <value>
        /// The sources defined type id.
        /// </value>
        [DataMember]
        public int? SourcesTypeId { get; set; }

        /// <summary>
        /// Gets or sets the name of the qualifier column/property on the <see cref="Rock.Model.EntityType"/> that this NoteType applies to. If this is not 
        /// provided, the note type can be used on all entities of the provided <see cref="Rock.Model.EntityType"/>.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the name of the qualifier column that this NoteType applies to.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string EntityTypeQualifierColumn { get; set; }

        /// <summary>
        /// Gets or sets the qualifier value in the qualifier column that this note type applies to.  For instance this note type and related notes will only be applicable to entity 
        /// if the value in the EntityTypeQualiferColumn matches this value. This property should not be populated without also populating the EntityTypeQualifierColumn property.
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
        /// Gets or sets the Sources <see cref="Rock.Model.DefinedType"/> that contain the sources that are applicable to this NoteType.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.DefinedValue"/> that contains the sources that are applicable to this NoteType.
        /// </value>
        [DataMember]
        public virtual DefinedType Sources { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.EntityType"/> of the entities that <see cref="Rock.Model.Note">Notes</see> of this NoteType 
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.EntityType"/> that this NoteType is associated with.
        /// </value>
        [DataMember]
        public virtual EntityType EntityType { get; set; }

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

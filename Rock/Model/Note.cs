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
    /// Represents a note that is entered in RockChMS and is associated with a specific entity. For example, a note could be entered on a person, GroupMember, a device, etc.
    /// </summary>
    [Table( "Note" )]
    [DataContract]
    public partial class Note : Model<Note>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if this note is part of the RockChMS core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this note is part of the RockChMS core system/framework; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.NoteType"/>. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int"/> representing the Id of the <see cref="Rock.Model.NoteType"/>
        /// </value>
        [Required]
        [DataMember]
        public int NoteTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the entity that this note is related to.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the entity (object) that this note is related to.
        /// </value>
        [DataMember]
        public int? EntityId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the SourceType <see cref="Rock.Model.DefinedValue"/>. This shows how/where the note was created.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Source Type <see cref="Rock.Model.DefinedValue"/>.
        /// </value>
        [DataMember]
        public int? SourceTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the caption
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the caption of the Note.
        /// </value>
        [MaxLength( 200 )]
        [DataMember]
        public string Caption { get; set; }

        /// <summary>
        /// Gets or sets the date and time that the note was created.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date and time when the note was created.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public DateTime CreationDateTime { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this note is an alert.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this note is an alert; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool? IsAlert { get; set; }

        /// <summary>
        /// Gets or sets the text/body of the note.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the text/body of the note.
        /// </value>
        [DataMember]
        public string Text { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the Note Type
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.NoteType"/> of this note.
        /// </value>
        [DataMember]
        public virtual NoteType NoteType { get; set; }

        /// <summary>
        /// Gets or sets the source of the note.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.DefienedValue"/> representing the source of the note.
        /// </value>
        [DataMember]
        public virtual DefinedValue SourceType { get; set; }

        /// <summary>
        /// Gets the parent security authority of this Note. Where security is inherited from.
        /// </summary>
        /// <value>
        /// The parent authority.
        /// </value>
        public override Security.ISecured ParentAuthority
        {
            get
            {
                return this.NoteType;
            }
        }

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
            return this.Text;
        }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// Note Configuration class.
    /// </summary>
    public partial class NoteConfiguration : EntityTypeConfiguration<Note>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NoteConfiguration"/> class.
        /// </summary>
        public NoteConfiguration()
        {
            this.HasRequired( p => p.NoteType ).WithMany().HasForeignKey( p => p.NoteTypeId ).WillCascadeOnDelete( true );
            this.HasOptional( p => p.SourceType ).WithMany().HasForeignKey( p => p.SourceTypeValueId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

}

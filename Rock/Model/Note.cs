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
    /// Note POCO Entity.
    /// </summary>
    [Table( "Note" )]
    [DataContract( IsReference = true )]
    public partial class Note : Model<Note>
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
        /// Gets or sets the Note Type Id.
        /// </summary>
        /// <value>
        /// Note Type Id.
        /// </value>
        [Required]
        [DataMember]
        public int NoteTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Entity Id.
        /// </summary>
        /// <value>
        /// Entity Id.
        /// </value>
        [DataMember]
        public int? EntityId { get; set; }

        /// <summary>
        /// Gets or sets the source type value id.
        /// </summary>
        /// <value>
        /// The source type value id.
        /// </value>
        [DataMember]
        public int? SourceTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the caption.
        /// </summary>
        /// <value>
        /// The caption.
        /// </value>
        [MaxLength( 200 )]
        [DataMember]
        public string Caption { get; set; }

        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        /// <value>
        /// The date.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the alert.
        /// </summary>
        /// <value>
        /// The alert.
        /// </value>
        [DataMember]
        public bool? IsAlert { get; set; }

        /// <summary>
        /// Gets or sets the Value.
        /// </summary>
        /// <value>
        /// Value.
        /// </value>
        [DataMember]
        public string Text { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the Note Type.
        /// </summary>
        /// <value>
        /// A <see cref="NoteType"/> object.
        /// </value>
        [DataMember]
        public virtual NoteType NoteType { get; set; }

        /// <summary>
        /// Gets or sets the type of the source.
        /// </summary>
        /// <value>
        /// The type of the source.
        /// </value>
        [DataMember]
        public virtual DefinedValue SourceType { get; set; }

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

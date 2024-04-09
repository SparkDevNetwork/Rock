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
using Newtonsoft.Json;
using Rock.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

namespace Rock.Model
{
    /// <summary>
    /// Represents a note that is entered in Rock and is associated with a specific entity. For example, a note could be entered on a person, GroupMember, a device, etc or for a specific subset of an entity type.
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "Note" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.NOTE )]
    public partial class Note : Model<Note>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if this note is part of the Rock core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this note is part of the Rock core system/framework; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.NoteType"/>. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.NoteType"/>
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
        /// Gets or sets the caption
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the caption of the Note.
        /// </value>
        [MaxLength( 200 )]
        [DataMember]
        public string Caption { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this note is an alert.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this note is an alert; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool? IsAlert { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this note should be pinned to top
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this note is an alert; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsPinned { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this note is viewable to only the person that created the note
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is private note; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsPrivateNote { get; set; }

        /// <summary>
        /// Gets or sets the text/body of the note.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the text/body of the note.
        /// </value>
        [DataMember]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the parent note identifier.
        /// </summary>
        /// <value>
        /// The parent note identifier.
        /// </value>
        [DataMember]
        [IgnoreCanDelete]
        public int? ParentNoteId { get; set; }

        /// <summary>
        /// Gets or sets the approval status.
        /// </summary>
        /// <value>
        /// The approval status.
        /// </value>
        [DataMember]
        [Obsolete( "This property is no longer used and will be removed in the future." )]
        [RockObsolete( "1.16" )]
        public NoteApprovalStatus ApprovalStatus { get; set; }

        /// <summary>
        /// Gets or sets the PersonAliasId of the <see cref="Rock.Model.Person"/> who either approved or declined the Note. If no approval action has been performed on this Note, this value will be null.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the PersonAliasId of hte <see cref="Rock.Model.Person"/> who either approved or declined the ContentItem. This value will be null if no approval action has been
        /// performed on this add.
        /// </value>
        [DataMember]
        [Obsolete( "This property is no longer used and will be removed in the future." )]
        [RockObsolete( "1.16" )]
        public int? ApprovedByPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the approved date.
        /// </summary>
        /// <value>
        /// The approved date.
        /// </value>
        [DataMember]
        [Obsolete( "This property is no longer used and will be removed in the future." )]
        [RockObsolete( "1.16" )]
        public DateTime? ApprovedDateTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [notifications sent].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [notifications sent]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool NotificationsSent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [approvals sent].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [approvals sent]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        [Obsolete( "This property is no longer used and will be removed in the future." )]
        [RockObsolete( "1.16" )]
        public bool ApprovalsSent { get; set; }

        /// <summary>
        /// Gets or sets the URL where the Note was created. Use NoteUrl with a hash anchor of the Note.NoteAnchorId so that Notifications and Approvals can know where to view the note
        /// </summary>
        /// <value>
        /// The note URL.
        /// </value>
        [DataMember]
        public string NoteUrl { get; set; }

        /// <summary>
        /// Gets or sets the last time the note text was edited. Use this instead of ModifiedDateTime to determine the last time a person edited a note 
        /// </summary>
        /// <value>
        /// The edited date time.
        /// </value>
        [DataMember]
        public DateTime? EditedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the person alias that last edited the note text. Use this instead of ModifiedByPersonAliasId to determine the last person to edit the note text
        /// </summary>
        /// <value>
        /// The edited by person alias identifier.
        /// </value>
        [DataMember]
        public int? EditedByPersonAliasId { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the Note Type
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.NoteType"/> of this note.
        /// </value>
        [DataMember]
        public virtual NoteType NoteType { get; set; }

        /// <summary>
        /// Gets or sets the parent note.
        /// </summary>
        /// <value>
        /// The parent note.
        /// </value>
        [DataMember]
        [JsonIgnore]
        public virtual Note ParentNote { get; set; }

        /// <summary>
        /// Gets or sets the person alias that last edited the note text. Use this instead of ModifiedByPersonAlias to determine the last person to edit the note text
        /// </summary>
        /// <value>
        /// The edited by person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias EditedByPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the child notes.
        /// </summary>
        /// <value>
        /// The child notes.
        /// </value>
        [DataMember]
        public virtual ICollection<Note> ChildNotes { get; set; } = new Collection<Note>();

        /// <summary>
        /// Gets or sets the note attachments.
        /// </summary>
        /// <value>
        /// The note attachments.
        /// </value>
        [DataMember]
        public virtual ICollection<NoteAttachment> Attachments { get; set; } = new Collection<NoteAttachment>();

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
            this.HasOptional( p => p.ParentNote ).WithMany( p => p.ChildNotes ).HasForeignKey( p => p.ParentNoteId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.EditedByPersonAlias ).WithMany().HasForeignKey( p => p.EditedByPersonAliasId ).WillCascadeOnDelete( false );
        }
    }

    #endregion Entity Configuration
}

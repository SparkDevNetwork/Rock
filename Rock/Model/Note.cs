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
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;

using Newtonsoft.Json;

using Rock.Data;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a note that is entered in Rock and is associated with a specific entity. For example, a note could be entered on a person, GroupMember, a device, etc or for a specific subset of an entity type.
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "Note" )]
    [DataContract]
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
        public NoteApprovalStatus ApprovalStatus { get; set; }

        /// <summary>
        /// Gets or sets the PersonAliasId of the <see cref="Rock.Model.Person"/> who either approved or declined the Note. If no approval action has been performed on this Note, this value will be null.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the PersonAliasId of hte <see cref="Rock.Model.Person"/> who either approved or declined the ContentItem. This value will be null if no approval action has been
        /// performed on this add.
        /// </value>
        [DataMember]
        public int? ApprovedByPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the approved date.
        /// </summary>
        /// <value>
        /// The approved date.
        /// </value>
        [DataMember]
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
        /// Gets the childs note that the current person is allowed to view
        /// </summary>
        /// <value>
        /// The viewable child notes.
        /// </value>
        [LavaInclude]
        [NotMapped]
        public virtual List<Note> ViewableChildNotes
        {
            get
            {
                // only get notes they have auth to VIEW ( note that VIEW has special rules based on approval status, etc. See Note.IsAuthorized for details )
                var currentPerson = System.Web.HttpContext.Current?.Items["CurrentPerson"] as Person;

                var viewableChildNotes = ChildNotes.ToList().Where( a => a.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) ).ToList();

                return viewableChildNotes;
            }
        }

        /// <summary>
        /// Gets or sets the note attachments.
        /// </summary>
        /// <value>
        /// The note attachments.
        /// </value>
        [DataMember]
        public virtual ICollection<NoteAttachment> Attachments { get; set; } = new Collection<NoteAttachment>();

        /// <summary>
        /// Gets the created by person photo URL.
        /// </summary>
        /// <value>
        /// The created by person photo URL.
        /// </value>
        [LavaInclude]
        public virtual string CreatedByPersonPhotoUrl
        {
            get
            {
                return Person.GetPersonPhotoUrl( this.CreatedByPersonAlias.Person );
            }
        }

        /// <summary>
        /// Gets the id to use in the note's anchor tag
        /// </summary>
        /// <value>
        /// The note anchor identifier.
        /// </value>
        [LavaInclude]
        public virtual string NoteAnchorId => $"NoteRef-{this.Guid.ToString( "N" )}";

        /// <summary>
        /// Gets the name of the person that last edited the note text. Use this instead of ModifiedByPersonName to determine the last person to edit the note text
        /// </summary>
        /// <value>
        /// The edited by person alias.
        /// </value>
        [LavaInclude]
        public virtual string EditedByPersonName
        {
            get
            {
                var editedByPerson = EditedByPersonAlias?.Person ?? CreatedByPersonAlias?.Person;
                return editedByPerson?.FullName;
            }
        }

        /// <summary>
        /// Gets the name of the entity (If it is a Note on a Person, it would be the person's name, etc)
        /// </summary>
        /// <value>
        /// The name of the entity.
        /// </value>
        [LavaInclude]
        public virtual string EntityName
        {
            get
            {
                using ( var rockContext = new RockContext() )
                {
                    var noteTypeEntityTypeId = NoteTypeCache.Get( this.NoteTypeId )?.EntityTypeId;
                    if ( noteTypeEntityTypeId.HasValue && this.EntityId.HasValue )
                    {
                        var entity = new EntityTypeService( rockContext ).GetEntity( this.NoteType.EntityTypeId, this.EntityId.Value );
                        return entity?.ToString();
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the approval URL.
        /// </summary>
        /// <value>
        /// The approval URL.
        /// </value>
        [LavaInclude]
        public virtual string ApprovalUrl
        {
            get
            {
                string approvalUrlTemplate = NoteTypeCache.Get( this.NoteTypeId )?.ApprovalUrlTemplate;
                if ( string.IsNullOrWhiteSpace( approvalUrlTemplate ) )
                {
                    approvalUrlTemplate = "{{ 'Global' | Attribute:'InternalApplicationRoot' }}{{ Note.NoteUrl }}#{{ Note.NoteAnchorId }}";
                }

                var mergeFields = new Dictionary<string, object> { { "Note", this } };

                string approvalUrl = approvalUrlTemplate.ResolveMergeFields( mergeFields );

                return approvalUrl;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the currently logged in person is watching this specific note
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is current person watching; otherwise, <c>false</c>.
        /// </value>
        [LavaInclude]
        public virtual bool IsCurrentPersonWatching
        {
            get
            {
                var currentPerson = System.Web.HttpContext.Current?.Items["CurrentPerson"] as Person;
                var currentPersonId = currentPerson?.Id;
                if ( currentPersonId.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        bool isWatching = new NoteWatchService( rockContext ).Queryable()
                                .Where( a => a.NoteId == this.Id 
                                    && a.WatcherPersonAlias.PersonId == currentPersonId.Value 
                                    && a.IsWatching == true ).Any();

                        return isWatching;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Gets the count of that are descendants (replies) of this note.
        /// </summary>
        /// <value>
        /// The viewable descendents count.
        /// </value>
        [LavaInclude]
        public virtual int ViewableDescendentsCount
        {
            get
            {
                if ( !_viewableDescendentsCount.HasValue )
                {
                    var currentPerson = System.Web.HttpContext.Current?.Items["CurrentPerson"] as Person;

                    using ( var rockContext = new RockContext() )
                    {
                        var noteDescendents = new NoteService( rockContext ).GetAllDescendents( this.Id ).ToList();
                        var viewableDescendents = noteDescendents.ToList().Where( a => a.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) ).ToList();
                        _viewableDescendentsCount = viewableDescendents.Count();
                    }
                }

                return _viewableDescendentsCount.Value;
            }
        }

        private int? _viewableDescendentsCount = null;

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
                var noteType = NoteTypeCache.Get( this.NoteTypeId );
                return noteType ?? base.ParentAuthority;
            }
        }

        /// <summary>
        /// Determines whether the specified action is authorized on this note.
        /// Special note on the VIEW action: a person can view a note if they have normal VIEW access, but also have any of the following is true of the note:
        ///  - Approved,
        ///  - The current person is the one who created the note,
        ///  - The current person is the one who last edited the note,
        ///  - No Approval is required,
        ///  - The current person is an approver
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public override bool IsAuthorized( string action, Person person )
        {
            if ( this.IsPrivateNote )
            {
                // If this is a private note, the creator has FULL access to it. Everybody else has NO access (including admins)
                if ( this.CreatedByPersonAlias?.PersonId == person?.Id )
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            if ( action.Equals( Rock.Security.Authorization.APPROVE, StringComparison.OrdinalIgnoreCase ) )
            {
                // If checking the APPROVE action, let people Approve private notes that they created (see above), otherwise just use the normal IsAuthorized
                return base.IsAuthorized( action, person );
            }
            else if ( action.Equals( Rock.Security.Authorization.VIEW, StringComparison.OrdinalIgnoreCase ) )
            {
                // View has special rules depending on the approval status and APPROVE verb

                // first check if have normal VIEW access on the base
                if ( !base.IsAuthorized( Authorization.VIEW, person ) )
                {
                    return false;
                }

                if ( this.ApprovalStatus == NoteApprovalStatus.Approved )
                {
                    return true;
                }
                else if ( this.CreatedByPersonAliasId == person?.PrimaryAliasId )
                {
                    return true;
                }
                else if ( this.EditedByPersonAliasId == person?.PrimaryAliasId )
                {
                    return true;
                }
                else if ( NoteTypeCache.Get( this.NoteTypeId )?.RequiresApprovals == false )
                {
                    return true;
                }
                else if ( this.IsAuthorized( Authorization.APPROVE, person ) )
                {
                    return true;
                }

                return false;
            }
            else if ( action.Equals( Rock.Security.Authorization.EDIT, StringComparison.OrdinalIgnoreCase ) )
            {
                // If this note was created by the logged person, they should be be able to EDIT their own note,
                // otherwise EDIT (and DELETE) of other people's notes require ADMINISTRATE
                if ( CreatedByPersonAlias?.PersonId == person?.Id )
                {
                    return true;
                }
                else 
                {
                    return base.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, person );
                }
            }
            else
            {
                // If this note was created by the logged person, they should be be able to do any action (except for APPROVE)
                if ( CreatedByPersonAlias?.PersonId == person?.Id )
                {
                    return true;
                }

                return base.IsAuthorized( action, person );
            }
        }

        /// <summary>
        /// Determines whether the specified action is private.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public override bool IsPrivate( string action, Person person )
        {
            if ( CreatedByPersonAlias != null && person != null &&
                CreatedByPersonAlias.PersonId == person.Id &&
                IsPrivateNote )
            {
                return true;
            }

            return base.IsPrivate( action, person );
        }

        #endregion


        #region overrides

        /// <summary>
        /// Method that will be called on an entity immediately after the item is saved by context
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entry">The entry.</param>
        /// <param name="state">The state.</param>
        public override void PreSaveChanges( Data.DbContext dbContext, DbEntityEntry entry, EntityState state )
        {
            if ( state == EntityState.Added )
            {
                var noteType = NoteTypeCache.Get( this.NoteTypeId );
                if ( noteType?.AutoWatchAuthors == true )
                {
                    // if this is a new note, and AutoWatchAuthors, then add a NoteWatch so the author will get notified when there are any replies
                    var rockContext = dbContext as RockContext;
                    if ( rockContext != null && this.CreatedByPersonAliasId.HasValue )
                    {
                        var noteWatchService = new NoteWatchService( rockContext );

                        // we don't know the Note.Id yet, so just assign the NoteWatch.Note and EF will populate the NoteWatch.NoteId automatically
                        var noteWatch = new NoteWatch
                        {
                            IsWatching = true,
                            WatcherPersonAliasId = this.CreatedByPersonAliasId.Value,
                            Note = this
                        };

                        noteWatchService.Add( noteWatch );
                    }
                }
            }

            base.PreSaveChanges( dbContext, entry, state );
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
            this.HasOptional( p => p.ParentNote ).WithMany( p => p.ChildNotes ).HasForeignKey( p => p.ParentNoteId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.EditedByPersonAlias ).WithMany().HasForeignKey( p => p.EditedByPersonAliasId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

    #region Enumerations

    /// <summary>
    /// Represents the approval status of a note
    /// </summary>
    public enum NoteApprovalStatus
    {
        /// <summary>
        /// The <see cref="Note"/> is pending approval.
        /// </summary>
        PendingApproval = 0,

        /// <summary>
        /// The <see cref="Note"/> has been approved.
        /// </summary>
        Approved = 1,

        /// <summary>
        /// The <see cref="Note"/> was denied.
        /// </summary>
        Denied = 2
    }

    #endregion

}

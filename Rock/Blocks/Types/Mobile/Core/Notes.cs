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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Blocks.Types.Mobile.Core
{
    /// <summary>
    /// Displays entity notes to the user and allows adding new notes.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockMobileBlockType" />

    [DisplayName( "Notes" )]
    [Category( "Mobile > Core" )]
    [Description( "Displays entity notes to the user and allows adding new notes." )]
    [IconCssClass( "fa fa-sticky-note" )]

    #region Block Attributes

    [EntityTypeField( "Entity Type",
        Description = "The type of entity",
        IsRequired = false,
        Key = AttributeKey.ContextEntityType,
        Order = 0 )]

    [NoteTypeField( "Note Types",
        Description = "Optional list of note types to limit display to",
        AllowMultiple = true,
        IsRequired = false,
        Order = 1,
        Key = AttributeKey.NoteTypes )]

    [FileField( Rock.SystemGuid.BinaryFiletype.DEFAULT,
        "Default Note Image",
        Description = "This image is displayed next to the note if the author has no profile image.",
        IsRequired = false,
        Key = AttributeKey.DefaultNoteImage,
        Order = 2,
        FieldTypeClass = "Rock.Field.Types.ImageFieldType" )]

    #endregion

    [ContextAware]
    public class Notes : RockMobileBlockType
    {
        /// <summary>
        /// The block setting attribute keys for the MobileContent block.
        /// </summary>
        public static class AttributeKey
        {
            /// <summary>
            /// The context entity type key
            /// </summary>
            public const string ContextEntityType = "ContextEntityType";

            /// <summary>
            /// The note types key
            /// </summary>
            public const string NoteTypes = "NoteTypes";

            /// <summary>
            /// This image is displayed next to the note if the author has no profile image.
            /// </summary>
            public const string DefaultNoteImage = "DefaultNoteImage";
        }

        /// <summary>
        /// Gets the type of the context entity.
        /// </summary>
        /// <value>
        /// The type of the context entity.
        /// </value>
        protected string ContextEntityType => GetAttributeValue( AttributeKey.ContextEntityType );

        /// <summary>
        /// Gets the note type unique identifiers selected in the block configuration.
        /// </summary>
        /// <value>
        /// The note type unique identifiers selected in the block configuration.
        /// </value>
        protected ICollection<Guid> NoteTypes => GetAttributeValue( AttributeKey.NoteTypes ).SplitDelimitedValues().AsGuidList();

        /// <summary>
        /// Gets the default note image to display next to the note if the author has no profile image.
        /// </summary>
        /// <value>
        /// The default note image to display next to the note if the author has no profile image.
        /// </value>
        protected Guid? DefaultNoteImage => GetAttributeValue( AttributeKey.DefaultNoteImage ).AsGuidOrNull();

        #region IRockMobileBlockType Implementation

        /// <summary>
        /// Gets the required mobile application binary interface version required to render this block.
        /// </summary>
        /// <value>
        /// The required mobile application binary interface version required to render this block.
        /// </value>
        public override int RequiredMobileAbiVersion => 2;

        /// <summary>
        /// Gets the class name of the mobile block to use during rendering on the device.
        /// </summary>
        /// <value>
        /// The class name of the mobile block to use during rendering on the device
        /// </value>
        public override string MobileBlockType => "Rock.Mobile.Blocks.Core.Notes";

        /// <summary>
        /// Gets the property values that will be sent to the device in the application bundle.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetMobileConfigurationValues()
        {
            string defaultNoteImageUrl = null;

            if ( DefaultNoteImage.HasValue )
            {
                // Can't use defaultNoteImageFile.Url because it will build the path
                // relative to the current request, which won't always work with mobile
                // applications. So force it to use the PublicApplicationRoot.
                using ( var rockContext = new RockContext() )
                {
                    var defaultNoteImageFile = new BinaryFileService( rockContext ).Get( DefaultNoteImage.Value );

                    if ( defaultNoteImageFile != null )
                    {
                        var url = defaultNoteImageFile.Path;
                        url = url.StartsWith( "~" ) ? System.Web.VirtualPathUtility.ToAbsolute( url ) : url;
                        if ( !url.StartsWith( "http", StringComparison.OrdinalIgnoreCase ) )
                        {
                            var uri = new Uri( GlobalAttributesCache.Get().GetValue( "PublicApplicationRoot" ) );
                            if ( uri != null )
                            {
                                url = uri.Scheme + "://" + uri.GetComponents( UriComponents.HostAndPort, UriFormat.UriEscaped ) + url;
                            }

                            defaultNoteImageUrl = url;
                        }
                    }
                }
            }

            return new
            {
                DefaultNoteImageUrl = defaultNoteImageUrl
            };
        }

        #endregion

        /// <summary>
        /// Gets the viewable note types.
        /// </summary>
        /// <returns></returns>
        private ICollection<NoteTypeCache> GetViewableNoteTypes()
        {
            var contextEntityType = EntityTypeCache.Get( ContextEntityType );

            if ( contextEntityType == null )
            {
                return new NoteTypeCache[0];
            }

            return NoteTypeCache.GetByEntity( contextEntityType.Id, string.Empty, string.Empty, true )
                .Where( a => !NoteTypes.Any() || NoteTypes.Contains( a.Guid ) )
                .Where( a => a.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                .ToList();
        }

        /// <summary>
        /// Gets the editable note types.
        /// </summary>
        /// <returns></returns>
        private ICollection<NoteTypeCache> GetEditableNoteTypes()
        {
            var contextEntityType = EntityTypeCache.Get( ContextEntityType );

            if ( contextEntityType == null )
            {
                return new NoteTypeCache[0];
            }

            return NoteTypeCache.GetByEntity( contextEntityType.Id, string.Empty, string.Empty, true )
                .Where( a => !NoteTypes.Any() || NoteTypes.Contains( a.Guid ) )
                .Where( a => a.UserSelectable )
                .Where( a => a.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                .ToList();
        }

        /// <summary>
        /// Gets the note object that will be sent to the shell.
        /// </summary>
        /// <param name="note">The database note.</param>
        /// <returns>The note object that the shell understands.</returns>
        private object GetNoteObject( Note note )
        {
            var baseUrl = GlobalAttributesCache.Value( "PublicApplicationRoot" );
            var canEdit = note.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
            var canReply = note.NoteType.AllowsReplies;

            // If the note type specifies the max reply depth then calculate and check.
            if ( canReply && note.NoteType.MaxReplyDepth.HasValue )
            {
                int replyDepth = 0;
                for ( var noteParent = note; noteParent != null; noteParent = noteParent.ParentNote )
                {
                    replyDepth += 1;
                }

                canReply = replyDepth < note.NoteType.MaxReplyDepth.Value;
            }

            return new
            {
                note.Guid,
                NoteTypeGuid = note.NoteType.Guid,
                note.Text,
                PhotoUrl = note.CreatedByPersonAlias?.Person?.PhotoId != null ? $"{baseUrl}{note.CreatedByPersonAlias.Person.PhotoUrl}" : null,
                Name = note.CreatedByPersonName,
                Date = note.CreatedDateTime.HasValue ? ( DateTimeOffset? ) new DateTimeOffset( note.CreatedDateTime.Value ) : null,
                ReplyCount = note.ChildNotes.Count( b => b.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) ),
                note.IsAlert,
                IsPrivate = note.IsPrivateNote,
                CanEdit = canEdit,
                CanDelete = canEdit,
                CanReply = canReply
            };
        }

        /// <summary>
        /// Gets the notes for the entity.
        /// </summary>
        /// <param name="parentNoteGuid">The parent note unique identifier.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="count">The count.</param>
        /// <returns>The list of notes found.</returns>
        private List<object> GetEntityNotes( Guid? parentNoteGuid, int startIndex, int count )
        {
            using ( var rockContext = new RockContext() )
            {
                var noteService = new NoteService( rockContext );
                var viewableNoteTypeIds = GetViewableNoteTypes().Select( t => t.Id ).ToList();

                var entityType = EntityTypeCache.Get( ContextEntityType );
                var entity = entityType != null ? RequestContext.GetContextEntity( entityType.GetEntityType() ) : null;
                if ( entity == null )
                {
                    // Indicate to caller "not found" error.
                    return null;
                }

                var notesQuery = noteService.Queryable()
                    .AsNoTracking()
                    .Include( a => a.CreatedByPersonAlias.Person )
                    .Include( a => a.ParentNote )
                    .Include( a => a.ChildNotes )
                    .Where( a => viewableNoteTypeIds.Contains( a.NoteTypeId ) )
                    .Where( a => a.EntityId == entity.Id );

                if ( parentNoteGuid.HasValue )
                {
                    notesQuery = notesQuery.Where( a => a.ParentNote.Guid == parentNoteGuid.Value );
                }
                else
                {
                    notesQuery = notesQuery.Where( a => !a.ParentNoteId.HasValue );
                }

                var viewableNotes = notesQuery
                    .OrderByDescending( a => a.IsAlert == true )
                    .ThenByDescending( a => a.CreatedDateTime )
                    .ToList()
                    .Where( a => a.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                    .Skip( startIndex )
                    .Take( count )
                    .ToList();

                var noteData = viewableNotes
                    .Select( a => GetNoteObject( a ) )
                    .ToList();

                return noteData;
            }
        }

        #region Action Methods

        /// <summary>
        /// Gets the initial data that will tell the note block how to function.
        /// </summary>
        /// <param name="count">The number of initial notes to load.</param>
        /// <returns>A response that contains the initial shell block data.</returns>
        [BlockAction]
        public BlockActionResult GetInitialData( int count )
        {
            var notes = GetEntityNotes( null, 0, count );

            if ( notes == null )
            {
                return ActionNotFound();
            }

            var editableNoteTypes = GetEditableNoteTypes()
                .Select( a => new
                {
                    a.Guid,
                    a.Name,
                    a.UserSelectable
                } );

            return ActionOk( new
            {
                EditableNoteTypes = editableNoteTypes,
                Notes = notes
            } );
        }

        /// <summary>
        /// Gets a subset of the notes to be displayed.
        /// </summary>
        /// <param name="parentNoteGuid">The parent note unique identifier.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="count">The number of notes to return.</param>
        /// <returns>A response that contains the requested notes or an error.</returns>
        [BlockAction]
        public BlockActionResult GetNotes( Guid? parentNoteGuid, int startIndex, int count )
        {
            var notes = GetEntityNotes( parentNoteGuid, startIndex, count );

            if ( notes == null )
            {
                return ActionNotFound();
            }

            return ActionOk( notes );
        }

        /// <summary>
        /// Saves the note.
        /// </summary>
        /// <param name="noteGuid">The note unique identifier.</param>
        /// <param name="parentNoteGuid">The parent note unique identifier.</param>
        /// <param name="noteTypeGuid">The note type unique identifier.</param>
        /// <param name="text">The text of the note.</param>
        /// <param name="isAlert">if set to <c>true</c> the note is an alert.</param>
        /// <param name="isPrivate">if set to <c>true</c> the note is private.</param>
        /// <returns>A response that contains the saved note data or an error.</returns>
        [BlockAction]
        public BlockActionResult SaveNote( Guid? noteGuid, Guid? parentNoteGuid, Guid noteTypeGuid, string text, bool isAlert, bool isPrivate )
        {
            var noteType = NoteTypeCache.Get( noteTypeGuid );

            if ( noteType == null )
            {
                return ActionBadRequest( "Invalid note type." );
            }

            var entityType = EntityTypeCache.Get( ContextEntityType );
            var entity = entityType != null ? RequestContext.GetContextEntity( entityType.GetEntityType() ) : null;
            if ( entity == null )
            {
                return ActionBadRequest( "Unknown note type." );
            }

            using ( var rockContext = new RockContext() )
            {
                var noteService = new NoteService( rockContext );
                Note note;

                var parentNote = parentNoteGuid.HasValue ? noteService.Get( parentNoteGuid.Value ) : null;

                if ( !noteGuid.HasValue )
                {
                    if ( !noteType.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                    {
                        return ActionForbidden( "Not authorized to add note." );
                    }

                    note = rockContext.Notes.Create();
                    note.IsSystem = false;
                    note.EntityId = entity.Id;
                    note.ParentNoteId = parentNote?.Id;

                    noteService.Add( note );
                }
                else
                {
                    note = noteService.Get( noteGuid.Value );

                    if ( note == null )
                    {
                        return ActionNotFound();
                    }

                    if ( !note.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                    {
                        return ActionForbidden( "Not authorized to edit note." );
                    }
                }

                // If the note is new or is owned by the current person then
                // update the private flag.
                if ( note.Id == 0 || ( note.CreatedByPersonId.HasValue && RequestContext.CurrentPerson?.Id == note.CreatedByPersonId ) )
                {
                    note.IsPrivateNote = isPrivate;
                }

                // It's up to the client to handle logic for non-user selectable
                // note types.
                note.NoteTypeId = noteType.Id;

                string personalNoteCaption = "You - Personal Note";
                if ( string.IsNullOrWhiteSpace( note.Caption ) )
                {
                    note.Caption = note.IsPrivateNote ? personalNoteCaption : string.Empty;
                }
                else
                {
                    // if the note still has the personalNoteCaption, but was changed to have IsPrivateNote to false, change the caption to empty string
                    if ( note.Caption == personalNoteCaption && !note.IsPrivateNote )
                    {
                        note.Caption = string.Empty;
                    }
                }

                note.Text = text;
                note.IsAlert = isAlert;

                note.EditedByPersonAliasId = RequestContext.CurrentPerson?.PrimaryAliasId;
                note.EditedDateTime = RockDateTime.Now;

                if ( noteType.RequiresApprovals )
                {
                    if ( note.IsAuthorized( Authorization.APPROVE, RequestContext.CurrentPerson ) )
                    {
                        note.ApprovalStatus = NoteApprovalStatus.Approved;
                        note.ApprovedByPersonAliasId = RequestContext.CurrentPerson?.PrimaryAliasId;
                        note.ApprovedDateTime = RockDateTime.Now;
                    }
                    else
                    {
                        note.ApprovalStatus = NoteApprovalStatus.PendingApproval;
                    }
                }
                else
                {
                    note.ApprovalStatus = NoteApprovalStatus.Approved;
                }

                rockContext.SaveChanges();

                return ActionOk( GetNoteObject( note ) );
            }
        }

        /// <summary>
        /// Deletes the note.
        /// </summary>
        /// <param name="noteGuid">The note unique identifier.</param>
        /// <returns>A response that contains either an error or informs the client the note was deleted.</returns>
        [BlockAction]
        public BlockActionResult DeleteNote( Guid noteGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var service = new NoteService( rockContext );
                var note = service.Get( noteGuid );

                if ( note == null )
                {
                    return ActionNotFound();
                }

                if ( !note.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    // Rock.Constants strings include HTML so don't use.
                    return ActionForbidden( "You are not authorized to delete this note." );
                }

                if ( service.CanDeleteChildNotes( note, RequestContext.CurrentPerson, out var errorMessage ) && service.CanDelete( note, out errorMessage ) )
                {
                    service.Delete( note, true );
                    rockContext.SaveChanges();

                    return ActionOk();
                }
                else
                {
                    return ActionForbidden( errorMessage );
                }
            }
        }

        #endregion
    }
}

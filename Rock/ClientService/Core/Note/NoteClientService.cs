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
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

using Rock.Core.NotificationMessageTypes;
using Rock.Data;
using Rock.Enums.Core;
using Rock.Model;
using Rock.Net;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Controls;
using Rock.Web.Cache;

namespace Rock.ClientService.Core.Note
{
    /// <summary>
    /// Provides methods to work with <see cref="DefinedValue"/> and translate
    /// information into data that can be consumed by the clients. This class
    /// is intended to be used with the notes container control.
    /// </summary>
    public class NoteClientService : ClientServiceBase
    {
        #region Properties

        /// <summary>
        /// Determines if notes that specify a specific created date and time
        /// are allowed when saving notes.
        /// </summary>
        public bool AllowBackdatedNotes { get; set; }

        /// <summary>
        /// Determines which note types are allowed when saving and retrieving
        /// notes. If <c>null</c> then no filtering will be performed. If empty
        /// then no note types will be allowed.
        /// </summary>
        public List<NoteTypeCache> AllowedNoteTypes { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteClientService"/> class.
        /// </summary>
        /// <param name="rockContext">The rock context to use when accessing the database.</param>
        /// <param name="person">The person to use for security checks.</param>
        public NoteClientService( RockContext rockContext, Person person )
            : base( rockContext, person )
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Begins the edit process of a note by returning the <see cref="NoteEditBag"/>
        /// that contains any information required to edit the requested note.
        /// </summary>
        /// <param name="note">The note to be edited.</param>
        /// <param name="errorMessage">Contains any error message when <c>null</c> is returned.</param>
        /// <returns>An instance of <see cref="NoteEditBag"/> that provides the editable details of the note.</returns>
        public NoteEditBag EditNote( Model.Note note, out string errorMessage )
        {
            if ( note == null )
            {
                throw new ArgumentNullException( nameof( note ) );
            }

            if ( EnableSecurity && !note.IsAuthorized( Authorization.EDIT, Person ) )
            {
                errorMessage = "Not authorized to edit note.";
                return null;
            }

            note.LoadAttributes( RockContext );

            errorMessage = null;

            return new NoteEditBag
            {
                IdKey = note.IdKey,
                ParentNoteIdKey = note.ParentNoteId.HasValue
                    ? IdHasher.Instance.GetHash( note.ParentNoteId.Value )
                    : null,
                NoteTypeIdKey = IdHasher.Instance.GetHash( note.NoteTypeId ),
                Text = note.Text,
                IsAlert = note.IsAlert ?? false,
                IsPrivate = note.IsPrivateNote,
                IsPinned = note.IsPinned,
                CreatedDateTime = note.CreatedDateTime?.ToRockDateTimeOffset(),
                AttributeValues = note.GetPublicAttributeValuesForEdit( Person, enforceSecurity: EnableSecurity )
            };
        }

        /// <summary>
        /// Sets the watched state of a specific note.
        /// </summary>
        /// <param name="note">The note to be updated.</param>
        /// <param name="isWatching"><c>true</c> if the note should be watched; otherwise <c>false</c>.</param>
        /// <param name="errorMessage">Contains any error message when <c>false</c> is returned.</param>
        /// <returns><c>true</c> if successful; otherwise <c>false</c>.</returns>
        public bool WatchNote( Model.Note note, bool isWatching, out string errorMessage )
        {
            if ( note == null )
            {
                throw new ArgumentNullException( nameof( note ) );
            }

            if ( Person == null )
            {
                errorMessage = "Must be logged in to watch notes.";
                return false;
            }

            var noteWatchService = new NoteWatchService( RockContext );

            if ( EnableSecurity && !note.IsAuthorized( Authorization.VIEW, Person ) )
            {
                errorMessage = "You do not have permission to view that note.";
                return false;
            }

            var noteWatch = noteWatchService.Queryable()
                .Where( nw => nw.NoteId == note.Id
                    && nw.WatcherPersonAlias.PersonId == Person.Id )
                .FirstOrDefault();

            if ( noteWatch == null )
            {
                noteWatch = new NoteWatch
                {
                    NoteId = note.Id,
                    WatcherPersonAliasId = Person.PrimaryAliasId
                };

                noteWatchService.Add( noteWatch );
            }

            noteWatch.IsWatching = isWatching;
            RockContext.SaveChanges();

            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Deletes the requested note from the database.
        /// </summary>
        /// <param name="note">The note to be deleted.</param>
        /// <param name="errorMessage">Contains any error message when <c>false</c> is returned.</param>
        /// <returns><c>true</c> if successful; otherwise <c>false</c>.</returns>
        public bool DeleteNote( Model.Note note, out string errorMessage )
        {
            var noteService = new NoteService( RockContext );

            if ( note == null )
            {
                throw new ArgumentNullException( nameof( note ) );
            }

            if ( EnableSecurity && !note.IsAuthorized( Authorization.EDIT, Person ) )
            {
                errorMessage = "You do not have permission to delete that note.";
                return false;
            }
            else if ( !noteService.CanDeleteChildNotes( note, Person, out errorMessage ) )
            {
                return false;
            }

            noteService.Delete( note, true );
            RockContext.SaveChanges();

            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Saves the changes made to a note.
        /// </summary>
        /// <param name="note">The request that describes the note and the changes that were made.</param>
        /// <param name="contextEntity">The entity that this note will be attached to.</param>
        /// <param name="pageId">The identifier of the current page the note is being edited on.</param>
        /// <param name="currentPageUrl">The full URL of the current page the note is being edited on.</param>
        /// <param name="requestContext">The active request context when the note is edited.</param>
        /// <param name="errorMessage">Contains any error message when <c>null</c> is returned.</param>
        /// <returns>A new <see cref="NoteBag"/> instance that describes the note for display purposes.</returns>
        public NoteBag SaveNote( SaveNoteRequestBag note, IEntity contextEntity, int pageId, string currentPageUrl, RockRequestContext requestContext, out string errorMessage )
        {
            if ( note == null )
            {
                throw new ArgumentNullException( nameof( note ) );
            }

            if ( contextEntity == null )
            {
                throw new ArgumentNullException( nameof( contextEntity ) );
            }

            if ( note == null || !note.IsValidProperty( nameof( NoteEditBag.IdKey ) ) )
            {
                errorMessage = "Request details are not valid.";
                return null;
            }

            var noteService = new NoteService( RockContext );
            Model.Note noteEntity = null;
            var mentionedPersonIds = new List<int>();

            if ( note.Bag.IdKey.IsNotNullOrWhiteSpace() )
            {
                noteEntity = noteService.Get( note.Bag.IdKey, false );

                if ( noteEntity == null )
                {
                    errorMessage = "Note not found.";
                    return null;
                }

                if ( EnableSecurity && !noteEntity.IsAuthorized( Authorization.EDIT, Person ) )
                {
                    errorMessage = "Not authorized to edit note.";
                    return null;
                }
            }
            else
            {
                if ( !note.IsValidProperty( nameof( note.Bag.ParentNoteIdKey ) ) )
                {
                    errorMessage = "New note details must include parent note identifier.";
                    return null;
                }

#if REVIEW_WEBFORMS
                noteEntity = RockContext.Set<Model.Note>().Create();
#else
                noteEntity = RockContext.Set<Model.Note>().CreateProxy();
#endif
                noteEntity.EntityId = contextEntity.Id;
                noteEntity.ParentNoteId = IdHasher.Instance.GetId( note.Bag.ParentNoteIdKey );

                // If this is a reply, check if a reply is allowed.
                if ( noteEntity.ParentNoteId.HasValue )
                {
                    var parentNote = noteService.Get( noteEntity.ParentNoteId.Value );

                    if ( !noteService.CanReplyToNote( parentNote, out errorMessage ) )
                    {
                        return null;
                    }
                }

                noteService.Add( noteEntity );
            }

            if ( note.IsValidProperty( nameof( note.Bag.NoteTypeIdKey ) ) )
            {
                // Find the note type from either the request or the existing note.
                var noteTypeId = IdHasher.Instance.GetId( note.Bag.NoteTypeIdKey );
                var noteType = noteTypeId.HasValue ? NoteTypeCache.Get( noteTypeId.Value ) : null;

                if ( noteType == null )
                {
                    errorMessage = "Note type is invalid.";
                    return null;
                }

                // Check if the specified note type is valid for selection
                var noteTypes = AllowedNoteTypes
                    ?? NoteTypeCache.GetByEntity( contextEntity.TypeId, string.Empty, string.Empty, false );
                var isValidNoteType = noteTypes
                    .Any( nt => nt.UserSelectable && nt.Id == noteType.Id );

                if ( !isValidNoteType )
                {
                    errorMessage = "Note type is invalid.";
                    return null;
                }

                // If the note has child notes, ensure that they have the same note type as the parent.
                SetParentNoteType( noteEntity, noteType.Id );
            }

            note.IfValidProperty( nameof( note.Bag.Text ), () =>
            {
                var noteTypeCache = NoteTypeCache.Get( noteEntity.NoteTypeId );
                if ( noteTypeCache.FormatType != NoteFormatType.Unstructured && noteTypeCache.IsMentionEnabled )
                {
                    mentionedPersonIds = noteService.GetNewPersonIdsMentionedInContent( note.Bag.Text, noteEntity.Text );
                }
                noteEntity.Text = note.Bag.Text;
            } );

            note.IfValidProperty( nameof( note.Bag.IsAlert ),
                () => noteEntity.IsAlert = note.Bag.IsAlert );

            note.IfValidProperty( nameof( note.Bag.IsPrivate ), () =>
            {
                noteEntity.IsPrivateNote = note.Bag.IsPrivate;

                noteEntity.UpdateCaption();
            } );

            note.IfValidProperty( nameof( note.Bag.IsPinned ),
                () => noteEntity.IsPinned = note.Bag.IsPinned );

            if ( AllowBackdatedNotes )
            {
                note.IfValidProperty( nameof( note.Bag.CreatedDateTime ),
                    () => noteEntity.CreatedDateTime = note.Bag.CreatedDateTime?.DateTime );
            }

            noteEntity.EditedByPersonAliasId = Person?.PrimaryAliasId;
            noteEntity.EditedDateTime = RockDateTime.Now;
            noteEntity.NoteUrl = currentPageUrl;

            noteEntity.LoadAttributes( RockContext );
            noteEntity.SetPublicAttributeValues( note.Bag.AttributeValues, Person, enforceSecurity: true );

            // If the note was loaded, we checked security. But if it was
            // a new note, we were not able to check security until after
            // the NoteTypeId was set.
            if ( noteEntity.Id == 0 && !noteEntity.IsAuthorized( Authorization.EDIT, Person ) )
            {
                errorMessage = "Not authorized to edit note.";
                return null;
            }

            RockContext.SaveChanges();
            noteEntity.SaveAttributeValues( RockContext );

            // If we have any new mentioned person ids, start a background
            // task to create the notifications.
            if ( mentionedPersonIds.Any() )
            {
                // The request context will not be safe to use inside Task.Run.
                var pageParameters = requestContext.GetPageParameters();

                Task.Run( () =>
                {
                    foreach ( var personId in mentionedPersonIds )
                    {
                        NoteMention.CreateNotificationMessage( noteEntity, personId, Person.Id, pageId, pageParameters );
                    }
                } );
            }

            var watchedNoteIds = GetWatchedNoteIds( new List<Model.Note> { noteEntity } );

            errorMessage = null;

            return GetNoteBag( noteEntity, watchedNoteIds );
        }

        /// <summary>
        /// Gets the notes that can be viewed by the person for the specified entity.
        /// </summary>
        /// <param name="entity">The entity that the notes must be attached to.</param>
        /// <returns>A list of <see cref="Note"/> objects.</returns>
        public IEnumerable<Model.Note> GetViewableNotes( IEntity entity )
        {
            var entityTypeId = entity.TypeId;

            var noteService = new NoteService( RockContext );
#if REVIEW_WEBFORMS
            var noteQry = noteService.Queryable()
#else
            IQueryable<Model.Note> noteQry = noteService.Queryable()
#endif
                .Include( n => n.CreatedByPersonAlias.Person )
                .Include( n => n.EditedByPersonAlias.Person );

            if ( EnableSecurity )
            {
                noteQry = noteQry.AreViewableBy( Person?.Id );
            }

            noteQry = noteQry
                .AsNoTracking()
                .Where( n => n.NoteType.EntityTypeId == entityTypeId && n.EntityId == entity.Id );

            // Limit to the selected note types.
            if ( AllowedNoteTypes != null )
            {
                var noteTypeIds = AllowedNoteTypes.Select( nt => nt.Id ).ToList();

                noteQry = noteQry.Where( n => noteTypeIds.Contains( n.NoteTypeId ) );
            }

            if ( EnableSecurity )
            {
                return noteQry
                    .ToList()
                    .Where( n => n.IsAuthorized( Authorization.VIEW, Person ) );
            }

            return noteQry;
        }

        /// <summary>
        /// Orders the collection of notes according to standard note display
        /// rules for the note collection control.
        /// </summary>
        /// <param name="notes">The notes to be ordered.</param>
        /// <param name="descending"><c>true</c> if the notes should be displayed in descending order (recent on top).</param>
        /// <returns>The notes in the proper order.</returns>
        public IEnumerable<Model.Note> OrderNotes( IEnumerable<Model.Note> notes, bool descending )
        {
            if ( descending )
            {
                return notes.OrderByDescending( n => n.IsAlert == true )
                    .ThenByDescending( n => n.IsPinned == true )
                    .ThenByDescending( n => n.CreatedDateTime );
            }
            else
            {
                return notes.OrderByDescending( n => n.IsAlert == true )
                    .ThenByDescending( n => n.IsPinned == true )
                    .ThenBy( n => n.CreatedDateTime );
            }
        }

        /// <summary>
        /// Gets the identifiers of the notes currently being watched by the person.
        /// </summary>
        /// <param name="notes">The notes that will be displayed to the person.</param>
        /// <returns>A list of identifiers representing which notes are currently being watched.</returns>
        public List<int> GetWatchedNoteIds( List<Model.Note> notes )
        {
            if ( Person == null )
            {
                return new List<int>();
            }

            var noteIds = notes.Select( n => n.Id ).ToList();
            var noteWatchService = new NoteWatchService( RockContext );

            return noteWatchService.Queryable()
                .Where( nw => nw.NoteId.HasValue
                    && noteIds.Contains( nw.NoteId.Value )
                    && nw.WatcherPersonAlias.PersonId == Person.Id
                    && nw.IsWatching )
                .Select( nw => nw.NoteId.Value )
                .ToList();
        }

        /// <summary>
        /// Gets the note bag that will represent the given note.
        /// </summary>
        /// <param name="note">The note to be wrapped in a bag.</param>
        /// <param name="watchedNoteIds">The identifiers of the notes that are being watched.</param>
        /// <returns>An instance of <see cref="NoteBag"/> that represents the note.</returns>
        public NoteBag GetNoteBag( Model.Note note, List<int> watchedNoteIds )
        {
            var noteType = NoteTypeCache.Get( note.NoteTypeId );

            return new NoteBag
            {
                IdKey = note.IdKey,
                NoteTypeIdKey = noteType.IdKey,
                Caption = note.Caption,
                Text = note.Text,
                ApprovalStatus = noteType.RequiresApprovals
                    ? note.ApprovalStatus
                    : NoteApprovalStatus.Approved,
                AnchorId = note.NoteAnchorId,
                IsAlert = note.IsAlert ?? false,
                IsPinned = note.IsPinned,
                IsPrivate = note.IsPrivateNote,
                IsWatching = noteType.AllowsWatching && watchedNoteIds.Contains( note.Id ),
                IsEditable = note.IsAuthorized( Authorization.EDIT, Person ),
                IsDeletable = note.IsAuthorized( Authorization.EDIT, Person ),
                ParentNoteIdKey = note.ParentNoteId.HasValue
                    ? IdHasher.Instance.GetHash( note.ParentNoteId.Value )
                    : null,
                CreatedDateTime = note.CreatedDateTime?.ToRockDateTimeOffset(),
                CreatedByIdKey = note.CreatedByPersonAlias?.Person.IdKey,
                CreatedByName = note.CreatedByPersonAlias?.Person.FullName,
                CreatedByPhotoUrl = note.CreatedByPersonAlias?.Person.PhotoUrl,
                EditedDateTime = note.EditedDateTime?.ToRockDateTimeOffset(),
                EditedByName = note.EditedByPersonName,
                AttributeValues = GetFormattedAttributeValues( note, Person )
            };
        }

        /// <summary>
        /// Gets the formatted attribute values for a note.
        /// </summary>
        /// <param name="note">The note.</param>
        /// <param name="currentPerson">The person that will see the note.</param>
        /// <returns>A dictionary of attribute keys and the formatted HTML values.</returns>
        private static Dictionary<string, string> GetFormattedAttributeValues( Model.Note note, Person currentPerson )
        {
            var values = new Dictionary<string, string>();

            var items = note.Attributes
                .Values
                .OrderBy( a => a.Order )
                .Where( a => a.IsAuthorized( Authorization.VIEW, currentPerson ) )
                .Select( a => new
                {
                    a.Name,
                    Value = note.GetAttributeHtmlValue( a.Key )
                } )
                .Where( a => a.Value.IsNotNullOrWhiteSpace() );

            foreach ( var item in items )
            {
                values.TryAdd( item.Name, item.Value );
            }

            return values;
        }

        /// <summary>
        /// Gets the note type bag that will represent the given note type.
        /// </summary>
        /// <param name="noteType">The note type object to be represented.</param>
        /// <returns>A new instance of <see cref="NoteTypeBag"/>.</returns>
        public NoteTypeBag GetNoteTypeBag( NoteTypeCache noteType )
        {
            var note = new Model.Note
            {
                NoteTypeId = noteType.Id
            };

            note.LoadAttributes();

            return new NoteTypeBag
            {
                IdKey = noteType.IdKey,
                Name = noteType.Name,
                Color = noteType.Color,
                IconCssClass = noteType.IconCssClass,
                UserSelectable = noteType.UserSelectable
                    && ( !EnableSecurity || noteType.IsAuthorized( Authorization.EDIT, Person ) ),
                AllowsReplies = noteType.AllowsReplies,
                MaxReplyDepth = noteType.MaxReplyDepth ?? -1,
                AllowsWatching = noteType.AllowsWatching,
                RequiresApprovals = noteType.RequiresApprovals,
                IsMentionEnabled = noteType.FormatType != NoteFormatType.Unstructured && noteType.IsMentionEnabled,
                Attributes = note.GetPublicAttributesForEdit( Person, enforceSecurity: EnableSecurity )
            };
        }

        /// <summary>
        /// Sets the note type for a parent note and all child notes.
        /// </summary>
        /// <param name="parentNote">The parent note object.</param>
        /// <param name="noteTypeId">The identifier of the new Note Type.</param>
        private static void SetParentNoteType( Model.Note parentNote, int noteTypeId )
        {
            if ( parentNote == null )
            {
                return;
            }

            parentNote.NoteTypeId = noteTypeId;

            if ( parentNote.ChildNotes != null )
            {
                foreach ( var childNote in parentNote.ChildNotes )
                {
                    SetParentNoteType( childNote, noteTypeId );
                }
            }
        }

        #endregion
    }
}

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

using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

using Rock.Attribute;
using Rock.Core.NotificationMessageTypes;
using Rock.Data;
using Rock.Enums.Core;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks.Core.Notes;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Context aware block for adding notes to an entity.
    /// </summary>
    [DisplayName( "Notes" )]
    [Category( "Core" )]
    [Description( "Context aware block for adding notes to an entity." )]
    [IconCssClass( "fa fa-note" )]
    [SupportedSiteTypes( SiteType.Web )]
    [Rock.Web.UI.ContextAware]

    #region Block Attributes

    [TextField( "Heading",
        Description = "The text to display as the heading.",
        IsRequired = false,
        Order = 1,
        Key = AttributeKey.Heading )]

    [TextField( "Heading Icon CSS Class",
        Description = "The css class name to use for the heading icon. ",
        IsRequired = false,
        Order = 2,
        Key = AttributeKey.HeadingIcon )]

    [TextField( "Note Term",
        Description = "The term to use for note (i.e. 'Note', 'Comment').",
        IsRequired = false,
        DefaultValue = "Note",
        Order = 3,
        Key = AttributeKey.NoteTerm )]

    [CustomDropdownListField( "Display Type",
        Description = "The format to use for displaying notes.",
        ListSource = "Full,Light",
        IsRequired = true,
        DefaultValue = "Full",
        Order = 4,
        Key = AttributeKey.DisplayType )]

    [BooleanField( "Use Person Icon",
        DefaultValue = "false",
        Order = 5,
        Key = AttributeKey.UsePersonIcon )]

    [BooleanField( "Show Alert Checkbox",
        DefaultValue = "true",
        Order = 6,
        Key = AttributeKey.ShowAlertCheckbox )]

    [BooleanField( "Show Private Checkbox",
        DefaultValue = "true",
        Order = 7,
        Key = AttributeKey.ShowPrivateCheckbox )]

    [BooleanField( "Show Security Button",
        DefaultValue = "true",
        Order = 8,
        Key = AttributeKey.ShowSecurityButton )]

    [BooleanField( "Allow Anonymous",
        DefaultValue = "false",
        Order = 9,
        Key = AttributeKey.AllowAnonymous )]

    [BooleanField( "Add Always Visible",
        Description = "Should the add entry screen always be visible (vs. having to click Add button to display the entry screen).",
        DefaultValue = "false",
        Order = 10,
        Key = AttributeKey.AddAlwaysVisible )]

    [CustomDropdownListField( "Display Order",
        Description = "Descending will render with entry field at top and most recent note at top.  Ascending will render with entry field at bottom and most recent note at the end.  Ascending will also disable the more option",
        ListSource = "Ascending,Descending",
        IsRequired = true,
        DefaultValue = "Descending",
        Order = 11,
        Key = AttributeKey.DisplayOrder )]

    [BooleanField( "Allow Backdated Notes",
        DefaultValue = "false",
        Order = 12,
        Key = AttributeKey.AllowBackdatedNotes )]

    [NoteTypeField( "Note Types",
        Description = "Optional list of note types to limit display to",
        AllowMultiple = true,
        IsRequired = false,
        Order = 12,
        Key = AttributeKey.NoteTypes )]

    [BooleanField( "Display Note Type Heading",
        Description = "Should each note's Note Type be displayed as a heading above each note?",
        DefaultValue = "false",
        Order = 13,
        Key = AttributeKey.DisplayNoteTypeHeading )]

    [BooleanField( "Expand Replies",
        Description = "Should replies be automatically expanded?",
        DefaultValue = "false",
        Order = 14,
        Key = AttributeKey.ExpandReplies )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "33566B2B-D74F-4148-B962-1897D418C6DF" )]
    [Rock.SystemGuid.BlockTypeGuid( "D87B84DC-7AD9-42A2-B18D-88B7E71DADA8" )]
    public class Notes : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string Heading = "Heading";
            public const string HeadingIcon = "HeadingIcon";
            public const string NoteTerm = "NoteTerm";
            public const string DisplayType = "DisplayType";
            public const string UsePersonIcon = "UsePersonIcon";
            public const string ShowAlertCheckbox = "ShowAlertCheckbox";
            public const string ShowPrivateCheckbox = "ShowPrivateCheckbox";
            public const string ShowSecurityButton = "ShowSecurityButton";
            public const string AllowAnonymous = "AllowAnonymous";
            public const string AddAlwaysVisible = "AddAlwaysVisible";
            public const string DisplayOrder = "DisplayOrder";
            public const string AllowBackdatedNotes = "AllowBackdatedNotes";
            public const string NoteTypes = "NoteTypes";
            public const string DisplayNoteTypeHeading = "DisplayNoteTypeHeading";
            public const string ExpandReplies = "ExpandReplies";
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var contextEntity = GetContextEntity();

            if ( contextEntity == null )
            {
                return new
                {
                };
            }

            var noteTypes = GetConfiguredNoteTypes( contextEntity.TypeId );

            using ( var rockContext = new RockContext() )
            {
                var notes = GetViewableNotes( rockContext, RequestContext.CurrentPerson, contextEntity, noteTypes.Select( nt => nt.Id ).ToList() );
                var watchedNoteIds = GetWatchedNoteIds( notes, RequestContext.CurrentPerson, rockContext );

                if ( GetAttributeValue( AttributeKey.DisplayOrder ) == "Descending" )
                {
                    notes = notes.OrderByDescending( n => n.IsAlert == true )
                        .ThenByDescending( n => n.IsPinned == true )
                        .ThenByDescending( n => n.CreatedDateTime )
                        .ToList();
                }
                else
                {
                    notes = notes.OrderByDescending( n => n.IsAlert == true )
                        .ThenByDescending( n => n.IsPinned == true )
                        .ThenBy( n => n.CreatedDateTime )
                        .ToList();
                }

                notes.LoadAttributes( rockContext );

                return new NotesInitializationBox
                {
                    ErrorMessage = ( string ) null,
                    EntityIdKey = contextEntity.IdKey,
                    EntityTypeIdKey = EntityTypeCache.Get( contextEntity.TypeId ).IdKey,
                    Notes = notes.Select( n => GetNoteBag( n, watchedNoteIds, RequestContext.CurrentPerson ) ).ToList(),
                    NoteTypes = noteTypes.Select( nt => GetNoteTypeBag( nt, RequestContext.CurrentPerson ) ).ToList(),
                    Title = GetAttributeValue( AttributeKey.Heading ),
                    TitleIconCssClass = GetAttributeValue( AttributeKey.HeadingIcon ),
                    ShowAdd = GetAttributeValue( AttributeKey.AllowAnonymous ).AsBoolean() || RequestContext.CurrentPerson != null,
                    IsDescending = GetAttributeValue( AttributeKey.DisplayOrder ) == "Descending",
                    NoteLabel = GetAttributeValue( AttributeKey.NoteTerm ),
                    IsLightMode = GetAttributeValue( AttributeKey.DisplayType ) == "Light",
                    ShowAlertCheckBox = GetAttributeValue( AttributeKey.ShowAlertCheckbox ).AsBoolean(),
                    ShowPrivateCheckBox = GetAttributeValue( AttributeKey.ShowPrivateCheckbox ).AsBoolean(),
                    ShowSecurityButton = GetAttributeValue( AttributeKey.ShowSecurityButton ).AsBoolean(),
                    AddAlwaysVisible = GetAttributeValue( AttributeKey.AddAlwaysVisible ).AsBoolean(),
                    ShowCreateDateInput = GetAttributeValue( AttributeKey.AllowBackdatedNotes ).AsBoolean(),
                    DisplayNoteTypeHeading = GetAttributeValue( AttributeKey.DisplayNoteTypeHeading ).AsBoolean(),
                    UsePersonIcon = GetAttributeValue( AttributeKey.UsePersonIcon ).AsBoolean(),
                    ExpandReplies = GetAttributeValue( AttributeKey.ExpandReplies ).AsBoolean(),
                    PersonAvatarUrl = RequestContext.CurrentPerson?.PhotoUrl
                };
            }
        }

        /// <summary>
        /// Gets the notes that can be viewed by the person for the specified entity.
        /// </summary>
        /// <param name="rockContext">The rock context to use when accessing the database.</param>
        /// <param name="currentPerson">The current person that the notes will be shown to.</param>
        /// <param name="entity">The entity that the notes must be attached to.</param>
        /// <param name="noteTypeIds">The note type identifiers used to limit which notes are included.</param>
        /// <returns>A list of <see cref="Note"/> objects.</returns>
        private static List<Note> GetViewableNotes( RockContext rockContext, Person currentPerson, IEntity entity, List<int> noteTypeIds )
        {
            var entityTypeId = entity.TypeId;
            var noteService = new NoteService( rockContext );
            var noteQry = noteService.Queryable()
                .Include( n => n.CreatedByPersonAlias.Person )
                .Include( n => n.EditedByPersonAlias.Person )
                .AsNoTracking()
                .Where( n => n.NoteType.EntityTypeId == entityTypeId
                    && n.EntityId == entity.Id );

            // Limit to the selected note types.
            if ( noteTypeIds != null && noteTypeIds.Count > 0 )
            {
                noteQry = noteQry.Where( n => noteTypeIds.Contains( n.NoteTypeId ) );
            }

            var notes = noteQry.ToList()
                .Where( n => n.IsAuthorized( Authorization.VIEW, currentPerson ) )
                .ToList();

            return notes;
        }

        /// <summary>
        /// Gets the identifiers of the notes currently being watched by the person.
        /// </summary>
        /// <param name="notes">The notes that will be displayed to the person.</param>
        /// <param name="currentPerson">The current person the notes will be displayed to.</param>
        /// <param name="rockContext">The rock context to use when accessing the database.</param>
        /// <returns>A list of identifiers representing which notes are currently being watched.</returns>
        private static List<int> GetWatchedNoteIds( List<Note> notes, Person currentPerson, RockContext rockContext )
        {
            if ( currentPerson == null )
            {
                return new List<int>();
            }

            var noteIds = notes.Select( n => n.Id ).ToList();
            var noteWatchService = new NoteWatchService( rockContext );

            var watchedNoteIds = noteWatchService.Queryable()
                .Where( nw => nw.NoteId.HasValue
                    && noteIds.Contains( nw.NoteId.Value )
                    && nw.WatcherPersonAlias.PersonId == currentPerson.Id
                    && nw.IsWatching )
                .Select( nw => nw.NoteId.Value )
                .ToList();

            return watchedNoteIds;
        }

        /// <summary>
        /// Gets the note bag that will represent the given note.
        /// </summary>
        /// <param name="note">The note to be wrapped in a bag.</param>
        /// <param name="watchedNoteIds">The identifiers of the notes that are being watched.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <returns>An instance of <see cref="NoteBag"/> that represents the note.</returns>
        private static NoteBag GetNoteBag( Note note, List<int> watchedNoteIds, Person currentPerson )
        {
            var noteType = NoteTypeCache.Get( note.NoteTypeId );

            return new NoteBag
            {
                IdKey = note.IdKey,
                NoteTypeIdKey = noteType.IdKey,
                Caption = note.Caption,
                Text = note.Text,
                AnchorId = note.NoteAnchorId,
                IsAlert = note.IsAlert ?? false,
                IsPinned = note.IsPinned,
                IsPrivate = note.IsPrivateNote,
                IsWatching = noteType.AllowsWatching && watchedNoteIds.Contains( note.Id ),
                IsEditable = note.IsAuthorized( Authorization.EDIT, currentPerson ),
                IsDeletable = note.IsAuthorized( Authorization.EDIT, currentPerson ),
                ParentNoteIdKey = note.ParentNoteId.HasValue
                    ? IdHasher.Instance.GetHash( note.ParentNoteId.Value )
                    : null,
                CreatedDateTime = note.CreatedDateTime?.ToRockDateTimeOffset(),
                CreatedByIdKey = note.CreatedByPersonAlias?.Person.IdKey,
                CreatedByName = note.CreatedByPersonAlias?.Person.FullName,
                CreatedByPhotoUrl = note.CreatedByPersonAlias?.Person.PhotoUrl,
                EditedDateTime = note.EditedDateTime?.ToRockDateTimeOffset(),
                EditedByName = note.EditedByPersonName,
                AttributeValues = GetFormattedAttributeValues( note, currentPerson )
            };
        }

        /// <summary>
        /// Gets the formatted attribute values for a note.
        /// </summary>
        /// <param name="note">The note.</param>
        /// <param name="currentPerson">The person that will see the note.</param>
        /// <returns>A dictionary of attribute keys and the formatted HTML values.</returns>
        private static Dictionary<string, string> GetFormattedAttributeValues( Note note, Person currentPerson )
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
        /// <param name="currentPerson">The current person.</param>
        /// <returns>A new instance of <see cref="NoteTypeBag"/>.</returns>
        private static NoteTypeBag GetNoteTypeBag( NoteTypeCache noteType, Person currentPerson )
        {
            var note = new Note
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
                UserSelectable = noteType.UserSelectable && noteType.IsAuthorized( Authorization.EDIT, currentPerson ),
                AllowsReplies = noteType.AllowsReplies,
                MaxReplyDepth = noteType.MaxReplyDepth ?? -1,
                AllowsWatching = noteType.AllowsWatching,
                IsMentionEnabled = noteType.FormatType != NoteFormatType.Unstructured && noteType.IsMentionEnabled,
                Attributes = note.GetPublicAttributesForEdit( currentPerson )
            };
        }

        /// <summary>
        /// Gets the configured note types for this block. This considers the
        /// context entity type and then the block settings.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier for determining possible note types.</param>
        /// <returns>A list of <see cref="NoteTypeCache"/> objects that represent the configured note types.</returns>
        private List<NoteTypeCache> GetConfiguredNoteTypes( int entityTypeId )
        {
            var noteTypes = NoteTypeCache.GetByEntity( entityTypeId, string.Empty, string.Empty, true )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();

            // If block is configured to only allow certain note types, limit notes to those types.
            var configuredNoteTypes = GetAttributeValue( AttributeKey.NoteTypes ).SplitDelimitedValues().AsGuidList();
            if ( configuredNoteTypes.Any() )
            {
                noteTypes = noteTypes.Where( n => configuredNoteTypes.Contains( n.Guid ) ).ToList();
            }

            return noteTypes;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Begins the edit process of a note by returning the <see cref="NoteEditBag"/>
        /// that contains any information required to edit the requested note.
        /// </summary>
        /// <param name="request">The request that describes which note to edit.</param>
        /// <returns>An instance of <see cref="NoteEditBag"/> that provides the editable details of the note.</returns>
        [BlockAction]
        public BlockActionResult EditNote( EditNoteRequestBag request )
        {
            if ( request == null || request.IdKey.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Request details are not valid." );
            }

            using ( var rockContext = new RockContext() )
            {
                var noteService = new NoteService( rockContext );
                Note note = noteService.Get( request.IdKey, false );

                if ( note == null )
                {
                    return ActionNotFound( "Note not found." );
                }

                if ( !note.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionForbidden( "Not authorized to edit note." );
                }

                note.LoadAttributes( rockContext );

                var editBag = new NoteEditBag
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
                    AttributeValues = note.GetPublicAttributeValuesForEdit( RequestContext.CurrentPerson )
                };

                return ActionOk( editBag );
            }
        }

        /// <summary>
        /// Saves the changes made to a note.
        /// </summary>
        /// <param name="request">The request that describes the note and the changes that were made.</param>
        /// <returns>A new <see cref="NoteBag"/> instance that describes the note for display purposes.</returns>
        [BlockAction]
        public BlockActionResult SaveNote( SaveNoteRequestBag request )
        {
            var contextEntity = GetContextEntity();

            if ( contextEntity == null )
            {
                return ActionBadRequest( "No context is available." );
            }

            if ( request == null || !request.IsValidProperty( nameof( NoteEditBag.IdKey ) ) )
            {
                return ActionBadRequest( "Request details are not valid." );
            }

            using ( var rockContext = new RockContext() )
            {
                var noteService = new NoteService( rockContext );
                Note note = null;
                var mentionedPersonIds = new List<int>();

                if ( request.Bag.IdKey.IsNotNullOrWhiteSpace() )
                {
                    note = noteService.Get( request.Bag.IdKey, false );

                    if ( note == null )
                    {
                        return ActionNotFound( "Note not found." );
                    }

                    if ( !note.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                    {
                        return ActionForbidden( "Not authorized to edit note." );
                    }
                }

                if ( note == null )
                {
                    if ( !request.IsValidProperty( nameof( request.Bag.ParentNoteIdKey ) ) )
                    {
                        return ActionBadRequest( "New note details must include parent note identifier." );
                    }

                    note = new Note
                    {
                        EntityId = contextEntity.Id,
                        ParentNoteId = IdHasher.Instance.GetId( request.Bag.ParentNoteIdKey )
                    };

                    // If this is a reply, check if a reply is allowed.
                    if ( note.ParentNoteId.HasValue )
                    {
                        var parentNote = noteService.Get( note.ParentNoteId.Value );

                        if ( !noteService.CanReplyToNote( parentNote, out var errorMessage ) )
                        {
                            return ActionBadRequest( errorMessage );
                        }
                    }

                    noteService.Add( note );
                }

                if ( request.IsValidProperty( nameof( request.Bag.NoteTypeIdKey ) ) )
                {
                    // Find the note type from either the request or the existing note.
                    var noteTypeId = IdHasher.Instance.GetId( request.Bag.NoteTypeIdKey );
                    var noteType = noteTypeId.HasValue ? NoteTypeCache.Get( noteTypeId.Value ) : null;

                    if ( noteType == null )
                    {
                        return ActionBadRequest( "Note type is invalid." );
                    }

                    // Check if the specified note type is valid for selection
                    var isValidNoteType = GetConfiguredNoteTypes( contextEntity.TypeId )
                            .Any( nt => nt.UserSelectable && nt.Id == noteType.Id );

                    if ( !isValidNoteType )
                    {
                        return ActionBadRequest( "Note type is invalid." );
                    }

                    // If the note has child notes, ensure that they have the same note type as the parent.
                    SetParentNoteType( note, noteType.Id );
                }

                request.IfValidProperty( nameof( request.Bag.Text ), () =>
                {
                    var noteTypeCache = NoteTypeCache.Get( note.NoteTypeId );
                    if ( noteTypeCache.FormatType != NoteFormatType.Unstructured && noteTypeCache.IsMentionEnabled )
                    {
                        mentionedPersonIds = noteService.GetNewPersonIdsMentionedInContent( request.Bag.Text, note.Text );
                    }
                    note.Text = request.Bag.Text;
                } );

                request.IfValidProperty( nameof( request.Bag.IsAlert ),
                    () => note.IsAlert = request.Bag.IsAlert );

                request.IfValidProperty( nameof( request.Bag.IsPrivate ), () =>
                {
                    note.IsPrivateNote = request.Bag.IsPrivate;

                    note.UpdateCaption();
                } );

                request.IfValidProperty( nameof( request.Bag.IsPinned ), () =>
                {
                    note.IsPinned = request.Bag.IsPinned;
                } );

                if ( GetAttributeValue( AttributeKey.AllowBackdatedNotes ).AsBoolean() )
                {
                    request.IfValidProperty( nameof( request.Bag.CreatedDateTime ),
                        () => note.CreatedDateTime = request.Bag.CreatedDateTime?.DateTime );
                }

                note.EditedByPersonAliasId = RequestContext.CurrentPerson?.PrimaryAliasId;
                note.EditedDateTime = RockDateTime.Now;
                note.NoteUrl = this.GetCurrentPageUrl();

#pragma warning disable CS0618 // Type or member is obsolete
                // Set this so anything doing direct SQL queries will still find
                // the right set of notes.
                note.ApprovalStatus = NoteApprovalStatus.Approved;
#pragma warning restore CS0618 // Type or member is obsolete

                note.LoadAttributes( rockContext );
                note.SetPublicAttributeValues( request.Bag.AttributeValues, RequestContext.CurrentPerson );

                // If the note was loaded, we checked security. But if it was
                // a new note, we were not able to check security until after
                // the NoteTypeId was set.
                if ( note.Id == 0 && !note.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionForbidden( "Not authorized to edit note." );
                }

                rockContext.SaveChanges();
                note.SaveAttributeValues( rockContext );

                // If we have any new mentioned person ids, start a background
                // task to create the notifications.
                if ( mentionedPersonIds.Any() )
                {
                    Task.Run( () =>
                    {
                        foreach ( var personId in mentionedPersonIds )
                        {
                            NoteMention.CreateNotificationMessage( note, personId, RequestContext.CurrentPerson.Id, PageCache.Id, RequestContext.GetPageParameters() );
                        }
                    } );
                }

                // Load the entity in a new context to force all the navigation
                // properties to be valid.
                using ( var rockContext2 = new RockContext() )
                {
                    var savedNote = new NoteService( rockContext2 ).Queryable()
                        .Include( n => n.CreatedByPersonAlias.Person )
                        .Include( n => n.EditedByPersonAlias.Person )
                        .Where( n => n.Id == note.Id )
                        .FirstOrDefault();

                    var watchedNoteIds = GetWatchedNoteIds( new List<Note> { note }, RequestContext.CurrentPerson, rockContext2 );

                    savedNote.LoadAttributes( rockContext2 );

                    return ActionOk( GetNoteBag( savedNote, watchedNoteIds, RequestContext.CurrentPerson ) );
                }
            }
        }

        /// <summary>
        /// Sets the note type for a parent note and all child notes.
        /// </summary>
        /// <param name="parentNote">The parent note object.</param>
        /// <param name="noteTypeId">The identifier of the new Note Type.</param>
        private void SetParentNoteType( Note parentNote, int noteTypeId )
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

        /// <summary>
        /// Deletes the requested note from the database.
        /// </summary>
        /// <param name="request">The request that describes which note to delete.</param>
        /// <returns>An empty 200-OK response if the note was deleted.</returns>
        [BlockAction]
        public BlockActionResult DeleteNote( DeleteNoteRequestBag request )
        {
            if ( request == null || request.IdKey.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Action must specify item to delete." );
            }

            using ( var rockContext = new RockContext() )
            {
                var noteService = new NoteService( rockContext );
                var note = noteService.Get( request.IdKey, false );

                if ( note == null )
                {
                    return ActionNotFound( "Not not found." );
                }
                else if ( !note.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionForbidden( "You do not have permission to delete that note." );
                }
                else if ( !noteService.CanDeleteChildNotes( note, RequestContext.CurrentPerson, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                noteService.Delete( note, true );
                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        /// <summary>
        /// Sets the watched state of a specific note.
        /// </summary>
        /// <param name="request">The request that describes which note and if it is to be watched or unwatched.</param>
        /// <returns>An empty 200-OK response if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult WatchNote( WatchNoteRequestBag request )
        {
            if ( request == null || request.IdKey.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Action must specify item to watch." );
            }

            if ( RequestContext.CurrentPerson == null )
            {
                return ActionForbidden( "Must be logged in to watch notes." );
            }

            using ( var rockContext = new RockContext() )
            {
                var noteService = new NoteService( rockContext );
                var noteWatchService = new NoteWatchService( rockContext );
                var note = noteService.Get( request.IdKey, false );

                if ( note == null )
                {
                    return ActionNotFound( "Not not found." );
                }
                else if ( !note.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                {
                    return ActionForbidden( "You do not have permission to view that note." );
                }

                var noteWatch = noteWatchService.Queryable()
                    .Where( nw => nw.NoteId == note.Id
                        && nw.WatcherPersonAlias.PersonId == RequestContext.CurrentPerson.Id )
                    .FirstOrDefault();

                if ( noteWatch == null )
                {
                    noteWatch = new NoteWatch
                    {
                        NoteId = note.Id,
                        WatcherPersonAliasId = RequestContext.CurrentPerson.PrimaryAliasId
                    };

                    noteWatchService.Add( noteWatch );
                }

                noteWatch.IsWatching = request.IsWatching;
                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using DocumentFormat.OpenXml.Wordprocessing;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    /// <summary>
    /// Provides functionality to create and manage notes within the AI agent.
    /// </summary>
    /// <remarks>
    /// This skill allows for operations such as adding, updating, deleting, and listing notes. It also provides tools to look up note types and retrieve specific notes.
    /// </remarks>
    [AgentSkillGuid( "216E5428-DE1A-4458-A22C-22812955264A" )]
    [EntityTypeGuid( "76DD142A-FB37-4B9E-A1F0-305A5B675B76" )]
    [Description( "This skill provides functionality to manage notes." )]
    internal sealed class NoteSkill : AgentSkillComponent
    {
        #region Fields

        /// <summary>
        /// The logger instance for logging messages.
        /// </summary>
        private readonly ILogger<NoteSkill> _logger;

        /// <summary>
        /// The factory for creating RockContext instances.
        /// </summary>
        private readonly IRockContextFactory _rockContextFactory;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteSkill"/> class.
        /// </summary>
        /// <param name="rockContextFactory">Factory to create rock contexts.</param>
        /// <param name="logger">Logger for diagnostics and error reporting.</param>
        public NoteSkill( IRockContextFactory rockContextFactory, ILogger<NoteSkill> logger )
        {
            _rockContextFactory = rockContextFactory ?? throw new ArgumentNullException( nameof( rockContextFactory ) );
            _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
        }

        #endregion

        #region Agent Tools

        /// <summary>
        /// Looks up all available note types for the current user.
        /// </summary>
        /// <returns>A <see cref="RockToolResult"/> containing the list of note types or an error message.</returns>
        [AgentToolGuid( "51046397-D246-4296-A1C0-EC6BF0D01FAA" )]
        public RockToolResult LookupNoteTypes()
        {
            var currentPerson = AgentRequestContext.RockRequestContext.CurrentPerson;
            var noteTypes = GetNoteTypes( currentPerson );

            if ( !noteTypes.Any() )
            {
                return RockToolResult.NoData();
            }

            return RockToolResult.Success( noteTypes )
                .WithHistoryKey( "note-types" );
        }

        /// <summary>
        /// Retrieves a specific note by its identifier key.
        /// </summary>
        /// <param name="idKey">The identifier key of the note to retrieve.</param>
        /// <returns>A <see cref="RockToolResult"/> containing the note details or an error message.</returns>
        [AgentToolGuid( "C5690ED4-5CB3-4299-9E75-1D4E6FF7D323" )]
        public RockToolResult GetNote( string idKey )
        {
            using ( var rockContext = _rockContextFactory.CreateRockContext() )
            {
                var noteService = new Rock.Model.NoteService( rockContext );
                var note = noteService.Get( idKey, false );

                if ( note == null )
                {
                    return RockToolResult.Error( "Invalid note idKey provided." );
                }

                var currentPerson = AgentRequestContext.RockRequestContext.CurrentPerson;
                if ( !note.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) )
                {
                    return RockToolResult.Error( "You are not authorized to view this note." );
                }

                return RockToolResult.Success( GetNoteResult( note, rockContext ) )
                    .WithHistoryContent( new
                    {
                        IdKey = note.IdKey,
                        Text = note.Text.Truncate( 200 ),
                    }, note.IdKey );
            }
        }

        /// <summary>
        /// Adds a new note to the specified entity of the given note type.
        /// </summary>
        /// <param name="noteTypeIdKey">The identifier key of the note type.</param>
        /// <param name="entityIdKey">The identifier key of the entity to associate with the note.</param>
        /// <param name="note">The text of the note.</param>
        /// <param name="isAlert">Indicates whether the note is an alert.</param>
        /// <param name="isPrivateNote">Indicates whether the note is private.</param>
        /// <param name="isPinned">Indicates whether the note is pinned.</param>
        /// <returns>A <see cref="RockToolResult"/> indicating the success or failure of the operation.</returns>
        [AgentToolGuid( "FB0E044A-068A-4B47-9990-B2A582F87B3A" )]
        [AgentUsage( "Adds a note to the specified entity of the given note type." )]
        [AgentToolPrerequisite( "Call the LookupNoteTypes function to determine available note types. Select one that matches the note sentiment." )]
        public RockToolResult AddNote(
            string noteTypeIdKey,
            string entityIdKey,
            string note,
            bool isAlert = false,
            [Description("If the note contains sensitive information that should only be visible to the creator, set this to true." ) ]
            bool isPrivateNote = false,
            bool isPinned = false )
        {
            var currentPerson = AgentRequestContext.RockRequestContext.CurrentPerson;
            if ( currentPerson == null )
            {
                return RockToolResult.Error( "You must be logged in to add a note." );
            }

            var noteType = NoteTypeCache.Get( noteTypeIdKey, false );
            if ( noteType == null )
            {
                return RockToolResult.Error( "Invalid note type provided." )
                    .WithInstructions( "Call the LookupNoteTypes function to determine available note types. Select one that matches the note sentiment." ); ;
            }

            if ( !noteType.IsAuthorized( Authorization.EDIT, currentPerson ) )
            {
                return RockToolResult.Error( "You are not authorized to add a note of this type." );
            }

            var entityId = IdHasher.Instance.GetId( entityIdKey );
            if ( !entityId.HasValue || entityId <= 0 )
            {
                return RockToolResult.Error( "Invalid entity provided." );
            }

            using ( var rockContext = _rockContextFactory.CreateRockContext() )
            {
                var noteService = new Rock.Model.NoteService( rockContext );

                var newNote = new Rock.Model.Note
                {
                    NoteTypeId = noteType.Id,
                    EntityId = entityId.Value,
                    Text = note,
                    IsAlert = isAlert,
                    IsPinned = isPinned,
                    IsSystem = false,
                    IsPrivateNote = isPrivateNote,
                    CreatedByPersonAliasId = currentPerson.PrimaryAliasId,
                };

                noteService.Add( newNote );

                try
                {
                    rockContext.SaveChanges();
                }
                catch ( Exception ex )
                {
                    _logger.LogError( ex, "An error occurred while saving a new note." );
                    return RockToolResult.Error( "An error occurred while saving the note." );
                }

                var result = GetNoteResult( newNote, rockContext );

                return RockToolResult.Success( result )
                .WithHistoryContent( newNote.IdKey, newNote.IdKey )
                .WithInstructions( "The note has been added. Display the text, type, and call out if the note was marked as alert or private." );
            }
        }

        /// <summary>
        /// Updates an existing note.
        /// </summary>
        /// <param name="noteIdKey">The identifier key of the note to update.</param>
        /// <param name="note">The updated text of the note (optional).</param>
        /// <param name="noteTypeIdKey">The updated note type identifier key (optional).</param>
        /// <param name="isAlert">The updated alert status (optional).</param>
        /// <param name="isPrivateNote">The updated private status (optional).</param>
        /// <param name="isPinned">The updated pinned status (optional).</param>
        /// <returns>A <see cref="RockToolResult"/> indicating the success or failure of the operation.</returns>
        [AgentToolGuid( "322A8DE0-6F51-4882-9EEB-8A8792607A8B" )]
        public RockToolResult UpdateNote(
            string noteIdKey,
            string note = null,
            string noteTypeIdKey = null,
            bool? isAlert = null,
            bool? isPrivateNote = null,
            bool? isPinned = null )
        {
            var currentPerson = AgentRequestContext.RockRequestContext.CurrentPerson;
            if ( currentPerson == null )
            {
                return RockToolResult.Error( "You must be logged in to update a note." );
            }

            using ( var rockContext = _rockContextFactory.CreateRockContext() )
            {
                var noteService = new Rock.Model.NoteService( rockContext );
                var existingNote = noteService.Get( noteIdKey, false );

                if ( existingNote == null )
                {
                    return RockToolResult.Error( "Invalid note idKey provided." );
                }

                if ( !existingNote.NoteType.IsAuthorized( Authorization.EDIT, currentPerson ) )
                {
                    return RockToolResult.Error( "You are not authorized to edit this note." );
                }

                if ( note.IsNotNullOrWhiteSpace() )
                {
                    existingNote.Text = note;
                }

                if ( noteTypeIdKey.IsNotNullOrWhiteSpace() )
                {
                    var noteType = NoteTypeCache.Get( noteTypeIdKey, false );
                    if ( noteType == null )
                    {
                        return RockToolResult.Error( "Invalid note type." );
                    }
                    if ( !noteType.IsAuthorized( Authorization.EDIT, currentPerson ) )
                    {
                        return RockToolResult.Error( "You are not authorized to change this note to the specified type." );
                    }
                    existingNote.NoteTypeId = noteType.Id;
                }

                if ( isAlert.HasValue )
                {
                    existingNote.IsAlert = isAlert.Value;
                }

                if ( isPrivateNote.HasValue )
                {
                    existingNote.IsPrivateNote = isPrivateNote.Value;
                }

                if ( isPinned.HasValue )
                {
                    existingNote.IsPinned = isPinned.Value;
                }

                try
                {
                    rockContext.SaveChanges();
                }
                catch ( Exception ex )
                {
                    _logger.LogError( ex, "An error occurred while updating a note." );
                    return RockToolResult.Error( "An error occurred while updating the note." );
                }

                var result = GetNoteResult( existingNote, rockContext );

                return RockToolResult.Success( result )
                .WithHistoryContent( noteIdKey, noteIdKey );
            }
        }

        /// <summary>
        /// Deletes a note by its identifier key.
        /// </summary>
        /// <param name="idKey">The identifier key of the note to delete.</param>
        /// <returns>A <see cref="RockToolResult"/> indicating the success or failure of the operation.</returns>
        [AgentToolGuid( "DC4F7ABA-50F1-4ADD-A1E0-A9DAE8D51D2D" )]
        [AgentGuardrail( "This action will permanently delete the specified note. Ensure that this action is intentional and that you have the correct note identifier before proceeding." )]
        public RockToolResult DeleteNote( string idKey )
        {
            var currentPerson = AgentRequestContext.RockRequestContext.CurrentPerson;
            if ( currentPerson == null )
            {
                return RockToolResult.Error( "You must be logged in to update a note." );
            }

            using ( var rockContext = _rockContextFactory.CreateRockContext() )
            {
                var noteService = new Rock.Model.NoteService( rockContext );
                var existingNote = noteService.Get( idKey, false );

                if ( existingNote == null )
                {
                    return RockToolResult.Error( "Invalid note idKey provided." );
                }

                if ( !existingNote.NoteType.IsAuthorized( Authorization.EDIT, currentPerson ) )
                {
                    return RockToolResult.Error( "You are not authorized to delete this note." );
                }

                noteService.Delete( existingNote );

                try
                {
                    rockContext.SaveChanges();
                }
                catch ( Exception ex )
                {
                    _logger.LogError( ex, "An error occurred while deleting a note." );
                    return RockToolResult.Error( "An error occurred while deleting the note." );
                }

                return RockToolResult.Success()
                    .WithHistoryContent( idKey, idKey );
            }
        }

        /// <summary>
        /// Lists notes based on the specified filters.
        /// </summary>
        /// <param name="startDate">The start date for filtering notes (optional).</param>
        /// <param name="endDate">The end date for filtering notes (optional).</param>
        /// <param name="noteTypeIdKey">The identifier key of the note type to filter by (optional).</param>
        /// <param name="entityIdKey">The identifier key of the entity to filter by (optional).</param>
        /// <param name="entityTypeIdKey">The identifier key of the entity type to filter by (optional).</param>
        /// <param name="createdByPersonIdKey">The identifier key of the person who created the notes (optional).</param>
        /// <param name="isAlert">The alert status to filter by (optional).</param>
        /// <param name="isPrivateNote">The private status to filter by (optional).</param>
        /// <param name="isPinned">The pinned status to filter by (optional).</param>
        /// <param name="pageNumber">The page number for pagination (optional).</param>
        /// <returns>A <see cref="RockToolResult"/> containing the list of notes or an error message.</returns>
        [AgentToolGuid( "22B609E6-5D0A-4588-8BB9-456EF6F7D4A4" )]
        public RockToolResult ListNotes(
            DateTime? startDate = null,
            DateTime? endDate = null,
            string noteTypeIdKey = null,
            string entityIdKey = null,
            string entityTypeIdKey = null,
            string createdByPersonIdKey = null,
            bool? isAlert = null,
            bool? isPrivateNote = null,
            bool? isPinned = null,
            int pageNumber = 1 )
        {
            var currentPerson = AgentRequestContext.RockRequestContext.CurrentPerson;

            // Normalize and validate the inputs.
            var pgNumber = pageNumber < 1 ? 1 : pageNumber;
            const int pageSize = 25;
            var offset = ( pgNumber - 1 ) * pageSize;
            var take = pageSize + 1; // lookahead for hasMore

            if ( startDate.HasValue && endDate.HasValue && endDate.Value <= startDate.Value )
            {
                return RockToolResult.Error( "The endDate must be after the startDate." );
            }

            using ( var rockContext = _rockContextFactory.CreateRockContext() )
            {
                var noteService = new Rock.Model.NoteService( rockContext );

                // Base query + eager loads to avoid N+1 during projection
                var notesQuery = noteService.Queryable()
                    .AsNoTracking()
                    .Include( n => n.CreatedByPersonAlias.Person )
                    .Include( n => n.NoteType )
                    .Where( n => n.NoteType.UserSelectable );

                if ( startDate.HasValue )
                {
                    notesQuery = notesQuery.Where( n => n.CreatedDateTime >= startDate.Value );
                }

                if ( endDate.HasValue )
                {
                    // exclusive end is usually cleaner for paging windows
                    notesQuery = notesQuery.Where( n => n.CreatedDateTime < endDate.Value );
                }

                string noteTypeName = null;
                if ( noteTypeIdKey.IsNotNullOrWhiteSpace() )
                {
                    var noteType = NoteTypeCache.Get( noteTypeIdKey, false );
                    if ( noteType == null )
                    {
                        return RockToolResult.Error( "Invalid note type." );
                    }
                    noteTypeName = noteType.Name;
                    notesQuery = notesQuery.Where( n => n.NoteTypeId == noteType.Id );
                }

                if ( entityIdKey.IsNotNullOrWhiteSpace() )
                {
                    var entityId = IdHasher.Instance.GetId( entityIdKey );
                    if ( !entityId.HasValue || entityId <= 0 )
                    {
                        return RockToolResult.Error( "Invalid entity provided." );
                    }
                    notesQuery = notesQuery.Where( n => n.EntityId == entityId.Value );
                }

                // BC TODO: Should this come from an enum?
                // How should the LLM know what the values are?
                if ( entityTypeIdKey.IsNotNullOrWhiteSpace() )
                {
                    var entityTypeId = IdHasher.Instance.GetId( entityTypeIdKey );
                    if ( !entityTypeId.HasValue || entityTypeId <= 0 )
                    {
                        return RockToolResult.Error( "Invalid entity type provided." );
                    }

                    notesQuery = notesQuery.Where( n => n.NoteType.EntityTypeId == entityTypeId.Value );
                }

                if ( createdByPersonIdKey.IsNotNullOrWhiteSpace() )
                {
                    var createdByPersonId = IdHasher.Instance.GetId( createdByPersonIdKey );
                    if ( !createdByPersonId.HasValue || createdByPersonId <= 0 )
                    {
                        return RockToolResult.Error( "Invalid created by person provided." );
                    }

                    notesQuery = notesQuery.Where( n => n.CreatedByPersonAlias.PersonId == createdByPersonId.Value );
                }

                if ( isAlert.HasValue )
                {
                    notesQuery = notesQuery.Where( n => n.IsAlert == isAlert.Value );
                }

                if ( isPrivateNote.HasValue )
                {
                    notesQuery = notesQuery.Where( n => n.IsPrivateNote == isPrivateNote.Value );
                }

                if ( isPinned.HasValue )
                {
                    notesQuery = notesQuery.Where( n => n.IsPinned == isPinned.Value );
                }

                // Deterministic order (coalesce nulls)
                var ordered = notesQuery
                    .OrderByDescending( n => n.CreatedDateTime ?? DateTime.MinValue )
                    .ThenBy( n => n.Id );

                // We need enough *authorized* rows to page correctly: offset + take
                var needed = offset + take;
                var buffer = new List<Rock.Model.Note>( needed + pageSize );

                // Pull in chunks to avoid materializing the whole result set
                var fetched = 0;
                var chunk = Math.Max( pageSize * 2, 50 );

                while ( buffer.Count < needed )
                {
                    var batch = ordered.Skip( fetched ).Take( chunk ).ToList();
                    if ( batch.Count == 0 )
                    {
                        break;
                    }

                    foreach ( var n in batch )
                    {
                        if ( n.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) )
                        {
                            buffer.Add( n );
                            if ( buffer.Count >= needed )
                            {
                                break;
                            }
                        }
                    }

                    fetched += batch.Count;
                }

                // Page over the AUTHORIZED subset (+ lookahead)
                var pageSlice = buffer.Skip( offset ).Take( take ).ToList();
                if ( pageSlice.Count == 0 )
                {
                    return RockToolResult.NoData();
                }

                var hasMore = pageSlice.Count > pageSize;
                if ( hasMore )
                {
                    pageSlice.RemoveAt( pageSlice.Count - 1 );
                }

                // Project AFTER paging (saves CPU/mem)
                var items = pageSlice
                    .Select( note => GetNoteResult( note, rockContext ) )
                    .ToList();

                // Slim it down for the history content
                var historyItems = items.Select( note => new
                {
                    IdKey = note.IdKey,
                    Text = note.Text.Truncate( 200 ),
                } );

                // Metadata
                var meta = new Dictionary<string, object>
                {
                    { "pageNumber", pgNumber },
                    { "pageSize", pageSize },
                    { "returnedRows", items.Count },
                    { "hasMore", hasMore },
                    { "startDate", startDate },
                    { "endDate", endDate },
                    { "filters", new Dictionary<string, object>
                        {
                            { "isAlert", isAlert },
                            { "isPrivateNote", isPrivateNote },
                            { "noteType", noteTypeName ?? "None" },
                        }
                    }
                };

                return RockToolResult.Success( items )
                    .WithHistoryContent( new
                    {
                        Items = historyItems,
                        PageNumber = pageNumber
                    }, "notes-list" )
                    .WithMetadata( meta );
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Retrieves the list of note types available to the current user.
        /// </summary>
        /// <param name="currentPerson">The current person.</param>
        /// <returns>A list of <see cref="NoteTypeResult"/> objects representing the available note types.</returns>
        private static List<NoteTypeResult> GetNoteTypes( Rock.Model.Person currentPerson )
        {
            var noteTypes = NoteTypeCache.All()
                .Where( nt => nt.UserSelectable )
                .Where( a => a.IsAuthorized( Authorization.VIEW, currentPerson ) );

            var noteTypeResults = new List<NoteTypeResult>();

            foreach ( var noteType in noteTypes )
            {
                // Populate entity type information
                var noteTypeResult = new NoteTypeResult
                {
                    Id = noteType.Id,
                    Name = noteType.Name,
                };

                if ( noteType.EntityTypeId.HasValue )
                {
                    var entityType = EntityTypeCache.Get( noteType.EntityTypeId.Value );
                    if ( entityType != null )
                    {
                        noteTypeResult.EntityType = new KeyNameResult
                        {
                            Id = entityType.Id,
                            Name = entityType.FriendlyName,
                        };
                    }
                }

                noteTypeResults.Add( noteTypeResult );
            }

            return noteTypeResults;
        }

        /// <summary>
        /// Retrieves the result object for a specific note.
        /// </summary>
        /// <param name="note">The note object.</param>
        /// <param name="rockContext">The Rock context.</param>
        /// <returns>A <see cref="NoteResult"/> object containing the note details.</returns>
        private static NoteResult GetNoteResult( Rock.Model.Note note, RockContext rockContext )
        {
            if ( note == null )
            {
                return null;
            }

            // This may not be populated since we could have just saved, so eager
            // load it if necessary.
            var noteType = NoteTypeCache.Get( note.NoteTypeId );
            string entityName = null;
            string entityTypeName = null;

            // Fetch the entity type and name
            if ( note.EntityId.HasValue && noteType.EntityTypeId.HasValue )
            {
                var entity = new EntityTypeService( rockContext ).GetEntity( noteType.EntityTypeId.Value, note.EntityId.Value );

                if ( entity != null )
                {
                    entityName = entity?.ToString();
                }
            }

            if ( noteType.EntityTypeId.HasValue )
            {
                var entityType = EntityTypeCache.Get( noteType.EntityTypeId.Value );
                entityTypeName = entityType.FriendlyName;
            }

            return new NoteResult
            {
                Author = note.CreatedByPersonAlias != null
                    ? new PersonResult
                    {
                        Id = note.CreatedByPersonAlias.Person.Id,
                        NickName = note.CreatedByPersonAlias.Person.NickName,
                        LastName = note.CreatedByPersonAlias.Person.LastName
                    }
                    : null,
                NoteType = new NoteTypeResult
                {
                    EntityType = new KeyNameResult
                    {
                        Id = noteType.EntityTypeId,
                        Name = entityTypeName
                    },
                    Id = noteType.Id,
                    Name = noteType.Name
                },
                EntityName = entityName,
                Caption = note.Caption,
                IsAlert = note.IsAlert ?? false,
                Id = note.Id,
                IsPinned = note.IsPinned,
                IsPrivate = note.IsPrivateNote,
                Text = note.Text,
            };
        }

        #endregion
    }
}

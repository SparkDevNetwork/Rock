using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using Rock.AI.Agent.Classes.Common;
using Rock.SystemGuid;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class NoteSkill
    {
        #region Tool(s)

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
    }
}

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.Attribute;
using Rock.Configuration;
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
            string cursor = null )
        {
            var currentPerson = AgentRequestContext.RockRequestContext.CurrentPerson;
            var helper = new AgentToolHelper( AgentRequestContext, _logger );
            var noteService = new Rock.Model.NoteService( AgentRequestContext.RockContext );

            // Base query + eager loads to avoid N+1 during projection
            var qry = noteService.Queryable()
                .AsNoTracking()
                .Include( n => n.CreatedByPersonAlias.Person )
                .Include( n => n.NoteType )
                .Where( n => n.NoteType.UserSelectable );

            qry = helper.WhereOptionalPropertyBetween( qry, n => n.CreatedDateTime, startDate, endDate );
            qry = helper.WhereOptionalIdKey( qry, n => n.NoteTypeId, noteTypeIdKey );
            qry = helper.WhereOptionalIdKey( qry, n => n.EntityId, entityIdKey );
            qry = helper.WhereOptionalIdKey( qry, n => n.NoteType.EntityTypeId, entityTypeIdKey );
            qry = helper.WhereOptionalIdKey( qry, n => n.CreatedByPersonAlias.PersonId, createdByPersonIdKey );
            qry = helper.WhereOptionalProperty( qry, n => n.IsAlert, isAlert );
            qry = helper.WhereOptionalProperty( qry, n => n.IsPrivateNote, isPrivateNote );
            qry = helper.WhereOptionalProperty( qry, n => n.IsPinned, isPinned );

            var paginator = new Data.CursorPaginator<Model.Note>( q =>
                q.OrderByDescending( n => n.CreatedDateTime )
                .ThenBy( n => n.Id ) );

            var cursorPage = helper.GetCursorPaginatedItems( qry, paginator, cursor );

            var resultPage = cursorPage.WithItems( cursorPage.Items
                    .Select( n => GetNoteResult( n, AgentRequestContext.RockContext ) )
                    .ToList() );

            var historyPage = cursorPage.WithItems( cursorPage.Items.Select( n => new
            {
                n.IdKey,
                Name = n.Text.Truncate( 200 ),
            } ).ToList() );

            return helper.GetPaginatedResult( resultPage, historyPage );
        }

        #endregion
    }
}

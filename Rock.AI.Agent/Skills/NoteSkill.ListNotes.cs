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

using System;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class NoteSkill
{
    #region Tool(s)

    [Description( "Lists notes that match the specified criteria." )]
    [AgentToolGuid( "22B609E6-5D0A-4588-8BB9-456EF6F7D4A4" )]
    public AgentToolResult ListNotes(
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
        var currentPerson = AgentRequestContext.CurrentPerson;
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

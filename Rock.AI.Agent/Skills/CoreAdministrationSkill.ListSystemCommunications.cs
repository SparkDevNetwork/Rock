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

using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.CoreAdministrationSkill;
using Rock.Data;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CoreAdministrationSkill
{
    #region Tool(s)

    /// <summary>
    /// Lists the system communication templates configured in Rock.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the only tool in the skill that uses cursor paging, and the only
    /// one that must. There is no SystemCommunicationCache, so it genuinely
    /// queries the database, and SystemCommunication derives from Model&lt;T&gt;
    /// and is therefore secured per row.
    /// </para>
    /// <para>
    /// That combination is exactly what CursorPaginator exists for.
    /// GetPaginatedItems performs no authorization at all, and IsAuthorized
    /// cannot be translated to SQL, so the fetch, filter, and refetch loop inside
    /// the paginator is the only correct mechanism. Do not assume an entity is
    /// unsecured without checking: nearly every Rock entity derives from
    /// Model&lt;T&gt;.
    /// </para>
    /// </remarks>
    [Description( "Lists the system communication templates configured in Rock, such as the templates used by workflow actions to send email." )]
    [AgentPurpose( "Finds the system communication template to use when configuring something that sends a message." )]
    [AgentToolGuid( "83AFE4C8-F8BC-4BF8-A7D6-6FDCF8AD8561" )]
    public AgentToolResult ListSystemCommunications( string partialName = null, string categoryIdKey = null, string cursor = null )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        var currentPerson = AgentRequestContext.CurrentPerson;

        var query = new SystemCommunicationService( AgentRequestContext.RockContext ).Queryable();

        if ( partialName.IsNotNullOrWhiteSpace() )
        {
            query = query.Where( sc => sc.Title.Contains( partialName ) );
        }

        query = helper.WhereOptionalIdKey( query, sc => sc.CategoryId, categoryIdKey );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        // The ThenBy on Id is required, not stylistic. Without a unique
        // tiebreaker two identically titled communications produce the same
        // cursor, and the seek predicate then silently skips or repeats a row.
        var paginator = new CursorPaginator<Rock.Model.SystemCommunication>( currentPerson, qry => qry
            .OrderBy( sc => sc.Title )
            .ThenBy( sc => sc.Id ) );

        var cursorPage = helper.GetCursorPaginatedItems( query, paginator, cursor );

        var resultPage = cursorPage.WithItems( cursorPage.Items
            .Select( sc => new SystemCommunicationResult
            {
                Id = sc.Id,
                Guid = sc.Guid,
                Title = sc.Title,
                Category = sc.Category != null
                    ? new KeyNameResult { Id = sc.Category.Id, Guid = sc.Category.Guid, Name = sc.Category.Name }
                    : null
            } )
            .ToList() );

        var historyPage = cursorPage.WithItems( cursorPage.Items
            .Select( sc => new KeyNameResult { Id = sc.Id, Guid = sc.Guid, Name = sc.Title } ) );

        return helper.GetPaginatedResult( resultPage, historyPage );
    }

    #endregion
}

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
using Rock.AI.Agent.Classes.Skills.ReportingSkill;
using Rock.Data;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class ReportingSkill
{
    #region Tool(s)

    /// <summary>
    /// Lists the reports configured in Rock for an entity type.
    /// </summary>
    /// <remarks>
    /// A live instance can hold hundreds of reports, so the entity type is
    /// required to narrow the set and the results are cursor paged. Reports derive
    /// from Model&lt;T&gt; and are secured per row, so this uses CursorPaginator to
    /// filter by view permission while paging.
    /// </remarks>
    [Description( "Lists the reports configured in Rock for a given entity type. Reports are the saved, formatted outputs built on a data view or entity." )]
    [AgentPurpose( "Finds a report so it can be read or run." )]
    [AgentToolPrerequisite( "Call ListEntityTypes to determine the entityTypeIdKey, and ListCategories with the Report entity type to determine the categoryIdKey." )]
    [AgentToolGuid( "6729CFE6-C23F-4DD2-BBF9-DBBD5F5C190A" )]
    public AgentToolResult ListReports( string entityTypeIdKey, string partialName = null, string categoryIdKey = null, string cursor = null )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        var currentPerson = AgentRequestContext.CurrentPerson;

        var entityType = helper.GetRequiredEntity<Rock.Model.EntityType>( entityTypeIdKey, checkSecurity: false );

        if ( entityType == null )
        {
            return helper.ErrorResult
                .WithInstructions( "Call the ListEntityTypes function to determine the available entity types." );
        }

        var query = new ReportService( AgentRequestContext.RockContext ).Queryable()
            .Where( r => r.EntityTypeId == entityType.Id );

        if ( partialName.IsNotNullOrWhiteSpace() )
        {
            query = query.Where( r => r.Name.Contains( partialName ) );
        }

        query = helper.WhereOptionalIdKey( query, r => r.CategoryId, categoryIdKey );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        // The ThenBy on Id is the unique tiebreaker the cursor needs so two
        // identically named reports cannot produce the same cursor.
        var paginator = new CursorPaginator<Rock.Model.Report>( currentPerson, qry => qry
            .OrderBy( r => r.Name )
            .ThenBy( r => r.Id ) );

        var cursorPage = helper.GetCursorPaginatedItems( query, paginator, cursor );

        var resultPage = cursorPage.WithItems( cursorPage.Items
            .Select( r => new ReportResult
            {
                Id = r.Id,
                Guid = r.Guid,
                Name = r.Name,
                Category = r.Category != null
                    ? new KeyNameResult { Id = r.Category.Id, Guid = r.Category.Guid, Name = r.Category.Name }
                    : null
            } )
            .ToList() );

        var historyPage = cursorPage.WithItems( cursorPage.Items
            .Select( r => new KeyNameResult { Id = r.Id, Guid = r.Guid, Name = r.Name } ) );

        return helper.GetPaginatedResult( resultPage, historyPage );
    }

    #endregion
}

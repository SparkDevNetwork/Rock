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
    /// Lists the data views configured in Rock for an entity type.
    /// </summary>
    /// <remarks>
    /// Data views run to the thousands on a large instance, so the entity type is
    /// required and results are cursor paged. Data views derive from Model&lt;T&gt;
    /// and are secured per row, so this uses CursorPaginator to filter by view
    /// permission while paging.
    /// </remarks>
    [Description( "Lists the data views configured in Rock for a given entity type. A data view is a saved, reusable filter that selects a set of records." )]
    [AgentPurpose( "Finds a data view so it can be read or run." )]
    [AgentToolPrerequisite( "Call ListEntityTypes to determine the entityTypeIdKey, and ListCategories with the DataView entity type to determine the categoryIdKey." )]
    [AgentToolGuid( "B7EA3A42-8C2A-42AA-B244-FA14F6551DD6" )]
    public AgentToolResult ListDataViews( string entityTypeIdKey, string partialName = null, string categoryIdKey = null, string cursor = null )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        var currentPerson = AgentRequestContext.CurrentPerson;

        var entityType = helper.GetRequiredEntity<Rock.Model.EntityType>( entityTypeIdKey, checkSecurity: false );

        if ( entityType == null )
        {
            return helper.ErrorResult
                .WithInstructions( "Call the ListEntityTypes function to determine the available entity types." );
        }

        var query = new DataViewService( AgentRequestContext.RockContext ).Queryable()
            .Where( dv => dv.EntityTypeId == entityType.Id );

        if ( partialName.IsNotNullOrWhiteSpace() )
        {
            query = query.Where( dv => dv.Name.Contains( partialName ) );
        }

        query = helper.WhereOptionalIdKey( query, dv => dv.CategoryId, categoryIdKey );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var paginator = new CursorPaginator<Rock.Model.DataView>( currentPerson, qry => qry
            .OrderBy( dv => dv.Name )
            .ThenBy( dv => dv.Id ) );

        var cursorPage = helper.GetCursorPaginatedItems( query, paginator, cursor );

        var resultPage = cursorPage.WithItems( cursorPage.Items
            .Select( dv => new DataViewResult
            {
                Id = dv.Id,
                Guid = dv.Guid,
                Name = dv.Name,
                Category = dv.Category != null
                    ? new KeyNameResult { Id = dv.Category.Id, Guid = dv.Category.Guid, Name = dv.Category.Name }
                    : null
            } )
            .ToList() );

        var historyPage = cursorPage.WithItems( cursorPage.Items
            .Select( dv => new KeyNameResult { Id = dv.Id, Guid = dv.Guid, Name = dv.Name } ) );

        return helper.GetPaginatedResult( resultPage, historyPage );
    }

    #endregion
}

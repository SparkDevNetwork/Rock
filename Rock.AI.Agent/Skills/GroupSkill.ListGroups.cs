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
using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.Data;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class GroupSkill
{
    #region Tool(s)

    [Description( "Returns a list of groups." )]
    [AgentPurpose( "Returns a list of groups." )]
    [AgentToolGuid( "94fde11f-6243-4f4e-8854-66c9625b9de1" )]
    public IAgentToolResult ListGroups(
        string groupTypeIdKey,
        string partialName = null,
        string cursor = null )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        var currentPerson = AgentRequestContext.CurrentPerson;

        var query = new GroupService( AgentRequestContext.RockContext )
            .Queryable()
            .Where( g => g.IsActive );

        query = helper.WhereOptionalIdKey( query, g => g.GroupTypeId, groupTypeIdKey );

        if ( partialName.IsNotNullOrWhiteSpace() )
        {
            query = query.Where( g => g.Name.Contains( partialName ) );
        }

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var paginator = new CursorPaginator<Model.Group>( currentPerson, qry => qry
            .OrderBy( cr => cr.Name )
            .ThenBy( cr => cr.Id ) );

        var cursorPage = helper.GetCursorPaginatedItems( query, paginator, cursor );

        cursorPage.Items.LoadAttributes( AgentRequestContext.RockContext );

        var resultPage = cursorPage.WithItems( cursorPage.Items
            .Select( g => new GroupResult
            {
                Id = g.Id,
                Name = g.Name,
                AttributeValues = g.GetGridAttributeValueResults( AgentRequestContext ).ToList(),
            } )
            .ToList() );

        var historyPage = cursorPage.WithItems( cursorPage.Items.Select( cr => new KeyNameResult
        {
            Id = cr.Id,
            Name = cr.ToString()
        } ) );

        return helper.GetPaginatedResult( resultPage, historyPage );
    }

    #endregion
}

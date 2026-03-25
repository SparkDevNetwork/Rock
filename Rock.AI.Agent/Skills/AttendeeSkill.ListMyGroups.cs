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
using System.Data.Entity;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.Data;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class AttendeeSkill
{
    #region Tool(s)

    [Description( "Returns a list of groups the current person is a member of." )]
    [AgentPurpose( "Returns a list of groups the current person is a member of." )]
    [AgentToolGuid( "5ca4d7d7-52be-4e07-94dd-adb284641903" )]
    public IAgentToolResult ListMyGroups( string cursor = null )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        var currentPerson = AgentRequestContext.CurrentPerson;
        var groupTypeIds = GetConfiguredGroupTypes().Select( gt => gt.Id ).ToList();

        if ( currentPerson == null )
        {
            return Error( "A user must be logged in to list their groups." );
        }

        var query = new GroupService( AgentRequestContext.RockContext )
            .Queryable()
            .Include( g => g.GroupType )
            .Where( g => g.IsActive
                && groupTypeIds.Contains( g.GroupTypeId )
                && g.Members.Any( gm => gm.PersonId == currentPerson.Id
                    && !gm.IsArchived
                    && gm.GroupMemberStatus == GroupMemberStatus.Active ) );

        var paginator = new CursorPaginator<Model.Group>( currentPerson, qry => qry
            .OrderBy( g => g.Name )
            .ThenBy( g => g.Id ) );

        var cursorPage = helper.GetCursorPaginatedItems( query, paginator, cursor );

        var resultPage = cursorPage.WithItems( cursorPage.Items
            .Select( g => new GroupResult
            {
                Id = g.Id,
                Name = g.Name,
                GroupType = new GroupTypeResult
                {
                    Name = g.GroupType?.Name
                },
            } )
            .ToList() );

        var historyPage = cursorPage.WithItems( cursorPage.Items.Select( g => new KeyNameResult
        {
            Id = g.Id,
            Name = g.Name,
        } ) );

        return helper.GetPaginatedResult( resultPage, historyPage );
    }

    #endregion
}

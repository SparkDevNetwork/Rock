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
using Rock.AI.Agent.Classes.Entity;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class GroupSkill
{
    #region Tool(s)

    [Description( "Returns a list of group members." )]
    [AgentPurpose( "Returns a list of group members." )]
    [AgentToolGuid( "66580b04-14e6-4fa8-8367-efe8f2a7e7ed" )]
    public AgentToolResult ListGroupMembers(
        string groupIdKey,
        string roleIdKey = null,
        string personIdKey = null,
        int pageNumber = 1 )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        var currentPerson = AgentRequestContext.CurrentPerson;
        var groupTypeIds = GetConfiguredGroupTypes().Select( gt => gt.Id ).ToList();

        var group = helper.GetRequiredEntity<Model.Group>( groupIdKey, checkSecurity: true );

        if ( group != null && !group.IsAuthorized( Authorization.VIEW, currentPerson ) )
        {
            helper.AddError( "You do not have permission to view the group." );
        }

        if ( group != null && !groupTypeIds.Contains( group.GroupTypeId ) )
        {
            helper.AddError( "The specified group is not of a valid group type." );
        }

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var query = new GroupMemberService( AgentRequestContext.RockContext )
            .Queryable();

        query = helper.WhereRequiredIdKey( query, gm => gm.GroupId, groupIdKey );
        query = helper.WhereOptionalIdKey( query, gm => gm.GroupRoleId, roleIdKey );
        query = helper.WhereOptionalIdKey( query, gm => gm.PersonId, personIdKey );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var projectedQry = query
            .AsExpandable()
            .OrderBy( gm => gm.Person.LastName )
            .ThenBy( gm => gm.Person.NickName )
            .ThenBy( gm => gm.Person.Id )
            .Select( gm => new GroupMemberResult
            {
                Id = gm.Id,
                Guid = gm.Guid,
                Person = PersonResult.NameOnly( gm.Person ),
                Role = new KeyNameResult
                {
                    Id = gm.GroupRoleId,
                    Guid = gm.GroupRole.Guid,
                    Name = gm.GroupRole.Name
                },
                Status = gm.GroupMemberStatus,
                AttributeValues = gm.GroupMemberAttributeValues.GetGridAttributeValueResults( AgentRequestContext ).ToList(),
            } );

        var page = helper.GetPaginatedItems( projectedQry, pageNumber );

        var historyPage = page.WithItems( page.Items.Select( gm => new GroupMemberResult
        {
            Id = gm.Id,
            Guid = gm.Guid,
            Person = gm.Person,
        } ) );

        return helper.GetPaginatedResult( page, historyPage );
    }

    #endregion
}

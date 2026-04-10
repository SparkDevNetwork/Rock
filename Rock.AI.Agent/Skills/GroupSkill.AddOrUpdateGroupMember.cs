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

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.Configuration;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class GroupSkill
{
    #region Tool(s)

    [Description( "Adds a new or updates an existing group member." )]
    [AgentToolGuid( "085141cc-09c1-40d8-831c-faf4da96d604" )]
    public AgentToolResult AddOrUpdateGroupMember(
        string groupMemberIdKey = null,
        string groupIdKey = null,
        string personIdKey = null,
        string roleIdKey = null,
        GroupMemberStatus? status = null,
        List<AttributeValueResult> attributeValues = null )
    {
        using var rockContext = RockApp.Current.CreateRockContext();
        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );
        var groupTypeIds = GetConfiguredGroupTypes().Select( gt => gt.Id ).ToList();

        GroupMember groupMember;

        if ( groupMemberIdKey.IsNotNullOrWhiteSpace() )
        {
            groupMember = helper.GetRequiredEntity<GroupMember>( groupMemberIdKey );
        }
        else
        {
            groupMember = rockContext.Set<GroupMember>().Create();
            new GroupMemberService( rockContext ).Add( groupMember );

            groupMember.GroupMemberStatus = GroupMemberStatus.Active;
            groupMember.DateTimeAdded = RockDateTime.Now;

            if ( groupIdKey.IsNullOrWhiteSpace() )
            {
                helper.AddError( $"{nameof( groupIdKey )} is required when creating a new group member." );
            }

            if ( roleIdKey.IsNullOrWhiteSpace() )
            {
                helper.AddError( $"{nameof( roleIdKey )} is required when creating a new group member." );
            }
        }

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        if ( groupMember.Id == 0 )
        {
            helper.UpdateNavigationProperty( groupMember, gm => gm.Group, groupIdKey );
        }
        else if ( groupIdKey.IsNotNullOrWhiteSpace() )
        {
            helper.AddError( $"{nameof( groupIdKey )} cannot be specified when updating an existing group member." );
        }

        if ( !groupTypeIds.Contains( groupMember.Group.GroupTypeId ) )
        {
            helper.AddError( $"The specified group is not of a supported group type." );
        }

        helper.UpdateNavigationProperty( groupMember, gm => gm.Person, personIdKey );
        helper.UpdateNavigationProperty( groupMember, gm => gm.GroupRole, roleIdKey );
        helper.UpdateProperty( groupMember, gm => gm.GroupMemberStatus, status );

        helper.SetAttributeValues( groupMember, attributeValues, enforceSecurity: true );

        helper.SaveChangesIfNoErrors();

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var result = new GroupMemberResult
        {
            Id = groupMember.Id,
            Person = PersonResult.NameOnly( groupMember.Person ),
            Group = new KeyNameResult( groupMember.Group.Id, groupMember.Group.Name ),
            Role = new KeyNameResult( groupMember.GroupRole.Id, groupMember.GroupRole.Name ),
            Status = groupMember.GroupMemberStatus,
            AttributeValues = [.. groupMember.GetAttributeValueResults( AgentRequestContext )],
        };

        var toolResult = Success( result )
            .WithHistoryContent( new KeyNameResult
            {
                Id = groupMember.Id,
            } )
            .WithInstructions( $"The group member has been {( groupMemberIdKey.IsNullOrWhiteSpace() ? "created" : "updated" )}." ); ;

        return toolResult;
    }

    #endregion
}

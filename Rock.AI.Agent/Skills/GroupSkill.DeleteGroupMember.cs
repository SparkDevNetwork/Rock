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
using System.Linq;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Annotations;
using Rock.Configuration;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class GroupSkill
{
    #region Tool(s)

    [Description( "Removes a group member from a group." )]
    [AgentToolGuid( "c2800552-0beb-4f54-8f1d-5eeb01df4192" )]
    [AgentGuardrail( "This action will permanently delete the specified group member. Ensure that this action is intentional and that you have the correct group member identifier before proceeding." )]
    public AgentToolResult DeleteGroupMember( string groupMemberIdKey )
    {
        using var rockContext = RockApp.Current.CreateRockContext();
        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );
        var currentPerson = AgentRequestContext.CurrentPerson;
        var groupMemberService = new GroupMemberService( rockContext );
        var groupTypeIds = GetAvailableGroupTypes().Select( gt => gt.Id ).ToList();

        var existingGroupMember = helper.GetRequiredEntity<GroupMember>( groupMemberIdKey, checkSecurity: false );

        if ( existingGroupMember != null && !groupTypeIds.Contains( existingGroupMember.GroupTypeId ) )
        {
            if ( !CanGroupTypeBeConfiguredForRequest( existingGroupMember.GroupTypeId, helper ) )
            {
                helper.AddError( "The specified group member does not belong to a valid group type." );
            }
        }

        if ( existingGroupMember?.Group != null )
        {
            var isAuthorized = existingGroupMember.Group.IsAuthorized( Authorization.EDIT, currentPerson )
                || existingGroupMember.Group.IsAuthorized( Authorization.MANAGE_MEMBERS, currentPerson );

            if ( !isAuthorized )
            {
                helper.AddError( $"You do not have permission to manage the specified group's members." );
            }
        }

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        groupMemberService.Delete( existingGroupMember );

        try
        {
            rockContext.SaveChanges();
        }
        catch ( Exception ex )
        {
            _logger.LogError( ex, "An error occurred while deleting a group member." );
            return Error( "An error occurred while deleting the group member." );
        }

        return Success( "The group member has been deleted." );
    }

    #endregion
}

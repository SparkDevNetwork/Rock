using System.Collections.Generic;
using System.ComponentModel;

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
    public IAgentToolResult AddOrUpdateGroupMember(
        string groupMemberIdKey = null,
        string groupIdKey = null,
        string personIdKey = null,
        string roleIdKey = null,
        GroupMemberStatus? status = null,
        List<AttributeValueResult> attributeValues = null )
    {
        using var rockContext = RockApp.Current.CreateRockContext();
        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );

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

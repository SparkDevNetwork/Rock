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
using Rock.AI.Agent.Classes.Skills.CommunicationSkill;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CommunicationSkill
{
    #region Tool(s)

    /// <summary>
    /// Gets a single communication list in full detail.
    /// </summary>
    [Description( "Gets a single communication list in full detail, including its active member count." )]
    [AgentPurpose( "Retrieves how a communication list is configured and how many people it reaches." )]
    [AgentToolPrerequisite( "Call LookupCommunicationLists to determine the communicationListIdKey." )]
    [AgentToolGuid( "4CBCB5A1-5C3A-4F9A-8203-EF8EDC56EE37" )]
    public AgentToolResult GetCommunicationList( string communicationListIdKey )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        var rockContext = AgentRequestContext.RockContext;

        var group = helper.GetRequiredEntity<Model.Group>( communicationListIdKey, checkSecurity: true );

        if ( group == null )
        {
            return helper.ErrorResult
                .WithInstructions( "Call the LookupCommunicationLists function to determine the available communication lists." );
        }

        var communicationListGroupTypeId = GroupTypeCache.GetId( Rock.SystemGuid.GroupType.GROUPTYPE_COMMUNICATIONLIST.AsGuid() ) ?? 0;

        if ( group.GroupTypeId != communicationListGroupTypeId )
        {
            return Error( "That group is not a communication list." )
                .WithInstructions( "Call the LookupCommunicationLists function to determine the available communication lists." );
        }

        // The public facing name lives in a group attribute, so the attributes
        // must be loaded before it can be read. It falls back to the
        // administrative name when no public name is configured.
        group.LoadAttributes( rockContext );
        var publicName = group.GetAttributeValue( "PublicName" );

        // The member count is only surfaced to an internal audience, so the count
        // query is skipped entirely for a public one.
        var isInternal = AgentRequestContext.AudienceType == AudienceType.Internal;

        int? activeMemberCount = isInternal
            ? new GroupMemberService( rockContext ).Queryable()
                .Count( gm => gm.GroupId == group.Id && gm.GroupMemberStatus == GroupMemberStatus.Active )
            : ( int? ) null;

        var result = new CommunicationListDetailResult
        {
            Id = group.Id,
            Guid = group.Guid,
            Name = group.Name,
            PublicName = publicName.IsNotNullOrWhiteSpace() ? publicName : group.Name,
            Description = group.Description,
            IsActive = group.IsActive,
            ActiveMemberCount = activeMemberCount
        };

        return Success( result )
            .WithHistoryContent( new KeyNameResult( group.Id, group.Guid, group.Name ) );
    }

    #endregion
}

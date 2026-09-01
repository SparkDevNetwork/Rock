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
using Rock.AI.Agent.Classes.Skills.CommunicationSkill;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CommunicationSkill
{
    #region Tool(s)

    /// <summary>
    /// Looks up the communication lists configured in Rock.
    /// </summary>
    /// <remarks>
    /// A Lookup because the set is treated as a bounded configuration surface
    /// returned whole. Only active lists the current person is authorized to view
    /// are returned, matching what the communication authoring screens show.
    /// </remarks>
    [Description( "Looks up the active communication lists configured in Rock. A communication list is a group people subscribe to for bulk email or SMS." )]
    [AgentPurpose( "Finds a communication list so its details or recipients can be retrieved." )]
    [AgentToolGuid( "00286ADB-19E8-4AB9-8AA8-477E2FBB19B6" )]
    public AgentToolResult LookupCommunicationLists()
    {
        var rockContext = AgentRequestContext.RockContext;
        var currentPerson = AgentRequestContext.CurrentPerson;

        var communicationListGroupTypeId = GroupTypeCache.GetId( Rock.SystemGuid.GroupType.GROUPTYPE_COMMUNICATIONLIST.AsGuid() ) ?? 0;

        // The administrative name is only surfaced to an internal audience; a
        // public one sees the public facing name instead.
        var isInternal = AgentRequestContext.AudienceType == AudienceType.Internal;

        // Authorization on a communication list is per group and cannot be
        // translated to SQL, so the set is materialized and then filtered in
        // memory, exactly as the communication authoring screens do.
        var authorizedGroups = new GroupService( rockContext ).Queryable().AsNoTracking()
            .Where( g => g.GroupTypeId == communicationListGroupTypeId && g.IsActive )
            .OrderBy( g => g.Order )
            .ThenBy( g => g.Name )
            .ToList()
            .Where( g => g.IsAuthorized( Authorization.VIEW, currentPerson ) )
            .ToList();

        // The public facing name lives in a group attribute, so attributes are
        // bulk loaded across the authorized set rather than one query per list.
        authorizedGroups.LoadAttributes( rockContext );

        var results = authorizedGroups
            .Select( g =>
            {
                // The public facing name falls back to the administrative name
                // when one is not configured.
                var publicName = g.GetAttributeValue( "PublicName" );

                return new CommunicationListResult
                {
                    Id = g.Id,
                    Guid = g.Guid,
                    Name = isInternal ? g.Name : null,
                    PublicName = publicName.IsNotNullOrWhiteSpace() ? publicName : g.Name,
                    Description = g.Description
                };
            } )
            .ToList();

        if ( !results.Any() )
        {
            return NoData()
                .WithInstructions( "No communication lists are configured, or you do not have access to any." );
        }

        return Success( results )
            .WithHistoryKey( "communication-lists" );
    }

    #endregion
}

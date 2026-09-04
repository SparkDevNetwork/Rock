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
using Rock.Model;
using Rock.SystemGuid;
using Rock.Web.Cache;

using GroupTypeResult = Rock.AI.Agent.Classes.Skills.GroupSkill.GroupTypeResult;

namespace Rock.AI.Agent.Skills;

internal partial class GroupSkill
{
    #region Tool(s)

    [Description( "Retrieves the group types configured in Rock." )]
    [AgentPurpose( "Retrieves the group types and roles configured in Rock." )]
    [AgentToolGuid( "23c5ad96-68f8-4d3e-b2e9-96e179a08e5a" )]
    public AgentToolResult LookupGroupTypes()
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        var availableGroupTypeIds = GetAvailableGroupTypes().Select( gt => gt.Id ).ToList();

        // Internal audience can see all group types on the system, but are
        // flagged if they are available based on the configured group types.
        // Public audience can only see the group types that are configured.
        var groupTypes = AgentRequestContext.AudienceType == Enums.AI.Agent.AudienceType.Internal
            ? GroupTypeCache.All( AgentRequestContext.RockContext )
                .Where( gt => gt.IsAuthorized( Rock.Security.Authorization.VIEW, AgentRequestContext.CurrentPerson ) )
            : GetAvailableGroupTypes();

        var groupTypeResults = groupTypes
            .OrderBy( gt => gt.Name )
            .Select( gt => new GroupTypeResult
            {
                Id = gt.Id,
                Guid = gt.Guid,
                IsNotAvailable = !availableGroupTypeIds.Contains( gt.Id ),
                Name = gt.Name,
                Roles = gt.Roles
                    .Select( r => KeyNameResult.FromCache( r ) )
                    .ToList(),
            } )
            .ToList();

        var result = Success( groupTypeResults );

        if ( groupTypeResults.Count > 50 )
        {
            result = result.WithoutHistoryContent();
        }

        return result;
    }

    #endregion
}

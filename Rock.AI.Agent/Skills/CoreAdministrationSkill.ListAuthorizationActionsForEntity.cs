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
using Rock.AI.Agent.Classes.Skills.CoreAdministrationSkill;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CoreAdministrationSkill
{
    #region Tool(s)

    /// <summary>
    /// Lists the actions an entity supports securing.
    /// </summary>
    /// <remarks>
    /// These are the action names, such as View, Edit, and Administrate, that an
    /// authorization rule can govern. They vary by entity type; a group, for
    /// example, adds actions like ManageMembers. Rock refers to these as actions,
    /// not verbs.
    /// </remarks>
    [Description( "Lists the actions an entity supports securing, such as View, Edit, and Administrate. Use the returned action names with the other authorization tools." )]
    [AgentPurpose( "Determines which actions can be granted or denied on an entity." )]
    [AgentToolPrerequisite( "Call ListEntityTypes to determine the entityTypeIdKey." )]
    [AgentToolGuid( "7E4933CE-3E2E-4755-B3E8-7424F0642A5A" )]
    public AgentToolResult ListAuthorizationActionsForEntity( string entityTypeIdKey, string entityIdKey = null )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        var rockContext = AgentRequestContext.RockContext;

        if ( !TryGetAdministrableEntity( helper, rockContext, entityTypeIdKey, entityIdKey, out var securedEntity ) )
        {
            return helper.ErrorResult
                .WithInstructions( $"Call the {nameof( ListEntityTypes )} function to determine the available entity types." );
        }

        var results = securedEntity.SupportedActions
            .Select( a => new AuthorizationActionResult
            {
                Action = a.Key,
                Description = a.Value,
                IsAllowedByDefault = securedEntity.IsAllowedByDefault( a.Key )
            } )
            .ToList();

        return Success( results )
            .WithoutHistoryContent();
    }

    #endregion
}

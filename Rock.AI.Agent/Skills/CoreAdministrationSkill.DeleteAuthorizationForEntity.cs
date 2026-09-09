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

using System.Linq;
using System.ComponentModel;

using Rock.AI.Agent.Annotations;
using Rock.Configuration;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CoreAdministrationSkill
{
    #region Tool(s)

    /// <summary>
    /// Deletes a single authorization rule from an entity.
    /// </summary>
    /// <remarks>
    /// The entity is required alongside the rule so the rule can be confirmed to
    /// belong to it before it is deleted, which prevents deleting an unrelated rule
    /// by key. A deletion that would remove the caller's own ability to administer
    /// the entity is always refused; that can only be done through Rock's security
    /// screen.
    /// </remarks>
    [AgentGuardrail( "This permanently removes an authorization rule, which can change who has access to the entity, including your own. Confirm the rule is the correct one before proceeding." )]
    [Description( "Deletes a single authorization (security) rule from an entity." )]
    [AgentUsage( "Supply the entityTypeIdKey, and the entityIdKey when the rule is on a specific entity, alongside the authIdKey. Omit entityIdKey to delete a rule from the entity type's defaults." )]
    [AgentToolPrerequisite( "Call ListAuthorizationForEntity to determine the authIdKey." )]
    [AgentToolGuid( "AE38A4D5-2F9D-4865-A5EC-AA7719311D50" )]
    public AgentToolResult DeleteAuthorizationForEntity( string authIdKey, string entityTypeIdKey, string entityIdKey = null )
    {
        using var rockContext = RockApp.Current.CreateRockContext();

        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );

        if ( !TryGetAdministrableEntity( helper, rockContext, entityTypeIdKey, entityIdKey, out var securedEntity ) )
        {
            return helper.ErrorResult
                .WithInstructions( $"Call the {nameof( ListEntityTypes )} function to determine the available entity types." );
        }

        var authService = new AuthService( rockContext );

        var auth = helper.GetOptionalEntity<Rock.Model.Auth>( authIdKey, checkSecurity: false );

        if ( auth == null )
        {
            return helper.ErrorResult
                .WithInstructions( $"Call the {nameof( ListAuthorizationForEntity )} function to determine the available rules." );
        }

        if ( auth.EntityTypeId != securedEntity.TypeId || auth.EntityId != securedEntity.Id )
        {
            return Error( "The authIdKey does not identify a rule on this entity." )
                .WithInstructions( $"Call the {nameof( ListAuthorizationForEntity )} function to determine the available rules." );
        }

        var action = auth.Action;

        // Guard before deleting: evaluate the rules as they would be with this rule
        // removed.
        if ( action == Authorization.ADMINISTRATE )
        {
            var afterChange = authService.GetAuths( securedEntity.TypeId, securedEntity.Id, action ).ToList()
                .Where( a => a.Id != auth.Id )
                .Select( a => ( a.Order, a.AllowOrDeny == "A", a.SpecialRole, a.PersonAliasId, a.GroupId ) )
                .ToList();

            if ( !DoesCurrentPersonRetainAccess( authService, securedEntity, action, afterChange, rockContext ) )
            {
                return SelfLockoutError();
            }
        }

        authService.Delete( auth );

        helper.SaveChangesIfNoErrors();

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        // Refresh the authorization cache for this action, or the deletion is not
        // observed until the cache expires.
        Authorization.RefreshAction( securedEntity.TypeId, securedEntity.Id, action, rockContext );

        return Success()
            .WithInstructions( $"The authorization rule has been deleted. Call {nameof( ListAuthorizationForEntity )} to see the remaining rules." );
    }

    #endregion
}

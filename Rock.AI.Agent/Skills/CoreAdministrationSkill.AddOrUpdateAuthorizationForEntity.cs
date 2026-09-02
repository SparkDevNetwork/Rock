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

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.CoreAdministrationSkill;
using Rock.Configuration;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CoreAdministrationSkill
{
    #region Tool(s)

    /// <summary>
    /// Adds an authorization rule to an entity, or updates an existing one.
    /// </summary>
    /// <remarks>
    /// A rule grants or denies one action to one subject: a person, a security role
    /// group, or a special role. Provide exactly one subject when adding. The
    /// subject of an existing rule cannot be changed; delete it and add a new one.
    /// Rules are evaluated in order, so a change is best followed by reading the
    /// full list back. A change that would remove the caller's own ability to
    /// administer the entity is always refused; that can only be done through Rock's
    /// security screen.
    /// </remarks>
    [AgentGuardrail( "This changes who can access the specified entity. A mistake can grant access that should be restricted, or remove access that is needed, including your own. Confirm the entity, action, subject, and allow or deny are correct before proceeding." )]
    [Description( "Adds an authorization (security) rule to an entity or updates an existing one, granting or denying one action to a person, security role, or special role." )]
    [AgentUsage( "When adding, provide exactly one of personIdKey, groupIdKey, or specialRole. Supplying authIdKey updates that rule's allow/deny and order. Omit entityIdKey to change the entity type's default rules." )]
    [AgentToolPrerequisite( "Call ListAuthorizationActionsForEntity to determine the action, and ListAuthorizationForEntity to find an existing rule's authIdKey." )]
    [AgentToolGuid( "FF3C1804-8980-4043-A35C-45830EB3336F" )]
    public AgentToolResult AddOrUpdateAuthorizationForEntity(
        string entityTypeIdKey,
        string action,
        AllowOrDeny allowOrDeny,
        string entityIdKey = null,
        string authIdKey = null,
        string personIdKey = null,
        string groupIdKey = null,
        [Description( "A special role the rule targets when it does not target a specific person or group." )]
        SpecialRole? specialRole = null,
        int? order = null )
    {
        using var rockContext = RockApp.Current.CreateRockContext();

        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );

        if ( !TryGetAdministrableEntity( helper, rockContext, entityTypeIdKey, entityIdKey, out var securedEntity ) )
        {
            return helper.ErrorResult
                .WithInstructions( $"Call the {nameof( ListEntityTypes )} function to determine the available entity types." );
        }

        if ( !securedEntity.SupportedActions.ContainsKey( action ) )
        {
            return Error( $"'{action}' is not an action this entity supports." )
                .WithInstructions( $"Call the {nameof( ListAuthorizationActionsForEntity )} function to determine the supported actions." );
        }

        var authService = new AuthService( rockContext );
        var storedAllowOrDeny = allowOrDeny == AllowOrDeny.Allow ? "A" : "D";
        var isUpdate = authIdKey.IsNotNullOrWhiteSpace();
        var guardsAdministrate = action == Authorization.ADMINISTRATE;

        Rock.Model.Auth auth;

        if ( isUpdate )
        {
            auth = GetValidatedAuth( helper, authService, authIdKey, securedEntity, action );

            if ( auth == null )
            {
                return Error( "The authIdKey does not identify a rule on this entity for this action." )
                    .WithInstructions( $"Call the {nameof( ListAuthorizationForEntity )} function to determine the available rules." );
            }

            if ( personIdKey.IsNotNullOrWhiteSpace() || groupIdKey.IsNotNullOrWhiteSpace() || specialRole.HasValue )
            {
                return Error( "The subject of an existing rule cannot be changed. Delete the rule and add a new one instead." );
            }

            // Guard before mutating: evaluate the rules as they would be after this
            // change, with the target rule's new allow/deny and order applied.
            if ( guardsAdministrate )
            {
                var afterChange = authService.GetAuths( securedEntity.TypeId, securedEntity.Id, action ).ToList()
                    .Select( a => a.Id == auth.Id
                        ? ( order ?? a.Order, storedAllowOrDeny == "A", a.SpecialRole, a.PersonAliasId, a.GroupId )
                        : ( a.Order, a.AllowOrDeny == "A", a.SpecialRole, a.PersonAliasId, a.GroupId ) )
                    .ToList();

                if ( !DoesCurrentPersonRetainAccess( authService, securedEntity, action, afterChange, rockContext ) )
                {
                    return SelfLockoutError();
                }
            }

            auth.AllowOrDeny = storedAllowOrDeny;

            if ( order.HasValue )
            {
                auth.Order = order.Value;
            }
        }
        else
        {
            var hasSpecialRole = specialRole.HasValue && specialRole.Value != SpecialRole.None;
            var subjectCount = ( personIdKey.IsNotNullOrWhiteSpace() ? 1 : 0 )
                + ( groupIdKey.IsNotNullOrWhiteSpace() ? 1 : 0 )
                + ( hasSpecialRole ? 1 : 0 );

            if ( subjectCount != 1 )
            {
                return Error( "Provide exactly one subject: personIdKey, groupIdKey, or specialRole." );
            }

            int? personAliasId = null;
            int? groupId = null;
            var roleValue = SpecialRole.None;

            if ( personIdKey.IsNotNullOrWhiteSpace() )
            {
                var person = helper.GetRequiredEntity<Rock.Model.Person>( personIdKey );

                if ( person == null )
                {
                    return helper.ErrorResult;
                }

                personAliasId = person.PrimaryAliasId;

                if ( !personAliasId.HasValue )
                {
                    return Error( "The specified person does not have a primary alias and cannot be used in a rule." );
                }
            }
            else if ( groupIdKey.IsNotNullOrWhiteSpace() )
            {
                var group = helper.GetRequiredEntity<Rock.Model.Group>( groupIdKey );

                if ( group == null )
                {
                    return helper.ErrorResult;
                }

                if ( RoleCache.Get( group.Id ) == null )
                {
                    return Error( "The specified group is not a security role and cannot be used in a rule." );
                }

                groupId = group.Id;
            }
            else
            {
                roleValue = specialRole.Value;
            }

            var existingAuths = authService.GetAuths( securedEntity.TypeId, securedEntity.Id, action ).ToList();

            // AddOrUpdate: a rule with the same subject is updated rather than
            // duplicated, so repeat calls converge instead of stacking rules.
            auth = existingAuths.FirstOrDefault( a =>
                a.SpecialRole == roleValue && a.PersonAliasId == personAliasId && a.GroupId == groupId );

            var isNewRule = auth == null;
            var newOrder = order ?? ( existingAuths.Count > 0 ? existingAuths.Max( a => a.Order ) + 1 : 0 );

            if ( guardsAdministrate && storedAllowOrDeny == "D" )
            {
                var afterChange = existingAuths
                    .Where( a => isNewRule || a.Id != auth.Id )
                    .Select( a => ( a.Order, a.AllowOrDeny == "A", a.SpecialRole, a.PersonAliasId, a.GroupId ) )
                    .ToList();

                afterChange.Add( ( newOrder, false, roleValue, personAliasId, groupId ) );

                if ( !DoesCurrentPersonRetainAccess( authService, securedEntity, action, afterChange, rockContext ) )
                {
                    return SelfLockoutError();
                }
            }

            if ( isNewRule )
            {
                auth = new Rock.Model.Auth
                {
                    EntityTypeId = securedEntity.TypeId,
                    EntityId = securedEntity.Id,
                    Action = action,
                    AllowOrDeny = storedAllowOrDeny,
                    SpecialRole = roleValue,
                    PersonAliasId = personAliasId,
                    GroupId = groupId,
                    Order = newOrder
                };

                authService.Add( auth );
            }
            else
            {
                auth.AllowOrDeny = storedAllowOrDeny;

                if ( order.HasValue )
                {
                    auth.Order = order.Value;
                }
            }
        }

        helper.SaveChangesIfNoErrors();

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        // Refresh the authorization cache for this action, or the change is not
        // observed until the cache expires.
        Authorization.RefreshAction( securedEntity.TypeId, securedEntity.Id, action, rockContext );

        var result = BuildRuleResult( auth.Id, auth.Guid, action, auth.AllowOrDeny == "A", auth.Order, auth.SpecialRole, auth.PersonAliasId, auth.GroupId, false, null, rockContext );

        return Success( result )
            .WithInstructions( $"The authorization rule has been saved. Call {nameof( ListAuthorizationForEntity )} to see the full ordered list, since rules interact by order." )
            .WithHistoryContent( new KeyNameResult( auth.Id, auth.Guid, $"{action} {allowOrDeny}" ) );
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Loads an <see cref="Auth"/> by its IdKey and confirms it belongs to the
    /// given entity and action, so a caller cannot edit an unrelated rule by key.
    /// </summary>
    /// <param name="helper">The tool helper errors are accumulated on.</param>
    /// <param name="authService">The auth service (unused for the lookup but kept for symmetry with the Security block pattern).</param>
    /// <param name="authIdKey">The IdKey of the rule.</param>
    /// <param name="securedEntity">The entity the rule must belong to.</param>
    /// <param name="action">The action the rule must belong to.</param>
    /// <returns>The validated auth, or <c>null</c> if it does not belong to the entity and action.</returns>
    private Rock.Model.Auth GetValidatedAuth( AgentToolHelper helper, AuthService authService, string authIdKey, ISecured securedEntity, string action )
    {
        var auth = helper.GetOptionalEntity<Rock.Model.Auth>( authIdKey, checkSecurity: false );

        if ( auth == null )
        {
            return null;
        }

        if ( auth.EntityTypeId != securedEntity.TypeId || auth.EntityId != securedEntity.Id || auth.Action != action )
        {
            return null;
        }

        return auth;
    }

    /// <summary>
    /// The standard error returned when a change would lock the caller out of
    /// administering the entity. This is never overridable through the agent; the
    /// change can only be made through Rock's security screen.
    /// </summary>
    private AgentToolResult SelfLockoutError()
    {
        return Error( "This change would remove your own ability to administer this entity, so it was not made. If this is genuinely intended, make the change through Rock's security screen instead." );
    }

    #endregion
}

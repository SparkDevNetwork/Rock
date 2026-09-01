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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.CoreAdministrationSkill;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CoreAdministrationSkill
{
    #region Tool(s)

    /// <summary>
    /// Lists the authorization rules that apply to an entity.
    /// </summary>
    /// <remarks>
    /// Rules set directly on the entity are returned alongside the rules inherited
    /// from its parent authorities (such as its category), the inherited ones
    /// flagged and read-only. When no entityIdKey is supplied, the entity type's
    /// own default rules are returned. Viewing authorization requires the caller to
    /// be able to administer the entity, matching Rock's own security screen.
    /// </remarks>
    [Description( "Lists the authorization (security) rules that apply to an entity, including rules inherited from parent authorities." )]
    [AgentPurpose( "Sees who is allowed or denied each action on an entity." )]
    [AgentUsage( "Omit entityIdKey to read the entity type's default rules. Omit action to read every action." )]
    [AgentToolPrerequisite( "Call ListEntityTypes to determine the entityTypeIdKey, and ListAuthorizationActionsForEntity to determine the action names." )]
    [AgentToolGuid( "25CA6D47-0883-40C4-B222-BC0C64693C11" )]
    public AgentToolResult ListAuthorizationForEntity( string entityTypeIdKey, string entityIdKey = null, string action = null )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        var rockContext = AgentRequestContext.RockContext;

        if ( !TryGetAdministrableEntity( helper, rockContext, entityTypeIdKey, entityIdKey, out var securedEntity ) )
        {
            return helper.ErrorResult
                .WithInstructions( $"Call the {nameof( ListEntityTypes )} function to determine the available entity types." );
        }

        List<string> actions;

        if ( action.IsNotNullOrWhiteSpace() )
        {
            if ( !securedEntity.SupportedActions.ContainsKey( action ) )
            {
                return Error( $"'{action}' is not an action this entity supports." )
                    .WithInstructions( $"Call the {nameof( ListAuthorizationActionsForEntity )} function to determine the supported actions." );
            }

            actions = new List<string> { action };
        }
        else
        {
            actions = securedEntity.SupportedActions.Keys.ToList();
        }

        var authService = new AuthService( rockContext );
        var results = new List<AuthorizationRuleResult>();

        foreach ( var actionKey in actions )
        {
            var itemAuths = authService.GetAuths( securedEntity.TypeId, securedEntity.Id, actionKey ).ToList();

            foreach ( var auth in itemAuths )
            {
                results.Add( BuildRuleResult( auth.Id, auth.Guid, actionKey, auth.AllowOrDeny == "A", auth.Order, auth.SpecialRole, auth.PersonAliasId, auth.GroupId, false, null, rockContext ) );
            }

            AddInheritedRules( authService, securedEntity, actionKey, itemAuths, results, rockContext );
        }

        return Success( results )
            .WithoutHistoryContent();
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Resolves a securable entity from its entity type and optional entity IdKey,
    /// and confirms the current person may administer it. When no entity IdKey is
    /// supplied, the entity type's own default is targeted, matching Rock's
    /// security screen. Any problem is reported as an error on
    /// <paramref name="helper"/>.
    /// </summary>
    /// <param name="helper">The tool helper errors are accumulated on.</param>
    /// <param name="rockContext">The context used to load the entity.</param>
    /// <param name="entityTypeIdKey">The entity type IdKey.</param>
    /// <param name="entityIdKey">The entity IdKey, or <c>null</c> for the type default.</param>
    /// <param name="securedEntity">On success, the resolved securable entity.</param>
    /// <returns><c>true</c> when the entity resolved and the caller may administer it.</returns>
    private bool TryGetAdministrableEntity( AgentToolHelper helper, RockContext rockContext, string entityTypeIdKey, string entityIdKey, out ISecured securedEntity )
    {
        securedEntity = null;

        var entityType = EntityTypeCache.GetByIdKey( entityTypeIdKey );

        if ( entityType == null )
        {
            helper.AddError( $"The {nameof( entityTypeIdKey )} is not valid." );
            return false;
        }

        var type = entityType.GetEntityType();

        if ( type == null )
        {
            helper.AddError( $"The entity type '{entityType.FriendlyName}' could not be loaded." );
            return false;
        }

        ISecured entity;

        if ( entityIdKey.IsNotNullOrWhiteSpace() )
        {
            entity = Rock.Reflection.GetIEntityForEntityType( type, entityIdKey, false, rockContext ) as ISecured;

            if ( entity == null )
            {
                helper.AddError( $"The {nameof( entityIdKey )} is not valid, or the entity is not securable." );
                return false;
            }
        }
        else
        {
            // No entity was named, so target the entity type's own default rules.
            entity = Activator.CreateInstance( type ) as ISecured;

            if ( entity == null )
            {
                helper.AddError( $"The entity type '{entityType.FriendlyName}' is not securable." );
                return false;
            }
        }

        if ( !entity.IsAuthorized( Authorization.ADMINISTRATE, AgentRequestContext.CurrentPerson ) )
        {
            helper.AddError( "You are not authorized to administer security for this item." );
            return false;
        }

        securedEntity = entity;
        return true;
    }

    /// <summary>
    /// Walks the parent authority chain collecting inherited rules for one action,
    /// skipping any rule already represented on the entity or by a nearer ancestor.
    /// The walk is iterative with a visited set so a circular parent chain cannot
    /// recurse until the stack overflows.
    /// </summary>
    /// <param name="authService">The auth service.</param>
    /// <param name="securedEntity">The entity whose inherited rules are collected.</param>
    /// <param name="action">The action being inspected.</param>
    /// <param name="itemAuths">The rules set directly on the entity, used for de-duplication.</param>
    /// <param name="results">The accumulated results the inherited rules are added to.</param>
    /// <param name="rockContext">The context used to resolve rule subjects.</param>
    private void AddInheritedRules( AuthService authService, ISecured securedEntity, string action, List<Auth> itemAuths, List<AuthorizationRuleResult> results, RockContext rockContext )
    {
        var seenSubjects = new HashSet<string>( itemAuths.Select( a => SubjectKey( a.SpecialRole, a.PersonAliasId, a.GroupId ) ) );
        var visitedAuthorities = new HashSet<string>();

        // ParentAuthorityPre is inspected without recursing; ParentAuthority is
        // walked up the chain, matching Rock's security screen.
        var parents = new List<(ISecured Authority, bool Recurse)>
        {
            ( securedEntity.ParentAuthorityPre, false ),
            ( securedEntity.ParentAuthority, true )
        };

        foreach ( var (startingAuthority, recurse) in parents )
        {
            var parent = startingAuthority;

            while ( parent != null )
            {
                if ( !visitedAuthorities.Add( $"{parent.TypeId}|{parent.Id}" ) )
                {
                    // Already visited, so the chain is circular. Stop rather than loop.
                    break;
                }

                var parentEntityType = EntityTypeCache.Get( parent.TypeId, rockContext );
                var parentTitle = $"{parent} ({parentEntityType?.FriendlyName ?? parentEntityType?.Name})".Trim();

                foreach ( var auth in authService.GetAuths( parent.TypeId, parent.Id, action ) )
                {
                    var subjectKey = SubjectKey( auth.SpecialRole, auth.PersonAliasId, auth.GroupId );

                    if ( !seenSubjects.Add( subjectKey ) )
                    {
                        continue;
                    }

                    results.Add( BuildRuleResult( auth.Id, null, action, auth.AllowOrDeny == "A", auth.Order, auth.SpecialRole, auth.PersonAliasId, auth.GroupId, true, parentTitle, rockContext ) );
                }

                parent = recurse ? parent.ParentAuthority : null;
            }
        }
    }

    /// <summary>
    /// Builds a single rule result, resolving the subject it applies to.
    /// </summary>
    private AuthorizationRuleResult BuildRuleResult( int id, Guid? guid, string action, bool isAllow, int order, SpecialRole specialRole, int? personAliasId, int? groupId, bool isInherited, string inheritedFrom, RockContext rockContext )
    {
        return new AuthorizationRuleResult
        {
            Id = id,
            Guid = guid,
            Action = action,
            AllowOrDeny = isAllow ? AllowOrDeny.Allow : AllowOrDeny.Deny,
            Order = order,
            Subject = BuildSubject( specialRole, personAliasId, groupId, rockContext ),
            IsInherited = isInherited,
            InheritedFrom = inheritedFrom
        };
    }

    /// <summary>
    /// Resolves the subject an authorization rule applies to.
    /// </summary>
    private AuthorizationSubjectResult BuildSubject( SpecialRole specialRole, int? personAliasId, int? groupId, RockContext rockContext )
    {
        if ( specialRole != SpecialRole.None )
        {
            return new AuthorizationSubjectResult { Kind = "SpecialRole", SpecialRole = specialRole };
        }

        if ( personAliasId.HasValue )
        {
            var person = new PersonAliasService( rockContext ).Get( personAliasId.Value )?.Person;

            return new AuthorizationSubjectResult
            {
                Kind = "Person",
                Person = person != null
                    ? new KeyNameResult { Id = person.Id, Guid = person.Guid, Name = person.FullName }
                    : null
            };
        }

        if ( groupId.HasValue )
        {
            var group = GroupCache.Get( groupId.Value, rockContext );

            return new AuthorizationSubjectResult
            {
                Kind = "Group",
                Group = KeyNameResult.FromCache( group )
            };
        }

        return new AuthorizationSubjectResult { Kind = "Unknown" };
    }

    /// <summary>
    /// Builds a key identifying the subject of a rule, used to de-duplicate an
    /// entity's rules against the inherited ones.
    /// </summary>
    private static string SubjectKey( SpecialRole specialRole, int? personAliasId, int? groupId )
    {
        return $"{( int ) specialRole}|{personAliasId}|{groupId}";
    }

    /// <summary>
    /// Evaluates whether the current person would still be authorized for an action
    /// after a proposed change to an entity's own rules. This is the self-lockout
    /// guard for the write and delete tools: it evaluates the post-change rules of
    /// the entity, then the inherited rules, in order, exactly the way Rock resolves
    /// authorization, and falls back to the action's default when nothing matches.
    /// </summary>
    /// <remarks>
    /// Rules on the entity are considered before inherited rules, and both are
    /// considered in <c>Order</c>. The first rule that applies to the person settles
    /// the decision. It does not account for the global "super admin" fallback, so
    /// it can report a lockout for someone who would in fact retain access through a
    /// global role; that is the safe direction to err, since the caller can override
    /// with a deliberate confirmation.
    /// </remarks>
    /// <param name="authService">The auth service used to read inherited rules.</param>
    /// <param name="securedEntity">The entity the change applies to.</param>
    /// <param name="action">The action being changed.</param>
    /// <param name="itemRulesAfterChange">The entity's own rules for the action as they would be after the change.</param>
    /// <param name="rockContext">The context used to resolve person aliases and inherited rules.</param>
    /// <returns><c>true</c> if the current person would remain authorized; otherwise <c>false</c>.</returns>
    private bool DoesCurrentPersonRetainAccess( AuthService authService, ISecured securedEntity, string action, List<(int Order, bool IsAllow, SpecialRole Role, int? PersonAliasId, int? GroupId)> itemRulesAfterChange, RockContext rockContext )
    {
        var person = AgentRequestContext.CurrentPerson;

        if ( person == null )
        {
            return false;
        }

        var aliasIds = new HashSet<int>( new PersonAliasService( rockContext )
            .Queryable()
            .Where( pa => pa.PersonId == person.Id )
            .Select( pa => pa.Id ) );

        // The entity's own rules decide first, in order.
        foreach ( var rule in itemRulesAfterChange.OrderBy( r => r.Order ) )
        {
            if ( RuleAppliesToPerson( rule.Role, rule.PersonAliasId, rule.GroupId, aliasIds, person ) )
            {
                return rule.IsAllow;
            }
        }

        // Then the inherited rules, nearest authority first.
        var visitedAuthorities = new HashSet<string>();
        var parents = new List<(ISecured Authority, bool Recurse)>
        {
            ( securedEntity.ParentAuthorityPre, false ),
            ( securedEntity.ParentAuthority, true )
        };

        foreach ( var (startingAuthority, recurse) in parents )
        {
            var parent = startingAuthority;

            while ( parent != null )
            {
                if ( !visitedAuthorities.Add( $"{parent.TypeId}|{parent.Id}" ) )
                {
                    break;
                }

                foreach ( var auth in authService.GetAuths( parent.TypeId, parent.Id, action ) )
                {
                    if ( RuleAppliesToPerson( auth.SpecialRole, auth.PersonAliasId, auth.GroupId, aliasIds, person ) )
                    {
                        return auth.AllowOrDeny == "A";
                    }
                }

                parent = recurse ? parent.ParentAuthority : null;
            }
        }

        return securedEntity.IsAllowedByDefault( action );
    }

    /// <summary>
    /// Determines whether an authorization rule applies to the current person,
    /// considering special roles, a direct person match, and security role
    /// membership.
    /// </summary>
    private bool RuleAppliesToPerson( SpecialRole role, int? personAliasId, int? groupId, HashSet<int> personAliasIds, Rock.Model.Person person )
    {
        switch ( role )
        {
            case SpecialRole.AllUsers:
                return true;

            // The agent always acts as an authenticated person.
            case SpecialRole.AllAuthenticatedUsers:
                return true;

            case SpecialRole.AllUnAuthenticatedUsers:
                return false;
        }

        if ( personAliasId.HasValue && personAliasIds.Contains( personAliasId.Value ) )
        {
            return true;
        }

        if ( groupId.HasValue && ( RoleCache.Get( groupId.Value )?.IsPersonInRole( person.Guid ) ?? false ) )
        {
            return true;
        }

        return false;
    }

    #endregion
}

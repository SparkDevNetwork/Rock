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
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Administration.Security;
using Rock.ViewModels.Core.Grid;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Administration
{
    /// <summary>
    /// Displays and edits the security (authorization) rules for a specific secured entity.
    /// </summary>
    [DisplayName( "Security" )]
    [Category( "Administration" )]
    [Description( "Displays security settings for a specific entity." )]
    [SupportedSiteTypes( SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "E59972C2-210D-4BC0-8D06-F8F815D6ECBE" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "18152F35-BE5C-4F9F-A13F-CC79116EBFD4" )]
    [Rock.SystemGuid.BlockTypeGuid( "20474B3D-0DE7-4B63-B7B9-E042DBEF788C" )]
    public class Security : RockBlockType
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string EntityTypeId = "EntityTypeId";
            public const string EntityId = "EntityId";
        }

        /// <summary>
        /// The role dropdown values that represent the special (non-group) roles.
        /// </summary>
        private static class SpecialRoleValue
        {
            public const string AllUsers = "-1";
            public const string AllAuthenticatedUsers = "-2";
            public const string AllUnAuthenticatedUsers = "-3";
        }

        #endregion

        #region Constants

        /// <summary>
        /// The message shown when the current person is not allowed to administrate the entity.
        /// </summary>
        private const string NotAuthorizedMessage = "Unfortunately, you are not able to edit security because you do not belong to a role that has been configured to allow administration of this item.";

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<SecurityInitializationBag, SecurityOptionsBag>();

            if ( !TryGetSecuredEntity( out var securedEntity, out var errorMessage ) )
            {
                box.ErrorMessage = errorMessage;
                return box;
            }

            if ( !securedEntity.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
            {
                box.ErrorMessage = NotAuthorizedMessage;
                return box;
            }

            var actions = GetOrderedActions( securedEntity );
            var currentAction = GetDefaultAction( securedEntity, actions );

            box.Bag = new SecurityInitializationBag
            {
                Actions = actions,
                CurrentAction = currentAction,
                ActionData = BuildActionData( securedEntity, currentAction )
            };

            box.Options = new SecurityOptionsBag
            {
                Roles = GetRoleListItems()
            };

            return box;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the permission data for a single action (used when switching tabs).
        /// </summary>
        /// <param name="action">The action to load.</param>
        [BlockAction]
        public BlockActionResult GetActionData( string action )
        {
            if ( !TryGetEntityForAction( action, out var securedEntity, out var errorResult ) )
            {
                return errorResult;
            }

            return ActionOk( BuildActionData( securedEntity, action ) );
        }

        /// <summary>
        /// Sets a rule to allow or deny.
        /// </summary>
        /// <param name="authGuid">The unique identifier of the rule to change.</param>
        /// <param name="allowOrDeny">"A" to allow or "D" to deny.</param>
        /// <param name="action">The action currently being viewed.</param>
        [BlockAction]
        public BlockActionResult SetAllowDeny( string authGuid, string allowOrDeny, string action )
        {
            if ( !TryGetEntityForAction( action, out var securedEntity, out var errorResult ) )
            {
                return errorResult;
            }

            if ( allowOrDeny != "A" && allowOrDeny != "D" )
            {
                return ActionBadRequest( "Invalid value." );
            }

            var authService = new AuthService( RockContext );
            var auth = GetValidatedAuth( authService, authGuid, securedEntity, action );
            if ( auth != null )
            {
                auth.AllowOrDeny = allowOrDeny;
                RockContext.SaveChanges();

                Authorization.RefreshAction( securedEntity.TypeId, securedEntity.Id, action );
            }

            return ActionOk( BuildActionData( securedEntity, action ) );
        }

        /// <summary>
        /// Deletes a rule from the permission list.
        /// </summary>
        /// <param name="authGuid">The unique identifier of the rule to delete.</param>
        /// <param name="action">The action currently being viewed.</param>
        [BlockAction]
        public BlockActionResult DeleteRule( string authGuid, string action )
        {
            if ( !TryGetEntityForAction( action, out var securedEntity, out var errorResult ) )
            {
                return errorResult;
            }

            var authService = new AuthService( RockContext );
            var auth = GetValidatedAuth( authService, authGuid, securedEntity, action );
            if ( auth != null )
            {
                authService.Delete( auth );
                RockContext.SaveChanges();

                Authorization.RefreshAction( securedEntity.TypeId, securedEntity.Id, action );
            }

            return ActionOk( BuildActionData( securedEntity, action ) );
        }

        /// <summary>
        /// Re-orders a rule within the permission list.
        /// </summary>
        /// <param name="key">The unique identifier of the rule being moved.</param>
        /// <param name="beforeKey">The unique identifier of the rule it was dropped before, or <c>null</c> for the end of the list.</param>
        /// <param name="action">The action currently being viewed.</param>
        [BlockAction]
        public BlockActionResult ReorderRule( string key, string beforeKey, string action )
        {
            if ( !TryGetEntityForAction( action, out var securedEntity, out var errorResult ) )
            {
                return errorResult;
            }

            var authService = new AuthService( RockContext );
            var rules = authService.GetAuths( securedEntity.TypeId, securedEntity.Id, action ).ToList();

            if ( !rules.ReorderEntity( key, beforeKey ) )
            {
                return ActionBadRequest( "Invalid reorder attempt." );
            }

            RockContext.SaveChanges();
            Authorization.RefreshAction( securedEntity.TypeId, securedEntity.Id, action );

            return ActionOk( BuildActionData( securedEntity, action ) );
        }

        /// <summary>
        /// Adds a role (or special role) to the permission list for one or more actions.
        /// </summary>
        /// <param name="bag">The role and actions to add.</param>
        [BlockAction]
        public BlockActionResult AddRole( AddRoleRequestBag bag )
        {
            // bag?.CurrentAction lets a null bag fall through to the "Invalid action."
            // result rather than throwing, matching the original guard.
            if ( !TryGetEntityForAction( bag?.CurrentAction, out var securedEntity, out var errorResult ) )
            {
                return errorResult;
            }

            if ( !TryResolveRole( bag.RoleValue, out var specialRole, out var groupId ) )
            {
                return ActionBadRequest( "Invalid role." );
            }

            var selectedActions = ( bag.Actions ?? new List<string>() )
                .Where( a => securedEntity.SupportedActions.ContainsKey( a ) )
                .Distinct()
                .ToList();

            var authService = new AuthService( RockContext );

            // Fetch every rule for this entity once and group by action, rather than
            // querying the database per action inside the loop.
            var authsByAction = authService.Get( securedEntity.TypeId, securedEntity.Id )
                .ToList()
                .GroupBy( a => a.Action )
                .ToDictionary( g => g.Key, g => g.ToList() );

            var addedActions = new List<string>();

            foreach ( var actionKey in selectedActions )
            {
                authsByAction.TryGetValue( actionKey, out var existingAuths );
                existingAuths = existingAuths ?? new List<Auth>();

                var alreadyExists = existingAuths.Any( a => a.SpecialRole == specialRole && a.GroupId == groupId );
                if ( alreadyExists )
                {
                    continue;
                }

                var order = existingAuths.Count > 0 ? existingAuths.Max( a => a.Order ) + 1 : 0;

                authService.Add( new Auth
                {
                    EntityTypeId = securedEntity.TypeId,
                    EntityId = securedEntity.Id,
                    Action = actionKey,
                    AllowOrDeny = "A",
                    SpecialRole = specialRole,
                    GroupId = groupId,
                    Order = order
                } );

                addedActions.Add( actionKey );
            }

            // Persist all new rules in a single round-trip, then refresh the
            // authorization cache for each action that actually changed.
            if ( addedActions.Count > 0 )
            {
                RockContext.SaveChanges();

                foreach ( var actionKey in addedActions )
                {
                    Authorization.RefreshAction( securedEntity.TypeId, securedEntity.Id, actionKey );
                }
            }

            return ActionOk( BuildActionData( securedEntity, bag.CurrentAction ) );
        }

        /// <summary>
        /// Adds a person to the permission list for the current action.
        /// </summary>
        /// <param name="personAliasGuid">The person alias unique identifier emitted by the person picker.</param>
        /// <param name="action">The action currently being viewed.</param>
        [BlockAction]
        public BlockActionResult AddUser( string personAliasGuid, string action )
        {
            if ( !TryGetEntityForAction( action, out var securedEntity, out var errorResult ) )
            {
                return errorResult;
            }

            var aliasGuid = personAliasGuid.AsGuidOrNull();
            if ( !aliasGuid.HasValue )
            {
                return ActionBadRequest( "Invalid person." );
            }

            var personAliasId = new PersonAliasService( RockContext ).Get( aliasGuid.Value )?.Id;
            if ( !personAliasId.HasValue )
            {
                return ActionBadRequest( "Invalid person." );
            }

            var authService = new AuthService( RockContext );
            var existingAuths = authService.GetAuths( securedEntity.TypeId, securedEntity.Id, action ).ToList();

            var alreadyExists = existingAuths.Any( a => a.PersonAliasId.HasValue && a.PersonAliasId.Value == personAliasId.Value );
            if ( !alreadyExists )
            {
                var order = existingAuths.Count > 0 ? existingAuths.Max( a => a.Order ) + 1 : 0;

                var auth = new Auth
                {
                    EntityTypeId = securedEntity.TypeId,
                    EntityId = securedEntity.Id,
                    Action = action,
                    AllowOrDeny = "A",
                    SpecialRole = SpecialRole.None,
                    PersonAliasId = personAliasId,
                    Order = order
                };
                authService.Add( auth );
                RockContext.SaveChanges();

                Authorization.RefreshAction( securedEntity.TypeId, securedEntity.Id, action );
            }

            return ActionOk( BuildActionData( securedEntity, action ) );
        }

        /// <summary>
        /// Gets the actions that may be granted to a role in the Add Role form. The
        /// action being viewed is always offered and pre-selected; other actions are
        /// offered only when the role does not already have a rule for them.
        /// </summary>
        /// <param name="roleValue">The selected role value.</param>
        /// <param name="action">The action currently being viewed.</param>
        [BlockAction]
        public BlockActionResult GetRoleActions( string roleValue, string action )
        {
            if ( !TryGetEntityForAction( action, out var securedEntity, out var errorResult ) )
            {
                return errorResult;
            }

            if ( !TryResolveRole( roleValue, out var specialRole, out var groupId ) )
            {
                return ActionBadRequest( "Invalid role." );
            }

            // Fetch every rule for this entity once, then evaluate each action in
            // memory rather than querying the database per action.
            var authsByAction = new AuthService( RockContext )
                .Get( securedEntity.TypeId, securedEntity.Id )
                .ToList()
                .GroupBy( a => a.Action )
                .ToDictionary( g => g.Key, g => g.ToList() );

            var items = new List<SecurityRoleActionItemBag>();

            foreach ( var supportedAction in securedEntity.SupportedActions )
            {
                var actionKey = supportedAction.Key;

                if ( actionKey == action )
                {
                    items.Add( new SecurityRoleActionItemBag
                    {
                        Action = actionKey,
                        Title = actionKey.SplitCase(),
                        IsSelected = true
                    } );
                    continue;
                }

                var roleHasAction = authsByAction.TryGetValue( actionKey, out var auths )
                    && auths.Any( a => a.SpecialRole == specialRole && a.GroupId == groupId );

                if ( !roleHasAction )
                {
                    items.Add( new SecurityRoleActionItemBag
                    {
                        Action = actionKey,
                        Title = actionKey.SplitCase(),
                        IsSelected = false
                    } );
                }
            }

            return ActionOk( items );
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Resolves the secured entity from the page parameters.
        /// </summary>
        /// <param name="securedEntity">On return, the resolved secured entity.</param>
        /// <param name="errorMessage">On return, the reason the entity could not be resolved.</param>
        /// <returns><c>true</c> if the entity was resolved; otherwise <c>false</c>.</returns>
        private bool TryGetSecuredEntity( out ISecured securedEntity, out string errorMessage )
        {
            securedEntity = null;
            errorMessage = null;

            var entityType = GetEntityType();
            var type = entityType?.GetEntityType();
            if ( type == null )
            {
                var name = entityType?.FriendlyName ?? string.Empty;
                errorMessage = $"The requested entity type ('{name}') could not be loaded to determine security attributes.";
                return false;
            }

            var entityIdParam = PageParameter( PageParameterKey.EntityId );
            if ( entityIdParam.IsNotNullOrWhiteSpace() && entityIdParam != "0" )
            {
                securedEntity = Reflection.GetIEntityForEntityType( type, entityIdParam ) as ISecured;
            }

            // When no specific entity is requested (or it could not be found),
            // secure the entity type itself.
            if ( securedEntity == null )
            {
                securedEntity = Activator.CreateInstance( type ) as ISecured;
            }

            if ( securedEntity == null )
            {
                errorMessage = "The item you are trying to secure does not exist or does not implement ISecured.";
                return false;
            }

            // Blocks expose additional actions defined by their block type.
            if ( securedEntity is Block block )
            {
                MergeBlockSecurityActions( block );
            }

            return true;
        }

        /// <summary>
        /// Resolves the entity type from the page parameter, which may be an Id, Guid, or IdKey.
        /// </summary>
        /// <returns>The matching <see cref="EntityTypeCache"/>, or <c>null</c>.</returns>
        private EntityTypeCache GetEntityType()
        {
            var entityTypeIdParam = PageParameter( PageParameterKey.EntityTypeId );

            if ( entityTypeIdParam.AsGuidOrNull() is Guid entityTypeGuid )
            {
                var id = EntityTypeCache.GetId( entityTypeGuid );
                return id.HasValue ? EntityTypeCache.Get( id.Value ) : null;
            }

            if ( int.TryParse( entityTypeIdParam, out var parsedEntityTypeId ) )
            {
                return EntityTypeCache.Get( parsedEntityTypeId );
            }

            if ( entityTypeIdParam.IsNotNullOrWhiteSpace() )
            {
                return EntityTypeCache.GetByIdKey( entityTypeIdParam );
            }

            return null;
        }

        /// <summary>
        /// Resolves the secured entity and confirms the current person may administrate it.
        /// </summary>
        /// <param name="securedEntity">On return, the authorized secured entity.</param>
        /// <returns><c>true</c> if the entity was resolved and the person is authorized; otherwise <c>false</c>.</returns>
        private bool TryGetAuthorizedEntity( out ISecured securedEntity )
        {
            securedEntity = null;

            if ( !TryGetSecuredEntity( out var entity, out _ ) )
            {
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
            {
                return false;
            }

            securedEntity = entity;
            return true;
        }

        /// <summary>
        /// Resolves and authorizes the secured entity and confirms it supports the action.
        /// Combines the guard every editing block action shares so each one can fail fast
        /// with the correct result.
        /// </summary>
        /// <param name="action">The action that must be supported by the entity.</param>
        /// <param name="securedEntity">On return, the authorized secured entity, or <c>null</c> when the guard fails.</param>
        /// <param name="errorResult">On return, the result to return to the caller when this method returns <c>false</c>.</param>
        /// <returns><c>true</c> if the entity was resolved, authorized, and supports the action; otherwise <c>false</c>.</returns>
        private bool TryGetEntityForAction( string action, out ISecured securedEntity, out BlockActionResult errorResult )
        {
            securedEntity = null;
            errorResult = null;

            if ( !TryGetAuthorizedEntity( out var entity ) )
            {
                errorResult = ActionForbidden( NotAuthorizedMessage );
                return false;
            }

            if ( action.IsNullOrWhiteSpace() || !entity.SupportedActions.ContainsKey( action ) )
            {
                errorResult = ActionBadRequest( "Invalid action." );
                return false;
            }

            securedEntity = entity;
            return true;
        }

        /// <summary>
        /// Merges the security actions defined by a block's block type into its supported actions.
        /// </summary>
        /// <param name="block">The block whose supported actions should be extended.</param>
        private void MergeBlockSecurityActions( Block block )
        {
            var blockCache = BlockCache.Get( block.Id );
            if ( blockCache?.BlockType == null )
            {
                return;
            }

            foreach ( var action in blockCache.BlockType.SecurityActions )
            {
                block.SupportedActions[action.Key] = action.Value;
            }
        }

        /// <summary>
        /// Builds the ordered list of action tabs, with the Administrate action moved to the end.
        /// </summary>
        /// <param name="securedEntity">The secured entity.</param>
        /// <returns>The ordered actions.</returns>
        private List<SecurityActionBag> GetOrderedActions( ISecured securedEntity )
        {
            var actions = securedEntity.SupportedActions
                .Select( a => new SecurityActionBag
                {
                    Action = a.Key,
                    Title = a.Key.SplitCase(),
                    Description = a.Value
                } )
                .ToList();

            var administrateIndex = actions.FindIndex( a => a.Action == Authorization.ADMINISTRATE );
            if ( administrateIndex != -1 )
            {
                var administrateAction = actions[administrateIndex];
                actions.RemoveAt( administrateIndex );
                actions.Add( administrateAction );
            }

            return actions;
        }

        /// <summary>
        /// Gets the action that should be selected by default, preferring View when supported.
        /// </summary>
        /// <param name="securedEntity">The secured entity.</param>
        /// <param name="orderedActions">The ordered actions.</param>
        /// <returns>The default action key.</returns>
        private string GetDefaultAction( ISecured securedEntity, List<SecurityActionBag> orderedActions )
        {
            if ( securedEntity.SupportedActions.ContainsKey( Authorization.VIEW ) )
            {
                return Authorization.VIEW;
            }

            return orderedActions.FirstOrDefault()?.Action ?? Authorization.VIEW;
        }

        /// <summary>
        /// Builds the permission data (item rules, inherited rules, and the default-access notice) for an action.
        /// </summary>
        /// <param name="securedEntity">The secured entity.</param>
        /// <param name="action">The action to build data for.</param>
        /// <returns>The action data.</returns>
        private SecurityActionDataBag BuildActionData( ISecured securedEntity, string action )
        {
            var authService = new AuthService( RockContext );

            var itemAuths = authService.GetAuths( securedEntity.TypeId, securedEntity.Id, action ).ToList();
            var itemRules = itemAuths.Select( a => new AuthRule( a ) ).ToList();

            var parentRules = new List<ParentAuthRule>();
            AddParentRules( authService, itemRules, parentRules, securedEntity.ParentAuthorityPre, action, false );
            AddParentRules( authService, itemRules, parentRules, securedEntity.ParentAuthority, action, true );

            var hasAllUsersEntry = itemRules.Any( r => r.SpecialRole == SpecialRole.AllUsers )
                || parentRules.Any( r => r.Rule.SpecialRole == SpecialRole.AllUsers );

            string noMatchMessage = null;
            if ( !hasAllUsersEntry )
            {
                var allowedOrDenied = securedEntity.IsAllowedByDefault( action ) ? "allowed" : "denied";
                noMatchMessage = $"The permission list does not include an \"All Users\" entry. Non-matching people will be {allowedOrDenied} access.";
            }

            return new SecurityActionDataBag
            {
                ItemRules = BuildItemRulesGrid( itemAuths ),
                ParentRules = BuildParentRulesGrid( parentRules ),
                NoMatchMessage = noMatchMessage
            };
        }

        /// <summary>
        /// Walks the parent authority chain collecting inherited rules, skipping any rule
        /// already represented on the item or by an ancestor closer to the entity.
        /// </summary>
        /// <param name="authService">The auth service.</param>
        /// <param name="itemRules">The rules defined directly on the entity.</param>
        /// <param name="parentRules">The accumulated inherited rules.</param>
        /// <param name="parent">The parent authority to inspect.</param>
        /// <param name="action">The action being inspected.</param>
        /// <param name="recurse">Whether to continue up the chain from this parent.</param>
        private void AddParentRules( AuthService authService, List<AuthRule> itemRules, List<ParentAuthRule> parentRules, ISecured parent, string action, bool recurse )
        {
            if ( parent == null )
            {
                return;
            }

            var entityType = EntityTypeCache.Get( parent.TypeId );

            foreach ( var auth in authService.GetAuths( parent.TypeId, parent.Id, action ) )
            {
                var rule = new AuthRule( auth );

                var alreadyOnItem = itemRules.Any( r =>
                    r.SpecialRole == rule.SpecialRole &&
                    r.PersonId == rule.PersonId &&
                    r.GroupId == rule.GroupId );

                var alreadyInherited = parentRules.Any( r =>
                    r.Rule.SpecialRole == rule.SpecialRole &&
                    r.Rule.PersonId == rule.PersonId &&
                    r.Rule.GroupId == rule.GroupId );

                if ( !alreadyOnItem && !alreadyInherited )
                {
                    var friendlyName = entityType?.FriendlyName ?? entityType?.Name;
                    parentRules.Add( new ParentAuthRule
                    {
                        Rule = rule,
                        EntityTitle = $"{parent} <small>({friendlyName})</small>".TrimStart()
                    } );
                }
            }

            if ( recurse )
            {
                AddParentRules( authService, itemRules, parentRules, parent.ParentAuthority, action, true );
            }
        }

        /// <summary>
        /// Builds the grid data for the rules defined directly on the entity.
        /// </summary>
        /// <param name="auths">The auth entities for the action.</param>
        /// <returns>The grid data.</returns>
        private GridDataBag BuildItemRulesGrid( List<Auth> auths )
        {
            var rows = auths
                .Select( a =>
                {
                    var rule = new AuthRule( a );
                    return new ItemRuleRow
                    {
                        Guid = a.Guid.ToString(),
                        DisplayName = rule.DisplayName,
                        AllowOrDeny = rule.AllowOrDeny.ToString()
                    };
                } )
                .ToList();

            return new GridBuilder<ItemRuleRow>()
                .AddTextField( "guid", r => r.Guid )
                .AddTextField( "displayName", r => r.DisplayName )
                .AddTextField( "allowOrDeny", r => r.AllowOrDeny )
                .Build( rows );
        }

        /// <summary>
        /// Builds the grid data for the inherited (parent authority) rules.
        /// </summary>
        /// <param name="parentRules">The inherited rules.</param>
        /// <returns>The grid data.</returns>
        private GridDataBag BuildParentRulesGrid( List<ParentAuthRule> parentRules )
        {
            return new GridBuilder<ParentAuthRule>()
                .AddTextField( "displayName", r => r.Rule.DisplayName )
                .AddTextField( "allowOrDeny", r => r.Rule.AllowOrDeny.ToString() )
                .AddTextField( "entityTitle", r => r.EntityTitle )
                .Build( parentRules );
        }

        /// <summary>
        /// Gets the role options for the Add Role dropdown, including the special roles
        /// followed by the configured security roles.
        /// </summary>
        /// <returns>The role list items.</returns>
        private List<ListItemBag> GetRoleListItems()
        {
            var roles = new List<ListItemBag>
            {
                new ListItemBag { Text = "[All Users]", Value = SpecialRoleValue.AllUsers },
                new ListItemBag { Text = "[All Authenticated Users]", Value = SpecialRoleValue.AllAuthenticatedUsers },
                new ListItemBag { Text = "[All Un-Authenticated Users]", Value = SpecialRoleValue.AllUnAuthenticatedUsers }
            };

            foreach ( var role in RoleCache.AllRoles() )
            {
                var name = role.IsSecurityTypeGroup ? role.Name : $"GROUP - {role.Name}";
                roles.Add( new ListItemBag { Text = name, Value = role.Id.ToString() } );
            }

            return roles;
        }

        /// <summary>
        /// Resolves a role dropdown value into a special role and/or group id, validating
        /// that any group is a real security role.
        /// </summary>
        /// <param name="roleValue">The selected role value.</param>
        /// <param name="specialRole">On return, the resolved special role.</param>
        /// <param name="groupId">On return, the resolved group id (or <c>null</c> for a special role).</param>
        /// <returns><c>true</c> if the value resolved to a valid role; otherwise <c>false</c>.</returns>
        private bool TryResolveRole( string roleValue, out SpecialRole specialRole, out int? groupId )
        {
            specialRole = SpecialRole.None;
            groupId = null;

            switch ( roleValue )
            {
                case SpecialRoleValue.AllUsers:
                    specialRole = SpecialRole.AllUsers;
                    return true;
                case SpecialRoleValue.AllAuthenticatedUsers:
                    specialRole = SpecialRole.AllAuthenticatedUsers;
                    return true;
                case SpecialRoleValue.AllUnAuthenticatedUsers:
                    specialRole = SpecialRole.AllUnAuthenticatedUsers;
                    return true;
            }

            var parsedGroupId = roleValue.AsIntegerOrNull();
            if ( !parsedGroupId.HasValue || RoleCache.Get( parsedGroupId.Value ) == null )
            {
                return false;
            }

            groupId = parsedGroupId.Value;
            return true;
        }

        /// <summary>
        /// Gets an auth entity by its unique identifier, confirming it belongs to the
        /// supplied secured entity and action before returning it.
        /// </summary>
        /// <param name="authService">The auth service.</param>
        /// <param name="authGuid">The unique identifier of the auth.</param>
        /// <param name="securedEntity">The secured entity the auth must belong to.</param>
        /// <param name="action">The action the auth must belong to.</param>
        /// <returns>The validated auth, or <c>null</c> if it does not belong to the entity/action.</returns>
        private Auth GetValidatedAuth( AuthService authService, string authGuid, ISecured securedEntity, string action )
        {
            var guid = authGuid.AsGuidOrNull();
            if ( !guid.HasValue )
            {
                return null;
            }

            var auth = authService.Get( guid.Value );
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

        #endregion

        #region Support Classes

        /// <summary>
        /// A single row in the item permissions grid.
        /// </summary>
        private class ItemRuleRow
        {
            public string Guid { get; set; }

            public string DisplayName { get; set; }

            public string AllowOrDeny { get; set; }
        }

        /// <summary>
        /// An inherited rule along with the title of the parent authority it came from.
        /// </summary>
        private class ParentAuthRule
        {
            public AuthRule Rule { get; set; }

            public string EntityTitle { get; set; }
        }

        #endregion
    }
}

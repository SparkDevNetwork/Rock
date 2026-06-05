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

using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Security.VerifySecurity;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Security
{
    /// <summary>
    /// Verifies the security of an entity and how it applies to a specified user.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Verify Security" )]
    [Category( "Security" )]
    [Description( "Verify the security of an entity and how it applies to a specified user." )]
    [IconCssClass( "ti ti-lock" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "81AA9B27-1663-4B04-B2A2-13BFF409ECA1" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "1208DFB0-188B-44B4-AD54-7EAC00687A1F" )]
    [Rock.SystemGuid.BlockTypeGuid( "65F18F6C-AD97-42A7-958D-20359E804965" )]
    public class VerifySecurity : RockBlockType
    {
        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<VerifySecurityOptionsBag>();
            var builder = GetGridBuilder();

            box.GridDefinition = builder.BuildDefinition();
            box.Options = GetBoxOptions();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the block.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private VerifySecurityOptionsBag GetBoxOptions()
        {
            var entityTypeItems = EntityTypeCache.All( RockContext )
                .Where( t => t.IsEntity && t.IsSecured )
                .OrderByDescending( t => t.IsCommon )
                .ThenBy( t => t.FriendlyName )
                .Select( t => new ListItemBag
                {
                    Value = t.Guid.ToString(),
                    Text = t.FriendlyName,
                    Category = t.IsCommon ? "Common" : "All Entities"
                } )
                .ToList();

            return new VerifySecurityOptionsBag
            {
                EntityTypeItems = entityTypeItems
            };
        }

        /// <summary>
        /// Gets the grid builder for the security results grid.
        /// </summary>
        /// <returns>The grid builder for the security results grid.</returns>
        private GridBuilder<SecurityResultRow> GetGridBuilder()
        {
            return new GridBuilder<SecurityResultRow>()
                .WithBlock( this )
                .AddTextField( "action", r => r.Action )
                .AddTextField( "sourceType", r => r.SourceType )
                .AddField( "sourceId", r => r.SourceId )
                .AddTextField( "sourceName", r => r.SourceName )
                .AddTextField( "role", r => r.Role )
                .AddTextField( "access", r => r.Access )
                .AddField( "isUnlockable", r => r.IsUnlockable )
                .AddTextField( "authIdKey", r => r.AuthIdKey );
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Checks the security of the selected entity for the selected person
        /// and returns one grid row per supported action describing the
        /// explicit authorization rule that governs it.
        /// </summary>
        /// <param name="selection">The selection describing the person and entity to check.</param>
        /// <returns>A grid data bag with the security results.</returns>
        [BlockAction]
        public BlockActionResult CheckSecurity( VerifySecuritySelectionBag selection )
        {
            var entity = GetSecuredEntity( selection );

            if ( entity == null )
            {
                return ActionBadRequest( "Could not find the entity, maybe the wrong Id was specified." );
            }

            var person = GetSelectedPerson( selection );
            var rows = BuildSecurityRows( entity, person );

            return ActionOk( GetGridBuilder().Build( rows ) );
        }

        /// <summary>
        /// Adds an explicit Allow rule for the selected person on the action
        /// that the specified authorization rule denied.
        /// </summary>
        /// <param name="selection">The selection describing the person and entity being checked.</param>
        /// <param name="authIdKey">The IdKey of the denying authorization rule.</param>
        /// <returns>A confirmation message when the rule has been added.</returns>
        [BlockAction]
        public BlockActionResult UnlockAccess( VerifySecuritySelectionBag selection, string authIdKey )
        {
            var authService = new AuthService( RockContext );
            var auth = authService.Get( authIdKey, !PageCache.Layout.Site.DisablePredictableIds );

            if ( auth == null )
            {
                return ActionBadRequest( "The security rule could not be found." );
            }

            var person = GetSelectedPerson( selection );

            if ( person == null )
            {
                return ActionBadRequest( "A person is required to add an Allow rule." );
            }

            var entity = GetSecuredEntity( selection );

            if ( entity == null )
            {
                return ActionBadRequest( "Could not find the entity, maybe the wrong Id was specified." );
            }

            /*
                6/4/26 - MSE

                The denying rule can belong to a parent authority (for example a
                parent page) rather than the entity being checked. When the person
                already has an explicit rule on that source entity it is flipped to
                Allow in place, which intentionally also opens up every other entity
                that inherits security from it. Otherwise a new Allow rule is added
                to the checked entity only.

                Reason: Popping a lock edits the rule at its source instead of stacking a new one.
            */
            var explicitAuth = authService.Queryable()
                .Where( a => a.EntityTypeId == auth.EntityTypeId && a.EntityId == auth.EntityId && a.Action == auth.Action )
                .Where( a => a.PersonAlias.PersonId == person.Id )
                .FirstOrDefault();

            if ( explicitAuth != null )
            {
                explicitAuth.AllowOrDeny = "A";
                RockContext.SaveChanges();

                // The flipped rule may live on a parent authority rather than the
                // checked entity, so refresh the cache entry it actually belongs to.
                Authorization.RefreshAction( explicitAuth.EntityTypeId, explicitAuth.EntityId ?? 0, explicitAuth.Action, RockContext );
            }
            else
            {
                Authorization.AllowPerson( entity, auth.Action, person, RockContext );
            }

            return ActionOk( $"An explicit Allow rule has been added for {person.FullName}" );
        }

        /// <summary>
        /// Clears the authorization cache so the next security checks read
        /// fresh rules from the database.
        /// </summary>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult ClearAuthorizationCache()
        {
            Authorization.Clear();

            return ActionOk();
        }

        #endregion Block Actions

        #region Private Methods

        /// <summary>
        /// Gets the secured entity described by the selection. The identifier
        /// is matched as an integer Id first, then a Guid, then an IdKey.
        /// </summary>
        /// <param name="selection">The selection describing the entity to load.</param>
        /// <returns>The secured entity, or null when it could not be found.</returns>
        private ISecured GetSecuredEntity( VerifySecuritySelectionBag selection )
        {
            if ( selection == null || !selection.EntityTypeGuid.HasValue || selection.EntityIdentifier.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var entityType = EntityTypeCache.Get( selection.EntityTypeGuid.Value )?.GetEntityType();

            if ( entityType == null )
            {
                return null;
            }

            var entity = Reflection.GetIEntityForEntityType( entityType, selection.EntityIdentifier, allowIntegerIdentifier: true, RockContext );

            return entity as ISecured;
        }

        /// <summary>
        /// Gets the person whose access is being checked, falling back to the
        /// current person when no person was selected.
        /// </summary>
        /// <param name="selection">The selection describing the person to check.</param>
        /// <returns>The person to check, or null when no person could be resolved.</returns>
        private Person GetSelectedPerson( VerifySecuritySelectionBag selection )
        {
            if ( selection?.PersonAliasGuid != null )
            {
                return new PersonAliasService( RockContext ).Get( selection.PersonAliasGuid.Value )?.Person;
            }

            return GetCurrentPerson();
        }

        /// <summary>
        /// Builds one grid row per supported action of the entity, each
        /// describing the explicit authorization rule that governs the action
        /// and where that rule came from.
        /// </summary>
        /// <param name="entity">The secured entity whose actions are checked.</param>
        /// <param name="person">The person whose access is checked, or null for an unauthenticated user.</param>
        /// <returns>The list of security result rows.</returns>
        private List<SecurityResultRow> BuildSecurityRows( ISecured entity, Person person )
        {
            var context = new SecurityWalkContext
            {
                Person = person,
                PersonAliasIds = GetPersonAliasIds( person ),
                AuthService = new AuthService( RockContext )
            };

            var rows = new List<SecurityResultRow>();

            foreach ( var action in entity.SupportedActions )
            {
                var auth = GetEffectiveAuth( entity, action.Key, context, isRootEntity: true, checkParentAuthority: true, out var authoritativeEntity );

                if ( auth == null )
                {
                    rows.Add( new SecurityResultRow
                    {
                        Action = action.Key,
                        SourceType = string.Empty,
                        SourceName = string.Empty,
                        Access = "Unknown",
                        Role = "No explicit permissions found",
                        IsUnlockable = false
                    } );

                    continue;
                }

                var roleName = "Unknown";

                if ( auth.SpecialRole != SpecialRole.None )
                {
                    roleName = auth.SpecialRole.ToStringSafe().SplitCase();
                }
                else if ( auth.PersonAlias != null )
                {
                    roleName = auth.PersonAlias.ToStringSafe();
                }
                else if ( auth.Group != null )
                {
                    roleName = auth.Group.ToStringSafe();
                }

                var row = new SecurityResultRow
                {
                    Action = action.Key,
                    Role = roleName,
                    Access = auth.AllowOrDeny == "A" ? "Allow" : "Deny",
                    IsUnlockable = auth.AllowOrDeny != "A",
                    AuthIdKey = auth.IdKey
                };

                if ( authoritativeEntity is IEntity authoritative )
                {
                    // An Id of zero means the rule guards the entity type's
                    // administration security rather than a specific instance.
                    row.SourceType = authoritative.TypeName;
                    row.SourceId = authoritative.Id != 0 ? ( int? ) authoritative.Id : null;
                    row.SourceName = authoritative.Id != 0 ? authoritative.ToString() : "(Entity Administration Security)";
                }
                else if ( authoritativeEntity is GlobalDefault )
                {
                    row.SourceType = "(Global Default)";
                    row.SourceName = string.Empty;
                }
                else
                {
                    row.SourceType = "Unknown";
                    row.SourceName = "Unknown";
                }

                rows.Add( row );
            }

            return rows;
        }

        /// <summary>
        /// Gets all of the person's alias identifiers so person-specific rules
        /// can be matched without loading aliases per rule.
        /// </summary>
        /// <param name="person">The person whose alias identifiers are loaded.</param>
        /// <returns>The set of alias identifiers, empty when there is no person.</returns>
        private HashSet<int> GetPersonAliasIds( Person person )
        {
            if ( person == null )
            {
                return new HashSet<int>();
            }

            var aliasIds = new PersonAliasService( RockContext )
                .Queryable()
                .Where( a => a.PersonId == person.Id )
                .Select( a => a.Id )
                .ToList();

            return new HashSet<int>( aliasIds );
        }

        /// <summary>
        /// Finds the explicit authorization rule that governs the action by
        /// walking the same resolution order as Rock's authorization engine:
        /// the entity's own rules first, then the pre-parent authority for the
        /// root entity, then the parent authority chain.
        /// </summary>
        /// <param name="entity">The secured entity whose rules are searched.</param>
        /// <param name="action">The action that is being authorized.</param>
        /// <param name="context">The shared state for the authority chain walk.</param>
        /// <param name="isRootEntity">True on the first call, false when called recursively.</param>
        /// <param name="checkParentAuthority">True when the parent authorities should also be searched.</param>
        /// <param name="authoritativeEntity">On return, the secured entity that the matching rule belongs to.</param>
        /// <returns>The matching authorization rule, or null when no explicit rule applies.</returns>
        private Auth GetEffectiveAuth( ISecured entity, string action, SecurityWalkContext context, bool isRootEntity, bool checkParentAuthority, out ISecured authoritativeEntity )
        {
            var auth = GetMatchingAuth( entity, action, context );

            if ( auth != null )
            {
                authoritativeEntity = entity;
                return auth;
            }

            if ( checkParentAuthority )
            {
                if ( isRootEntity && entity.ParentAuthorityPre != null )
                {
                    var preAuth = GetEffectiveAuth( entity.ParentAuthorityPre, action, context, isRootEntity: false, checkParentAuthority: false, out var preAuthority );

                    if ( preAuth != null )
                    {
                        authoritativeEntity = preAuthority;
                        return preAuth;
                    }
                }

                if ( entity.ParentAuthority != null )
                {
                    var parentAuth = GetEffectiveAuth( entity.ParentAuthority, action, context, isRootEntity: false, checkParentAuthority: true, out var parentAuthority );

                    if ( parentAuth != null )
                    {
                        authoritativeEntity = parentAuthority;
                        return parentAuth;
                    }
                }
            }

            authoritativeEntity = null;
            return null;
        }

        /// <summary>
        /// Finds the first of the entity's own authorization rules, in order,
        /// that applies to the person for the action.
        /// </summary>
        /// <param name="entity">The secured entity whose rules are searched.</param>
        /// <param name="action">The action that is being authorized.</param>
        /// <param name="context">The shared state for the authority chain walk.</param>
        /// <returns>The matching authorization rule, or null when none applies.</returns>
        private Auth GetMatchingAuth( ISecured entity, string action, SecurityWalkContext context )
        {
            var rules = GetAuthRules( entity, context )
                .Where( a => a.Action == action );

            foreach ( var rule in rules )
            {
                if ( rule.SpecialRole == SpecialRole.AllUsers )
                {
                    return rule;
                }

                if ( rule.SpecialRole == SpecialRole.AllAuthenticatedUsers && context.Person != null )
                {
                    return rule;
                }

                if ( rule.SpecialRole == SpecialRole.AllUnAuthenticatedUsers && context.Person == null )
                {
                    return rule;
                }

                if ( rule.SpecialRole == SpecialRole.None && context.Person != null )
                {
                    // The rule may apply to the person directly.
                    if ( rule.PersonAliasId.HasValue && context.PersonAliasIds.Contains( rule.PersonAliasId.Value ) )
                    {
                        return rule;
                    }

                    // The rule may apply through a security role the person belongs to.
                    if ( rule.GroupId.HasValue )
                    {
                        var role = RoleCache.Get( rule.GroupId.Value );

                        if ( role != null && role.IsPersonInRole( context.Person.Guid ) )
                        {
                            return rule;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the entity's authorization rules ordered by rule order. The
        /// authority chain is identical for every action, so results are cached
        /// per entity for the duration of the walk.
        /// </summary>
        /// <param name="entity">The secured entity whose rules are loaded.</param>
        /// <param name="context">The shared state for the authority chain walk.</param>
        /// <returns>The entity's authorization rules.</returns>
        private List<Auth> GetAuthRules( ISecured entity, SecurityWalkContext context )
        {
            var key = (entity.TypeId, entity.Id);

            if ( !context.RulesByEntity.TryGetValue( key, out var rules ) )
            {
                rules = context.AuthService.Get( entity.TypeId, entity.Id )
                    .Include( a => a.PersonAlias.Person )
                    .Include( a => a.Group )
                    .ToList();

                context.RulesByEntity.Add( key, rules );
            }

            return rules;
        }

        #endregion Private Methods

        #region Supporting Classes

        /// <summary>
        /// The state shared by the recursive authority chain walk so every
        /// action reuses the same person data and cached rule lookups.
        /// </summary>
        private class SecurityWalkContext
        {
            /// <summary>
            /// Gets or sets the person whose access is being checked, or null
            /// for an unauthenticated user.
            /// </summary>
            public Person Person { get; set; }

            /// <summary>
            /// Gets or sets the person's alias identifiers used to match
            /// person-specific rules.
            /// </summary>
            public HashSet<int> PersonAliasIds { get; set; }

            /// <summary>
            /// Gets or sets the service used to load authorization rules.
            /// </summary>
            public AuthService AuthService { get; set; }

            /// <summary>
            /// Gets the authorization rules already loaded during this walk,
            /// keyed by the entity they belong to.
            /// </summary>
            public Dictionary<(int TypeId, int EntityId), List<Auth>> RulesByEntity { get; } = new Dictionary<(int TypeId, int EntityId), List<Auth>>();
        }

        /// <summary>
        /// A POCO to represent one security result row in the grid.
        /// </summary>
        private class SecurityResultRow
        {
            /// <summary>
            /// Gets or sets the action that was checked, such as View or Edit.
            /// </summary>
            public string Action { get; set; }

            /// <summary>
            /// Gets or sets the type name of the entity the governing rule
            /// belongs to.
            /// </summary>
            public string SourceType { get; set; }

            /// <summary>
            /// Gets or sets the identifier of the entity the governing rule
            /// belongs to, or null when the rule is not instance-specific.
            /// </summary>
            public int? SourceId { get; set; }

            /// <summary>
            /// Gets or sets the name of the entity the governing rule belongs to.
            /// </summary>
            public string SourceName { get; set; }

            /// <summary>
            /// Gets or sets the user, role or special role the governing rule
            /// applies to.
            /// </summary>
            public string Role { get; set; }

            /// <summary>
            /// Gets or sets the resulting access: Allow, Deny or Unknown.
            /// </summary>
            public string Access { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether an explicit Allow rule
            /// can be added for this action.
            /// </summary>
            public bool IsUnlockable { get; set; }

            /// <summary>
            /// Gets or sets the IdKey of the governing authorization rule, or
            /// null when no explicit rule was found.
            /// </summary>
            public string AuthIdKey { get; set; }
        }

        #endregion Supporting Classes
    }
}

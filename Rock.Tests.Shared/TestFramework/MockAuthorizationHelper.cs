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

using System.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Tests.Shared.TestFramework
{
    /// <summary>
    /// Helper methods for seeding authorization rules into a mocked
    /// <see cref="RockContext"/> so security-checking code can be exercised.
    /// </summary>
    /// <remarks>
    /// Rules are read by <see cref="Authorization"/> through the authorization
    /// cache, which loads from the <see cref="Auth"/> set on the mocked context.
    /// Seeding a rule at entity id <c>0</c> covers a new entity, whose
    /// authorization resolves to the global default, as well as an existing entity
    /// whose parent authority chain resolves to that same default, so a single
    /// grant is usually enough for both the add and update paths of a tool.
    /// </remarks>
    public static class MockAuthorizationHelper
    {
        /// <summary>
        /// Grants the specified action to all users on a secured entity type.
        /// </summary>
        /// <typeparam name="TSecured">The secured entity type to grant access on.</typeparam>
        /// <param name="rockContext">The mocked context to seed the rule into.</param>
        /// <param name="action">The action to grant, such as <see cref="Authorization.EDIT"/>.</param>
        /// <param name="entityId">The entity id the rule applies to. Defaults to <c>0</c>, which covers new entities and inherited defaults.</param>
        public static void AllowAllUsers<TSecured>( RockContext rockContext, string action, int entityId = 0 )
            where TSecured : IEntity
        {
            AddRule<TSecured>( rockContext, action, "A", entityId );
        }

        /// <summary>
        /// Denies the specified action to all users on a secured entity type.
        /// </summary>
        /// <typeparam name="TSecured">The secured entity type to deny access on.</typeparam>
        /// <param name="rockContext">The mocked context to seed the rule into.</param>
        /// <param name="action">The action to deny, such as <see cref="Authorization.EDIT"/>.</param>
        /// <param name="entityId">The entity id the rule applies to. Defaults to <c>0</c>, which covers new entities and inherited defaults.</param>
        public static void DenyAllUsers<TSecured>( RockContext rockContext, string action, int entityId = 0 )
            where TSecured : IEntity
        {
            var entityTypeId = EntityTypeCache.Get( typeof( TSecured ), true, rockContext ).Id;

            AddRule( rockContext, entityTypeId, action, "D", entityId );
        }

        /// <summary>
        /// Grants the specified action to all users at the global default, the root
        /// of every entity's authorization chain.
        /// </summary>
        /// <remarks>
        /// Use this when an entity inherits its security from a parent of a
        /// different type (a defined value from its defined type, for example), so a
        /// grant on the entity's own type would never be reached. A grant here
        /// applies to anything that falls through to the default, which is the
        /// realistic way to say "everyone may perform this action by default."
        /// </remarks>
        /// <param name="rockContext">The mocked context to seed the rule into.</param>
        /// <param name="action">The action to grant, such as <see cref="Authorization.EDIT"/>.</param>
        public static void AllowAllUsersByDefault( RockContext rockContext, string action )
        {
            var entityTypeId = EntityTypeCache.Get( typeof( GlobalDefault ), true, rockContext ).Id;

            AddRule( rockContext, entityTypeId, action, "A", 0 );
        }

        /// <summary>
        /// Adds a single authorization rule to the mocked context for a secured
        /// entity type, resolving the entity type id for <typeparamref name="TSecured"/>.
        /// </summary>
        /// <typeparam name="TSecured">The secured entity type the rule applies to.</typeparam>
        /// <param name="rockContext">The mocked context to seed the rule into.</param>
        /// <param name="action">The action the rule governs.</param>
        /// <param name="allowOrDeny">"A" to allow, "D" to deny.</param>
        /// <param name="entityId">The entity id the rule applies to. Defaults to <c>0</c>.</param>
        /// <param name="specialRole">The special role the rule applies to. Defaults to <see cref="SpecialRole.AllUsers"/>.</param>
        /// <param name="personAliasId">The person alias the rule applies to, for a person-specific rule.</param>
        /// <param name="groupId">The group (role) the rule applies to, for a role-specific rule.</param>
        /// <param name="order">The order of the rule within the entity's rules.</param>
        /// <returns>The created <see cref="Auth"/> rule.</returns>
        public static Auth AddRule<TSecured>( RockContext rockContext, string action, string allowOrDeny, int entityId = 0, SpecialRole specialRole = SpecialRole.AllUsers, int? personAliasId = null, int? groupId = null, int order = 0 )
            where TSecured : IEntity
        {
            var entityTypeId = EntityTypeCache.Get( typeof( TSecured ), true, rockContext ).Id;

            return AddRule( rockContext, entityTypeId, action, allowOrDeny, entityId, specialRole, personAliasId, groupId, order );
        }

        /// <summary>
        /// Adds a single authorization rule to the mocked context.
        /// </summary>
        /// <param name="rockContext">The mocked context to seed the rule into.</param>
        /// <param name="entityTypeId">The entity type the rule applies to.</param>
        /// <param name="action">The action the rule governs.</param>
        /// <param name="allowOrDeny">"A" to allow, "D" to deny.</param>
        /// <param name="entityId">The entity id the rule applies to. Defaults to <c>0</c>.</param>
        /// <param name="specialRole">The special role the rule applies to. Defaults to <see cref="SpecialRole.AllUsers"/>.</param>
        /// <param name="personAliasId">The person alias the rule applies to, for a person-specific rule.</param>
        /// <param name="groupId">The group (role) the rule applies to, for a role-specific rule.</param>
        /// <param name="order">The order of the rule within the entity's rules.</param>
        /// <returns>The created <see cref="Auth"/> rule.</returns>
        public static Auth AddRule( RockContext rockContext, int entityTypeId, string action, string allowOrDeny, int entityId = 0, SpecialRole specialRole = SpecialRole.AllUsers, int? personAliasId = null, int? groupId = null, int order = 0 )
        {
            var authSet = rockContext.Set<Auth>();

            // Take the next id past the highest already present, so a test that has
            // seeded its own non-sequential Auth rows cannot collide with this one.
            var nextId = ( authSet.Max( a => ( int? ) a.Id ) ?? 0 ) + 1;

            var auth = new Auth
            {
                Id = nextId,
                Guid = System.Guid.NewGuid(),
                EntityTypeId = entityTypeId,
                EntityId = entityId,
                Action = action,
                AllowOrDeny = allowOrDeny,
                SpecialRole = specialRole,
                PersonAliasId = personAliasId,
                GroupId = groupId,
                Order = order
            };

            authSet.Add( auth );

            return auth;
        }
    }
}

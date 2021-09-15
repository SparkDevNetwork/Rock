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
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;
using Rock.Security;

namespace Rock.Web.Cache
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class AuthorizationEntityTypeAuthCache : ItemCache<AuthorizationEntityTypeAuthCache>
    {
        #region Constructor

        /// <summary>
        /// </summary>
        private AuthorizationEntityTypeAuthCache()
        {
        }

        #endregion Constructor

        #region Properties

        /// <summary>
        /// Gets the entity type identifier.
        /// </summary>
        /// <value>
        /// The entity type identifier.
        /// </value>
        [DataMember]
        public int EntityTypeId { get; private set; }

        /// <summary>
        /// A lookup for AuthRules by EntityId
        /// </summary>
        /// <value>
        /// The authentication entity rule list.
        /// </value>
        [DataMember]
        private ConcurrentDictionary<int, AuthRule[]> AuthRuleListByEntityId { get; set; }

        /// <summary>
        /// A lookup for AuthRules by EntityId and Action
        /// </summary>
        /// <value>
        /// The authentication entity rule list by entity identifier action.
        /// </value>
        [DataMember]
        private ConcurrentDictionary<int, ConcurrentDictionary<string, AuthRule[]>> AuthRuleListByEntityIdAction { get; set; }

        /// <summary>
        /// Gets the AuthRules for the specified entityId
        /// </summary>
        /// <param name="entityId">The entity identifier.</param>
        /// <returns></returns>
        public AuthRule[] GetByEntityId( int entityId )
        {
            return AuthRuleListByEntityId.GetValueOrNull( entityId ) ?? new AuthRule[0];
        }

        /// <summary>
        /// Gets the AuthRules for the specified entityId and action
        /// </summary>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public AuthRule[] GetByEntityIdAction( int entityId, string action )
        {
            var ruleList = AuthRuleListByEntityIdAction.GetValueOrNull( entityId )?.GetValueOrNull( action ) ?? new AuthRule[0];
            return ruleList;
        }

        #endregion Properties

        #region Static Methods

        /// <summary>
        /// Gets the <see cref="AuthorizationEntityTypeAuthCache" /> for the specified EntityTypeId
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <returns></returns>
        public static AuthorizationEntityTypeAuthCache Get( int entityTypeId )
        {
            return Get( entityTypeId, null );
        }

        /// <summary>
        /// Gets the <see cref="AuthorizationEntityTypeAuthCache" /> for the specified EntityTypeId
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static AuthorizationEntityTypeAuthCache Get( int entityTypeId, RockContext rockContext )
        {
            return GetOrAddExisting( entityTypeId, () => QueryDb( entityTypeId, rockContext ) );
        }

        /// <summary>
        /// Queries the database.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private static AuthorizationEntityTypeAuthCache QueryDb( int entityTypeId, RockContext rockContext )
        {
            if ( rockContext != null )
            {
                return QueryDbWithContext( entityTypeId, rockContext );
            }

            using ( var rockContext2 = new RockContext() )
            {
                return QueryDbWithContext( entityTypeId, rockContext2 );
            }
        }

        /// <summary>
        /// Queries the database with context.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private static AuthorizationEntityTypeAuthCache QueryDbWithContext( int entityTypeId, RockContext rockContext )
        {
            var securityGroupTypeId = GroupTypeCache.GetId( SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid() ) ?? 0;

            // fetch into a list, then sort in C#
            var authList = new AuthService( rockContext )
                    .Queryable()
                    .Where( t =>
                         t.EntityTypeId == entityTypeId
                         && ( t.Group == null || ( t.Group.IsActive && ( t.Group.IsSecurityRole || t.Group.GroupTypeId == securityGroupTypeId ) ) )
                        && t.EntityId.HasValue )
                    .Select( a => new
                    {
                        a.Id,
                        a.EntityId,
                        a.Action,
                        a.AllowOrDeny,
                        a.SpecialRole,
                        a.PersonAliasId,
                        PersonId = a.PersonAlias != null ? a.PersonAlias.PersonId : ( int? ) null,
                        a.GroupId,
                        a.Order
                    } )
                    .ToList()
                    .OrderBy( a => a.Action )
                    .ThenBy( a => a.Order )
                    .ThenBy( a => a.Id )
                    .ToList();

            var authEntityRuleListByEntityId = authList
                .GroupBy( a => a.EntityId.Value )
                .ToDictionary(
                    k => k.Key,
                    v => v.Select(
                        auth => new AuthEntityRule(
                            auth.Id,
                            entityTypeId,
                            auth.Action,
                            auth.EntityId,
                            auth.AllowOrDeny,
                            auth.SpecialRole,
                            auth.PersonId,
                            auth.PersonAliasId,
                            auth.GroupId,
                            auth.Order ) )
                        .ToArray() );

            var authRuleListByEntityId = authEntityRuleListByEntityId.ToDictionary(
                k => k.Key,
                v => v.Value.Select( s => s.AuthRule ).ToArray() );

            var authRuleListByEntityIdAction = authEntityRuleListByEntityId
                .ToDictionary(
                    k => k.Key,
                    v =>
                    {
                        var lookup = v.Value
                            .GroupBy( g => g.Action )
                                .ToDictionary(
                                    kk => kk.Key,
                                    vv => vv.Select( s => s.AuthRule ).ToArray() );

                        return new ConcurrentDictionary<string, AuthRule[]>( lookup );
                    } );

            return new AuthorizationEntityTypeAuthCache
            {
                EntityTypeId = entityTypeId,
                AuthRuleListByEntityId = new ConcurrentDictionary<int, AuthRule[]>( authRuleListByEntityId ),
                AuthRuleListByEntityIdAction = new ConcurrentDictionary<int, ConcurrentDictionary<string, AuthRule[]>>( authRuleListByEntityIdAction )
            };
        }
    }

    #endregion Static Methods
}
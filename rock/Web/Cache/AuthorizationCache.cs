// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Data.Entity;
using System.Linq;
using System.Runtime.Caching;

using Rock.Data;
using Rock.Model;
using Rock.Security;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Authorization rules for each EntityType/Entity/Action combination
    /// </summary>
    [Serializable]
    public class AuthorizationCache
    {
        #region Properties

        /// <summary>
        /// Gets or sets the entity type identifier.
        /// </summary>
        /// <value>
        /// The entity type identifier.
        /// </value>
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        /// <value>
        /// The entity identifier.
        /// </value>
        public int? EntityId { get; set; }

        /// <summary>
        /// Gets or sets the action.
        /// </summary>
        /// <value>
        /// The action.
        /// </value>
        public string Action { get; set; }

        /// <summary>
        /// Gets or sets the permissions.
        /// </summary>
        /// <value>
        /// The permissions.
        /// </value>
        public List<AuthRule> Permissions { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Prevents a default instance of the <see cref="AuthorizationCache"/> class from being created.
        /// </summary>
        private AuthorizationCache() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationCache"/> class.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="action">The action.</param>
        /// <param name="permissions">The permissions.</param>
        private AuthorizationCache( int entityTypeId, int? entityId, string action, List<AuthRule> permissions )
        {
            EntityTypeId = entityTypeId;
            EntityId = entityId;
            Action = action;
            Permissions = permissions;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Reads the specified entity type identifier.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="action">The action.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static AuthorizationCache Read( int entityTypeId, int? entityId, string action, RockContext rockContext = null )
        {
            return GetOrAddExisting( AuthorizationCache.CacheKey( entityTypeId, entityId, action ),
                () => Load( entityTypeId, entityId, action, rockContext ) );
        }

        private static AuthorizationCache Load( int entityTypeId, int? entityId, string action, RockContext rockContext )
        {
            if ( rockContext != null )
            {
                return Load2( entityTypeId, entityId, action, rockContext );
            }

            using ( var rockContext2 = new RockContext() )
            {
                return Load2( entityTypeId, entityId, action, rockContext2 );
            }
        }

        private static AuthorizationCache Load2( int entityTypeId, int? entityId, string action, RockContext rockContext )
        {
            var securityGroupType = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid(), rockContext );
            int securityGroupTypeId = securityGroupType != null ? securityGroupType.Id : 0;

            var authRules = new List<AuthRule>();

            foreach ( Auth auth in new AuthService( rockContext )
                .GetAuths( entityTypeId, entityId, action )
                .AsNoTracking()
                .Where( t =>
                    t.Group == null ||
                    ( t.Group.IsActive && ( t.Group.IsSecurityRole || t.Group.GroupTypeId == securityGroupTypeId ) )
                ) )
            {
                authRules.Add( new AuthRule( auth ) );
            }

            return new AuthorizationCache( entityTypeId, entityId, action, authRules );
        }

        /// <summary>
        /// Gets the existing or a new item from cache
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="valueFactory">The value factory.</param>
        /// <returns></returns>
        private static AuthorizationCache GetOrAddExisting( string key, Func<AuthorizationCache> valueFactory )
        {
            RockMemoryCache cache = RockMemoryCache.Default;

            object cacheValue = cache.Get( key );
            if ( cacheValue != null )
            {
                return (AuthorizationCache)cacheValue;
            }

            AuthorizationCache value = valueFactory();
            if ( value != null )
            {
                cache.Set( key, value, new CacheItemPolicy() );
            }
            return value;
        }

        /// <summary>
        /// Gets the cache key for the selected entitytype/entity/action.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        private static string CacheKey( int entityTypeId, int? entityId, string action )
        {
            return string.Format( "Rock:Auth:{0}:{1}:{2}", entityTypeId, ( entityId.HasValue ? entityId.Value.ToString() : "" ), action );
        }


        /// <summary>
        /// Flushes the specified entity type identifier.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="action">The action.</param>
        public static void Flush( int entityTypeId, int? entityId, string action )
        {
            RockMemoryCache cache = RockMemoryCache.Default;
            cache.Remove( CacheKey( entityTypeId, entityId, action ) );
        }

        #endregion

    }
}
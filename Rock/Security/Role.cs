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
using System.Collections.Concurrent;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Caching;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Security
{
    /// <summary>
    /// Information about a Role that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    public class Role
    {
        /// <summary>
        /// Use Static Read() method to instantiate a new Role object
        /// </summary>
        private Role() { }

        /// <summary>
        /// Gets the id.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is security type group.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is security type group; otherwise, <c>false</c>.
        /// </value>
        public bool IsSecurityTypeGroup { get; private set; }

        /// <summary>
        /// Gets the people.
        /// </summary>
        /// <value>
        /// The people.
        /// </value>
        public ConcurrentDictionary<Guid, bool> People { get; private set; }

        /// <summary>
        /// Is user in role
        /// </summary>
        /// <param name="personGuid">The person unique identifier.</param>
        /// <returns></returns>
        public bool IsPersonInRole( Guid? personGuid )
        {
            if ( personGuid.HasValue )
            {
                bool inRole = false;
                if ( People.TryGetValue( personGuid.Value, out inRole ) )
                {
                    return inRole;
                }
            }

            return false;
        }

        #region Static Methods

        /// <summary>
        /// Caches the key.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        private static string CacheKey( int id )
        {
            return string.Format( "Rock:Role:{0}", id );
        }

        /// <summary>
        /// Returns Role object from cache.  If role does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static Role Read( int id )
        {
            return GetOrAddExisting( Role.CacheKey( id ),
                () => LoadById( id ) );
        }

        /// <summary>
        /// Gets the or add existing.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="valueFactory">The value factory.</param>
        /// <returns></returns>
        public static Role GetOrAddExisting( string key, Func<Role> valueFactory )
        {
            RockMemoryCache cache = RockMemoryCache.Default;

            object cacheValue = cache.Get( key );
            if ( cacheValue != null )
            {
                return (Role)cacheValue;
            }

            Role value = valueFactory();
            if ( value != null )
            {
                cache.Set( key, value, new CacheItemPolicy() );
            }
            return value;
        }

        /// <summary>
        /// Loads the by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public static Role LoadById( int id )
        {
            using ( var rockContext = new RockContext() )
            {
                var securityGroupType = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid(), rockContext );
                int securityGroupTypeId = securityGroupType != null ? securityGroupType.Id : 0;

                Rock.Model.GroupService groupService = new Rock.Model.GroupService( rockContext );
                Rock.Model.GroupMemberService groupMemberService = new Rock.Model.GroupMemberService( rockContext );
                Rock.Model.Group groupModel = groupService.Get( id );

                if ( groupModel != null && ( groupModel.IsSecurityRole == true || groupModel.GroupTypeId == securityGroupTypeId ) )
                {
                    var role = new Role();
                    role.Id = groupModel.Id;
                    role.Name = groupModel.Name;
                    role.People = new ConcurrentDictionary<Guid,bool>();

                    var groupMembersQry = groupMemberService.Queryable().Where( a => a.GroupId == groupModel.Id );

                    // Add the members
                    foreach ( var personGuid in groupMembersQry
                        .Where( m => 
                            m.PersonId != null &&
                            m.GroupMemberStatus == Model.GroupMemberStatus.Active )
                        .Select( m => m.Person.Guid )
                        .ToList()
                        .Distinct() )
                    {
                        role.People.TryAdd( personGuid, true );
                    }

                    role.IsSecurityTypeGroup = groupModel.GroupTypeId == securityGroupTypeId;
                        
                    return role;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns a list of all the possible Roles
        /// </summary>
        /// <returns></returns>
        public static List<Role> AllRoles()
        {
            List<Role> roles = new List<Role>();

            var securityGroupType = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid() );
            int securityGroupTypeId = securityGroupType != null ? securityGroupType.Id : 0;

            Rock.Model.GroupService groupService = new Rock.Model.GroupService( new RockContext() );
            foreach ( int id in groupService.Queryable()
                .Where( g => 
                    g.GroupTypeId == securityGroupTypeId ||
                    g.IsSecurityRole == true )
                .OrderBy( g => g.Name )
                .Select( g => g.Id )
                .ToList() )
            {
                roles.Add( Role.Read( id ) );
            }

            return roles;
        }

        /// <summary>
        /// Removes role from cache
        /// </summary>
        /// <param name="id">The id.</param>
        public static void Flush( int id )
        {
            ObjectCache cache = Rock.Web.Cache.RockMemoryCache.Default;
            cache.Remove( Role.CacheKey( id ) );
        }

        #endregion
    }
}
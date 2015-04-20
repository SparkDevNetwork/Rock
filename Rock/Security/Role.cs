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
using System.Linq;
using System.Runtime.Caching;
using Rock.Data;

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
        /// Gets the users that belong to the role
        /// </summary>
        public List<string> Users { get; private set; }

        /// <summary>
        /// Is user in role
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool IsUserInRole( string user )
        {
            return Users.Contains( user );
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
            string cacheKey = Role.CacheKey( id );

            ObjectCache cache = Rock.Web.Cache.RockMemoryCache.Default;
            Role role = cache[cacheKey] as Role;

            if ( role != null )
            {
                return role;
            }
            else
            {
                using ( var rockContext = new RockContext() )
                {
                    Rock.Model.GroupService groupService = new Rock.Model.GroupService( rockContext );
                    Rock.Model.Group groupModel = groupService.Get( id );

                    if ( groupModel != null && groupModel.IsSecurityRole == true )
                    {
                        role = new Role();
                        role.Id = groupModel.Id;
                        role.Name = groupModel.Name;
                        role.Users = new List<string>();
                        role.IsSecurityTypeGroup = groupModel.GroupType.Guid.Equals( Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid() );

                        foreach ( Rock.Model.GroupMember member in groupModel.Members )
                        {
                            role.Users.Add( member.Person.Guid.ToString() );
                        }

                        cache.Set( cacheKey, role, new CacheItemPolicy() );

                        return role;
                    }
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

            Guid securityRoleGuid = Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid();

            Rock.Model.GroupService groupService = new Rock.Model.GroupService( new RockContext() );
            foreach ( int id in groupService.Queryable()
                .Where( g => 
                    g.GroupType.Guid.Equals(securityRoleGuid) ||
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
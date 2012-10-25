//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

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

            ObjectCache cache = MemoryCache.Default;
            Role role = cache[cacheKey] as Role;

            if ( role != null )
                return role;
            else
            {
                Rock.Groups.GroupService groupService = new Rock.Groups.GroupService();
                Rock.Groups.Group groupModel = groupService.Get( id );

                if ( groupModel != null && groupModel.IsSecurityRole == true )
                {
                    role = new Role();
                    role.Id = groupModel.Id;
                    role.Name = groupModel.Name;
                    role.Users = new List<string>();

                    foreach ( Rock.Groups.GroupMember member in groupModel.Members )
                    {
                        role.Users.Add( member.Person.Guid.ToString() );
                    }

                    cache.Set( cacheKey, role, new CacheItemPolicy() );

                    return role;
                }
                else
                    return null;

            }
        }

        /// <summary>
        /// Returns a list of all the possible Roles
        /// </summary>
        /// <returns></returns>
        public static List<Role> AllRoles()
        {
            List<Role> roles = new List<Role>();

            Rock.Groups.GroupService groupService = new Rock.Groups.GroupService();
            foreach ( int id in groupService.Queryable()
                .Where( g => g.IsSecurityRole == true )
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
            ObjectCache cache = MemoryCache.Default;
            cache.Remove( Role.CacheKey( id ) );
        }

        #endregion
    }
}
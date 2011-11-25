using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Caching;

namespace Rock.Cms.Security
{
    /// <summary>
    /// Information about a Role that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
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
        /// Gets the GUID.
        /// </summary>
        public Guid Guid { get; private set; }

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
        public bool UserInRole( string user )
        {
            return Users.Contains( user );
        }

        #region Static Methods

        private static string CacheKey( Guid guid )
        {
            return string.Format( "Rock:Role:{0}", guid );
        }

        /// <summary>
        /// Returns Role object from cache.  If role does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static Role Read( string roleGuid )
        {
            Guid guid;
            if ( !Guid.TryParse( roleGuid, out guid ) )
                throw new ArgumentOutOfRangeException( "roleGuid", "Not a valid Guid" );

            string cacheKey = Role.CacheKey( guid );

            ObjectCache cache = MemoryCache.Default;
            Role role = cache[cacheKey] as Role;

            if ( role != null )
                return role;
            else
            {
                Rock.Services.Groups.GroupService groupService = new Rock.Services.Groups.GroupService();
                Rock.Models.Groups.Group groupModel = groupService.
                    Queryable().
                    Where( g => g.Guid == guid && g.IsSecurityRole == true).FirstOrDefault();
                if ( groupModel != null )
                {
                    role = new Role();
                    role.Id = groupModel.Id;
                    role.Guid = groupModel.Guid;
                    role.Name = groupModel.Name;
                    role.Users = new List<string>();

                    foreach ( Rock.Models.Groups.Member member in groupModel.Members )
                        foreach ( Rock.Models.Cms.User userModel in member.Person.Users )
                            role.Users.Add( userModel.Username );

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

            Rock.Services.Groups.GroupService groupService = new Rock.Services.Groups.GroupService();
            foreach(Guid guid in groupService.
                Queryable().Where( g => g.IsSecurityRole == true).Select( g => g.Guid).ToList())
            {
                roles.Add( Role.Read( guid.ToString() ) );
            }

            return roles;
        }

        /// <summary>
        /// Removes role from cache
        /// </summary>
        /// <param name="guid"></param>
        public static void Flush( Guid guid )
        {
            ObjectCache cache = MemoryCache.Default;
            cache.Remove( Role.CacheKey( guid ) );
        }

        #endregion
    }
}
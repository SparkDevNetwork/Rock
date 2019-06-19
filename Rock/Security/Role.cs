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
using System.Collections.Concurrent;
using System.Collections.Generic;

using Rock.Web.Cache;

namespace Rock.Security
{
    /// <summary>
    /// Information about a Role that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [RockObsolete( "1.8" )]
    [Obsolete( "Use RoleCache Instead")]
    public class Role
    {
        /// <summary>
        /// Use Static Read() method to instantiate a new Role object
        /// </summary>
        private Role() { }

        private Role( RoleCache role )
        {
            Id = role.Id;
            Name = role.Name;
            IsSecurityTypeGroup = role.IsSecurityTypeGroup;
            People = new ConcurrentDictionary<Guid, bool>( role.People );
        }

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
            if (!personGuid.HasValue) return false;

            var inRole = false;
            return People.TryGetValue( personGuid.Value, out inRole ) && inRole;
        }

        /// <summary>
        /// Copies from new cache.
        /// </summary>
        /// <param name="cacheEntity">The cache entity.</param>
        protected void CopyFromNewCache( IEntityCache cacheEntity )
        {
          }

        #region Static Methods

        /// <summary>
        /// Performs an implicit conversion from <see cref="Role"/> to <see cref="RoleCache"/>.
        /// </summary>
        /// <param name="c">The c.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator RoleCache( Role c )
        {
            return RoleCache.Get( c.Id );
        }

        /// <summary>
        /// Returns Role object from cache.  If role does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static Role Read( int id )
        {
            return new Role( RoleCache.Get( id ) );
        }

        /// <summary>
        /// Gets the or add existing.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="valueFactory">The value factory.</param>
        /// <returns></returns>
        public static Role GetOrAddExisting( string key, Func<Role> valueFactory )
        {
            // This mehod should not have been public, but it was, so leaving it with just a get.
            return new Role( RoleCache.Get( key.AsInteger() ) );
        }

        /// <summary>
        /// Loads the by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public static Role LoadById( int id )
        {
            return new Role( RoleCache.LoadById( id ) );
        }

        /// <summary>
        /// Returns a list of all the possible Roles
        /// </summary>
        /// <returns></returns>
        public static List<Role> AllRoles()
        {
            var roles = new List<Role>();

            var cacheRoles = RoleCache.AllRoles();
            if ( cacheRoles == null ) return roles;

            foreach ( var cacheRole in cacheRoles )
            {
                roles.Add( new Role( cacheRole ) );
            }

            return roles;
        }

        /// <summary>
        /// Removes role from cache
        /// </summary>
        /// <param name="id">The id.</param>
        public static void Flush( int id )
        {
            RoleCache.Remove( id );
        }

        #endregion
    }
}
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
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a Role that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [DataContract]
    public class RoleCache : ItemCache<RoleCache>
    {
        #region Constructors

        /// <summary>
        /// Use Static Get() method to instantiate a new Global Attributes object
        /// </summary>
        private RoleCache()
        {
        }

        #endregion

        /// <summary>
        /// Gets the id.
        /// </summary>
		[DataMember]
        public int Id { get; private set; }

        /// <summary>
        /// Gets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        [DataMember]
        public Guid Guid { get; private set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is security type group.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is security type group; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsSecurityTypeGroup { get; private set; }

        /// <summary>
        /// Gets the people.
        /// </summary>
        /// <value>
        /// The people.
        /// </value>
        [DataMember]
        public ConcurrentDictionary<Guid, bool> People { get; private set; } = new ConcurrentDictionary<Guid, bool>();

        /// <summary>
        /// Is user in role
        /// </summary>
        /// <param name="personGuid">The person unique identifier.</param>
        /// <returns></returns>
        public bool IsPersonInRole( Guid? personGuid )
        {
            if ( !personGuid.HasValue )
            {
                return false;
            }

            bool inRole;
            return People.TryGetValue( personGuid.Value, out inRole ) && inRole;
        }

        #region Static Methods


        /// <summary>
        /// Returns Role object from cache.  If role does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static RoleCache Get( int id )
        {
            return GetOrAddExisting( id, () => LoadById( id ) );
        }

        /// <summary>
        /// Returns Role object from cache.  If role does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="guid">The guid.</param>
        /// <returns></returns>
        public static RoleCache Get( Guid guid )
        {
            // See if the Id is stored in CacheIdFromGuid.
            int? idFromGuid = IdFromGuidCache.GetId<RoleCache>( guid );
            RoleCache roleCache;
            if ( idFromGuid.HasValue )
            {
                roleCache = Get( idFromGuid.Value );
                return roleCache;
            }

            // If not, query the database for it, and then add to cache (if found).
            var id = new Rock.Model.GroupService( new RockContext() ).GetId( guid );
            if ( id == null )
            {
                return null;
            }
            
            roleCache = Get( id.Value );
            if ( roleCache != null )
            {
                IdFromGuidCache.UpdateCacheId<RoleCache>( guid, roleCache.Id );
                UpdateCacheItem( roleCache.Id.ToString(), roleCache );
            }

            return roleCache;
        }

        /// <summary>
        /// Loads the by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public static RoleCache LoadById( int id )
        {
            using ( var rockContext = new RockContext() )
            {
                var securityGroupType = GroupTypeCache.Get( SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid(), rockContext );
                var securityGroupTypeId = securityGroupType?.Id ?? 0;

                var groupService = new Model.GroupService( rockContext );
                var groupMemberService = new Model.GroupMemberService( rockContext );
                var groupModel = groupService.Get( id );

                if ( groupModel == null || !groupModel.IsActive ||
                    ( !groupModel.IsSecurityRole && groupModel.GroupTypeId != securityGroupTypeId ) )
                {
                    return null;
                }

                var role = new RoleCache
                {
                    Id = groupModel.Id,
                    Guid = groupModel.Guid,
                    Name = groupModel.Name,
                    People = new ConcurrentDictionary<Guid, bool>()
                };

                var groupMembersQry = groupMemberService.Queryable().Where( a => a.GroupId == groupModel.Id );

                // Add the members
                foreach ( var personGuid in groupMembersQry
                    .Where( m => m.GroupMemberStatus == Model.GroupMemberStatus.Active )
                    .Select( m => m.Person.Guid )
                    .ToList()
                    .Distinct() )
                {
                    role.People.TryAdd( personGuid, true );
                }

                role.IsSecurityTypeGroup = groupModel.IsSecurityRoleOrSecurityGroupType();

                return role;
            }
        }

        /// <summary>
        /// Returns a list of all the possible Roles
        /// </summary>
        /// <returns></returns>
        public static List<RoleCache> AllRoles()
        {
            var roles = new List<RoleCache>();

            var securityGroupType = GroupTypeCache.Get( SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid() );
            var securityGroupTypeId = securityGroupType?.Id ?? 0;

            var groupService = new Model.GroupService( new RockContext() );
            foreach ( var id in groupService.Queryable()
                .Where( g =>
                    g.IsActive &&
                    ( g.GroupTypeId == securityGroupTypeId || g.IsSecurityRole ) )
                .OrderBy( g => g.Name )
                .Select( g => g.Id )
                .ToList() )
            {
                roles.Add( Get( id ) );
            }

            return roles;
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }
        #endregion
    }
}
//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Auth POCO Service class
    /// </summary>
    public partial class AuthService 
    {
        /// <summary>
        /// Gets Auths by Entity Type And Entity Id
        /// </summary>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <param name="entityId">Entity Id.</param>
        /// <returns>
        /// An enumerable list of Auth objects.
        /// </returns>
        public IEnumerable<Auth> Get( int entityTypeId, int? entityId )
        {
            return Repository
                .Find( t => 
                    t.EntityTypeId == entityTypeId && 
                    ( t.EntityId == entityId || ( entityId == null && t.EntityId == null ) ) 
                )
                .OrderBy( t => t.Order );
        }
        
        /// <summary>
        /// Gets Auths by Group Id
        /// </summary>
        /// <param name="groupId">Group Id.</param>
        /// <returns>An enumerable list of Auth objects.</returns>
        public IEnumerable<Auth> GetByGroupId( int? groupId )
        {
            return Repository.Find( t => ( t.GroupId == groupId || ( groupId == null && t.GroupId == null ) ) ).OrderBy( t => t.Order );
        }
        
        /// <summary>
        /// Gets Auths by Person Id
        /// </summary>
        /// <param name="personId">Person Id.</param>
        /// <returns>An enumerable list of Auth objects.</returns>
        public IEnumerable<Auth> GetByPersonId( int? personId )
        {
            return Repository.Find( t => ( t.PersonId == personId || ( personId == null && t.PersonId == null ) ) ).OrderBy( t => t.Order );
        }

        /// <summary>
        /// Gets the authorizations for the entity and action.
        /// </summary>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public IQueryable<Auth> GetAuths( int entityTypeId, int? entityId, string action )
        {
            return Queryable().
                    Where( A => A.EntityTypeId == entityTypeId &&
                        A.EntityId == entityId &&
                        A.Action == action ).
                    OrderBy( A => A.Order );
        }
    }
}

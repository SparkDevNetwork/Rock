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
    /// Data access/service class for <see cref="Rock.Model.Auth"/> entity type objects.
    /// </summary>
    public partial class AuthService 
    {
        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.Auth"/> entities by <see cref="Rock.Model.EntityType"/> and entity Id.
        /// </summary>
        /// <param name="entityTypeId">A <see cref="System.Int32"/> representing the EntityId of the <see cref="Rock.Model.EntityType" /> that this Auth entity applies to. </param>
        /// <param name="entityId">A <see cref="System.Int32"/> represent the EntityId of the entity that is being secured.</param>
        /// <returns>
        /// An enumerable list of <see cref="Rock.Model.Auth" /> entities that secure a specific entity.
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
        /// Returns an enumerable collection of <see cref="Rock.Model.Auth"/> entities by <see cref="Rock.Model.Group"/>.
        /// </summary>
        /// <param name="groupId">A <see cref="System.Int32"/> representing the GroupId of the Security Role <see cref="Rock.Model.Group"/> to search by.</param>
        /// <returns>
        /// An enumerable collection of <see cref="Rock.Model.Auth"/> entities that apply to the specified <see cref="Rock.Model.Group"/>.
        /// </returns>
        public IEnumerable<Auth> GetByGroupId( int? groupId )
        {
            return Repository.Find( t => ( t.GroupId == groupId || ( groupId == null && t.GroupId == null ) ) ).OrderBy( t => t.Order );
        }
        
        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.Auth"/> entities by <see cref="Rock.Model.Person"/>.
        /// </summary>
        /// <param name="personId">A <see cref="System.Int32"/> representing the PersonId of the <see cref="Rock.Model.Person"/> to search by </param>
        /// <returns>
        /// An enumerable collection of <see cref="Rock.Model.Auth"/> entities that apply to the specified <see cref="Rock.Model.Person"/>.
        /// </returns>
        public IEnumerable<Auth> GetByPersonId( int? personId )
        {
            return Repository.Find( t => ( t.PersonId == personId || ( personId == null && t.PersonId == null ) ) ).OrderBy( t => t.Order );
        }

        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Model.Auth"/> entities (Authorizations) by entity and action.
        /// </summary>
        /// <param name="entityTypeId">A <see cref="System.Int32"/> representing the EntityTypeId of the <see cref="Rock.Model.EntityType"/> to search by.</param>
        /// <param name="entityId">A <see cref="System.Int32"/> representing the EntityId of the entity to search by.</param>
        /// <param name="action">A <see cref="System.String"/> representing the name of the action to search by.</param>
        /// <returns>A queryable collection of <see cref="Rock.Model.Auth"/> entities (Authorizations) for the specified entity and action.</returns>
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

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
using System.Linq;

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
        public IQueryable<Auth> Get( int entityTypeId, int? entityId )
        {
            return Queryable( "PersonAlias" )
                .Where( t =>
                    t.EntityTypeId == entityTypeId &&
                    ( t.EntityId == entityId || ( entityId == null && t.EntityId == null ) )
                )
                .OrderBy( t => t.Order ).ThenBy( t => t.Id );
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
            return Get( entityTypeId, entityId )
                .Where( t => t.Action == action );
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.Auth"/> entities by <see cref="Rock.Model.EntityType"/> and entity Id.
        /// </summary>
        /// <param name="groupId">A <see cref="System.Int32"/> represent the groupId of the security Role.</param>
        /// <returns>
        /// An enumerable list of <see cref="Rock.Model.Auth" /> entities that secure a specific entity.
        /// </returns>
        public IQueryable<Auth> GetByGroup( int groupId )
        {
            return Queryable()
                .Where( t => t.GroupId == groupId )
                .OrderBy( t => t.Order );
        }
    }
}

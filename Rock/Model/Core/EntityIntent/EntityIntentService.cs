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
using System.Collections.Generic;
using System.Linq;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class EntityIntentService
    {
        #region Public Methods

        /// <summary>
        /// Gets the entity intents for the specified entity type and entity identifier.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="entityId">The entity identifier.</param>
        /// <returns>The entity intents for the specified entity type and entity identifier.</returns>
        public IQueryable<EntityIntent> GetForEntity<TEntity>( int entityId ) where TEntity : IEntity
        {
            var entityTypeCache = GetEntityTypeCache<TEntity>();
            if ( entityTypeCache == null )
            {
                return Enumerable.Empty<EntityIntent>().AsQueryable();
            }

            return Queryable()
                .Where( ei =>
                    ei.EntityTypeId == entityTypeCache.Id
                    && ei.EntityId == entityId
                );
        }

        /// <summary>
        /// Gets the distinct interaction intent defined value ids for the specified entity type and entity identifier.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="entityId">The entity identifier.</param>
        /// <returns>The distinct interaction intent defined value ids for the specified entity type and entity identifier.</returns>
        public List<int> GetIntentValueIds<TEntity>( int entityId ) where TEntity : IEntity
        {
            return GetForEntity<TEntity>( entityId )
                .Select( ei => ei.IntentValueId )
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Sets the intents for the specified entity type and entity identifier.
        /// </summary>
        /// <remarks>
        /// <see cref="EntityIntent"/> records will be added and deleted as needed to synchronize the entity's intents with the provided <paramref name="intentValueIds"/>.
        /// <para>Changes will not be saved until you call <see cref="DbContext.SaveChanges()"/>.</para>
        /// </remarks>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="intentValueIds">The interaction intent defined value identifiers of the intents to set for this entity.</param>
        public void SetIntents<TEntity>( int entityId, List<int> intentValueIds ) where TEntity : IEntity
        {
            if ( entityId <= 0 )
            {
                return;
            }

            var entityTypeCache = GetEntityTypeCache<TEntity>();
            if ( entityTypeCache == null )
            {
                return;
            }

            // Get the entity's existing intents to determine if we need to delete any old ones or add any new ones.
            var existingIntents = GetForEntity<TEntity>( entityId ).ToList();

            // Failsafe in case null was provided.
            intentValueIds = intentValueIds ?? new List<int>();

            // Delete any existing intents not represented in the provided list of intent value ids.
            var intentsToDelete = existingIntents.Where( ei => !intentValueIds.Contains( ei.IntentValueId ) ).ToList();
            foreach ( var intent in intentsToDelete )
            {
                Delete( intent );
            }

            // Add any missing intents.
            var newIntentValueIds = intentValueIds.Where( id => !existingIntents.Any( ei => ei.IntentValueId == id ) ).ToList();
            foreach ( var intentValueId in newIntentValueIds )
            {
                Add( new EntityIntent
                {
                    EntityTypeId = entityTypeCache.Id,
                    EntityId = entityId,
                    IntentValueId = intentValueId
                } );
            }
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Gets the entity type cache for the specified entity type.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <returns>The entity type cache for the specified entity type.</returns>
        private EntityTypeCache GetEntityTypeCache<TEntity>()
        {
            var rockContext = this.Context as RockContext;
            return EntityTypeCache.Get( typeof( TEntity ), createIfNotFound: false, rockContext );
        }

        #endregion Private Methods
    }
}

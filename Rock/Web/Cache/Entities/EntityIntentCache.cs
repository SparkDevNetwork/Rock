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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache.Entities
{
    /// <summary>
    /// Information about an entity intent that is cached by Rock.
    /// </summary>
    [Serializable]
    [DataContract]
    public class EntityIntentCache : ModelCache<EntityIntentCache, EntityIntent>
    {
        #region Properties

        /// <summary>
        /// Gets or sets the entity type identifier.
        /// </summary>
        /// <value>
        /// The entity type identifier.
        /// </value>
        [DataMember]
        public int EntityTypeId { get; private set; }

        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        /// <value>
        /// The entity identifier.
        /// </value>
        [DataMember]
        public int EntityId { get; private set; }

        /// <summary>
        /// Gets or sets the interaction intent defined value identifier.
        /// </summary>
        /// <value>
        /// The interaction intent defined value identifier
        /// </value>
        [DataMember]
        public int IntentValueId { get; private set; }

        /// <summary>
        /// Gets the entity type cache.
        /// </summary>
        public EntityTypeCache EntityType => EntityTypeCache.Get( this.EntityTypeId );

        /// <summary>
        /// Gets the interaction intent defined value cache.
        /// </summary>
        public DefinedValueCache IntentValue => DefinedValueCache.Get( this.IntentValueId );

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var entityIntent = entity as EntityIntent;
            if ( entityIntent == null )
                return;

            EntityTypeId = entityIntent.EntityTypeId;
            EntityId = entityIntent.EntityId;
            IntentValueId = entityIntent.IntentValueId;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.IntentValue?.ToString();
        }

        #endregion Public Methods

        #region Static Methods

        /// <summary>
        /// Gets the entity intents for the specified entity type and entity identifier.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="entityId">The entity identifier.</param>
        /// <returns>The entity intents for the specified entity type and entity identifier.</returns>
        public static List<EntityIntentCache> GetForEntity<TEntity>( int entityId ) where TEntity : IEntity
        {
            var entityTypeCache = EntityTypeCache.Get( typeof( TEntity ), createIfNotFound: false );
            if ( entityTypeCache == null )
            {
                return new List<EntityIntentCache>();
            }

            return All()
                .Where( ei =>
                    ei.EntityTypeId == entityTypeCache.Id
                    && ei.EntityId == entityId
                )
                .ToList();
        }

        /// <summary>
        /// Gets the distinct interaction intent defined value ids for the specified entity type and entity identifier.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="entityId">The entity identifier.</param>
        /// <returns>The distinct interaction intent defined value ids for the specified entity type and entity identifier.</returns>
        public static List<int> GetIntentValueIds<TEntity>( int entityId ) where TEntity : IEntity
        {
            return GetForEntity<TEntity>( entityId )
                .Select( ei => ei.IntentValueId )
                .Distinct()
                .ToList();
        }

        #endregion Static Methods
    }
}

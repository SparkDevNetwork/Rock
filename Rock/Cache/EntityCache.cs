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
using System.Data.Entity;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;
using DbContext = Rock.Data.DbContext;

namespace Rock.Cache
{
    /// <summary>
    /// An abstract class for entities that need to be cached.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TT">The type of the t.</typeparam>
    /// <seealso cref="Rock.Cache.ItemCache{T}" />
    /// <seealso cref="Rock.Cache.IEntityCache" />
    [Serializable]
    [DataContract]
    public abstract class EntityCache<T, TT> : ItemCache<T>, IEntityCache
        where T : IEntityCache, new()
        where TT : Entity<TT>, new()
    {

        #region Properties

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [DataMember]
        public virtual int Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        [DataMember]
        public virtual Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the foreign identifier.
        /// </summary>
        /// <value>
        /// The foreign identifier.
        /// </value>
        [DataMember]
        public int? ForeignId { get; set; }

        /// <summary>
        /// Gets or sets the foreign unique identifier.
        /// </summary>
        /// <value>
        /// The foreign unique identifier.
        /// </value>
        [DataMember]
        public Guid? ForeignGuid { get; set; }

        /// <summary>
        /// Gets or sets the foreign key.
        /// </summary>
        /// <value>
        /// The foreign key.
        /// </value>
        [DataMember]
        public string ForeignKey { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Set's the cached objects properties from the model/entities properties.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public virtual void SetFromEntity( IEntity entity )
        {
            Id = entity.Id;
            Guid = entity.Guid;
            ForeignId = entity.ForeignId;
            ForeignGuid = entity.ForeignGuid;
            ForeignKey = entity.ForeignKey;
        }

        /// <summary>
        /// Gets the cached object by id.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public static T Get( int id )
        {
            return Get( id, null );
        }

        /// <summary>
        /// Gets the cached object by id using the included RockContext if needed.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static T Get( int id, RockContext rockContext )
        {
            return id == 0 ? default(T) : GetOrAddExisting( id, () => QueryDb( id, rockContext ) );
        }

        /// <summary>
        /// Gets a cached item using a guid string
        /// </summary>
        /// <param name="guidString">The unique identifier string.</param>
        /// <returns></returns>
        public static T Get( string guidString )
        {
            var guid = guidString.AsGuidOrNull();
            return guid.HasValue ? Get( guid.Value ) : default( T );
        }

        /// <summary>
        /// Gets the cached object by guid.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <returns></returns>
        public static T Get( Guid guid )
        {
            return Get( guid, null );
        }

        /// <summary>
        /// Gets the cached object by guid using the included RockContext if needed.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static T Get( Guid guid, RockContext rockContext )
        {
            // First check to see if there's an item in the All list 
            var cachedEntity = All().FirstOrDefault( i => i.Guid.Equals( guid ) );
            if ( cachedEntity != null ) return cachedEntity;

            // If not, query the database for it, and then add to cache (if found)
            cachedEntity = QueryDb( guid, rockContext );
            if ( cachedEntity != null )
            {
                UpdateCacheItem( cachedEntity.Id.ToString(), cachedEntity, TimeSpan.MaxValue );
            }

            return cachedEntity;
        }

        /// <summary>
        /// Gets a cached object by using a model/entity
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public static T Get( TT entity )
        {
            if ( entity == null ) return default( T );

            var value = new T();
            value.SetFromEntity( entity );
            UpdateCacheItem( entity.Id.ToString(), value, TimeSpan.MaxValue );

            return value;

        }

        /// <summary>
        /// Gets all the instances of this type of model/entity that are currently in cache.
        /// </summary>
        /// <returns></returns>
        public static List<T> All()
        {
            return All(null);
        }

        /// <summary>
        /// Gets all the instances of this type of model/entity that are currently in cache.
        /// </summary>
        /// <returns></returns>
        public static List<T> All( RockContext rockContext )
        {
            var cachedKeys = GetOrAddKeys(() => QueryDbForAllIds(rockContext));
            if ( cachedKeys == null) return new List<T>();

            var allValues = new List<T>();
            foreach ( var key in cachedKeys )
            {
                var value = Get( key.AsInteger() );
                if ( value != null )
                {
                    allValues.Add( value );
                }
            }

            return allValues;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Recreates the list of all cached keys for this entity type by querying the database.
        /// </summary>
        /// <returns></returns>
        protected static void LoadAll()
        {
            LoadAll( null );
        }

        /// <summary>
        /// Recreates the list of all cached keys for this entity type by querying the database.
        /// </summary>
        /// <returns></returns>
        protected static void LoadAll( RockContext rockContext )
        {
            AddKeys( () => QueryDbForAllIds( rockContext ) );
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Queries the database by id.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private static T QueryDb( int id, DbContext rockContext )
        {
            if ( rockContext != null )
            {
                return QueryDbWithContext( id, rockContext );
            }

            using ( var newRockContext = new RockContext() )
            {
                return QueryDbWithContext( id, newRockContext );
            }
        }

        /// <summary>
        /// Queries the database by id with context.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private static T QueryDbWithContext( int id, DbContext rockContext )
        {
            var service = new Service<TT>( rockContext );
            var entity = service.Get( id );

            if ( entity == null ) return default( T );

            var value = new T();
            value.SetFromEntity( entity );
            return value;

        }

        /// <summary>
        /// Queries the database by guid.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private static T QueryDb( Guid guid, DbContext rockContext )
        {
            if ( rockContext != null )
            {
                return QueryDbWithContext( guid, rockContext );
            }

            using ( var newRockContext = new RockContext() )
            {
                return QueryDbWithContext( guid, newRockContext );
            }
        }

        /// <summary>
        /// Queries the database by guid with context.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private static T QueryDbWithContext( Guid guid, DbContext rockContext )
        {
            var service = new Service<TT>( rockContext );
            var entity = service.Get( guid );

            if ( entity == null ) return default( T );

            var value = new T();
            value.SetFromEntity( entity );
            return value;

        }

        /// <summary>
        /// Queries the database for all ids.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private static List<string> QueryDbForAllIds( DbContext rockContext )
        {
            if ( rockContext != null )
            {
                return QueryDbForAllIdsWithContext( rockContext );
            }

            using ( var newRockContext = new RockContext() )
            {
                return QueryDbForAllIdsWithContext( newRockContext );
            }
        }

        /// <summary>
        /// Queries the database for all ids with context.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private static List<string> QueryDbForAllIdsWithContext( DbContext rockContext )
        {
            var service = new Service<TT>( rockContext );
            return service.Queryable().AsNoTracking()
                .Select( i => i.Id )
                .ToList()
                .ConvertAll( i => i.ToString() );
        }

        #endregion

    }
}

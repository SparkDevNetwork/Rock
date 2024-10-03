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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;

using DbContext = Rock.Data.DbContext;

namespace Rock.Web.Cache
{
    /// <summary>
    /// An abstract class for entities that need to be cached.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TT">The type of the t.</typeparam>
    /// <seealso cref="ItemCache{T}" />
    /// <seealso cref="IEntityCache" />
    [Serializable]
    [DataContract]
    public abstract class EntityCache<T, TT> : EntityItemCache<T>, IEntityCache, IHasLifespan
        where T : IEntityCache, new()
        where TT : Entity<TT>, new()
    {
        #region Properties

        /// <summary>
        /// The EntityType of the cached entity
        /// </summary>
        /// <value>
        /// The entity type identifier.
        /// </value>
        public int CachedEntityTypeId
        {
            get
            {
                return EntityTypeCache.Get<TT>().Id;
            }
        }

        /// <inheritdoc cref="IEntity.IdKey"/>
        [DataMember]
        public virtual string IdKey { get; protected set; }

        #region Lifespan

        /// <summary>
        /// The amount of time that this item will live in the cache before expiring. If null, then the
        /// default lifespan is used.
        /// </summary>
        public virtual TimeSpan? Lifespan => null;

        #endregion Lifespan

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the cached object's properties from the model/entity's properties.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public virtual void SetFromEntity( IEntity entity )
        {
            Id = entity.Id;
            Guid = entity.Guid;
            ForeignId = entity.ForeignId;
            ForeignGuid = entity.ForeignGuid;
            ForeignKey = entity.ForeignKey;
            IdKey = entity.IdKey;
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
            if ( id == 0 )
            {
                return default( T );
            }

            return GetOrAddExisting( id, () =>
            {
                var result = QueryDb( id, rockContext );
                if ( result != null )
                {
                    IdFromGuidCache.UpdateCacheId<T>( result.Guid, id );
                }

                return result;
            } );
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
        /// Gets a cached item using a generic key.
        /// </summary>
        /// <remarks>
        /// The key is a string representation of either an integer identifier,
        /// a unique identifier, or a hashed identifier.
        /// </remarks>
        /// <param name="key">The key to be parsed and used to load the cached entity.</param>
        /// <param name="allowIntegerIdentifier">if set to <c>true</c> integer identifiers will be allowed; otherwise <c>null</c> will be returned if an integer identifier is provided.</param>
        /// <returns>The cached <typeparamref name="T"/> or <c>null</c> if not found in cache or the database.</returns>
        [RockInternal( "1.16.7" )]
        public static T Get( string key, bool allowIntegerIdentifier )
        {
            int? id = allowIntegerIdentifier ? key.AsIntegerOrNull() : null;

            if ( !id.HasValue )
            {
                var guid = key.AsGuidOrNull();

                if ( guid.HasValue )
                {
                    return Get( guid.Value );
                }

                id = Rock.Utility.IdHasher.Instance.GetId( key );
            }

            return id.HasValue ? Get( id.Value ) : default;
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
        /// Gets a cached item using an IdKey.
        /// </summary>
        /// <param name="idKey">The IdKey.</param>
        /// <returns>T.</returns>
        public static T GetByIdKey( string idKey )
        {
            var idFromIdKey = Rock.Utility.IdHasher.Instance.GetId( idKey );
            if ( !idFromIdKey.HasValue )
            {
                return default( T );
            }

            return Get( idFromIdKey.Value );
        }

        /// <summary>
        /// Gets a cached item using an IdKey.
        /// </summary>
        /// <param name="idKey">The IdKey.</param>
        /// <param name="rockContext">The context to use when database access is required.</param>
        /// <returns>T.</returns>
        public static T GetByIdKey( string idKey, RockContext rockContext )
        {
            var idFromIdKey = Rock.Utility.IdHasher.Instance.GetId( idKey );
            if ( !idFromIdKey.HasValue )
            {
                return default( T );
            }

            return Get( idFromIdKey.Value, rockContext );
        }

        /// <summary>
        /// Gets the Id for the cache object, or NULL if it doesn't exist
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <returns></returns>
        public static int? GetId( Guid guid )
        {
            return Get( guid, null )?.Id;
        }

        /// <summary>
        /// Gets the Guid for the cache object, or NULL if it doesn't exist
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        internal static Guid? GetGuid( int id )
        {
            return Get( id, null )?.Guid;
        }

        /// <summary>
        /// Gets the cached object by guid using the included RockContext if needed.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static T Get( Guid guid, RockContext rockContext )
        {
            // see if the Id is stored in CacheIdFromGuid
            int? idFromGuid = IdFromGuidCache.GetId<T>( guid );
            T cachedEntity;
            if ( idFromGuid.HasValue )
            {
                cachedEntity = Get( idFromGuid.Value, rockContext );
                return cachedEntity;
            }

            // If not, query the database for it, and then add to cache (if found)
            cachedEntity = QueryDb( guid, rockContext );
            if ( cachedEntity != null )
            {
                IdFromGuidCache.UpdateCacheId<T>( guid, cachedEntity.Id );
                UpdateCacheItem( cachedEntity.Id.ToString(), cachedEntity );
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
            if ( entity == null )
                return default( T );

            var value = new T();
            value.SetFromEntity( entity );

            // The entity Id is 0 if the entity is yet to be saved to the database. We want to avoid adding such entities to the cache.
            if ( entity.Id > 0 )
            {
                IdFromGuidCache.UpdateCacheId<T>( entity.Guid, entity.Id );
                UpdateCacheItem( entity.Id.ToString(), value );
            }

            return value;
        }

        /// <summary>
        /// Attempts to get an item from the cache without adding it if it does
        /// not already exist.
        /// </summary>
        /// <param name="id">The identifier of the cached item to be loaded.</param>
        /// <param name="item">On return will contain the item.</param>
        /// <returns><c>true</c> if the item was found in cache, <c>false</c> otherwise.</returns>
        public static bool TryGet( int id, out T item )
        {
            return ItemCache<T>.TryGet( id.ToString(), out item );
        }

        /// <summary>
        /// Attempts to get an item from the cache without adding it if it does
        /// not already exist.
        /// </summary>
        /// <param name="guid">The unique identifier of the cached item to be loaded.</param>
        /// <param name="item">On return will contain the item.</param>
        /// <returns><c>true</c> if the item was found in cache, <c>false</c> otherwise.</returns>
        public static bool TryGet( Guid guid, out T item )
        {
            if ( !IdFromGuidCache.TryGetId<T>( guid.ToString(), out var id ) )
            {
                item = default;
                return false;
            }

            return ItemCache<T>.TryGet( id.ToString(), out item );
        }

        /// <summary>
        /// Get the specified entity, or throw an Exception if it does not exist.
        /// </summary>
        /// <param name="entityDescription"></param>
        /// <param name="id"></param>
        /// <param name="rockContext"></param>
        /// <returns></returns>
        public static T GetOrThrow( string entityDescription, int id, RockContext rockContext = null )
        {
            try
            {
                return Get( id, rockContext );
            }
            catch
            {
                throw new Exception( $"System configuration error. Entity not found [Type=\"{typeof( T ).Name}\",Name=\"{entityDescription}\", Id=\"{id}\"]." );
            }
        }

        /// <summary>
        /// Get the specified entity, or throw an Exception if it does not exist.
        /// </summary>
        /// <param name="entityDescription"></param>
        /// <param name="guid"></param>
        /// <param name="rockContext"></param>
        /// <returns></returns>
        public static T GetOrThrow( string entityDescription, Guid guid, RockContext rockContext = null )
        {
            try
            {
                return Get( guid, rockContext );
            }
            catch
            {
                throw new Exception( $"System configuration error. Entity not found [Type=\"{typeof( T ).Name}\",Name=\"{entityDescription}\", Guid=\"{guid}\"]." );
            }
        }

        /// <summary>
        /// Gets all the cache objects for the specified identifiers.
        /// </summary>
        /// <param name="ids">The identifiers of the cache objects to retrieve.</param>
        /// <param name="rockContext">The rock context to use if database access is needed.</param>
        /// <returns>An enumeration of the cached objects.</returns>
        internal static IEnumerable<T> GetMany( ICollection<int> ids, RockContext rockContext = null )
        {
            if ( ids == null )
            {
                return new List<T>();
            }

            var cachedItems = new List<T>( ids.Count );
            var idsToLoad = new List<int>( ids.Count );

            // Try to get items that already exist in cache.
            foreach ( var id in ids )
            {
                if ( TryGet( id, out var cachedItem ) )
                {
                    cachedItems.Add( cachedItem );
                }
                else
                {
                    idsToLoad.Add( id );
                }
            }

            if ( !idsToLoad.Any() )
            {
                return cachedItems;
            }

            // Get any remaining items that still need to be loaded from the database.
            bool disposeOfContext = false;

            if ( rockContext == null )
            {
                rockContext = new RockContext();
                disposeOfContext = true;
            }

            var service = new Service<TT>( rockContext );

            while ( idsToLoad.Any() )
            {
                var idsBatch = idsToLoad.Take( 1000 ).ToList();
                idsToLoad = idsToLoad.Skip( 1000 ).ToList();

                var itemsQry = GetQueryableForBulkLoad( rockContext )
                    .AsNoTracking()
                    .Where( a => idsBatch.Contains( a.Id ) );

                var items = itemsQry.ToList();

                // Pre-load all the attributes.
                if ( typeof( IHasAttributes ).IsAssignableFrom( typeof( TT ) ) && typeof( T ) != typeof( AttributeCache ) )
                {
                    items.Cast<IHasAttributes>().LoadAttributes( rockContext );
                }

                cachedItems.AddRange( items.Select( a => Get( ( TT ) a ) ) );
            }

            if ( disposeOfContext )
            {
                rockContext.Dispose();
            }

            return cachedItems;
        }

        /// <summary>
        /// Gets all the cache objects for the specified identifiers.
        /// </summary>
        /// <param name="guids">The unique identifiers of the cache objects to retrieve.</param>
        /// <param name="rockContext">The rock context to use if database access is needed.</param>
        /// <returns>An enumeration of the cached objects.</returns>
        internal static IEnumerable<T> GetMany( ICollection<Guid> guids, RockContext rockContext = null )
        {
            if ( guids == null )
            {
                return new List<T>();
            }

            var cachedItems = new List<T>( guids.Count );
            var guidsToLoad = new List<Guid>();

            // Try to get items that already exist in cache.
            foreach ( var guid in guids )
            {
                if ( TryGet( guid, out var cachedItem ) )
                {
                    cachedItems.Add( cachedItem );
                }
                else
                {
                    guidsToLoad.Add( guid );
                }
            }

            if ( !guidsToLoad.Any() )
            {
                return cachedItems;
            }

            // Get any remaining items that still need to be loaded from the database.
            bool disposeOfContext = false;

            if ( rockContext == null )
            {
                rockContext = new RockContext();
                disposeOfContext = true;
            }

            var service = new Service<TT>( rockContext );

            while ( guidsToLoad.Any() )
            {
                var guidsBatch = guidsToLoad.Take( 1000 ).ToList();
                guidsToLoad = guidsToLoad.Skip( 1000 ).ToList();

                var itemsQry = GetQueryableForBulkLoad( rockContext )
                    .AsNoTracking()
                    .Where( a => guidsBatch.Contains( a.Guid ) );

                var items = itemsQry.ToList();

                // Pre-load all the attributes.
                if ( typeof( IHasAttributes ).IsAssignableFrom( typeof( TT ) ) && typeof( T ) != typeof( AttributeCache ) )
                {
                    items.Cast<IHasAttributes>().LoadAttributes( rockContext );
                }

                cachedItems.AddRange( items.Select( a => Get( ( TT ) a ) ) );
            }

            if ( disposeOfContext )
            {
                rockContext.Dispose();
            }

            return cachedItems;
        }

        /// <summary>
        /// Gets the queryable for bulk loading of entities to be cached.
        /// The queryable returned will have any required navigation properties
        /// already included.
        /// </summary>
        /// <param name="rockContext">The rock context to use when creating the service.</param>
        /// <returns>A queryable for the entity.</returns>
        private static IQueryable<IEntity> GetQueryableForBulkLoad( RockContext rockContext )
        {
            // In the future this should be some sort of lookup.
            if ( typeof( TT ) == typeof( Rock.Model.Attribute ) )
            {
                return new AttributeService( rockContext ).Queryable()
                    .Include( a => a.Categories )
                    .Include( a => a.AttributeQualifiers );
            }

            return new Service<TT>( rockContext ).Queryable();
        }

        /// <summary>
        /// Removes or invalidates the CachedItem based on EntityState
        /// </summary>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="entityState">State of the entity. If unknown, use <see cref="EntityState.Detached" /></param>
        public static void UpdateCachedEntity( int entityId, EntityState entityState )
        {
            // NOTE: Don't read the Item into the Cache here since it could be part of a transaction that could be rolled back.
            // Reading it from the database here could also cause a deadlock depending on the database isolation level.
            // Just remove it from Cache, and update the AllIds based on entityState

            if ( entityState == EntityState.Deleted )
            {
                Remove( entityId );
            }
            else if ( entityState == EntityState.Added )
            {
                // add this entity to All Ids, but don't fetch it into cache until somebody asks for it
                AddToAllIds( entityId );
            }
            else
            {
                FlushItem( entityId );
            }
        }

        /// <summary>
        /// If not already populated, recreates the list of keys for every entity using the keyFactory.
        /// Then returns the list of all items of this type.
        /// NOTE: This will contain all the items of this type that are in the database.
        /// </summary>
        /// <returns></returns>
        public static List<T> All()
        {
            return All( null );
        }

        /// <summary>
        /// If not already populated, recreates the list of keys for every entity using the keyFactory.
        /// Then returns the list of all items of this type.
        /// NOTE: This will contain all the items of this type that are in the database,
        /// </summary>
        /// <returns></returns>
        public static List<T> All( RockContext rockContext )
        {
            var cachedKeys = GetOrAddKeys( () => QueryDbForAllIds( rockContext ) );
            if ( cachedKeys == null )
            {
                return new List<T>();
            }

            return GetMany( cachedKeys.ToList().AsIntegerList(), rockContext ).ToList();
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

            if ( entity == null )
                return default( T );

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
            if ( guid.IsEmpty() )
            {
                return default( T );
            }

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

            if ( entity == null )
                return default( T );

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

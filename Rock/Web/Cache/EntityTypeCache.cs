//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Runtime.Caching;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a entityType that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    public class EntityTypeCache : Rock.Model.EntityTypeDto
    {
        private EntityTypeCache() : base() { }
        private EntityTypeCache( Rock.Model.EntityType model ) : base( model ) { }

        private static Dictionary<string, int> entityTypes = new Dictionary<string, int>();

        #region Static Methods

        private static string CacheKey( int id )
        {
            return string.Format( "Rock:EntityType:{0}", id );
        }

        /// <summary>
        /// Reads the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static EntityTypeCache Read( string name )
        {
            if ( entityTypes.ContainsKey( name ) )
                return Read( entityTypes[name] );

            var entityTypeService = new Rock.Model.EntityTypeService();
            var entityTypeModel = entityTypeService.Get( name, true, null );
            return Read( entityTypeModel );
        }

        /// <summary>
        /// Gets the id.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static int? GetId( string name )
        {
            if (String.IsNullOrEmpty(name))
                return null;

            return Read( name ).Id;
        }

        /// <summary>
        /// Returns EntityType object from cache.  If entityType does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static EntityTypeCache Read( int id )
        {
            string cacheKey = EntityTypeCache.CacheKey( id );

            ObjectCache cache = MemoryCache.Default;
            EntityTypeCache entityType = cache[cacheKey] as EntityTypeCache;

            if ( entityType != null )
                return entityType;
            else
            {
                Rock.Model.EntityTypeService entityTypeService = new Rock.Model.EntityTypeService();
                Rock.Model.EntityType entityTypeModel = entityTypeService.Get( id );
                if ( entityTypeModel != null )
                {
                    entityType = CopyModel( entityTypeModel );

                    cache.Set( cacheKey, entityType, new CacheItemPolicy() );

                    return entityType;
                }
                else
                    return null;
            }
        }

        /// <summary>
        /// Reads the specified field type model.
        /// </summary>
        /// <param name="entityTypeModel">The field type model.</param>
        /// <returns></returns>
        public static EntityTypeCache Read( Rock.Model.EntityType entityTypeModel )
        {
            string cacheKey = EntityTypeCache.CacheKey( entityTypeModel.Id );

            ObjectCache cache = MemoryCache.Default;
            EntityTypeCache entityType = cache[cacheKey] as EntityTypeCache;

            if ( entityType != null )
                return entityType;
            else
            {
                entityType = EntityTypeCache.CopyModel( entityTypeModel );
                cache.Set( cacheKey, entityType, new CacheItemPolicy() );

                return entityType;
            }
        }

        /// <summary>
        /// Copies the model.
        /// </summary>
        /// <param name="entityTypeModel">The field type model.</param>
        /// <returns></returns>
        public static EntityTypeCache CopyModel( Rock.Model.EntityType entityTypeModel )
        {
            EntityTypeCache entityType = new EntityTypeCache( entityTypeModel );

            // update static dictionary object with name/id combination
            if ( entityTypes.ContainsKey( entityType.Name ) )
            {
                entityTypes[entityType.Name] = entityType.Id;
            }
            else
            {
                entityTypes.Add( entityType.Name, entityType.Id );
            }

            return entityType;
        }

        /// <summary>
        /// Removes entityType from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            ObjectCache cache = MemoryCache.Default;
            cache.Remove( EntityTypeCache.CacheKey( id ) );
        }

        #endregion
    }
}
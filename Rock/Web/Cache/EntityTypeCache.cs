//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Runtime.Caching;

using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a entityType that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    public class EntityTypeCache
    {
        #region Static Fields

        // Locking object
        private static readonly Object obj = new object();

        private static Dictionary<string, int> entityTypes = new Dictionary<string, int>();

        #endregion

        #region Constructors

        private EntityTypeCache()
        {
        }

        private EntityTypeCache( EntityType model )
        {
            CopyFromModel( model );
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>
        /// The id.
        /// </value>
        public virtual int Id { get; set; }

        /// <summary>
        /// Gets or sets the GUID.
        /// </summary>
        /// <value>
        /// The GUID.
        /// </value>
        public virtual Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the assembly.
        /// </summary>
        /// <value>
        /// The name of the assembly.
        /// </value>
        public string AssemblyName { get; set; }

        /// <summary>
        /// Gets or sets the name of the friendly.
        /// </summary>
        /// <value>
        /// The name of the friendly.
        /// </value>
        public string FriendlyName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is entity.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is entity; otherwise, <c>false</c>.
        /// </value>
        public bool IsEntity { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is secured.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is secured; otherwise, <c>false</c>.
        /// </value>
        public bool IsSecured { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        public void CopyFromModel( EntityType entityType )
        {
            this.Id = entityType.Id;
            this.Guid = entityType.Guid;
            this.Name = entityType.Name;
            this.AssemblyName = entityType.AssemblyName;
            this.FriendlyName = entityType.FriendlyName;
            this.IsEntity = entityType.IsEntity;
            this.IsSecured = entityType.IsSecured;

            lock ( obj )
            {
                // update static dictionary object with name/id combination
                if ( entityTypes.ContainsKey( entityType.Name ) )
                {
                    entityTypes[entityType.Name] = entityType.Id;
                }
                else
                {
                    entityTypes.Add( entityType.Name, entityType.Id );
                }
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion

        #region Static Methods

        private static string CacheKey( int id )
        {
            return string.Format( "Rock:EntityType:{0}", id );
        }

        /// <summary>
        /// Gets the id.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static int? GetId( Type type )
        {
            return Read( type ).Id;
        }

        /// <summary>
        /// Gets the id.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static int? GetId( string name )
        {
            if ( String.IsNullOrEmpty( name ) )
            {
                return null;
            }

            return Read( name ).Id;
        }

        /// <summary>
        /// Reads the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static EntityTypeCache Read( Type type )
        {
            int? entityTypeId = null;

            lock ( obj )
            {
                if ( entityTypes.ContainsKey( type.FullName ) )
                {
                    entityTypeId = entityTypes[type.FullName];
                }
            }

            if ( entityTypeId.HasValue )
            {
                return Read( entityTypeId.Value );
            }

            var entityTypeService = new EntityTypeService();
            var entityTypeModel = entityTypeService.Get( type, true, null );
            return Read( entityTypeModel );
        }

        /// <summary>
        /// Returns EntityType object from cache.  If entityBlockType does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static EntityTypeCache Read( string name )
        {
            int? entityTypeId = null;

            lock ( obj )
            {
                if ( entityTypes.ContainsKey( name ) )
                {
                    entityTypeId = entityTypes[name];
                }
            }

            if ( entityTypeId.HasValue )
            {
                return Read( entityTypeId.Value );
            }

            var entityTypeService = new EntityTypeService();
            var entityTypeModel = entityTypeService.Get( name, true, null );
            return Read( entityTypeModel );
        }

        /// <summary>
        /// Returns EntityType object from cache.  If entityBlockType does not already exist in cache, it
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
            {
                return entityType;
            }
            else
            {
                var entityTypeService = new EntityTypeService();
                var entityTypeModel = entityTypeService.Get( id );
                if ( entityTypeModel != null )
                {
                    entityType = new EntityTypeCache( entityTypeModel );

                    var cachePolicy = new CacheItemPolicy();
                    cache.Set( cacheKey, entityType, cachePolicy );
                    cache.Set( entityType.Guid.ToString(), entityType.Id, cachePolicy );

                    return entityType;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        public static EntityTypeCache Read( Guid guid )
        {
            ObjectCache cache = MemoryCache.Default;
            object cacheObj = cache[guid.ToString()];

            if ( cacheObj != null )
            {
                return Read( (int)cacheObj );
            }
            else
            {
                var entityTypeService = new EntityTypeService();
                var entityTypeModel = entityTypeService.Get( guid );
                if ( entityTypeModel != null )
                {
                    var entityType = new EntityTypeCache( entityTypeModel );

                    var cachePolicy = new CacheItemPolicy();
                    cache.Set( EntityTypeCache.CacheKey( entityType.Id ), entityType, cachePolicy );
                    cache.Set( entityType.Guid.ToString(), entityType.Id, cachePolicy );

                    return entityType;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Reads the specified field type model.
        /// </summary>
        /// <param name="entityTypeModel">The field type model.</param>
        /// <returns></returns>
        public static EntityTypeCache Read( EntityType entityTypeModel )
        {
            string cacheKey = EntityTypeCache.CacheKey( entityTypeModel.Id );

            ObjectCache cache = MemoryCache.Default;
            EntityTypeCache entityType = cache[cacheKey] as EntityTypeCache;

            if ( entityType != null )
            {
                return entityType;
            }
            else
            {
                entityType = new EntityTypeCache( entityTypeModel );

                var cachePolicy = new CacheItemPolicy();
                cache.Set( cacheKey, entityType, cachePolicy );
                cache.Set( entityType.Guid.ToString(), entityType.Id, cachePolicy );

                return entityType;
            }
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
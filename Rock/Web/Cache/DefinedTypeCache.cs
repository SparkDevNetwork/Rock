//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Runtime.Caching;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a definedType that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    public class DefinedTypeCache : Rock.Core.DefinedTypeDto
    {
        private DefinedTypeCache() : base() { }
		private DefinedTypeCache( Rock.Core.DefinedType model ) : base( model ) { }

        /// <summary>
        /// Gets the type of the field.
        /// </summary>
        /// <value>
        /// The type of the field.
        /// </value>
        public FieldTypeCache FieldType
        {
            get 
            { 
                if (FieldTypeId.HasValue)
                    return FieldTypeCache.Read( FieldTypeId.Value );
                return null;
            }
        }

        #region Static Methods

        private static string CacheKey( int id )
        {
            return string.Format( "Rock:DefinedType:{0}", id );
        }

        /// <summary>
        /// Returns DefinedType object from cache.  If definedType does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static DefinedTypeCache Read( int id )
        {
            string cacheKey = DefinedTypeCache.CacheKey( id );

            ObjectCache cache = MemoryCache.Default;
            DefinedTypeCache definedType = cache[cacheKey] as DefinedTypeCache;

            if ( definedType != null )
                return definedType;
            else
            {
                Rock.Core.DefinedTypeService definedTypeService = new Rock.Core.DefinedTypeService();
                Rock.Core.DefinedType definedTypeModel = definedTypeService.Get( id );
                if ( definedTypeModel != null )
                {
                    definedType = CopyModel( definedTypeModel );

                    cache.Set( cacheKey, definedType, new CacheItemPolicy() );

                    return definedType;
                }
                else
                    return null;
            }
        }

        /// <summary>
        /// Reads the specified defined type model.
        /// </summary>
        /// <param name="definedTypeModel">The defined type model.</param>
        /// <returns></returns>
        public static DefinedTypeCache Read( Rock.Core.DefinedType definedTypeModel )
        {
            string cacheKey = DefinedTypeCache.CacheKey( definedTypeModel.Id );

            ObjectCache cache = MemoryCache.Default;
            DefinedTypeCache definedType = cache[cacheKey] as DefinedTypeCache;

			if ( definedType != null )
				return definedType;
			else
			{
				definedType = DefinedTypeCache.CopyModel( definedTypeModel );
				cache.Set( cacheKey, definedType, new CacheItemPolicy() );

				return definedType;
			}
        }

        /// <summary>
        /// Copies the model.
        /// </summary>
        /// <param name="definedTypeModel">The defined type model.</param>
        /// <returns></returns>
        public static DefinedTypeCache CopyModel( Rock.Core.DefinedType definedTypeModel )
        {
            DefinedTypeCache definedType = new DefinedTypeCache(definedTypeModel);
            return definedType;
        }

        /// <summary>
        /// Removes definedType from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            ObjectCache cache = MemoryCache.Default;
            cache.Remove( DefinedTypeCache.CacheKey( id ) );
        }

        #endregion
    }
}
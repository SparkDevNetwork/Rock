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
    /// Information about a fieldType that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    public class FieldTypeCache : Rock.Core.FieldTypeDto
    {
        private FieldTypeCache() : base() { }
		private FieldTypeCache( Rock.Core.FieldType model ) : base( model ) { }

        /// <summary>
        /// Gets the field 
        /// </summary>
        public Rock.Field.IFieldType Field { get; private set; }

        #region Static Methods

        private static string CacheKey( int id )
        {
            return string.Format( "Rock:FieldType:{0}", id );
        }

        /// <summary>
        /// Returns FieldType object from cache.  If fieldType does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static FieldTypeCache Read( int id )
        {
            string cacheKey = FieldTypeCache.CacheKey( id );

            ObjectCache cache = MemoryCache.Default;
            FieldTypeCache fieldType = cache[cacheKey] as FieldTypeCache;

            if ( fieldType != null )
                return fieldType;
            else
            {
                Rock.Core.FieldTypeService fieldTypeService = new Rock.Core.FieldTypeService();
                Rock.Core.FieldType fieldTypeModel = fieldTypeService.Get( id );
                if ( fieldTypeModel != null )
                {
                    fieldType = CopyModel( fieldTypeModel );

                    cache.Set( cacheKey, fieldType, new CacheItemPolicy() );

                    return fieldType;
                }
                else
                    return null;
            }
        }

        /// <summary>
        /// Reads the specified field type model.
        /// </summary>
        /// <param name="fieldTypeModel">The field type model.</param>
        /// <returns></returns>
        public static FieldTypeCache Read( Rock.Core.FieldType fieldTypeModel )
        {
            string cacheKey = FieldTypeCache.CacheKey( fieldTypeModel.Id );

            ObjectCache cache = MemoryCache.Default;
            FieldTypeCache fieldType = cache[cacheKey] as FieldTypeCache;

			if ( fieldType != null )
				return fieldType;
			else
			{
				fieldType = FieldTypeCache.CopyModel( fieldTypeModel );
				cache.Set( cacheKey, fieldType, new CacheItemPolicy() );

				return fieldType;
			}
        }

        /// <summary>
        /// Copies the model.
        /// </summary>
        /// <param name="fieldTypeModel">The field type model.</param>
        /// <returns></returns>
        public static FieldTypeCache CopyModel( Rock.Core.FieldType fieldTypeModel )
        {
			FieldTypeCache fieldType = new FieldTypeCache( fieldTypeModel );
			fieldType.Field = Rock.Field.Helper.InstantiateFieldType( fieldType.Assembly, fieldType.Class );

            return fieldType;
        }

        /// <summary>
        /// Removes fieldType from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            ObjectCache cache = MemoryCache.Default;
            cache.Remove( FieldTypeCache.CacheKey( id ) );
        }

        #endregion
    }
}
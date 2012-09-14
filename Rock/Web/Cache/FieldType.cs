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
    public class FieldType : Rock.Core.FieldTypeDto
    {
        private FieldType() : base() { }
		private FieldType( Rock.Core.FieldType model ) : base( model ) { }

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
        public static FieldType Read( int id )
        {
            string cacheKey = FieldType.CacheKey( id );

            ObjectCache cache = MemoryCache.Default;
            FieldType fieldType = cache[cacheKey] as FieldType;

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
        public static FieldType Read( Rock.Core.FieldType fieldTypeModel )
        {
            string cacheKey = FieldType.CacheKey( fieldTypeModel.Id );

            ObjectCache cache = MemoryCache.Default;
            FieldType fieldType = cache[cacheKey] as FieldType;

			if ( fieldType != null )
				return fieldType;
			else
			{
				fieldType = FieldType.CopyModel( fieldTypeModel );
				cache.Set( cacheKey, fieldType, new CacheItemPolicy() );

				return fieldType;
			}
        }

        /// <summary>
        /// Copies the model.
        /// </summary>
        /// <param name="fieldTypeModel">The field type model.</param>
        /// <returns></returns>
        public static FieldType CopyModel( Rock.Core.FieldType fieldTypeModel )
        {
			FieldType fieldType = new FieldType( fieldTypeModel );
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
            cache.Remove( FieldType.CacheKey( id ) );
        }

        #endregion
    }
}
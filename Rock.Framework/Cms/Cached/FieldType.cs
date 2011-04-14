using System;
using System.Collections.Generic;
using System.Runtime.Caching;

namespace Rock.Cms.Cached
{
    /// <summary>
    /// Information about a fieldType that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    public class FieldType
    {
        /// <summary>
        /// Use Static Read() method to instantiate a new FieldType object
        /// </summary>
        private FieldType() { }

        public int Id { get; private set; }
        public string Path { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public Rock.FieldTypes.IFieldType Field { get; private set; }

        #region Static Methods

        private static string CacheKey( int id )
        {
            return string.Format( "Rock:FieldType:{0}", id );
        }

        /// <summary>
        /// Returns FieldType object from cache.  If fieldType does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="guid"></param>
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
                Rock.Services.Core.FieldTypeService fieldTypeService = new Services.Core.FieldTypeService();
                Rock.Models.Core.FieldType fieldTypeModel = fieldTypeService.GetFieldType( id );
                if ( fieldTypeModel != null )
                {
                    fieldType = new FieldType();
                    fieldType.Id = fieldTypeModel.Id;
                    fieldType.Name = fieldTypeModel.Name;
                    fieldType.Description = fieldTypeModel.Description;
                    fieldType.Field = Rock.FieldTypes.FieldHelper.InstantiateFieldType( fieldTypeModel.Assembly, fieldTypeModel.Class );

                    cache.Set( cacheKey, fieldType, new CacheItemPolicy() );

                    return fieldType;
                }
                else
                    return null;
            }
        }

        /// <summary>
        /// Removes fieldType from cache
        /// </summary>
        /// <param name="guid"></param>
        public static void Flush( int id )
        {
            ObjectCache cache = MemoryCache.Default;
            cache.Remove( FieldType.CacheKey( id ) );
        }

        #endregion
    }
}
//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.Runtime.Caching;

namespace Rock.Web.Cache
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

        /// <summary>
        /// Gets the id.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Gets the path.
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the description.
        /// </summary>
        public string Description { get; private set; }

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
                    fieldType = new FieldType();
                    fieldType.Id = fieldTypeModel.Id;
                    fieldType.Name = fieldTypeModel.Name;
                    fieldType.Description = fieldTypeModel.Description;
                    fieldType.Field = Rock.Field.Helper.InstantiateFieldType( fieldTypeModel.Assembly, fieldTypeModel.Class );

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
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            ObjectCache cache = MemoryCache.Default;
            cache.Remove( FieldType.CacheKey( id ) );
        }

        #endregion
    }
}
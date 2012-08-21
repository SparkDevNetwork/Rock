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
    public class DefinedType
    {
        /// <summary>
        /// Use Static Read() method to instantiate a new DefinedType object
        /// </summary>
        private DefinedType() { }

        /// <summary>
        /// Gets the id.
        /// </summary>
        public int Id { get; private set; }

        private int? FieldTypeId { get; set; }

        /// <summary>
        /// Gets the type of the field.
        /// </summary>
        /// <value>
        /// The type of the field.
        /// </value>
        public FieldType FieldType
        {
            get 
            { 
                if (FieldTypeId.HasValue)
                    return FieldType.Read( FieldTypeId.Value );
                return null;
            }
        }

        /// <summary>
        /// Gets the order.
        /// </summary>
        public int Order { get; private set; }

        /// <summary>
        /// Gets the category.
        /// </summary>
        public string Category { get; private set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the description.
        /// </summary>
        public string Description { get; private set; }

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
        public static DefinedType Read( int id )
        {
            string cacheKey = DefinedType.CacheKey( id );

            ObjectCache cache = MemoryCache.Default;
            DefinedType definedType = cache[cacheKey] as DefinedType;

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
        public static DefinedType Read( Rock.Core.DefinedType definedTypeModel )
        {
            DefinedType definedType = DefinedType.CopyModel( definedTypeModel );

            string cacheKey = DefinedType.CacheKey( definedTypeModel.Id );
            ObjectCache cache = MemoryCache.Default;
            cache.Set( cacheKey, definedType, new CacheItemPolicy() );

            return definedType;
        }

        /// <summary>
        /// Copies the model.
        /// </summary>
        /// <param name="definedTypeModel">The defined type model.</param>
        /// <returns></returns>
        public static DefinedType CopyModel( Rock.Core.DefinedType definedTypeModel )
        {
            DefinedType definedType = new DefinedType();
            definedType.Id = definedTypeModel.Id;
            definedType.Order = definedTypeModel.Order;
            definedType.Category = definedTypeModel.Category;
            definedType.Name = definedTypeModel.Name;
            definedType.Description = definedTypeModel.Description;
            definedType.FieldTypeId = definedTypeModel.FieldTypeId;

            return definedType;
        }

        /// <summary>
        /// Removes definedType from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            ObjectCache cache = MemoryCache.Default;
            cache.Remove( DefinedType.CacheKey( id ) );
        }

        #endregion
    }
}
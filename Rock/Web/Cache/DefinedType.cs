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
    public class DefinedType : Rock.Core.DefinedTypeDto
    {
        private DefinedType() : base() { }
		private DefinedType( Rock.Core.DefinedType model ) : base( model ) { }

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
            string cacheKey = DefinedType.CacheKey( definedTypeModel.Id );

            ObjectCache cache = MemoryCache.Default;
            DefinedType definedType = cache[cacheKey] as DefinedType;

			if ( definedType != null )
				return definedType;
			else
			{
				definedType = DefinedType.CopyModel( definedTypeModel );
				cache.Set( cacheKey, definedType, new CacheItemPolicy() );

				return definedType;
			}
        }

        /// <summary>
        /// Copies the model.
        /// </summary>
        /// <param name="definedTypeModel">The defined type model.</param>
        /// <returns></returns>
        public static DefinedType CopyModel( Rock.Core.DefinedType definedTypeModel )
        {
            DefinedType definedType = new DefinedType(definedTypeModel);
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
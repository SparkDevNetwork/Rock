//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Web.UI;

using Rock.Field;

namespace Rock.Web.Cache
{
    /// <summary>
	/// Information about an defined type that is required by the rendering engine.
    /// This information will be cached by Rock. 
    /// 
	/// NOTE: Because this defined type object is cached and shared by all entities 
	/// using the defined type, a particlar instance's values are not included as a 
	/// property of this defined type object.
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

        /// <summary>
        /// Gets the FieldTypeId.
        /// </summary>
        public int? FieldTypeId { get; private set; }
		
        /// <summary>
        /// Gets the Order.
        /// </summary>
        public int Order { get; private set; }

        /// <summary>
        /// Gets the category.
        /// </summary>
        public string Category 
        { 
            get
            {
                return string.IsNullOrEmpty( category ) ? "DefinedType" : category;
            }

            private set
            {
				if ( value == "DefinedType" )
                    category = null;
                else
                    category = value;
            }
        }
        private string category;

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
        /// Adds DefinedType model to cache, and returns cached object
        /// </summary>
        /// <param name="DefinedTypeModel">The DefinedTypeModel to cache</param>
        /// <returns></returns>
		public static DefinedType Read( Rock.Core.DefinedType dtModel )
        {
			DefinedType definedType = DefinedType.CopyModel( dtModel );

			string cacheKey = DefinedType.CacheKey( dtModel.Id );
            ObjectCache cache = MemoryCache.Default;
			cache.Set( cacheKey, definedType, new CacheItemPolicy() );

			return definedType;
        }

        /// <summary>
		/// Returns DefinedType object from cache.  If the definedType does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
		/// <param name="id">The id of the DefinedType to read</param>
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
				Rock.Core.DefinedTypeService dtService = new Rock.Core.DefinedTypeService();
				Rock.Core.DefinedType dtModel = dtService.Get( id );
                if ( dtModel != null )
                {
					definedType = DefinedType.CopyModel( dtModel );

					cache.Set( cacheKey, definedType, new CacheItemPolicy() );

					return definedType;
                }
                else
                    return null;

            }
        }

        /// <summary>
		/// Copies the properties of a <see cref="Rock.Core.DefinedType"/> object to a <see cref="DefinedType"/> object/>
        /// </summary>
        /// <param name="dtModel">The definedType model.</param>
        /// <returns></returns>
		public static DefinedType CopyModel( Rock.Core.DefinedType dtModel )
        {
			DefinedType definedType = new DefinedType();
			definedType.Id = dtModel.Id;
			definedType.FieldTypeId = dtModel.FieldTypeId;
			definedType.Order = dtModel.Order;
			definedType.Category = dtModel.Category;
			definedType.Name = dtModel.Name;
			definedType.Description = dtModel.Description;
						
			return definedType;
        }

        /// <summary>
        /// Removes defined type from cache
        /// </summary>
        /// <param name="id">The id of the defined type to remove from cache</param>
        public static void Flush( int id )
        {
            ObjectCache cache = MemoryCache.Default;
			cache.Remove( DefinedType.CacheKey( id ) );
        }

        #endregion
    }
}
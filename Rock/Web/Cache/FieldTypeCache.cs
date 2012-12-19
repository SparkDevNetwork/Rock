//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Runtime.Caching;

using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a fieldType that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    public class FieldTypeCache : CachedModel<FieldType>
    {
        #region Constructors

        private FieldTypeCache()
        {
        }

        private FieldTypeCache( FieldType fieldType )
        {
            CopyFromModel( fieldType );
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this instance is system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is system; otherwise, <c>false</c>.
        /// </value>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the assembly.
        /// </summary>
        /// <value>
        /// The assembly.
        /// </value>
        public string Assembly { get; set; }

        /// <summary>
        /// Gets or sets the class.
        /// </summary>
        /// <value>
        /// The class.
        /// </value>
        public string Class { get; set; }

        /// <summary>
        /// Gets the field 
        /// </summary>
        public Rock.Field.IFieldType Field { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( Data.IEntity model )
        {
            base.CopyFromModel( model );

            if ( model is FieldType )
            {
                var fieldType = (FieldType)model;
                this.IsSystem = fieldType.IsSystem;
                this.Name = fieldType.Name;
                this.Description = fieldType.Description;
                this.Assembly = fieldType.Assembly;
                this.Class = fieldType.Class;

                this.Field = Rock.Field.Helper.InstantiateFieldType( fieldType.Assembly, fieldType.Class );
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
            {
                return fieldType;
            }
            else
            {
                var fieldTypeService = new FieldTypeService();
                var fieldTypeModel = fieldTypeService.Get( id );
                if ( fieldTypeModel != null )
                {
                    fieldTypeModel.LoadAttributes();
                    fieldType = new FieldTypeCache( fieldTypeModel );

                    var cachePolicy = new CacheItemPolicy();
                    cache.Set( cacheKey, fieldType, cachePolicy );
                    cache.Set( fieldType.Guid.ToString(), fieldType.Id, cachePolicy );
                    
                    return fieldType;
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
        public static FieldTypeCache Read( Guid guid )
        {
            ObjectCache cache = MemoryCache.Default;
            object cacheObj = cache[guid.ToString()];

            if ( cacheObj != null )
            {
                return Read( (int)cacheObj );
            }
            else
            {
                var fieldTypeService = new FieldTypeService();
                var fieldTypeModel = fieldTypeService.Get( guid );
                if ( fieldTypeModel != null )
                {
                    fieldTypeModel.LoadAttributes();
                    var fieldType = new FieldTypeCache( fieldTypeModel );

                    var cachePolicy = new CacheItemPolicy();
                    cache.Set( FieldTypeCache.CacheKey( fieldType.Id ), fieldType, cachePolicy );
                    cache.Set( fieldType.Guid.ToString(), fieldType.Id, cachePolicy );

                    return fieldType;
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
        /// <param name="fieldTypeModel">The field type model.</param>
        /// <returns></returns>
        public static FieldTypeCache Read( FieldType fieldTypeModel )
        {
            string cacheKey = FieldTypeCache.CacheKey( fieldTypeModel.Id );

            ObjectCache cache = MemoryCache.Default;
            FieldTypeCache fieldType = cache[cacheKey] as FieldTypeCache;

            if ( fieldType != null )
            {
                return fieldType;
            }
            else
            {
                fieldType = new FieldTypeCache( fieldTypeModel );

                var cachePolicy = new CacheItemPolicy();
                cache.Set( cacheKey, fieldType, cachePolicy );
                cache.Set( fieldType.Guid.ToString(), fieldType.Id, cachePolicy );
                
                return fieldType;
            }
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
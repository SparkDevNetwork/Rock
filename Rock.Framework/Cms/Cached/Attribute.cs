using System;
using System.Collections.Generic;
using System.Runtime.Caching;

namespace Rock.Cms.Cached
{
    /// <summary>
    /// Information about an attribute that is required by the rendering engine.
    /// This information will be cached by Rock
    /// </summary>
    public class Attribute
    {
        /// <summary>
        /// Use Static Read() method to instantiate a new Attribute object
        /// </summary>
        private Attribute() { }

        /// <summary>
        /// Gets the id.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Gets the key.
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the description.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Gets a value indicating if this attribute should be displayed in a column when this attribute's parent object is
        /// listed in a grid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if it should be added as a column; otherwise, <c>false</c>.
        /// </value>
        public bool GridColumn { get; private set; }

        /// <summary>
        /// Gets the default value for the attribute
        /// </summary>
        public string DefaultValue { get; private set; }

        /// <summary>
        /// Gets the qualifier values if any have been defined for the attribute
        /// </summary>
        public Dictionary<string, KeyValuePair<string, string>> QualifierValues { get; private set; }

        private int FieldTypeId { get; set; }

        /// <summary>
        /// Gets the type of the field.
        /// </summary>
        /// <value>
        /// The type of the field.
        /// </value>
        public FieldType FieldType 
        {
            get { return FieldType.Read( FieldTypeId ); }
        }

        /// <summary>
        /// Creates a <see cref="System.Web.UI.Control"/> based on the attribute's field type.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="setValue">if set to <c>true</c> set the control's value</param>
        /// <returns></returns>
        public System.Web.UI.Control CreateControl( string value, bool setValue)
        {
            this.FieldType.Field.QualifierValues = this.QualifierValues;
            return this.FieldType.Field.CreateControl( string.IsNullOrEmpty( value ) ? DefaultValue : value, setValue );
        }

        #region Static Methods

        private static string CacheKey( int id )
        {
            return string.Format( "Rock:Attribute:{0}", id );
        }

        /// <summary>
        /// Adds Attribute model to cache, and returns cached object
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static Attribute Read( Rock.Models.Core.Attribute attributeModel )
        {
            Attribute attribute = Attribute.CopyModel( attributeModel );

            string cacheKey = Attribute.CacheKey( attributeModel.Id );
            ObjectCache cache = MemoryCache.Default;
            cache.Set( cacheKey, attribute, new CacheItemPolicy() );

            return attribute;
        }

        /// <summary>
        /// Returns Attribute object from cache.  If attribute does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static Attribute Read( int id )
        {
            string cacheKey = Attribute.CacheKey( id );

            ObjectCache cache = MemoryCache.Default;
            Attribute attribute = cache[cacheKey] as Attribute;

            if ( attribute != null )
                return attribute;
            else
            {
                Rock.Services.Core.AttributeService attributeService = new Services.Core.AttributeService();
                Rock.Models.Core.Attribute attributeModel = attributeService.Get( id );
                if ( attributeModel != null )
                {
                    attribute = Attribute.CopyModel( attributeModel );

                    cache.Set( cacheKey, attribute, new CacheItemPolicy() );

                    return attribute;
                }
                else
                    return null;

            }
        }

        /// <summary>
        /// Copies the properties of a <see cref="Rock.Models.Core.Attribute"/> object to a <see cref="Attribute"/> object/>
        /// </summary>
        /// <param name="attributeModel">The attribute model.</param>
        /// <returns></returns>
        public static Attribute CopyModel( Rock.Models.Core.Attribute attributeModel )
        {
            Attribute attribute = new Attribute();
            attribute.Id = attributeModel.Id;
            attribute.Key = attributeModel.Key;
            attribute.Name = attributeModel.Name;
            attribute.Description = attributeModel.Description;
            attribute.GridColumn = attributeModel.GridColumn;
            attribute.FieldTypeId = attributeModel.FieldTypeId;
            attribute.DefaultValue = attributeModel.DefaultValue;

            attribute.QualifierValues = new Dictionary<string,KeyValuePair<string, string>>();
            foreach ( Rock.Models.Core.AttributeQualifier qualifier in attributeModel.AttributeQualifiers )
                attribute.QualifierValues.Add( qualifier.Key, new KeyValuePair<string, string>( qualifier.Name, qualifier.Value ) );

            return attribute;
        }

        /// <summary>
        /// Removes attribute from cache
        /// </summary>
        /// <param name="guid"></param>
        public static void Flush( int id )
        {
            ObjectCache cache = MemoryCache.Default;
            cache.Remove( Attribute.CacheKey( id ) );
        }

        #endregion
    }
}
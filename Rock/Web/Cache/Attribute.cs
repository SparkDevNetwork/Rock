//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.Collections.Generic;
using System.Runtime.Caching;
using System.Web.UI;

using Rock.Field;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about an attribute that is required by the rendering engine.
    /// This information will be cached by Rock. 
    /// 
    /// NOTE: Because this attribute object is cached and shared by all entities 
    /// using the attribute, a particlar instance's values are not included as a 
    /// property of this attribute object.
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
        /// Gets the category.
        /// </summary>
        public string Category 
        { 
            get
            {
                return string.IsNullOrEmpty( category ) ? "Attributes" : category;
            }

            private set
            {
                if ( value == "Attributes" )
                    category = null;
                else
                    category = value;
            }
        }
        private string category;

        /// <summary>
        /// Gets if attribute supports multiple values
        /// </summary>
        public bool MultiValue { get; private set; }

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
        public Dictionary<string, ConfigurationValue> QualifierValues { get; private set; }

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
        /// Gets a value indicating whether this <see cref="Attribute"/> is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        public bool Required { get; private set; }

        /// <summary>
        /// Creates a <see cref="System.Web.UI.Control"/> based on the attribute's field type.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="setValue">if set to <c>true</c> set the control's value</param>
        /// <returns></returns>
        public Control CreateControl( string value, bool setValue)
        {
            Control editControl = this.FieldType.Field.EditControl( this.QualifierValues);
            if ( setValue )
                this.FieldType.Field.SetEditValue( editControl, this.QualifierValues, value );
            return editControl;
        }

        #region Static Methods

        private static string CacheKey( int id )
        {
            return string.Format( "Rock:Attribute:{0}", id );
        }

        /// <summary>
        /// Adds Attribute model to cache, and returns cached object
        /// </summary>
        /// <param name="attributeModel">The attributeModel to cache</param>
        /// <returns></returns>
        public static Attribute Read( Rock.Core.Attribute attributeModel )
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
        /// <param name="id">The id of the Attribute to read</param>
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
                Rock.Core.AttributeService attributeService = new Rock.Core.AttributeService();
                Rock.Core.Attribute attributeModel = attributeService.Get( id );
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
        /// Copies the properties of a <see cref="Rock.Core.Attribute"/> object to a <see cref="Attribute"/> object/>
        /// </summary>
        /// <param name="attributeModel">The attribute model.</param>
        /// <returns></returns>
        public static Attribute CopyModel( Rock.Core.Attribute attributeModel )
        {
            Attribute attribute = new Attribute();
            attribute.Id = attributeModel.Id;
            attribute.Key = attributeModel.Key;
            attribute.Name = attributeModel.Name;
            attribute.Category = attributeModel.Category;
            attribute.Description = attributeModel.Description;
            attribute.GridColumn = attributeModel.GridColumn;
            attribute.FieldTypeId = attributeModel.FieldTypeId;
            attribute.DefaultValue = attributeModel.DefaultValue;
            attribute.Required = attributeModel.Required;
            attribute.MultiValue = attributeModel.MultiValue;

            attribute.QualifierValues = new Dictionary<string, ConfigurationValue>();
            foreach ( Rock.Core.AttributeQualifier qualifier in attributeModel.AttributeQualifiers )
                attribute.QualifierValues.Add( qualifier.Key, new ConfigurationValue( qualifier.Value ) );

            return attribute;
        }

        /// <summary>
        /// Removes attribute from cache
        /// </summary>
        /// <param name="id">The id of the attribute to remove from cache</param>
        public static void Flush( int id )
        {
            ObjectCache cache = MemoryCache.Default;
            cache.Remove( Attribute.CacheKey( id ) );
        }

        #endregion
    }
}
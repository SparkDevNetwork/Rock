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
    /// Information about an attribute that is required by the rendering engine.
    /// This information will be cached by Rock. 
    /// 
    /// NOTE: Because this attribute object is cached and shared by all entities 
    /// using the attribute, a particlar instance's values are not included as a 
    /// property of this attribute object.
    /// </summary>
    [Serializable]
    public class Attribute : Rock.Core.AttributeDto
    {
		private Attribute() : base() { }
		private Attribute( Rock.Core.Attribute model ) : base( model ) { }

        /// <summary>
        /// Gets the category.
        /// </summary>
		//public override string Category 
		//{ 
		//	get
		//	{
		//		return string.IsNullOrEmpty( base.Category ) ? "Attributes" : base.Category;
		//	}

		//	private set
		//	{
		//		if ( value == "Attributes" )
		//			base.Category = null;
		//		else
		//			base.Category = value;
		//	}
		//}

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
		/// Gets the qualifier values if any have been defined for the attribute
		/// </summary>
		public Dictionary<string, ConfigurationValue> QualifierValues { get; private set; }

        /// <summary>
        /// Creates a <see cref="System.Web.UI.Control"/> based on the attribute's field type.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="setValue">if set to <c>true</c> set the control's value</param>
        /// <returns></returns>
        public Control CreateControl( string value, bool setValue)
        {
            Control editControl = this.FieldType.Field.EditControl( QualifierValues);
            if ( setValue )
                this.FieldType.Field.SetEditValue( editControl, QualifierValues, value );
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
			string cacheKey = Attribute.CacheKey( attributeModel.Id );

			ObjectCache cache = MemoryCache.Default;
			Attribute attribute = cache[cacheKey] as Attribute;

			if ( attribute != null )
				return attribute;
			else
			{
				attribute = Attribute.CopyModel( attributeModel );
				cache.Set( cacheKey, attribute, new CacheItemPolicy() );

				return attribute;
			}
        }

        /// <summary>
        /// Adds Attribute model to cache, and returns cached object.  
        /// </summary>
        /// <param name="attributeModel">The attribute model.</param>
        /// <param name="qualifiers">The qualifiers.</param>
        /// <returns></returns>
        public static Attribute Read( Rock.Core.Attribute attributeModel, Dictionary<string, string> qualifiers )
        {
            Attribute attribute = Attribute.CopyModel( attributeModel, qualifiers );

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
            var qualifiers = new Dictionary<string, string>();

            foreach ( Rock.Core.AttributeQualifier qualifier in attributeModel.AttributeQualifiers )
                qualifiers.Add( qualifier.Key, qualifier.Value );

            return CopyModel( attributeModel, qualifiers );
        }

        private static Attribute CopyModel( Rock.Core.Attribute attributeModel, Dictionary<string, string> qualifiers )
        {
            var attribute = new Attribute(attributeModel);

            attribute.QualifierValues = new Dictionary<string, ConfigurationValue>();
            foreach ( var qualifier in qualifiers )
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
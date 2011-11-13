using System;
using System.Collections.Generic;
using System.Runtime.Caching;

namespace Rock.Cms.Cached
{
    /// <summary>
    /// Information about a attribute that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    public class Attribute
    {
        /// <summary>
        /// Use Static Read() method to instantiate a new Attribute object
        /// </summary>
        private Attribute() { }

        public int Id { get; private set; }
        public string Key { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public bool GridColumn { get; private set; }
        public string DefaultValue { get; private set; }
        public Dictionary<string, KeyValuePair<string, string>> QualifierValues { get; private set; }

        private int FieldTypeId { get; set; }
        public FieldType FieldType 
        {
            get { return FieldType.Read( FieldTypeId ); }
        }

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
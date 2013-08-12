//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Global Attributes
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    public class GlobalAttributesCache
    {
        #region Constructors

        /// <summary>
        /// Use Static Read() method to instantiate a new Global Attributes object
        /// </summary>
        private GlobalAttributesCache() { }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the attribute keys.  Used to iterate all values when merging possible merge fields
        /// </summary>
        /// <value>
        /// The attribute keys.
        /// </value>
        public Dictionary<int, string> AttributeKeys { get; set; }

        /// <summary>
        /// Gets or sets the attribute values.
        /// </summary>
        /// <value>
        /// The attribute values.
        /// </value>
        public Dictionary<string, KeyValuePair<string, string>> AttributeValues { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the Global Attribute values for the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public string GetValue( string key )
        {
            if ( AttributeValues.Keys.Contains( key ) )
            {
                return AttributeValues[key].Value;
            }
            else
            {
                string name = key.SplitCase();
                string value = string.Empty;

                var attribute = new AttributeService().Get( null, string.Empty, string.Empty, key );
                if ( attribute != null )
                {
                    AttributeKeys.Add( attribute.Id, attribute.Key );
                    name = attribute.Name;

                    // TODO: Need to add support for multiple values
                    var attributeValue = new AttributeValueService().GetByAttributeIdAndEntityId( attribute.Id, null ).FirstOrDefault();
                    value = ( attributeValue != null && !string.IsNullOrEmpty( attributeValue.Value ) ) ? attributeValue.Value : attribute.DefaultValue;
                }

                // Add value even if attribute name was not found to prevent repeatedly searching db for non-existent attribute
                AttributeValues.Add( key, new KeyValuePair<string, string>( name, value ) );
                return value;
            }
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="currentPersonId">The current person id.</param>
        /// <param name="saveValue">if set to <c>true</c> [save value].</param>
        public void SetValue( string key, string value, int? currentPersonId, bool saveValue )
        {
            if ( saveValue )
            {
                using (new Rock.Data.UnitOfWorkScope())
                {
                    // Save new value
                    var attributeValueService = new AttributeValueService();
                    var attributeValue = attributeValueService.GetGlobalAttributeValue( key );

                    if ( attributeValue == null )
                    {
                        var attributeService = new AttributeService();
                        var attribute = attributeService.GetGlobalAttribute( key );
                        if ( attribute == null )
                        {
                            attribute = new Rock.Model.Attribute();
                            attribute.FieldTypeId = FieldTypeCache.Read(new Guid(SystemGuid.FieldType.TEXT)).Id;
                            attribute.EntityTypeQualifierColumn = string.Empty;
                            attribute.EntityTypeQualifierValue = string.Empty;
                            attribute.Key = key;
                            attribute.Name = key.SplitCase();
                            attributeService.Add(attribute, currentPersonId);
                            attributeService.Save(attribute, currentPersonId);

                            AttributeKeys.Add( attribute.Id, key );
                        }

                        attributeValue = new AttributeValue();
                        attributeValueService.Add( attributeValue, currentPersonId );
                        attributeValue.IsSystem = false;
                        attributeValue.AttributeId = attribute.Id;

                        if ( !AttributeValues.Keys.Contains( key ) )
                        {
                            AttributeValues.Add( key, new KeyValuePair<string, string>( attribute.Name, value ) );
                        }
                    }

                    attributeValue.Value = value;
                    attributeValueService.Save( attributeValue, currentPersonId );
                }
            }

            // Update cached value
            if ( AttributeValues != null && AttributeValues.Keys.Contains( key ) )
            {
                string attributeName = AttributeValues[key].Key;
                AttributeValues[key] = new KeyValuePair<string, string>( attributeName, value );
            }
            else if ( AttributeValues != null )
            {
                AttributeValues[key] = new KeyValuePair<string, string>( key, value );
            }
        }

        #endregion

        #region Static Methods

        private static string CacheKey()
        {
            return "Rock:GlobalAttributes";
        }

        /// <summary>
        /// Returns Global Attributes from cache.  If they are not already in cache, they
        /// will be read and added to cache
        /// </summary>
        /// <returns></returns>
        public static GlobalAttributesCache Read()
        {
            string cacheKey = GlobalAttributesCache.CacheKey();

            ObjectCache cache = MemoryCache.Default;
            GlobalAttributesCache globalAttributes = cache[cacheKey] as GlobalAttributesCache;

            if ( globalAttributes != null )
            {
                return globalAttributes;
            }
            else
            {
                globalAttributes = new GlobalAttributesCache();
                globalAttributes.AttributeKeys = new Dictionary<int, string>();
                globalAttributes.AttributeValues = new Dictionary<string, KeyValuePair<string, string>>();

                var attributeService = new Rock.Model.AttributeService();
                var attributeValueService = new Rock.Model.AttributeValueService();

                foreach ( Rock.Model.Attribute attribute in attributeService.GetGlobalAttributes() )
                {
                    globalAttributes.AttributeKeys.Add( attribute.Id, attribute.Key );

                    // TODO: Need to add support for multiple values
                    var attributeValue = attributeValueService.GetByAttributeIdAndEntityId( attribute.Id, null ).FirstOrDefault();
                    globalAttributes.AttributeValues.Add( attribute.Key, new KeyValuePair<string, string>( attribute.Name, ( attributeValue != null && !string.IsNullOrEmpty( attributeValue.Value ) ) ? attributeValue.Value : attribute.DefaultValue ) );

                }

                cache.Set( cacheKey, globalAttributes, new CacheItemPolicy() );

                return globalAttributes;
            }
        }

        /// <summary>
        /// Removes Global Attributes from cache
        /// </summary>
        public static void Flush()
        {
            ObjectCache cache = MemoryCache.Default;
            cache.Remove( GlobalAttributesCache.CacheKey() );
        }

        #endregion
    }
}
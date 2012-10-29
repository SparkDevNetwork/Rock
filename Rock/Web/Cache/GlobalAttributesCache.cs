//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Caching;

using Rock.Core;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Global Attributes
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    public class GlobalAttributesCache
    {
        /// <summary>
        /// Use Static Read() method to instantiate a new Global Attributes object
        /// </summary>
        private GlobalAttributesCache() { }

        /// <summary>
        /// Gets or sets the attribute values.
        /// </summary>
        /// <value>
        /// The attribute values.
        /// </value>
        public Dictionary<string, KeyValuePair<string, string>> AttributeValues { get; set; }

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
                return globalAttributes;
            else
            {
                globalAttributes = new GlobalAttributesCache();
                globalAttributes.AttributeValues = new Dictionary<string, KeyValuePair<string, string>>();

                var attributeService = new Rock.Core.AttributeService();
                var attributeValueService = new Rock.Core.AttributeValueService();

                foreach ( var attribute in attributeService.GetGlobalAttributes() )
                {
                    // TODO: Need to add support for multiple values
                    var attributeValue = attributeValueService.GetByAttributeIdAndEntityId( attribute.Id, null ).FirstOrDefault();
                    globalAttributes.AttributeValues.Add( attribute.Key,
                        new KeyValuePair<string, string>(
                            attribute.Name,
                            ( attributeValue != null && !string.IsNullOrEmpty( attributeValue.Value ) ) ? attributeValue.Value : attribute.DefaultValue
                        )
                    );
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

        /// <summary>
        /// Gets the Global Attribute values for the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public string GetValue(string key)
        {
            if (AttributeValues != null && AttributeValues.Keys.Contains( key ) )
                return AttributeValues[key].Value;
            return null;
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
                // Save new value
                var attributeValueService = new AttributeValueService();
                var attributeValue = attributeValueService.GetGlobalAttributeValue(key);

                if ( attributeValue == null )
                {
                    var attributeService = new AttributeService();
                    var attribute = attributeService.Get(
                        string.Empty, string.Empty, string.Empty, key );
                    if ( attribute != null )
                    {
                        attributeValue = new AttributeValue();
                        attributeValue.IsSystem = false;
                        attributeValue.AttributeId = attribute.Id;
                        attributeValue.Value = value;
                        attributeValueService.Save( attributeValue, currentPersonId );
                    }
                }
                else
                {
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
        }

        #endregion
    }
}
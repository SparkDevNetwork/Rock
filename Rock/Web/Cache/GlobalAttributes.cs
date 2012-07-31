//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Caching;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Global Attributes
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    public class GlobalAttributes
    {
        /// <summary>
        /// Use Static Read() method to instantiate a new Global Attributes object
        /// </summary>
        private GlobalAttributes() { }

        /// <summary>
        /// Global Attribute Value for the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public string AttributeValue (string key)
        {
            if (AttributeValues.Keys.Contains(key))
                return AttributeValues[key].Value;
            return null;
        }

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
        public static GlobalAttributes Read()
        {
            string cacheKey = GlobalAttributes.CacheKey();

            ObjectCache cache = MemoryCache.Default;
            GlobalAttributes globalAttributes = cache[cacheKey] as GlobalAttributes;

            if ( globalAttributes != null )
                return globalAttributes;
            else
            {
                globalAttributes = new GlobalAttributes();
                globalAttributes.AttributeValues = new Dictionary<string, KeyValuePair<string, string>>();

                var attributeService = new Rock.Core.AttributeService();
                var attributeValueService = new Rock.Core.AttributeValueService();

                foreach ( Rock.Core.Attribute attribute in attributeService.Queryable().
                    Where( a => a.Entity == "" &&
                        ( a.EntityQualifierColumn ?? string.Empty ) == "" &&
                        ( a.EntityQualifierValue ?? string.Empty ) == "" ) )
                {
                    // TODO: Need to add support for multiple values
                    var attributeValue = attributeValueService.GetByAttributeIdAndEntityId( attribute.Id, null ).FirstOrDefault();
                    globalAttributes.AttributeValues.Add( attribute.Key, new KeyValuePair<string, string>( attribute.Name, (attributeValue != null && !string.IsNullOrEmpty(attributeValue.Value)) ? attributeValue.Value : attribute.DefaultValue ) );
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
            cache.Remove( GlobalAttributes.CacheKey() );
        }

        /// <summary>
        /// Gets the Global Attribute values for the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static string Value(string key)
        {
            GlobalAttributes globalAttributes = Read();
            return globalAttributes.AttributeValue( key );
        }

        #endregion
    }
}
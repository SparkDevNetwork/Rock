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
        /// Gets or sets the attributes.  Used to iterate all values when merging possible merge fields
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        public List<AttributeCache> Attributes { get; set; }

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

                var attributeCache = Attributes.FirstOrDefault(a => a.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
                if ( attributeCache != null )
                {
                    var attributeValue = new AttributeValueService().GetByAttributeIdAndEntityId( attributeCache.Id, null ).FirstOrDefault();
                    string value = ( attributeValue != null && !string.IsNullOrEmpty( attributeValue.Value ) ) ? attributeValue.Value : attributeCache.DefaultValue;
                    AttributeValues.Add( attributeCache.Key, new KeyValuePair<string, string>( attributeCache.Name, value ) );

                    return value;
                }

                return string.Empty;
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

                            Attributes.Add( AttributeCache.Read( attribute.Id ) );
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

            var attributeCache = Attributes.FirstOrDefault( a => a.Key.Equals( key, StringComparison.OrdinalIgnoreCase ) );
            if ( attributeCache != null ) // (Should never be null)
            {
                if ( AttributeValues.Keys.Contains( key ) )
                {
                    AttributeValues[key] = new KeyValuePair<string, string>( attributeCache.Name, value );
                }
                else
                {
                    AttributeValues.Add( key, new KeyValuePair<string, string>( attributeCache.Name, value ) );
                }
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
                globalAttributes.Attributes = new List<AttributeCache>();
                globalAttributes.AttributeValues = new Dictionary<string, KeyValuePair<string, string>>();

                var attributeService = new Rock.Model.AttributeService();
                var attributeValueService = new Rock.Model.AttributeValueService();

                foreach ( Rock.Model.Attribute attribute in attributeService.GetGlobalAttributes() )
                {
                    var attributeCache = AttributeCache.Read( attribute );
                    globalAttributes.Attributes.Add( attributeCache );

                    var attributeValue = attributeValueService.GetByAttributeIdAndEntityId( attribute.Id, null ).FirstOrDefault();
                    string value = ( attributeValue != null && !string.IsNullOrEmpty( attributeValue.Value ) ) ? attributeValue.Value : attributeCache.DefaultValue;
                    globalAttributes.AttributeValues.Add( attributeCache.Key, new KeyValuePair<string, string>( attributeCache.Name, value ) );
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
        /// Gets the global attribute values as merge fields for dotLiquid merging.
        /// </summary>
        /// <param name="currentPerson">The current person.</param>
        /// <returns></returns>
        public static Dictionary<string, object> GetMergeFields(Person currentPerson)
        {
            var configValues = new Dictionary<string, object>();

            // Add any global attribute values that user has authorization to view
            var globalAttributeValues = new Dictionary<string, object>();
            var globalAttributes = Rock.Web.Cache.GlobalAttributesCache.Read();
            foreach ( var attributeCache in globalAttributes.Attributes.OrderBy( a => a.Key ) )
            {
                if ( attributeCache.IsAuthorized( "View", currentPerson ) )
                {
                    string value = attributeCache.FieldType.Field.FormatValue( null, globalAttributes.AttributeValues[attributeCache.Key].Value, attributeCache.QualifierValues, false );
                    globalAttributeValues.Add( attributeCache.Key, value );
                }
            }
            configValues.Add( "GlobalAttribute", globalAttributeValues );

            //    // Add any application config values as available merge objects
            //    var appSettingValues = new Dictionary<string, object>();
            //    foreach ( string key in System.Configuration.ConfigurationManager.AppSettings.AllKeys )
            //    {
            //        appSettingValues.Add( key, System.Configuration.ConfigurationManager.AppSettings[key] );
            //    }
            //    configValues.Add( "AppSetting", appSettingValues );
            
            // Recursively resolve any of the config values that may have other merge codes as part of their value
            foreach ( var collection in configValues.ToList() )
            {
                var collectionDictionary = collection.Value as Dictionary<string, object>;
                foreach ( var item in collectionDictionary.ToList() )
                {
                    collectionDictionary[item.Key] = ResolveConfigValue( item.Value as string, configValues );
                }
            }

            return configValues;
        }

        /// <summary>
        /// Resolves the config value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="configValues">The config values.</param>
        /// <returns></returns>
        private static string ResolveConfigValue( string value, Dictionary<string, object> configValues )
        {
            string result = value.ResolveMergeFields( configValues );

            // If anything was resolved, keep resolving until nothing changed.
            while ( result != value )
            {
                value = result;
                result = ResolveConfigValue( result, configValues );
            }

            return result;
        }

        #endregion
    }
}
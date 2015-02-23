// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Global Attributes
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    public class GlobalAttributesCache
    {

        #region Contants

        /// <summary>
        /// This setting is the guid for the organization's location record.
        /// </summary>
        private static readonly string ORG_LOC_GUID = "com.rockrms.orgLocationGuid";
        
        /// <summary>
        /// This setting is the state for the organization's location record.
        /// </summary>
        private static readonly string ORG_LOC_STATE = "com.rockrms.orgLocationState";

        /// <summary>
        /// This setting is the country for the organization's location record.
        /// </summary>
        private static readonly string ORG_LOC_COUNTRY = "com.rockrms.orgLocationCountry";

        #endregion

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
        public Dictionary<string, string> AttributeValues { get; set; }

        /// <summary>
        /// Gets or sets the attribute values formatted.
        /// </summary>
        /// <value>
        /// The attribute values formatted.
        /// </value>
        public Dictionary<string, string> AttributeValuesFormatted { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the Global Attribute values for the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public string GetValue( string key, RockContext rockContext = null )
        {
            if ( AttributeValues.Keys.Contains( key ) )
            {
                return AttributeValues[key];
            }
            else
            {
                var attributeCache = Attributes.FirstOrDefault( a => a.Key.Equals( key, StringComparison.OrdinalIgnoreCase ) );
                if ( attributeCache != null )
                {
                    var attributeValue = new AttributeValueService( rockContext ?? new RockContext() ).GetByAttributeIdAndEntityId( attributeCache.Id, null );
                    string value = ( attributeValue != null && !string.IsNullOrEmpty( attributeValue.Value ) ) ? attributeValue.Value : attributeCache.DefaultValue;
                    AttributeValues.Add( key, value );

                    return value;
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the value formatted.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public string GetValueFormatted( string key, RockContext rockContext = null )
        {
            if ( AttributeValuesFormatted.Keys.Contains( key ) )
            {
                return AttributeValuesFormatted[key];
            }
            else
            {
                string value = GetValue( key, rockContext );
                if ( !string.IsNullOrWhiteSpace( value ) )
                {
                    var attributeCache = Attributes.FirstOrDefault( a => a.Key.Equals( key, StringComparison.OrdinalIgnoreCase ) );
                    if ( attributeCache != null )
                    {
                        value = attributeCache.FieldType.Field.FormatValue( null, value, attributeCache.QualifierValues, false );
                    }
                }

                AttributeValuesFormatted.Add( key, value );

                return value;
            }
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="saveValue">if set to <c>true</c> [save value].</param>
        /// <param name="rockContext">The rock context.</param>
        public void SetValue( string key, string value, bool saveValue, RockContext rockContext = null )
        {
            if ( saveValue )
            {
                if ( rockContext == null )
                {
                    rockContext = new RockContext();
                }

                // Save new value
                var attributeValueService = new AttributeValueService( rockContext );
                var attributeValue = attributeValueService.GetGlobalAttributeValue( key );

                if ( attributeValue == null )
                {
                    var attributeService = new AttributeService( rockContext );
                    var attribute = attributeService.GetGlobalAttribute( key );
                    if ( attribute == null )
                    {
                        attribute = new Rock.Model.Attribute();
                        attribute.FieldTypeId = FieldTypeCache.Read( new Guid( SystemGuid.FieldType.TEXT ) ).Id;
                        attribute.EntityTypeQualifierColumn = string.Empty;
                        attribute.EntityTypeQualifierValue = string.Empty;
                        attribute.Key = key;
                        attribute.Name = key.SplitCase();
                        attributeService.Add( attribute );
                        rockContext.SaveChanges();

                        Attributes.Add( AttributeCache.Read( attribute.Id ) );
                    }

                    attributeValue = new AttributeValue();
                    attributeValue.IsSystem = false;
                    attributeValue.AttributeId = attribute.Id;
                    attributeValueService.Add( attributeValue );
                }

                attributeValue.Value = value;
                rockContext.SaveChanges();
            }

            AttributeValues.AddOrReplace( key, value);

            var attributeCache = Attributes.FirstOrDefault( a => a.Key.Equals( key, StringComparison.OrdinalIgnoreCase ) );
            if ( attributeCache != null )
            {
                value = attributeCache.FieldType.Field.FormatValue( null, value, attributeCache.QualifierValues, false );
            }
            AttributeValuesFormatted.AddOrReplace( key, value );

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
        public static GlobalAttributesCache Read( RockContext rockContext = null )
        {
            string cacheKey = GlobalAttributesCache.CacheKey();

            ObjectCache cache = RockMemoryCache.Default;
            GlobalAttributesCache globalAttributes = cache[cacheKey] as GlobalAttributesCache;

            if ( globalAttributes != null )
            {
                return globalAttributes;
            }
            else
            {
                globalAttributes = new GlobalAttributesCache();
                globalAttributes.Attributes = new List<AttributeCache>();
                globalAttributes.AttributeValues = new Dictionary<string, string>();
                globalAttributes.AttributeValuesFormatted = new Dictionary<string, string>();

                rockContext = rockContext ?? new RockContext();
                var attributeService = new Rock.Model.AttributeService( rockContext );
                var attributeValueService = new Rock.Model.AttributeValueService( rockContext );

                var attributes = attributeService.GetGlobalAttributes();
                var attributeValues = attributeValueService.Queryable()
                    .Where( v =>
                        (!v.EntityId.HasValue || v.EntityId.Value == 0) &&
                        attributes.Select( a => a.Id ).ToList().Contains( v.AttributeId ) )
                    .ToList();

                foreach ( var attribute in attributes )
                {
                    var attributeCache = AttributeCache.Read( attribute );
                    globalAttributes.Attributes.Add( attributeCache );

                    var attributeValue = attributeValues.FirstOrDefault( v => v.AttributeId == attribute.Id);
                    string value = ( attributeValue != null && !string.IsNullOrEmpty( attributeValue.Value ) ) ? attributeValue.Value : attributeCache.DefaultValue;
                    globalAttributes.AttributeValues.Add( attributeCache.Key, value );
                    globalAttributes.AttributeValuesFormatted.Add( attributeCache.Key, attributeCache.FieldType.Field.FormatValue( null, value, attributeCache.QualifierValues, false ) );
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
            ObjectCache cache = RockMemoryCache.Default;
            cache.Remove( GlobalAttributesCache.CacheKey() );

            if ( HttpContext.Current != null )
            {
                var appSettings = HttpContext.Current.Application;
                appSettings[ORG_LOC_GUID] = null;
                appSettings[ORG_LOC_STATE] = null;
                appSettings[ORG_LOC_COUNTRY] = null;
            }
        }

        /// <summary>
        /// Gets the global attribute values as merge fields for dotLiquid merging.
        /// </summary>
        /// <param name="currentPerson">The current person.</param>
        /// <returns></returns>
        public static Dictionary<string, object> GetMergeFields( Person currentPerson )
        {
            var configValues = new Dictionary<string, object>();

            // Add any global attribute values that user has authorization to view
            var globalAttributeValues = new Dictionary<string, object>();
            var globalAttributes = Rock.Web.Cache.GlobalAttributesCache.Read();
            foreach ( var attributeCache in globalAttributes.Attributes.OrderBy( a => a.Key ) )
            {
                if ( attributeCache.IsAuthorized( Authorization.VIEW, currentPerson ) )
                {
                    globalAttributeValues.Add( attributeCache.Key, globalAttributes.GetValueFormatted( attributeCache.Key ) );
                }
            }
            configValues.Add( "GlobalAttribute", globalAttributeValues );

            // Recursively resolve any of the config values that may have other merge codes as part of their value
            foreach ( var collection in configValues.ToList() )
            {
                var collectionDictionary = collection.Value as Dictionary<string, object>;
                foreach ( var item in collectionDictionary.ToList() )
                {
                    collectionDictionary[item.Key] = ResolveConfigValue( item.Value as string, configValues, currentPerson );
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
        private static string ResolveConfigValue( string value, Dictionary<string, object> configValues, Person currentPerson )
        {
            string result = value.ResolveMergeFields( configValues, currentPerson );

            // If anything was resolved, keep resolving until nothing changed.
            while ( result != value )
            {
                value = result;
                result = ResolveConfigValue( result, configValues, currentPerson );
            }

            return result;
        }

        /// <summary>
        /// Gets the organization location.
        /// </summary>
        /// <value>
        /// The organization location.
        /// </value>
        public Location OrganizationLocation
        {
            get 
            {
                Guid? locGuid = GetValue( "OrganizationAddress" ).AsGuidOrNull();
                if ( locGuid.HasValue )
                {
                    return new Rock.Model.LocationService( new RockContext() ).Get( locGuid.Value );
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the state of the organization.
        /// </summary>
        /// <value>
        /// The state of the organization.
        /// </value>
        public string OrganizationState
        {
            get
            {
                // Check to see if there is an global attribute for organization address
                Guid? locGuid = GetValue( "OrganizationAddress" ).AsGuidOrNull();
                if ( locGuid.HasValue )
                {
                    if ( HttpContext.Current != null )
                    {
                        var appSettings = HttpContext.Current.Application;

                        // If the organization location is still same as last check, use saved values
                        if ( appSettings[ORG_LOC_GUID] != null &&
                            locGuid.Equals( (Guid)appSettings[ORG_LOC_GUID] ) &&
                            appSettings[ORG_LOC_STATE] != null )
                        {
                            return appSettings[ORG_LOC_STATE].ToString();
                        }
                        else
                        {
                            // otherwise read the new location and save
                            appSettings[ORG_LOC_GUID] = locGuid.Value;
                            var location = new Rock.Model.LocationService( new RockContext() ).Get( locGuid.Value );
                            if ( location != null )
                            {
                                appSettings[ORG_LOC_STATE] = location.State;
                                appSettings[ORG_LOC_COUNTRY] = location.Country;
                                return location.State;
                            }
                        }
                    }
                    else
                    {
                        var location = new Rock.Model.LocationService( new RockContext() ).Get( locGuid.Value );
                        if ( location != null )
                        {
                            return location.State;
                        }
                    }
                }

                return string.Empty;
            }
        }


        /// <summary>
        /// Gets the organization country.
        /// </summary>
        /// <value>
        /// The organization country.
        /// </value>
        public string OrganizationCountry
        {
            get
            {
                // Check to see if there is an global attribute for organization address
                Guid? locGuid = GetValue( "OrganizationAddress" ).AsGuidOrNull();
                if ( locGuid.HasValue )
                {
                    if ( HttpContext.Current != null )
                    {
                        var appSettings = HttpContext.Current.Application;

                        // If the organization location is still same as last check, use saved values
                        if ( appSettings[ORG_LOC_GUID] != null &&
                            locGuid.Equals( (Guid)appSettings[ORG_LOC_GUID] ) &&
                            appSettings[ORG_LOC_COUNTRY] != null )
                        {
                            return appSettings[ORG_LOC_COUNTRY].ToString();
                        }
                        else
                        {
                            // otherwise read the new location and save 
                            appSettings[ORG_LOC_GUID] = locGuid.Value;
                            var location = new Rock.Model.LocationService( new RockContext() ).Get( locGuid.Value );
                            if ( location != null )
                            {
                                appSettings[ORG_LOC_STATE] = location.State;
                                appSettings[ORG_LOC_COUNTRY] = location.Country;
                                return location.Country;
                            }
                        }
                    }
                    else
                    {
                        var location = new Rock.Model.LocationService( new RockContext() ).Get( locGuid.Value );
                        if ( location != null )
                        {
                            return location.Country;
                        }
                    }
                }

                return string.Empty;
            }
        }

        #endregion
    }
}
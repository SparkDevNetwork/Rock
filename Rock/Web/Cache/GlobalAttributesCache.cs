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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Caching;
using System.Web;

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

        private object _obj = new object();

        /// <summary>
        /// Gets or sets the attributes.  Used to iterate all values when merging possible merge fields
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        public List<AttributeCache> Attributes 
        {
            get
            {
                var attributes = new List<AttributeCache>();

                lock ( _obj )
                {
                    if ( attributeIds == null )
                    {
                        using ( var rockContext = new RockContext() )
                        {
                            attributeIds = new Model.AttributeService( rockContext )
                                .Queryable().AsNoTracking()
                                .Where( t => 
                                    !t.EntityTypeId.HasValue && 
                                    ( t.EntityTypeQualifierColumn == null || t.EntityTypeQualifierColumn == string.Empty ) &&
                                    ( t.EntityTypeQualifierValue == null || t.EntityTypeQualifierValue == string.Empty ) )
                                .Select( t => t.Id )
                                .ToList();
                        }
                    }
                }

                foreach ( int id in attributeIds )
                {
                    var attribute = AttributeCache.Read( id );
                    if ( attribute != null )
                    {
                        attributes.Add( attribute );
                    }
                }

                return attributes;
            }

            set
            {
                lock ( _obj )
                {
                    if ( value != null )
                    {
                        attributeIds = value.Select( a => a.Id ).ToList();
                    }
                    else
                    {
                        attributeIds = null;
                    }
                }
            }
        }
        private List<int> attributeIds = null;

        /// <summary>
        /// Gets or sets the attribute values.
        /// </summary>
        /// <value>
        /// The attribute values.
        /// </value>
        public ConcurrentDictionary<string, string> AttributeValues { get; set; }

        /// <summary>
        /// Gets or sets the attribute values formatted.
        /// </summary>
        /// <value>
        /// The attribute values formatted.
        /// </value>
        public ConcurrentDictionary<string, string> AttributeValuesFormatted { get; set; }

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
            string value = string.Empty;
            if ( AttributeValues.TryGetValue( key, out value ) )
            {
                return value;
            }

            var attributeCache = Attributes.FirstOrDefault( a => a.Key.Equals( key, StringComparison.OrdinalIgnoreCase ) );
            if ( attributeCache != null )
            {
                if ( rockContext != null )
                {
                    return GetValue( key, attributeCache, rockContext );
                }
                else
                {
                    using ( var myRockContext = new RockContext() )
                    {
                        return GetValue( key, attributeCache, myRockContext );
                    }
                }
            }

            return string.Empty;
         }

        private string GetValue( string key, AttributeCache attributeCache, RockContext rockContext )
        {
            var attributeValue = new AttributeValueService( rockContext ).GetByAttributeIdAndEntityId( attributeCache.Id, null );
            string value = ( attributeValue != null && !string.IsNullOrEmpty( attributeValue.Value ) ) ? attributeValue.Value : attributeCache.DefaultValue;
            AttributeValues.AddOrUpdate( key, value, ( k, v ) => value );
            return value;
        }

        /// <summary>
        /// Gets the value formatted.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public string GetValueFormatted( string key, RockContext rockContext = null )
        {
            string formattedValue = string.Empty;
            if ( AttributeValuesFormatted.TryGetValue( key, out formattedValue ) )
            {
                return formattedValue;
            }

            string value = GetValue( key, rockContext );
            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                var attributeCache = Attributes.FirstOrDefault( a => a.Key.Equals( key, StringComparison.OrdinalIgnoreCase ) );
                if ( attributeCache != null )
                {
                    value = attributeCache.FieldType.Field.FormatValue( null, value, attributeCache.QualifierValues, false );
                }
            }

            AttributeValuesFormatted.AddOrUpdate( key, value, (k, v) => value );

            return value;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="saveValue">if set to <c>true</c> [save value].</param>
        public void SetValue( string key, string value, bool saveValue )
        {
            using ( var rockContext = new RockContext() )
            {
                SetValue( key, value, saveValue, rockContext );
            }
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="saveValue">if set to <c>true</c> [save value].</param>
        /// <param name="rockContext">The rock context.</param>
        public void SetValue( string key, string value, bool saveValue, RockContext rockContext )
        {
            AttributeCache attributeCache = null;

            if ( saveValue )
            {
                // Save new value
                rockContext = rockContext ?? new RockContext();
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
                    }

                    attributeValue = new AttributeValue();
                    attributeValue.IsSystem = false;
                    attributeValue.AttributeId = attribute.Id;
                    attributeValueService.Add( attributeValue );
                }

                attributeValue.Value = value;
                rockContext.SaveChanges();
            }

            lock(_obj)
            {
                attributeIds = null;
            }

            AttributeValues.AddOrUpdate( key, value, ( k, v ) => value );

            attributeCache = Attributes.FirstOrDefault( a => a.Key.Equals( key, StringComparison.OrdinalIgnoreCase ) );
            if ( attributeCache != null )
            {
                value = attributeCache.FieldType.Field.FormatValue( null, value, attributeCache.QualifierValues, false );
            }
            AttributeValuesFormatted.AddOrUpdate( key, value, (k, v) => value);

        }

        #endregion

        #region Static Methods

        private static RockMemoryCache _cache = RockMemoryCache.Default;

        private static string CacheKey()
        {
            return "Rock:GlobalAttributes";
        }

        /// <summary>
        /// Gets the existing or a new item from cache
        /// </summary>
        /// <typeparam name="TT">The type of the t.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="valueFactory">The value factory.</param>
        /// <returns></returns>
        public static GlobalAttributesCache GetOrAddExisting( string key, Func<GlobalAttributesCache> valueFactory )
        {
            var newValue = new Lazy<GlobalAttributesCache>( valueFactory );
            var oldValue = _cache.AddOrGetExisting( key, newValue, new CacheItemPolicy() ) as Lazy<GlobalAttributesCache>;
            try
            {
                return ( oldValue ?? newValue ).Value;
            }
            catch
            {
                _cache.Remove( key );
                throw;
            }
        }

        /// <summary>
        /// Returns Global Attributes from cache.  If they are not already in cache, they
        /// will be read and added to cache
        /// </summary>
        /// <returns></returns>
        public static GlobalAttributesCache Read()
        {
            using ( var rockContext = new RockContext() )
            {
                return Read( rockContext );
            }
        }

        /// <summary>
        /// Reads the specified rock context.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static GlobalAttributesCache Read( RockContext rockContext )
        {
            return GetOrAddExisting( GlobalAttributesCache.CacheKey(),
                () => Load( rockContext ) );
        }

        private static GlobalAttributesCache Load( RockContext rockContext )
        {
            if ( rockContext != null )
            {
                return Load2( rockContext );
            }

            using ( var rockContext2 = new RockContext() )
            {
                return Load2( rockContext2 );
            }
        }

        private static GlobalAttributesCache Load2( RockContext rockContext )
        {
            var globalAttributes = new GlobalAttributesCache();
            globalAttributes.AttributeValues = new ConcurrentDictionary<string, string>();
            globalAttributes.AttributeValuesFormatted = new ConcurrentDictionary<string, string>();
            return globalAttributes;
        }

        /// <summary>
        /// Returns the global attribute value for the given key.
        /// </summary>
        /// <returns></returns>
        public static string Value( string key )
        {
            return Read().GetValue( key );
        }

        /// <summary>
        /// Removes Global Attributes from cache
        /// </summary>
        public static void Flush()
        {
            _cache.Remove( GlobalAttributesCache.CacheKey() );

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
        /// <param name="currentPerson">The current person.</param>
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
                    using ( var rockContext = new RockContext() )
                    {
                        return new Rock.Model.LocationService( rockContext ).Get( locGuid.Value );
                    }
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
                            using ( var rockContext = new RockContext() )
                            {
                                var location = new Rock.Model.LocationService( rockContext ).Get( locGuid.Value );
                                if ( location != null )
                                {
                                    appSettings[ORG_LOC_STATE] = location.State;
                                    appSettings[ORG_LOC_COUNTRY] = location.Country;
                                    return location.State;
                                }
                            }
                        }
                    }
                    else
                    {
                        using ( var rockContext = new RockContext() )
                        {
                            var location = new Rock.Model.LocationService( rockContext ).Get( locGuid.Value );
                            if ( location != null )
                            {
                                return location.State;
                            }
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
                            using ( var rockContext = new RockContext() )
                            {
                                var location = new Rock.Model.LocationService( rockContext ).Get( locGuid.Value );
                                if ( location != null )
                                {
                                    appSettings[ORG_LOC_STATE] = location.State;
                                    appSettings[ORG_LOC_COUNTRY] = location.Country;
                                    return location.Country;
                                }
                            }
                        }
                    }
                    else
                    {
                        using ( var rockContext = new RockContext() )
                        {
                            var location = new Rock.Model.LocationService( rockContext ).Get( locGuid.Value );
                            if ( location != null )
                            {
                                return location.Country;
                            }
                        }
                    }
                }

                return string.Empty;
            }
        }

        #endregion
    }
}
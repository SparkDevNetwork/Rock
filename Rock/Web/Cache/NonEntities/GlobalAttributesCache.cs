// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
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
    [DataContract]
    public class GlobalAttributesCache : ItemCache<GlobalAttributesCache>
    {
        #region Contants

        /// <summary>
        /// This setting is the guid for the organization's location record.
        /// </summary>
        private const string ORG_LOC_GUID = "com.rockrms.orgLocationGuid";

        /// <summary>
        /// This setting is the state for the organization's location record.
        /// </summary>
        private const string ORG_LOC_STATE = "com.rockrms.orgLocationState";

        /// <summary>
        /// This setting is the country for the organization's location record.
        /// </summary>
        private const string ORG_LOC_COUNTRY = "com.rockrms.orgLocationCountry";

        /// <summary>
        /// This setting is the formatted organization's location (used by legacy lava support).
        /// </summary>
        private const string ORG_LOC_FORMATTED = "com.rockrms.orgLoctionFormatted";

        #endregion

        #region Constructors

        /// <summary>
        /// Use Static Get() method to instantiate a new Global Attributes object
        /// </summary>
        private GlobalAttributesCache()
        {
        }

        #endregion

        #region Properties

        private readonly object _obj = new object();

        /// <summary>
        /// Gets or sets the attributes.  Used to iterate all values when merging possible merge fields
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
		[DataMember]
        public List<AttributeCache> Attributes
        {
            get
            {
                var attributes = new List<AttributeCache>();

                if ( _attributeIds == null )
                {
                    lock ( _obj )
                    {
                        if ( _attributeIds == null )
                        {
                            using ( var rockContext = new RockContext() )
                            {
                                _attributeIds = new AttributeService( rockContext )
                                    .Queryable().AsNoTracking()
                                    .Where( t =>
                                         !t.EntityTypeId.HasValue &&
                                         ( t.EntityTypeQualifierColumn == null ||
                                          t.EntityTypeQualifierColumn == string.Empty ) &&
                                         ( t.EntityTypeQualifierValue == null ||
                                          t.EntityTypeQualifierValue == string.Empty ) )
                                    .Select( t => t.Id )
                                    .ToList();
                            }
                        }
                    }
                }

                foreach ( var id in _attributeIds )
                {
                    var attribute = AttributeCache.Get( id );
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
                    _attributeIds = value?.Select( a => a.Id ).ToList();
                }
            }
        }
        private List<int> _attributeIds;

        /// <summary>
        /// Gets or sets the attribute values.
        /// </summary>
        /// <value>
        /// The attribute values.
        /// </value>
        [DataMember]
        private ConcurrentDictionary<string, string> AttributeValues { get; set; } = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// Gets or sets the attribute values formatted.
        /// </summary>
        /// <value>
        /// The attribute values formatted.
        /// </value>
        [DataMember]
        private ConcurrentDictionary<string, string> AttributeValuesFormatted { get; set; } = new ConcurrentDictionary<string, string>();

        #endregion

        #region Obsolete Methods

        /// <summary>
        /// Reads this instance.
        /// </summary>
        /// <returns></returns>
        [RockObsolete( "1.8" )]
        [Obsolete( "Use Get instead" )]
        public static GlobalAttributesCache Read()
        {
            return Get();
        }

        /// <summary>
        /// Reads the specified rock context.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        [RockObsolete( "1.8" )]
        [Obsolete( "Use Get instead" )]
        public static GlobalAttributesCache Read( RockContext rockContext )
        {
            return Get();
        }

        #endregion


        #region Public Methods

        /// <summary>
        /// Gets the Global Attribute values for the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public string GetValue( string key )
        {
            return GetValue( key, null );
        }

        /// <summary>
        /// Gets the Global Attribute values for the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public string GetValue( string key, RockContext rockContext )
        {
            string value;

            if ( AttributeValues.TryGetValue( key, out value ) )
            {
                return value;
            }

            var attributeCache = Attributes.FirstOrDefault( a => a.Key.Equals( key, StringComparison.OrdinalIgnoreCase ) );
            if ( attributeCache == null ) return string.Empty;

            if ( rockContext != null )
            {
                return GetValue( key, attributeCache, rockContext );
            }

            using ( var myRockContext = new RockContext() )
            {
                return GetValue( key, attributeCache, myRockContext );
            }

        }

        private string GetValue( string key, AttributeCache attributeCache, RockContext rockContext )
        {
            var attributeValue = new AttributeValueService( rockContext ).GetByAttributeIdAndEntityId( attributeCache.Id, null );
            var value = ( !string.IsNullOrEmpty( attributeValue?.Value ) ) ? attributeValue.Value : attributeCache.DefaultValue;
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
            string formattedValue;
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

            AttributeValuesFormatted.AddOrUpdate( key, value, ( k, v ) => value );

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
                        attribute = new Model.Attribute
                        {
                            FieldTypeId = FieldTypeCache.Get( new Guid( SystemGuid.FieldType.TEXT ) ).Id,
                            EntityTypeQualifierColumn = string.Empty,
                            EntityTypeQualifierValue = string.Empty,
                            Key = key,
                            Name = key.SplitCase()
                        };
                        attributeService.Add( attribute );
                        rockContext.SaveChanges();
                    }

                    attributeValue = new AttributeValue
                    {
                        IsSystem = false,
                        AttributeId = attribute.Id
                    };
                    attributeValueService.Add( attributeValue );
                }

                attributeValue.Value = value;
                rockContext.SaveChanges();
            }

            lock ( _obj )
            {
                _attributeIds = null;
            }

            AttributeValues.AddOrUpdate( key, value, ( k, v ) => value );

            var attributeCache = Attributes.FirstOrDefault( a => a.Key.Equals( key, StringComparison.OrdinalIgnoreCase ) );
            if ( attributeCache != null )
            {
                value = attributeCache.FieldType.Field.FormatValue( null, value, attributeCache.QualifierValues, false );
            }
            AttributeValuesFormatted.AddOrUpdate( key, value, ( k, v ) => value );

        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Gets the or add existing.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="valueFactory">The value factory.</param>
        /// <returns></returns>
        [RockObsolete( "1.8" )]
        [Obsolete("No longer needed")]
        public new static GlobalAttributesCache GetOrAddExisting( string key, Func<GlobalAttributesCache> valueFactory )
        {
            // Note we still need the private method, we are just making the public method obsolete
            return ItemCache<GlobalAttributesCache>.GetOrAddExisting( key, Load );
        }

        /// <summary>
        /// Gets this instance.
        /// </summary>
        /// <returns></returns>
        public static GlobalAttributesCache Get()
        {
            // NOTE this can be changed plain GetOrAddExisting once the above obsolete 
            return ItemCache<GlobalAttributesCache>.GetOrAddExisting( AllString, Load );
        }

        private static GlobalAttributesCache Load()
        {
            var globalAttributes = new GlobalAttributesCache
            {
                AttributeValues = new ConcurrentDictionary<string, string>(),
                AttributeValuesFormatted = new ConcurrentDictionary<string, string>()
            };
            return globalAttributes;
        }

        /// <summary>
        /// Returns the global attribute value for the given key.
        /// </summary>
        /// <returns></returns>
        public static string Value( string key )
        {
            // pass null to the Read(RockContext rockContext) overload so that it doesn't create the RockContext unless it needs to fetch it from the database. This speeds this up from 0.250ms/call to about .001ms/call
            return Get().GetValue( key );
        }

        /// <summary>
        /// Removes Global Attributes from cache
        /// </summary>
        public static void Remove()
        {
            Clear();

            if ( HttpContext.Current == null ) return;

            var appSettings = HttpContext.Current.Application;
            appSettings[ORG_LOC_GUID] = null;
            appSettings[ORG_LOC_STATE] = null;
            appSettings[ORG_LOC_COUNTRY] = null;
        }

        /// <summary>
        /// Gets the legacy global attribute values as merge fields for dotLiquid merging.
        /// Note: You should use LavaHelper.GetCommonMergeFields instead of this
        /// </summary>
        /// <param name="currentPerson">The current person.</param>
        /// <returns></returns>
        internal static Dictionary<string, object> GetLegacyMergeFields( Person currentPerson )
        {
            var configValues = new Dictionary<string, object>();

            // Add any global attribute values that user has authorization to view
            var globalAttributes = Get();
            var globalAttributeValues = globalAttributes.Attributes
                .OrderBy( a => a.Key )
                .Where( attributeCache => attributeCache.IsAuthorized( Authorization.VIEW, currentPerson ) )
                .ToDictionary<AttributeCache, string, object>( attributeCache => attributeCache.Key, attributeCache => globalAttributes.GetValueFormatted( attributeCache.Key ) );

            configValues.Add( "GlobalAttribute", globalAttributeValues );

            // Recursively resolve any of the config values that may have other merge codes as part of their value
            foreach ( var collection in configValues.ToList() )
            {
                var collectionDictionary = collection.Value as Dictionary<string, object>;
                if ( collectionDictionary == null ) continue;

                foreach ( var item in collectionDictionary.ToList() )
                {
                    collectionDictionary[item.Key] =
                        ResolveConfigValue( item.Value as string, configValues, currentPerson );
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
            var result = value.ResolveMergeFields( configValues, currentPerson );

            // If anything was resolved, keep resolving until nothing changed.
            while ( result != value )
            {
                value = result;
                result = ResolveConfigValue( result, configValues, currentPerson );
            }

            return result;
        }

        /// <summary>
        /// Gets the current graduation year based on grade transition date
        /// </summary>
        /// <value>
        /// Returns current year if transition month/day has not passed, otherwise will return next year
        /// </value>
        public int CurrentGraduationYear
        {
            get
            {
                var formattedTransitionDate = GetValue( "GradeTransitionDate" ) + "/" + RockDateTime.Today.Year;
                DateTime transitionDate;

                // Check Date Validity
                if ( !DateTime.TryParseExact( formattedTransitionDate, new[] { "MM/dd/yyyy", "M/dd/yyyy", "M/d/yyyy", "MM/d/yyyy" }, CultureInfo.InvariantCulture,
                    DateTimeStyles.AllowWhiteSpaces, out transitionDate ) )
                {
                    transitionDate = new DateTime( RockDateTime.Today.Year, 6, 1 );
                }

                return RockDateTime.Now.Date < transitionDate ? transitionDate.Year : transitionDate.Year + 1;
            }
        }
        /// <summary>
        /// Gets the organization location (OrganizationAddress)
        /// </summary>
        /// <value>
        /// The organization location.
        /// </value>
        public Location OrganizationLocation
        {
            get
            {
                var locGuid = GetValue( "OrganizationAddress" ).AsGuidOrNull();
                if ( !locGuid.HasValue ) return null;

                using ( var rockContext = new RockContext() )
                {
                    return new LocationService( rockContext ).Get( locGuid.Value );
                }

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
                var locGuid = GetValue( "OrganizationAddress" ).AsGuidOrNull();
                if ( !locGuid.HasValue ) return string.Empty;

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

                    // otherwise read the new location and save
                    appSettings[ORG_LOC_GUID] = locGuid.Value;
                    using ( var rockContext = new RockContext() )
                    {
                        var location = new LocationService( rockContext ).Get( locGuid.Value );
                        if ( location == null ) return string.Empty;

                        appSettings[ORG_LOC_STATE] = location.State;
                        appSettings[ORG_LOC_COUNTRY] = location.Country;
                        return location.State;
                    }
                }

                using ( var rockContext = new RockContext() )
                {
                    var location = new LocationService( rockContext ).Get( locGuid.Value );
                    if ( location != null )
                    {
                        return location.State;
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
                var locGuid = GetValue( "OrganizationAddress" ).AsGuidOrNull();
                if ( !locGuid.HasValue ) return string.Empty;

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

                    // otherwise read the new location and save 
                    appSettings[ORG_LOC_GUID] = locGuid.Value;
                    using ( var rockContext = new RockContext() )
                    {
                        var location = new LocationService( rockContext ).Get( locGuid.Value );
                        if ( location == null ) return string.Empty;

                        appSettings[ORG_LOC_STATE] = location.State;
                        appSettings[ORG_LOC_COUNTRY] = location.Country;
                        return location.Country;
                    }
                }

                using ( var rockContext = new RockContext() )
                {
                    var location = new LocationService( rockContext ).Get( locGuid.Value );
                    if ( location != null )
                    {
                        return location.Country;
                    }
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the organization location formatted.
        /// </summary>
        /// <value>
        /// The organization location formatted.
        /// </value>
        public string OrganizationLocationFormatted
        {
            get
            {
                // Check to see if there is an global attribute for organization address
                var locGuid = GetValue( "OrganizationAddress" ).AsGuidOrNull();
                if ( !locGuid.HasValue ) return string.Empty;

                if ( HttpContext.Current != null )
                {
                    var appSettings = HttpContext.Current.Application;

                    // If the organization location is still same as last check, use saved values
                    if ( appSettings[ORG_LOC_GUID] != null &&
                         locGuid.Equals( (Guid)appSettings[ORG_LOC_GUID] ) &&
                         appSettings[ORG_LOC_FORMATTED] != null )
                    {
                        return appSettings[ORG_LOC_FORMATTED].ToString();
                    }

                    // otherwise read the new location and save 
                    appSettings[ORG_LOC_GUID] = locGuid.Value;
                    using ( var rockContext = new RockContext() )
                    {
                        var location = new LocationService( rockContext ).Get( locGuid.Value );
                        if ( location == null ) return string.Empty;

                        appSettings[ORG_LOC_FORMATTED] = location.ToString();
                        return location.Country;
                    }
                }

                using ( var rockContext = new RockContext() )
                {
                    var location = new LocationService( rockContext ).Get( locGuid.Value );
                    if ( location != null )
                    {
                        return location.ToString();
                    }
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the lava support level.
        /// </summary>
        /// <value>
        /// The lava support level.
        /// </value>
        public Lava.LavaSupportLevel LavaSupportLevel => GetValue( "core.LavaSupportLevel" ).ConvertToEnumOrNull<Lava.LavaSupportLevel>() ?? Lava.LavaSupportLevel.Legacy;

        /// <summary>
        /// Gets a value indicating whether Envelope Number feature is enabled
        /// </summary>
        /// <value>
        /// <c>true</c> if [enable giving envelope number]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableGivingEnvelopeNumber => GetValue( "core.EnableGivingEnvelopeNumber" ).AsBoolean();

        #endregion
    }
}
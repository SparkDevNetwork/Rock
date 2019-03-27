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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web
{
    /// <summary>
    /// System Settings can be used to persist a key/value 
    /// </summary>
    [Serializable]
    [DataContract]
    public class SystemSettings
    {
        #region Constructors

        private SystemSettings() { }

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
        private List<AttributeCache> Attributes
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
                                    .GetSystemSettings()
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
        }
        private List<int> _attributeIds;

        #endregion

        #region Static Methods

        private static string CacheKey
        {
            get
            {
                return "Rock:SystemSettings";
            }
        }

        private static SystemSettings Get()
        {
            return RockCache.GetOrAddExisting( CacheKey, () => LoadSettings() ) as SystemSettings;
        }

        /// <summary>
        /// Gets the RockInstanceId for this particular installation.
        /// </summary>
        /// <returns>the Guid of this Rock instance</returns>
        public static Guid GetRockInstanceId()
        {
            var settings = Get();
            var attributeCache = settings.Attributes.FirstOrDefault( a => a.Key.Equals( SystemSettingKeys.ROCK_INSTANCE_ID, StringComparison.OrdinalIgnoreCase ) );
            if ( attributeCache != null )
            {
                return attributeCache.Guid;
            }

            return new Guid(); // 0000-0000-0000...
        }

        /// <summary>
        /// Gets the Global Attribute values for the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static string GetValue( string key )
        {
            var settings = Get();
            var attributeCache = settings.Attributes.FirstOrDefault( a => a.Key.Equals( key, StringComparison.OrdinalIgnoreCase ) );
            if ( attributeCache != null )
            {
                return attributeCache.DefaultValue;
            }

            return string.Empty;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public static void SetValue( string key, string value )
        {
            var rockContext = new Rock.Data.RockContext();
            var attributeService = new AttributeService( rockContext );
            var attribute = attributeService.GetSystemSetting( key );
            
            if ( attribute == null )
            {
                attribute = new Rock.Model.Attribute();
                attribute.FieldTypeId = FieldTypeCache.Get( new Guid( SystemGuid.FieldType.TEXT ) ).Id;
                attribute.EntityTypeQualifierColumn = Rock.Model.Attribute.SYSTEM_SETTING_QUALIFIER;
                attribute.EntityTypeQualifierValue = string.Empty;
                attribute.Key = key;
                attribute.Name = key.SplitCase();
                attribute.DefaultValue = value;
                attributeService.Add( attribute );
            }
            else
            {
                attribute.DefaultValue = value;
            }

            // NOTE: Service Layer will automatically update this Cache (see Attribute.cs UpdateCache)
            rockContext.SaveChanges();
        }

        /// <summary>
        /// Gets the value from web configuration.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static string GetValueFromWebConfig( string key )
        {
            return System.Configuration.ConfigurationManager.AppSettings[key] ?? string.Empty;
        }

        /// <summary>
        /// Sets the value to web configuration.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public static void SetValueToWebConfig( string key, string value )
        {
            try
            {
                if ( System.Configuration.ConfigurationManager.AppSettings[key] != null )
                {
                    System.Configuration.Configuration rockWebConfig = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration( "~" );
                    rockWebConfig.AppSettings.Settings[key].Value = value;
                    rockWebConfig.Save();
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, null );
            }
        }

        /// <summary>
        /// Sets values to web configuration.
        /// Use this when saving multiple keys so a save is not called for each key.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public static void SetValueToWebConfig( Dictionary<string,string> settings )
        {
            bool changed = false;
            System.Configuration.Configuration rockWebConfig = null;

            try
            {
                rockWebConfig = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration( "~" );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, null );
            }

            foreach ( var setting in settings )
            {
                if ( System.Configuration.ConfigurationManager.AppSettings[setting.Key] != null )
                {
                    rockWebConfig.AppSettings.Settings[setting.Key].Value = setting.Value;
                    changed = true;
                }
            }

            try
            {
                if ( changed )
                {
                    rockWebConfig.Save();
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, null );
            }
        }

        /// <summary>
        /// Returns Global Attributes from cache.  If they are not already in cache, they
        /// will be read and added to cache
        /// </summary>
        /// <returns></returns>
        private static SystemSettings LoadSettings()
        {
            var systemSettings = new SystemSettings();
            return systemSettings;
        }

        /// <summary>
        /// Flushes this instance.
        /// </summary>
        [RockObsolete( "1.8" )]
        [Obsolete( "Use Remove() method instead" )]
        public static void Flush()
        {
            Remove();
        }

        /// <summary>
        /// Removes this instance.
        /// </summary>
        public static void Remove()
        {
            RockCache.Remove( CacheKey );
        }

        #endregion

    }
}
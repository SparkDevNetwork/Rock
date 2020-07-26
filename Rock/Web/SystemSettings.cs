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
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
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
        #region Properties

        /// <summary>
        /// Gets or sets the system settings values.
        /// </summary>
        /// <value>
        /// The system settings values.
        /// </value>
        [DataMember]
        private ConcurrentDictionary<string, string> SystemSettingsValues { get; set; } = new ConcurrentDictionary<string, string>( StringComparer.OrdinalIgnoreCase );
        private DateTime concurrentDateTime;
        private readonly ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim();

        private DateTime ConcurrentLastUpdated
        {
            get
            {

                cacheLock.EnterReadLock();
                try
                {
                    return concurrentDateTime;
                }
                finally
                {
                    cacheLock.ExitReadLock();
                }
            }
            set
            {
                cacheLock.EnterWriteLock();
                try
                {
                    concurrentDateTime = value;
                }
                finally
                {
                    cacheLock.ExitWriteLock();
                }
            }
        }
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
            return GetValue( Rock.SystemKey.SystemSetting.ROCK_INSTANCE_ID ).AsGuidOrNull() ?? new Guid();
        }

        /// <summary>
        /// Gets the configured Rock FirstDayOfWeek setting
        /// </summary>
        /// <value>
        /// The start day of week.
        /// </value>
        public static DayOfWeek StartDayOfWeek
        {
            get
            {
                if ( startDayOfWeekCache == null )
                {
                    startDayOfWeekCache = GetValue( Rock.SystemKey.SystemSetting.START_DAY_OF_WEEK ).ConvertToEnumOrNull<DayOfWeek>() ?? RockDateTime.DefaultFirstDayOfWeek;
                }

                return startDayOfWeekCache.Value;
            }
        }

        private static DayOfWeek? startDayOfWeekCache;

        /// <summary>
        /// Gets the last updated.
        /// </summary>
        /// <value>
        /// The last updated.
        /// </value>
        public static DateTime LastUpdated
        {
            get
            {
                return Get().ConcurrentLastUpdated;
            }
        }

        /// <summary>
        /// Gets the System Settings values for the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static string GetValue( string key )
        {
            string result;
            if ( Get().SystemSettingsValues.TryGetValue( key, out result ) )
            {
                return result;
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
        public static void SetValueToWebConfig( Dictionary<string, string> settings )
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
        /// Loads/Reload the system settings from the database
        /// </summary>
        /// <returns></returns>
        private static SystemSettings LoadSettings()
        {
            var systemSettings = new SystemSettings()
            {
                SystemSettingsValues = new ConcurrentDictionary<string, string>()
            };

            using ( var rockContext = new RockContext() )
            {
                var systemSettingAttributes = new AttributeService( rockContext ).GetSystemSettings().ToAttributeCacheList();
                var keyValueLookup = systemSettingAttributes.ToDictionary( k => k.Key, v => v.DefaultValue );

                // RockInstanceId is not the default value but the Guid. So we'll do that one seperately.
                keyValueLookup.AddOrReplace( Rock.SystemKey.SystemSetting.ROCK_INSTANCE_ID, systemSettingAttributes.Where( s => s.Key == Rock.SystemKey.SystemSetting.ROCK_INSTANCE_ID ).Select( s => s.Guid ).FirstOrDefault().ToString() );

                systemSettings.SystemSettingsValues = new ConcurrentDictionary<string, string>( keyValueLookup, StringComparer.OrdinalIgnoreCase );
            }

            systemSettings.ConcurrentLastUpdated = DateTime.Now;
            return systemSettings;
        }

        /// <summary>
        /// Flushes this instance.
        /// </summary>
        [RockObsolete( "1.8" )]
        [Obsolete( "Use Remove() method instead", true )]
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

            // use startDayOfWeekCache to optimize how long it takes to get the StartDayOfWeek, since will be used for all .GetSundayDate() calls (1 millions calls was taking 15seconds, but this reduces that down to 25 ms)
            startDayOfWeekCache = null;
        }

        #endregion

        /// <summary>
        /// Finalizes an instance of the <see cref="SystemSettings"/> class.
        /// </summary>
        ~SystemSettings()
        {
            if ( cacheLock != null )
            {
                cacheLock.Dispose();
            }
        }
    }
}
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
using System.Runtime.Caching;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Cache;

namespace Rock.Web
{
    /// <summary>
    /// System Settings can be used to persist a key/value 
    /// </summary>
    [Serializable]
    public class SystemSettings
    {
        #region Constructors

        private SystemSettings() { }

        #endregion

        #region Properties

        private List<CacheAttribute> Attributes { get; set; }

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

            bool isNew = false;
            if ( attribute == null )
            {
                attribute = new Rock.Model.Attribute();
                attribute.FieldTypeId = CacheFieldType.Get( new Guid( SystemGuid.FieldType.TEXT ) ).Id;
                attribute.EntityTypeQualifierColumn = Rock.Model.Attribute.SYSTEM_SETTING_QUALIFIER;
                attribute.EntityTypeQualifierValue = string.Empty;
                attribute.Key = key;
                attribute.Name = key.SplitCase();
                attribute.DefaultValue = value;
                attributeService.Add( attribute );
                isNew = true;
            }
            else
            {
                attribute.DefaultValue = value;
            }

            rockContext.SaveChanges();

            CacheAttribute.Remove( attribute.Id );
            if ( isNew )
            {
                CacheAttribute.RemoveEntityAttributes();
            }

            var settings = Get();
            var attributeCache = settings.Attributes.FirstOrDefault( a => a.Key.Equals( key, StringComparison.OrdinalIgnoreCase ) );
            if ( attributeCache != null )
            {
                attributeCache.DefaultValue = value;
            }
            else
            {
                settings.Attributes.Add( CacheAttribute.Get( attribute.Id ) );
            }
            RockCache.AddOrUpdate( CacheKey, settings );
        }

        /// <summary>
        /// Returns Global Attributes from cache.  If they are not already in cache, they
        /// will be read and added to cache
        /// </summary>
        /// <returns></returns>
        private static SystemSettings LoadSettings()
        {
            var systemSettings = new SystemSettings();
            systemSettings.Attributes = new List<CacheAttribute>();

            var rockContext = new RockContext();
            var attributeService = new Rock.Model.AttributeService( rockContext );

            foreach ( Rock.Model.Attribute attribute in attributeService.GetSystemSettings() )
            {
                var attributeCache = CacheAttribute.Get( attribute );
                systemSettings.Attributes.Add( attributeCache );
            }

            return systemSettings;
        }

        /// <summary>
        /// Flushes this instance.
        /// </summary>
        [Obsolete( "Use Remove() method instead")]
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
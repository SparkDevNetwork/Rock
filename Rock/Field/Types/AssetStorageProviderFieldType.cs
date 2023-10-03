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
#if WEBFORMS
using System.Web.UI;
#endif
using System.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Web.Cache;
using Rock.Web.Cache.Entities;

namespace Rock.Field.Types
{
    /// <summary>
    /// Class AssetStorageProviderFieldType.
    /// Implements the <see cref="Rock.Field.FieldType" />
    /// Implements the <see cref="Rock.Field.IEntityFieldType" />
    /// </summary>
    /// <seealso cref="Rock.Field.FieldType" />
    /// <seealso cref="Rock.Field.IEntityFieldType" />
    [RockPlatformSupport( Utility.RockPlatform.WebForms )]
    [Rock.SystemGuid.FieldTypeGuid( "1596F562-E8D0-4C5F-9A00-23B5594F17E2" )]
    public class AssetStorageProviderFieldType : FieldType, IEntityFieldType, IEntityReferenceFieldType
    {
        /// <inheritdoc />
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            Guid? assetStorageProviderGuid = privateValue.AsGuidOrNull();
            if ( assetStorageProviderGuid.HasValue )
            {
                var assetStorageProviderCache = AssetStorageProviderCache.Get( assetStorageProviderGuid.Value );
                if ( assetStorageProviderCache != null )
                {
                    return assetStorageProviderCache.Name;
                }
            }

            return string.Empty;
        }

        #region IEntityFieldType

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>IEntity.</returns>
        public IEntity GetEntity( string value )
        {
            return GetEntity( value, null );
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>IEntity.</returns>
        public IEntity GetEntity( string value, RockContext rockContext )
        {
            var guid = value.AsGuidOrNull();
            if ( guid.HasValue )
            {
                rockContext = rockContext ?? new RockContext();
                return new AssetStorageProviderService( rockContext ).Get( guid.Value );
            }

            return null;
        }
        #endregion

        #region IEntityReferenceFieldType

        /// <summary>
        /// Gets the referenced entities for the given raw value.
        /// </summary>
        /// <param name="privateValue">The private database value that will be associated with the entities.</param>
        /// <param name="privateConfigurationValues">The private configuration values that describe the field type settings.</param>
        /// <returns>
        /// A list of <see cref="ReferencedEntity" /> objects that identify which entities this value depends on.
        /// </returns>
        List<ReferencedEntity> IEntityReferenceFieldType.GetReferencedEntities( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var assetStorageProviderGuid = privateValue.AsGuidOrNull();

            if ( !assetStorageProviderGuid.HasValue )
            {
                return null;
            }

            var assetStorageProviderId = AssetStorageProviderCache.GetId( assetStorageProviderGuid.Value );

            if ( !assetStorageProviderId.HasValue )
            {
                return null;
            }

            return new List<ReferencedEntity>()
            {
                new ReferencedEntity( EntityTypeCache.GetId<AssetStorageProvider>().Value, assetStorageProviderId.Value )
            };
        }

        /// <summary>
        /// Gets property (database column) names that will trigger an update of
        /// the persisted values when they change.
        /// </summary>
        /// <param name="privateConfigurationValues">The private configuration values that describe the field type settings.</param>
        /// <returns>
        /// A dictionary whose key is the entity type identifier and the values are a list of property names on that entity type to be monitored.
        /// </returns>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<AssetStorageProvider>().Value, nameof( AssetStorageProvider.Name ) )
            };
        }

        #endregion

        #region WebForms
#if WEBFORMS

        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns>System.String.</returns>
        public override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            return !condensed
                ? GetTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) )
                : GetCondensedTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) );
        }

        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The id.</param>
        /// <returns>The control</returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            return new AssetStorageProviderPicker { ID = id, ShowAll = false };
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns>System.String.</returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var picker = control as AssetStorageProviderPicker;
            if ( picker != null )
            {
                int? itemId = picker.SelectedValue.AsIntegerOrNull();
                Guid? itemGuid = null;
                if ( itemId.HasValue )
                {
                    itemGuid = AssetStorageProviderCache.GetGuid( itemId.Value );
                }

                return itemGuid?.ToString();
            }

            return null;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var picker = control as AssetStorageProviderPicker;
            if ( picker != null )
            {
                int? itemId = null;
                Guid? itemGuid = value.AsGuidOrNull();
                if ( itemGuid.HasValue )
                {
                    itemId = AssetStorageProviderCache.GetId( itemGuid.Value );
                }

                picker.SetValue( itemId );
            }
        }

        /// <summary>
        /// Gets the edit value as the IEntity.Id
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns>System.Nullable&lt;System.Int32&gt;.</returns>
        public int? GetEditValueAsEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var guid = GetEditValue( control, configurationValues ).AsGuid();
            return AssetStorageProviderCache.GetId( guid );
        }

        /// <summary>
        /// Sets the edit value from IEntity.Id value
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        public void SetEditValueFromEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues, int? id )
        {
            var guidValue = AssetStorageProviderCache.GetGuid( id ?? 0 )?.ToString() ?? string.Empty;
            SetEditValue( control, configurationValues, guidValue );
        }

#endif
        #endregion
    }
}

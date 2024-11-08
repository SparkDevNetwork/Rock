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
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

using System;
using System.Collections.Generic;
using System.Linq;

#if WEBFORMS
using System.Web.UI;
#endif

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to save and display a prayer request. Stored as PrayerRequest.Guid.
    /// </summary>
    [Serializable]
    [RockPlatformSupport( Utility.RockPlatform.WebForms )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.PRAYER_REQUEST )]
    public class PrayerRequestFieldType : FieldType, IEntityFieldType, IEntityReferenceFieldType
    {
        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var formattedValue = privateValue;

            using ( var rockContext = new RockContext() )
            {
                PrayerRequest prayerRequest = null;

                var guid = privateValue.AsGuidOrNull();
                if ( guid.HasValue )
                {
                    prayerRequest = new PrayerRequestService( rockContext ).GetNoTracking( guid.Value );
                }

                if ( prayerRequest != null && prayerRequest.Text.IsNotNullOrWhiteSpace() )
                {
                    formattedValue = prayerRequest.Text;
                }
            }

            return formattedValue;
        }

        #endregion

        #region Edit Control

        // simple text box implemented by base FieldType

        #endregion

        #region Entity Methods

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The entity.</returns>
        public IEntity GetEntity( string value )
        {
            return GetEntity( value, null );
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The entity.</returns>
        public IEntity GetEntity( string value, RockContext rockContext )
        {
            rockContext = rockContext ?? new RockContext();
            var guid = value.AsGuidOrNull();

            if ( guid.HasValue )
            {
                return new PrayerRequestService( rockContext ).Get( guid.Value );
            }

            return null;
        }

        #endregion

        #region IEntityReferenceFieldType

        /// <inheritdoc/>
        List<ReferencedEntity> IEntityReferenceFieldType.GetReferencedEntities( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var guid = privateValue.AsGuidOrNull();

            if ( !guid.HasValue )
            {
                return null;
            }

            using ( var rockContext = new RockContext() )
            {
                var prayerRequestId = new PrayerRequestService( rockContext ).GetId( guid.Value );

                if ( !prayerRequestId.HasValue )
                {
                    return null;
                }

                return new List<ReferencedEntity>
                {
                    new ReferencedEntity( EntityTypeCache.GetId<PrayerRequest>().Value, prayerRequestId.Value )
                };
            }
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<PrayerRequest>().Value, nameof( PrayerRequest.Text ) )
            };
        }

        #endregion

        #region WebForms
#if WEBFORMS

        /// <summary>
        /// Returns the field's current value(s).
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column).</param>
        /// <returns>The field's current value(s).</returns>
        public override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            return !condensed
                ? GetTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) )
                : GetCondensedTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) );
        }

        /// <summary>
        /// Gets the edit value as the IEntity.Id.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns>The edit value as the IEntity.Id.</returns>
        public int? GetEditValueAsEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var guid = GetEditValue( control, configurationValues ).AsGuid();
            using ( var rockContext = new RockContext() )
            {
                return new PrayerRequestService( rockContext ).GetId( guid );
            }
        }

        /// <summary>
        /// Sets the edit value from IEntity.Id value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        public void SetEditValueFromEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues, int? id )
        {
            using ( var rockContext = new RockContext() )
            {
                var itemGuid = new PrayerRequestService( rockContext ).GetGuid( id ?? 0 );
                var guidValue = itemGuid.HasValue ? itemGuid.ToString() : string.Empty;
                SetEditValue( control, configurationValues, guidValue );
            }
        }

#endif
        #endregion
    }
}
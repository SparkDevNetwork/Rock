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
using System.Data.Entity.Spatial;
using System.Linq;
#if WEBFORMS
using System.Web.UI;
using System.Web.UI.WebControls;

#endif

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks.Reporting.ServiceMetricsEntry;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

/*
 * 8/18/2022 - DSH
 * 
 * This field type persists values about the Location. It tracks all the different
 * properties that might change and will update if those change. However, addresses
 * get formatted by Lava. We could mark this field type as Volatile, but that could
 * be a decent performance hit for something that will probably never happen. If an
 * admin does change the Lava for a countries address formatting, the nightly job
 * to update persisted attribute values will eventually get these back in sync.
 */

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to save and display a location value
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.LOCATION )]
    public class LocationFieldType : FieldType, IEntityFieldType, IEntityReferenceFieldType
    {
        #region Configuration

        private const string CURRENT_PICKER_MODE = "currentPickerMode";

        /// <summary>
        /// A class that has all of the configuration keys for Location List Field Type.
        /// </summary>
        public static class ConfigurationKey
        {
            /// <summary>
            /// The location type
            /// </summary>
            public const string AllowedPickerMode = "allowedPickerModes";
        }


        #endregion

        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( string.IsNullOrWhiteSpace( privateValue ) )
            {
                return string.Empty;
            }

            var locGuid = privateValue.AsGuidOrNull();
            if ( locGuid.HasValue )
            {
                // Check to see if this is the org address first (to avoid db read)
                var globalAttributesCache = GlobalAttributesCache.Get();
                var orgLocGuid = globalAttributesCache.GetValue( "OrganizationAddress" ).AsGuidOrNull();
                if ( orgLocGuid.HasValue && orgLocGuid.Value == locGuid.Value )
                {
                    return globalAttributesCache.OrganizationLocationFormatted;
                }

                using ( var rockContext = new RockContext() )
                {
                    var service = new LocationService( rockContext );
                    var location = service.GetNoTracking( locGuid.Value );
                    if ( location != null )
                    {
                        return location.ToString();
                    }
                }
            }

            return string.Empty;
        }

        #endregion

        #region Edit Control

        /// <inheritdoc/>
        public override string GetPublicValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            return GetTextValue( privateValue, privateConfigurationValues );
        }

        /// <inheritdoc/>
        public override string GetPrivateEditValue( string publicValue, Dictionary<string, string> privateConfigurationValues )
        {
            var locationValue = publicValue.FromJsonOrNull<ListItemBag>();
            var addressValue = publicValue.FromJsonOrNull<AddressControlBag>();

            if ( locationValue != null && locationValue.Value.IsNotNullOrWhiteSpace() )
            {
                return locationValue.Value;
            }
            else if ( addressValue != null )
            {
                // Check if we have any values.
                if ( string.IsNullOrWhiteSpace( addressValue.Street1 )
                     && string.IsNullOrWhiteSpace( addressValue.Street2 )
                     && string.IsNullOrWhiteSpace( addressValue.City )
                     && string.IsNullOrWhiteSpace( addressValue.PostalCode ) )
                {
                    return string.Empty;
                }

                var globalAttributesCache = GlobalAttributesCache.Get();

                using ( var rockContext = new RockContext() )
                {
                    try
                    {
                        var locationService = new LocationService( rockContext );
                        var location = locationService.Get( addressValue.Street1,
                            addressValue.Street2,
                            addressValue.City,
                            addressValue.State,
                            addressValue.PostalCode,
                            addressValue.Country.IfEmpty( globalAttributesCache.OrganizationCountry ) );

                        if ( location == null )
                        {
                            return string.Empty;
                        }

                        return location.Guid.ToString();
                    }
                    catch ( Exception )
                    {
                        return addressValue.ToJson() ?? string.Empty;
                    }
                }
            }
            else if(publicValue.IsNotNullOrWhiteSpace())
            {
                if ( publicValue.Contains( "POINT" ) || publicValue.Contains( "POLYGON" ) )
                {
                    DbGeography geoPoint = DbGeography.FromText( publicValue.FromJsonOrNull<string>() );
                    var location = new LocationService( new RockContext() ).GetByGeoPoint( geoPoint );
                    return location.Guid.ToString();
                }
            }

            return string.Empty;
        }

        /// <inheritdoc/>
        public override string GetPublicEditValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( Guid.TryParse( privateValue, out Guid guid ) )
            {
                var location = new LocationService( new RockContext() ).Get( guid );
                if ( location != null )
                {
                    if ( location.IsNamedLocation )
                    {
                        return new ListItemBag()
                        {
                            Value = location.Guid.ToString(),
                            Text = location.Name,
                        }.ToCamelCaseJson( false, true );
                    }
                    else if ( !string.IsNullOrWhiteSpace( location.GetFullStreetAddress().Replace( ",", string.Empty ) ) )
                    {
                        return new AddressControlBag
                        {
                            Street1 = location.Street1,
                            Street2 = location.Street2,
                            City = location.City,
                            State = location.State,
                            PostalCode = location.PostalCode,
                            Country = location.Country
                        }.ToCamelCaseJson( false, true );
                    }
                    else if ( location.GeoPoint != null )
                    {
                        return location.GeoPoint.AsText().ToJson();
                    }
                    else if ( location.GeoFence != null )
                    {
                        return location.GeoFence.AsText().ToJson();
                    }
                }
                else
                {
                    var partialAddress = privateValue.FromJsonOrNull<AddressControlBag>();
                    if ( partialAddress != null )
                    {
                        return partialAddress.ToCamelCaseJson( false, true );
                    }
                    else
                    {
                        var globalAttributesCache = GlobalAttributesCache.Get();

                        return new AddressControlBag
                        {
                            State = globalAttributesCache.OrganizationState,
                            Country = globalAttributesCache.OrganizationCountry
                        }.ToCamelCaseJson( false, true );
                    }
                }
            }

            return string.Empty;
        }

        #endregion

        #region Entity Methods

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public IEntity GetEntity( string value )
        {
            return GetEntity( value, null );
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public IEntity GetEntity( string value, RockContext rockContext )
        {
            Guid? guid = value.AsGuidOrNull();
            if ( guid.HasValue )
            {
                rockContext = rockContext ?? new RockContext();
                return new LocationService( rockContext ).Get( guid.Value );
            }

            return null;
        }

        #endregion

        #region Filter Control

        /// <summary>
        /// Determines whether this filter has a filter control
        /// </summary>
        /// <returns></returns>
        public override bool HasFilterControl()
        {
            return false;
        }

        #endregion

        #region Persistence

        /// <inheritdoc/>
        public override PersistedValues GetPersistedValues( string privateValue, Dictionary<string, string> privateConfigurationValues, IDictionary<string, object> cache )
        {
            var textValue = string.Empty;

            if ( privateValue.IsNotNullOrWhiteSpace() )
            {
                var locGuid = privateValue.AsGuidOrNull();
                if ( locGuid.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var service = new LocationService( rockContext );
                        var location = service.GetNoTracking( new Guid( privateValue ) );
                        if ( location != null )
                        {
                            textValue = location.ToString();
                        }
                    }
                }
            }

            return new PersistedValues
            {
                TextValue = textValue,
                CondensedTextValue = textValue.Truncate( CondensedTruncateLength ),
                HtmlValue = textValue.EncodeHtml(),
                CondensedHtmlValue = textValue.Truncate( CondensedTruncateLength ).EncodeHtml()
            };
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
                var locationId = new LocationService( rockContext ).GetId( guid.Value );

                if ( !locationId.HasValue )
                {
                    return null;
                }

                return new List<ReferencedEntity>
                {
                    new ReferencedEntity( EntityTypeCache.GetId<Location>().Value, locationId.Value )
                };
            }
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            // This field type references various properties of a Location and
            // should have its persisted values updated when changed.
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<Location>().Value, nameof( Location.Name ) ),
                new ReferencedProperty( EntityTypeCache.GetId<Location>().Value, nameof( Location.Street1 ) ),
                new ReferencedProperty( EntityTypeCache.GetId<Location>().Value, nameof( Location.Street2 ) ),
                new ReferencedProperty( EntityTypeCache.GetId<Location>().Value, nameof( Location.City ) ),
                new ReferencedProperty( EntityTypeCache.GetId<Location>().Value, nameof( Location.State ) ),
                new ReferencedProperty( EntityTypeCache.GetId<Location>().Value, nameof( Location.PostalCode ) ),
                new ReferencedProperty( EntityTypeCache.GetId<Location>().Value, nameof( Location.Country ) ),
                new ReferencedProperty( EntityTypeCache.GetId<Location>().Value, nameof( Location.GeoPoint ) ),
                new ReferencedProperty( EntityTypeCache.GetId<Location>().Value, nameof( Location.GeoFence ) )
            };
        }

        #endregion

        #region WebForms
#if WEBFORMS

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var configKeys = base.ConfigurationKeys();
            configKeys.Add( ConfigurationKey.AllowedPickerMode );
            configKeys.Add( CURRENT_PICKER_MODE );

            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            // Control to pick the available location types
            var cblAvailableLocationTypes = new RockCheckBoxList();
            cblAvailableLocationTypes.RepeatDirection = RepeatDirection.Horizontal;
            cblAvailableLocationTypes.AutoPostBack = true;
            cblAvailableLocationTypes.SelectedIndexChanged += OnQualifierUpdated;
            cblAvailableLocationTypes.Label = "Available Location Types";
            cblAvailableLocationTypes.Help = "Select the location types that can be used by the Location Picker.";

            var locationTypes = ( LocationPickerMode[] ) Enum.GetValues( typeof( LocationPickerMode ) );
            foreach ( LocationPickerMode locationType in locationTypes )
            {
                if ( locationType != LocationPickerMode.None && locationType != LocationPickerMode.All )
                {
                    cblAvailableLocationTypes.Items.Add( new ListItem( locationType.ConvertToString(), locationType.ConvertToInt().ToString() ) );
                }
            }

            controls.Add( cblAvailableLocationTypes );

            // Control to pick the default location type
            var rblDefaultLocationType = new RockRadioButtonList();
            rblDefaultLocationType.RepeatDirection = RepeatDirection.Horizontal;
            rblDefaultLocationType.AutoPostBack = true;
            rblDefaultLocationType.SelectedIndexChanged += OnQualifierUpdated;
            rblDefaultLocationType.Label = "Default Location Type";
            rblDefaultLocationType.Help = "Select the location type that is initially displayed.";

            foreach ( LocationPickerMode locationType in locationTypes )
            {
                if ( locationType != LocationPickerMode.None && locationType != LocationPickerMode.All )
                {
                    rblDefaultLocationType.Items.Add( new ListItem( locationType.ConvertToString(), locationType.ConvertToInt().ToString() ) );
                }
            }

            controls.Add( rblDefaultLocationType );

            return controls;
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            Dictionary<string, ConfigurationValue> configurationValues = new Dictionary<string, ConfigurationValue>();
            configurationValues.Add( ConfigurationKey.AllowedPickerMode, new ConfigurationValue( "Available Location Types", "Select the location types that can be used by the Location Picker.", string.Empty ) );
            configurationValues.Add( CURRENT_PICKER_MODE, new ConfigurationValue( "Default Location Type", "Select the location type that is initially displayed.", string.Empty ) );

            if ( controls != null && controls.Count == 2 )
            {
                if ( controls[0] != null && controls[0] is RockCheckBoxList )
                {
                    configurationValues[ConfigurationKey.AllowedPickerMode].Value = ( ( RockCheckBoxList ) controls[0] ).SelectedValues.AsDelimited( "," );
                }

                if ( controls[1] != null && controls[1] is RockRadioButtonList )
                {
                    configurationValues[CURRENT_PICKER_MODE].Value = ( ( RockRadioButtonList ) controls[1] ).SelectedValue;
                }
            }

            return configurationValues;
        }

        /// <summary>
        /// Sets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="configurationValues">The configuration values.</param>
        public override void SetConfigurationValues( List<Control> controls, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( controls != null && controls.Count == 2 )
            {
                if ( controls[0] != null && controls[0] is RockCheckBoxList && configurationValues.ContainsKey( ConfigurationKey.AllowedPickerMode ) )
                {
                    var selectedValues = configurationValues[ConfigurationKey.AllowedPickerMode].Value?.Split( ',' );
                    if ( selectedValues != null )
                    {
                        ( ( RockCheckBoxList ) controls[0] ).Items.Cast<ListItem>().ToList().ForEach( li => li.Selected = selectedValues.Any( v => v == li.Value ) );
                    }
                }

                if ( controls[1] != null && controls[1] is RockRadioButtonList && configurationValues.ContainsKey( CURRENT_PICKER_MODE ) )
                {
                    var selectedValue = configurationValues[CURRENT_PICKER_MODE].Value;
                    if ( selectedValue != null )
                    {
                        ( ( RockRadioButtonList ) controls[1] ).SelectedValue = configurationValues[CURRENT_PICKER_MODE].Value;
                    }
                }
            }
        }

        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            return !condensed
                ? GetTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) )
                : GetCondensedTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) );
        }

        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value ( as Guid )
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            // If there are no configuration values then set to legacy defaults
            if ( !configurationValues.Any() )
            {
                return new LocationPicker { ID = id, AllowedPickerModes = LocationPickerMode.All };
            }

            // Default allowed location picker modes is all
            LocationPickerMode allowedPickerModes = LocationPickerMode.All;

            // Get the selected current picker mode, use address if there isn't one specified. This is the mode the picker will start with unless the location specifies a different location type.

            var currentPickerMode = configurationValues.ContainsKey( CURRENT_PICKER_MODE ) ? configurationValues[CURRENT_PICKER_MODE].Value.ConvertToEnumOrNull<LocationPickerMode>() ?? LocationPickerMode.Address : LocationPickerMode.Address;

            string[] allowedPickerModesConfig = null;
            if ( configurationValues.ContainsKey( ConfigurationKey.AllowedPickerMode ) && configurationValues[ConfigurationKey.AllowedPickerMode].Value.IsNotNullOrWhiteSpace() )
            {
                allowedPickerModesConfig = configurationValues[ConfigurationKey.AllowedPickerMode].Value.Split( ',' );
            }

            if ( allowedPickerModesConfig != null && allowedPickerModesConfig.Any() )
            {
                allowedPickerModes = LocationPickerMode.None;

                foreach ( var allowedPickerMode in allowedPickerModesConfig )
                {
                    allowedPickerModes |= allowedPickerMode.ConvertToEnum<LocationPickerMode>();
                }
            }

            return new LocationPicker
            {
                ID = id,
                CurrentPickerMode = currentPickerMode,
                AllowedPickerModes = allowedPickerModes
            };

        }

        /// <summary>
        /// Reads new values entered by the user for the field ( as Guid )
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var picker = control as LocationPicker;

            if ( picker != null )
            {
                return picker.Location?.Guid.ToString() ?? string.Empty;
            }

            return null;
        }

        /// <summary>
        /// Sets the value. ( as Guid )
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var picker = control as LocationPicker;
            if ( picker != null )
            {
                Guid? locationGuid = value.AsGuidOrNull();
                if ( locationGuid.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var location = new LocationService( rockContext ).Get( locationGuid.Value );
                        picker.SetBestPickerModeForLocation( location );
                        picker.Location = location;
                    }
                }
                else
                {
                    picker.SetBestPickerModeForLocation( null );
                    picker.Location = null;
                }
            }
        }

        /// <summary>
        /// Gets the edit value as the IEntity.Id
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public int? GetEditValueAsEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            Guid guid = GetEditValue( control, configurationValues ).AsGuid();
            var itemId = new LocationService( new RockContext() ).GetId( guid );
            return itemId;
        }

        /// <summary>
        /// Sets the edit value from IEntity.Id value
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        public void SetEditValueFromEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues, int? id )
        {
            var itemGuid = new LocationService( new RockContext() ).GetGuid( id ?? 0 );
            SetEditValue( control, configurationValues, itemGuid?.ToString() );
        }

        /// <summary>
        /// Creates the control needed to filter (query) values using this field type.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public override System.Web.UI.Control FilterControl( System.Collections.Generic.Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, Rock.Reporting.FilterMode filterMode )
        {
            // This field type does not support filtering
            return null;
        }

#endif
        #endregion
    }
}
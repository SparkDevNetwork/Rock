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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to save and display a location value
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms )]
    public class LocationFieldType : FieldType, IEntityFieldType
    {
        #region Configuration

        private const string ALLOWED_PICKER_MODES = "allowedPickerModes";
        private const string CURRENT_PICKER_MODE = "currentPickerMode";

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var configKeys = base.ConfigurationKeys();
            configKeys.Add( ALLOWED_PICKER_MODES );
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
                if ( locationType != LocationPickerMode.None && locationType !=LocationPickerMode.All)
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
            configurationValues.Add( ALLOWED_PICKER_MODES, new ConfigurationValue( "Available Location Types", "Select the location types that can be used by the Location Picker.", string.Empty ) );
            configurationValues.Add( CURRENT_PICKER_MODE, new ConfigurationValue( "Default Location Type", "Select the location type that is initially displayed.", string.Empty ) );

            if ( controls != null && controls.Count == 2)
            {
                if ( controls[0] != null && controls[0] is RockCheckBoxList )
                {
                    configurationValues[ALLOWED_PICKER_MODES].Value = ( ( RockCheckBoxList ) controls[0] ).SelectedValues.AsDelimited( "," );
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
            if ( controls != null && controls.Count == 2)
            {
                if ( controls[0] != null && controls[0] is RockCheckBoxList && configurationValues.ContainsKey( ALLOWED_PICKER_MODES ) )
                {
                    var selectedValues = configurationValues[ALLOWED_PICKER_MODES].Value?.Split( ',' );
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

        #endregion Configuration

        #region Formatting

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
            string formattedValue = string.Empty;

            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                Guid? locGuid = value.AsGuidOrNull();
                if ( locGuid.HasValue )
                {
                    // Check to see if this is the org address first (to avoid db read)
                    var globalAttributesCache = GlobalAttributesCache.Get();
                    var orgLocGuid = globalAttributesCache.GetValue( "OrganizationAddress" ).AsGuidOrNull();
                    if ( orgLocGuid.HasValue && orgLocGuid.Value == locGuid.Value )
                    {
                        return globalAttributesCache.OrganizationLocationFormatted;
                    }
                }

                using ( var rockContext = new RockContext() )
                {
                    var service = new LocationService( rockContext );
                    var location = service.GetNoTracking( new Guid( value ) );
                    if ( location != null )
                    {
                        formattedValue = location.ToString();
                    }
                }
            }

            return base.FormatValue( parentControl, formattedValue, null, condensed );
        }

        #endregion

        #region Edit Control

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
            var currentPickerMode = configurationValues[CURRENT_PICKER_MODE].Value.ConvertToEnumOrNull<LocationPickerMode>() ?? LocationPickerMode.Address;

            string[] allowedPickerModesConfig = null;
            if ( configurationValues[ALLOWED_PICKER_MODES].Value.IsNotNullOrWhiteSpace() )
            {
                allowedPickerModesConfig = configurationValues[ALLOWED_PICKER_MODES].Value.Split( ',' );
            }

            if( allowedPickerModesConfig != null && allowedPickerModesConfig.Any() )
            {
                allowedPickerModes = LocationPickerMode.None;

                foreach( var allowedPickerMode in allowedPickerModesConfig )
                {
                    allowedPickerModes |= allowedPickerMode.ConvertToEnum<LocationPickerMode>();
                }
            }
            
            return new LocationPicker {
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

        #endregion

        #region Entity Methods
        
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

        /// <summary>
        /// Determines whether this filter has a filter control
        /// </summary>
        /// <returns></returns>
        public override bool HasFilterControl()
        {
            return false;
        }

        #endregion

    }
}
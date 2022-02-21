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

using System;
using System.Collections.Generic;
using System.Web.UI;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Class that represents the LocationList field type.
    /// </summary>
    /// <seealso cref="Rock.Field.FieldType" />
    [RockPlatformSupport( Utility.RockPlatform.WebForms )]
    public class LocationListFieldType : FieldType, IEntityFieldType
    {
        #region Configuration
        /// <summary>
        /// A class that has all of the configuration keys for Location List Field Type.
        /// </summary>
        public static class ConfigurationKey
        {
            /// <summary>
            /// The location type
            /// </summary>
            public const string LocationType = "LocationType";

            /// <summary>
            /// The parent location
            /// </summary>
            public const string ParentLocation = "ParentLocation";

            /// <summary>
            /// The allow adding new locations
            /// </summary>
            public const string AllowAddingNewLocations = "AllowAddingNewLocations";

            /// <summary>
            /// The show city state
            /// </summary>
            public const string ShowCityState = "ShowCityState";

            /// <summary>
            /// The address required
            /// </summary>
            public const string AddressRequired = "AddressRequired";
        }

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var configurationKeys = base.ConfigurationKeys();

            configurationKeys.Add( ConfigurationKey.LocationType );
            configurationKeys.Add( ConfigurationKey.ParentLocation );
            configurationKeys.Add( ConfigurationKey.AllowAddingNewLocations );
            configurationKeys.Add( ConfigurationKey.ShowCityState );
            configurationKeys.Add( ConfigurationKey.AddressRequired );

            return configurationKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            var locationTypeDefinedType = DefinedTypeCache.Get( SystemGuid.DefinedType.LOCATION_TYPE.AsGuid() );

            var dvpLocationType = new DefinedValuePicker();
            controls.Add( dvpLocationType );
            dvpLocationType.AutoPostBack = true;
            dvpLocationType.SelectedIndexChanged += OnQualifierUpdated;
            dvpLocationType.Label = "Location Type";
            dvpLocationType.DefinedTypeId = locationTypeDefinedType.Id;

            var lpParentLocation = new LocationPicker();
            controls.Add( lpParentLocation );
            lpParentLocation.Label = "Parent Location";
            lpParentLocation.Required = true;
            lpParentLocation.AllowedPickerModes = LocationPickerMode.Named;
            lpParentLocation.CurrentPickerMode = LocationPickerMode.Named;
            lpParentLocation.SelectLocation += OnQualifierUpdated;

            var cbAllowAddingNewLocations = new RockCheckBox();
            controls.Add( cbAllowAddingNewLocations );
            cbAllowAddingNewLocations.AutoPostBack = true;
            cbAllowAddingNewLocations.CheckedChanged += OnQualifierUpdated;
            cbAllowAddingNewLocations.Label = "Allow Adding New Locations";

            var cbShowCityState = new RockCheckBox();
            controls.Add( cbShowCityState );
            cbShowCityState.AutoPostBack = true;
            cbShowCityState.CheckedChanged += OnQualifierUpdated;
            cbShowCityState.Label = "Show City / State";

            var cbAddressRequired = new RockCheckBox();
            controls.Add( cbAddressRequired );
            cbAddressRequired.AutoPostBack = true;
            cbAddressRequired.CheckedChanged += OnQualifierUpdated;
            cbAddressRequired.Label = "Address Required";

            return controls;
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            var configurationValues = new Dictionary<string, ConfigurationValue>();
            configurationValues.Add( ConfigurationKey.LocationType, new ConfigurationValue( "Location Type", string.Empty, string.Empty ) );
            configurationValues.Add( ConfigurationKey.AddressRequired, new ConfigurationValue( "Address Required", string.Empty, "false" ) );
            configurationValues.Add( ConfigurationKey.AllowAddingNewLocations, new ConfigurationValue( "Allow Adding New Locations", string.Empty, "false" ) );
            configurationValues.Add( ConfigurationKey.ParentLocation, new ConfigurationValue( "Parent Location", string.Empty, string.Empty ) );
            configurationValues.Add( ConfigurationKey.ShowCityState, new ConfigurationValue( "Show City / State", string.Empty, "false" ) );

            if ( controls.Count > 0 && controls[0] is DefinedValuePicker )
            {
                configurationValues[ConfigurationKey.LocationType].Value = ( ( DefinedValuePicker ) controls[0] ).SelectedDefinedValueId.ToString();
            }

            if ( controls.Count > 1 && controls[1] is LocationPicker )
            {
                configurationValues[ConfigurationKey.ParentLocation].Value = ( ( LocationPicker ) controls[1] ).Location?.Id.ToStringSafe();
            }

            if ( controls.Count > 2 && controls[2] is RockCheckBox )
            {
                configurationValues[ConfigurationKey.AllowAddingNewLocations].Value = ( ( RockCheckBox ) controls[2] ).Checked.ToString();
            }

            if ( controls.Count > 3 && controls[3] is RockCheckBox )
            {
                configurationValues[ConfigurationKey.ShowCityState].Value = ( ( RockCheckBox ) controls[3] ).Checked.ToString();
            }

            if ( controls.Count > 4 && controls[4] is RockCheckBox )
            {
                configurationValues[ConfigurationKey.AddressRequired].Value = ( ( RockCheckBox ) controls[4] ).Checked.ToString();
            }

            return configurationValues;
        }

        /// <summary>
        /// Sets the configuration value.
        /// </summary>
        /// <param name="controls"></param>
        /// <param name="configurationValues"></param>
        public override void SetConfigurationValues( List<Control> controls, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( configurationValues != null )
            {
                if ( controls.Count > 0 && controls[0] is DefinedValuePicker && configurationValues.ContainsKey( ConfigurationKey.LocationType ) )
                {
                    ( ( DefinedValuePicker ) controls[0] ).SetValue( configurationValues[ConfigurationKey.LocationType].Value );
                }

                if ( controls.Count > 1 && controls[1] is LocationPicker && configurationValues.ContainsKey( ConfigurationKey.ParentLocation ) )
                {
                    var locationId = configurationValues[ConfigurationKey.ParentLocation].Value.AsIntegerOrNull();
                    Location location = null;
                    if ( locationId != null )
                    {
                        location = GetLocationById( locationId.Value );
                    }

                    ( ( LocationPicker ) controls[1] ).Location = location;
                }

                if ( controls.Count > 2 && controls[2] is RockCheckBox && configurationValues.ContainsKey( ConfigurationKey.AllowAddingNewLocations ) )
                {
                    ( ( RockCheckBox ) controls[2] ).Checked = configurationValues[ConfigurationKey.AllowAddingNewLocations].Value.AsBoolean();
                }

                if ( controls.Count > 3 && controls[3] is RockCheckBox && configurationValues.ContainsKey( ConfigurationKey.ShowCityState ) )
                {
                    ( ( RockCheckBox ) controls[3] ).Checked = configurationValues[ConfigurationKey.ShowCityState].Value.AsBoolean();
                }

                if ( controls.Count > 4 && controls[4] is RockCheckBox && configurationValues.ContainsKey( ConfigurationKey.AddressRequired ) )
                {
                    ( ( RockCheckBox ) controls[4] ).Checked = configurationValues[ConfigurationKey.AddressRequired].Value.AsBoolean();
                }
            }
        }
        #endregion

        #region Edit Control
        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            var locationList = new LocationList
            {
                ID = id,
                LocationTypeValueId = configurationValues.GetConfigurationValueAsString( ConfigurationKey.LocationType ).AsIntegerOrNull(),
                ParentLocationId = configurationValues.GetConfigurationValueAsString( ConfigurationKey.ParentLocation ).AsIntegerOrNull(),
                AllowAdd = configurationValues.GetConfigurationValueAsString( ConfigurationKey.AllowAddingNewLocations ).AsBoolean(),
                ShowCityState = configurationValues.GetConfigurationValueAsString( ConfigurationKey.ShowCityState ).AsBoolean(),
                IsAddressRequired = configurationValues.GetConfigurationValueAsString( ConfigurationKey.AddressRequired ).AsBoolean(),
            };
            return locationList;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var locationList = control as LocationList;

            if ( locationList == null )
            {
                return null;
            }

            var locationId = locationList.SelectedValue.AsIntegerOrNull();
            if ( locationId == null )
            {
                return null;
            }

            var location = GetLocationById( locationId.Value );
            if ( location == null )
            {
                return null;
            }

            return location.Guid.ToString();
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var locationList = control as LocationList;

            if ( locationList == null )
            {
                return;
            }

            var locationGuid = value.AsGuid();
            var location = GetLocationByGuid( locationGuid );

            if ( location != null )
            {
                locationList.SelectedValue = location.Id.ToString();
            }
        }
        #endregion

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
            var locationGuid = value.AsGuid();
            var location = GetLocationByGuid( locationGuid );
            if ( location == null )
            {
                return string.Empty;
            }

            if ( configurationValues.GetConfigurationValueAsString( ConfigurationKey.ShowCityState ).AsBoolean() )
            {
                value = $"{location.Name} ({location.City}, {location.State})";
            }
            else
            {
                value = location.Name;
            }

            return base.FormatValue( parentControl, value, configurationValues, condensed );
        }
        #endregion

        #region IEntityFieldType implementation
        /// <summary>
        /// Gets the edit value as the IEntity.Id
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public int? GetEditValueAsEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var guid = GetEditValue( control, configurationValues ).AsGuid();
            var location = GetLocationByGuid( guid );

            if ( location == null )
            {
                return null;
            }

            return location.Id;
        }

        /// <summary>
        /// Sets the edit value from IEntity.Id value
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void SetEditValueFromEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues, int? id )
        {
            var locationEditValue = string.Empty;
            if ( id != null )
            {
                var location = GetLocationById( id.Value );
                if ( location != null )
                {
                    locationEditValue = location.Guid.ToString();
                }
            }

            SetEditValue( control, configurationValues, locationEditValue );
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEntity GetEntity( string value )
        {
            var guid = value.AsGuid();
            return GetLocationByGuid( guid );
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEntity GetEntity( string value, RockContext rockContext )
        {
            var guid = value.AsGuid();
            return GetLocationByGuid( guid, rockContext );
        }
        #endregion

        private Location GetLocationByGuid( Guid guid )
        {
            if ( guid.IsEmpty() )
            {
                return null;
            }

            using ( var rockContext = new RockContext() )
            {
                return GetLocationByGuid( guid, rockContext );
            }
        }

        private Location GetLocationByGuid( Guid guid, RockContext rockContext )
        {
            if ( guid.IsEmpty() )
            {
                return null;
            }

            var locationService = new LocationService( rockContext );
            return locationService.Get( guid );
        }

        private Location GetLocationById( int id )
        {
            using ( var rockContext = new RockContext() )
            {
                return GetLocationById( id, rockContext );
            }
        }

        private Location GetLocationById( int id, RockContext rockContext )
        {
            var locationService = new LocationService( rockContext );
            return locationService.Get( id );
        }
    }
}

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

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to save and display an address value
    /// </summary>
    [Rock.Attribute.RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    public class AddressFieldType : FieldType, IEntityFieldType
    {

        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string value, Dictionary<string, string> configurationValues )
        {
            string formattedValue = string.Empty;

            if ( !string.IsNullOrWhiteSpace( value ) )
            {
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

            return formattedValue;
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
            string formattedValue = GetTextValue( value, configurationValues.ToDictionary( k => k.Key, k => k.Value.Value ) );

            return base.FormatValue( parentControl, formattedValue, null, condensed );
        }

        #endregion

        #region Edit Control

        /// <inheritdoc/>
        public override string GetClientValue( string value, Dictionary<string, string> configurationValues )
        {
            var guid = value.AsGuidOrNull();
            Location location = null;

            if ( guid.HasValue )
            {
                location = new LocationService( new RockContext() ).Get( guid.Value );
            }

            if ( location != null )
            {
                return new AddressFieldValue
                {
                    Street1 = location.Street1,
                    Street2 = location.Street2,
                    City = location.City,
                    State = location.State,
                    PostalCode = location.PostalCode,
                    Country = location.Country
                }.ToCamelCaseJson( false, true );
            }
            else
            {
                var globalAttributesCache = GlobalAttributesCache.Get();

                return new AddressFieldValue
                {
                    State = globalAttributesCache.OrganizationState,
                    Country = globalAttributesCache.OrganizationCountry
                }.ToCamelCaseJson( false, true );
            }
        }

        /// <inheritdoc/>
        public override string GetValueFromClient( string clientValue, Dictionary<string, string> configurationValues )
        {
            var addressValue = clientValue.FromJsonOrNull<AddressFieldValue>();

            if (addressValue == null)
            {
                return string.Empty;
            }

            var globalAttributesCache = GlobalAttributesCache.Get();

            using ( var rockContext = new RockContext() )
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
            return new AddressControl { ID = id, SetDefaultValues = false };
        }

        /// <summary>
        /// Reads new values entered by the user for the field ( as Guid )
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var addressControl = control as AddressControl;
            using ( var rockContext = new RockContext() )
            {
                var locationService = new LocationService( rockContext );
                string result = null;

                if ( addressControl != null
                     && addressControl.HasValue )
                {
                    var guid = Guid.Empty;

                    var location = locationService.Get( addressControl.Street1, addressControl.Street2, addressControl.City, addressControl.State, addressControl.PostalCode, addressControl.Country );

                    if ( location != null )
                    {
                        guid = location.Guid;
                    }

                    result = guid.ToString();
                }

                return result;
            }
        }

        /// <summary>
        /// Sets the value. ( as Guid )
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            if ( value != null )
            {
                var addressControl = control as AddressControl;

                if ( addressControl != null )
                {
                    Guid guid;
                    Guid.TryParse( value, out guid );
                    var location = new LocationService( new RockContext() ).Get( guid );
                    if ( location != null )
                    {
                        /* 22-Sep-2020 - SK
                          Always Set Country first as it loads all the related state in the dropdown before it's being set.
                        */
                        addressControl.Country = location.Country;
                        addressControl.Street1 = location.Street1;
                        addressControl.Street2 = location.Street2;
                        addressControl.City = location.City;
                        addressControl.State = location.State;
                        addressControl.PostalCode = location.PostalCode;
                        addressControl.County = location.County;
                    }
                    else
                    {
                        addressControl.Country = addressControl.GetDefaultCountry();
                        addressControl.Street1 = string.Empty;
                        addressControl.Street2 = string.Empty;
                        addressControl.City = string.Empty;
                        addressControl.County = string.Empty;
                        addressControl.State = addressControl.GetDefaultState();
                        addressControl.PostalCode = string.Empty;
                    }
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
            var item = new LocationService( new RockContext() ).Get( guid );
            return item != null ? item.Id : (int?)null;
        }

        /// <summary>
        /// Sets the edit value from IEntity.Id value
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        public void SetEditValueFromEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues, int? id )
        {
            var item = new LocationService( new RockContext() ).Get( id ?? 0 );
            string guidValue = item != null ? item.Guid.ToString() : string.Empty;
            SetEditValue( control, configurationValues, guidValue );
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

        private class AddressFieldValue
        {
            public string Street1 { get; set; }

            public string Street2 { get; set; }

            public string City { get; set; }

            public string State { get; set; }

            public string PostalCode { get; set; }

            public string Country { get; set; }
        }
    }
}
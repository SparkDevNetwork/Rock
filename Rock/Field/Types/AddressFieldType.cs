﻿// <copyright>
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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to save and display an address value
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [IconSvg( @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 16 16""><g><path d=""M8,1A5.25,5.25,0,0,0,2.75,6.25c0,2.12.74,2.71,4.71,8.47a.66.66,0,0,0,1.08,0c4-5.76,4.71-6.35,4.71-8.47A5.25,5.25,0,0,0,8,1ZM8,13.19c-.48-.7-.91-1.31-1.3-1.85C4.32,8,4.06,7.53,4.06,6.25a3.94,3.94,0,0,1,7.88,0c0,1.28-.26,1.7-2.64,5.09C8.91,11.86,8.48,12.5,8,13.19ZM8,4a2.19,2.19,0,1,0,2.19,2.19A2.19,2.19,0,0,0,8,4Z""/></g></svg>" )]
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
        public override string GetPublicValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var guid = privateValue.AsGuidOrNull();
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
        public override string GetPrivateEditValue( string publicValue, Dictionary<string, string> privateConfigurationValues )
        {
            var addressValue = publicValue.FromJsonOrNull<AddressFieldValue>();

            if (addressValue == null)
            {
                return string.Empty;
            }

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
            if ( addressControl == null || !addressControl.HasValue )
            {
                return null;
            }

            using ( var rockContext = new RockContext() )
            {
                var locationService = new LocationService( rockContext );
                string result = null;

                var editLocation = new Location();
                addressControl.GetValues( editLocation );
                var addressIsValid = locationService.ValidateAddressRequirements( editLocation, out _ );

                // Only get a LocationGuid if the AddressControl has a value and has met the ValidateAddressRequirements rules
                if ( addressIsValid )
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
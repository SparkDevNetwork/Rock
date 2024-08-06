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
using Rock.Model;
using Rock.Web.UI.Controls;
using System.Collections.Generic;
using System.Linq;


#if WEBFORMS
using System.Web.UI;
#endif

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to save and display a phone number
    /// </summary>
    [FieldTypeUsage( FieldTypeUsage.Advanced )]
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [IconSvg( @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 16 16""><path d=""M1.52,10.6l3-1.27a.86.86,0,0,1,1,.24l1.21,1.48a9.59,9.59,0,0,0,4.36-4.36L9.58,5.48a.85.85,0,0,1-.25-1l1.27-3a.87.87,0,0,1,1-.5l2.76.64a.85.85,0,0,1,.66.83A12.52,12.52,0,0,1,2.49,15a.85.85,0,0,1-.83-.66L1,11.58A.87.87,0,0,1,1.52,10.6Z""/></svg>" )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.PHONE_NUMBER )]
    public class PhoneNumberFieldType : FieldType
    {
        /// <inheritdoc/>
        public override string GetPublicEditValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            // Parse the input value to obtain the country code and the remaining digits of the phone number.
            string countryCodePart;
            string numberPart;

            var isValid = PhoneNumber.TryParseNumber( privateValue, out countryCodePart, out numberPart );
            if ( isValid )
            {
                // Reformat the number according to the country code.
                var formattedNumber = PhoneNumber.FormattedNumber( countryCodePart, numberPart, includeCountryCode: false );
                if ( !string.IsNullOrWhiteSpace( formattedNumber ) )
                {
                    numberPart = formattedNumber;
                }

                return new PhoneNumberFieldValue
                {
                    CountryCode = countryCodePart,
                    Number = numberPart
                }.ToCamelCaseJson( false, true );
            }

            return new PhoneNumberFieldValue
            {
                CountryCode = GetDefaultCountryCode(),
                Number = numberPart
            }.ToCamelCaseJson( false, true );
        }

        /// <inheritdoc/>
        public override string GetPrivateEditValue( string publicValue, Dictionary<string, string> privateConfigurationValues )
        {
            var number = publicValue.FromJsonOrNull<PhoneNumberFieldValue>();
            // Store the value as a formatted phone number.
            return GetFormattedPhoneNumber( number );
        }

        /// <inheritdoc/>
        public override string GetPublicValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            return base.GetPublicValue( privateValue, privateConfigurationValues );
        }

        /// <summary>
        /// Convert a phone number object to a formatted phone number string
        /// </summary>
        /// <param name="phone">A PhoneNumberFieldValue.</param>
        /// <returns>A formatted string of the given phone number</returns>
        private string GetFormattedPhoneNumber( PhoneNumberFieldValue phone )
        {
            if ( phone == null )
            {
                return null;
            }

            // Include the country code only if it is not the default value.
            var countryCode = phone.CountryCode;
            var includeCountryCode = ( countryCode != GetDefaultCountryCode() );

            var value = PhoneNumber.FormattedNumber( countryCode, phone.Number, includeCountryCode );
            return value;
        }

        #region WebForms
#if WEBFORMS

        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( System.Collections.Generic.Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            return new PhoneNumberBox { ID = id };
        }

        /// <inheritdoc/>
        public override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            return GetTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) );
        }

        /// <inheritdoc/>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var phoneControl = control as PhoneNumberBox;
            if ( phoneControl == null )
            {
                return null;
            }

            // Store the value as a formatted phone number.
            return GetFormattedPhoneNumber( new PhoneNumberFieldValue { Number = phoneControl.Number, CountryCode = phoneControl.CountryCode } );
        }

        /// <inheritdoc/>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var phoneControl = control as PhoneNumberBox;
            if ( phoneControl == null )
            {
                return;
            }

            // Parse the input value to obtain the country code and the remaining digits of the phone number.
            string countryCodePart;
            string numberPart;

            var isValid = PhoneNumber.TryParseNumber( value, out countryCodePart, out numberPart );
            if ( isValid )
            {
                // Reformat the number according to the country code.
                var formattedNumber = PhoneNumber.FormattedNumber( countryCodePart, numberPart, includeCountryCode: false );
                if ( !string.IsNullOrWhiteSpace( formattedNumber ) )
                {
                    numberPart = formattedNumber;
                }
                phoneControl.CountryCode = countryCodePart;
                phoneControl.Number = numberPart;
            }
            else
            {
                phoneControl.CountryCode = GetDefaultCountryCode();
                phoneControl.Number = numberPart;
            }
        }
#endif
        #endregion

        private string _defaultCountryCode = null;

        private string GetDefaultCountryCode()
        {
            if ( _defaultCountryCode == null )
            {
                _defaultCountryCode = PhoneNumber.DefaultCountryCode();
            }
            return _defaultCountryCode;
        }

        private class PhoneNumberFieldValue
        {
            public string Number { get; set; }
            public string CountryCode { get; set; }
        }
    }
}
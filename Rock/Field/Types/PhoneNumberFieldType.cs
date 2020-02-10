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
using System.Collections.Generic;
using System.Web.UI;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to save and display a phone number
    /// </summary>
    public class PhoneNumberFieldType : FieldType
    {
        private class JsonPhoneNumber
        {
            public string Number { get; set; }
            public string CountryCode { get; set; }

            public static JsonPhoneNumber GetJsonPhoneNumberFromString( string value )
            {
                JsonPhoneNumber jsonPhoneNumber = null;
                try
                {
                    jsonPhoneNumber = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonPhoneNumber>( value );
                }
                catch
                {
                    // Not a valid JSON object so handle like phone number.
                }

                if ( jsonPhoneNumber == null )
                {
                    return new JsonPhoneNumber
                    {
                        CountryCode = GetDefaultCountryCode(),
                        Number = value
                    };
                }
                return jsonPhoneNumber;
            }

            private static string GetDefaultCountryCode()
            {
                var defaultCountryCodeAttribute = AttributeCache.Get( Rock.SystemGuid.Attribute.GLOBAL_DEFAULT_PHONE_COUNTRY_CODE.AsGuid() );
                var defaultCountryCode = "1";
                if ( defaultCountryCodeAttribute != null )
                {
                    var defaultCountryCodeValue = defaultCountryCodeAttribute.GetAttributeValue( defaultCountryCodeAttribute.Key );

                    if ( !string.IsNullOrWhiteSpace( defaultCountryCodeValue ) )
                    {
                        defaultCountryCode = defaultCountryCodeValue;
                    }
                    else if ( !string.IsNullOrWhiteSpace( defaultCountryCodeAttribute.DefaultValue ) )
                    {
                        defaultCountryCode = defaultCountryCodeAttribute.DefaultValue;
                    }
                }

                return defaultCountryCode;
            }

            public static string GetStringFromJsonPhoneNumber( JsonPhoneNumber jsonPhoneNumber )
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject( jsonPhoneNumber );
            }

            public static string GetStringFromPhoneNumber( string countryCode, string number )
            {
                var jsonPhoneNumber = new JsonPhoneNumber
                {
                    CountryCode = countryCode,
                    Number = number
                };

                return GetStringFromJsonPhoneNumber( jsonPhoneNumber );
            }
        }

        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override System.Web.UI.Control EditControl( System.Collections.Generic.Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            return new PhoneNumberBox { ID = id };
        }

        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var phoneNumberBox = control as PhoneNumberBox;
            if ( phoneNumberBox == null )
            {
                base.SetEditValue( control, configurationValues, value );
                return;
            }

            var jsonPhoneNumber = JsonPhoneNumber.GetJsonPhoneNumberFromString( value );
            phoneNumberBox.Number = jsonPhoneNumber.Number;
            phoneNumberBox.CountryCode = jsonPhoneNumber.CountryCode;
        }

        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var phoneNumberBox = control as PhoneNumberBox;
            var editValue = string.Empty;

            if ( phoneNumberBox == null )
            {
                return base.GetEditValue( control, configurationValues );
            }

            return JsonPhoneNumber.GetStringFromPhoneNumber( phoneNumberBox.CountryCode, phoneNumberBox.Number );
        }

        public override string FormatValue( System.Web.UI.Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            var jsonPhoneNumber = JsonPhoneNumber.GetJsonPhoneNumberFromString( value );
            var formattedValue = string.Format( "+{0} {1}", jsonPhoneNumber.CountryCode, jsonPhoneNumber.Number );

            return base.FormatValue( parentControl, formattedValue, null, condensed );
        }
    }
}
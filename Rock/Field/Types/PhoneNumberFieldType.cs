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
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to save and display a phone number
    /// </summary>
    public class PhoneNumberFieldType : FieldType
    {
        /// <summary>
        /// Internal call used to serialize and deserialize the phone number.
        /// </summary>
        private class JsonPhoneNumber
        {
            /// <summary>
            /// Gets or sets the base phone number.
            /// </summary>
            /// <value>
            /// The base phone number.
            /// </value>
            public string Number { get; set; }


            /// <summary>
            /// Gets or sets the phone number's country code.
            /// </summary>
            /// <value>
            /// The phone number's country code.
            /// </value>
            public string CountryCode { get; set; }

            /// <summary>
            /// Gets the builds the phone number object from a JSON string.
            /// </summary>
            /// <param name="value">The JSON string for the phone number.</param>
            /// <returns></returns>
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
                        CountryCode = PhoneNumber.DefaultCountryCode(),
                        Number = value
                    };
                }
                return jsonPhoneNumber;
            }

            /// <summary>
            /// Gets the JSON string the represents the phone number object.
            /// </summary>
            /// <param name="jsonPhoneNumber">The phone number object.</param>
            /// <returns></returns>
            public static string GetStringFromJsonPhoneNumber( JsonPhoneNumber jsonPhoneNumber )
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject( jsonPhoneNumber );
            }

            /// <summary>
            /// Builds a valid JSON string using the supplied country code and number.
            /// </summary>
            /// <param name="countryCode">The country code.</param>
            /// <param name="number">The number.</param>
            /// <returns></returns>
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

        /// <summary>
        /// Sets the correct values on the phone number control.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var phoneNumberBox = control as PhoneNumberBox;
            if ( phoneNumberBox == null )
            {
                base.SetEditValue( control, configurationValues, value );
                return;
            }

            var jsonPhoneNumber = JsonPhoneNumber.GetJsonPhoneNumberFromString( value );
            phoneNumberBox.Number = PhoneNumber.FormattedNumber( jsonPhoneNumber.CountryCode, jsonPhoneNumber.Number, false );
            phoneNumberBox.CountryCode = jsonPhoneNumber.CountryCode;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns>A JSON string representing the phone number.</returns>
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

        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns>Returns the formatted phone number.</returns>
        public override string FormatValue( System.Web.UI.Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            var jsonPhoneNumber = JsonPhoneNumber.GetJsonPhoneNumberFromString( value );
            var formattedPhoneNumber = PhoneNumber.FormattedNumber( jsonPhoneNumber.CountryCode, jsonPhoneNumber.Number, !jsonPhoneNumber.CountryCode.Equals( PhoneNumber.DefaultCountryCode() ) );

            return base.FormatValue( parentControl, formattedPhoneNumber, null, condensed );
        }
    }
}
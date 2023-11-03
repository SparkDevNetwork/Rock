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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Field.Types;
using Rock.Tests.Integration.TestData;
using Rock.Tests.Shared;
using Rock.Web.UI.Controls;

namespace Rock.Tests.Integration.Core.Field.Types
{
    [TestClass]
    public class PhoneNumberFieldTypeTests
    {
        [ClassInitialize]
        public static void ClassInitialize( TestContext testContext )
        {
            GlobalSettingsDataManager.Instance.AddOrUpdatePhoneNumberCountryCode( "81",
                "Test Country 1",
                @"^(\d{2})(\d{4})(\d{4})$",
                @"$1-$2-$3" );

            // Add a Phone Country Code with the country code included in the format string.
            GlobalSettingsDataManager.Instance.AddOrUpdatePhoneNumberCountryCode( "82",
                "Test Country 2",
                @"^(\d{2})(\d{4})(\d{4})$",
                @"+82 $1-$2-$3" );

        }

        [DataTestMethod]
        [DataRow( "987 6543", "1", "987-6543", "987-6543" )]
        [DataRow( "987-654-3210", "1", "(987) 654-3210", "(987) 654-3210" )]
        [DataRow( "1 (987) 654-3210", "1", "(987) 654-3210", "(987) 654-3210" )]
        public void ParsePhoneNumber_InputIncludesDefaultCountryCode_ReturnsValueWithNoCountryCode( string number, string countryCodePart, string numberPart, string formattedValue )
        {
            VerifyParsedPhoneNumberParts( number, countryCodePart, numberPart, formattedValue );
        }

        [DataTestMethod]
        [DataRow( "+81 (11) 2222 3333", "81", "11-2222-3333", "+81 11-2222-3333" )]
        [DataRow( "+82-11 2222 3333", "82", "11-2222-3333", "+82 11-2222-3333" )]
        public void ParsePhoneNumber_InputIncludesNonDefaultCountryCode_ReturnsValueWithCountryCode( string number, string countryCodePart, string numberPart, string formattedValue )
        {
            // Note that the country code must include the "+" prefix and be terminated by a non-digit to be parsed correctly.
            VerifyParsedPhoneNumberParts( number, countryCodePart, numberPart, formattedValue );
        }

        [DataTestMethod]
        [DataRow( "+82-112222 3333", "82", "11-2222-3333", "+82 11-2222-3333" )]
        [DataRow( "+82 (11)22223333", "82", "11-2222-3333", "+82 11-2222-3333" )]
        public void ParsePhoneNumber_FormatExpressionIncludesCountryCode_ReturnsValueWithSingleCountryCode( string number, string countryCodePart, string numberPart, string formattedValue )
        {
            VerifyParsedPhoneNumberParts( number, countryCodePart, numberPart, formattedValue );
        }

        [DataTestMethod]
        [DataRow( "+99 12345678", "99", "12345678", "+99 12345678" )]
        public void ParsePhoneNumber_InputIncludesUndefinedCountryCode_ReturnsInputCountryCode( string number, string countryCodePart, string numberPart, string formattedValue )
        {
            VerifyParsedPhoneNumberParts( number, countryCodePart, numberPart, formattedValue );
        }

        /// <summary>
        /// Verify that the input phone number string can be correctly parsed into its component parts and formatted correctly for persisted storage.
        /// </summary>
        /// <param name="inputNumber">The input phone number, including any allowable punctuation or delimiters.</param>
        /// <param name="expectedCountryCode">The country code that should be associated with this phone number.</param>
        /// <param name="expectedFormattedNumber">The formatted phone number that should be derived from the input.</param>
        /// <param name="expectedFormattedValue">The formatted value that should be used to persist the data.</param>
        private void VerifyParsedPhoneNumberParts( string inputNumber, string expectedCountryCode, string expectedFormattedNumber, string expectedFormattedValue )
        {
            var phoneNumberFieldType = new PhoneNumberFieldType();
            var phoneNumberControl = new PhoneNumberBox();

            phoneNumberFieldType.SetEditValue( phoneNumberControl, null, inputNumber );

            // Verify that the phone number parts are correctly identified in the edit control.
            Assert.That.AreEqual( expectedCountryCode, phoneNumberControl.CountryCode );
            Assert.That.AreEqual( expectedFormattedNumber, phoneNumberControl.Number );

            // Verify that the formatted phone number used to store the value is correct.
            // The country code prefix should only be included for non-default values.
            var formattedValue = phoneNumberFieldType.GetEditValue( phoneNumberControl, null );

            Assert.That.AreEqual( expectedFormattedValue, formattedValue );
        }

    }
}

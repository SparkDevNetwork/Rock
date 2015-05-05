// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;

using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Control for displaying a formatted list of a Person's Phone numbers, where DataField is a IEnumerable of PhoneNumber
    /// </summary>
    [ToolboxData( "<{0}:PhoneNumbersField runat=server></{0}:PhoneNumbersField>" )]
    public class PhoneNumbersField : RockBoundField
    {
        /// <summary>
        /// Formats the specified field value for a cell in the <see cref="T:System.Web.UI.WebControls.BoundField" /> object.
        /// </summary>
        /// <param name="dataValue">The field value to format.</param>
        /// <param name="encode">true to encode the value; otherwise, false.</param>
        /// <returns>
        /// The field value converted to the format specified by <see cref="P:System.Web.UI.WebControls.BoundField.DataFormatString" />.
        /// </returns>
        protected override string FormatDataValue( object dataValue, bool encode )
        {
            var phoneNumbers = dataValue as IEnumerable<Rock.Model.PhoneNumber>;
            if ( phoneNumbers != null && phoneNumbers.Any() )
            {
                dataValue = "<ul class='list-unstyled phonenumbers'>";
                foreach ( var phoneNumber in phoneNumbers )
                {
                    dataValue += "<li>";
                    dataValue += FormatPhoneNumber( phoneNumber.IsUnlisted, phoneNumber.CountryCode, phoneNumber.Number, phoneNumber.NumberTypeValueId ?? 0 );
                    dataValue += "</li>";
                }

                dataValue += "</ul>";
            }
            else
            {
                dataValue = string.Empty;
            }

            return base.FormatDataValue( dataValue, false );
        }

        /// <summary>
        /// Formats the phone number.
        /// </summary>
        /// <param name="unlisted">if set to <c>true</c> [unlisted].</param>
        /// <param name="countryCode">The country code.</param>
        /// <param name="number">The number.</param>
        /// <param name="phoneNumberTypeId">The phone number type identifier.</param>
        /// <returns></returns>
        protected string FormatPhoneNumber( bool unlisted, object countryCode, object number, int phoneNumberTypeId )
        {
            string formattedNumber = "Unlisted";
            if ( !unlisted )
            {
                string cc = countryCode as string ?? string.Empty;
                string n = number as string ?? string.Empty;

                if ( DisplayCountryCode )
                {
                    formattedNumber = PhoneNumber.FormattedNumber( cc, n, true );
                }
                else
                {
                    formattedNumber = PhoneNumber.FormattedNumber( cc, n );
                }
            }

            var phoneType = DefinedValueCache.Read( phoneNumberTypeId );
            if ( phoneType != null )
            {
                return string.Format( "{0} <small>{1}</small>", formattedNumber, phoneType.Value );
            }

            return formattedNumber;
        }

        /// <summary>
        /// Gets or sets a value indicating whether [display country code].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [display country code]; otherwise, <c>false</c>.
        /// </value>
        public bool DisplayCountryCode { get; set; }
    }
}

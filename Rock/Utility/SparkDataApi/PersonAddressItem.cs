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
using System.Text.RegularExpressions;

namespace Rock.Utility.SparkDataApi
{
    /// <summary>
    /// The Person and Address data used to pass to NCOA
    /// </summary>
    public class PersonAddressItem
    {
        /// <summary>
        /// Gets or sets the person identifier.
        /// </summary>
        /// <value>
        /// The person identifier.
        /// </value>
        public int PersonId { get; set; }

        /// <summary>
        /// Gets or sets the person alias identifier.
        /// </summary>
        /// <value>
        /// The person alias identifier.
        /// </value>
        public int PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the family identifier.
        /// </summary>
        /// <value>
        /// The family identifier.
        /// </value>
        public int FamilyId { get; set; }

        /// <summary>
        /// Gets or sets the location identifier.
        /// </summary>
        /// <value>
        /// The location identifier.
        /// </value>
        public int LocationId { get; set; }
        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        /// <value>
        /// The first name.
        /// </value>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        /// <value>
        /// The last name.
        /// </value>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the street1.
        /// </summary>
        /// <value>
        /// The street1.
        /// </value>
        public string Street1 { get; set; }

        /// <summary>
        /// Gets or sets the street2.
        /// </summary>
        /// <value>
        /// The street2.
        /// </value>
        public string Street2 { get; set; }

        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        /// <value>
        /// The city.
        /// </value>
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the postal code.
        /// </summary>
        /// <value>
        /// The postal code.
        /// </value>
        public string PostalCode { get; set; }

        /// <summary>
        /// Gets or sets the country.
        /// </summary>
        /// <value>
        /// The country.
        /// </value>
        public string Country { get; set; }

        /// <summary>
        /// Builds the upload string to send the address data to the NCOA API.
        /// </summary>
        /// <returns></returns>
        public string BuildUploadString()
        {
            return
                $"individual_id={PersonId}_{PersonAliasId}_{FamilyId}_{LocationId}&" +
                $"individual_first_name={FirstName.ScrubNameFieldForUpload()}&" +
                $"individual_last_name={LastName.ScrubNameFieldForUpload()}&" +
                $"address_line_1={Street1.ScrubAddressFieldForUpload()}&" +
                $"address_line_2={Street2.ScrubAddressFieldForUpload()}&" +
                $"address_city_name={City.ScrubAddressFieldForUpload()}&" +
                $"address_state_code={State.ScrubAddressFieldForUpload()}&" +
                $"address_postal_code={PostalCode.ScrubPostalCodeForUpload()}&";
        }

    }

    /// <summary>
    /// String parsing extensions for NCOA address items.
    /// </summary>
    public static class PersonAddressItemStringExtensions
    {
        /// <summary>
        /// Removes leading and trailing spaces, replaces all whitespace characters with a single space.  Replaces various forms of dash with a standard hyphen (-), and backslashes with forward slashes.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ScrubWhiteSpaceAndPunctuation( this string input )
        {
            // Substitute any whitespace (including multiple characters) for a single space.
            var output = Regex.Replace( input, @"\s+", " " );

            // Remove leading/trailing whitespace.
            output = output.Trim();

            // Replace special dashes with a standard hyphen.
            output = Regex.Replace( output, @"\p{Pd}", " -" );

            // Replace backslahses with a forward slash.
            output = Regex.Replace( output, @"\\", " /" );

            return output;
        }

        /// <summary>
        /// Scrub a name field for upload to the NCOA API.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ScrubNameFieldForUpload( this string input )
        {
            var output = input.ScrubWhiteSpaceAndPunctuation();

            // remove all non-word characters (\W) and punctuation connectors (\p{Pc}), except spaces (\s), periods (\.), dashes (\p{Pd}), and apostrophes (').
            var regexPattern = @"[\W\p{Pc}-[\s\.\p{Pd}']]";
            output = Regex.Replace( output, regexPattern, string.Empty );

            return output;
        }

        /// <summary>
        /// Scrub an address field for upload to the NCOA API.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ScrubAddressFieldForUpload( this string input )
        {
            var output = input.ScrubWhiteSpaceAndPunctuation();

            // remove all non-word characters (\W) and punctuation connectors (\p{Pc}), except spaces (\s), slashes(\/), dashes (\p{Pd}), and pound signs (#).
            var regexPattern = @"[\W\p{Pc}-[\s\/\p{Pd}#]]";
            output = Regex.Replace( output, regexPattern, string.Empty );

            return output;
        }

        /// <summary>
        /// Scrub an postal code field for upload to the NCOA API.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ScrubPostalCodeForUpload( this string input )
        {
            var output = input.ScrubWhiteSpaceAndPunctuation();

            // remove all non-numeric characters (\D), except dashes (\p{Pd}).
            var regexPattern = @"[\D-[\p{Pd}]]";
            output = Regex.Replace( output, regexPattern, string.Empty );

            // permit only the first instance of a dash character.
            var dashIndex = output.IndexOf( '-' );
            if ( dashIndex != -1 )
            {
                output = output.Substring( 0, dashIndex + 1 ) + output.Substring( dashIndex + 1 ).Replace( " -", string.Empty );
            }

            return output;
        }
    }
}

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

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// The results from the GetConfiguration API action of the PhoneNumberBox control.
    /// </summary>
    public class PhoneNumberBoxGetConfigurationResultsBag
    {
        /// <summary>
        /// The default country code
        /// </summary>
        public string DefaultCountryCode { get; set; }

        /// <summary>
        /// A list of each of the country codes, in the order they should be shown
        /// </summary>
        public List<string> CountryCodes { get; set; }

        /// <summary>
        /// The list of validation/formatting rules for each country code
        /// </summary>
        public Dictionary<string, List<PhoneNumberCountryCodeRulesConfigurationBag>> Rules { get; set; }

        /// <summary>
        /// Gets or sets the SMS opt in text.
        /// </summary>
        /// <value>
        /// The SMS opt in text.
        /// </value>
        public string SmsOptInText { get; set; } = string.Empty;
    }
}

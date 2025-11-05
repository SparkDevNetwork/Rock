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

namespace Rock.ViewModels.Blocks.Finance.BenevolenceRequestDetail
{
    /// <summary>
    /// Represents a collection of properties related to a phone number,
    /// including the country code, the number itself, and the type of
    /// phone number <u>that are relevevant in the context of a benevolence request.</u>.
    /// </summary>
    public class PhoneNumberBag
    {
        /// <summary>
        /// Gets or sets the country code for the phone number.
        /// This is typically the international dialing code (e.g., 1 for USA/Canada).
        /// </summary>
        public string CountryCode { get; set; }

        /// <summary>
        /// Gets or sets the phone number.
        /// This should be the local portion of the phone number, excluding the country code.
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// Gets or sets the formatted representation of the phone number, exluding the country code.
        /// </summary>
        public string NumberFormatted { get; set; }
    }
}

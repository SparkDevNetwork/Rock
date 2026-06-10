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

namespace Rock.ViewModels.Blocks.Security.PhoneNumberIdentification
{
    /// <summary>
    /// The request to send a verification code to a phone number.
    /// </summary>
    public class PhoneNumberIdentificationSendCodeRequestBag
    {
        /// <summary>
        /// Gets or sets the phone number to send the verification code to.
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the country code of the phone number, used to address the SMS to the correct country.
        /// </summary>
        public string CountryCode { get; set; }
    }
}

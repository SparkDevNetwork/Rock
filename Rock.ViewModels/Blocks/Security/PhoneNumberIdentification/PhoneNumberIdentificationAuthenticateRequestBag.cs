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
    /// The request to authenticate a person the individual selected from multiple phone number matches.
    /// </summary>
    public class PhoneNumberIdentificationAuthenticateRequestBag
    {
        /// <summary>
        /// Gets or sets the encrypted token identifying the verification record the code belongs to.
        /// </summary>
        public string VerificationToken { get; set; }

        /// <summary>
        /// Gets or sets the verification code entered by the individual.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the identifier key of the person the individual selected to authenticate as.
        /// </summary>
        public string PersonValue { get; set; }
    }
}

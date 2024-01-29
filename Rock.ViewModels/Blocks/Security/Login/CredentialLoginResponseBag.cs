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

namespace Rock.ViewModels.Blocks.Security.Login
{
    /// <summary>
    /// A bag that contains the credential login response information.
    /// </summary>
    public class CredentialLoginResponseBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether authentication was successful.
        /// </summary>
        /// <value><c>true</c> if authentication was success; otherwise, <c>false</c>.</value>
        public bool IsAuthenticated { get; set; }

        /// <summary>
        /// The error message.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the individual is locked out.
        /// </summary>
        /// <value><c>true</c> if the individual is locked out; otherwise, <c>false</c>.</value>
        public bool IsLockedOut { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the individual needs to confirm their account.
        /// </summary>
        /// <value><c>true</c> if the individual needs to confirm their account; otherwise, <c>false</c>.</value>
        public bool IsConfirmationRequired { get; set; }

        /// <summary>
        /// Gets or sets the redirect URL if authentication was successful.
        /// </summary>
        /// <value>The redirect URL.</value>
        public string RedirectUrl { get; set; }

        /// <summary>
        /// Gets or sets the MFA details.
        /// </summary>
        /// <value>
        /// The MFA details.
        /// </value>
        public PasswordlessLoginMfaBag Mfa { get; set; }
    }
}

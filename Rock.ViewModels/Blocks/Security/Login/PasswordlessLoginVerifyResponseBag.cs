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
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Security.Login
{
    /// <summary>
    /// A bag that contains the passwordless login verify response information.
    /// </summary>
    public class PasswordlessLoginVerifyResponseBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the person is authenticated.
        /// </summary>
        /// <value><c>true</c> if the person is authenticated; otherwise, <c>false</c>.</value>
        public bool IsAuthenticated { get; set; }

        /// <summary>
        /// The error message.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Indicates whether account registration is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if account registration is required; otherwise, <c>false</c>.
        /// </value>
        public bool IsRegistrationRequired { get; set; }

        /// <summary>
        /// Indicates whether person selection is required.
        /// </summary>
        public bool IsPersonSelectionRequired { get; set; }

        /// <summary>
        /// The people matching the email or phone number.
        /// </summary>
        /// <remarks>Only set when multiple matches are found.</remarks>
        public List<ListItemBag> MatchingPeople { get; set; }

        /// <summary>
        /// Gets or sets the MFA details.
        /// </summary>
        /// <value>
        /// The MFA details.
        /// </value>
        public CredentialLoginMfaBag Mfa { get; set; }

        /// <summary>
        /// Gets or sets the registration URL.
        /// </summary>
        public string RegistrationUrl { get; set; }
    }
}

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
using Rock.Attribute;
using Rock.Enums.Blocks.Security.Login;
using Rock.Model;

namespace Rock.Security.Authentication.OneTimePasscode
{
    /// <summary>
    /// Represents a result of performing one-time passcode authentication.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <strong>This is an internal API</strong> that supports the Rock
    ///         infrastructure and not subject to the same compatibility standards
    ///         as public APIs. It may be changed or removed without notice in any
    ///         release and should therefore not be directly used in any plug-ins.
    ///     </para>
    /// </remarks>
    [RockInternal( "1.15" )]
    public class OneTimePasscodeAuthenticationResult
    {
        /// <summary>
        /// Gets or sets the authenticated user login.
        /// </summary>
        /// <value>
        /// The authenticated user login.
        /// </value>
        public UserLogin AuthenticatedUser { get; set; }

        /// <summary>
        /// The error message.
        /// </summary>
        public string ErrorMessage { get; set; }
    
        /// <summary>
        /// Indicates whether the user is authenticated.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the user is authenticated; otherwise, <c>false</c>.
        /// </value>
        public bool IsAuthenticated { get; internal set; }

        /// <summary>
        /// Indicates whether person selection is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if person selection is required; otherwise, <c>false</c>.
        /// </value>
        public bool IsPersonSelectionRequired { get; set; }

        /// <summary>
        /// Indicates whether account registration is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if account registration is required; otherwise, <c>false</c>.
        /// </value>
        public bool IsRegistrationRequired { get; set; }

        /// <summary>
        /// The people matching the email or phone number.
        /// </summary>
        /// <remarks>Only set when multiple matches are found.</remarks>
        public List<MatchingPersonResult> MatchingPeopleResults { get; set; }

        /// <summary>
        /// Gets or sets the registration URL.
        /// </summary>
        public string RegistrationUrl { get; set; }

        /// <summary>
        /// The encrypted state that was generated when the OTP was sent.
        /// </summary>
        public string State { get; set; }
    }
}

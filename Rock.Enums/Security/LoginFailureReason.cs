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
using System.ComponentModel;

namespace Rock.Enums.Security
{
    /// <summary>
    /// The possible reasons for a login failure.
    /// </summary>
    public enum LoginFailureReason
    {
        /// <summary>
        /// See the login failure message for more details.
        /// </summary>
        Other = 0,

        /// <summary>
        /// The username or email does not exist.
        /// </summary>
        UserNotFound = 1,

        /// <summary>
        /// The account is not confirmed.
        /// </summary>
        UserNotConfirmed = 2,

        /// <summary>
        /// The login requires additional verification, such as two-factor authentication.
        /// </summary>
        RequiresVerification = 3,

        /// <summary>
        /// The password provided is incorrect.
        /// </summary>
        InvalidPassword = 4,

        /// <summary>
        /// The password has expired.
        /// </summary>
        PasswordChangeRequired = 5,

        /// <summary>
        /// The account is locked due to multiple failed login attempts.
        /// </summary>
        LockedOut = 6,

        /// <summary>
        /// The request is from an invalid OIDC client Id.
        /// </summary>
        [Description( "Invalid OIDC Client ID" )]
        InvalidOidcClientId = 7
    }
}

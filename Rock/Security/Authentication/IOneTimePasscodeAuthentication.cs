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
using Rock.Attribute;
using Rock.Data;
using Rock.Security.Authentication.OneTimePasscode;

namespace Rock.Security.Authentication
{
    /// <summary>
    /// Represents a one-time passcode authentication provider.
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
    public interface IOneTimePasscodeAuthentication
    {
        /// <summary>
        /// Authenticates the user using a one-time passcode authentication provider.
        /// </summary>
        /// <param name="options">The authentication options.</param>
        /// <returns>The one-time passcode authentication result.</returns>
        OneTimePasscodeAuthenticationResult Authenticate( OneTimePasscodeAuthenticationOptions options );

        /// <summary>
        /// Sends a one time passcode (OTP) via email or SMS.
        /// </summary>
        /// <param name="sendOneTimePasscodeOptions">The OTP options.</param>
        /// <param name="rockContext">The Rock context.</param>
        /// <returns>A result containing the encrypted passwordless state, if successful.</returns>
        SendOneTimePasscodeResult SendOneTimePasscode( SendOneTimePasscodeOptions sendOneTimePasscodeOptions, RockContext rockContext );
    }
}

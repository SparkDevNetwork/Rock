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
using System;
using System.Collections.Generic;
using Rock.Attribute;
using Rock.Security.Authentication.ExternalRedirectAuthentication;

namespace Rock.Security.Authentication
{
    /// <summary>
    /// Represents an external redirect authentication provider.
    /// </summary>
    /// <remarks>
    ///     Can be used for both OAuth1- and OAuth2-based authentication.
    ///     For OAuth1-based authentication, fetch the request token in the <see cref="GenerateExternalLoginUrl(string, string)"/> method.
    ///     <para>
    ///         <strong>This is an internal API</strong> that supports the Rock
    ///         infrastructure and not subject to the same compatibility standards
    ///         as public APIs. It may be changed or removed without notice in any
    ///         release and should therefore not be directly used in any plug-ins.
    ///     </para>
    /// </remarks>
    [RockInternal( "1.15" )]
    public interface IExternalRedirectAuthentication
    {
        /// <summary>
        /// Authenticates the user using an external redirect authentication provider.
        /// </summary>
        /// <param name="options">The authentication options.</param>
        /// <returns>The external redirect authentication result.</returns>
        ExternalRedirectAuthenticationResult Authenticate( ExternalRedirectAuthenticationOptions options );

        /// <summary>
        /// Generates the log in URL.
        /// </summary>
        /// <param name="externalProviderReturnUrl">The callback URL for external auth provider to return to to complete authentication.</param>
        /// <param name="successfulAuthenticationRedirectUrl">The URL to redirect to after completing authentication.</param>
        /// <returns></returns>
        Uri GenerateExternalLoginUrl( string externalProviderReturnUrl, string successfulAuthenticationRedirectUrl );

        /// <summary>
        /// Determines if authentication should proceed with this authentication provider based on the request parameters.
        /// </summary>
        /// <param name="parameters">The request parameters.</param>
        /// <returns><c>true</c> if authentication should proceed with this authentication provider; otherwise, <c>false</c> is returned.</returns>
        bool IsReturningFromExternalAuthentication( IDictionary<string, string> parameters );
    }
}

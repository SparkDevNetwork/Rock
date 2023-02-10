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
using Rock.Model;
using Rock.Security.Authentication.CredentialAuthentication;

namespace Rock.Security.Authentication
{
    /// <summary>
    /// Represents a password-based authentication provider.
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
    public interface ICredentialAuthentication
    {
        /// <summary>
        /// Authenticates the user based on user name and password.
        /// </summary>
        /// <param name="options">The authentication options.</param>
        /// <returns><c>true</c> if authenticated; otherwise, <c>false</c> is returned.</returns>
        bool Authenticate( CredentialAuthenticationOptions options );

        /// <summary>
        /// Encrypts the <paramref name="password"/>.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <returns>The encrypted password.</returns>
        string EncryptPassword( string password );
    }
}

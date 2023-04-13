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

namespace Rock.Security.Authentication.ExternalRedirectAuthentication
{
    /// <summary>
    /// Represents a result of performing external redirect authentication.
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
    public class ExternalRedirectAuthenticationResult
    {
        /// <summary>
        /// Indicates whether the user is authenticated.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the user is authenticated; otherwise, <c>false</c>.
        /// </value>
        public bool IsAuthenticated { get; set; }

        /// <summary>
        /// Gets or sets the name of the authenticated user.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the post-authentication return URL.
        /// </summary>
        public string ReturnUrl { get; set; }
    }
}

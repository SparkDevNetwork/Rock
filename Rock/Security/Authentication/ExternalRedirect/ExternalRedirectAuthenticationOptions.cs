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

namespace Rock.Security.Authentication.ExternalRedirectAuthentication
{
    /// <summary>
    /// Represents options to perform external redirect authentication.
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
    public class ExternalRedirectAuthenticationOptions
    {
        /// <summary>
        /// The callback URL for the external auth provider to return to to complete authentication.
        /// </summary>
        public string RedirectUrl { get; set; }

        /// <summary>
        /// Gets or sets the request parameters.
        /// </summary>
        public IDictionary<string, string> Parameters { get; set; }
    }
}

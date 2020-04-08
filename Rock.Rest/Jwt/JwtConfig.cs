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
using Rock.Model;

namespace Rock.Rest.Jwt
{
    /// <summary>
    /// Represents a JSON Web Token Configuration. These records are persisted as JSON Web Token Configuration DefinedValues
    /// </summary>
    public class JwtConfig
    {
        /// <summary>
        /// Gets or sets the audience. If not set, Rock will not require Audience validation.
        /// </summary>
        /// <value>
        /// The audience.
        /// </value>
        public string Audience { get; set; }

        /// <summary>
        /// Gets or sets the issuer. If not set, Rock will not require Issuer validation.
        /// </summary>
        /// <value>
        /// The issuer.
        /// </value>
        public string Issuer { get; set; }

        /// <summary>
        /// Gets or sets the OpenId configuration URL.
        /// This is required for JWT validation.
        /// Looks like: https://xxxxx.auth0.com/.well-known/openid-configuration
        /// </summary>
        /// <value>
        /// The open identifier configuration URL.
        /// </value>
        public string OpenIdConfigurationUrl { get; set; }

        /// <summary>
        /// Gets or sets the search type value identifier. This is a defined value that correlates to <see cref="PersonSearchKey.SearchTypeValueId"/>.
        /// This is required for JWT validation.
        /// </summary>
        /// <value>
        /// The search type value identifier.
        /// </value>
        public int? SearchTypeValueId { get; set; }
    }
}

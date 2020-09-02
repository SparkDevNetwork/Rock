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

namespace Rock.Oidc.Configuration
{
    /// <summary>
    /// A class that represents the Rock Oidc Settings.
    /// </summary>
    public class RockOidcSettings
    {
        /// <summary>
        /// Gets or sets the access token lifetime.
        /// </summary>
        /// <value>
        /// The access token lifetime.
        /// </value>
        public int AccessTokenLifetime { get; set; }
        /// <summary>
        /// Gets or sets the identity token lifetime.
        /// </summary>
        /// <value>
        /// The identity token lifetime.
        /// </value>
        public int IdentityTokenLifetime { get; set; }
        /// <summary>
        /// Gets or sets the refresh token lifetime.
        /// </summary>
        /// <value>
        /// The refresh token lifetime.
        /// </value>
        public int RefreshTokenLifetime { get; set; }
        /// <summary>
        /// Gets or sets the signing key lifetime.
        /// </summary>
        /// <value>
        /// The signing key lifetime.
        /// </value>
        public int SigningKeyLifetime { get; set; }

        /// <summary>
        /// Gets the default settings.
        /// </summary>
        /// <returns></returns>
        public static RockOidcSettings GetDefaultSettings()
        {
            return new RockOidcSettings
            {
                AccessTokenLifetime = 1 * 60 * 60, // One hour
                IdentityTokenLifetime = 20 * 60, // twenty minutes
                RefreshTokenLifetime = 14 * 24 * 60 * 60, // two weeks
                SigningKeyLifetime = 24 * 60 * 60 // one day
            };
        }
    }
}

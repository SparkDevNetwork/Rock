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

namespace Rock.Enums.Net
{
    /// <summary>
    /// Used to set the SameSite field on cookies to indicate if those
    /// cookies should be included by the client on future "same-site"
    /// or "cross-site" requests.
    /// </summary>
    public enum CookieSameSiteMode
    {
        /// <summary>
        /// No SameSite field will be set, the client should follow its
        /// default cookie policy.
        /// </summary>
        Unspecified = -1,

        /// <summary>
        /// Indicates the client should disable same-site restrictions.
        /// </summary>
        None = 0,

        /// <summary>
        /// Indicates the client should send the cookie with "same-site"
        /// requests, and with "cross-site" top-level navigations.
        /// </summary>
        Lax = 1,

        /// <summary>
        /// Indicates the client should only send the cookie with "same-site"
        /// requests.
        /// </summary>
        Strict = 2
    }
}

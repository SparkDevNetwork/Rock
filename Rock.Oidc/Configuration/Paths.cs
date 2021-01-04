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
    /// OIDC Paths/Routes
    /// </summary>
    public static class Paths
    {
        /// <summary>
        /// The authorize path
        /// </summary>
        public static readonly string AuthorizePath = "/Auth/Authorize";

        /// <summary>
        /// The token path
        /// </summary>
        public static readonly string TokenPath = "/Auth/Token";

        /// <summary>
        /// The logout path
        /// </summary>
        public static readonly string LogoutPath = "/Auth/Logout";

        /// <summary>
        /// The user information
        /// </summary>
        public static readonly string UserInfo = "/Auth/UserInfo";
    }
}
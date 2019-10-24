﻿// <copyright>
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

namespace Rock.Rest
{
    /// <summary>
    /// Tokens that are used in the headers of API requests
    /// </summary>
    public static class HeaderTokens
    {
        /// <summary>
        /// The Authorization Token key
        /// </summary>
        public const string AuthorizationToken = "Authorization-Token";

        /// <summary>
        /// The JSON Web Token
        /// </summary>
        public const string JWT = "Authorization";

        /// <summary>
        /// The JSON Web Token Prefix
        /// </summary>
        public const string JwtPrefix = "Bearer ";
    }
}

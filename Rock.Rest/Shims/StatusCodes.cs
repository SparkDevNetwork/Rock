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

#if WEBFORMS
namespace Microsoft.AspNetCore.Http
{
    /// <summary>
    /// A collection of constants for HTTP status codes.
    /// </summary>
    public static class StatusCodes
    {
        /// <summary>
        /// HTTP status code 101.
        /// </summary>
        public const int Status101SwitchingProtocols = 101;

        /// <summary>
        /// HTTP status code 200.
        /// </summary>
        public const int Status200OK = 200;

        /// <summary>
        /// HTTP status code 201.
        /// </summary>
        public const int Status201Created = 201;

        /// <summary>
        /// HTTP status code 204.
        /// </summary>
        public const int Status204NoContent = 204;

        /// <summary>
        /// HTTP status code 400.
        /// </summary>
        public const int Status400BadRequest = 400;

        /// <summary>
        /// HTTP status code 401.
        /// </summary>
        public const int Status401Unauthorized = 401;

        /// <summary>
        /// HTTP status code 403.
        /// </summary>
        public const int Status403Forbidden = 403;

        /// <summary>
        /// HTTP status code 404.
        /// </summary>
        public const int Status404NotFound = 404;

        /// <summary>
        /// HTTP status code 409.
        /// </summary>
        public const int Status409Conflict = 409;

        /// <summary>
        /// HTTP status code 500.
        /// </summary>
        public const int Status500InternalServerError = 500;
    }
}
#endif

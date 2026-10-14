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
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Utility
{
    /// <summary>
    /// Helper class used to determine if the running version of Rock is
    /// considered secure by the Rock servers.
    /// </summary>
    internal static class RockSecurityVersionHelper
    {
        #region Fields

        /// <summary>
        /// The URL of the endpoint that reports the security status of a
        /// Rock version.
        /// </summary>
        private const string SecurityStatusUrl = "https://apigateway.rockrms.com/api/v1/rock-support/secured-version";

        /// <summary>
        /// The key used to store the status in cache.
        /// </summary>
        private const string CacheKey = "Rock:RockSecurityVersionHelper:Status";

        /// <summary>
        /// The HTTP client used to communicate with the remote server. This is
        /// shared to avoid exhausting sockets.
        /// </summary>
        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds( 5 )
        };

        /// <summary>
        /// Ensures only one request to the remote server is made at a time.
        /// </summary>
        private static readonly SemaphoreSlim _requestLock = new SemaphoreSlim( 1, 1 );

        #endregion

        #region Methods

        /// <summary>
        /// Gets the security status of the running version of Rock. The result
        /// is cached until the process restarts or the cache is cleared. If the
        /// remote server could not be contacted then the version is reported
        /// as secure.
        /// </summary>
        /// <returns>The security status of the running version of Rock.</returns>
        public static async Task<RockSecurityVersionStatus> GetStatusAsync()
        {
            if ( RockCache.Get( CacheKey ) is RockSecurityVersionStatus cachedStatus )
            {
                return cachedStatus;
            }

            await _requestLock.WaitAsync();

            try
            {
                // Check again in case another request populated the cache
                // while we were waiting.
                if ( RockCache.Get( CacheKey ) is RockSecurityVersionStatus existingStatus )
                {
                    return existingStatus;
                }

                var status = await RequestStatusAsync();

                RockCache.AddOrUpdate( CacheKey, status );

                return status;
            }
            finally
            {
                _requestLock.Release();
            }
        }

        /// <summary>
        /// Requests the security status of the running version of Rock from
        /// the remote server.
        /// </summary>
        /// <returns>The security status of the running version of Rock.</returns>
        private static async Task<RockSecurityVersionStatus> RequestStatusAsync()
        {
            var currentVersion = VersionInfo.VersionInfo.GetRockSemanticVersionNumber();
            var status = new RockSecurityVersionStatus
            {
                IsSecure = true,
                CurrentVersion = currentVersion
            };

            try
            {
                var requestUrl = $"{SecurityStatusUrl}?version={Uri.EscapeDataString( currentVersion )}";
                string content;

                using ( var response = await _httpClient.GetAsync( requestUrl ) )
                {
                    if ( response.StatusCode != HttpStatusCode.OK )
                    {
                        return status;
                    }

                    content = await response.Content.ReadAsStringAsync();
                }

                if ( content.IsNullOrWhiteSpace() )
                {
                    return status;
                }

                var responseBag = content.FromJsonOrNull<SecurityStatusResponse>();

                if ( responseBag?.IsSecure == false )
                {
                    status.IsSecure = false;
                    status.SecureVersion = responseBag.SecureVersion;
                    status.Message = responseBag.Message;
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( new Exception( "Unable to determine the security status of this version of Rock.", ex ) );
            }

            return status;
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// The response returned by the remote server.
        /// </summary>
        private class SecurityStatusResponse
        {
            /// <summary>
            /// Gets or sets a value indicating whether the version is secure.
            /// A missing value is treated as secure.
            /// </summary>
            public bool? IsSecure { get; set; }

            /// <summary>
            /// Gets or sets the minimum version that is considered secure.
            /// </summary>
            public string SecureVersion { get; set; }

            /// <summary>
            /// Gets or sets the optional custom message to display.
            /// </summary>
            public string Message { get; set; }
        }

        #endregion
    }

    /// <summary>
    /// The security status of the running version of Rock.
    /// </summary>
    internal class RockSecurityVersionStatus
    {
        /// <summary>
        /// Gets or sets a value indicating whether the running version of
        /// Rock is considered secure.
        /// </summary>
        /// <value><c>true</c> if the running version is secure; otherwise, <c>false</c>.</value>
        public bool IsSecure { get; set; }

        /// <summary>
        /// Gets or sets the running version of Rock.
        /// </summary>
        /// <value>The running version of Rock.</value>
        public string CurrentVersion { get; set; }

        /// <summary>
        /// Gets or sets the minimum version of Rock that is considered secure.
        /// </summary>
        /// <value>The minimum secure version of Rock.</value>
        public string SecureVersion { get; set; }

        /// <summary>
        /// Gets or sets the optional custom message to display.
        /// </summary>
        /// <value>The custom message.</value>
        public string Message { get; set; }
    }
}

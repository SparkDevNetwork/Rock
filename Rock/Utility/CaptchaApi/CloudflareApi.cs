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
using System.Collections.Generic;
using System.Threading.Tasks;

using Newtonsoft.Json;
using RestSharp;
using Rock.Web;

namespace Rock.Utility.CaptchaApi
{
    /// <summary>
    /// Cloudflare API calls
    /// </summary>
    public class CloudflareApi
    {
        private const string CLOUDFLARE_SERVER = "https://challenges.cloudflare.com";
        private readonly RestClient _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudflareApi"/> class.
        /// </summary>
        public CloudflareApi()
        {
            _client = new RestClient( CLOUDFLARE_SERVER );
        }

        /// <summary>
        /// Determines whether the specified token is valid.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>
        ///   <c>true</c> if the specified token is valid; otherwise, <c>false</c>.
        /// </returns>
        public bool IsTurnstileTokenValid( string token )
        {
            var siteKey = SystemSettings.GetValue( SystemKey.SystemSetting.CAPTCHA_SITE_KEY );
            var secret = SystemSettings.GetValue( SystemKey.SystemSetting.CAPTCHA_SECRET_KEY );

            if ( string.IsNullOrWhiteSpace( siteKey ) || string.IsNullOrWhiteSpace( secret) )
            {
                return true;
            }

            if ( string.IsNullOrWhiteSpace( token ) )
            {
                return false;
            }

            try
            {
                var request = new RestRequest( "/turnstile/v0/siteverify", Method.POST );

                request.AddParameter( "secret", secret );
                request.AddParameter( "response", token );
                request.Timeout = 5000;

                var response = _client.Execute<CloudFlareCaptchaResponse>( request );

                return response.Data.Success;
            }
            catch ( Exception e )
            {
                Rock.Model.ExceptionLogService.LogException( e );
                return false;
            }
        }

        /// <summary>
        /// Determines whether the specified token is valid.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="remoteIpAddress">The IP address of the client, this may currently be <c>null</c>.</param>
        /// <returns>
        ///   <c>true</c> if the specified token is valid; otherwise, <c>false</c>.
        /// </returns>
        public async Task<bool> IsTurnstileTokenValidAsync( string token, string remoteIpAddress )
        {
            var siteKey = SystemSettings.GetValue( SystemKey.SystemSetting.CAPTCHA_SITE_KEY );
            var secret = SystemSettings.GetValue( SystemKey.SystemSetting.CAPTCHA_SECRET_KEY );

            if ( string.IsNullOrWhiteSpace( siteKey ) || string.IsNullOrWhiteSpace( secret ) )
            {
                return true;
            }

            if ( string.IsNullOrWhiteSpace( token ) )
            {
                return false;
            }

            try
            {
                var request = new RestRequest( "/turnstile/v0/siteverify", Method.POST );

                request.AddParameter( "secret", secret );
                request.AddParameter( "response", token );

                if ( remoteIpAddress.IsNotNullOrWhiteSpace() )
                {
                    request.AddParameter( "remoteip", remoteIpAddress );
                }

                request.Timeout = 5000;

                var response = await _client.ExecuteTaskAsync<CloudFlareCaptchaResponse>( request );

                return response.Data.Success;
            }
            catch ( Exception e )
            {
                Rock.Model.ExceptionLogService.LogException( e );
                return false;
            }
        }

        /// <summary>
        /// Support class to handle the response Cloudflares captcha reponse
        /// </summary>
        private sealed class CloudFlareCaptchaResponse
        {
            [JsonProperty( "success" )]
            public bool Success { get; set; }

            [JsonProperty( "challenge_ts" )]
            public DateTime ChallengeTimeStamp { get; set; }

            [JsonProperty( "hostname" )]
            public string HostName { get; set; }

            [JsonProperty( "error-codes" )]
            public List<string> ErrorCodes { get; set; }

            [JsonProperty( "action" )]
            public string Action { get; set; }

            [JsonProperty( "cdata" )]
            public string CustomerData { get; set; }
        }
    }
}

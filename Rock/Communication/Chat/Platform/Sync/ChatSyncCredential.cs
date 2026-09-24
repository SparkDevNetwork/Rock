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
using System.Net.Http;

using Newtonsoft.Json.Linq;

using Rock.Communication.Chat.Platform.Configuration;

namespace Rock.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// The platform token a sync is made under.
    /// </summary>
    internal static class ChatSyncCredential
    {
        /// <summary>
        /// Exchanges the church's signed sync token for a platform token.
        /// </summary>
        /// <param name="configuration">The church's chat settings.</param>
        /// <param name="churchToken">The church's sync-scope token, or null when the church could not sign one.</param>
        /// <param name="failure">Why there is no token, when there is none.</param>
        /// <param name="handler">The transport, or null for the ordinary one.</param>
        /// <returns>The platform token, or null.</returns>
        /// <remarks>
        /// The platform's data API verifies only tokens the platform signed, so a church token sent
        /// to it directly is refused on every call. One exchange covers a whole run: the platform
        /// grants no token with less than two minutes left, and the longest run polls for one.
        /// </remarks>
        public static string Exchange( ChatPlatformConfiguration configuration, string churchToken, out string failure, HttpMessageHandler handler = null )
        {
            if ( churchToken.IsNullOrWhiteSpace() )
            {
                failure = "this church could not sign a request to the chat platform";
                return null;
            }

            var url = ( configuration.ProjectUrl ?? string.Empty ).TrimEnd( '/' ) + "/functions/v1/token-exchange";

            try
            {
                using ( var client = handler == null ? new HttpClient() : new HttpClient( handler, false ) )
                using ( var request = new HttpRequestMessage( HttpMethod.Post, url ) )
                {
                    request.Headers.TryAddWithoutValidation( "Authorization", "Bearer " + churchToken );
                    request.Headers.TryAddWithoutValidation( "apikey", configuration.PublishableKey );

                    using ( var response = client.SendAsync( request ).GetAwaiter().GetResult() )
                    {
                        var body = ReadBody( response );
                        var token = ( string ) body?["access_token"];

                        if ( response.IsSuccessStatusCode && token.IsNotNullOrWhiteSpace() )
                        {
                            failure = null;
                            return token;
                        }

                        var code = ( string ) body?["error"]?["code"];
                        failure = "the chat platform refused this church's credential: "
                            + ( code.IsNotNullOrWhiteSpace() ? code : "HTTP " + ( int ) response.StatusCode );
                        return null;
                    }
                }
            }
            catch ( Exception exception )
            {
                failure = "the chat platform could not be reached to exchange this church's credential: "
                    + ( exception.InnerException ?? exception ).Message;
                return null;
            }
        }

        /// <summary>
        /// The response body as JSON, or null when it is not JSON.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>The body, or null.</returns>
        private static JObject ReadBody( HttpResponseMessage response )
        {
            try
            {
                var text = response.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
                return text.IsNullOrWhiteSpace() ? null : JObject.Parse( text );
            }
            catch
            {
                // Intentionally ignored: a gateway that answered in its own words has no token to read.
                return null;
            }
        }
    }
}

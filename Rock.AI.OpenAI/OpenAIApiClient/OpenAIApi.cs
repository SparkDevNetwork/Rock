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
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

using RestSharp;
using RestSharp.Authenticators;

using Rock.AI.OpenAI.OpenAIApiClient.Classes.ChatCompletions;
using Rock.AI.OpenAI.OpenAIApiClient.Classes.Moderations;
using Rock.AI.OpenAI.OpenAIApiClient.Classes.TextCompletions;

namespace Rock.AI.OpenAI.OpenAIApiClient
{
    internal class OpenAIApi
    {
        private string _openAIApiHost = "https://api.openai.com/v1";
        private const int _apiTimeoutLength = 30000;

        private RestClient _client = null;
        private string _organization = string.Empty;
        private string _apiKey = null;

        #region Constructors

        /// <summary>
        /// Default constructor that provides the secret key
        /// </summary>
        /// <param name="secretKey"></param>
        public OpenAIApi( string secretKey, string organization )
        {
            _client = GetOpenAIClient( secretKey );
            _organization = organization;
            _apiKey = secretKey;
        }

        /// <summary>
        /// Create a new instance of the client.
        /// </summary>
        /// <param name="secretKey"></param>
        /// <param name="organization"></param>
        /// <param name="apiHostUrl"></param>
        public OpenAIApi( string secretKey, string organization, string apiHostUrl )
        {
            _openAIApiHost = apiHostUrl;
            _organization = organization;
            _apiKey = secretKey;

            _client = GetOpenAIClient( secretKey );
        }

        #endregion

        #region Properties

        /// <summary>
        /// The host address of the OpenAI Api Service.
        /// </summary>
        public string ApiHostUrl
        {
            get
            {
                return _openAIApiHost;
            }
        }

        #endregion

        #region Utilities
        /// <summary>
        /// Internal method to get the api client needed for requests
        /// </summary>
        /// <param name="secretKey"></param>
        /// <returns></returns>
        private RestClient GetOpenAIClient( string secretKey )
        {
            var client = new RestClient( _openAIApiHost );
            client.Authenticator = new JwtAuthenticator( secretKey );
            client.Timeout = _apiTimeoutLength;

            // Due to issues with RestSharp's deserialization of properties with forward slashes
            // 
            client.AddHandler( "application/json", OpenAINewtonsoftJsonSerializer.Default );

            return client;
        }

        /// <summary>
        /// Creates a standard rest request for OpenAI
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        private RestSharp.RestRequest GetOpenAIRequest( string resource, Method method = Method.GET )
        {
            var request = new RestSharp.RestRequest( resource, method );
            request.AddHeader( "OpenAI-Organization", _organization );

            return request;
        }

        #endregion

        #region Requests
        /// <summary>
        /// Performs a chat completions request on the OpenAI API.
        /// </summary>
        /// <param name="completionRequest"></param>
        /// <returns></returns>
        internal async Task<OpenAIChatCompletionsResponse> GetChatCompletions( OpenAIChatCompletionsRequest completionRequest )
        {
            var request = GetOpenAIRequest( "chat/completions", Method.POST );

            request.AddParameter( "application/json", completionRequest.ToJson(), ParameterType.RequestBody );

            // Execute request
            var response = await _client.ExecuteTaskAsync<OpenAIChatCompletionsResponse>( request ).ConfigureAwait( false );

            if ( response.StatusCode == System.Net.HttpStatusCode.OK )
            {
                return response.Data;
            }

            string message = ParseErrorMessage( response );

            return new OpenAIChatCompletionsResponse() { IsSuccessful = false, ErrorMessage = message };
        }

        /// <summary>
        /// Performs a chat completions request on the OpenAI API and streams the output.
        /// </summary>
        /// <param name="completionRequest"></param>
        /// <param name="responseStream"></param>
        /// <returns></returns>
        internal async Task StreamChatCompletions( OpenAIChatCompletionsRequest completionRequest, Stream responseStream )
        {
            // RestSharp does not directly support streaming so we'll use HttpClient

            var httpClient = new HttpClient();

            var requestBody = completionRequest.ToJson();
            var requestContent = new StringContent( requestBody, Encoding.UTF8, "application/json" );

            var request = new HttpRequestMessage( HttpMethod.Post, $"{_openAIApiHost}/chat/completions" )
            {
                Headers =
                {
                    { "Authorization", $"Bearer {_apiKey}" }
                },
                Content = requestContent
            };

            var response = await httpClient.SendAsync( request, HttpCompletionOption.ResponseHeadersRead );
            response.EnsureSuccessStatusCode();

            var responseStreamFromOpenAI = await response.Content.ReadAsStreamAsync();
            var reader = new StreamReader( responseStreamFromOpenAI );
            var writer = new StreamWriter( responseStream ) { AutoFlush = true };

            while ( !reader.EndOfStream )
            {
                var line = await reader.ReadLineAsync();
                if ( line != null && line.StartsWith( "data: " ) )
                {
                    var jsonData = line.Substring( 6 ).Trim();
                    if ( !string.IsNullOrEmpty( jsonData ) && jsonData != "[DONE]" )
                    {
                        try
                        {
                            var jsonElement = JsonConvert.DeserializeObject<JObject>( jsonData );
                            if ( jsonElement?["choices"]?[0]?["delta"]?["content"] != null )
                            {
                                var chunk = jsonElement["choices"][0]["delta"]["content"].ToString();
                                await writer.WriteAsync( chunk );
                                await writer.FlushAsync();
                            }
                        }
                        catch ( JsonException )
                        {
                            // Ignore any parsing errors
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Performs a text completions request on the OpenAI API.
        /// </summary>
        /// <param name="completionRequest"></param>
        /// <returns></returns>
        internal async Task<OpenAITextCompletionsResponse> GetTextCompletions( OpenAITextCompletionsRequest completionRequest )
        {
            var request = GetOpenAIRequest( "completions", Method.POST );

            request.AddParameter( "application/json", completionRequest.ToJson(), ParameterType.RequestBody );

            // Execute request
            var response = await _client.ExecuteTaskAsync<OpenAITextCompletionsResponse>( request ).ConfigureAwait( false );

            if ( response.StatusCode == System.Net.HttpStatusCode.OK )
            {
                return response.Data;
            }

            string message = ParseErrorMessage( response );

            return new OpenAITextCompletionsResponse() { IsSuccessful = false, ErrorMessage = message };
        }

        /// <summary>
        /// Performs a moderations request on the OpenAI API.
        /// </summary>
        /// <param name="moderationRequest"></param>
        /// <returns></returns>
        internal async Task<OpenAIModerationsResponse> GetModerations( OpenAIModerationsRequest moderationRequest )
        {
            var request = GetOpenAIRequest( "moderations", Method.POST );

            request.AddParameter( "application/json", moderationRequest.ToJson(), ParameterType.RequestBody );

            // Execute request
            var response = await _client.ExecuteTaskAsync<OpenAIModerationsResponse>( request );

            if ( response.StatusCode == System.Net.HttpStatusCode.OK )
            {
                return response.Data;
            }

            string message = ParseErrorMessage( response );

            return new OpenAIModerationsResponse() { IsSuccessful = false, ErrorMessage = message };
        }

        private static string ParseErrorMessage( IRestResponse response )
        {
            // Process the error response.
            string message = null;

            var errorResponse = response.Content.FromJsonDynamicOrNull() as IDictionary<string, object>;
            if ( errorResponse != null && errorResponse.ContainsKey( "error" ) )
            {
                var errorInfo = errorResponse["error"] as IDictionary<string, object>;
                if ( errorInfo != null && errorInfo.ContainsKey( "message" ) )
                {
                    message = errorInfo["message"].ToStringSafe();
                }
            }

            //  If there is no extended error information, return the response status description.
            if ( message.IsNullOrWhiteSpace() )
            {
                message = response.ErrorMessage;
                if ( string.IsNullOrWhiteSpace( message ) )
                {
                    message = response.StatusDescription;
                }
            }

            return message;
        }

        #endregion

    }
}

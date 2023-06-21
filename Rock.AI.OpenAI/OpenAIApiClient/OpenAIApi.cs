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


using System.Threading.Tasks;
using RestSharp;
using RestSharp.Authenticators;
using Rock.AI.OpenAI.OpenAIApiClient.Classes.TextCompletions;
using Rock.AI.OpenAI.OpenAIApiClient.Classes.Moderations;
using Rock.AI.OpenAI.OpenAIApiClient.Enums;
using Rock.AI.OpenAI.OpenAIApiClient.Classes.ChatCompletions;

namespace Rock.AI.OpenAI.OpenAIApiClient
{
    internal class OpenAIApi
    {
        private const string _openAIApiHost = "https://api.openai.com/v1";
        private const int _apiTimeoutLength = 30000;

        private RestClient _client = null;
        private string _organization = string.Empty;

        #region Static Properties
        public const OpenAIModel OpenAIDefaultTextCompletionsModel = OpenAIModel.DaVinci3;
        public const OpenAIModel OpenAIDefaultChatCompletionsModel = OpenAIModel.GPT4;
        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor that provides the secret key
        /// </summary>
        /// <param name="secretKey"></param>
        public OpenAIApi( string secretKey, string organization )
        {
            _client = GetOpenAIClient( secretKey );
            _organization = organization;
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

            return client;
        }

        /// <summary>
        /// Creates a standard rest request for OpenAI
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        private RestRequest GetOpenAIRequest( string resource, Method method = Method.GET )
        {
            var request = new RestRequest( resource, method );
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
            else
            {
                return new OpenAIChatCompletionsResponse() { IsSuccessful = false, ErrorMessage = response.ErrorMessage };
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
            else
            {
                return new OpenAITextCompletionsResponse() { IsSuccessful = false, ErrorMessage = response.ErrorMessage };
            }
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
            else
            {
                return new OpenAIModerationsResponse() { IsSuccessful = false, ErrorMessage = response.ErrorMessage };
            }
        }

        
        #endregion

    }
}

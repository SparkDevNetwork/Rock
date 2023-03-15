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
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using RestSharp;
using RestSharp.Authenticators;
using Rock.AI.Classes.Completions;
using Rock.AI.OpenAI.OpenAIApiClient.Classes;
using Rock.AI.OpenAI.OpenAIApiClient.Classes.Completions;
using Rock.AI.OpenAI.OpenAIApiClient.Classes.Moderations;

namespace Rock.AI.OpenAI.OpenAIApiClient
{
    internal class OpenAIApi
    {

        private const string _openAIApiHost = "https://api.openai.com/v1";
        private const int _apiTimeoutLength = 30000;
        private const string _defaultGptModel = "gpt-3.5-turbo";


        private RestClient _client = null;
        private string _organization = string.Empty;
        

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
        private RestRequest GetOpenAIRequest( string resource )
        {
            var request = new RestRequest( resource );
            request.AddHeader( "OpenAI-Organization", _organization );

            return request; 
        }

        #endregion

        #region Requests
        /// <summary>
        /// Performs a completions request on the OpenAI API.
        /// </summary>
        /// <param name="completionRequest"></param>
        /// <returns></returns>
        internal async Task<OpenAICompletionsResponse> GetCompletions( OpenAICompletionsRequest completionRequest )
        {
            var request = GetOpenAIRequest( "completions" );
                                    
            request.AddParameter( "application/json", completionRequest.ToJson(), ParameterType.RequestBody );

            // Execute request
            var response = await _client.ExecuteTaskAsync<OpenAICompletionsResponse>( request );

            if ( response.StatusCode == System.Net.HttpStatusCode.OK )
            {
                return response.Data;
            }

            return null;
        }

        /// <summary>
        /// Performs a moderations request on the OpenAI API.
        /// </summary>
        /// <param name="moderationRequest"></param>
        /// <returns></returns>
        internal async Task<OpenAIModerationsResponse> GetModerations( OpenAIModerationsRequest moderationRequest )
        {
            var request = GetOpenAIRequest( "moderations" );

            request.AddParameter( "application/json", moderationRequest.ToJson(), ParameterType.RequestBody );

            // Execute request
            var response = await _client.ExecuteTaskAsync<OpenAIModerationsResponse>( request );

            if ( response.StatusCode == System.Net.HttpStatusCode.OK )
            {
                return response.Data;
            }

            return null;
        }
        #endregion

    }
}

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
        #endregion

        #region Requests
        /// <summary>
        /// Performs a completion on the OpenAI API.
        /// </summary>
        /// <param name="completionRequest"></param>
        /// <returns></returns>
        public async Task<OpenAICompletionResponse> GetCompletion( CompletionRequest completionRequest )
        {
            var request = new RestRequest( "completions" );
            request.AddHeader( "OpenAI-Organization", _organization );

            // Create request
            var openAICompletionRequest = new OpenAICompletionRequest();
            openAICompletionRequest.Model = completionRequest.Model;
            openAICompletionRequest.Prompt = completionRequest.Prompt;
            openAICompletionRequest.Temperature = completionRequest.Temperature;
            openAICompletionRequest.N = completionRequest.DesiredCompletionCount;

            // Provide a default model
            if ( openAICompletionRequest.Model.IsNullOrWhiteSpace() )
            {
                openAICompletionRequest.Model = _defaultGptModel;
            }

            request.AddParameter( "application/json", openAICompletionRequest.ToJson(), ParameterType.RequestBody );

            // Execute request
            var response = await _client.ExecuteTaskAsync<OpenAICompletionResponse>( request );

            if ( response.StatusCode == System.Net.HttpStatusCode.OK )
            {
                return response.Data;
            }

            return null;
        }
        #endregion

    }
}

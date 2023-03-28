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
using Newtonsoft.Json;
using Rock.AI.Classes.TextCompletions;

namespace Rock.AI.OpenAI.OpenAIApiClient.Classes.TextCompletions
{
    /// <summary>
    /// The Reponse object for a completion.
    /// </summary>
    internal class OpenAITextCompletionsResponse
    {
        #region Properties

        /// <summary>
        /// Unique identifier for the completion request.
        /// </summary>
        [JsonProperty( "id" )]
        public string Id { get; set; }

        /// <summary>
        /// This will always be the text of "text_completion"
        /// </summary>
        [JsonProperty( "object" )]
        public string Object { get; set; }

        /// <summary>
        /// Unix timestamp that indicates when the completion request was created.
        /// </summary>
        [JsonProperty( "created" )]
        public long Created { get; set; }

        /// <summary>
        /// Specifies the ID of the language model used to generate the completion
        /// </summary>
        [JsonProperty( "model" )]
        public string Model { get; set; }

        /// <summary>
        ///  Array of one or more completion candidates
        /// </summary>
        [JsonProperty( "choices" )]
        public List<OpenAITextCompletionsResponseChoice> Choices { get; set; }

        /// <summary>
        /// Information on the resource usage of the request.
        /// </summary>
        [JsonProperty( "usage" )]
        public OpenAITextCompletionsResponseUsage Usage { get; set; }

        /// <summary>
        /// Determines if the request was successful.
        /// </summary>
        public bool IsSuccessful { get; set; } = true;

        /// <summary>
        /// Error messages from the request.
        /// </summary>
        public string ErrorMessage { get; set; }

        #endregion

        #region Methods
        /// <summary>
        /// Converst the OpenAI completion response to a generic response.
        /// </summary>
        /// <returns></returns>
        public TextCompletionsResponse AsTextCompletionsResponse()
        {
            var response = new TextCompletionsResponse();

            response.IsSuccessful = this.IsSuccessful;
            response.ErrorMessage = this.ErrorMessage;

            if ( this.IsSuccessful )
            {
                response.Id = this.Id;
                response.TokensUsed = this.Usage.TotalTokens;
                response.CompletionDateTime = DateTimeOffset.FromUnixTimeSeconds( this.Created ).UtcDateTime;

                foreach ( var choice in this.Choices )
                {
                    response.Choices.Add( choice.AsCompletionResponseChoice() );
                }
            }

            return response;
        }

        #endregion
    }
}

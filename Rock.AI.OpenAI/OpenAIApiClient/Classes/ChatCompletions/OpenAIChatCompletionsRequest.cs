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
using System.Linq;

using Newtonsoft.Json;

using Rock.AI.Classes.ChatCompletions;
using Rock.AI.OpenAI.Utilities;

namespace Rock.AI.OpenAI.OpenAIApiClient.Classes.ChatCompletions
{
    /// <summary>
    /// The Request object for a chat completion.
    /// </summary>
    internal class OpenAIChatCompletionsRequest
    {
        /// <summary>
        /// The string representation of the model to use. 
        /// </summary>
        [JsonProperty( "model" )]
        public string Model { get; set; }

        /// <summary>
        /// The prompt for the completion.
        /// </summary>
        [JsonProperty( "messages" )]
        public List<OpenAIChatCompletionsRequestMessage> Messages { get; set; } = new List<OpenAIChatCompletionsRequestMessage>();

        /// <summary>
        /// Max Tokens to allow. For now we'll default this to be the max size.
        /// </summary>
        [JsonProperty( "max_tokens", NullValueHandling = NullValueHandling.Ignore )]
        public int MaxTokens
        {
            get
            {
                if ( _maxTokens != 0 )
                {
                    return _maxTokens;
                }

                // Estimate number of tokens in the messages content
                var messagesContent = OpenAIUtilities.TokenCount( string.Join( " ", this.Messages.Select( m => m.Content ) ) );
                var messagesRoles = OpenAIUtilities.TokenCount( string.Join( " ", this.Messages.Select( m => m.Role ) ) );

                // Add a couple of extra tokens:
                // When using the API, you should be aware that the total tokens count includes not only the tokens in your messages
                // but also a few extra tokens for internal formatting purposes.
                var messagesTokenCount = messagesContent + messagesRoles + ( 12 * this.Messages.Count );

                // As of July 2024 the most common output token max.
                const int defaultMaxOutputTokens = 4096;

                // Return the difference
                return defaultMaxOutputTokens - messagesTokenCount;
            }
            set
            {
                _maxTokens = value;
            }
        }
        private int _maxTokens = 0;

        /// <summary>
        /// Temperature
        /// </summary>
        [JsonProperty( "temperature" )]
        public double Temperature { get; set; }

        /// <summary>
        /// Diversity of the generated output.
        /// </summary>
        [JsonProperty( "top_p" )]
        public int TopP { get; set; }

        /// <summary>
        /// Whether to stream the request.
        /// </summary>
        [JsonProperty( "stream" )]
        public bool Stream { get; set; }

        /// <summary>
        /// Parameter that can be used to specify a sequence of tokens that the model should stop generating at.
        /// </summary>
        [JsonProperty( "stop" )]
        public string Stop { get; set; }

        /// <summary>
        /// Convert a generic AI completion request to a OpenAI completion request.
        /// </summary>
        /// <param name="request"></param>
        internal OpenAIChatCompletionsRequest( ChatCompletionsRequest request )
        {
            this.Model = request.Model;
            this.Temperature = request.Temperature;
            this.MaxTokens = request.MaxTokens;

            // Convert messages
            foreach ( var message in request.Messages )
            {
                this.Messages.Add( new OpenAIChatCompletionsRequestMessage( message ) );
            }
        }
    }
}

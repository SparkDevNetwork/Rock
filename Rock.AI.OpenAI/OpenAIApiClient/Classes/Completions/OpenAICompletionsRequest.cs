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

using Newtonsoft.Json;
using Rock.AI.Classes.Completions;
using Rock.AI.OpenAI.Provider;

namespace Rock.AI.OpenAI.OpenAIApiClient.Classes.Completions
{
    /// <summary>
    /// The Request object for a completion.
    /// </summary>
    internal class OpenAICompletionsRequest
    {
        /// <summary>
        /// The model to use.
        /// </summary>
        [JsonProperty( "model" )]
        public string Model { get; set; }

        /// <summary>
        /// The prompt for the completion.
        /// </summary>
        [JsonProperty( "prompt" )]
        public string Prompt { get; set; }

        /// <summary>
        /// Max Tokens to allow.
        /// </summary>
        [JsonProperty( "max_tokens" )]
        public int MaxTokens { get; set; }

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
        /// The number of choices to return.
        /// </summary>
        [JsonProperty( "n" )]
        public int N { get; set; }

        /// <summary>
        /// Whether to stream the request.
        /// </summary>
        [JsonProperty( "stream" )]
        public bool Stream { get; set; }

        /// <summary>
        /// Request the model to return the log probability of each generated token in the output text.
        /// </summary>
        [JsonProperty( "logprobs" )]
        public int Logprobs { get; set; }

        /// <summary>
        /// Parameter that can be used to specify a sequence of tokens that the model should stop generating at.
        /// </summary>
        [JsonProperty( "stop" )]
        public string Stop { get; set; }

        /// <summary>
        /// Convert a generic AI completion request to a OpenAI completion request.
        /// </summary>
        /// <param name="request"></param>
        internal OpenAICompletionsRequest ( CompletionsRequest request )
        {
            
            this.Model = request.Model;
            this.Prompt = request.Prompt;
            this.Temperature = request.Temperature;
            this.N = request.DesiredCompletionCount;

            // Provide a default model
            if ( this.Model.IsNullOrWhiteSpace() )
            {
                this.Model = OpenAIApi.OpenAIDefaultCompletionsModel;
            }
        }
    }
}

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

namespace Rock.AI.OpenAI.OpenAIApiClient.Classes.ChatCompletions
{
    /// <summary>
    /// The Usage data for the Response object for a completion.
    /// </summary>
    internal class OpenAIChatCompletionsResponseUsage
    {
        /// <summary>
        /// How many tokens the prompt consumed.
        /// </summary>
        [JsonProperty( "prompt_tokens" )]
        public int PromptTokens { get; set; }

        /// <summary>
        /// How many tokens the completion consumed.
        /// </summary>
        [JsonProperty( "completion_tokens" )]
        public int CompletionTokens { get; set; }

        /// <summary>
        /// The total number of tokens the completion consumed.
        /// </summary>
        [JsonProperty( "total_tokens" )]
        public int TotalTokens { get; set; }
    }
}

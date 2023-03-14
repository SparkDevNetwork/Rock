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
using Newtonsoft.Json;

namespace Rock.AI.OpenAI.OpenAIApiClient.Classes
{
    /// <summary>
    /// The Reponse object for a completion.
    /// </summary>
    internal class OpenAICompletionResponse
    {
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
        public int Created { get; set; }

        /// <summary>
        /// Specifies the ID of the language model used to generate the completion
        /// </summary>
        [JsonProperty( "model" )]
        public string Model { get; set; }

        /// <summary>
        ///  Array of one or more completion candidates
        /// </summary>
        [JsonProperty( "choices" )]
        public List<OpenAICompletionResponseChoice> Choices { get; set; }

        /// <summary>
        /// Information on the resource usage of the request.
        /// </summary>
        [JsonProperty( "usage" )]
        public OpenAICompletionResponseUsage Usage { get; set; }
    }
}

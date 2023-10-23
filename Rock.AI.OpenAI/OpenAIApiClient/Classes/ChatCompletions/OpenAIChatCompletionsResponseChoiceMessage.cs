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
using Rock.AI.Classes.TextCompletions;
using Rock.AI.OpenAI.OpenAIApiClient.Enums;

namespace Rock.AI.OpenAI.OpenAIApiClient.Classes.ChatCompletions
{
    /// <summary>
    /// The Choice data for the Response object for a completion.
    /// </summary>
    internal class OpenAIChatCompletionsResponseChoiceMessage
    {
        #region Properties

        /// <summary>
        /// The text reponse for the completion.
        /// </summary>
        [JsonProperty( "role" )]
        public OpenAIChatMessageRole Role { get; set; }

        /// <summary>
        /// The returned content of the completion.
        /// </summary>
        [JsonProperty( "content" )]
        public string Content { get; set; }

        #endregion
    }
}

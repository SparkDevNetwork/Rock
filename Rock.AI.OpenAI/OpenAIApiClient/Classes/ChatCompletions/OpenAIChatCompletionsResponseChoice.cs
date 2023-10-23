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
using Rock.AI.Classes.ChatCompletions;
using Rock.AI.Classes.TextCompletions;
using Rock.AI.OpenAI.Utilities;

namespace Rock.AI.OpenAI.OpenAIApiClient.Classes.ChatCompletions
{
    /// <summary>
    /// The Choice data for the Response object for a completion.
    /// </summary>
    internal class OpenAIChatCompletionsResponseChoice
    {
        #region Properties

        /// <summary>
        /// The text reponse for the completion.
        /// </summary>
        [JsonProperty( "message" )]
        public OpenAIChatCompletionsResponseChoiceMessage Message { get; set; }

        /// <summary>
        /// The returned order of the completion.
        /// </summary>
        [JsonProperty( "index" )]
        public int Index { get; set; }

        /// <summary>
        /// Information about the reason why the completion request was completed, such as whether it was successful or encountered
        /// an error. Possible values include: stop, timeout, length, input_empty, model_error, server_error
        /// </summary>
        [JsonProperty( "finish_reason" )]
        public string FinishReason { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Converts the OpenAI completion response choice to a generic one.
        /// </summary>
        /// <returns></returns>
        internal ChatCompletionsResponseChoice AsChatCompletionResponseChoice()
        {
            var completionResponseChoice = new ChatCompletionsResponseChoice();
            completionResponseChoice.Text = this.Message.Content;
            completionResponseChoice.Role = OpenAIUtilities.ConvertOpenAIChatRoleToRockChatRole( this.Message.Role );
            return completionResponseChoice;
        }

        #endregion
    }
}

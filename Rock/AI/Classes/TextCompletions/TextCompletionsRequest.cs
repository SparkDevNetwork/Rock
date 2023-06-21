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

namespace Rock.AI.Classes.TextCompletions
{
    /// <summary>
    /// The class for creating a new request for a completion.
    /// </summary>
    public class TextCompletionsRequest
    {
        /// <summary>
        /// The model to use for the completion. See the documentation for your provider for valid values.
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// The starting text or context that the language model uses to generate its completion or continuation. The prompt can be a sentence, a phrase, or a longer passage
        /// of text, and it provides the model with information on the desired tone, style, topic, and other relevant factors. By adjusting the prompt, users can control the
        /// output of the model and guide it towards a specific direction or outcome. In other words, the prompt helps to set the context and constraints for the generated text.
        /// </summary>
        public string Prompt { get; set; }

        /// <summary>
        /// The level of randomness or creativity in the generated text. See documentation for your provider for valid values.
        /// </summary>
        public double Temperature { get; set; } = 0.8;

        /// <summary>
        /// The number of completions to return for the given prompt.
        /// </summary>
        public int DesiredCompletionCount { get; set; } = 1;

        /// <summary>
        /// The maximum number of tokens the completion should be.
        /// </summary>
        public int MaxTokens { get; set; }
    }
}

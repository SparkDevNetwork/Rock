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

namespace Rock.AI.Agent
{
    /// <summary>
    /// Settings for the prompt information used by <see cref="Model.AISkillFunction"/>.
    /// </summary>
    internal class PromptInformationSettings
    {
        /// <summary>
        /// If the function is an <see cref="Enums.Core.AI.Agent.FunctionType.AIPrompt"/>
        /// and this is enabled, then the <see cref="Prompt"/> will be parsed by Lava before
        /// being sent to the langauge model.
        /// </summary>
        public bool PreRenderLava { get; set; }

        /// <summary>
        /// The temperature setting for the AI model. This controls the
        /// randomness of the output. 
        /// </summary>
        public decimal Temperature { get; set; } = 0.7M;

        /// <summary>
        /// The maximum number of tokens to generate in the response. If <c>null</c>
        /// then no limit will be imposed.
        /// </summary>
        public int? MaxTokens { get; set; }

        /// <summary>
        /// The raw prompt template that will be sent to the AI model.
        /// </summary>
        public string Prompt { get; set; }

        /// <summary>
        /// A JSON Schema object that defines the structure of input parameters
        /// for the prompt.
        /// </summary>
        public string PromptParametersSchema { get; set; }
    }
}

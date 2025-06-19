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

using Rock.ViewModels.Utility;
using Rock.Enums.Core.AI.Agent;

namespace Rock.ViewModels.Blocks.Core.AI.AISkillFunctionList
{
    /// <summary>
    /// The item details for the AI Skill Function List block.
    /// </summary>
    public class AISkillFunctionBag : EntityBagBase
    {
        /// <summary>
        /// The description of the function, which provides additional context
        /// or information about its intended purpose and functionality.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The type of function represented by this entity. This indicates
        /// how the function will be configured and executed in the language
        /// model.
        /// </summary>
        public FunctionType FunctionType { get; set; }

        /// <summary>
        /// The friendly name of the function that will be used to identify it
        /// in the UI.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A concise, but descriptive, hint to the language model that provides
        /// context about when this function should be used in response to an
        /// individual's input.
        /// </summary>
        public string UsageHint { get; set; }

        /// <summary>
        /// Indicates that the prompt should be rendered using Lava before being
        /// send to the language model.
        /// </summary>
        public bool PreRenderLava { get; set; }

        /// <summary>
        /// Determines the randomness of the response.
        /// </summary>
        public decimal Temperature { get; set; }

        /// <summary>
        /// The maximum number of tokens that the language model should consume.
        /// </summary>
        public int? MaxTokens { get; set; }

        /// <summary>
        /// The prompt text to be sent to the language model or executed by Lava.
        /// </summary>
        public string Prompt { get; set; }

        /// <summary>
        /// A JSON Schema object that defines the structure of input parameters.
        /// </summary>
        public string PromptParametersSchema { get; set; }
    }
}

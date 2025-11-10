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
using System.Collections.Generic;
using Rock.Enums.AI.Agent;

namespace Rock.ViewModels.Blocks.AI.AISkillToolList
{
    /// <summary>
    /// The item details for the AI Skill Tool List block.
    /// </summary>
    public class AISkillToolBag : EntityBagBase
    {
        /// <summary>
        /// The description of the tool, which provides additional context
        /// or information about its intended purpose and functionality.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The type of tool represented by this entity. This indicates
        /// how the tool will be configured and executed in the language
        /// model.
        /// </summary>
        public ToolType ToolType { get; set; }

        /// <summary>
        /// The instructions that provide context about when and how this
        /// tool should be executed.
        /// </summary>
        public List<ListItemBag> Instructions { get; set; }

        /// <summary>
        /// The friendly name of the tool that will be used to identify it
        /// in the UI.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The preamble text that will be displayed in the UI when an agent is
        /// executing this tool.
        /// </summary>
        public string Preamble { get; set; }

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
        /// The parameters that are defined for the prompt template.
        /// </summary>
        public List<ParameterSchemaBag> PromptParameters { get; set; } = new List<ParameterSchemaBag>();
    }
}

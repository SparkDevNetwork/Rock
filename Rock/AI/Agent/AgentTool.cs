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

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Rock.Enums.AI.Agent;

namespace Rock.AI.Agent
{
    /// <summary>
    /// Represents a tool that can be executed by an AI agent within the Rock
    /// AI framework. Supports native code, Lava templating, and semantic (AI
    /// prompt) tool types. Contains metadata about the tool's identity, usage,
    /// parameters, role, and execution settings.
    /// </summary>
    internal class AgentTool
    {
        /// <summary>
        /// AI tool names only support alphanumeric characters and spaces.
        /// </summary>
        private static readonly Regex InvalidNameCharacters = new Regex( @"[^a-zA-Z0-9_]", RegexOptions.Compiled );

        /// <summary>
        /// The unique identifier of this tool. If this is not filled in
        /// with a valid value then the tool will be ignored.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// The unique name used to identify this semantic tool within the skill.
        /// This name is how the tool is called from the language model or other
        /// orchestrations. Should be short, descriptive, and use PascalCase.
        /// Example: "TranslateToFrench", "SummarizeText", "CapitalOf"
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The key name derived from the name.
        /// </summary>
        public string Key => InvalidNameCharacters.Replace( Name, string.Empty );

        /// <summary>
        /// A short human-readable description of what the tool does. This is
        /// displayed in the UI and helps users understand the tool's purpose.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// A short human-readable message that will be displayed in the agent
        /// UI when the tool is executing.
        /// </summary>
        public string Preamble { get; set; } = string.Empty;

        /// <summary>
        /// The instructions to provide to the AI model about how to use this
        /// tool.
        /// </summary>
        public ToolInstructionSettings Instructions { get; set; } = new ToolInstructionSettings();

        /// <summary>
        /// The type of tool. <see cref="ToolType.ExecuteCode"/> is not a
        /// valid value and intances with this value will be ignored.
        /// </summary>
        public ToolType ToolType { get; set; } = ToolType.AIPrompt;

        /// <summary>
        /// The role helps the agent determine what AI model to use.
        /// </summary>
        /// <remarks>
        /// This is only valid when <see cref="ToolType"/> is set to
        /// <see cref="ToolType.AIPrompt"/>.
        /// </remarks>
        public ModelServiceRole Role { get; set; }

        /// <summary>
        /// The raw prompt template sent to the AI model. 
        /// </summary>
        public string Prompt { get; set; } = string.Empty;

        /// <summary>
        /// The parameters defined on the tool. This is only valid when
        /// <see cref="ToolType"/> is <see cref="ToolType.ExecuteLava"/>.
        /// </summary>
        public List<ParameterSchema> Parameters { get; set; } = new List<ParameterSchema>();

        /// <summary>
        /// If set to true, the prompt will be processed using Lava templating
        /// before being sent to the AI model. This is only applicable for AI
        /// Prompt tools.
        /// </summary>
        public bool EnableLavaPreRendering { get; set; }

        /// <summary>
        /// Controls the creativity of the AI's responses. 
        /// Lower values (e.g., 0.2) make output more focused and deterministic.
        /// Higher values (e.g., 0.8) increase randomness and creativity.
        /// Use lower temperatures for fact-based tasks, and higher for brainstorming or open-ended prompts.
        /// Default is 0.7. 
        /// </summary>
        /// <remarks>
        /// This is only valid when <see cref="ToolType"/> is set to
        /// <see cref="ToolType.AIPrompt"/>.
        /// </remarks>
        public double? Temperature { get; set; } = 0.7;

        /// <summary>
        /// Specifies the maximum number of tokens the AI can generate in its response.
        /// A token is roughly 4 characters or 0.75 words.
        /// Lower values speed up responses and reduce cost, while higher values allow more detailed output.
        /// Use 10–50 tokens for short answers, 100–300 for summaries, and 700+ for creative writing.
        /// </summary>
        /// <remarks>
        /// This is only valid when <see cref="ToolType"/> is set to
        /// <see cref="ToolType.AIPrompt"/>.
        /// </remarks>
        public int? MaxTokens { get; set; }
    }
}

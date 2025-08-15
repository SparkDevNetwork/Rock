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
    /// Represents a function or skill that can be executed by an AI agent within the Rock AI framework.
    /// Supports native code, Lava templating, and semantic (AI prompt) function types.
    /// Contains metadata about the function's identity, usage, parameters, role, and execution settings.
    /// </summary>
    internal class AgentFunction
    {
        /// <summary>
        /// AI function names only support alphanumeric characters and spaces.
        /// </summary>
        private static readonly Regex InvalidNameCharacters = new Regex( @"[^a-zA-Z0-9_]", RegexOptions.Compiled );

        /// <summary>
        /// The unique identifier of this function. If this is not filled in
        /// with a valid value then the function will be ignored.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// The unique name used to identify this semantic function within the plugin.
        /// This name is how the function is called from Semantic Kernel or other orchestrations.
        /// Should be short, descriptive, and use PascalCase.
        /// Example: "TranslateToFrench", "SummarizeText", "CapitalOf"
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The key name derived from the name.
        /// </summary>
        public string Key => InvalidNameCharacters.Replace( Name, string.Empty );

        /// <summary>
        /// A short human-readable description of what the semantic function does.
        /// This is displayed in the UI and helps users understand the function's purpose.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// A short of what the semantic function does. This is used to provide
        /// additional context to the language model. Leave blank when the name
        /// provides enough context.
        /// </summary>
        public string Instructions { get; set; } = string.Empty;

        /// <summary>
        /// The type of function:
        /// * ExecuteCode - Native code function
        /// * ExecuteLava - Lava function
        /// * AiPrompt - Semantic function (AI Prompt)
        /// </summary>
        public FunctionType FunctionType { get; set; } = FunctionType.AIPrompt;

        /// <summary>
        /// The role helps the agent determine what AI model to use.
        /// </summary>
        /// <remarks>
        /// This is only valid when <see cref="FunctionType"/> is set to
        /// <see cref="FunctionType.AIPrompt"/>.
        /// </remarks>
        public ModelServiceRole Role { get; set; }

        /// <summary>
        /// The raw prompt template sent to the AI model. 
        /// </summary>
        public string Prompt { get; set; } = string.Empty;

        /// <summary>
        /// The parameters defined on the function. This is only valid when
        /// <see cref="FunctionType"/> is <see cref="FunctionType.ExecuteLava"/>.
        /// </summary>
        public List<ParameterSchema> Parameters { get; set; } = new List<ParameterSchema>();

        /// <summary>
        /// If set to true, the prompt will be processed using Lava templating before being sent to the AI model. This 
        /// is only applicable for AI Prompt functions.
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
        /// This is only valid when <see cref="FunctionType"/> is set to
        /// <see cref="FunctionType.AIPrompt"/>.
        /// </remarks>
        public double? Temperature { get; set; } = 0.7;

        /// <summary>
        /// Specifies the maximum number of tokens the AI can generate in its response.
        /// A token is roughly 4 characters or 0.75 words.
        /// Lower values speed up responses and reduce cost, while higher values allow more detailed output.
        /// Use 10–50 tokens for short answers, 100–300 for summaries, and 700+ for creative writing.
        /// </summary>
        /// <remarks>
        /// This is only valid when <see cref="FunctionType"/> is set to
        /// <see cref="FunctionType.AIPrompt"/>.
        /// </remarks>
        public int? MaxTokens { get; set; }
    }
}

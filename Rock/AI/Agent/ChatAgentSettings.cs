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

using Rock.Enums.AI.Agent;

namespace Rock.AI.Agent
{
    /// <summary>
    /// Additional settings for <see cref="Model.AIAgent"/> that do not need to
    /// be directly queried from the database. These only apply when
    /// <see cref="Model.AIAgent.AgentType"/> is <see cref="AgentType.Chat"/>.
    /// </summary>
    internal class ChatAgentSettings
    {
        /// <summary>
        /// The token threshold before auto-summarization will be triggered
        /// when a new user message is added. This only applies to persisted
        /// sessions.
        /// </summary>
        public int AutoSummarizeThreshold { get; set; } = 2_000;

        /// <summary>
        /// Determines if system skills are excluded from this agent. These are
        /// standard skills and tools that would generally be considered
        /// required for other skills to operate correctly.
        /// </summary>
        public bool IsExcludingSystemSkills { get; set; }

        /// <summary>
        /// The role that the agent will use when determining which language
        /// model to use.
        /// </summary>
        public ModelServiceRole Role { get; set; }

        /// <summary>
        /// The Lava template that will be used to generate the current person
        /// details to be added to the system prompt of a chat agent. If blank
        /// then a default template will be used.
        /// </summary>
        public string CurrentPersonTemplate { get; set; }
    }
}

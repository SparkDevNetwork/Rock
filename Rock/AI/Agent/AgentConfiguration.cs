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

using Rock.Enums.Core.AI.Agent;

namespace Rock.AI.Agent
{
    /// <summary>
    /// Encapsulates the core configuration settings for a Rock AI agent,
    /// including agent identity, persona, provider, skill set, summarization threshold, and model role.
    /// This class is used by the chat agent factory and runtime to coordinate agent-specific behaviors and capabilities.
    /// </summary>
    internal class AgentConfiguration
    {
        /// <summary>
        /// Gets the unique identifier for this AI agent configuration.
        /// </summary>
        public int AgentId { get; }

        /// <summary>
        /// Gets the token threshold before auto-summarization will be triggered
        /// when a new user message is added. This only applies to persisted sessions.
        /// </summary>
        public int AutoSummarizeThreshold { get; }

        /// <summary>
        /// Gets the provider component responsible for supplying AI/model capabilities to this agent.
        /// </summary>
        public AgentProviderComponent Provider { get; }

        /// <summary>
        /// Gets the primary model service role used by the agent (e.g., Default, Code, Research).
        /// </summary>
        public ModelServiceRole Role { get; }

        /// <summary>
        /// Gets the persona string used to describe the agent’s role, behavior, or system prompt context.
        /// </summary>
        public string Persona { get; }

        /// <summary>
        /// Gets the collection of skills (semantic or native) enabled for this agent.
        /// </summary>
        public IReadOnlyCollection<SkillConfiguration> Skills { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentConfiguration"/> class with the specified settings.
        /// </summary>
        /// <param name="agentId">The unique identifier for the agent.</param>
        /// <param name="provider">The provider component responsible for supplying AI/model capabilities to this agent.</param>
        /// <param name="persona">The persona or system prompt context for this agent.</param>
        /// <param name="settings">The agent settings object, including summarization threshold and model role.</param>
        /// <param name="skills">A list of skills (semantic or native) enabled for this agent.</param>
        public AgentConfiguration(
            int agentId,
            AgentProviderComponent provider,
            string persona,
            AgentSettings settings,
            IReadOnlyList<SkillConfiguration> skills )
        {
            AgentId = agentId;
            AutoSummarizeThreshold = settings.AutoSummarizeThreshold;
            Provider = provider;
            Persona = persona ?? string.Empty;
            Role = settings.Role;
            Skills = skills ?? new List<SkillConfiguration>();
        }
    }
}
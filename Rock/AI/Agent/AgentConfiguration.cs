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

using Rock.Data;
using Rock.Enums.AI.Agent;
using Rock.Web.Cache.Entities;

namespace Rock.AI.Agent
{
    /// <summary>
    /// Encapsulates the core configuration settings for a Rock AI agent,
    /// including agent identity, instructions, provider, skill set, summarization
    /// threshold, and model role. This class is used by the chat agent
    /// factory and runtime to coordinate agent-specific behaviors and capabilities.
    /// </summary>
    internal class AgentConfiguration
    {
        /// <summary>
        /// Gets the unique identifier for this AI agent configuration.
        /// </summary>
        public int AgentId { get; }

        /// <inheritdoc cref="Model.AIAgent.AgentType"/>
        public AgentType AgentType { get; }

        /// <inheritdoc cref="Model.AIAgent.AudienceType"/>
        public AudienceType AudienceType { get; }

        /// <summary>
        /// Gets the token threshold before auto-summarization will be triggered
        /// when a new user message is added. This only applies to persisted sessions.
        /// </summary>
        public int AutoSummarizeThreshold { get; }

        /// <inheritdoc cref="Model.AIAgent.Name"/>
        public string Name { get; }

        /// <summary>
        /// Gets the provider component responsible for supplying AI/model capabilities to this agent.
        /// </summary>
        public AgentProviderComponent Provider { get; }

        /// <summary>
        /// Gets the primary model service role used by the agent (e.g., Default, Code, Research).
        /// </summary>
        public ModelServiceRole Role { get; }

        /// <inheritdoc cref="ChatAgentSettings.CurrentPersonTemplate"/>
        public string CurrentPersonTemplate { get; set; }

        /// <summary>
        /// Gets the instructions used to describe the agent’s role, behavior, or system prompt context.
        /// </summary>
        public string Instructions { get; }

        /// <summary>
        /// Gets the collection of skills (semantic or native) enabled for this agent.
        /// </summary>
        public IReadOnlyCollection<SkillConfiguration> Skills { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentConfiguration"/> class with the specified settings.
        /// </summary>
        /// <param name="agent">The cache object that represents the agent to define the configuration for.</param>
        /// <param name="skills">A list of skills (semantic or native) enabled for this agent.</param>
        /// <param name="provider">The provider component responsible for supplying AI/model capabilities to this agent.</param>
        internal AgentConfiguration( AIAgentCache agent, List<SkillConfiguration> skills, AgentProviderComponent provider )
        {
            AgentId = agent.Id;
            AgentType = agent.AgentType;
            AudienceType = agent.AudienceType;
            Instructions = agent.Instructions ?? string.Empty;
            Name = agent.Name;
            Provider = provider;
            Skills = skills ?? new List<SkillConfiguration>();

            var settings = agent.GetAdditionalSettings<ChatAgentSettings>();
            AutoSummarizeThreshold = settings.AutoSummarizeThreshold;
            Role = settings.Role;
            CurrentPersonTemplate = settings.CurrentPersonTemplate;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentConfiguration"/> class with the specified settings.
        /// </summary>
        /// <remarks>
        /// This should be used by unit testing only.
        /// </remarks>
        /// <param name="agentId">The unique identifier for the agent.</param>
        /// <param name="provider">The provider component responsible for supplying AI/model capabilities to this agent.</param>
        /// <param name="name">The name of the agent as configured in the UI.</param>
        /// <param name="agentType">The type of agent represented by this instance.</param>
        /// <param name="audienceType">The audience type for this agent, which determines its intended use case.</param>
        /// <param name="instructions">The instructions or system prompt context for this agent.</param>
        /// <param name="settings">The agent settings object, including summarization threshold and model role.</param>
        /// <param name="skills">A list of skills (semantic or native) enabled for this agent.</param>
        internal AgentConfiguration(
            int agentId,
            AgentProviderComponent provider,
            string name,
            AgentType agentType,
            AudienceType audienceType,
            string instructions,
            ChatAgentSettings settings,
            IReadOnlyList<SkillConfiguration> skills )
        {
            AgentId = agentId;
            AgentType = agentType;
            AudienceType = audienceType;
            AutoSummarizeThreshold = settings.AutoSummarizeThreshold;
            Name = name;
            Provider = provider;
            Instructions = instructions ?? string.Empty;
            Role = settings.Role;
            Skills = skills ?? new List<SkillConfiguration>();
        }
    }
}
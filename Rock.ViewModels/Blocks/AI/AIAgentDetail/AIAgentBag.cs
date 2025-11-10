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

using Rock.Enums.AI.Agent;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.AI.AIAgentDetail
{
    /// <summary>
    /// The item details for the AI Agent Detail block.
    /// </summary>
    public class AIAgentBag : EntityBagBase
    {
        /// <summary>
        /// The type of agent represented by this instance.
        /// </summary>
        public AgentType AgentType { get; set; }

        /// <summary>
        /// The intended audience for this agent, which can be used to help
        /// determine the appropraite functionality to expose.
        /// </summary>
        public AudienceType AudienceType { get; set; }

        /// <summary>
        /// The token threshold before auto-summarization will be triggered
        /// when a new user message is added. This only applies to persisted
        /// sessions.
        /// </summary>
        public int AutoSummarizeThreshold { get; set; }

        /// <summary>
        /// The binary file that contains the image to use as the avatar to
        /// represent the agent. This will be used in the administrative UI and
        /// the chat UI to represent the agent visually.
        /// </summary>
        public ListItemBag AvatarBinaryFile { get; set; }

        /// <summary>
        /// The description of the agent, which provides additional context or
        /// information about its intended purpose and functionality.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The instructions for the agent. This has different meanings
        /// depending on the type of agent.
        /// </summary>
        public string Instructions { get; set; }

        /// <summary>
        /// Determines if system skills are excluded from this agent. These are
        /// standard skills and tools that would generally be considered
        /// required for other skills to operate correctly.
        /// </summary>
        public bool IsExcludingSystemSkills { get; set; }

        /// <summary>
        /// The friendly name of the agent that will be used to identify it in the UI.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The role that this agent uses when determining which AI model to use.
        /// </summary>
        public ModelServiceRole Role { get; set; }

        /// <summary>
        /// The Lava template that will be used to generate the current person
        /// details to be added to the system prompt of a chat agent. If blank
        /// then a default template will be used.
        /// </summary>
        public string CurrentPersonTemplate { get; set; }

        /// <summary>
        /// A collection containing the Rock.Model.AIAgentSkill entities
        /// that represent the skills attached to this agent.
        /// </summary>
        public List<AgentSkillBag> Skills { get; set; }

        /// <summary>
        /// The URL friendly slug that will be used to uniquely identify this
        /// agent when used in MCP mode.
        /// </summary>
        public string Slug { get; set; }
    }
}

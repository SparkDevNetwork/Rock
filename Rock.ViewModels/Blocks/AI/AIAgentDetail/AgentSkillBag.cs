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

namespace Rock.ViewModels.Blocks.AI.AIAgentDetail
{
    /// <summary>
    /// Represents a single skill being viewed or edited in the Agent
    /// Detail block.
    /// </summary>
    public class AgentSkillBag
    {
        /// <summary>
        /// The unique identifier of the agent skill record that links the
        /// agent to the skill.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// The unique identifier of the skill being used by the agent.
        /// </summary>
        public Guid SkillGuid { get; set; }

        /// <summary>
        /// The name of the skill being used by the agent.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The user-friendly description of the skill being used by the agent.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The list of tools that are enabled for the skill to use.
        /// </summary>
        public List<Guid> EnabledTools { get; set; }

        /// <summary>
        /// The total number of tools the skill provides. This is the same set
        /// of tools that can be picked when editing the skill.
        /// </summary>
        public int AvailableToolCount { get; set; }

        /// <summary>
        /// The number of the skill's tools that are enabled for this agent.
        /// Only tools that still exist on the skill are counted, so this will
        /// never be greater than the available tool count.
        /// </summary>
        public int EnabledToolCount { get; set; }

        /// <summary>
        /// The configuration values for the skill.
        /// </summary>
        public Dictionary<string, string> ConfigurationValues { get; set; }
    }
}

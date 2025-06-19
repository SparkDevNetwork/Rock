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

namespace Rock.AI.Agent
{
    /// <summary>
    /// Represents the settings for a skill that has been attached to an agent.
    /// </summary>
    internal class AgentSkillSettings
    {
        /// <summary>
        /// The list of functions that are disabled for this skill. The UI will
        /// show the enabled functions, but we store as the list of disabled
        /// ones so that when a new function is added, it will automatically be
        /// included in the agent.
        /// </summary>
        public List<Guid> DisabledFunctions { get; set; } = new List<Guid>();

        /// <summary>
        /// The configuration values of the skill. When a skill is attached to
        /// an agent, there is UI to configure the skill settings. These are
        /// represented by the configuration values and are unique to the
        /// specific instance of the linkage between the agent and the skill.
        /// </summary>
        public Dictionary<string, string> ConfigurationValues { get; set; } = new Dictionary<string, string>();
    }
}

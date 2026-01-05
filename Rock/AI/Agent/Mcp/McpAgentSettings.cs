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

namespace Rock.AI.Agent.Mcp
{
    /// <summary>
    /// Additional settings for <see cref="Model.AIAgent"/>. These provide
    /// configuration details that only apply when the <see cref="Model.AIAgent.AgentType"/>
    /// is configured for <see cref="AgentType.Mcp"/>.
    /// </summary>
    internal class McpAgentSettings
    {
        /// <summary>
        /// Determines if system skills are excluded from this agent. These are
        /// standard skills and tools that would generally be considered
        /// required for other skills to operate correctly.
        /// </summary>
        public bool IsExcludingSystemSkills { get; set; }

        /// <summary>
        /// The slug that will be used with the REST API that provides the MCP
        /// server functionality. This should be unique across all agents.
        /// </summary>
        public string Slug { get; set; }
    }
}

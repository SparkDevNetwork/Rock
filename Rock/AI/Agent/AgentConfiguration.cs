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
    internal class AgentConfiguration
    {
        public int AgentId { get; }

        public IAgentProvider Provider { get; }

        public ModelServiceRole Role { get; }

        public string Persona { get; }

        public IReadOnlyCollection<SkillConfiguration> Skills { get; }

        public AgentConfiguration( int agentId, IAgentProvider provider, string persona, ModelServiceRole role, IReadOnlyList<SkillConfiguration> skills )
        {
            AgentId = agentId;
            Provider = provider;
            Persona = persona ?? string.Empty;
            Role = role;
            Skills = skills ?? new List<SkillConfiguration>();
        }
    }
}

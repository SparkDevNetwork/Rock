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
using Rock.Enums.AI.Agent;

namespace Rock.AI.Agent.Annotations
{
    /// <summary>
    /// Marks a property as ignored when being serialized for the specified
    /// type of agent.
    /// </summary>
    internal class JsonIgnoreAgentTypeAttribute : System.Attribute
    {
        /// <summary>
        /// The type of agent that will cause the property to be ignored.
        /// </summary>
        public AgentType AgentType { get; }

        /// <summary>
        /// Creates a new instance of <see cref="JsonIgnoreAgentTypeAttribute"/>.
        /// </summary>
        /// <param name="agentType">The type of agent that will cause the property to be ignored.</param>
        public JsonIgnoreAgentTypeAttribute( AgentType agentType )
        {
            AgentType = agentType;
        }
    }
}

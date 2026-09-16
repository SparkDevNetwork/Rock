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

using Rock.AI.Agent;
using Rock.Data;
using Rock.Enums.AI.Agent;
using Rock.Model;

namespace Rock.Tests.Shared.TestAccess.AI.Agent
{
    /// <summary>
    /// A concrete <see cref="AgentRequestContext"/> for use in tests, including
    /// tests for plugin skills.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The constructor on <see cref="AgentRequestContext"/> is internal so that
    /// plugins cannot derive from it in production code. This assembly has
    /// <c>InternalsVisibleTo</c> access, so it can provide a ready-made test
    /// double that a plugin test can construct directly and hand a mocked
    /// <see cref="RockContext"/>.
    /// </para>
    /// <para>
    /// The values a context exposes are set through the constructor because the
    /// base properties are read-only. If the base surface changes, only this
    /// type needs to be updated to match.
    /// </para>
    /// </remarks>
    public sealed class TestAgentRequestContext : AgentRequestContext
    {
        /// <inheritdoc/>
        public override int? AgentId { get; }

        /// <inheritdoc/>
        public override Guid? AgentGuid { get; }

        /// <inheritdoc/>
        public override string AgentName { get; }

        /// <inheritdoc/>
        public override AgentType AgentType { get; }

        /// <inheritdoc/>
        public override AudienceType AudienceType { get; }

        /// <inheritdoc/>
        public override Person CurrentPerson { get; }

        /// <inheritdoc/>
        public override string RootUrlPath { get; }

        /// <inheritdoc/>
        public override RockContext RockContext { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestAgentRequestContext"/> class.
        /// </summary>
        /// <param name="rockContext">The context tools should query through. This is typically a mocked context.</param>
        /// <param name="currentPerson">The person the agent is acting on behalf of, or <c>null</c> for anonymous.</param>
        /// <param name="agentType">The type of agent to report.</param>
        /// <param name="audienceType">The audience type to report.</param>
        /// <param name="rootUrlPath">The root URL path to report.</param>
        /// <param name="agentName">The agent name to report.</param>
        /// <param name="agentId">The agent identifier to report.</param>
        /// <param name="agentGuid">The agent unique identifier to report.</param>
        public TestAgentRequestContext(
            RockContext rockContext,
            Person currentPerson = null,
            AgentType agentType = AgentType.Chat,
            AudienceType audienceType = AudienceType.Internal,
            string rootUrlPath = "https://rock.example",
            string agentName = "Test Agent",
            int? agentId = null,
            Guid? agentGuid = null )
        {
            RockContext = rockContext;
            CurrentPerson = currentPerson;
            AgentType = agentType;
            AudienceType = audienceType;
            RootUrlPath = rootUrlPath;
            AgentName = agentName;
            AgentId = agentId;
            AgentGuid = agentGuid;
        }
    }
}

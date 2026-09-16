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

using Rock.AI.Agent;

namespace Rock.Tests.Shared.TestAccess.AI.Agent
{
    /// <summary>
    /// Test-only helpers for <see cref="AgentSkillComponent"/>.
    /// </summary>
    /// <remarks>
    /// The chat pipeline initializes a skill through an internal method so it is
    /// not part of the plugin contract. This assembly has
    /// <c>InternalsVisibleTo</c> access, so it can offer a stable way for tests -
    /// including plugin tests - to prime a skill with configuration values and a
    /// request context without standing up the full pipeline. If the underlying
    /// initializer changes, only this file needs to be updated to match.
    /// </remarks>
    public static class AgentSkillComponentExtensions
    {
        /// <summary>
        /// Primes a skill with a request context and no configuration values, as
        /// the chat pipeline would before invoking a tool.
        /// </summary>
        /// <param name="skill">The skill to initialize.</param>
        /// <param name="agentRequestContext">The request context the skill's tools should read from.</param>
        public static void InitializeForTesting( this AgentSkillComponent skill, AgentRequestContext agentRequestContext )
        {
            skill.InitializeForTesting( new Dictionary<string, string>(), agentRequestContext );
        }

        /// <summary>
        /// Primes a skill with configuration values and a request context, as the
        /// chat pipeline would before invoking a tool.
        /// </summary>
        /// <param name="skill">The skill to initialize.</param>
        /// <param name="configurationValues">The configuration values to apply to the skill.</param>
        /// <param name="agentRequestContext">The request context the skill's tools should read from.</param>
        public static void InitializeForTesting( this AgentSkillComponent skill, IReadOnlyDictionary<string, string> configurationValues, AgentRequestContext agentRequestContext )
        {
            skill.Initialize( configurationValues, agentRequestContext );
        }
    }
}

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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

using Rock.AI.Agent;

namespace Rock.Tests.Shared.TestAccess.AI.Agent
{
    /// <summary>
    /// Builds agent skill components for tests, primed with a request context as
    /// the chat pipeline would before invoking a tool.
    /// </summary>
    /// <remarks>
    /// The skill is constructed through <see cref="ActivatorUtilities"/> against the
    /// provided service provider, so a skill that takes dependencies beyond its
    /// logger has those resolved from that container the same way the real pipeline
    /// resolves them. The logger is passed explicitly, so a skill remains
    /// constructible even though the test container does not register logging. Pass
    /// the scoped app from <c>TestHelper.CreateScopedRockApp()</c> as the provider.
    /// </remarks>
    public static class AgentSkillTestFactory
    {
        /// <summary>
        /// Creates a skill of the requested type, resolving dependencies from the
        /// provided service provider and priming it with the request context.
        /// </summary>
        /// <typeparam name="TSkill">The skill component type to build.</typeparam>
        /// <param name="serviceProvider">The service provider dependencies are resolved from.</param>
        /// <param name="agentRequestContext">The request context the skill's tools should read from.</param>
        /// <returns>An initialized skill instance.</returns>
        public static TSkill CreateSkill<TSkill>( IServiceProvider serviceProvider, AgentRequestContext agentRequestContext )
            where TSkill : AgentSkillComponent
        {
            return CreateSkill<TSkill>( serviceProvider, new Dictionary<string, string>(), agentRequestContext );
        }

        /// <summary>
        /// Creates a skill of the requested type, resolving dependencies from the
        /// provided service provider and priming it with configuration values and
        /// the request context.
        /// </summary>
        /// <typeparam name="TSkill">The skill component type to build.</typeparam>
        /// <param name="serviceProvider">The service provider dependencies are resolved from.</param>
        /// <param name="configurationValues">The configuration values to apply to the skill.</param>
        /// <param name="agentRequestContext">The request context the skill's tools should read from.</param>
        /// <returns>An initialized skill instance.</returns>
        public static TSkill CreateSkill<TSkill>( IServiceProvider serviceProvider, IReadOnlyDictionary<string, string> configurationValues, AgentRequestContext agentRequestContext )
            where TSkill : AgentSkillComponent
        {
            // The logger is supplied explicitly, so it is matched by type and the
            // rest of the constructor is resolved from the service provider. A skill
            // that later takes additional dependencies gets them from the scoped
            // app's container without this factory changing.
            var skill = ActivatorUtilities.CreateInstance<TSkill>( serviceProvider, NullLogger<TSkill>.Instance );

            skill.InitializeForTesting( configurationValues, agentRequestContext );

            return skill;
        }
    }
}

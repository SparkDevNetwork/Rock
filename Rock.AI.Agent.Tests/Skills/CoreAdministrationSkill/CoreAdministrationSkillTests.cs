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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.AI.Agent;
using Rock.AI.Agent.Skills;
using Rock.Data;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.Security;
using Rock.Tests.Shared.TestAccess.AI.Agent;
using Rock.Tests.Shared.TestFramework;

namespace Rock.AI.Agent.Tests.Skills.CoreAdministrationSkill;

/// <summary>
/// Mocked-database unit tests for <see cref="CoreAdministrationSkill"/>. Each
/// tool's tests live in their own partial file, mirroring how the skill itself
/// is split into one file per tool. The shared setup helpers below are kept
/// here so every partial can reach them.
/// </summary>
/// <remarks>
/// These tests exercise a tool method directly against a mocked
/// <see cref="RockContext"/> from <c>TestHelper.CreateScopedRockApp()</c>. They
/// do not stand up the chat pipeline or call any language model, so they are
/// fast and deterministic and run without configuration.
/// </remarks>
[TestClass]
public partial class CoreAdministrationSkillTests
{
    #region Support

    /// <summary>
    /// Builds a skill instance primed with the request context, exactly as the
    /// chat pipeline would before invoking a tool.
    /// </summary>
    /// <param name="agentRequestContext">The request context the tools should read from.</param>
    /// <returns>An initialized <see cref="CoreAdministrationSkill"/>.</returns>
    private static Rock.AI.Agent.Skills.CoreAdministrationSkill CreateSkill( System.IServiceProvider serviceProvider, AgentRequestContext agentRequestContext )
    {
        return AgentSkillTestFactory.CreateSkill<Rock.AI.Agent.Skills.CoreAdministrationSkill>( serviceProvider, agentRequestContext );
    }

    /// <summary>
    /// Creates a request context backed by the mocked context, with sensible
    /// defaults for an internal, anonymous caller.
    /// </summary>
    /// <param name="rockContext">The mocked context the tools should query and save through.</param>
    /// <param name="currentPerson">The person the agent is acting on behalf of, or <c>null</c> for anonymous.</param>
    /// <param name="audienceType">The audience type to report.</param>
    /// <returns>A concrete <see cref="AgentRequestContext"/> for the test.</returns>
    private static AgentRequestContext CreateRequestContext( RockContext rockContext, Rock.Model.Person currentPerson = null, AudienceType audienceType = AudienceType.Internal )
    {
        return new TestAgentRequestContext( rockContext, currentPerson, audienceType: audienceType );
    }

    /// <summary>
    /// Grants EDIT to all users on the specified secured entity type so a
    /// write-path tool that authorizes the entity before saving is allowed to
    /// proceed.
    /// </summary>
    /// <typeparam name="TSecured">The secured entity type to grant EDIT on.</typeparam>
    /// <param name="rockContext">The mocked context to seed the rule into.</param>
    private static void AllowAllUsersToEdit<TSecured>( RockContext rockContext )
        where TSecured : IEntity
    {
        MockAuthorizationHelper.AllowAllUsers<TSecured>( rockContext, Authorization.EDIT );
    }

    #endregion
}

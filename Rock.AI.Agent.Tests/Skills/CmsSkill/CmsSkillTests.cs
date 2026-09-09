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
using Rock.Tests.Shared.TestAccess.AI.Agent;

namespace Rock.AI.Agent.Tests.Skills.CmsSkill;

/// <summary>
/// Mocked-database unit tests for <see cref="CmsSkill"/>. Each tool's tests live
/// in their own partial file; shared setup helpers are kept here.
/// </summary>
[TestClass]
public partial class CmsSkillTests
{
    #region Support

    private static Rock.AI.Agent.Skills.CmsSkill CreateSkill( System.IServiceProvider serviceProvider, AgentRequestContext agentRequestContext )
    {
        return AgentSkillTestFactory.CreateSkill<Rock.AI.Agent.Skills.CmsSkill>( serviceProvider, agentRequestContext );
    }

    private static AgentRequestContext CreateRequestContext( RockContext rockContext, Rock.Model.Person currentPerson = null, AudienceType audienceType = AudienceType.Internal )
    {
        return new TestAgentRequestContext( rockContext, currentPerson, audienceType: audienceType );
    }

    #endregion
}

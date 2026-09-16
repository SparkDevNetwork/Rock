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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.AI.Agent;
using Rock.AI.Agent.Skills;
using Rock.Data;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.Tests.Shared.TestAccess.AI.Agent;

namespace Rock.AI.Agent.Tests.Skills.CommunicationSkill;

/// <summary>
/// Mocked-database unit tests for <see cref="CommunicationSkill"/>. Each tool's
/// tests live in their own partial file; shared setup helpers are kept here.
/// </summary>
[TestClass]
public partial class CommunicationSkillTests
{
    #region Support

    private static Rock.AI.Agent.Skills.CommunicationSkill CreateSkill( System.IServiceProvider serviceProvider, AgentRequestContext agentRequestContext )
    {
        return AgentSkillTestFactory.CreateSkill<Rock.AI.Agent.Skills.CommunicationSkill>( serviceProvider, agentRequestContext );
    }

    private static AgentRequestContext CreateRequestContext( RockContext rockContext, Rock.Model.Person currentPerson = null, AudienceType audienceType = AudienceType.Internal )
    {
        return new TestAgentRequestContext( rockContext, currentPerson, audienceType: audienceType );
    }

    /// <summary>
    /// Seeds the communication list group type so the tools can resolve it by
    /// its well-known guid, and returns it.
    /// </summary>
    private static GroupType SeedCommunicationListGroupType( RockContext rockContext )
    {
        var groupType = new GroupType
        {
            Id = 100,
            Guid = Rock.SystemGuid.GroupType.GROUPTYPE_COMMUNICATIONLIST.AsGuid(),
            Name = "Communication List"
        };

        rockContext.Set<GroupType>().Add( groupType );

        return groupType;
    }

    /// <summary>
    /// Seeds an active communication list (a group of the communication list
    /// type) and returns it.
    /// </summary>
    private static Rock.Model.Group SeedCommunicationList( RockContext rockContext, int groupTypeId, int id, string name )
    {
        var group = new Rock.Model.Group
        {
            Id = id,
            Guid = Guid.NewGuid(),
            Name = name,
            GroupTypeId = groupTypeId,
            IsActive = true
        };

        rockContext.Set<Rock.Model.Group>().Add( group );

        return group;
    }

    #endregion
}

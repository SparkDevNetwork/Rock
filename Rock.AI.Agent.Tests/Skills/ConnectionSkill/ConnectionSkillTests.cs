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
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.AI.Agent;
using Rock.AI.Agent.Skills;
using Rock.Data;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.Tests.Shared.TestAccess.AI.Agent;

namespace Rock.AI.Agent.Tests.Skills.ConnectionSkill;

/// <summary>
/// Mocked-database unit tests for <see cref="ConnectionSkill"/>. Each tool's
/// tests live in their own partial file; shared setup helpers are kept here.
/// </summary>
[TestClass]
public partial class ConnectionSkillTests
{
    #region Support

    private static Rock.AI.Agent.Skills.ConnectionSkill CreateSkill( System.IServiceProvider serviceProvider, AgentRequestContext agentRequestContext )
    {
        return AgentSkillTestFactory.CreateSkill<Rock.AI.Agent.Skills.ConnectionSkill>( serviceProvider, agentRequestContext );
    }

    private static AgentRequestContext CreateRequestContext( RockContext rockContext, Rock.Model.Person currentPerson = null, AudienceType audienceType = AudienceType.Internal )
    {
        return new TestAgentRequestContext( rockContext, currentPerson, audienceType: audienceType );
    }

    /// <summary>
    /// Seeds a connection type with a single status, and an opportunity of that
    /// type, and returns the opportunity. The mocked context performs no
    /// navigation-property fixup, so the type and status relationships are
    /// wired by hand because request security falls back through them.
    /// </summary>
    private static ConnectionOpportunity SeedConnectionOpportunity( RockContext rockContext, int id )
    {
        var connectionType = new ConnectionType
        {
            Id = id + 1000,
            Guid = Guid.NewGuid(),
            Name = $"Connection Type {id}",
            ConnectionStatuses = new List<ConnectionStatus>()
        };

        rockContext.Set<ConnectionType>().Add( connectionType );

        var connectionStatus = new ConnectionStatus
        {
            Id = id + 2000,
            Guid = Guid.NewGuid(),
            Name = $"Status {id}",
            ConnectionTypeId = connectionType.Id,
            ConnectionType = connectionType,
            IsActive = true
        };

        rockContext.Set<ConnectionStatus>().Add( connectionStatus );
        connectionType.ConnectionStatuses.Add( connectionStatus );

        var connectionOpportunity = new ConnectionOpportunity
        {
            Id = id,
            Guid = Guid.NewGuid(),
            Name = $"Opportunity {id}",
            PublicName = $"Opportunity {id}",
            ConnectionTypeId = connectionType.Id,
            ConnectionType = connectionType,
            IsActive = true
        };

        rockContext.Set<ConnectionOpportunity>().Add( connectionOpportunity );

        return connectionOpportunity;
    }

    /// <summary>
    /// Seeds an existing request for the person in the opportunity and returns it.
    /// </summary>
    private static ConnectionRequest SeedConnectionRequest( RockContext rockContext, int id, ConnectionOpportunity connectionOpportunity, Rock.Model.Person person )
    {
        var connectionStatus = connectionOpportunity.ConnectionType.ConnectionStatuses.First();

        var connectionRequest = new ConnectionRequest
        {
            Id = id,
            Guid = Guid.NewGuid(),
            ConnectionOpportunityId = connectionOpportunity.Id,
            ConnectionOpportunity = connectionOpportunity,
            ConnectionTypeId = connectionOpportunity.ConnectionTypeId,
            ConnectionStatusId = connectionStatus.Id,
            ConnectionStatus = connectionStatus,
            PersonAliasId = person.PrimaryAliasId.Value,
            PersonAlias = person.Aliases.First(),
            ConnectionState = ConnectionState.Active,
            Comments = "Original comments",
            ConnectionRequestActivities = new List<ConnectionRequestActivity>()
        };

        rockContext.Set<ConnectionRequest>().Add( connectionRequest );

        return connectionRequest;
    }

    /// <summary>
    /// Gets the IdKey of the opportunity's status, for adding a new request.
    /// </summary>
    /// <remarks>
    /// The tool only falls back to the connection type's default status while
    /// the new request's Id is still 0. Everything in a test shares one mocked
    /// context, so an incidental save during the tool's security checks (such
    /// as creating a missing entity type for the security cache) assigns the
    /// pending request an Id early. Real Rock loads caches through their own
    /// context, so this does not happen there. Tests that add requests pass
    /// the status explicitly so they do not depend on that fallback.
    /// </remarks>
    private static string GetStatusIdKey( ConnectionOpportunity connectionOpportunity )
    {
        return Rock.Utility.IdHasher.Instance.GetHash( connectionOpportunity.ConnectionType.ConnectionStatuses.First().Id );
    }

    /// <summary>
    /// Makes new connection requests start with an empty activity collection.
    /// Entity Framework would lazy load an empty collection, but the mocked
    /// context does not, and the tool's result reads the activities.
    /// </summary>
    private static void SetupNewConnectionRequests( RockContext rockContext )
    {
        Mock.Get( rockContext.Set<ConnectionRequest>() )
            .Setup( m => m.Create() )
            .Returns( () => new ConnectionRequest
            {
                ConnectionRequestActivities = new List<ConnectionRequestActivity>()
            } );
    }

    /// <summary>
    /// Seeds a connector group for the opportunity, with the person as a member
    /// of the given status, and returns the group. The connector group has no
    /// campus, so it applies to every campus.
    /// </summary>
    private static Rock.Model.Group SeedConnectorGroup( RockContext rockContext, int id, ConnectionOpportunity connectionOpportunity, Rock.Model.Person member, GroupMemberStatus memberStatus )
    {
        var group = new Rock.Model.Group
        {
            Id = id,
            Guid = Guid.NewGuid(),
            Name = $"Connectors {id}",
            IsActive = true,
            Members = new List<GroupMember>()
        };

        rockContext.Set<Rock.Model.Group>().Add( group );

        var groupMember = new GroupMember
        {
            Id = id + 1000,
            Guid = Guid.NewGuid(),
            GroupId = group.Id,
            Group = group,
            PersonId = member.Id,
            Person = member,
            GroupMemberStatus = memberStatus
        };

        rockContext.Set<GroupMember>().Add( groupMember );
        group.Members.Add( groupMember );

        var connectorGroup = new ConnectionOpportunityConnectorGroup
        {
            Id = id + 2000,
            Guid = Guid.NewGuid(),
            ConnectionOpportunityId = connectionOpportunity.Id,
            ConnectionOpportunity = connectionOpportunity,
            ConnectorGroupId = group.Id,
            ConnectorGroup = group
        };

        rockContext.Set<ConnectionOpportunityConnectorGroup>().Add( connectorGroup );

        return group;
    }

    #endregion
}

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
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.AI.Agent;
using Rock.AI.Agent.Skills;
using Rock.Data;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.Security;
using Rock.SystemKey;
using Rock.Tests.Shared.TestAccess.AI.Agent;
using Rock.Tests.Shared.TestFramework;

namespace Rock.AI.Agent.Tests.Skills.CommunicationSkill;

/// <summary>
/// Mocked-database unit tests for <see cref="CommunicationSkill"/>. Each tool's
/// tests live in their own partial file; shared setup helpers are kept here.
/// </summary>
[TestClass]
public partial class CommunicationSkillTests
{
    private const int AllowedNumberId = 1;
    private const int RestrictedNumberId = 2;
    private const string InvalidNumberMessage = "does not correspond to a valid active SMS-enabled system phone number";
    private const string FromNumberNotAuthorizedMessage = "You are not authorized to send from this communication's SMS 'from' number.";
    private const string MultipleRecipientsMessage = "This communication has more than one recipient and cannot be sent by the agent.";

    #region Support

    /// <summary>
    /// Seeds an allowed SMS number with no security rules and a restricted
    /// SMS number that denies VIEW to all users.
    /// </summary>
    private static void SeedSystemPhoneNumbers( RockContext rockContext, Rock.Model.Person restrictedNumberAssignedTo = null, Rock.Model.Person allowedNumberAssignedTo = null )
    {
        rockContext.Set<SystemPhoneNumber>().Add( new SystemPhoneNumber
        {
            Id = AllowedNumberId,
            Guid = Guid.NewGuid(),
            Name = "Main",
            Number = "6235559001",
            IsActive = true,
            IsSmsEnabled = true,
            AssignedToPersonAliasId = allowedNumberAssignedTo?.PrimaryAliasId
        } );

        rockContext.Set<SystemPhoneNumber>().Add( new SystemPhoneNumber
        {
            Id = RestrictedNumberId,
            Guid = Guid.NewGuid(),
            Name = "Pastor",
            Number = "6235559002",
            IsActive = true,
            IsSmsEnabled = true,
            AssignedToPersonAliasId = restrictedNumberAssignedTo?.PrimaryAliasId
        } );

        MockAuthorizationHelper.DenyAllUsers<SystemPhoneNumber>( rockContext, Authorization.VIEW, RestrictedNumberId );
    }

    /// <summary>
    /// Seeds the person's saved default SMS 'from' number preference.
    /// </summary>
    private static void SeedSavedDefaultSmsNumber( RockContext rockContext, Rock.Model.Person person, int systemPhoneNumberId )
    {
        rockContext.Set<PersonPreference>().Add( new PersonPreference
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            PersonAliasId = person.PrimaryAliasId.Value,
            PersonAlias = person.Aliases.First(),
            Key = PersonPreferenceService.GetGlobalPreferencePrefix() + PersonPreferenceKey.DEFAULT_SMS_PHONE_NUMBER,
            Value = systemPhoneNumberId.ToString(),
            LastAccessedDateTime = RockDateTime.Now
        } );
    }

    /// <summary>
    /// Seeds a recipient row for the person on the communication.
    /// </summary>
    private static void SeedRecipient( RockContext rockContext, int id, Rock.Model.Communication communication, Rock.Model.Person recipient )
    {
        rockContext.Set<CommunicationRecipient>().Add( new CommunicationRecipient
        {
            Id = id,
            Guid = Guid.NewGuid(),
            CommunicationId = communication.Id,
            Communication = communication,
            PersonAliasId = recipient.PrimaryAliasId.Value,
            PersonAlias = recipient.Aliases.First()
        } );
    }

    /// <summary>
    /// Calls SendCommunication for a communication that is expected to pass
    /// every check and asserts that it succeeded.
    /// </summary>
    private static void SendCommunicationAndAssertSuccess( Rock.AI.Agent.Skills.CommunicationSkill skill, string communicationIdKey )
    {
        var result = skill.SendCommunication( communicationIdKey );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus(), string.Join( " ", result.GetErrorMessages() ?? new System.Collections.Generic.List<string>() ) );
    }

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

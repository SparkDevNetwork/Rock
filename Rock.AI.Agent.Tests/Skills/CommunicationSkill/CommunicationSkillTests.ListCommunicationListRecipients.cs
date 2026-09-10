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

using Rock.Configuration;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.Tests.Shared.TestAccess.AI.Agent;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;

namespace Rock.AI.Agent.Tests.Skills.CommunicationSkill;

public partial class CommunicationSkillTests
{
    #region ListCommunicationListRecipients

    [TestMethod]
    public void ListCommunicationListRecipients_WithNoMembers_ReturnsNoData()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var groupType = SeedCommunicationListGroupType( rockContext );
        var group = SeedCommunicationList( rockContext, groupType.Id, 200, "Weekly Newsletter" );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListCommunicationListRecipients( IdHasher.Instance.GetHash( group.Id ) );

        Assert.AreEqual( ToolStatus.NoData, result.GetStatus() );
    }

    [TestMethod]
    public void ListCommunicationListRecipients_WithMembers_ReturnsRecipients()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var groupType = SeedCommunicationListGroupType( rockContext );
        var group = SeedCommunicationList( rockContext, groupType.Id, 200, "Weekly Newsletter" );

        // Two active members: one with an explicit communication preference, one
        // that falls back to the person's preference, so both sides of the
        // preference resolution run. One person has a messaging-enabled number so
        // the SMS-capable path runs as well.
        var personA = new Rock.Model.Person
        {
            Id = 300,
            Guid = new Guid( "a3000001-0000-4000-8000-000000000001" ),
            FirstName = "Anna",
            NickName = "Anna",
            LastName = "Adams",
            Email = "anna@example.com",
            IsEmailActive = true,
            EmailPreference = EmailPreference.EmailAllowed,
            CommunicationPreference = CommunicationType.Email
        };

        var personB = new Rock.Model.Person
        {
            Id = 301,
            Guid = new Guid( "a3000002-0000-4000-8000-000000000002" ),
            FirstName = "Ben",
            NickName = "Ben",
            LastName = "Baker",
            Email = "ben@example.com",
            IsEmailActive = true,
            EmailPreference = EmailPreference.EmailAllowed,
            CommunicationPreference = CommunicationType.SMS
        };

        rockContext.Set<Rock.Model.Person>().Add( personA );
        rockContext.Set<Rock.Model.Person>().Add( personB );

        rockContext.Set<GroupMember>().Add( new GroupMember
        {
            Id = 310,
            Guid = new Guid( "a3000003-0000-4000-8000-000000000003" ),
            GroupId = group.Id,
            PersonId = personA.Id,
            Person = personA,
            GroupMemberStatus = GroupMemberStatus.Active,
            // An explicit preference: the "not RecipientPreference" branch.
            CommunicationPreference = CommunicationType.Email
        } );

        rockContext.Set<GroupMember>().Add( new GroupMember
        {
            Id = 311,
            Guid = new Guid( "a3000004-0000-4000-8000-000000000004" ),
            GroupId = group.Id,
            PersonId = personB.Id,
            Person = personB,
            GroupMemberStatus = GroupMemberStatus.Active,
            // No member preference: falls back to the person's preference.
            CommunicationPreference = CommunicationType.RecipientPreference
        } );

        // A messaging-enabled number for Anna so the SMS-capable lookup finds her.
        rockContext.Set<PhoneNumber>().Add( new PhoneNumber
        {
            Id = 320,
            Guid = new Guid( "a3000005-0000-4000-8000-000000000005" ),
            PersonId = personA.Id,
            Number = "6235553100",
            IsMessagingEnabled = true
        } );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListCommunicationListRecipients( IdHasher.Instance.GetHash( group.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    [TestMethod]
    public void ListCommunicationListRecipients_WithMissingList_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListCommunicationListRecipients( IdHasher.Instance.GetHash( 999 ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    [TestMethod]
    public void ListCommunicationListRecipients_WithNonCommunicationListGroup_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var group = new Rock.Model.Group
        {
            Id = 201,
            Guid = new Guid( "a2000001-0000-4000-8000-000000000001" ),
            Name = "Regular Group",
            GroupTypeId = 555,
            IsActive = true
        };

        rockContext.Set<Rock.Model.Group>().Add( group );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListCommunicationListRecipients( IdHasher.Instance.GetHash( group.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "not a communication list" ) ) );
    }

    #endregion
}

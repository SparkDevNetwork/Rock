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
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.AI.Agent.Utilities.CommunicationSkill;
using Rock.Configuration;
using Rock.Enums.AI.Agent;
using Rock.Tests.Shared.TestAccess.AI.Agent;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;

namespace Rock.AI.Agent.Tests.Skills.CommunicationSkill;

public partial class CommunicationSkillTests
{
    #region GetDraftCommunicationInstructions

    private const string NoValidDefaultNumberMessage = "No valid default SMS 'from' number";

    [TestMethod]
    public void GetDraftCommunicationInstructions_FromNumberWithoutView_ReturnsInvalidNumberError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var person = MockData.CreatePerson( rockContext, "Current", "Person" );
        var recipient = MockData.CreatePerson( rockContext, "Recipient", "Person" );
        SeedSystemPhoneNumbers( rockContext );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, person ) );

        var result = skill.GetDraftCommunicationInstructions( IdHasher.Instance.GetHash( recipient.Id ), AgentCommunicationType.Sms, fromNumberIdKey: IdHasher.Instance.GetHash( RestrictedNumberId ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( InvalidNumberMessage ) ) );
    }

    [TestMethod]
    public void GetDraftCommunicationInstructions_SavedDefaultWithoutView_IsIgnored()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var person = MockData.CreatePerson( rockContext, "Current", "Person" );
        var recipient = MockData.CreatePerson( rockContext, "Recipient", "Person" );
        SeedSystemPhoneNumbers( rockContext );
        SeedSavedDefaultSmsNumber( rockContext, person, RestrictedNumberId );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, person ) );

        var result = skill.GetDraftCommunicationInstructions( IdHasher.Instance.GetHash( recipient.Id ), AgentCommunicationType.Sms );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( NoValidDefaultNumberMessage ) ) );
    }

    [TestMethod]
    public void GetDraftCommunicationInstructions_AssignedNumberWithoutView_IsNotUsedAsDefault()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var person = MockData.CreatePerson( rockContext, "Current", "Person" );
        var recipient = MockData.CreatePerson( rockContext, "Recipient", "Person" );
        SeedSystemPhoneNumbers( rockContext, restrictedNumberAssignedTo: person );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, person ) );

        var result = skill.GetDraftCommunicationInstructions( IdHasher.Instance.GetHash( recipient.Id ), AgentCommunicationType.Sms );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( NoValidDefaultNumberMessage ) ) );
    }

    [TestMethod]
    public void GetDraftCommunicationInstructions_FromNumberWithView_IsAccepted()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var person = MockData.CreatePerson( rockContext, "Current", "Person" );
        var recipient = MockData.CreatePerson( rockContext, "Recipient", "Person" );
        SeedSystemPhoneNumbers( rockContext );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, person ) );

        var result = skill.GetDraftCommunicationInstructions( IdHasher.Instance.GetHash( recipient.Id ), AgentCommunicationType.Sms, fromNumberIdKey: IdHasher.Instance.GetHash( AllowedNumberId ) );

        AssertFromNumberWasResolved( result );
    }

    [TestMethod]
    public void GetDraftCommunicationInstructions_SavedDefaultWithView_IsUsed()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var person = MockData.CreatePerson( rockContext, "Current", "Person" );
        var recipient = MockData.CreatePerson( rockContext, "Recipient", "Person" );
        SeedSystemPhoneNumbers( rockContext );
        SeedSavedDefaultSmsNumber( rockContext, person, AllowedNumberId );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, person ) );

        var result = skill.GetDraftCommunicationInstructions( IdHasher.Instance.GetHash( recipient.Id ), AgentCommunicationType.Sms );

        AssertFromNumberWasResolved( result );
    }

    [TestMethod]
    public void GetDraftCommunicationInstructions_AssignedNumberWithView_IsUsedAsDefault()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var person = MockData.CreatePerson( rockContext, "Current", "Person" );
        var recipient = MockData.CreatePerson( rockContext, "Recipient", "Person" );
        SeedSystemPhoneNumbers( rockContext, allowedNumberAssignedTo: person );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, person ) );

        var result = skill.GetDraftCommunicationInstructions( IdHasher.Instance.GetHash( recipient.Id ), AgentCommunicationType.Sms );

        AssertFromNumberWasResolved( result );
    }

    /// <summary>
    /// Asserts that the SMS 'from' number was accepted. The mock has no active
    /// SMS transport, so drafting stops later with an unsupported medium
    /// error, but it must not stop on the 'from' number.
    /// </summary>
    private static void AssertFromNumberWasResolved( Rock.AI.Agent.AgentToolResult result )
    {
        var errors = result.GetErrorMessages() ?? new List<string>();

        Assert.IsFalse( errors.Any( m => m.Contains( InvalidNumberMessage ) ), string.Join( " ", errors ) );
        Assert.IsFalse( errors.Any( m => m.Contains( NoValidDefaultNumberMessage ) ), string.Join( " ", errors ) );
    }

    #endregion
}

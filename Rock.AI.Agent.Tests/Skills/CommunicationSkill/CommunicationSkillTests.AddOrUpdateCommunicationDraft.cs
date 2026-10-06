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

using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.AI.Agent.Utilities.CommunicationSkill;
using Rock.Configuration;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.Tests.Shared.TestAccess.AI.Agent;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;

namespace Rock.AI.Agent.Tests.Skills.CommunicationSkill;

public partial class CommunicationSkillTests
{
    #region AddOrUpdateCommunicationDraft

    [TestMethod]
    public void AddOrUpdateCommunicationDraft_ExistingDraftByOtherPerson_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var sender = MockData.CreatePerson( rockContext, "Sender", "Person" );
        var currentPerson = MockData.CreatePerson( rockContext, "Current", "Person" );
        var recipient = MockData.CreatePerson( rockContext, "Recipient", "Person" );
        var draft = SeedDraftCommunication( rockContext, 400, sender );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.AddOrUpdateCommunicationDraft(
            recipientIdKey: IdHasher.Instance.GetHash( recipient.Id ),
            communicationType: AgentCommunicationType.Email,
            draftedSubject: "Updated subject",
            draftedBody: "Updated body",
            existingDraftIdKey: IdHasher.Instance.GetHash( draft.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "not authorized to edit this draft" ) ) );
        Assert.AreEqual( "Draft", draft.Subject );
    }

    [TestMethod]
    public void AddOrUpdateCommunicationDraft_ExistingDraftBySender_IsNotRefusedForSecurity()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var sender = MockData.CreatePerson( rockContext, "Sender", "Person" );
        var recipient = MockData.CreatePerson( rockContext, "Recipient", "Person" );
        var draft = SeedDraftCommunication( rockContext, 400, sender );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, sender ) );

        // No email transport is configured in the mocked environment, so the
        // tool cannot finish building the draft. This only verifies that the
        // sender is not refused by the ownership check.
        var result = skill.AddOrUpdateCommunicationDraft(
            recipientIdKey: IdHasher.Instance.GetHash( recipient.Id ),
            communicationType: AgentCommunicationType.Email,
            draftedSubject: "Updated subject",
            draftedBody: "Updated body",
            existingDraftIdKey: IdHasher.Instance.GetHash( draft.Id ) );

        Assert.IsFalse( ( result.GetErrorMessages() ?? new System.Collections.Generic.List<string>() ).Any( m => m.Contains( "not authorized" ) ) );
    }

    #endregion
}

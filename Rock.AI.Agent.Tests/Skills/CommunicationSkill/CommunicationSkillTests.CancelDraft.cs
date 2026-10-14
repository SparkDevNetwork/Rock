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
using Rock.Data;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.Security;
using Rock.Tests.Shared.TestAccess.AI.Agent;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;

namespace Rock.AI.Agent.Tests.Skills.CommunicationSkill;

public partial class CommunicationSkillTests
{
    #region CancelDraft

    [TestMethod]
    public void CancelDraft_BySender_DeletesDraft()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var sender = MockData.CreatePerson( rockContext, "Sender", "Person" );
        var draft = SeedDraftCommunication( rockContext, 400, sender );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, sender ) );

        var result = skill.CancelDraft( IdHasher.Instance.GetHash( draft.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.IsFalse( rockContext.Set<Rock.Model.Communication>().Any( c => c.Id == draft.Id ) );
    }

    [TestMethod]
    public void CancelDraft_ByOtherPersonWithEdit_DeletesDraft()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var sender = MockData.CreatePerson( rockContext, "Sender", "Person" );
        var currentPerson = MockData.CreatePerson( rockContext, "Current", "Person" );
        var draft = SeedDraftCommunication( rockContext, 400, sender );
        MockAuthorizationHelper.AllowAllUsers<Rock.Model.Communication>( rockContext, Authorization.EDIT, draft.Id );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.CancelDraft( IdHasher.Instance.GetHash( draft.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.IsFalse( rockContext.Set<Rock.Model.Communication>().Any( c => c.Id == draft.Id ) );
    }

    [TestMethod]
    public void CancelDraft_ByOtherPersonWithoutEdit_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var sender = MockData.CreatePerson( rockContext, "Sender", "Person" );
        var currentPerson = MockData.CreatePerson( rockContext, "Current", "Person" );
        var draft = SeedDraftCommunication( rockContext, 400, sender );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.CancelDraft( IdHasher.Instance.GetHash( draft.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "not authorized" ) ) );
        Assert.IsTrue( rockContext.Set<Rock.Model.Communication>().Any( c => c.Id == draft.Id ) );
    }

    /// <summary>
    /// Seeds a transient email communication sent by the person and returns
    /// it, the same shape the draft tools create. The mocked context performs
    /// no navigation-property fixup, so the sender's alias is wired by hand
    /// because the draft tools compare against it.
    /// </summary>
    private static Rock.Model.Communication SeedDraftCommunication( RockContext rockContext, int id, Rock.Model.Person sender )
    {
        var communication = new Rock.Model.Communication
        {
            Id = id,
            Guid = Guid.NewGuid(),
            Name = "Draft",
            Subject = "Draft",
            CommunicationType = CommunicationType.Email,
            Status = CommunicationStatus.Transient,
            SenderPersonAliasId = sender.PrimaryAliasId,
            SenderPersonAlias = sender.Aliases.First()
        };

        rockContext.Set<Rock.Model.Communication>().Add( communication );

        return communication;
    }

    #endregion
}

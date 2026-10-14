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

using Rock.Configuration;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.Security;
using Rock.Tasks;
using Rock.Tests.Shared.TestAccess.AI.Agent;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;

namespace Rock.AI.Agent.Tests.Skills.CommunicationSkill;

public partial class CommunicationSkillTests
{
    #region SendCommunication

    [TestMethod]
    public void SendCommunication_ByOtherPersonWithoutEdit_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var sender = MockData.CreatePerson( rockContext, "Sender", "Person" );
        var currentPerson = MockData.CreatePerson( rockContext, "Current", "Person" );
        var draft = SeedDraftCommunication( rockContext, 400, sender );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.SendCommunication( IdHasher.Instance.GetHash( draft.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "not authorized" ) ) );
        Assert.AreEqual( CommunicationStatus.Transient, draft.Status );
        Assert.IsFalse( scope.PublishedBusMessages.OfType<ProcessSendCommunication.Message>().Any() );
    }

    [TestMethod]
    public void SendCommunication_BySender_ApprovesAndQueuesCommunication()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var sender = MockData.CreatePerson( rockContext, "Sender", "Person" );
        var draft = SeedDraftCommunication( rockContext, 400, sender );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, sender ) );

        var result = skill.SendCommunication( IdHasher.Instance.GetHash( draft.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.AreEqual( CommunicationStatus.Approved, draft.Status );
        Assert.AreEqual( sender.PrimaryAliasId, draft.ReviewerPersonAliasId );
        Assert.IsTrue( scope.PublishedBusMessages.OfType<ProcessSendCommunication.Message>().Any( m => m.CommunicationId == draft.Id ) );
    }

    [TestMethod]
    public void SendCommunication_ByOtherPersonWithEdit_ApprovesAndQueuesCommunication()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var sender = MockData.CreatePerson( rockContext, "Sender", "Person" );
        var currentPerson = MockData.CreatePerson( rockContext, "Current", "Person" );
        var draft = SeedDraftCommunication( rockContext, 400, sender );
        MockAuthorizationHelper.AllowAllUsers<Rock.Model.Communication>( rockContext, Authorization.EDIT, draft.Id );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.SendCommunication( IdHasher.Instance.GetHash( draft.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.AreEqual( CommunicationStatus.Approved, draft.Status );
        Assert.IsTrue( scope.PublishedBusMessages.OfType<ProcessSendCommunication.Message>().Any( m => m.CommunicationId == draft.Id ) );
    }

    #endregion
}

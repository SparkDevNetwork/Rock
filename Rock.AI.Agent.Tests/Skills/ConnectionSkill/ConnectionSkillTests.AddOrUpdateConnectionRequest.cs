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

using Rock.AI.Agent.Classes;
using Rock.Configuration;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.Security;
using Rock.Tests.Shared.TestAccess.AI.Agent;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;

namespace Rock.AI.Agent.Tests.Skills.ConnectionSkill;

public partial class ConnectionSkillTests
{
    #region AddOrUpdateConnectionRequest

    [TestMethod]
    public void AddOrUpdateConnectionRequest_WithoutCurrentPerson_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var requester = MockData.CreatePerson( rockContext, "Requester", "Person" );
        var connectionOpportunity = SeedConnectionOpportunity( rockContext, 10 );
        var connectionRequest = SeedConnectionRequest( rockContext, 20, connectionOpportunity, requester );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.EDIT );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateConnectionRequest(
            connectionRequestIdKey: IdHasher.Instance.GetHash( connectionRequest.Id ),
            comments: new SetOrClear<string> { Value = "Updated comments" } );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.AreEqual( "Original comments", connectionRequest.Comments );
    }

    [TestMethod]
    public void AddOrUpdateConnectionRequest_UpdateWithOpportunityEdit_UpdatesRequest()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = MockData.CreatePerson( rockContext, "Current", "Person" );
        var requester = MockData.CreatePerson( rockContext, "Requester", "Person" );
        var connectionOpportunity = SeedConnectionOpportunity( rockContext, 10 );
        var connectionRequest = SeedConnectionRequest( rockContext, 20, connectionOpportunity, requester );
        MockAuthorizationHelper.AllowAllUsers<ConnectionOpportunity>( rockContext, Authorization.EDIT, connectionOpportunity.Id );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.AddOrUpdateConnectionRequest(
            connectionRequestIdKey: IdHasher.Instance.GetHash( connectionRequest.Id ),
            comments: new SetOrClear<string> { Value = "Updated comments" } );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.AreEqual( "Updated comments", connectionRequest.Comments );
    }

    [TestMethod]
    public void AddOrUpdateConnectionRequest_UpdateWithoutAuthorization_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = MockData.CreatePerson( rockContext, "Current", "Person" );
        var requester = MockData.CreatePerson( rockContext, "Requester", "Person" );
        var connectionOpportunity = SeedConnectionOpportunity( rockContext, 10 );
        var connectionRequest = SeedConnectionRequest( rockContext, 20, connectionOpportunity, requester );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.AddOrUpdateConnectionRequest(
            connectionRequestIdKey: IdHasher.Instance.GetHash( connectionRequest.Id ),
            comments: new SetOrClear<string> { Value = "Updated comments" } );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "not authorized" ) ) );
        Assert.AreEqual( "Original comments", connectionRequest.Comments );
    }

    [TestMethod]
    public void AddOrUpdateConnectionRequest_UpdateWithPersonIdKey_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = MockData.CreatePerson( rockContext, "Current", "Person" );
        var requester = MockData.CreatePerson( rockContext, "Requester", "Person" );
        var otherPerson = MockData.CreatePerson( rockContext, "Other", "Person" );
        var connectionOpportunity = SeedConnectionOpportunity( rockContext, 10 );
        var connectionRequest = SeedConnectionRequest( rockContext, 20, connectionOpportunity, requester );
        MockAuthorizationHelper.AllowAllUsers<ConnectionOpportunity>( rockContext, Authorization.EDIT, connectionOpportunity.Id );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.AddOrUpdateConnectionRequest(
            connectionRequestIdKey: IdHasher.Instance.GetHash( connectionRequest.Id ),
            personIdKey: IdHasher.Instance.GetHash( otherPerson.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.AreEqual( requester.PrimaryAliasId, connectionRequest.PersonAliasId );
    }

    [TestMethod]
    public void AddOrUpdateConnectionRequest_AddWithOpportunityEdit_AddsRequest()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = MockData.CreatePerson( rockContext, "Current", "Person" );
        var requester = MockData.CreatePerson( rockContext, "Requester", "Person" );
        var connectionOpportunity = SeedConnectionOpportunity( rockContext, 10 );
        SetupNewConnectionRequests( rockContext );
        MockAuthorizationHelper.AllowAllUsers<ConnectionOpportunity>( rockContext, Authorization.EDIT, connectionOpportunity.Id );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.AddOrUpdateConnectionRequest(
            connectionOpportunityIdKey: IdHasher.Instance.GetHash( connectionOpportunity.Id ),
            personIdKey: IdHasher.Instance.GetHash( requester.Id ),
            connectionStatusIdKey: GetStatusIdKey( connectionOpportunity ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.IsTrue( rockContext.Set<ConnectionRequest>().Any( r => r.ConnectionOpportunityId == connectionOpportunity.Id && r.PersonAliasId == requester.PrimaryAliasId ) );
    }

    [TestMethod]
    public void AddOrUpdateConnectionRequest_AddAsActiveConnectorGroupMember_AddsRequest()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = MockData.CreatePerson( rockContext, "Current", "Person" );
        var requester = MockData.CreatePerson( rockContext, "Requester", "Person" );
        var connectionOpportunity = SeedConnectionOpportunity( rockContext, 10 );
        SetupNewConnectionRequests( rockContext );
        SeedConnectorGroup( rockContext, 30, connectionOpportunity, currentPerson, GroupMemberStatus.Active );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.AddOrUpdateConnectionRequest(
            connectionOpportunityIdKey: IdHasher.Instance.GetHash( connectionOpportunity.Id ),
            personIdKey: IdHasher.Instance.GetHash( requester.Id ),
            connectionStatusIdKey: GetStatusIdKey( connectionOpportunity ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    [TestMethod]
    public void AddOrUpdateConnectionRequest_AddAsInactiveConnectorGroupMember_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = MockData.CreatePerson( rockContext, "Current", "Person" );
        var requester = MockData.CreatePerson( rockContext, "Requester", "Person" );
        var connectionOpportunity = SeedConnectionOpportunity( rockContext, 10 );
        SetupNewConnectionRequests( rockContext );
        SeedConnectorGroup( rockContext, 30, connectionOpportunity, currentPerson, GroupMemberStatus.Inactive );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.AddOrUpdateConnectionRequest(
            connectionOpportunityIdKey: IdHasher.Instance.GetHash( connectionOpportunity.Id ),
            personIdKey: IdHasher.Instance.GetHash( requester.Id ),
            connectionStatusIdKey: GetStatusIdKey( connectionOpportunity ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "not authorized" ) ) );
    }

    [TestMethod]
    public void AddOrUpdateConnectionRequest_AddWithoutAuthorization_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = MockData.CreatePerson( rockContext, "Current", "Person" );
        var requester = MockData.CreatePerson( rockContext, "Requester", "Person" );
        var connectionOpportunity = SeedConnectionOpportunity( rockContext, 10 );
        SetupNewConnectionRequests( rockContext );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.AddOrUpdateConnectionRequest(
            connectionOpportunityIdKey: IdHasher.Instance.GetHash( connectionOpportunity.Id ),
            personIdKey: IdHasher.Instance.GetHash( requester.Id ),
            connectionStatusIdKey: GetStatusIdKey( connectionOpportunity ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "not authorized" ) ) );
        Assert.IsFalse( rockContext.Set<ConnectionRequest>().Any( r => r.PersonAliasId == requester.PrimaryAliasId ) );
    }

    #endregion
}

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
using System.Text.Json;

using Microsoft.SemanticKernel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.ConnectionSkill;
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
    #region ListConnectionRequests

    [TestMethod]
    public void ListConnectionRequests_WithOnlyConnectionStates_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListConnectionRequests( connectionStates: new List<ConnectionState> { ConnectionState.Active } );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    [TestMethod]
    public void ListConnectionRequests_ByDefault_ReturnsActiveAndFutureFollowUpOnly()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var requester = MockData.CreatePerson( rockContext, "Requester", "Person" );
        var connectionOpportunity = SeedConnectionOpportunity( rockContext, 10 );
        var activeRequest = SeedConnectionRequest( rockContext, 20, connectionOpportunity, requester );
        var inactiveRequest = SeedConnectionRequest( rockContext, 21, connectionOpportunity, requester );
        inactiveRequest.ConnectionState = ConnectionState.Inactive;
        var futureFollowUpRequest = SeedConnectionRequest( rockContext, 22, connectionOpportunity, requester );
        futureFollowUpRequest.ConnectionState = ConnectionState.FutureFollowUp;
        var connectedRequest = SeedConnectionRequest( rockContext, 23, connectionOpportunity, requester );
        connectedRequest.ConnectionState = ConnectionState.Connected;
        connectedRequest.ConnectedDateTime = RockDateTime.Now.AddDays( -5 );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.VIEW );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListConnectionRequests( requesterPersonIdKey: IdHasher.Instance.GetHash( requester.Id ) );

        CollectionAssert.AreEquivalent( new[] { activeRequest.Id, futureFollowUpRequest.Id }, GetResultIds( result ) );
    }

    [TestMethod]
    public void ListConnectionRequests_WithInactiveState_ReturnsOnlyInactiveRequests()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var requester = MockData.CreatePerson( rockContext, "Requester", "Person" );
        var connectionOpportunity = SeedConnectionOpportunity( rockContext, 10 );
        SeedConnectionRequest( rockContext, 20, connectionOpportunity, requester );
        var inactiveRequest = SeedConnectionRequest( rockContext, 21, connectionOpportunity, requester );
        inactiveRequest.ConnectionState = ConnectionState.Inactive;
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.VIEW );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListConnectionRequests(
            requesterPersonIdKey: IdHasher.Instance.GetHash( requester.Id ),
            connectionStates: new List<ConnectionState> { ConnectionState.Inactive } );

        CollectionAssert.AreEqual( new[] { inactiveRequest.Id }, GetResultIds( result ) );
    }

    [TestMethod]
    public void ListConnectionRequests_WithConnectedState_ReturnsConnectedRequests()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var requester = MockData.CreatePerson( rockContext, "Requester", "Person" );
        var connectionOpportunity = SeedConnectionOpportunity( rockContext, 10 );
        var activeRequest = SeedConnectionRequest( rockContext, 20, connectionOpportunity, requester );
        var connectedRequest = SeedConnectionRequest( rockContext, 21, connectionOpportunity, requester );
        var connectedDateTime = RockDateTime.Now.AddDays( -5 );
        connectedRequest.ConnectionState = ConnectionState.Connected;
        connectedRequest.ConnectedDateTime = connectedDateTime;
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.VIEW );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListConnectionRequests(
            requesterPersonIdKey: IdHasher.Instance.GetHash( requester.Id ),
            connectionStates: new List<ConnectionState> { ConnectionState.Active, ConnectionState.Connected } );

        CollectionAssert.AreEquivalent( new[] { activeRequest.Id, connectedRequest.Id }, GetResultIds( result ) );

        var connectedRow = GetResultItems( result ).Single( r => r.Id == connectedRequest.Id );

        Assert.AreEqual( connectedDateTime, connectedRow.ConnectedDateTime );
    }

    [TestMethod]
    public void ListConnectionRequests_ConnectedWithoutConnectedDate_IsExcludedByDefault()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        // A request created as Connected, or imported, never gets a
        // ConnectedDateTime. It must still be left out by default.
        var requester = MockData.CreatePerson( rockContext, "Requester", "Person" );
        var connectionOpportunity = SeedConnectionOpportunity( rockContext, 10 );
        var activeRequest = SeedConnectionRequest( rockContext, 20, connectionOpportunity, requester );
        var connectedRequest = SeedConnectionRequest( rockContext, 21, connectionOpportunity, requester );
        connectedRequest.ConnectionState = ConnectionState.Connected;
        connectedRequest.ConnectedDateTime = null;
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.VIEW );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListConnectionRequests( requesterPersonIdKey: IdHasher.Instance.GetHash( requester.Id ) );

        CollectionAssert.AreEqual( new[] { activeRequest.Id }, GetResultIds( result ) );
    }

    [TestMethod]
    public void ListConnectionRequests_ReopenedWithStaleConnectedDate_IsIncludedByDefault()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        // A request moved from Connected back to Active keeps its old
        // ConnectedDateTime. It is open again, so it must be returned.
        var requester = MockData.CreatePerson( rockContext, "Requester", "Person" );
        var connectionOpportunity = SeedConnectionOpportunity( rockContext, 10 );
        var reopenedRequest = SeedConnectionRequest( rockContext, 20, connectionOpportunity, requester );
        reopenedRequest.ConnectionState = ConnectionState.Active;
        reopenedRequest.ConnectedDateTime = RockDateTime.Now.AddDays( -30 );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.VIEW );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListConnectionRequests( requesterPersonIdKey: IdHasher.Instance.GetHash( requester.Id ) );

        CollectionAssert.AreEqual( new[] { reopenedRequest.Id }, GetResultIds( result ) );
    }

    [TestMethod]
    public void ListConnectionRequests_WithCreatedRange_ReturnsOnlyRequestsCreatedInRange()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var now = RockDateTime.Now;
        var requester = MockData.CreatePerson( rockContext, "Requester", "Person" );
        var connectionOpportunity = SeedConnectionOpportunity( rockContext, 10 );
        var oldRequest = SeedConnectionRequest( rockContext, 20, connectionOpportunity, requester );
        oldRequest.CreatedDateTime = now.AddDays( -60 );
        var recentRequest = SeedConnectionRequest( rockContext, 21, connectionOpportunity, requester );
        recentRequest.CreatedDateTime = now.AddDays( -10 );
        var newestRequest = SeedConnectionRequest( rockContext, 22, connectionOpportunity, requester );
        newestRequest.CreatedDateTime = now.AddDays( -1 );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.VIEW );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListConnectionRequests(
            requesterPersonIdKey: IdHasher.Instance.GetHash( requester.Id ),
            createdStartDate: now.AddDays( -30 ),
            createdEndDate: now.AddDays( -5 ) );

        CollectionAssert.AreEqual( new[] { recentRequest.Id }, GetResultIds( result ) );
    }

    [TestMethod]
    public void ListConnectionRequests_WithOnlyCreatedRange_SatisfiesRequiredFilter()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var now = RockDateTime.Now;
        var requester = MockData.CreatePerson( rockContext, "Requester", "Person" );
        var connectionOpportunity = SeedConnectionOpportunity( rockContext, 10 );
        var oldRequest = SeedConnectionRequest( rockContext, 20, connectionOpportunity, requester );
        oldRequest.CreatedDateTime = now.AddDays( -60 );
        var recentRequest = SeedConnectionRequest( rockContext, 21, connectionOpportunity, requester );
        recentRequest.CreatedDateTime = now.AddDays( -2 );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.VIEW );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListConnectionRequests( createdStartDate: now.AddDays( -7 ) );

        CollectionAssert.AreEqual( new[] { recentRequest.Id }, GetResultIds( result ) );
    }

    [TestMethod]
    public void ListConnectionRequests_WithOnlyConnectedRange_ReturnsOnlyConnectedRequestsInRange()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var now = RockDateTime.Now;
        var requester = MockData.CreatePerson( rockContext, "Requester", "Person" );
        var connectionOpportunity = SeedConnectionOpportunity( rockContext, 10 );

        var connectedInRange = SeedConnectionRequest( rockContext, 20, connectionOpportunity, requester );
        connectedInRange.ConnectionState = ConnectionState.Connected;
        connectedInRange.ConnectedDateTime = now.AddDays( -3 );

        var connectedOutOfRange = SeedConnectionRequest( rockContext, 21, connectionOpportunity, requester );
        connectedOutOfRange.ConnectionState = ConnectionState.Connected;
        connectedOutOfRange.ConnectedDateTime = now.AddDays( -60 );

        // Reopened: it has a connected date in range but is no longer Connected.
        var reopenedInRange = SeedConnectionRequest( rockContext, 22, connectionOpportunity, requester );
        reopenedInRange.ConnectionState = ConnectionState.Active;
        reopenedInRange.ConnectedDateTime = now.AddDays( -3 );

        var connectedWithoutDate = SeedConnectionRequest( rockContext, 23, connectionOpportunity, requester );
        connectedWithoutDate.ConnectionState = ConnectionState.Connected;
        connectedWithoutDate.ConnectedDateTime = null;

        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.VIEW );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListConnectionRequests(
            connectedStartDate: now.AddDays( -7 ),
            connectedEndDate: now );

        CollectionAssert.AreEqual( new[] { connectedInRange.Id }, GetResultIds( result ) );
    }

    [TestMethod]
    public void ListConnectionRequests_WithConnectedRangeAndStatesWithoutConnected_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var requester = MockData.CreatePerson( rockContext, "Requester", "Person" );
        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListConnectionRequests(
            requesterPersonIdKey: IdHasher.Instance.GetHash( requester.Id ),
            connectionStates: new List<ConnectionState> { ConnectionState.Active, ConnectionState.Inactive },
            connectedStartDate: RockDateTime.Now.AddDays( -7 ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "Connected" ) ) );
    }

    [TestMethod]
    public void ListConnectionRequests_WithConnectedRangeAndOtherStatesWithConnected_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var requester = MockData.CreatePerson( rockContext, "Requester", "Person" );
        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListConnectionRequests(
            requesterPersonIdKey: IdHasher.Instance.GetHash( requester.Id ),
            connectionStates: new List<ConnectionState> { ConnectionState.Active, ConnectionState.Connected },
            connectedStartDate: RockDateTime.Now.AddDays( -7 ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "Connected only" ) ) );
    }

    [TestMethod]
    public void ListConnectionRequests_WithUnknownState_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var requester = MockData.CreatePerson( rockContext, "Requester", "Person" );
        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListConnectionRequests(
            requesterPersonIdKey: IdHasher.Instance.GetHash( requester.Id ),
            connectionStates: new List<ConnectionState> { ( ConnectionState ) 99 } );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    [TestMethod]
    public void ListConnectionRequests_ToolSchema_ListsConnectionStateNames()
    {
        // Tools are registered through KernelFunctionFactory with the agent
        // serializer options, so build the function the same way and check
        // that the language model sees the state names and can send them.
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );
        var serializerOptions = AgentSerializerOptions.GetOptions( AgentType.Chat, AudienceType.Internal );
        var method = typeof( Rock.AI.Agent.Skills.ConnectionSkill ).GetMethod( nameof( Rock.AI.Agent.Skills.ConnectionSkill.ListConnectionRequests ) );

        var function = KernelFunctionFactory.CreateFromMethod(
            method: method,
            target: skill,
            functionName: "ListConnectionRequests",
            jsonSerializerOptions: serializerOptions );

        var parameter = function.Metadata.Parameters.Single( p => p.Name == "connectionStates" );
        var schema = parameter.Schema.ToString();

        StringAssert.Contains( schema, "\"array\"" );
        StringAssert.Contains( schema, "\"Active\"" );
        StringAssert.Contains( schema, "\"Inactive\"" );
        StringAssert.Contains( schema, "\"FutureFollowUp\"" );
        StringAssert.Contains( schema, "\"Connected\"" );

        var states = JsonSerializer.Deserialize<List<ConnectionState>>( "[\"FutureFollowUp\",\"Connected\"]", serializerOptions );

        CollectionAssert.AreEqual( new[] { ConnectionState.FutureFollowUp, ConnectionState.Connected }, states );
    }

    /// <summary>
    /// Gets the connection request rows from a successful list result.
    /// </summary>
    private static ConnectionRequestResult[] GetResultItems( AgentToolResult result )
    {
        Assert.AreEqual( ToolStatus.Success, result.GetStatus(), string.Join( " ", result.GetErrorMessages() ?? new List<string>() ) );

        var page = ( PaginatedResult<ConnectionRequestResult> ) result.GetContent();

        return page.Items.ToArray();
    }

    /// <summary>
    /// Gets the identifiers of the connection requests in a successful list result.
    /// </summary>
    private static int[] GetResultIds( AgentToolResult result )
    {
        return GetResultItems( result ).Select( r => r.Id ).ToArray();
    }

    #endregion
}

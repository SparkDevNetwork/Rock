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

using Rock.Configuration;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.Security;
using Rock.Tests.Shared.TestAccess.AI.Agent;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;

namespace Rock.AI.Agent.Tests.Skills.PrayerSkill;

public partial class PrayerSkillTests
{
    #region DeletePrayerRequest

    [TestMethod]
    public void DeletePrayerRequest_WithoutEdit_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = MockData.CreatePerson( rockContext, "Current", "Person" );
        var prayerRequest = SeedPrayerRequest( rockContext, 10 );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.DeletePrayerRequest( IdHasher.Instance.GetHash( prayerRequest.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( rockContext.Set<PrayerRequest>().Any( pr => pr.Id == prayerRequest.Id ) );
    }

    [TestMethod]
    public void DeletePrayerRequest_WithEdit_DeletesRequest()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = MockData.CreatePerson( rockContext, "Current", "Person" );
        var prayerRequest = SeedPrayerRequest( rockContext, 10 );
        MockAuthorizationHelper.AllowAllUsers<PrayerRequest>( rockContext, Authorization.EDIT, prayerRequest.Id );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.DeletePrayerRequest( IdHasher.Instance.GetHash( prayerRequest.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus(), string.Join( " ", result.GetErrorMessages() ?? new List<string>() ) );
        Assert.IsFalse( rockContext.Set<PrayerRequest>().Any( pr => pr.Id == prayerRequest.Id ) );
    }

    [TestMethod]
    public void DeletePrayerRequest_RequesterWithoutEdit_DeletesRequest()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = MockData.CreatePerson( rockContext, "Current", "Person" );
        var prayerRequest = SeedPrayerRequest( rockContext, 10 );
        SetRequester( prayerRequest, currentPerson );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.DeletePrayerRequest( IdHasher.Instance.GetHash( prayerRequest.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus(), string.Join( " ", result.GetErrorMessages() ?? new List<string>() ) );
        Assert.IsFalse( rockContext.Set<PrayerRequest>().Any( pr => pr.Id == prayerRequest.Id ) );
    }

    [TestMethod]
    public void DeletePrayerRequest_CreatorWithoutEdit_DeletesRequest()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = MockData.CreatePerson( rockContext, "Current", "Person" );
        var prayerRequest = SeedPrayerRequest( rockContext, 10 );
        SetCreator( prayerRequest, currentPerson );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.DeletePrayerRequest( IdHasher.Instance.GetHash( prayerRequest.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus(), string.Join( " ", result.GetErrorMessages() ?? new List<string>() ) );
        Assert.IsFalse( rockContext.Set<PrayerRequest>().Any( pr => pr.Id == prayerRequest.Id ) );
    }

    [TestMethod]
    public void DeletePrayerRequest_UnrelatedPersonWithoutEdit_ReturnsErrorAndKeepsRequest()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = MockData.CreatePerson( rockContext, "Current", "Person" );
        var owner = MockData.CreatePerson( rockContext, "Owner", "Person" );
        var prayerRequest = SeedPrayerRequest( rockContext, 10 );
        SetRequester( prayerRequest, owner );
        SetCreator( prayerRequest, owner );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.DeletePrayerRequest( IdHasher.Instance.GetHash( prayerRequest.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        CollectionAssert.Contains( result.GetErrorMessages().ToList(), "You are not authorized to delete this prayer request." );
        Assert.IsTrue( rockContext.Set<PrayerRequest>().Any( pr => pr.Id == prayerRequest.Id ) );
    }

    [TestMethod]
    public void DeletePrayerRequest_WithoutView_ReturnsErrorAndKeepsRequest()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = MockData.CreatePerson( rockContext, "Current", "Person" );
        var prayerRequest = SeedPrayerRequest( rockContext, 10 );

        // The person has EDIT on the request, so only the VIEW check on
        // load can stop this.
        MockAuthorizationHelper.AllowAllUsers<PrayerRequest>( rockContext, Authorization.EDIT, prayerRequest.Id );
        MockAuthorizationHelper.DenyAllUsers<PrayerRequest>( rockContext, Authorization.VIEW, prayerRequest.Id );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.DeletePrayerRequest( IdHasher.Instance.GetHash( prayerRequest.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( rockContext.Set<PrayerRequest>().Any( pr => pr.Id == prayerRequest.Id ) );
    }

    [TestMethod]
    public void DeletePrayerRequest_AnonymousWithAllUsersEdit_ReturnsErrorAndKeepsRequest()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var prayerRequest = SeedPrayerRequest( rockContext, 10 );

        // The request has no requester or creator, and all users have
        // EDIT, so only the anonymous check can stop this.
        MockAuthorizationHelper.AllowAllUsers<PrayerRequest>( rockContext, Authorization.EDIT, prayerRequest.Id );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, null, AudienceType.Public ) );

        var result = skill.DeletePrayerRequest( IdHasher.Instance.GetHash( prayerRequest.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        CollectionAssert.Contains( result.GetErrorMessages().ToList(), "You are not authorized to delete this prayer request." );
        Assert.IsTrue( rockContext.Set<PrayerRequest>().Any( pr => pr.Id == prayerRequest.Id ) );
    }

    #endregion
}

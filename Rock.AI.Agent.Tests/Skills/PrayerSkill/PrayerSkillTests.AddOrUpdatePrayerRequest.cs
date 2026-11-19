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

using Moq;

using Rock.AI.Agent.Classes.Entity;
using Rock.Configuration;
using Rock.Data;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.Security;
using Rock.Tests.Shared.TestAccess.AI.Agent;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Tests.Skills.PrayerSkill;

public partial class PrayerSkillTests
{
    #region AddOrUpdatePrayerRequest

    [TestMethod]
    public void AddOrUpdatePrayerRequest_ExistingWithoutEdit_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = MockData.CreatePerson( rockContext, "Current", "Person" );
        var prayerRequest = SeedPrayerRequest( rockContext, 10 );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.AddOrUpdatePrayerRequest( prayerRequestIdKey: IdHasher.Instance.GetHash( prayerRequest.Id ), requestText: "P3C43-edited", isUrgent: true );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.AreEqual( OriginalText, prayerRequest.Text );
        Assert.IsFalse( prayerRequest.IsUrgent ?? false );
    }

    [TestMethod]
    public void AddOrUpdatePrayerRequest_ExistingWithEdit_UpdatesRequest()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = MockData.CreatePerson( rockContext, "Current", "Person" );
        var prayerRequest = SeedPrayerRequest( rockContext, 10 );
        MockAuthorizationHelper.AllowAllUsers<PrayerRequest>( rockContext, Authorization.EDIT, prayerRequest.Id );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.AddOrUpdatePrayerRequest( prayerRequestIdKey: IdHasher.Instance.GetHash( prayerRequest.Id ), requestText: "P3C43-edited" );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus(), string.Join( " ", result.GetErrorMessages() ?? new List<string>() ) );
        Assert.AreEqual( "P3C43-edited", prayerRequest.Text );
        Assert.IsTrue( prayerRequest.IsApproved ?? false );
    }

    [TestMethod]
    public void AddOrUpdatePrayerRequest_PublicAgentWithOtherPersonKey_ReturnsErrorAndSavesNothing()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = CreatePersonWithEmail( rockContext, "Current" );
        var otherPerson = CreatePersonWithEmail( rockContext, "Other" );
        var prayerRequest = SeedPrayerRequest( rockContext, 10 );
        SetupPrayerRequestCreate( rockContext );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson, AudienceType.Public ) );

        var result = skill.AddOrUpdatePrayerRequest( requestText: "TM003-new", categoryIdKey: IdHasher.Instance.GetHash( prayerRequest.CategoryId.Value ), personIdKey: IdHasher.Instance.GetHash( otherPerson.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        CollectionAssert.Contains( result.GetErrorMessages().ToList(), OwnPersonOnlyMessage );
        Assert.IsFalse( rockContext.Set<PrayerRequest>().Any( pr => pr.Id != 0 && pr.Id != prayerRequest.Id ) );
    }

    [TestMethod]
    public void AddOrUpdatePrayerRequest_PublicAgentWithOwnPersonKey_CreatesRequest()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = CreatePersonWithEmail( rockContext, "Current" );
        var prayerRequest = SeedPrayerRequest( rockContext, 10 );
        SetupPrayerRequestCreate( rockContext );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson, AudienceType.Public ) );

        var result = skill.AddOrUpdatePrayerRequest( requestText: "TM003-new", categoryIdKey: IdHasher.Instance.GetHash( prayerRequest.CategoryId.Value ), personIdKey: IdHasher.Instance.GetHash( currentPerson.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus(), string.Join( " ", result.GetErrorMessages() ?? new List<string>() ) );

        var newRequest = rockContext.Set<PrayerRequest>().Single( pr => pr.Id != prayerRequest.Id );
        Assert.AreEqual( currentPerson.Email, newRequest.Email );
        Assert.AreEqual( currentPerson.Id, newRequest.RequestedByPersonAlias.PersonId );
        Assert.IsFalse( newRequest.IsApproved ?? false );
        CollectionAssert.Contains( result.GetInstructions(), "The prayer request has been added and is pending approval." );
    }

    [TestMethod]
    public void AddOrUpdatePrayerRequest_InternalAgentWithViewablePersonKey_CopiesNameAndEmail()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = CreatePersonWithEmail( rockContext, "Current" );
        var otherPerson = CreatePersonWithEmail( rockContext, "Other" );
        var prayerRequest = SeedPrayerRequest( rockContext, 10 );
        SetupPrayerRequestCreate( rockContext );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.AddOrUpdatePrayerRequest( requestText: "TM003-new", categoryIdKey: IdHasher.Instance.GetHash( prayerRequest.CategoryId.Value ), personIdKey: IdHasher.Instance.GetHash( otherPerson.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus(), string.Join( " ", result.GetErrorMessages() ?? new List<string>() ) );

        var newRequest = rockContext.Set<PrayerRequest>().Single( pr => pr.Id != prayerRequest.Id );
        Assert.AreEqual( otherPerson.FirstName, newRequest.FirstName );
        Assert.AreEqual( otherPerson.LastName, newRequest.LastName );
        Assert.AreEqual( otherPerson.Email, newRequest.Email );
    }

    [TestMethod]
    public void AddOrUpdatePrayerRequest_AnonymousPublicAgentWithPersonKey_ReturnsErrorAndSavesNothing()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var otherPerson = CreatePersonWithEmail( rockContext, "Other" );
        var prayerRequest = SeedPrayerRequest( rockContext, 10 );
        SetupPrayerRequestCreate( rockContext );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, null, AudienceType.Public ) );

        var result = skill.AddOrUpdatePrayerRequest( requestText: "TM003-new", categoryIdKey: IdHasher.Instance.GetHash( prayerRequest.CategoryId.Value ), personIdKey: IdHasher.Instance.GetHash( otherPerson.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        CollectionAssert.Contains( result.GetErrorMessages().ToList(), OwnPersonOnlyMessage );
        Assert.IsFalse( rockContext.Set<PrayerRequest>().Any( pr => pr.Id != 0 && pr.Id != prayerRequest.Id ) );
    }

    [TestMethod]
    public void AddOrUpdatePrayerRequest_InternalAgentWithNotViewablePersonKey_ReturnsErrorAndSavesNothing()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = CreatePersonWithEmail( rockContext, "Current" );
        var otherPerson = CreatePersonWithEmail( rockContext, "Other" );
        var prayerRequest = SeedPrayerRequest( rockContext, 10 );
        SetupPrayerRequestCreate( rockContext );
        MockAuthorizationHelper.DenyAllUsers<Rock.Model.Person>( rockContext, Authorization.VIEW, otherPerson.Id );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.AddOrUpdatePrayerRequest( requestText: "TM003-new", categoryIdKey: IdHasher.Instance.GetHash( prayerRequest.CategoryId.Value ), personIdKey: IdHasher.Instance.GetHash( otherPerson.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        CollectionAssert.Contains( result.GetErrorMessages().ToList(), "The personIdKey is not valid." );
        Assert.IsFalse( rockContext.Set<PrayerRequest>().Any( pr => pr.Id != 0 && pr.Id != prayerRequest.Id ) );
    }

    [TestMethod]
    public void AddOrUpdatePrayerRequest_PublicAgentWithNameOnly_CreatesPendingRequestWithoutEmail()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = CreatePersonWithEmail( rockContext, "Current" );
        var prayerRequest = SeedPrayerRequest( rockContext, 10 );
        SetupPrayerRequestCreate( rockContext );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson, AudienceType.Public ) );

        var result = skill.AddOrUpdatePrayerRequest( requestText: "TM003-new", categoryIdKey: IdHasher.Instance.GetHash( prayerRequest.CategoryId.Value ), firstName: "Ted", lastName: "Decker" );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus(), string.Join( " ", result.GetErrorMessages() ?? new List<string>() ) );

        var newRequest = rockContext.Set<PrayerRequest>().Single( pr => pr.Id != prayerRequest.Id );
        Assert.AreEqual( "Ted", newRequest.FirstName );
        Assert.AreEqual( "Decker", newRequest.LastName );
        Assert.IsNull( newRequest.Email );
        Assert.IsNull( newRequest.RequestedByPersonAliasId );
        Assert.IsFalse( newRequest.IsApproved ?? false );
    }

    [TestMethod]
    public void AddOrUpdatePrayerRequest_PublicAgentEditorResendsExistingRequesterKey_UpdatesRequest()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = CreatePersonWithEmail( rockContext, "Current" );
        var requester = CreatePersonWithEmail( rockContext, "Requester" );
        var prayerRequest = SeedPrayerRequest( rockContext, 10 );
        prayerRequest.RequestedByPersonAlias = requester.Aliases.First();
        prayerRequest.RequestedByPersonAliasId = requester.PrimaryAliasId;
        MockAuthorizationHelper.AllowAllUsers<PrayerRequest>( rockContext, Authorization.EDIT, prayerRequest.Id );
        MockAuthorizationHelper.DenyAllUsers<Rock.Model.Person>( rockContext, Authorization.VIEW, requester.Id );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson, AudienceType.Public ) );

        var result = skill.AddOrUpdatePrayerRequest( prayerRequestIdKey: IdHasher.Instance.GetHash( prayerRequest.Id ), requestText: "TM003-edited", personIdKey: IdHasher.Instance.GetHash( requester.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus(), string.Join( " ", result.GetErrorMessages() ?? new List<string>() ) );
        Assert.AreEqual( "TM003-edited", prayerRequest.Text );
        Assert.AreEqual( requester.Id, prayerRequest.RequestedByPersonAlias.PersonId );
    }

    [TestMethod]
    public void AddOrUpdatePrayerRequest_RequesterWithoutEdit_UpdatesRequest()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = MockData.CreatePerson( rockContext, "Current", "Person" );
        var prayerRequest = SeedPrayerRequest( rockContext, 10 );
        SetRequester( prayerRequest, currentPerson );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.AddOrUpdatePrayerRequest( prayerRequestIdKey: IdHasher.Instance.GetHash( prayerRequest.Id ), requestText: "TM003-edited" );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus(), string.Join( " ", result.GetErrorMessages() ?? new List<string>() ) );
        Assert.AreEqual( "TM003-edited", prayerRequest.Text );
        Assert.IsTrue( prayerRequest.IsApproved ?? false );
    }

    [TestMethod]
    public void AddOrUpdatePrayerRequest_CreatorWithoutEdit_UpdatesRequest()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = MockData.CreatePerson( rockContext, "Current", "Person" );
        var prayerRequest = SeedPrayerRequest( rockContext, 10 );
        SetCreator( prayerRequest, currentPerson );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.AddOrUpdatePrayerRequest( prayerRequestIdKey: IdHasher.Instance.GetHash( prayerRequest.Id ), requestText: "TM003-edited" );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus(), string.Join( " ", result.GetErrorMessages() ?? new List<string>() ) );
        Assert.AreEqual( "TM003-edited", prayerRequest.Text );
    }

    [TestMethod]
    public void AddOrUpdatePrayerRequest_UnrelatedPersonWithoutEdit_ReturnsErrorAndLeavesRequestUnchanged()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = MockData.CreatePerson( rockContext, "Current", "Person" );
        var owner = MockData.CreatePerson( rockContext, "Owner", "Person" );
        var prayerRequest = SeedPrayerRequest( rockContext, 10 );
        SetRequester( prayerRequest, owner );
        SetCreator( prayerRequest, owner );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson, AudienceType.Public ) );

        var result = skill.AddOrUpdatePrayerRequest( prayerRequestIdKey: IdHasher.Instance.GetHash( prayerRequest.Id ), requestText: "TM003-edited" );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        CollectionAssert.Contains( result.GetErrorMessages().ToList(), "You are not authorized to edit this prayer request." );
        Assert.AreEqual( OriginalText, prayerRequest.Text );
        Assert.IsTrue( prayerRequest.IsApproved ?? false );
    }

    [TestMethod]
    public void AddOrUpdatePrayerRequest_PublicAgentAllowedEdit_ResetsApproval()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = MockData.CreatePerson( rockContext, "Current", "Person" );
        var approver = MockData.CreatePerson( rockContext, "Approver", "Person" );
        var prayerRequest = SeedPrayerRequest( rockContext, 10 );
        SetRequester( prayerRequest, currentPerson );
        prayerRequest.ApprovedByPersonAliasId = approver.PrimaryAliasId;
        prayerRequest.ApprovedOnDateTime = RockDateTime.Now;

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson, AudienceType.Public ) );

        var result = skill.AddOrUpdatePrayerRequest( prayerRequestIdKey: IdHasher.Instance.GetHash( prayerRequest.Id ), requestText: "TM003-edited" );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus(), string.Join( " ", result.GetErrorMessages() ?? new List<string>() ) );
        Assert.AreEqual( "TM003-edited", prayerRequest.Text );
        Assert.IsFalse( prayerRequest.IsApproved ?? false );
        Assert.IsNull( prayerRequest.ApprovedByPersonAliasId );
        Assert.IsNull( prayerRequest.ApprovedOnDateTime );
        CollectionAssert.Contains( result.GetInstructions(), "The prayer request has been updated and is pending approval." );
    }

    [TestMethod]
    public void AddOrUpdatePrayerRequest_InternalAgentAllowedEdit_KeepsApproval()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = MockData.CreatePerson( rockContext, "Current", "Person" );
        var approver = MockData.CreatePerson( rockContext, "Approver", "Person" );
        var prayerRequest = SeedPrayerRequest( rockContext, 10 );
        var approvedOnDateTime = RockDateTime.Now;
        SetRequester( prayerRequest, currentPerson );
        prayerRequest.ApprovedByPersonAliasId = approver.PrimaryAliasId;
        prayerRequest.ApprovedOnDateTime = approvedOnDateTime;

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.AddOrUpdatePrayerRequest( prayerRequestIdKey: IdHasher.Instance.GetHash( prayerRequest.Id ), requestText: "TM003-edited" );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus(), string.Join( " ", result.GetErrorMessages() ?? new List<string>() ) );
        Assert.IsTrue( prayerRequest.IsApproved ?? false );
        Assert.AreEqual( approver.PrimaryAliasId, prayerRequest.ApprovedByPersonAliasId );
        Assert.AreEqual( approvedOnDateTime, prayerRequest.ApprovedOnDateTime );
        CollectionAssert.Contains( result.GetInstructions(), "The prayer request has been updated and approved." );
    }

    [TestMethod]
    public void AddOrUpdatePrayerRequest_PublicAgentRequesterWithOtherPersonKey_ReturnsErrorAndSavesNothing()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();
        var rockContextMock = Mock.Get( rockContext );

        var currentPerson = CreatePersonWithEmail( rockContext, "Current" );
        var approver = CreatePersonWithEmail( rockContext, "Approver" );
        var otherPerson = CreatePersonWithEmail( rockContext, "Other" );
        var prayerRequest = SeedPrayerRequest( rockContext, 10 );
        SetRequester( prayerRequest, currentPerson );
        prayerRequest.ApprovedByPersonAliasId = approver.PrimaryAliasId;
        prayerRequest.ApprovedOnDateTime = RockDateTime.Now;

        // Register the entity types the person security check needs now,
        // so any save made while registering them happens before the call.
        EntityTypeCache.Get<Rock.Model.Person>( true, rockContext );
        EntityTypeCache.Get<GlobalDefault>( true, rockContext );
        rockContextMock.Invocations.Clear();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson, AudienceType.Public ) );

        var result = skill.AddOrUpdatePrayerRequest( prayerRequestIdKey: IdHasher.Instance.GetHash( prayerRequest.Id ), requestText: "TM003-edited", personIdKey: IdHasher.Instance.GetHash( otherPerson.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        CollectionAssert.Contains( result.GetErrorMessages().ToList(), OwnPersonOnlyMessage );
        Assert.AreEqual( OriginalText, prayerRequest.Text );

        // The mock database shares the tracked entity, so the approval
        // reset made before the error is visible in memory. The stored
        // request only changes if the context is saved, so check that no
        // save happened.
        rockContextMock.Verify( m => m.SaveChanges(), Times.Never() );
        rockContextMock.Verify( m => m.SaveChanges( It.IsAny<bool>() ), Times.Never() );
        rockContextMock.Verify( m => m.SaveChanges( It.IsAny<SaveChangesArgs>() ), Times.Never() );
    }

    [TestMethod]
    public void AddOrUpdatePrayerRequest_AnonymousWithAllUsersEdit_ReturnsErrorAndLeavesRequestUnchanged()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var prayerRequest = SeedPrayerRequest( rockContext, 10 );

        // The request has no requester or creator, and all users have
        // EDIT, so only the anonymous check can stop this.
        MockAuthorizationHelper.AllowAllUsers<PrayerRequest>( rockContext, Authorization.EDIT, prayerRequest.Id );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, null, AudienceType.Public ) );

        var result = skill.AddOrUpdatePrayerRequest( prayerRequestIdKey: IdHasher.Instance.GetHash( prayerRequest.Id ), requestText: "TM003-edited" );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        CollectionAssert.Contains( result.GetErrorMessages().ToList(), "You are not authorized to edit this prayer request." );
        Assert.AreEqual( OriginalText, prayerRequest.Text );
        Assert.IsTrue( prayerRequest.IsApproved ?? false );
    }

    [TestMethod]
    public void AddOrUpdatePrayerRequest_InternalAgentCreate_SetsApprovalAndDefaultExpiration()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = CreatePersonWithEmail( rockContext, "Current" );
        var prayerRequest = SeedPrayerRequest( rockContext, 10 );
        SetupPrayerRequestCreate( rockContext );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.AddOrUpdatePrayerRequest( requestText: "TM003-new", categoryIdKey: IdHasher.Instance.GetHash( prayerRequest.CategoryId.Value ), firstName: "Ted", lastName: "Decker" );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus(), string.Join( " ", result.GetErrorMessages() ?? new List<string>() ) );

        var newRequest = rockContext.Set<PrayerRequest>().Single( pr => pr.Id != prayerRequest.Id );
        Assert.IsTrue( newRequest.IsApproved ?? false );
        Assert.AreEqual( currentPerson.PrimaryAliasId, newRequest.ApprovedByPersonAliasId );
        Assert.IsNotNull( newRequest.ApprovedOnDateTime );
        Assert.AreEqual( RockDateTime.Today.AddDays( 14 ), newRequest.ExpirationDate );
        Assert.AreEqual( newRequest.ExpirationDate, ( result.GetContent() as PrayerRequestResult )?.ExpirationDate );
        CollectionAssert.Contains( result.GetInstructions(), "The prayer request has been added and approved." );
    }

    [TestMethod]
    public void AddOrUpdatePrayerRequest_InternalAgentCreateWithConfiguredExpireDays_UsesConfiguredDays()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = CreatePersonWithEmail( rockContext, "Current" );
        var prayerRequest = SeedPrayerRequest( rockContext, 10 );
        SetupPrayerRequestCreate( rockContext );

        var configuration = new Dictionary<string, string> { [ExpireDaysKey] = "30" };
        var skill = CreateSkill( scope.App, configuration, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.AddOrUpdatePrayerRequest( requestText: "TM003-new", categoryIdKey: IdHasher.Instance.GetHash( prayerRequest.CategoryId.Value ), firstName: "Ted", lastName: "Decker" );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus(), string.Join( " ", result.GetErrorMessages() ?? new List<string>() ) );

        var newRequest = rockContext.Set<PrayerRequest>().Single( pr => pr.Id != prayerRequest.Id );
        Assert.AreEqual( RockDateTime.Today.AddDays( 30 ), newRequest.ExpirationDate );
    }

    [TestMethod]
    [DataRow( "" )]
    [DataRow( "0" )]
    [DataRow( "-5" )]
    [DataRow( "abc" )]
    public void AddOrUpdatePrayerRequest_InternalAgentCreateWithInvalidExpireDays_UsesDefaultDays( string expireDays )
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = CreatePersonWithEmail( rockContext, "Current" );
        var prayerRequest = SeedPrayerRequest( rockContext, 10 );
        SetupPrayerRequestCreate( rockContext );

        var configuration = new Dictionary<string, string> { [ExpireDaysKey] = expireDays };
        var skill = CreateSkill( scope.App, configuration, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.AddOrUpdatePrayerRequest( requestText: "TM003-new", categoryIdKey: IdHasher.Instance.GetHash( prayerRequest.CategoryId.Value ), firstName: "Ted", lastName: "Decker" );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus(), string.Join( " ", result.GetErrorMessages() ?? new List<string>() ) );

        var newRequest = rockContext.Set<PrayerRequest>().Single( pr => pr.Id != prayerRequest.Id );
        Assert.AreEqual( RockDateTime.Today.AddDays( 14 ), newRequest.ExpirationDate );
    }

    [TestMethod]
    public void AddOrUpdatePrayerRequest_InternalAgentCreateWithExpirationDate_UsesDateOnly()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = CreatePersonWithEmail( rockContext, "Current" );
        var prayerRequest = SeedPrayerRequest( rockContext, 10 );
        SetupPrayerRequestCreate( rockContext );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.AddOrUpdatePrayerRequest( requestText: "TM003-new", categoryIdKey: IdHasher.Instance.GetHash( prayerRequest.CategoryId.Value ), firstName: "Ted", lastName: "Decker", expirationDate: RockDateTime.Today.AddDays( 45 ).AddHours( 13 ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus(), string.Join( " ", result.GetErrorMessages() ?? new List<string>() ) );

        var newRequest = rockContext.Set<PrayerRequest>().Single( pr => pr.Id != prayerRequest.Id );
        Assert.AreEqual( RockDateTime.Today.AddDays( 45 ), newRequest.ExpirationDate );
    }

    [TestMethod]
    public void AddOrUpdatePrayerRequest_InternalAgentCreateWithTodayExpirationDate_UsesToday()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = CreatePersonWithEmail( rockContext, "Current" );
        var prayerRequest = SeedPrayerRequest( rockContext, 10 );
        SetupPrayerRequestCreate( rockContext );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.AddOrUpdatePrayerRequest( requestText: "TM003-new", categoryIdKey: IdHasher.Instance.GetHash( prayerRequest.CategoryId.Value ), firstName: "Ted", lastName: "Decker", expirationDate: RockDateTime.Today );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus(), string.Join( " ", result.GetErrorMessages() ?? new List<string>() ) );

        var newRequest = rockContext.Set<PrayerRequest>().Single( pr => pr.Id != prayerRequest.Id );
        Assert.AreEqual( RockDateTime.Today, newRequest.ExpirationDate );
    }

    [TestMethod]
    public void AddOrUpdatePrayerRequest_InternalAgentCreateWithPastExpirationDate_ReturnsErrorAndSavesNothing()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = CreatePersonWithEmail( rockContext, "Current" );
        var prayerRequest = SeedPrayerRequest( rockContext, 10 );
        SetupPrayerRequestCreate( rockContext );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.AddOrUpdatePrayerRequest( requestText: "TM003-new", categoryIdKey: IdHasher.Instance.GetHash( prayerRequest.CategoryId.Value ), firstName: "Ted", lastName: "Decker", expirationDate: RockDateTime.Today.AddDays( -1 ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        CollectionAssert.Contains( result.GetErrorMessages().ToList(), "The expirationDate must be today or later." );
        Assert.IsFalse( rockContext.Set<PrayerRequest>().Any( pr => pr.Id != 0 && pr.Id != prayerRequest.Id ) );
    }

    [TestMethod]
    public void AddOrUpdatePrayerRequest_PublicAgentCreateWithExpirationDate_StaysPendingWithoutExpiration()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = CreatePersonWithEmail( rockContext, "Current" );
        var prayerRequest = SeedPrayerRequest( rockContext, 10 );
        SetupPrayerRequestCreate( rockContext );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson, AudienceType.Public ) );

        var result = skill.AddOrUpdatePrayerRequest( requestText: "TM003-new", categoryIdKey: IdHasher.Instance.GetHash( prayerRequest.CategoryId.Value ), personIdKey: IdHasher.Instance.GetHash( currentPerson.Id ), expirationDate: RockDateTime.Today.AddDays( 30 ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus(), string.Join( " ", result.GetErrorMessages() ?? new List<string>() ) );

        var newRequest = rockContext.Set<PrayerRequest>().Single( pr => pr.Id != prayerRequest.Id );
        Assert.IsFalse( newRequest.IsApproved ?? false );
        Assert.IsNull( newRequest.ApprovedByPersonAliasId );
        Assert.IsNull( newRequest.ApprovedOnDateTime );
        Assert.IsNull( newRequest.ExpirationDate );
        CollectionAssert.Contains( result.GetInstructions(), "The prayer request has been added and is pending approval." );
    }

    [TestMethod]
    public void AddOrUpdatePrayerRequest_CreateWithCampusIdKey_SetsCampus()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = CreatePersonWithEmail( rockContext, "Current" );
        var prayerRequest = SeedPrayerRequest( rockContext, 10 );
        var campus = SeedCampus( rockContext, 21, "Campus TM003-21" );
        SetupPrayerRequestCreate( rockContext );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.AddOrUpdatePrayerRequest( requestText: "TM003-new", categoryIdKey: IdHasher.Instance.GetHash( prayerRequest.CategoryId.Value ), firstName: "Ted", lastName: "Decker", campusIdKey: IdHasher.Instance.GetHash( campus.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus(), string.Join( " ", result.GetErrorMessages() ?? new List<string>() ) );

        var newRequest = rockContext.Set<PrayerRequest>().Single( pr => pr.Id != prayerRequest.Id );
        Assert.AreEqual( campus.Id, newRequest.CampusId );

        var content = result.GetContent() as PrayerRequestResult;
        Assert.AreEqual( campus.Id, content?.Campus?.Id );
        Assert.AreEqual( campus.Name, content?.Campus?.Name );
    }

    [TestMethod]
    public void AddOrUpdatePrayerRequest_CreateLinkedToPersonWithoutCampusIdKey_UsesRequesterCampus()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = CreatePersonWithEmail( rockContext, "Current" );
        var requester = CreatePersonWithEmail( rockContext, "Requester" );
        var prayerRequest = SeedPrayerRequest( rockContext, 10 );
        var campus = SeedCampus( rockContext, 22, "Campus TM003-22" );
        SetPrimaryCampus( requester, campus );
        SetupPrayerRequestCreate( rockContext );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.AddOrUpdatePrayerRequest( requestText: "TM003-new", categoryIdKey: IdHasher.Instance.GetHash( prayerRequest.CategoryId.Value ), personIdKey: IdHasher.Instance.GetHash( requester.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus(), string.Join( " ", result.GetErrorMessages() ?? new List<string>() ) );

        var newRequest = rockContext.Set<PrayerRequest>().Single( pr => pr.Id != prayerRequest.Id );
        Assert.AreEqual( campus.Id, newRequest.CampusId );
    }

    [TestMethod]
    public void AddOrUpdatePrayerRequest_CreateLinkedToPersonWithCampusIdKey_UsesGivenCampus()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = CreatePersonWithEmail( rockContext, "Current" );
        var requester = CreatePersonWithEmail( rockContext, "Requester" );
        var prayerRequest = SeedPrayerRequest( rockContext, 10 );
        var requesterCampus = SeedCampus( rockContext, 23, "Campus TM003-23" );
        var givenCampus = SeedCampus( rockContext, 24, "Campus TM003-24" );
        SetPrimaryCampus( requester, requesterCampus );
        SetupPrayerRequestCreate( rockContext );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.AddOrUpdatePrayerRequest( requestText: "TM003-new", categoryIdKey: IdHasher.Instance.GetHash( prayerRequest.CategoryId.Value ), personIdKey: IdHasher.Instance.GetHash( requester.Id ), campusIdKey: IdHasher.Instance.GetHash( givenCampus.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus(), string.Join( " ", result.GetErrorMessages() ?? new List<string>() ) );

        var newRequest = rockContext.Set<PrayerRequest>().Single( pr => pr.Id != prayerRequest.Id );
        Assert.AreEqual( givenCampus.Id, newRequest.CampusId );
    }

    [TestMethod]
    public void AddOrUpdatePrayerRequest_InternalAgentUpdateApprovedRequestWithNoExpiration_SetsDefaultExpiration()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = CreatePersonWithEmail( rockContext, "Current" );
        var prayerRequest = SeedPrayerRequest( rockContext, 10 );

        // An approved request the tool added before it set an expiration date.
        SetRequester( prayerRequest, currentPerson );
        prayerRequest.IsApproved = true;
        prayerRequest.ExpirationDate = null;

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.AddOrUpdatePrayerRequest( prayerRequestIdKey: IdHasher.Instance.GetHash( prayerRequest.Id ), requestText: "TM003-edited" );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus(), string.Join( " ", result.GetErrorMessages() ?? new List<string>() ) );
        Assert.AreEqual( RockDateTime.Today.AddDays( 14 ), prayerRequest.ExpirationDate );
    }

    [TestMethod]
    public void AddOrUpdatePrayerRequest_UpdateWithoutCampusOrExpiration_LeavesThemUnchanged()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = CreatePersonWithEmail( rockContext, "Current" );
        var prayerRequest = SeedPrayerRequest( rockContext, 10 );
        var requestCampus = SeedCampus( rockContext, 26, "Campus TM003-26" );
        var requesterCampus = SeedCampus( rockContext, 27, "Campus TM003-27" );
        var expirationDate = RockDateTime.Today.AddDays( 3 );

        // The requester's campus differs from the request's campus, so the
        // test also shows the requester campus default is not used on update.
        SetPrimaryCampus( currentPerson, requesterCampus );
        SetRequester( prayerRequest, currentPerson );
        prayerRequest.CampusId = requestCampus.Id;
        prayerRequest.Campus = requestCampus;
        prayerRequest.ExpirationDate = expirationDate;

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.AddOrUpdatePrayerRequest( prayerRequestIdKey: IdHasher.Instance.GetHash( prayerRequest.Id ), requestText: "TM003-edited" );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus(), string.Join( " ", result.GetErrorMessages() ?? new List<string>() ) );
        Assert.AreEqual( "TM003-edited", prayerRequest.Text );
        Assert.AreEqual( requestCampus.Id, prayerRequest.CampusId );
        Assert.AreEqual( expirationDate, prayerRequest.ExpirationDate );
    }

    [TestMethod]
    public void AddOrUpdatePrayerRequest_InternalAgentUpdateWithCampusAndExpiration_UpdatesThem()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = CreatePersonWithEmail( rockContext, "Current" );
        var prayerRequest = SeedPrayerRequest( rockContext, 10 );
        var oldCampus = SeedCampus( rockContext, 28, "Campus TM003-28" );
        var newCampus = SeedCampus( rockContext, 29, "Campus TM003-29" );

        SetRequester( prayerRequest, currentPerson );
        prayerRequest.CampusId = oldCampus.Id;
        prayerRequest.Campus = oldCampus;
        prayerRequest.ExpirationDate = RockDateTime.Today.AddDays( 3 );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.AddOrUpdatePrayerRequest( prayerRequestIdKey: IdHasher.Instance.GetHash( prayerRequest.Id ), campusIdKey: IdHasher.Instance.GetHash( newCampus.Id ), expirationDate: RockDateTime.Today.AddDays( 60 ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus(), string.Join( " ", result.GetErrorMessages() ?? new List<string>() ) );
        Assert.AreEqual( newCampus.Id, prayerRequest.CampusId );
        Assert.AreEqual( RockDateTime.Today.AddDays( 60 ), prayerRequest.ExpirationDate );
    }

    private const string OwnPersonOnlyMessage = "Prayer requests can only be linked to your own person record. Use firstName and lastName for anyone else.";

    private const string ExpireDaysKey = "expireDays";

    private static Rock.AI.Agent.Skills.PrayerSkill CreateSkill( System.IServiceProvider serviceProvider, IReadOnlyDictionary<string, string> configurationValues, AgentRequestContext agentRequestContext )
    {
        return AgentSkillTestFactory.CreateSkill<Rock.AI.Agent.Skills.PrayerSkill>( serviceProvider, configurationValues, agentRequestContext );
    }

    /// <summary>
    /// Seeds a campus with the specified identifier and name.
    /// </summary>
    private static Campus SeedCampus( RockContext rockContext, int id, string name )
    {
        var campus = new Campus
        {
            Id = id,
            Guid = System.Guid.NewGuid(),
            Name = name,
            IsActive = true
        };

        rockContext.Set<Campus>().Add( campus );

        return campus;
    }

    /// <summary>
    /// Sets the primary campus of the person.
    /// </summary>
    private static void SetPrimaryCampus( Rock.Model.Person person, Campus campus )
    {
        person.PrimaryCampusId = campus.Id;
        person.PrimaryCampus = campus;
    }

    private static Rock.Model.Person CreatePersonWithEmail( RockContext rockContext, string firstName )
    {
        var person = MockData.CreatePerson( rockContext, firstName, "Person" );

        person.Email = $"{firstName.ToLower()}@example.invalid";

        return person;
    }

    /// <summary>
    /// Lets the tool create a new prayer request in the mocked context, and
    /// registers the entity types the person security check needs now.
    /// Otherwise that check registers them later, and that save would give
    /// the new, unsaved prayer request an Id in the mock database. Also seeds
    /// the family group type, which the requester campus default needs to
    /// look up the person's family when they have no primary campus.
    /// </summary>
    private static void SetupPrayerRequestCreate( RockContext rockContext )
    {
        Mock.Get( rockContext.Set<PrayerRequest>() )
            .Setup( m => m.Create() )
            .Returns( () => new PrayerRequest() );

        rockContext.Set<GroupType>().Add( new GroupType
        {
            Id = 90,
            Guid = SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid(),
            Name = "Family"
        } );

        EntityTypeCache.Get<Rock.Model.Person>( true, rockContext );
        EntityTypeCache.Get<GlobalDefault>( true, rockContext );
    }

    #endregion
}

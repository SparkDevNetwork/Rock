using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Skills;
using Rock.Data;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.Security;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Tests.Skills.Prayer;

using Person = Rock.Model.Person;

[TestClass]
public class PrayerSkillTests : MockDatabaseTestsBase
{
    private const int PrayerRequestId = 1;
    private const string OriginalText = "P3C43-original";
    private const string OwnPersonOnlyMessage = "Prayer requests can only be linked to your own person record. Use firstName and lastName for anyone else.";

    [TestMethod]
    public void AddOrUpdatePrayerRequest_ExistingWithoutEdit_ReturnsError()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            var person = CreatePerson( rockContext, 1 );
            var prayerRequest = CreatePrayerRequest( rockContext );

            var skill = CreateSkill( rockContext, person );

            var result = skill.AddOrUpdatePrayerRequest( prayerRequestIdKey: prayerRequest.IdKey, requestText: "P3C43-edited", isUrgent: true );

            Assert.AreEqual( ToolStatus.Error, result.Status );
            Assert.AreEqual( OriginalText, prayerRequest.Text );
            Assert.IsFalse( prayerRequest.IsUrgent ?? false );
        }
    }

    [TestMethod]
    public void AddOrUpdatePrayerRequest_ExistingWithEdit_UpdatesRequest()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            var person = CreatePerson( rockContext, 1 );
            var prayerRequest = CreatePrayerRequest( rockContext );
            AllowAllUsersEdit( rockContext );

            var skill = CreateSkill( rockContext, person );

            var result = skill.AddOrUpdatePrayerRequest( prayerRequestIdKey: prayerRequest.IdKey, requestText: "P3C43-edited" );

            Assert.AreEqual( ToolStatus.Success, result.Status, string.Join( " ", result.ErrorMessages ?? new List<string>() ) );
            Assert.AreEqual( "P3C43-edited", prayerRequest.Text );
            Assert.IsTrue( prayerRequest.IsApproved ?? false );
        }
    }

    [TestMethod]
    public void DeletePrayerRequest_WithoutEdit_ReturnsError()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            var person = CreatePerson( rockContext, 1 );
            var prayerRequest = CreatePrayerRequest( rockContext );

            var skill = CreateSkill( rockContext, person );

            var result = skill.DeletePrayerRequest( prayerRequest.IdKey );

            Assert.AreEqual( ToolStatus.Error, result.Status );
            Assert.IsTrue( rockContext.Set<PrayerRequest>().Any( pr => pr.Id == PrayerRequestId ) );
        }
    }

    [TestMethod]
    public void DeletePrayerRequest_WithEdit_DeletesRequest()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            var person = CreatePerson( rockContext, 1 );
            var prayerRequest = CreatePrayerRequest( rockContext );
            AllowAllUsersEdit( rockContext );

            var skill = CreateSkill( rockContext, person );

            var result = skill.DeletePrayerRequest( prayerRequest.IdKey );

            Assert.AreEqual( ToolStatus.Success, result.Status, string.Join( " ", result.ErrorMessages ?? new List<string>() ) );
            Assert.IsFalse( rockContext.Set<PrayerRequest>().Any( pr => pr.Id == PrayerRequestId ) );
        }
    }

    [TestMethod]
    public void AddOrUpdatePrayerRequest_PublicAgentWithOtherPersonKey_ReturnsErrorAndSavesNothing()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            var person = CreatePerson( rockContext, 1 );
            var otherPerson = CreatePerson( rockContext, 2 );
            var prayerRequest = CreatePrayerRequest( rockContext );
            SetupPrayerRequestCreate( rockContext );

            var skill = CreateSkill( rockContext, person, AudienceType.Public );

            var result = skill.AddOrUpdatePrayerRequest( requestText: "TM003-new", categoryIdKey: prayerRequest.Category.IdKey, personIdKey: otherPerson.IdKey );

            Assert.AreEqual( ToolStatus.Error, result.Status );
            CollectionAssert.Contains( result.ErrorMessages.ToList(), OwnPersonOnlyMessage );
            Assert.IsFalse( rockContext.Set<PrayerRequest>().Any( pr => pr.Id != 0 && pr.Id != PrayerRequestId ) );
        }
    }

    [TestMethod]
    public void AddOrUpdatePrayerRequest_PublicAgentWithOwnPersonKey_CreatesRequest()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            var person = CreatePerson( rockContext, 1 );
            var prayerRequest = CreatePrayerRequest( rockContext );
            SetupPrayerRequestCreate( rockContext );

            var skill = CreateSkill( rockContext, person, AudienceType.Public );

            var result = skill.AddOrUpdatePrayerRequest( requestText: "TM003-new", categoryIdKey: prayerRequest.Category.IdKey, personIdKey: person.IdKey );

            Assert.AreEqual( ToolStatus.Success, result.Status, string.Join( " ", result.ErrorMessages ?? new List<string>() ) );

            var newRequest = rockContext.Set<PrayerRequest>().Single( pr => pr.Id != PrayerRequestId );
            Assert.AreEqual( person.Email, newRequest.Email );
            Assert.AreEqual( person.Id, newRequest.RequestedByPersonAlias.PersonId );
            Assert.IsFalse( newRequest.IsApproved ?? false );
            CollectionAssert.Contains( result.Instructions, "The prayer request has been added and is pending approval." );
        }
    }

    [TestMethod]
    public void AddOrUpdatePrayerRequest_InternalAgentWithViewablePersonKey_CopiesNameAndEmail()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            var person = CreatePerson( rockContext, 1 );
            var otherPerson = CreatePerson( rockContext, 2 );
            var prayerRequest = CreatePrayerRequest( rockContext );
            SetupPrayerRequestCreate( rockContext );

            var skill = CreateSkill( rockContext, person );

            var result = skill.AddOrUpdatePrayerRequest( requestText: "TM003-new", categoryIdKey: prayerRequest.Category.IdKey, personIdKey: otherPerson.IdKey );

            Assert.AreEqual( ToolStatus.Success, result.Status, string.Join( " ", result.ErrorMessages ?? new List<string>() ) );

            var newRequest = rockContext.Set<PrayerRequest>().Single( pr => pr.Id != PrayerRequestId );
            Assert.AreEqual( otherPerson.FirstName, newRequest.FirstName );
            Assert.AreEqual( otherPerson.LastName, newRequest.LastName );
            Assert.AreEqual( otherPerson.Email, newRequest.Email );
        }
    }

    [TestMethod]
    public void AddOrUpdatePrayerRequest_AnonymousPublicAgentWithPersonKey_ReturnsErrorAndSavesNothing()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            var otherPerson = CreatePerson( rockContext, 2 );
            var prayerRequest = CreatePrayerRequest( rockContext );
            SetupPrayerRequestCreate( rockContext );

            var skill = CreateSkill( rockContext, null, AudienceType.Public );

            var result = skill.AddOrUpdatePrayerRequest( requestText: "TM003-new", categoryIdKey: prayerRequest.Category.IdKey, personIdKey: otherPerson.IdKey );

            Assert.AreEqual( ToolStatus.Error, result.Status );
            CollectionAssert.Contains( result.ErrorMessages.ToList(), OwnPersonOnlyMessage );
            Assert.IsFalse( rockContext.Set<PrayerRequest>().Any( pr => pr.Id != 0 && pr.Id != PrayerRequestId ) );
        }
    }

    [TestMethod]
    public void AddOrUpdatePrayerRequest_InternalAgentWithNotViewablePersonKey_ReturnsErrorAndSavesNothing()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            var person = CreatePerson( rockContext, 1 );
            var otherPerson = CreatePerson( rockContext, 2 );
            var prayerRequest = CreatePrayerRequest( rockContext );
            SetupPrayerRequestCreate( rockContext );
            DenyAllUsersViewPerson( rockContext, otherPerson.Id );

            var skill = CreateSkill( rockContext, person );

            var result = skill.AddOrUpdatePrayerRequest( requestText: "TM003-new", categoryIdKey: prayerRequest.Category.IdKey, personIdKey: otherPerson.IdKey );

            Assert.AreEqual( ToolStatus.Error, result.Status );
            CollectionAssert.Contains( result.ErrorMessages.ToList(), "The personIdKey is not valid." );
            Assert.IsFalse( rockContext.Set<PrayerRequest>().Any( pr => pr.Id != 0 && pr.Id != PrayerRequestId ) );
        }
    }

    [TestMethod]
    public void AddOrUpdatePrayerRequest_PublicAgentWithNameOnly_CreatesPendingRequestWithoutEmail()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            var person = CreatePerson( rockContext, 1 );
            var prayerRequest = CreatePrayerRequest( rockContext );
            SetupPrayerRequestCreate( rockContext );

            var skill = CreateSkill( rockContext, person, AudienceType.Public );

            var result = skill.AddOrUpdatePrayerRequest( requestText: "TM003-new", categoryIdKey: prayerRequest.Category.IdKey, firstName: "Ted", lastName: "Decker" );

            Assert.AreEqual( ToolStatus.Success, result.Status, string.Join( " ", result.ErrorMessages ?? new List<string>() ) );

            var newRequest = rockContext.Set<PrayerRequest>().Single( pr => pr.Id != PrayerRequestId );
            Assert.AreEqual( "Ted", newRequest.FirstName );
            Assert.AreEqual( "Decker", newRequest.LastName );
            Assert.IsNull( newRequest.Email );
            Assert.IsNull( newRequest.RequestedByPersonAliasId );
            Assert.IsFalse( newRequest.IsApproved ?? false );
        }
    }

    [TestMethod]
    public void AddOrUpdatePrayerRequest_PublicAgentEditorResendsExistingRequesterKey_UpdatesRequest()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            var person = CreatePerson( rockContext, 1 );
            var requester = CreatePerson( rockContext, 2 );
            var prayerRequest = CreatePrayerRequest( rockContext );
            prayerRequest.RequestedByPersonAlias = requester.PrimaryAlias;
            prayerRequest.RequestedByPersonAliasId = requester.PrimaryAliasId;
            AllowAllUsersEdit( rockContext );
            DenyAllUsersViewPerson( rockContext, requester.Id );

            var skill = CreateSkill( rockContext, person, AudienceType.Public );

            var result = skill.AddOrUpdatePrayerRequest( prayerRequestIdKey: prayerRequest.IdKey, requestText: "TM003-edited", personIdKey: requester.IdKey );

            Assert.AreEqual( ToolStatus.Success, result.Status, string.Join( " ", result.ErrorMessages ?? new List<string>() ) );
            Assert.AreEqual( "TM003-edited", prayerRequest.Text );
            Assert.AreEqual( requester.Id, prayerRequest.RequestedByPersonAlias.PersonId );
        }
    }

    [TestMethod]
    public void AddOrUpdatePrayerRequest_RequesterWithoutEdit_UpdatesRequest()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            var person = CreatePerson( rockContext, 1 );
            var prayerRequest = CreatePrayerRequest( rockContext );
            SetRequester( prayerRequest, person );

            var skill = CreateSkill( rockContext, person );

            var result = skill.AddOrUpdatePrayerRequest( prayerRequestIdKey: prayerRequest.IdKey, requestText: "TM003-edited" );

            Assert.AreEqual( ToolStatus.Success, result.Status, string.Join( " ", result.ErrorMessages ?? new List<string>() ) );
            Assert.AreEqual( "TM003-edited", prayerRequest.Text );
            Assert.IsTrue( prayerRequest.IsApproved ?? false );
        }
    }

    [TestMethod]
    public void AddOrUpdatePrayerRequest_CreatorWithoutEdit_UpdatesRequest()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            var person = CreatePerson( rockContext, 1 );
            var prayerRequest = CreatePrayerRequest( rockContext );
            SetCreator( prayerRequest, person );

            var skill = CreateSkill( rockContext, person );

            var result = skill.AddOrUpdatePrayerRequest( prayerRequestIdKey: prayerRequest.IdKey, requestText: "TM003-edited" );

            Assert.AreEqual( ToolStatus.Success, result.Status, string.Join( " ", result.ErrorMessages ?? new List<string>() ) );
            Assert.AreEqual( "TM003-edited", prayerRequest.Text );
        }
    }

    [TestMethod]
    public void AddOrUpdatePrayerRequest_UnrelatedPersonWithoutEdit_ReturnsErrorAndLeavesRequestUnchanged()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            var person = CreatePerson( rockContext, 1 );
            var owner = CreatePerson( rockContext, 2 );
            var prayerRequest = CreatePrayerRequest( rockContext );
            SetRequester( prayerRequest, owner );
            SetCreator( prayerRequest, owner );

            var skill = CreateSkill( rockContext, person, AudienceType.Public );

            var result = skill.AddOrUpdatePrayerRequest( prayerRequestIdKey: prayerRequest.IdKey, requestText: "TM003-edited" );

            Assert.AreEqual( ToolStatus.Error, result.Status );
            CollectionAssert.Contains( result.ErrorMessages.ToList(), "You are not authorized to edit this prayer request." );
            Assert.AreEqual( OriginalText, prayerRequest.Text );
            Assert.IsTrue( prayerRequest.IsApproved ?? false );
        }
    }

    [TestMethod]
    public void DeletePrayerRequest_RequesterWithoutEdit_DeletesRequest()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            var person = CreatePerson( rockContext, 1 );
            var prayerRequest = CreatePrayerRequest( rockContext );
            SetRequester( prayerRequest, person );

            var skill = CreateSkill( rockContext, person );

            var result = skill.DeletePrayerRequest( prayerRequest.IdKey );

            Assert.AreEqual( ToolStatus.Success, result.Status, string.Join( " ", result.ErrorMessages ?? new List<string>() ) );
            Assert.IsFalse( rockContext.Set<PrayerRequest>().Any( pr => pr.Id == PrayerRequestId ) );
        }
    }

    [TestMethod]
    public void DeletePrayerRequest_CreatorWithoutEdit_DeletesRequest()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            var person = CreatePerson( rockContext, 1 );
            var prayerRequest = CreatePrayerRequest( rockContext );
            SetCreator( prayerRequest, person );

            var skill = CreateSkill( rockContext, person );

            var result = skill.DeletePrayerRequest( prayerRequest.IdKey );

            Assert.AreEqual( ToolStatus.Success, result.Status, string.Join( " ", result.ErrorMessages ?? new List<string>() ) );
            Assert.IsFalse( rockContext.Set<PrayerRequest>().Any( pr => pr.Id == PrayerRequestId ) );
        }
    }

    [TestMethod]
    public void DeletePrayerRequest_UnrelatedPersonWithoutEdit_ReturnsErrorAndKeepsRequest()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            var person = CreatePerson( rockContext, 1 );
            var owner = CreatePerson( rockContext, 2 );
            var prayerRequest = CreatePrayerRequest( rockContext );
            SetRequester( prayerRequest, owner );
            SetCreator( prayerRequest, owner );

            var skill = CreateSkill( rockContext, person );

            var result = skill.DeletePrayerRequest( prayerRequest.IdKey );

            Assert.AreEqual( ToolStatus.Error, result.Status );
            CollectionAssert.Contains( result.ErrorMessages.ToList(), "You are not authorized to delete this prayer request." );
            Assert.IsTrue( rockContext.Set<PrayerRequest>().Any( pr => pr.Id == PrayerRequestId ) );
        }
    }

    [TestMethod]
    public void DeletePrayerRequest_WithoutView_ReturnsErrorAndKeepsRequest()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            var person = CreatePerson( rockContext, 1 );
            var prayerRequest = CreatePrayerRequest( rockContext );

            // The person has EDIT on the request, so only the VIEW check on
            // load can stop this.
            AllowAllUsersEdit( rockContext );
            DenyAllUsersViewPrayerRequest( rockContext );

            var skill = CreateSkill( rockContext, person );

            var result = skill.DeletePrayerRequest( prayerRequest.IdKey );

            Assert.AreEqual( ToolStatus.Error, result.Status );
            Assert.IsTrue( rockContext.Set<PrayerRequest>().Any( pr => pr.Id == PrayerRequestId ) );
        }
    }

    [TestMethod]
    public void AddOrUpdatePrayerRequest_PublicAgentAllowedEdit_ResetsApproval()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            var person = CreatePerson( rockContext, 1 );
            var approver = CreatePerson( rockContext, 2 );
            var prayerRequest = CreatePrayerRequest( rockContext );
            SetRequester( prayerRequest, person );
            prayerRequest.ApprovedByPersonAliasId = approver.PrimaryAliasId;
            prayerRequest.ApprovedOnDateTime = RockDateTime.Now;

            var skill = CreateSkill( rockContext, person, AudienceType.Public );

            var result = skill.AddOrUpdatePrayerRequest( prayerRequestIdKey: prayerRequest.IdKey, requestText: "TM003-edited" );

            Assert.AreEqual( ToolStatus.Success, result.Status, string.Join( " ", result.ErrorMessages ?? new List<string>() ) );
            Assert.AreEqual( "TM003-edited", prayerRequest.Text );
            Assert.IsFalse( prayerRequest.IsApproved ?? false );
            Assert.IsNull( prayerRequest.ApprovedByPersonAliasId );
            Assert.IsNull( prayerRequest.ApprovedOnDateTime );
            CollectionAssert.Contains( result.Instructions, "The prayer request has been updated and is pending approval." );
        }
    }

    [TestMethod]
    public void AddOrUpdatePrayerRequest_InternalAgentAllowedEdit_KeepsApproval()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            var person = CreatePerson( rockContext, 1 );
            var approver = CreatePerson( rockContext, 2 );
            var prayerRequest = CreatePrayerRequest( rockContext );
            var approvedOnDateTime = RockDateTime.Now;
            SetRequester( prayerRequest, person );
            prayerRequest.ApprovedByPersonAliasId = approver.PrimaryAliasId;
            prayerRequest.ApprovedOnDateTime = approvedOnDateTime;

            var skill = CreateSkill( rockContext, person );

            var result = skill.AddOrUpdatePrayerRequest( prayerRequestIdKey: prayerRequest.IdKey, requestText: "TM003-edited" );

            Assert.AreEqual( ToolStatus.Success, result.Status, string.Join( " ", result.ErrorMessages ?? new List<string>() ) );
            Assert.IsTrue( prayerRequest.IsApproved ?? false );
            Assert.AreEqual( approver.PrimaryAliasId, prayerRequest.ApprovedByPersonAliasId );
            Assert.AreEqual( approvedOnDateTime, prayerRequest.ApprovedOnDateTime );
            CollectionAssert.Contains( result.Instructions, "The prayer request has been updated and approved." );
        }
    }

    [TestMethod]
    public void AddOrUpdatePrayerRequest_PublicAgentRequesterWithOtherPersonKey_ReturnsErrorAndSavesNothing()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            var person = CreatePerson( rockContext, 1 );
            var approver = CreatePerson( rockContext, 2 );
            var otherPerson = CreatePerson( rockContext, 3 );
            var prayerRequest = CreatePrayerRequest( rockContext );
            SetRequester( prayerRequest, person );
            prayerRequest.ApprovedByPersonAliasId = approver.PrimaryAliasId;
            prayerRequest.ApprovedOnDateTime = RockDateTime.Now;

            // Register the entity types the person security check needs now,
            // so any save made while registering them happens before the call.
            EntityTypeCache.Get<Person>( true, rockContext );
            EntityTypeCache.Get<GlobalDefault>( true, rockContext );
            rockContextMock.Invocations.Clear();

            var skill = CreateSkill( rockContext, person, AudienceType.Public );

            var result = skill.AddOrUpdatePrayerRequest( prayerRequestIdKey: prayerRequest.IdKey, requestText: "TM003-edited", personIdKey: otherPerson.IdKey );

            Assert.AreEqual( ToolStatus.Error, result.Status );
            CollectionAssert.Contains( result.ErrorMessages.ToList(), OwnPersonOnlyMessage );
            Assert.AreEqual( OriginalText, prayerRequest.Text );

            // The mock database shares the tracked entity, so the approval
            // reset made before the error is visible in memory. The stored
            // request only changes if the context is saved, so check that no
            // save happened.
            rockContextMock.Verify( m => m.SaveChanges(), Times.Never() );
            rockContextMock.Verify( m => m.SaveChanges( It.IsAny<bool>() ), Times.Never() );
            rockContextMock.Verify( m => m.SaveChanges( It.IsAny<SaveChangesArgs>() ), Times.Never() );
        }
    }

    [TestMethod]
    public void AddOrUpdatePrayerRequest_AnonymousWithAllUsersEdit_ReturnsErrorAndLeavesRequestUnchanged()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            var prayerRequest = CreatePrayerRequest( rockContext );

            // The request has no requester or creator, and all users have
            // EDIT, so only the anonymous check can stop this.
            AllowAllUsersEdit( rockContext );

            var skill = CreateSkill( rockContext, null, AudienceType.Public );

            var result = skill.AddOrUpdatePrayerRequest( prayerRequestIdKey: prayerRequest.IdKey, requestText: "TM003-edited" );

            Assert.AreEqual( ToolStatus.Error, result.Status );
            CollectionAssert.Contains( result.ErrorMessages.ToList(), "You are not authorized to edit this prayer request." );
            Assert.AreEqual( OriginalText, prayerRequest.Text );
            Assert.IsTrue( prayerRequest.IsApproved ?? false );
        }
    }

    [TestMethod]
    public void DeletePrayerRequest_AnonymousWithAllUsersEdit_ReturnsErrorAndKeepsRequest()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            var prayerRequest = CreatePrayerRequest( rockContext );

            // The request has no requester or creator, and all users have
            // EDIT, so only the anonymous check can stop this.
            AllowAllUsersEdit( rockContext );

            var skill = CreateSkill( rockContext, null, AudienceType.Public );

            var result = skill.DeletePrayerRequest( prayerRequest.IdKey );

            Assert.AreEqual( ToolStatus.Error, result.Status );
            CollectionAssert.Contains( result.ErrorMessages.ToList(), "You are not authorized to delete this prayer request." );
            Assert.IsTrue( rockContext.Set<PrayerRequest>().Any( pr => pr.Id == PrayerRequestId ) );
        }
    }

    private static void SetRequester( PrayerRequest prayerRequest, Person person )
    {
        prayerRequest.RequestedByPersonAlias = person.PrimaryAlias;
        prayerRequest.RequestedByPersonAliasId = person.PrimaryAliasId;
    }

    private static void SetCreator( PrayerRequest prayerRequest, Person person )
    {
        prayerRequest.CreatedByPersonAlias = person.PrimaryAlias;
        prayerRequest.CreatedByPersonAliasId = person.PrimaryAliasId;
    }

    private static void DenyAllUsersViewPrayerRequest( RockContext rockContext )
    {
        rockContext.Set<Auth>().Add( new Auth
        {
            Id = 3,
            EntityTypeId = EntityTypeCache.Get<PrayerRequest>( true, rockContext ).Id,
            EntityId = PrayerRequestId,
            SpecialRole = SpecialRole.AllUsers,
            Action = Authorization.VIEW,
            AllowOrDeny = "D"
        } );
    }

    private static PrayerSkill CreateSkill( RockContext rockContext, Person person, AudienceType audienceType = AudienceType.Internal )
    {
        var skill = new PrayerSkill( NullLogger<PrayerSkill>.Instance );
        skill.Initialize( new Dictionary<string, string>(), CreateAgentRequestContext( rockContext, person, audienceType ) );

        return skill;
    }

    private static Person CreatePerson( RockContext rockContext, int id )
    {
        var person = new Person
        {
            Id = id,
            Guid = Guid.NewGuid(),
            FirstName = $"Test{id}",
            NickName = $"Test{id}",
            LastName = "Person",
            Email = $"test{id}@example.invalid"
        };

        var alias = new PersonAlias
        {
            Id = id,
            Guid = Guid.NewGuid(),
            PersonId = id,
            AliasPersonId = id,
            Person = person
        };

        person.Aliases = new List<PersonAlias> { alias };

        rockContext.Set<Person>().Add( person );
        rockContext.Set<PersonAlias>().Add( alias );

        return person;
    }

    private static PrayerRequest CreatePrayerRequest( RockContext rockContext )
    {
        var category = new Category
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            Name = "General",
            EntityTypeId = EntityTypeCache.Get<PrayerRequest>( true, rockContext ).Id
        };

        rockContext.Set<Category>().Add( category );

        var prayerRequest = new PrayerRequest
        {
            Id = PrayerRequestId,
            Guid = Guid.NewGuid(),
            FirstName = "P3C43",
            LastName = "One",
            Text = OriginalText,
            CategoryId = category.Id,
            Category = category,
            IsActive = true,
            IsApproved = true,
            IsPublic = false,
            IsUrgent = false
        };

        rockContext.Set<PrayerRequest>().Add( prayerRequest );

        return prayerRequest;
    }

    private static void SetupPrayerRequestCreate( RockContext rockContext )
    {
        Mock.Get( rockContext.Set<PrayerRequest>() ).Setup( m => m.Create() ).Returns( () => new PrayerRequest() );

        // Register the entity types the person security check needs now.
        // Otherwise that check registers them later, and that save would give
        // the new, unsaved prayer request an Id in the mock database.
        EntityTypeCache.Get<Person>( true, rockContext );
        EntityTypeCache.Get<GlobalDefault>( true, rockContext );
    }

    private static void DenyAllUsersViewPerson( RockContext rockContext, int personId )
    {
        EntityTypeCache.Get<GlobalDefault>( true, rockContext );

        rockContext.Set<Auth>().Add( new Auth
        {
            Id = 2,
            EntityTypeId = EntityTypeCache.Get<Person>( true, rockContext ).Id,
            EntityId = personId,
            SpecialRole = SpecialRole.AllUsers,
            Action = Authorization.VIEW,
            AllowOrDeny = "D"
        } );
    }

    private static void AllowAllUsersEdit( RockContext rockContext )
    {
        rockContext.Set<Auth>().Add( new Auth
        {
            Id = 1,
            EntityTypeId = EntityTypeCache.Get<PrayerRequest>( true, rockContext ).Id,
            EntityId = PrayerRequestId,
            SpecialRole = SpecialRole.AllUsers,
            Action = Authorization.EDIT,
            AllowOrDeny = "A"
        } );
    }

    private static AgentRequestContext CreateAgentRequestContext( RockContext rockContext, Person currentPerson, AudienceType audienceType )
    {
        var mock = new Mock<AgentRequestContext>();

        mock.Setup( m => m.RockContext ).Returns( rockContext );
        mock.Setup( m => m.CurrentPerson ).Returns( currentPerson );
        mock.Setup( m => m.AudienceType ).Returns( audienceType );

        return mock.Object;
    }
}

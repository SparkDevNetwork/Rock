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

    private static PrayerSkill CreateSkill( RockContext rockContext, Person person )
    {
        var skill = new PrayerSkill( NullLogger<PrayerSkill>.Instance );
        skill.Initialize( new Dictionary<string, string>(), CreateAgentRequestContext( rockContext, person ) );

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
            LastName = "Person"
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

    private static AgentRequestContext CreateAgentRequestContext( RockContext rockContext, Person currentPerson )
    {
        var mock = new Mock<AgentRequestContext>();

        mock.Setup( m => m.RockContext ).Returns( rockContext );
        mock.Setup( m => m.CurrentPerson ).Returns( currentPerson );
        mock.Setup( m => m.AudienceType ).Returns( AudienceType.Internal );

        return mock.Object;
    }
}

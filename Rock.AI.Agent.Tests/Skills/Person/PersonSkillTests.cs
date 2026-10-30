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

namespace Rock.AI.Agent.Tests.Skills.Person;

[TestClass]
public class PersonSkillTests : MockDatabaseTestsBase
{
    private const int CallerPersonId = 1;
    private const int TargetPersonId = 2;
    private const int PhoneTypeValueId = 1;
    private const int LocationTypeValueId = 2;
    private const int NonPhoneTypeValueId = 3;
    private const int PhoneDefinedTypeId = 1;
    private const int OtherDefinedTypeId = 2;
    private const string OriginalEmail = "p9v2h-original@example.invalid";
    private const string NewEmail = "p9v2h-new@example.invalid";
    private const string OriginalNumber = "6235550201";
    private const string NewNumber = "6235550299";
    private const string NotAuthorizedMessage = "You are not authorized to edit this person.";
    private const string LookupsRequiredMessage = "Lookups Required";

    #region UpdatePerson

    [TestMethod]
    public void UpdatePerson_OtherPersonWithoutEdit_ReturnsError()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            var caller = CreatePerson( rockContext, CallerPersonId );
            var target = CreatePerson( rockContext, TargetPersonId );

            var skill = CreateSkill( rockContext, caller );

            var result = skill.UpdatePerson( target.IdKey, email: NewEmail );

            AssertNotAuthorized( result );
            Assert.AreEqual( OriginalEmail, target.Email );
        }
    }

    [TestMethod]
    public void UpdatePerson_NullCurrentPerson_ReturnsError()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            var target = CreatePerson( rockContext, TargetPersonId );

            var skill = CreateSkill( rockContext, null );

            var result = skill.UpdatePerson( target.IdKey, email: NewEmail );

            AssertNotAuthorized( result );
            Assert.AreEqual( OriginalEmail, target.Email );
        }
    }

    [TestMethod]
    public void UpdatePerson_Self_UpdatesEmail()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            var caller = CreatePerson( rockContext, CallerPersonId );

            var skill = CreateSkill( rockContext, caller );

            var result = skill.UpdatePerson( caller.IdKey, email: NewEmail );

            Assert.AreEqual( ToolStatus.Success, result.Status, string.Join( " ", result.ErrorMessages ?? new List<string>() ) );
            Assert.AreEqual( NewEmail, caller.Email );
        }
    }

    [TestMethod]
    public void UpdatePerson_WithEntityEdit_UpdatesEmail()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            var caller = CreatePerson( rockContext, CallerPersonId );
            var target = CreatePerson( rockContext, TargetPersonId );
            AllowAllUsersEdit( rockContext, TargetPersonId );

            var skill = CreateSkill( rockContext, caller );

            var result = skill.UpdatePerson( target.IdKey, email: NewEmail );

            Assert.AreEqual( ToolStatus.Success, result.Status, string.Join( " ", result.ErrorMessages ?? new List<string>() ) );
            Assert.AreEqual( NewEmail, target.Email );
        }
    }

    [TestMethod]
    public void UpdatePerson_WithEntityTypeEdit_UpdatesEmail()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            var caller = CreatePerson( rockContext, CallerPersonId );
            var target = CreatePerson( rockContext, TargetPersonId );

            // EntityId 0 is the type level rule, like the Staff Workers seed.
            AllowAllUsersEdit( rockContext, 0 );

            var skill = CreateSkill( rockContext, caller );

            var result = skill.UpdatePerson( target.IdKey, email: NewEmail );

            Assert.AreEqual( ToolStatus.Success, result.Status, string.Join( " ", result.ErrorMessages ?? new List<string>() ) );
            Assert.AreEqual( 0, result.ErrorMessages?.Count ?? 0 );
            Assert.AreEqual( NewEmail, target.Email );
        }
    }

    #endregion

    #region UpdatePhoneNumber

    [TestMethod]
    public void UpdatePhoneNumber_OtherPersonWithoutEdit_ReturnsError()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            CreateDefinedType( rockContext, PhoneDefinedTypeId, SystemGuid.DefinedType.PERSON_PHONE_TYPE.AsGuid() );
            var phoneType = CreateDefinedValue( rockContext, PhoneTypeValueId, PhoneDefinedTypeId, "Mobile" );
            var caller = CreatePerson( rockContext, CallerPersonId );
            var target = CreatePerson( rockContext, TargetPersonId );
            var phone = CreatePhoneNumber( rockContext, target, PhoneTypeValueId );

            var skill = CreateSkill( rockContext, caller );

            var result = skill.UpdatePhoneNumber( target.IdKey, NewNumber, phoneType.IdKey );

            AssertNotAuthorized( result );
            Assert.AreEqual( OriginalNumber, phone.Number );
            Assert.AreEqual( 1, rockContext.Set<PhoneNumber>().Count() );
        }
    }

    [TestMethod]
    public void UpdatePhoneNumber_SelfWithNoPhoneOfType_AddsPhone()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            CreateDefinedType( rockContext, PhoneDefinedTypeId, SystemGuid.DefinedType.PERSON_PHONE_TYPE.AsGuid() );
            var phoneType = CreateDefinedValue( rockContext, PhoneTypeValueId, PhoneDefinedTypeId, "Mobile" );
            var caller = CreatePerson( rockContext, CallerPersonId );

            var skill = CreateSkill( rockContext, caller );

            var result = skill.UpdatePhoneNumber( caller.IdKey, NewNumber, phoneType.IdKey, isMessagingEnabled: true );

            Assert.AreEqual( ToolStatus.Success, result.Status, string.Join( " ", result.ErrorMessages ?? new List<string>() ) );

            var phone = rockContext.Set<PhoneNumber>().Single();
            Assert.AreEqual( CallerPersonId, phone.PersonId );
            Assert.AreEqual( PhoneTypeValueId, phone.NumberTypeValueId );
            Assert.AreEqual( NewNumber, phone.Number );
            Assert.IsTrue( phone.IsMessagingEnabled );
        }
    }

    [TestMethod]
    public void UpdatePhoneNumber_NonPhoneTypeValue_ReturnsLookupsRequired()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            CreateDefinedType( rockContext, PhoneDefinedTypeId, SystemGuid.DefinedType.PERSON_PHONE_TYPE.AsGuid() );
            CreateDefinedType( rockContext, OtherDefinedTypeId, Guid.NewGuid() );
            CreateDefinedValue( rockContext, PhoneTypeValueId, PhoneDefinedTypeId, "Mobile" );
            var nonPhoneType = CreateDefinedValue( rockContext, NonPhoneTypeValueId, OtherDefinedTypeId, "Active" );
            var caller = CreatePerson( rockContext, CallerPersonId );

            var skill = CreateSkill( rockContext, caller );

            var result = skill.UpdatePhoneNumber( caller.IdKey, NewNumber, nonPhoneType.IdKey );

            Assert.AreEqual( ToolStatus.Error, result.Status );
            CollectionAssert.AreEqual( new List<string> { LookupsRequiredMessage }, result.ErrorMessages.ToList() );
            Assert.AreEqual( 0, rockContext.Set<PhoneNumber>().Count() );
        }
    }

    #endregion

    #region AddOrUpdateAddress

    [TestMethod]
    public void AddOrUpdateAddress_OtherPersonWithViewOnly_ReturnsError()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            var locationType = CreateDefinedValue( rockContext, LocationTypeValueId, OtherDefinedTypeId, "Home" );
            var caller = CreatePerson( rockContext, CallerPersonId );
            var target = CreatePerson( rockContext, TargetPersonId );

            var skill = CreateSkill( rockContext, caller );

            var result = skill.AddOrUpdateAddress( target.IdKey, locationType.IdKey, street1: "1 P9V2H Way", city: "Phoenix", state: "AZ", postalCode: "85001" );

            AssertNotAuthorized( result );
            Assert.AreEqual( 0, rockContext.Set<GroupLocation>().Count() );
            Assert.AreEqual( 0, rockContext.Set<Location>().Count() );
        }
    }

    #endregion

    #region Support Methods

    private static void AssertNotAuthorized( AgentToolResult result )
    {
        Assert.AreEqual( ToolStatus.Error, result.Status );
        CollectionAssert.Contains( result.ErrorMessages, NotAuthorizedMessage );
    }

    private static PersonSkill CreateSkill( RockContext rockContext, Model.Person person )
    {
        var skill = new PersonSkill( NullLogger<PersonSkill>.Instance );
        skill.Initialize( new Dictionary<string, string>(), CreateAgentRequestContext( rockContext, person ) );

        return skill;
    }

    private static Model.Person CreatePerson( RockContext rockContext, int id )
    {
        var person = new Model.Person
        {
            Id = id,
            Guid = Guid.NewGuid(),
            FirstName = $"Test{id}",
            NickName = $"Test{id}",
            LastName = "Person",
            Email = OriginalEmail,
            IsEmailActive = true
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

        rockContext.Set<Model.Person>().Add( person );
        rockContext.Set<PersonAlias>().Add( alias );

        return person;
    }

    private static DefinedType CreateDefinedType( RockContext rockContext, int id, Guid guid )
    {
        var definedType = new DefinedType
        {
            Id = id,
            Guid = guid,
            Name = $"Type{id}",
            IsActive = true
        };

        rockContext.Set<DefinedType>().Add( definedType );

        return definedType;
    }

    private static DefinedValue CreateDefinedValue( RockContext rockContext, int id, int definedTypeId, string value )
    {
        var definedValue = new DefinedValue
        {
            Id = id,
            Guid = Guid.NewGuid(),
            DefinedTypeId = definedTypeId,
            Value = value,
            IsActive = true
        };

        rockContext.Set<DefinedValue>().Add( definedValue );

        return definedValue;
    }

    private static PhoneNumber CreatePhoneNumber( RockContext rockContext, Model.Person person, int numberTypeValueId )
    {
        var phone = new PhoneNumber
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            PersonId = person.Id,
            Person = person,
            NumberTypeValueId = numberTypeValueId,
            Number = OriginalNumber,
            CountryCode = "1"
        };

        rockContext.Set<PhoneNumber>().Add( phone );

        return phone;
    }

    private static void AllowAllUsersEdit( RockContext rockContext, int personId )
    {
        rockContext.Set<Auth>().Add( new Auth
        {
            Id = 1,
            EntityTypeId = EntityTypeCache.Get<Model.Person>( true, rockContext ).Id,
            EntityId = personId,
            SpecialRole = SpecialRole.AllUsers,
            Action = Authorization.EDIT,
            AllowOrDeny = "A"
        } );
    }

    private static AgentRequestContext CreateAgentRequestContext( RockContext rockContext, Model.Person currentPerson )
    {
        var mock = new Mock<AgentRequestContext>();

        mock.Setup( m => m.RockContext ).Returns( rockContext );
        mock.Setup( m => m.CurrentPerson ).Returns( currentPerson );
        mock.Setup( m => m.AudienceType ).Returns( AudienceType.Internal );

        return mock.Object;
    }

    #endregion
}

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Skills;
using Rock.AI.Agent.Utilities.CommunicationSkill;
using Rock.Data;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.Security;
using Rock.SystemKey;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Tests.Skills.Communication;

using Person = Rock.Model.Person;

[TestClass]
public partial class CommunicationSkillTests : MockDatabaseTestsBase
{
    private const int AllowedNumberId = 1;
    private const int RestrictedNumberId = 2;
    private const int CommunicationId = 1;
    private const string InvalidNumberMessage = "does not correspond to a valid active SMS-enabled system phone number";
    private const string FromNumberNotAuthorizedMessage = "You are not authorized to send from this communication's SMS 'from' number.";

    #region SendCommunication

    [TestMethod]
    public void SendCommunication_SmsFromNumberWithoutView_ReturnsError()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            var person = CreatePerson( rockContext, 1 );
            var recipient = CreatePerson( rockContext, 2 );
            CreateSystemPhoneNumbers( rockContext );

            var communication = CreateTransientCommunication( rockContext, person, CommunicationType.SMS, RestrictedNumberId );
            AddRecipient( rockContext, 1, communication, recipient );

            var skill = CreateSkill( rockContext, person );

            var result = skill.SendCommunication( communication.IdKey );

            Assert.AreEqual( ToolStatus.Error, result.Status );
            CollectionAssert.Contains( result.ErrorMessages.ToList(), FromNumberNotAuthorizedMessage );
            Assert.AreEqual( CommunicationStatus.Transient, communication.Status );
            Assert.IsNull( communication.ReviewerPersonAliasId );
        }
    }

    [TestMethod]
    public void SendCommunication_RecipientPreferenceFromNumberWithoutView_ReturnsError()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            var person = CreatePerson( rockContext, 1 );
            var recipient = CreatePerson( rockContext, 2 );
            CreateSystemPhoneNumbers( rockContext );

            var communication = CreateTransientCommunication( rockContext, person, CommunicationType.RecipientPreference, RestrictedNumberId );
            AddRecipient( rockContext, 1, communication, recipient );

            var skill = CreateSkill( rockContext, person );

            var result = skill.SendCommunication( communication.IdKey );

            Assert.AreEqual( ToolStatus.Error, result.Status );
            CollectionAssert.Contains( result.ErrorMessages.ToList(), FromNumberNotAuthorizedMessage );
            Assert.AreEqual( CommunicationStatus.Transient, communication.Status );
        }
    }

    [TestMethod]
    public void SendCommunication_SmsFromNumberWithView_IsApproved()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            var person = CreatePerson( rockContext, 1 );
            var recipient = CreatePerson( rockContext, 2 );
            CreateSystemPhoneNumbers( rockContext );

            var communication = CreateTransientCommunication( rockContext, person, CommunicationType.SMS, AllowedNumberId );
            AddRecipient( rockContext, 1, communication, recipient );

            var skill = CreateSkill( rockContext, person );

            SendCommunicationIgnoringBus( skill, communication.IdKey );

            Assert.AreEqual( CommunicationStatus.Approved, communication.Status );
        }
    }

    [TestMethod]
    public void SendCommunication_EmailWithRestrictedSmsFromNumber_IsApproved()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            var person = CreatePerson( rockContext, 1 );
            var recipient = CreatePerson( rockContext, 2 );
            CreateSystemPhoneNumbers( rockContext );

            var communication = CreateTransientCommunication( rockContext, person, CommunicationType.Email, RestrictedNumberId );
            AddRecipient( rockContext, 1, communication, recipient );

            var skill = CreateSkill( rockContext, person );

            SendCommunicationIgnoringBus( skill, communication.IdKey );

            Assert.AreEqual( CommunicationStatus.Approved, communication.Status );
        }
    }

    #endregion

    #region UpdateCurrentPersonDefaultSmsPhoneNumber

    [TestMethod]
    public void UpdateCurrentPersonDefaultSmsPhoneNumber_NumberWithoutView_ReturnsError()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            var person = CreatePerson( rockContext, 1 );
            CreateSystemPhoneNumbers( rockContext );

            var skill = CreateSkill( rockContext, person );

            var result = skill.UpdateCurrentPersonDefaultSmsPhoneNumber( IdHasher.Instance.GetHash( RestrictedNumberId ) );

            Assert.AreEqual( ToolStatus.Error, result.Status );
            Assert.IsTrue( result.ErrorMessages.Any( m => m.Contains( InvalidNumberMessage ) ) );
            Assert.IsFalse( rockContext.Set<PersonPreference>().Any() );
        }
    }

    [TestMethod]
    public void UpdateCurrentPersonDefaultSmsPhoneNumber_NumberWithView_Succeeds()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            var person = CreatePerson( rockContext, 1 );
            CreateSystemPhoneNumbers( rockContext );

            var skill = CreateSkill( rockContext, person );

            // Saving the preference creates its own RockContext, which the
            // mock cannot provide. Reaching the save shows the number was
            // accepted; a refused number returns an error result instead.
            var reachedSave = false;

            try
            {
                var result = skill.UpdateCurrentPersonDefaultSmsPhoneNumber( IdHasher.Instance.GetHash( AllowedNumberId ) );

                Assert.AreEqual( ToolStatus.Success, result.Status, string.Join( " ", result.ErrorMessages ?? new List<string>() ) );
            }
            catch ( ArgumentException )
            {
                reachedSave = true;
            }

            Assert.IsTrue( reachedSave );
        }
    }

    #endregion

    #region GetDraftCommunicationInstructions

    [TestMethod]
    public void GetDraftCommunicationInstructions_FromNumberWithoutView_ReturnsInvalidNumberError()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            var person = CreatePerson( rockContext, 1 );
            var recipient = CreatePerson( rockContext, 2 );
            CreateSystemPhoneNumbers( rockContext );

            var skill = CreateSkill( rockContext, person );

            var result = skill.GetDraftCommunicationInstructions( recipient.IdKey, AgentCommunicationType.Sms, fromNumberIdKey: IdHasher.Instance.GetHash( RestrictedNumberId ) );

            Assert.AreEqual( ToolStatus.Error, result.Status );
            Assert.IsTrue( result.ErrorMessages.Any( m => m.Contains( InvalidNumberMessage ) ) );
        }
    }

    [TestMethod]
    public void GetDraftCommunicationInstructions_SavedDefaultWithoutView_IsIgnored()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            var person = CreatePerson( rockContext, 1 );
            var recipient = CreatePerson( rockContext, 2 );
            CreateSystemPhoneNumbers( rockContext );

            AddSavedDefaultSmsNumber( rockContext, person, RestrictedNumberId );

            var skill = CreateSkill( rockContext, person );

            var result = skill.GetDraftCommunicationInstructions( recipient.IdKey, AgentCommunicationType.Sms );

            Assert.AreEqual( ToolStatus.Error, result.Status );
            Assert.IsTrue( result.ErrorMessages.Any( m => m.Contains( "No valid default SMS 'from' number" ) ) );
        }
    }

    [TestMethod]
    public void GetDraftCommunicationInstructions_AssignedNumberWithoutView_IsNotUsedAsDefault()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            var person = CreatePerson( rockContext, 1 );
            var recipient = CreatePerson( rockContext, 2 );
            CreateSystemPhoneNumbers( rockContext, restrictedNumberAssignedTo: person );

            var skill = CreateSkill( rockContext, person );

            var result = skill.GetDraftCommunicationInstructions( recipient.IdKey, AgentCommunicationType.Sms );

            Assert.AreEqual( ToolStatus.Error, result.Status );
            Assert.IsTrue( result.ErrorMessages.Any( m => m.Contains( "No valid default SMS 'from' number" ) ) );
        }
    }

    [TestMethod]
    public void GetDraftCommunicationInstructions_FromNumberWithView_IsAccepted()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            var person = CreatePerson( rockContext, 1 );
            var recipient = CreatePerson( rockContext, 2 );
            CreateSystemPhoneNumbers( rockContext );

            var skill = CreateSkill( rockContext, person );

            var result = skill.GetDraftCommunicationInstructions( recipient.IdKey, AgentCommunicationType.Sms, fromNumberIdKey: IdHasher.Instance.GetHash( AllowedNumberId ) );

            AssertFromNumberWasResolved( result );
        }
    }

    [TestMethod]
    public void GetDraftCommunicationInstructions_SavedDefaultWithView_IsUsed()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            var person = CreatePerson( rockContext, 1 );
            var recipient = CreatePerson( rockContext, 2 );
            CreateSystemPhoneNumbers( rockContext );
            AddSavedDefaultSmsNumber( rockContext, person, AllowedNumberId );

            var skill = CreateSkill( rockContext, person );

            var result = skill.GetDraftCommunicationInstructions( recipient.IdKey, AgentCommunicationType.Sms );

            AssertFromNumberWasResolved( result );
        }
    }

    [TestMethod]
    public void GetDraftCommunicationInstructions_AssignedNumberWithView_IsUsedAsDefault()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            var person = CreatePerson( rockContext, 1 );
            var recipient = CreatePerson( rockContext, 2 );
            CreateSystemPhoneNumbers( rockContext, allowedNumberAssignedTo: person );

            var skill = CreateSkill( rockContext, person );

            var result = skill.GetDraftCommunicationInstructions( recipient.IdKey, AgentCommunicationType.Sms );

            AssertFromNumberWasResolved( result );
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Asserts that the SMS 'from' number was accepted. The mock has no active
    /// SMS transport, so drafting stops later with an unsupported medium
    /// error, but it must not stop on the 'from' number.
    /// </summary>
    private static void AssertFromNumberWasResolved( AgentToolResult result )
    {
        var errors = result.ErrorMessages ?? new List<string>();

        Assert.IsFalse( errors.Any( m => m.Contains( InvalidNumberMessage ) ), string.Join( " ", errors ) );
        Assert.IsFalse( errors.Any( m => m.Contains( "No valid default SMS 'from' number" ) ), string.Join( " ", errors ) );
    }

    private static void AddSavedDefaultSmsNumber( RockContext rockContext, Person person, int systemPhoneNumberId )
    {
        rockContext.Set<PersonPreference>().Add( new PersonPreference
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            PersonAliasId = person.PrimaryAliasId.Value,
            PersonAlias = person.Aliases.First(),
            Key = PersonPreferenceService.GetGlobalPreferencePrefix() + PersonPreferenceKey.DEFAULT_SMS_PHONE_NUMBER,
            Value = systemPhoneNumberId.ToString(),
            LastAccessedDateTime = RockDateTime.Now
        } );
    }

    private static Rock.AI.Agent.Skills.CommunicationSkill CreateSkill( RockContext rockContext, Person currentPerson )
    {
        var skill = new Rock.AI.Agent.Skills.CommunicationSkill( NullLogger<Rock.AI.Agent.Skills.CommunicationSkill>.Instance );

        skill.Initialize( new Dictionary<string, string>(), CreateAgentRequestContext( rockContext, currentPerson ) );

        return skill;
    }

    /// <summary>
    /// Calls SendCommunication for a communication that is expected to pass
    /// every check. The mocked message bus cannot send commands, so queueing
    /// the send throws after the communication has been approved and saved.
    /// </summary>
    private static void SendCommunicationIgnoringBus( Rock.AI.Agent.Skills.CommunicationSkill skill, string communicationIdKey )
    {
        try
        {
            skill.SendCommunication( communicationIdKey );
        }
        catch ( MockException )
        {
        }
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
            PrimaryAliasId = id
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

    /// <summary>
    /// Creates an allowed number with no security rules and a restricted
    /// number that denies VIEW to all users.
    /// </summary>
    private static void CreateSystemPhoneNumbers( RockContext rockContext, Person restrictedNumberAssignedTo = null, Person allowedNumberAssignedTo = null )
    {
        rockContext.Set<SystemPhoneNumber>().Add( new SystemPhoneNumber
        {
            Id = AllowedNumberId,
            Guid = Guid.NewGuid(),
            Name = "Main",
            Number = "6235559001",
            IsActive = true,
            IsSmsEnabled = true,
            AssignedToPersonAliasId = allowedNumberAssignedTo?.PrimaryAliasId
        } );

        rockContext.Set<SystemPhoneNumber>().Add( new SystemPhoneNumber
        {
            Id = RestrictedNumberId,
            Guid = Guid.NewGuid(),
            Name = "Pastor",
            Number = "6235559002",
            IsActive = true,
            IsSmsEnabled = true,
            AssignedToPersonAliasId = restrictedNumberAssignedTo?.PrimaryAliasId
        } );

        rockContext.Set<Auth>().Add( new Auth
        {
            Id = 1,
            EntityTypeId = EntityTypeCache.Get<SystemPhoneNumber>( true, rockContext ).Id,
            EntityId = RestrictedNumberId,
            SpecialRole = SpecialRole.AllUsers,
            Action = Authorization.VIEW,
            AllowOrDeny = "D"
        } );
    }

    private static Rock.Model.Communication CreateTransientCommunication( RockContext rockContext, Person sender, CommunicationType communicationType, int? smsFromNumberId )
    {
        var alias = sender.Aliases.First();

        var communication = new Rock.Model.Communication
        {
            Id = CommunicationId,
            Guid = Guid.NewGuid(),
            Status = CommunicationStatus.Transient,
            CommunicationType = communicationType,
            SenderPersonAliasId = alias.Id,
            SenderPersonAlias = alias,
            CreatedByPersonAliasId = alias.Id,
            CreatedByPersonAlias = alias,
            SmsFromSystemPhoneNumberId = smsFromNumberId
        };

        rockContext.Set<Rock.Model.Communication>().Add( communication );

        return communication;
    }

    private static void AddRecipient( RockContext rockContext, int id, Rock.Model.Communication communication, Person recipient )
    {
        rockContext.Set<CommunicationRecipient>().Add( new CommunicationRecipient
        {
            Id = id,
            Guid = Guid.NewGuid(),
            CommunicationId = communication.Id,
            Communication = communication,
            PersonAliasId = recipient.PrimaryAliasId.Value,
            PersonAlias = recipient.Aliases.First()
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

    #endregion
}

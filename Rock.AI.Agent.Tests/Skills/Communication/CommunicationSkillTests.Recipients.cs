using System;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;

namespace Rock.AI.Agent.Tests.Skills.Communication;

public partial class CommunicationSkillTests
{
    private const string MultipleRecipientsMessage = "This communication has more than one recipient and cannot be sent by the agent.";

    #region SendCommunication Recipients

    [TestMethod]
    public void SendCommunication_TwoDifferentRecipients_ReturnsError()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            var person = CreatePerson( rockContext, 1 );
            var recipientA = CreatePerson( rockContext, 2 );
            var recipientB = CreatePerson( rockContext, 3 );

            var communication = CreateTransientCommunication( rockContext, person, CommunicationType.Email, null );
            AddRecipient( rockContext, 1, communication, recipientA );
            AddRecipient( rockContext, 2, communication, recipientB );

            var skill = CreateSkill( rockContext, person );

            var result = skill.SendCommunication( communication.IdKey );

            Assert.AreEqual( ToolStatus.Error, result.Status );
            CollectionAssert.Contains( result.ErrorMessages.ToList(), MultipleRecipientsMessage );
            Assert.AreEqual( CommunicationStatus.Transient, communication.Status );
            Assert.IsNull( communication.ReviewerPersonAliasId );
        }
    }

    [TestMethod]
    public void SendCommunication_ListCommunication_ReturnsError()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            var person = CreatePerson( rockContext, 1 );
            var recipient = CreatePerson( rockContext, 2 );

            var communication = CreateTransientCommunication( rockContext, person, CommunicationType.Email, null );
            communication.ListGroupId = 5;
            AddRecipient( rockContext, 1, communication, recipient );

            var skill = CreateSkill( rockContext, person );

            var result = skill.SendCommunication( communication.IdKey );

            Assert.AreEqual( ToolStatus.Error, result.Status );
            CollectionAssert.Contains( result.ErrorMessages.ToList(), MultipleRecipientsMessage );
            Assert.AreEqual( CommunicationStatus.Transient, communication.Status );
        }
    }

    [TestMethod]
    public void SendCommunication_RecipientWithoutPersonAlias_ReturnsError()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            var person = CreatePerson( rockContext, 1 );

            // A personal device row for push has no person alias.
            var communication = CreateTransientCommunication( rockContext, person, CommunicationType.PushNotification, null );
            rockContext.Set<CommunicationRecipient>().Add( new CommunicationRecipient
            {
                Id = 1,
                Guid = Guid.NewGuid(),
                CommunicationId = communication.Id,
                Communication = communication,
                PersonalDeviceId = 1
            } );

            var skill = CreateSkill( rockContext, person );

            var result = skill.SendCommunication( communication.IdKey );

            Assert.AreEqual( ToolStatus.Error, result.Status );
            CollectionAssert.Contains( result.ErrorMessages.ToList(), MultipleRecipientsMessage );
            Assert.AreEqual( CommunicationStatus.Transient, communication.Status );
        }
    }

    [TestMethod]
    public void SendCommunication_RedraftedSameRecipient_IsApproved()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            var person = CreatePerson( rockContext, 1 );
            var recipient = CreatePerson( rockContext, 2 );

            // Re-drafting adds another recipient row for the same person.
            var communication = CreateTransientCommunication( rockContext, person, CommunicationType.Email, null );
            AddRecipient( rockContext, 1, communication, recipient );
            AddRecipient( rockContext, 2, communication, recipient );

            var skill = CreateSkill( rockContext, person );

            SendCommunicationIgnoringBus( skill, communication.IdKey );

            Assert.AreEqual( CommunicationStatus.Approved, communication.Status );
        }
    }

    #endregion
}

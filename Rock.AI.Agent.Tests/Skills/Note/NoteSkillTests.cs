using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.AI.Agent.Skills;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;

namespace Rock.AI.Agent.Tests.Skills.Note;

using Person = Rock.Model.Person;

[TestClass]
public class NoteSkillTests : MockDatabaseTestsBase
{
    private const int PersonEntityTypeId = 15;
    private const int NoteTypeId = 1;

    [TestMethod]
    public void ListNotes_ExcludesPrivateNotesOfOtherPeople()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        var personA = CreatePerson( rockContext, 1 );
        var personB = CreatePerson( rockContext, 2 );

        rockContext.Set<EntityType>().Add( new EntityType
        {
            Id = PersonEntityTypeId,
            Guid = new Guid( Rock.SystemGuid.EntityType.PERSON ),
            Name = "Rock.Model.Person",
            FriendlyName = "Person"
        } );

        var noteType = new NoteType
        {
            Id = NoteTypeId,
            Guid = Guid.NewGuid(),
            Name = "Personal Note",
            EntityTypeId = PersonEntityTypeId,
            UserSelectable = true
        };

        rockContext.Set<NoteType>().Add( noteType );

        rockContext.Set<Rock.Model.Note>().Add( CreateNote( 1, noteType, personA, "TH032-a-own", true ) );
        rockContext.Set<Rock.Model.Note>().Add( CreateNote( 2, noteType, personB, "TH032-private", true ) );

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            var skill = new NoteSkill( NullLogger<NoteSkill>.Instance );
            skill.Initialize( new Dictionary<string, string>(), CreateAgentRequestContext( rockContext, personA ) );

            var result = skill.ListNotes();

            var page = result.Content as PaginatedResult<NoteResult>;
            Assert.IsNotNull( page, "Expected a page of note results." );

            var texts = page.Items.Select( n => n.Text ).ToList();

            CollectionAssert.Contains( texts, "TH032-a-own" );
            CollectionAssert.DoesNotContain( texts, "TH032-private" );
        }
    }

    [TestMethod]
    public void ListNotes_ExcludesPendingNotesOfOtherPeople()
    {
        var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
        var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
        var rockContext = rockContextMock.Object;

        var personA = CreatePerson( rockContext, 1 );
        var personB = CreatePerson( rockContext, 2 );

        rockContext.Set<EntityType>().Add( new EntityType
        {
            Id = PersonEntityTypeId,
            Guid = new Guid( Rock.SystemGuid.EntityType.PERSON ),
            Name = "Rock.Model.Person",
            FriendlyName = "Person"
        } );

        var noteType = new NoteType
        {
            Id = NoteTypeId,
            Guid = Guid.NewGuid(),
            Name = "Approval Note",
            EntityTypeId = PersonEntityTypeId,
            UserSelectable = true,
            RequiresApprovals = true
        };

        rockContext.Set<NoteType>().Add( noteType );

        var ownPending = CreateNote( 1, noteType, personA, "TH032-a-pending", false );
        ownPending.ApprovalStatus = NoteApprovalStatus.PendingApproval;

        var otherPending = CreateNote( 2, noteType, personB, "TH032-b-pending", false );
        otherPending.ApprovalStatus = NoteApprovalStatus.PendingApproval;

        var otherApproved = CreateNote( 3, noteType, personB, "TH032-b-approved", false );
        otherApproved.ApprovalStatus = NoteApprovalStatus.Approved;

        rockContext.Set<Rock.Model.Note>().Add( ownPending );
        rockContext.Set<Rock.Model.Note>().Add( otherPending );
        rockContext.Set<Rock.Model.Note>().Add( otherApproved );

        using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
        {
            var skill = new NoteSkill( NullLogger<NoteSkill>.Instance );
            skill.Initialize( new Dictionary<string, string>(), CreateAgentRequestContext( rockContext, personA ) );

            var result = skill.ListNotes();

            var page = result.Content as PaginatedResult<NoteResult>;
            Assert.IsNotNull( page, "Expected a page of note results." );

            var texts = page.Items.Select( n => n.Text ).ToList();

            CollectionAssert.Contains( texts, "TH032-a-pending" );
            CollectionAssert.Contains( texts, "TH032-b-approved" );
            CollectionAssert.DoesNotContain( texts, "TH032-b-pending" );
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

    private static Rock.Model.Note CreateNote( int id, NoteType noteType, Person author, string text, bool isPrivate )
    {
        var alias = author.Aliases.First();

        return new Rock.Model.Note
        {
            Id = id,
            Guid = Guid.NewGuid(),
            NoteTypeId = noteType.Id,
            NoteType = noteType,
            Text = text,
            Caption = string.Empty,
            IsPrivateNote = isPrivate,
            CreatedByPersonAliasId = alias.Id,
            CreatedByPersonAlias = alias,
            CreatedDateTime = RockDateTime.Now.AddMinutes( -id )
        };
    }

    private static AgentRequestContext CreateAgentRequestContext( RockContext rockContext, Person currentPerson )
    {
        var mock = new Mock<AgentRequestContext>();

        mock.Setup( m => m.RockContext ).Returns( rockContext );
        mock.Setup( m => m.CurrentPerson ).Returns( currentPerson );
        mock.Setup( m => m.AudienceType ).Returns( Rock.Enums.AI.Agent.AudienceType.Internal );

        return mock.Object;
    }
}

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

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.Configuration;
using Rock.Model;
using Rock.Tests.Shared.TestAccess.AI.Agent;
using Rock.Tests.Shared.TestFramework;

namespace Rock.AI.Agent.Tests.Skills.NoteSkill;

public partial class NoteSkillTests
{
    #region ListNotes

    [TestMethod]
    public void ListNotes_ExcludesPrivateNotesOfOtherPeople()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = MockData.CreatePerson( rockContext, "Current", "Person" );
        var otherPerson = MockData.CreatePerson( rockContext, "Other", "Person" );
        var noteType = SeedPersonNoteType( rockContext, 10 );

        var ownNote = SeedNote( rockContext, 20, noteType, otherPerson, currentPerson );
        ownNote.Text = "TH032-a-own";
        ownNote.IsPrivateNote = true;
        ownNote.CreatedDateTime = RockDateTime.Now.AddMinutes( -1 );

        var otherNote = SeedNote( rockContext, 21, noteType, currentPerson, otherPerson );
        otherNote.Text = "TH032-private";
        otherNote.IsPrivateNote = true;
        otherNote.CreatedDateTime = RockDateTime.Now.AddMinutes( -2 );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.ListNotes();

        var page = result.GetContent() as PaginatedResult<NoteResult>;
        Assert.IsNotNull( page, "Expected a page of note results." );

        var texts = page.Items.Select( n => n.Text ).ToList();

        CollectionAssert.Contains( texts, "TH032-a-own" );
        CollectionAssert.DoesNotContain( texts, "TH032-private" );
    }

    [TestMethod]
    public void ListNotes_ExcludesPendingNotesOfOtherPeople()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = MockData.CreatePerson( rockContext, "Current", "Person" );
        var otherPerson = MockData.CreatePerson( rockContext, "Other", "Person" );
        var noteType = SeedPersonNoteType( rockContext, 10 );
        noteType.RequiresApprovals = true;

        var ownPending = SeedNote( rockContext, 20, noteType, otherPerson, currentPerson );
        ownPending.Text = "TH032-a-pending";
        ownPending.ApprovalStatus = NoteApprovalStatus.PendingApproval;
        ownPending.CreatedDateTime = RockDateTime.Now.AddMinutes( -1 );

        var otherPending = SeedNote( rockContext, 21, noteType, currentPerson, otherPerson );
        otherPending.Text = "TH032-b-pending";
        otherPending.ApprovalStatus = NoteApprovalStatus.PendingApproval;
        otherPending.CreatedDateTime = RockDateTime.Now.AddMinutes( -2 );

        var otherApproved = SeedNote( rockContext, 22, noteType, currentPerson, otherPerson );
        otherApproved.Text = "TH032-b-approved";
        otherApproved.ApprovalStatus = NoteApprovalStatus.Approved;
        otherApproved.CreatedDateTime = RockDateTime.Now.AddMinutes( -3 );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.ListNotes();

        var page = result.GetContent() as PaginatedResult<NoteResult>;
        Assert.IsNotNull( page, "Expected a page of note results." );

        var texts = page.Items.Select( n => n.Text ).ToList();

        CollectionAssert.Contains( texts, "TH032-a-pending" );
        CollectionAssert.Contains( texts, "TH032-b-approved" );
        CollectionAssert.DoesNotContain( texts, "TH032-b-pending" );
    }

    #endregion
}

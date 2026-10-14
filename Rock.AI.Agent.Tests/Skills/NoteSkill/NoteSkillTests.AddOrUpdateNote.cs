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

using Rock.Configuration;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.Security;
using Rock.Tests.Shared.TestAccess.AI.Agent;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;

namespace Rock.AI.Agent.Tests.Skills.NoteSkill;

public partial class NoteSkillTests
{
    #region AddOrUpdateNote

    [TestMethod]
    public void AddOrUpdateNote_AddWithNoteTypeEdit_AddsNote()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = MockData.CreatePerson( rockContext, "Current", "Person" );
        var target = MockData.CreatePerson( rockContext, "Target", "Person" );
        var noteType = SeedPersonNoteType( rockContext, 10 );
        MockAuthorizationHelper.AllowAllUsers<Rock.Model.NoteType>( rockContext, Authorization.EDIT, noteType.Id );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.AddOrUpdateNote(
            noteTypeIdKey: IdHasher.Instance.GetHash( noteType.Id ),
            entityIdKey: IdHasher.Instance.GetHash( target.Id ),
            note: "A new note." );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.IsTrue( rockContext.Set<Note>().Any( n => n.EntityId == target.Id && n.Text == "A new note." ) );
    }

    [TestMethod]
    public void AddOrUpdateNote_AddWithoutNoteTypeEdit_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = MockData.CreatePerson( rockContext, "Current", "Person" );
        var target = MockData.CreatePerson( rockContext, "Target", "Person" );
        var noteType = SeedPersonNoteType( rockContext, 10 );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.AddOrUpdateNote(
            noteTypeIdKey: IdHasher.Instance.GetHash( noteType.Id ),
            entityIdKey: IdHasher.Instance.GetHash( target.Id ),
            note: "A new note." );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "not authorized to add notes of this type" ) ) );
        Assert.IsFalse( rockContext.Set<Note>().Any( n => n.Text == "A new note." && n.Id != 0 ) );
    }

    [TestMethod]
    public void AddOrUpdateNote_AddWithNonUserSelectableNoteType_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = MockData.CreatePerson( rockContext, "Current", "Person" );
        var target = MockData.CreatePerson( rockContext, "Target", "Person" );
        var noteType = SeedPersonNoteType( rockContext, 10 );
        noteType.UserSelectable = false;
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.EDIT );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.AddOrUpdateNote(
            noteTypeIdKey: IdHasher.Instance.GetHash( noteType.Id ),
            entityIdKey: IdHasher.Instance.GetHash( target.Id ),
            note: "A new note." );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "cannot be added manually" ) ) );
    }

    [TestMethod]
    public void AddOrUpdateNote_AddWithMissingTargetEntity_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = MockData.CreatePerson( rockContext, "Current", "Person" );
        var noteType = SeedPersonNoteType( rockContext, 10 );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.EDIT );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.AddOrUpdateNote(
            noteTypeIdKey: IdHasher.Instance.GetHash( noteType.Id ),
            entityIdKey: IdHasher.Instance.GetHash( 999 ),
            note: "A new note." );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "was not found" ) ) );
    }

    [TestMethod]
    public void AddOrUpdateNote_AddWithoutTargetViewAuthorization_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = MockData.CreatePerson( rockContext, "Current", "Person" );
        var target = MockData.CreatePerson( rockContext, "Target", "Person" );
        var noteType = SeedPersonNoteType( rockContext, 10 );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.EDIT );
        MockAuthorizationHelper.DenyAllUsers<Rock.Model.Person>( rockContext, Authorization.VIEW, target.Id );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.AddOrUpdateNote(
            noteTypeIdKey: IdHasher.Instance.GetHash( noteType.Id ),
            entityIdKey: IdHasher.Instance.GetHash( target.Id ),
            note: "A new note." );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "not authorized to add notes to this entity" ) ) );
    }

    [TestMethod]
    public void AddOrUpdateNote_UpdateByCreator_UpdatesNote()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = MockData.CreatePerson( rockContext, "Current", "Person" );
        var target = MockData.CreatePerson( rockContext, "Target", "Person" );
        var noteType = SeedPersonNoteType( rockContext, 10 );
        var note = SeedNote( rockContext, 20, noteType, target, currentPerson );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.AddOrUpdateNote(
            noteIdKey: IdHasher.Instance.GetHash( note.Id ),
            note: "Updated text" );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.AreEqual( "Updated text", note.Text );
    }

    [TestMethod]
    public void AddOrUpdateNote_UpdateByNonCreatorWithNoteTypeEdit_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var creator = MockData.CreatePerson( rockContext, "Creator", "Person" );
        var currentPerson = MockData.CreatePerson( rockContext, "Current", "Person" );
        var target = MockData.CreatePerson( rockContext, "Target", "Person" );
        var noteType = SeedPersonNoteType( rockContext, 10 );
        var note = SeedNote( rockContext, 20, noteType, target, creator );

        // EDIT on the note type lets a person add notes, but editing somebody
        // else's note requires ADMINISTRATE.
        MockAuthorizationHelper.AllowAllUsers<Rock.Model.NoteType>( rockContext, Authorization.EDIT, noteType.Id );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.AddOrUpdateNote(
            noteIdKey: IdHasher.Instance.GetHash( note.Id ),
            note: "Updated text" );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "not authorized to edit this note" ) ) );
        Assert.AreEqual( "Original text", note.Text );
    }

    [TestMethod]
    public void AddOrUpdateNote_UpdateByNonCreatorWithAdministrate_UpdatesNote()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var creator = MockData.CreatePerson( rockContext, "Creator", "Person" );
        var currentPerson = MockData.CreatePerson( rockContext, "Current", "Person" );
        var target = MockData.CreatePerson( rockContext, "Target", "Person" );
        var noteType = SeedPersonNoteType( rockContext, 10 );
        var note = SeedNote( rockContext, 20, noteType, target, creator );
        MockAuthorizationHelper.AllowAllUsers<Rock.Model.NoteType>( rockContext, Authorization.ADMINISTRATE, noteType.Id );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.AddOrUpdateNote(
            noteIdKey: IdHasher.Instance.GetHash( note.Id ),
            note: "Updated text" );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.AreEqual( "Updated text", note.Text );
    }

    [TestMethod]
    public void AddOrUpdateNote_UpdatePrivateFlagByNonCreator_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var creator = MockData.CreatePerson( rockContext, "Creator", "Person" );
        var currentPerson = MockData.CreatePerson( rockContext, "Current", "Person" );
        var target = MockData.CreatePerson( rockContext, "Target", "Person" );
        var noteType = SeedPersonNoteType( rockContext, 10 );
        var note = SeedNote( rockContext, 20, noteType, target, creator );
        MockAuthorizationHelper.AllowAllUsers<Rock.Model.NoteType>( rockContext, Authorization.ADMINISTRATE, noteType.Id );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.AddOrUpdateNote(
            noteIdKey: IdHasher.Instance.GetHash( note.Id ),
            isPrivateNote: true );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "private" ) ) );
        Assert.IsFalse( note.IsPrivateNote );
    }

    #endregion
}

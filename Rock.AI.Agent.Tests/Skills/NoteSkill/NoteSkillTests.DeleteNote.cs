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
    #region DeleteNote

    [TestMethod]
    public void DeleteNote_ByCreator_DeletesNote()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = MockData.CreatePerson( rockContext, "Current", "Person" );
        var target = MockData.CreatePerson( rockContext, "Target", "Person" );
        var noteType = SeedPersonNoteType( rockContext, 10 );
        var note = SeedNote( rockContext, 20, noteType, target, currentPerson );
        SetupNoNoteReplies( rockContext );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.DeleteNote( IdHasher.Instance.GetHash( note.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.IsFalse( rockContext.Set<Note>().Any( n => n.Id == note.Id ) );
    }

    [TestMethod]
    public void DeleteNote_ByNonCreatorWithNoteTypeEdit_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var creator = MockData.CreatePerson( rockContext, "Creator", "Person" );
        var currentPerson = MockData.CreatePerson( rockContext, "Current", "Person" );
        var target = MockData.CreatePerson( rockContext, "Target", "Person" );
        var noteType = SeedPersonNoteType( rockContext, 10 );
        var note = SeedNote( rockContext, 20, noteType, target, creator );

        // EDIT on the note type lets a person add notes, but deleting somebody
        // else's note requires ADMINISTRATE.
        MockAuthorizationHelper.AllowAllUsers<Rock.Model.NoteType>( rockContext, Authorization.EDIT, noteType.Id );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.DeleteNote( IdHasher.Instance.GetHash( note.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "not authorized" ) ) );
        Assert.IsTrue( rockContext.Set<Note>().Any( n => n.Id == note.Id ) );
    }

    #endregion
}

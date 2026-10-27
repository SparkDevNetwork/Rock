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

using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.AI.Agent;
using Rock.AI.Agent.Skills;
using Rock.Data;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.Tests.Shared.TestAccess.AI.Agent;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Tests.Skills.NoteSkill;

/// <summary>
/// Mocked-database unit tests for <see cref="NoteSkill"/>. Each tool's tests live
/// in their own partial file; shared setup helpers are kept here.
/// </summary>
[TestClass]
public partial class NoteSkillTests
{
    #region Support

    private static Rock.AI.Agent.Skills.NoteSkill CreateSkill( System.IServiceProvider serviceProvider, AgentRequestContext agentRequestContext )
    {
        return AgentSkillTestFactory.CreateSkill<Rock.AI.Agent.Skills.NoteSkill>( serviceProvider, agentRequestContext );
    }

    private static AgentRequestContext CreateRequestContext( RockContext rockContext, Rock.Model.Person currentPerson = null, AudienceType audienceType = AudienceType.Internal )
    {
        return new TestAgentRequestContext( rockContext, currentPerson, audienceType: audienceType );
    }

    /// <summary>
    /// Seeds a user selectable note type for person notes and returns it.
    /// </summary>
    private static Rock.Model.NoteType SeedPersonNoteType( RockContext rockContext, int id )
    {
        var noteType = new Rock.Model.NoteType
        {
            Id = id,
            Guid = Guid.NewGuid(),
            Name = $"Note Type {id}",
            EntityTypeId = EntityTypeCache.Get<Rock.Model.Person>( true, rockContext ).Id,
            UserSelectable = true
        };

        rockContext.Set<Rock.Model.NoteType>().Add( noteType );

        return noteType;
    }

    /// <summary>
    /// Seeds an existing note on the target person, authored by the creator,
    /// and returns it. The mocked context performs no navigation-property
    /// fixup, so the author's alias is wired by hand because note security
    /// compares against it.
    /// </summary>
    private static Note SeedNote( RockContext rockContext, int id, Rock.Model.NoteType noteType, Rock.Model.Person target, Rock.Model.Person creator )
    {
        var note = new Note
        {
            Id = id,
            Guid = Guid.NewGuid(),
            NoteTypeId = noteType.Id,
            NoteType = noteType,
            EntityId = target.Id,
            Text = "Original text",
            CreatedByPersonAliasId = creator.PrimaryAliasId,
            CreatedByPersonAlias = creator.Aliases.First()
        };

        rockContext.Set<Note>().Add( note );

        return note;
    }

    /// <summary>
    /// Makes <see cref="NoteService.GetAllDescendents(int)"/> report that no
    /// note has any replies. That method uses a raw SQL query, which the mocked
    /// context cannot run, so the query is answered with an empty result.
    /// </summary>
    private static void SetupNoNoteReplies( RockContext rockContext )
    {
        var emptyQueryMock = new Mock<DbSqlQuery<Note>>();
        emptyQueryMock.As<IEnumerable<Note>>()
            .Setup( m => m.GetEnumerator() )
            .Returns( () => new List<Note>().GetEnumerator() );

        Mock.Get( rockContext.Set<Note>() )
            .Setup( m => m.SqlQuery( It.IsAny<string>(), It.IsAny<object[]>() ) )
            .Returns( emptyQueryMock.Object );
    }

    #endregion
}

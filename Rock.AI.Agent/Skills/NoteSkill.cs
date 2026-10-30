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

using System;
using System.ComponentModel;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.Data;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

/// <summary>
/// Provides functionality to create and manage notes within the AI agent.
/// </summary>
[AgentSkillGuid( "216E5428-DE1A-4458-A22C-22812955264A" )]
[EntityTypeGuid( "76DD142A-FB37-4B9E-A1F0-305A5B675B76" )]
[Description( "This skill provides functionality to manage notes." )]
internal sealed partial class NoteSkill : AgentSkillComponent
{
    #region Fields

    /// <summary>
    /// The logger for this instance.
    /// </summary>
    private readonly ILogger _logger;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="NoteSkill"/> class.
    /// </summary>
    /// <param name="logger">Logger for diagnostics and error reporting.</param>
    public NoteSkill( ILogger<NoteSkill> logger )
    {
        _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
    }

    #endregion

    #region Shared Helpers

    /// <summary>
    /// Retrieves the result object for a specific note.
    /// </summary>
    /// <param name="note">The note object.</param>
    /// <param name="rockContext">The Rock context.</param>
    /// <param name="currentPerson">The person the result is for, the entity name is only included if they can view the entity.</param>
    /// <returns>A <see cref="NoteResult"/> object containing the note details.</returns>
    private static NoteResult GetNoteResult( Rock.Model.Note note, RockContext rockContext, Rock.Model.Person currentPerson )
    {
        if ( note == null )
        {
            return null;
        }

        // This may not be populated since we could have just saved, so eager
        // load it if necessary.
        var noteType = NoteTypeCache.Get( note.NoteTypeId );
        string entityName = null;
        string entityTypeName = null;

        // Fetch the entity type and name
        if ( note.EntityId.HasValue && noteType.EntityTypeId.HasValue )
        {
            var entity = new EntityTypeService( rockContext ).GetEntity( noteType.EntityTypeId.Value, note.EntityId.Value );

            /*
                10/16/2026 - MSE

                The note result includes the name of the entity the note is
                attached to. Only include it when the entity is not secured or
                the person can view it, the same rule ValidateNoteTargetEntity
                uses when adding a note.

                Reason: Do not reveal the name of an entity the person cannot view.
            */
            if ( entity != null && ( !( entity is Rock.Security.ISecured securedEntity ) || securedEntity.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) ) )
            {
                entityName = entity?.ToString();
            }
        }

        if ( noteType.EntityTypeId.HasValue )
        {
            var entityType = EntityTypeCache.Get( noteType.EntityTypeId.Value );
            entityTypeName = entityType.FriendlyName;
        }

        return new NoteResult
        {
            Author = PersonResult.NameOnly( note.CreatedByPersonAlias ),
            NoteType = new NoteTypeResult
            {
                EntityType = new KeyNameResult
                {
                    Id = noteType.EntityTypeId,
                    Name = entityTypeName
                },
                Id = noteType.Id,
                Name = noteType.Name
            },
            EntityName = entityName,
            Caption = note.Caption,
            IsAlert = note.IsAlert ?? false,
            Id = note.Id,
            IsPinned = note.IsPinned,
            IsPrivate = note.IsPrivateNote,
            Text = note.Text,
        };
    }

    #endregion
}

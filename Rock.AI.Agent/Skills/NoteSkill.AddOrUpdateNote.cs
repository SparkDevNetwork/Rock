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

using System.ComponentModel;

using Rock.AI.Agent.Annotations;
using Rock.Configuration;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Utility;

namespace Rock.AI.Agent.Skills;

internal sealed partial class NoteSkill
{
    #region Tool(s)

    [Description( "Add a new note to an entity or updates an existing note." )]
    [AgentUsage( "noteTypeIdKey and entityIdKey are required when adding, but can't be changed when updating." )]
    [AgentToolPrerequisite( "Call the LookupNoteTypes function to determine available note types. Select one that matches the note sentiment." )]
    [AgentToolGuid( "FB0E044A-068A-4B47-9990-B2A582F87B3A" )]
    public AgentToolResult AddOrUpdateNote(
        string noteIdKey = null,
        string noteTypeIdKey = null,
        string entityIdKey = null,
        string note = null,
        bool? isAlert = null,
        [Description("If the note contains sensitive information that should only be visible to the creator, set this to true." ) ]
        bool? isPrivateNote = null,
        bool? isPinned = null )
    {
        var rockContext = RockApp.Current.CreateRockContext();
        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );
        var currentPerson = AgentRequestContext.CurrentPerson;

        if ( currentPerson == null )
        {
            return Error( "You must be logged in to add a note." );
        }

        Note noteEntity;

        if ( noteIdKey.IsNotNullOrWhiteSpace() )
        {
            noteEntity = helper.GetRequiredEntity<Note>( noteIdKey );

            // Note.IsAuthorized() only grants EDIT on an existing note to the
            // person that created it or somebody with ADMINISTRATE.
            if ( noteEntity != null && !noteEntity.IsAuthorized( Authorization.EDIT, currentPerson ) )
            {
                helper.AddError( "You are not authorized to edit this note." );
            }

            // The note blocks only allow the creator to change the private
            // flag. Otherwise somebody could lock the creator out of their
            // own note.
            var isPrivateNoteChanging = isPrivateNote.HasValue && isPrivateNote.Value != noteEntity?.IsPrivateNote;
            var isCreator = noteEntity?.CreatedByPersonAlias?.PersonId == currentPerson.Id;

            if ( noteEntity != null && isPrivateNoteChanging && !isCreator )
            {
                helper.AddError( "Only the person that created this note can change if it is private." );
            }
        }
        else
        {
            noteEntity = rockContext.Set<Note>().Create();
            new NoteService( rockContext ).Add( noteEntity );

            var noteType = helper.GetRequiredEntity<Model.NoteType>( noteTypeIdKey );

            if ( noteType != null )
            {
                noteEntity.NoteTypeId = noteType.Id;

                if ( !noteType.UserSelectable )
                {
                    helper.AddError( "Notes of this type cannot be added manually." );
                }

                if ( !noteType.IsAuthorized( Authorization.EDIT, currentPerson ) )
                {
                    helper.AddError( "You are not authorized to add notes of this type." );
                }
            }

            noteEntity.CreatedByPersonAliasId = currentPerson.PrimaryAliasId;
            noteEntity.EntityId = IdHasher.Instance.GetId( entityIdKey );

            if ( !noteEntity.EntityId.HasValue )
            {
                helper.AddError( $"{nameof( entityIdKey )} is required when adding a new note." );
            }
            else if ( noteType != null )
            {
                ValidateNoteTargetEntity( helper, rockContext, noteType.EntityTypeId, noteEntity.EntityId.Value, currentPerson );
            }
        }

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        helper.UpdateProperty( noteEntity, n => n.Text, note );
        helper.UpdateProperty( noteEntity, n => n.IsAlert, isAlert );
        helper.UpdateProperty( noteEntity, n => n.IsPinned, isPinned );
        helper.UpdateProperty( noteEntity, n => n.IsPrivateNote, isPrivateNote );

        helper.SaveChangesIfNoErrors();

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        return Success( GetNoteResult( noteEntity, rockContext ) )
            .WithHistoryContent( new
            {
                noteEntity.IdKey,
            } );
    }

    #endregion

    #region Methods

    /// <summary>
    /// Validates that the entity a new note will be attached to exists and
    /// that the person can view it. The note blocks get this check for free
    /// because they only add notes to the page's context entity, which is
    /// only available when the person has VIEW access to it.
    /// </summary>
    /// <param name="helper">The helper that will receive any errors.</param>
    /// <param name="rockContext">The context to use when loading the entity.</param>
    /// <param name="entityTypeId">The entity type identifier from the note type.</param>
    /// <param name="entityId">The identifier of the entity the note will be attached to.</param>
    /// <param name="currentPerson">The person that is adding the note.</param>
    private static void ValidateNoteTargetEntity( AgentToolHelper helper, RockContext rockContext, int entityTypeId, int entityId, Model.Person currentPerson )
    {
        var entity = new EntityTypeService( rockContext ).GetEntity( entityTypeId, entityId );

        if ( entity == null )
        {
            helper.AddError( "The entity to add the note to was not found. Make sure the entityIdKey matches the entity type of the note type." );
            return;
        }

        if ( entity is ISecured securedEntity && !securedEntity.IsAuthorized( Authorization.VIEW, currentPerson ) )
        {
            helper.AddError( "You are not authorized to add notes to this entity." );
        }
    }

    #endregion
}

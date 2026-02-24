using System.ComponentModel;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.Configuration;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Utility;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class NoteSkill
    {
        #region Tool(s)

        /// <summary>
        /// Adds or updates a note in the system.
        /// </summary>
        /// <param name="noteTypeIdKey">The identifier key of the note type.</param>
        /// <param name="entityIdKey">The identifier key of the entity to associate with the note.</param>
        /// <param name="note">The text of the note.</param>
        /// <param name="isAlert">Indicates whether the note is an alert.</param>
        /// <param name="isPrivateNote">Indicates whether the note is private.</param>
        /// <param name="isPinned">Indicates whether the note is pinned.</param>
        /// <returns>A <see cref="RockToolResult"/> indicating the success or failure of the operation.</returns>
        [Description( "Add a new note to an entity or updates an existing note." )]
        [AgentUsage( "noteTypeIdKey and entityIdKey are required when adding, but can't be changed when updating." )]
        [AgentToolPrerequisite( "Call the LookupNoteTypes function to determine available note types. Select one that matches the note sentiment." )]
        [AgentToolGuid( "FB0E044A-068A-4B47-9990-B2A582F87B3A" )]
        public RockToolResult AddOrUpdateNote(
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
            var currentPerson = AgentRequestContext.RockRequestContext.CurrentPerson;

            if ( currentPerson == null )
            {
                return Error( "You must be logged in to add a note." );
            }

            Note noteEntity;

            if ( noteIdKey.IsNotNullOrWhiteSpace() )
            {
                noteEntity = helper.GetRequiredEntity<Note>( noteIdKey );
            }
            else
            {
                noteEntity = rockContext.Set<Note>().Create();
                new NoteService( rockContext ).Add( noteEntity );

                var noteType = helper.GetRequiredEntity<Model.NoteType>( noteTypeIdKey );

                if ( noteType != null )
                {
                    noteEntity.NoteTypeId = noteType.Id;
                }

                noteEntity.CreatedByPersonAliasId = currentPerson.PrimaryAliasId;
                noteEntity.EntityId = IdHasher.Instance.GetId( entityIdKey );

                if ( !noteEntity.EntityId.HasValue )
                {
                    helper.AddError( $"{nameof( entityIdKey )} is required when adding a new note." );
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
    }
}

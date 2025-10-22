using System;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.Security;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class NoteSkill
    {
        #region Tool(s)

        /// <summary>
        /// Deletes a note by its identifier key.
        /// </summary>
        /// <param name="idKey">The identifier key of the note to delete.</param>
        /// <returns>A <see cref="RockToolResult"/> indicating the success or failure of the operation.</returns>
        [AgentToolGuid( "DC4F7ABA-50F1-4ADD-A1E0-A9DAE8D51D2D" )]
        [AgentGuardrail( "This action will permanently delete the specified note. Ensure that this action is intentional and that you have the correct note identifier before proceeding." )]
        public RockToolResult DeleteNote( string idKey )
        {
            var currentPerson = AgentRequestContext.RockRequestContext.CurrentPerson;
            if ( currentPerson == null )
            {
                return RockToolResult.Error( "You must be logged in to update a note." );
            }

            using ( var rockContext = _rockContextFactory.CreateRockContext() )
            {
                var noteService = new Rock.Model.NoteService( rockContext );
                var existingNote = noteService.Get( idKey, false );

                if ( existingNote == null )
                {
                    return RockToolResult.Error( "Invalid note idKey provided." );
                }

                if ( !existingNote.NoteType.IsAuthorized( Authorization.EDIT, currentPerson ) )
                {
                    return RockToolResult.Error( "You are not authorized to delete this note." );
                }

                noteService.Delete( existingNote );

                try
                {
                    rockContext.SaveChanges();
                }
                catch ( Exception ex )
                {
                    _logger.LogError( ex, "An error occurred while deleting a note." );
                    return RockToolResult.Error( "An error occurred while deleting the note." );
                }

                return RockToolResult.Success()
                    .WithHistoryContent( idKey, idKey );
            }
        }

        #endregion
    }
}

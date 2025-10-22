using Rock.AI.Agent.Classes.Common;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class NoteSkill
    {
        #region Tool(s)

        /// <summary>
        /// Retrieves a specific note by its identifier key.
        /// </summary>
        /// <param name="idKey">The identifier key of the note to retrieve.</param>
        /// <returns>A <see cref="RockToolResult"/> containing the note details or an error message.</returns>
        [AgentToolGuid( "C5690ED4-5CB3-4299-9E75-1D4E6FF7D323" )]
        public RockToolResult GetNote( string idKey )
        {
            using ( var rockContext = _rockContextFactory.CreateRockContext() )
            {
                var noteService = new Rock.Model.NoteService( rockContext );
                var note = noteService.Get( idKey, false );

                if ( note == null )
                {
                    return RockToolResult.Error( "Invalid note idKey provided." );
                }

                var currentPerson = AgentRequestContext.RockRequestContext.CurrentPerson;
                if ( !note.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) )
                {
                    return RockToolResult.Error( "You are not authorized to view this note." );
                }

                return RockToolResult.Success( GetNoteResult( note, rockContext ) )
                    .WithHistoryContent( new
                    {
                        IdKey = note.IdKey,
                        Text = note.Text.Truncate( 200 ),
                    }, note.IdKey );
            }
        }

        #endregion
    }
}

using Rock.AI.Agent.Classes.Common;
using Rock.Configuration;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class NoteSkill
    {
        #region Tool(s)

        /// <summary>
        /// Retrieves a specific note by its identifier key.
        /// </summary>
        /// <param name="noteIdKey">The identifier key of the note to retrieve.</param>
        /// <returns>A <see cref="RockToolResult"/> containing the note details or an error message.</returns>
        [AgentToolGuid( "C5690ED4-5CB3-4299-9E75-1D4E6FF7D323" )]
        public RockToolResult GetNote( string noteIdKey )
        {
            var helper = new AgentToolHelper( AgentRequestContext, _logger );
            var note = helper.GetRequiredEntity<Model.Note>( noteIdKey );

            if ( helper.HasErrors )
            {
                return helper.ErrorResult;
            }

            var currentPerson = AgentRequestContext.RockRequestContext.CurrentPerson;
            if ( !note.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) )
            {
                return Error( "You are not authorized to view this note." );
            }

            return Success( GetNoteResult( note, AgentRequestContext.RockContext ) )
                .WithHistoryContent( new
                {
                    note.IdKey,
                    Text = note.Text.Truncate( 200 ),
                }, note.IdKey );
        }

        #endregion
    }
}

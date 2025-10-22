using System;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Classes.Common;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class NoteSkill
    {
        #region Tool(s)


        /// <summary>
        /// Updates an existing note.
        /// </summary>
        /// <param name="noteIdKey">The identifier key of the note to update.</param>
        /// <param name="note">The updated text of the note (optional).</param>
        /// <param name="noteTypeIdKey">The updated note type identifier key (optional).</param>
        /// <param name="isAlert">The updated alert status (optional).</param>
        /// <param name="isPrivateNote">The updated private status (optional).</param>
        /// <param name="isPinned">The updated pinned status (optional).</param>
        /// <returns>A <see cref="RockToolResult"/> indicating the success or failure of the operation.</returns>
        [AgentToolGuid( "322A8DE0-6F51-4882-9EEB-8A8792607A8B" )]
        public RockToolResult UpdateNote(
            string noteIdKey,
            string note = null,
            string noteTypeIdKey = null,
            bool? isAlert = null,
            bool? isPrivateNote = null,
            bool? isPinned = null )
        {
            var currentPerson = AgentRequestContext.RockRequestContext.CurrentPerson;
            if ( currentPerson == null )
            {
                return RockToolResult.Error( "You must be logged in to update a note." );
            }

            using ( var rockContext = _rockContextFactory.CreateRockContext() )
            {
                var noteService = new Rock.Model.NoteService( rockContext );
                var existingNote = noteService.Get( noteIdKey, false );

                if ( existingNote == null )
                {
                    return RockToolResult.Error( "Invalid note idKey provided." );
                }

                if ( !existingNote.NoteType.IsAuthorized( Authorization.EDIT, currentPerson ) )
                {
                    return RockToolResult.Error( "You are not authorized to edit this note." );
                }

                if ( note.IsNotNullOrWhiteSpace() )
                {
                    existingNote.Text = note;
                }

                if ( noteTypeIdKey.IsNotNullOrWhiteSpace() )
                {
                    var noteType = NoteTypeCache.Get( noteTypeIdKey, false );
                    if ( noteType == null )
                    {
                        return RockToolResult.Error( "Invalid note type." );
                    }
                    if ( !noteType.IsAuthorized( Authorization.EDIT, currentPerson ) )
                    {
                        return RockToolResult.Error( "You are not authorized to change this note to the specified type." );
                    }
                    existingNote.NoteTypeId = noteType.Id;
                }

                if ( isAlert.HasValue )
                {
                    existingNote.IsAlert = isAlert.Value;
                }

                if ( isPrivateNote.HasValue )
                {
                    existingNote.IsPrivateNote = isPrivateNote.Value;
                }

                if ( isPinned.HasValue )
                {
                    existingNote.IsPinned = isPinned.Value;
                }

                try
                {
                    rockContext.SaveChanges();
                }
                catch ( Exception ex )
                {
                    _logger.LogError( ex, "An error occurred while updating a note." );
                    return RockToolResult.Error( "An error occurred while updating the note." );
                }

                var result = GetNoteResult( existingNote, rockContext );

                return RockToolResult.Success( result )
                .WithHistoryContent( noteIdKey, noteIdKey );
            }
        }

        #endregion
    }
}

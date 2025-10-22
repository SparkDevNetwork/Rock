using System;
using System.ComponentModel;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class NoteSkill
    {
        #region Tool(s)

        /// <summary>
        /// Adds a new note to the specified entity of the given note type.
        /// </summary>
        /// <param name="noteTypeIdKey">The identifier key of the note type.</param>
        /// <param name="entityIdKey">The identifier key of the entity to associate with the note.</param>
        /// <param name="note">The text of the note.</param>
        /// <param name="isAlert">Indicates whether the note is an alert.</param>
        /// <param name="isPrivateNote">Indicates whether the note is private.</param>
        /// <param name="isPinned">Indicates whether the note is pinned.</param>
        /// <returns>A <see cref="RockToolResult"/> indicating the success or failure of the operation.</returns>
        [AgentToolGuid( "FB0E044A-068A-4B47-9990-B2A582F87B3A" )]
        [AgentUsage( "Adds a note to the specified entity of the given note type." )]
        [AgentToolPrerequisite( "Call the LookupNoteTypes function to determine available note types. Select one that matches the note sentiment." )]
        public RockToolResult AddNote(
            string noteTypeIdKey,
            string entityIdKey,
            string note,
            bool isAlert = false,
            [Description("If the note contains sensitive information that should only be visible to the creator, set this to true." ) ]
            bool isPrivateNote = false,
            bool isPinned = false )
        {
            var currentPerson = AgentRequestContext.RockRequestContext.CurrentPerson;
            if ( currentPerson == null )
            {
                return RockToolResult.Error( "You must be logged in to add a note." );
            }

            var noteType = NoteTypeCache.Get( noteTypeIdKey, false );
            if ( noteType == null )
            {
                return RockToolResult.Error( "Invalid note type provided." )
                    .WithInstructions( "Call the LookupNoteTypes function to determine available note types. Select one that matches the note sentiment." ); ;
            }

            if ( !noteType.IsAuthorized( Authorization.EDIT, currentPerson ) )
            {
                return RockToolResult.Error( "You are not authorized to add a note of this type." );
            }

            var entityId = IdHasher.Instance.GetId( entityIdKey );
            if ( !entityId.HasValue || entityId <= 0 )
            {
                return RockToolResult.Error( "Invalid entity provided." );
            }

            using var rockContext = _rockContextFactory.CreateRockContext();
            var noteService = new Rock.Model.NoteService( rockContext );

            var newNote = new Rock.Model.Note
            {
                NoteTypeId = noteType.Id,
                EntityId = entityId.Value,
                Text = note,
                IsAlert = isAlert,
                IsPinned = isPinned,
                IsSystem = false,
                IsPrivateNote = isPrivateNote,
                CreatedByPersonAliasId = currentPerson.PrimaryAliasId,
            };

            noteService.Add( newNote );

            try
            {
                rockContext.SaveChanges();
            }
            catch ( Exception ex )
            {
                _logger.LogError( ex, "An error occurred while saving a new note." );
                return RockToolResult.Error( "An error occurred while saving the note." );
            }

            var result = GetNoteResult( newNote, rockContext );

            return RockToolResult.Success( result )
            .WithHistoryContent( newNote.IdKey, newNote.IdKey )
            .WithInstructions( "The note has been added. Display the text, type, and call out if the note was marked as alert or private." );
        }

        #endregion
    }
}

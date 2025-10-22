using System.Collections.Generic;
using System.Linq;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class NoteSkill
    {
        #region Tool(s)

        /// <summary>
        /// Looks up all available note types for the current user.
        /// </summary>
        /// <returns>A <see cref="RockToolResult"/> containing the list of note types or an error message.</returns>
        [AgentToolGuid( "51046397-D246-4296-A1C0-EC6BF0D01FAA" )]
        public RockToolResult LookupNoteTypes()
        {
            var currentPerson = AgentRequestContext.RockRequestContext.CurrentPerson;
            var noteTypes = GetNoteTypes( currentPerson );

            if ( !noteTypes.Any() )
            {
                return RockToolResult.NoData();
            }

            return RockToolResult.Success( noteTypes )
                .WithHistoryKey( "note-types" );
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Retrieves the list of note types available to the current user.
        /// </summary>
        /// <param name="currentPerson">The current person.</param>
        /// <returns>A list of <see cref="NoteTypeResult"/> objects representing the available note types.</returns>
        private static List<NoteTypeResult> GetNoteTypes( Rock.Model.Person currentPerson )
        {
            var noteTypes = NoteTypeCache.All()
                .Where( nt => nt.UserSelectable )
                .Where( a => a.IsAuthorized( Authorization.VIEW, currentPerson ) );

            var noteTypeResults = new List<NoteTypeResult>();

            foreach ( var noteType in noteTypes )
            {
                // Populate entity type information
                var noteTypeResult = new NoteTypeResult
                {
                    Id = noteType.Id,
                    Name = noteType.Name,
                };

                if ( noteType.EntityTypeId.HasValue )
                {
                    var entityType = EntityTypeCache.Get( noteType.EntityTypeId.Value );
                    if ( entityType != null )
                    {
                        noteTypeResult.EntityType = new KeyNameResult
                        {
                            Id = entityType.Id,
                            Name = entityType.FriendlyName,
                        };
                    }
                }

                noteTypeResults.Add( noteTypeResult );
            }

            return noteTypeResults;
        }

        #endregion
    }
}

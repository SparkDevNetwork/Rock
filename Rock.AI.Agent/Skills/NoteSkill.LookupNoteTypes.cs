using System.Collections.Generic;
using System.Linq;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.Model;
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
        public IAgentToolResult LookupNoteTypes()
        {
            var currentPerson = AgentRequestContext.CurrentPerson;
            var noteTypes = GetNoteTypes( currentPerson );

            if ( !noteTypes.Any() )
            {
                return NoData();
            }

            return Success( noteTypes )
                .WithHistoryKey( "note-types" );
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Retrieves the list of note types available to the current user.
        /// </summary>
        /// <param name="currentPerson">The current person.</param>
        /// <returns>A list of <see cref="NoteTypeResult"/> objects representing the available note types.</returns>
        private List<NoteTypeResult> GetNoteTypes( Rock.Model.Person currentPerson )
        {
            return NoteTypeCache.All( AgentRequestContext.RockContext )
                .Where( nt => nt.UserSelectable )
                .Where( nt => nt.IsAuthorized( Authorization.VIEW, currentPerson ) )
                .Select( noteType =>
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

                    return noteTypeResult;
                } )
                .ToList();
        }

        #endregion
    }
}

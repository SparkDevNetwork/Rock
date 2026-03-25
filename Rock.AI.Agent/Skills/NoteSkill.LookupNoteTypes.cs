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

using System.Collections.Generic;
using System.ComponentModel;
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

        [Description( "Provides a list of all note types available for use." )]
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

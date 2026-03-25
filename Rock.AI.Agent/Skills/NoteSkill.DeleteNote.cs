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

using System;
using System.ComponentModel;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Annotations;
using Rock.Configuration;
using Rock.Security;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class NoteSkill
    {
        #region Tool(s)

        [Description( "Deletes a note from the system." )]
        [AgentToolGuid( "DC4F7ABA-50F1-4ADD-A1E0-A9DAE8D51D2D" )]
        [AgentGuardrail( "This action will permanently delete the specified note. Ensure that this action is intentional and that you have the correct note identifier before proceeding." )]
        public IAgentToolResult DeleteNote( string noteIdKey )
        {
            var currentPerson = AgentRequestContext.CurrentPerson;
            if ( currentPerson == null )
            {
                return Error( "You must be logged in to update a note." );
            }

            using var rockContext = RockApp.Current.CreateRockContext();
            var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );
            var noteService = new Rock.Model.NoteService( rockContext );
            var existingNote = helper.GetRequiredEntity<Model.Note>( noteIdKey );

            if ( helper.HasErrors )
            {
                return helper.ErrorResult;
            }

            if ( !existingNote.NoteType.IsAuthorized( Authorization.EDIT, currentPerson ) )
            {
                return Error( "You are not authorized to delete this note." );
            }

            noteService.Delete( existingNote );

            try
            {
                rockContext.SaveChanges();
            }
            catch ( Exception ex )
            {
                _logger.LogError( ex, "An error occurred while deleting a note." );
                return Error( "An error occurred while deleting the note." );
            }

            return Success( "The note has been deleted." );
        }

        #endregion
    }
}

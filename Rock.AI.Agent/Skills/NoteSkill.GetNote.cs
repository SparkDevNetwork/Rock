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

using System.ComponentModel;

using Rock.Model;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class NoteSkill
{
    #region Tool(s)

    [Description( "Gets the details of a single note." )]
    [AgentToolGuid( "C5690ED4-5CB3-4299-9E75-1D4E6FF7D323" )]
    public AgentToolResult GetNote( string noteIdKey )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        var note = helper.GetRequiredEntity<Model.Note>( noteIdKey );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var currentPerson = AgentRequestContext.CurrentPerson;

        /*
            10/16/2026 - MSE

            Also apply the approval and private note rules the note timeline
            uses (same rule as NoteExtensionMethods.AreViewableBy), since
            Note.IsAuthorized does not look at the approval status.

            Reason: Do not return pending or denied notes of other people.
        */
        var isAuthor = currentPerson != null
            && note.CreatedByPersonAlias?.PersonId == currentPerson.Id;
        var requiresApproval = NoteTypeCache.Get( note.NoteTypeId )?.RequiresApprovals ?? false;
        var isViewable = ( isAuthor && note.ApprovalStatus != NoteApprovalStatus.Denied )
            || ( !note.IsPrivateNote && ( !requiresApproval || note.ApprovalStatus == NoteApprovalStatus.Approved ) );

        if ( !isViewable || !note.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) )
        {
            return Error( "You are not authorized to view this note." );
        }

        return Success( GetNoteResult( note, AgentRequestContext.RockContext, currentPerson ) )
            .WithHistoryContent( new
            {
                note.IdKey,
                Text = note.Text.Truncate( 200 ),
            }, note.IdKey );
    }

    #endregion
}

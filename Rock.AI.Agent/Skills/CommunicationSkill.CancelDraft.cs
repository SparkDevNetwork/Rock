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

using Rock.Configuration;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills
{
    internal partial class CommunicationSkill
    {
        #region Tool(s)

        [Description( "Cancels and deletes a draft communication that has not yet been sent." )]
        [AgentToolGuid( "8EC76EA6-83BE-4796-9B91-6B4A34C0C3AD" )]
        public IAgentToolResult CancelDraft( string communicationIdKey )
        {
            if ( communicationIdKey.IsNullOrWhiteSpace() )
            {
                return Error( "CommunicationIdKey is required." );
            }

            using var rockContext = RockApp.Current.CreateRockContext();
            var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );
            var communicationService = new CommunicationService( rockContext );

            var draft = helper.GetRequiredEntity<Model.Communication>( communicationIdKey, checkSecurity: false );

            if ( helper.HasErrors )
            {
                return helper.ErrorResult;
            }

            if ( draft.Status != CommunicationStatus.Transient )
            {
                return Error( "You can not cancel a communication that is not in a transient state." );
            }

            if ( !communicationService.CanDelete( draft, out var errorMessage ) )
            {
                return Error( $"Unable to delete communication: {errorMessage}" );
            }

            communicationService.Delete( draft );

            rockContext.SaveChanges();

            return Success( "The communication has been deleted." );
        }

        #endregion
    }
}

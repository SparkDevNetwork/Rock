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

using Rock.AI.Agent.Annotations;
using Rock.Configuration;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class ReminderSkill
    {
        #region Tool(s)

        [Description( "Deletes a reminder from the system." )]
        [AgentToolGuid( "7E894055-3701-4172-AF81-6D4EC6B78752" )]
        [AgentGuardrail( "This action will permanently delete the specified reminder. Ensure that this action is intentional and that you have the correct identifier before proceeding." )]
        public IAgentToolResult DeleteReminder( string reminderIdKey )
        {
            using var rockContext = RockApp.Current.CreateRockContext();
            var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );
            var reminderService = new ReminderService( rockContext );

            var reminder = helper.GetRequiredEntity<Reminder>( reminderIdKey );

            if ( helper.HasErrors )
            {
                return helper.ErrorResult;
            }

            reminderService.Delete( reminder );

            return Success( "The reminder has succesfully been deleted." );
        }

        #endregion
    }
}

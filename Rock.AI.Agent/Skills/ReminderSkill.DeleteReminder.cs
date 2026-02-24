using System.ComponentModel;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.Configuration;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class ReminderSkill
    {
        #region Tool(s)

        /// <summary>
        /// Deletes a reminder.
        /// </summary>
        /// <param name="reminderIdKey">The key of the reminder to delete.</param>
        /// <returns>A <see cref="RockToolResult"/> indicating the success or failure of the operation.</returns>
        [Description( "Deletes a reminder." )]
        [AgentToolGuid( "7E894055-3701-4172-AF81-6D4EC6B78752" )]
        [AgentGuardrail( "This action will permanently delete the specified reminder. Ensure that this action is intentional and that you have the correct identifier before proceeding." )]
        public RockToolResult DeleteReminder( string reminderIdKey )
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

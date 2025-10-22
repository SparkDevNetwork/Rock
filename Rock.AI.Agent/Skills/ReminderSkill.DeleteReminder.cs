using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
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
        [AgentToolGuid( "7E894055-3701-4172-AF81-6D4EC6B78752" )]
        [AgentGuardrail( "This action will permanently delete the specified reminder. Ensure that this action is intentional and that you have the correct identifier before proceeding." )]
        public RockToolResult DeleteReminder( string reminderIdKey )
        {
            using var rockContext = _rockContextFactory.CreateRockContext();
            var reminderService = new ReminderService( rockContext );
            var reminder = reminderService.Get( reminderIdKey, false );

            if ( reminder == null )
            {
                return RockToolResult.Error( "The specified reminder was not found." );
            }

            reminderService.Delete( reminder );

            return RockToolResult.Success( "The reminder has succesfully been deleted." )
                .WithHistoryContent( reminder.IdKey, reminder.IdKey );
        }

        #endregion
    }
}

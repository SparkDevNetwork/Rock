using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class ReminderSkill
    {
        #region Tool(s)

        /// <summary>
        /// Looks up all active reminder types.
        /// </summary>
        /// <returns>A <see cref="RockToolResult"/> containing the list of reminder types or an error message.</returns>
        [Description( "Looks up reminder types." )]
        [AgentToolGuid( "2452B308-F805-4DE6-83DE-1E340767A4EF" )]
        public RockToolResult LookupReminderTypes()
        {
            var reminderTypeService = new ReminderTypeService( AgentRequestContext.RockContext );

            var reminderTypes = reminderTypeService.Queryable()
                .AsNoTracking()
                .Include( rt => rt.EntityType )
                .Where( rt => rt.IsActive )
                .OrderByDescending( rt => rt.Order )
                .Select( rt => new ReminderTypeResult
                {
                    Id = rt.Id,
                    Name = rt.Name,
                    EntityType = new KeyNameResult
                    {
                        Id = rt.EntityTypeId,
                        Name = rt.EntityType.Name,
                    },
                    Description = rt.Description,
                    NotificationType = rt.NotificationType
                } )
                .ToList();

            if ( !reminderTypes.Any() )
            {
                return RockToolResult.NoData();
            }

            var trimmedForHistory = reminderTypes.Select( rt => new
            {
                rt.IdKey,
                rt.Name,
            } ).ToList();

            return RockToolResult.Success( reminderTypes )
                .WithHistoryContent( trimmedForHistory, "reminder-types" );
        }

        #endregion
    }
}

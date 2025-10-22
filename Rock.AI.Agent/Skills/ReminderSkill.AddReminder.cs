using System;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.Data;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class ReminderSkill
    {
        #region Tool(s)

        /// <summary>
        /// Adds a new reminder.
        /// </summary>
        /// <param name="entityIdKey">The key of the entity to associate with the reminder.</param>
        /// <param name="reminderTypeIdKey">The key of the reminder type.</param>
        /// <param name="note">The note for the reminder.</param>
        /// <param name="date">The date of the reminder.</param>
        /// <param name="repeatEveryXDays">The number of days between repeats (optional).</param>
        /// <param name="repeatAmount">The number of times to repeat (optional).</param>
        /// <returns>A <see cref="RockToolResult"/> indicating the success or failure of the operation.</returns>
        /// <remarks>
        /// The user must be logged in to add a reminder. If <paramref name="repeatEveryXDays"/> is provided without <paramref name="repeatAmount"/>,
        /// the reminder will repeat indefinitely every X days.
        /// </remarks>
        [AgentToolGuid( "AF90FA26-9A98-45EF-877C-6CF0EDE7035E" )]
        [AgentUsage( "Provide a repeatEveryXDays value with no repeatAmount to repeat indefinitely every X days. Provide both to repeat a specific number of times." )]
        [AgentToolPrerequisite( "Call LookupReminderTypes to determine available reminder types." )]
        public RockToolResult AddReminder(
            string entityIdKey,
            string reminderTypeIdKey,
            string note,
            DateTime date,
            int? repeatEveryXDays = null,
            int? repeatAmount = null )
        {
            var currentPerson = AgentRequestContext.RockRequestContext.CurrentPerson;
            if ( currentPerson == null )
            {
                return RockToolResult.Error( "You must be logged in to add a reminder." );
            }

            using var rockContext = _rockContextFactory.CreateRockContext();
            var reminderService = new ReminderService( rockContext );
            var reminderTypeService = new ReminderTypeService( rockContext );
            var personService = new PersonService( rockContext );

            // Validate reminder type
            var reminderType = reminderTypeService.Get( reminderTypeIdKey, false );
            if ( reminderType == null || !reminderType.IsActive )
            {
                return RockToolResult.Error( "The specified reminder type was not found or is inactive." );
            }

            // Resolve target entity (handles Person -> PersonAlias when needed)
            var targetEntityType = EntityTypeCache.Get( reminderType.EntityTypeId ).GetEntityType();
            var personAliasEntityType = EntityTypeCache.Get<PersonAlias>();

            IEntity entity = null;

            if ( personAliasEntityType.Id == reminderType.EntityTypeId )
            {
                var person = personService.Get( entityIdKey, false );

                if ( person?.PrimaryAlias != null )
                {
                    entity = person.PrimaryAlias;
                }
            }
            else
            {
                entity = Rock.Reflection.GetIEntityForEntityType( targetEntityType, entityIdKey, rockContext );
            }

            if ( entity == null )
            {
                return RockToolResult.Error( "The specified entity was not found." );
            }

            if ( !currentPerson.PrimaryAliasId.HasValue )
            {
                return RockToolResult.Error( "The assigned person does not have a primary alias." );
            }

            var reminder = new Reminder
            {
                ReminderTypeId = reminderType.Id,
                EntityId = entity.Id,
                PersonAliasId = currentPerson.PrimaryAliasId.Value,
                IsComplete = false,
                ReminderDate = date.Date,
                Note = note
            };

            if ( repeatEveryXDays.HasValue && repeatEveryXDays.Value > 0 )
            {
                reminder.RenewPeriodDays = repeatEveryXDays.Value;
            }

            if ( repeatAmount.HasValue && repeatAmount.Value > 0 )
            {
                reminder.RenewMaxCount = repeatAmount.Value;
            }

            reminderService.Add( reminder );

            try
            {
                rockContext.SaveChanges();
            }
            catch ( Exception ex )
            {
                _logger.LogError( ex, "Error adding reminder" );
                return RockToolResult.Error( "There was an error adding the reminder." );
            }

            var result = GetReminderResult( reminder, entity.ToStringSafe() );

            return RockToolResult.Success( result )
                .WithHistoryContent( reminder.IdKey, reminder.IdKey )
                .WithInstructions( "The reminder has been added. Display the note, type, and call out if the note was marked to repeat." );
        }

        #endregion
    }
}

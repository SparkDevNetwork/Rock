using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

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
        /// Updates an existing reminder.
        /// </summary>
        /// <param name="reminderIdKey">The key of the reminder to update.</param>
        /// <param name="entityIdKey">The key of the entity to associate with the reminder (optional).</param>
        /// <param name="reminderTypeIdKey">The key of the reminder type (optional).</param>
        /// <param name="note">The updated note for the reminder (optional).</param>
        /// <param name="date">The updated date of the reminder (optional).</param>
        /// <param name="isComplete">The updated completion status of the reminder (optional).</param>
        /// <param name="repeatEveryXDays">The updated number of days between repeats (optional).</param>
        /// <param name="repeatAmount">The updated number of times to repeat (optional).</param>
        /// <returns>A <see cref="RockToolResult"/> indicating the success or failure of the operation.</returns>
        [AgentToolGuid( "F7C1742B-31FB-4E80-9FEE-5B1E45D11A5F" )]
        public RockToolResult UpdateReminder(
            string reminderIdKey,
            string entityIdKey = null,
            string reminderTypeIdKey = null,
            string note = null,
            DateTime? date = null,
            bool? isComplete = null,

            [Description( "Pass 0 to clear any existing repeat." )]
            int? repeatEveryXDays = null,

            [Description( "Pass 0 to clear any existing repeat amount." )]
            int? repeatAmount = null )
        {
            var currentPerson = AgentRequestContext.RockRequestContext.CurrentPerson;
            if ( currentPerson == null )
            {
                return RockToolResult.Error( "You must be logged in to update a reminder." );
            }

            using var rockContext = _rockContextFactory.CreateRockContext();
            var reminderService = new ReminderService( rockContext );
            var reminderTypeService = new ReminderTypeService( rockContext );
            var personService = new PersonService( rockContext );

            var reminder = reminderService.Get( reminderIdKey, false );
            if ( reminder == null )
            {
                return RockToolResult.Error( "The specified reminder was not found." );
            }

            // Determine the effective reminder type (existing or new)
            ReminderType effectiveType = reminder.ReminderType;
            if ( reminderTypeIdKey.IsNotNullOrWhiteSpace() )
            {
                var newType = reminderTypeService.Get( reminderTypeIdKey, false );

                if ( newType == null || !newType.IsActive )
                {
                    return RockToolResult.Error( "The specified reminder type was not found or is inactive." );
                }

                if ( newType.EntityTypeId != effectiveType.EntityTypeId )
                {
                    return RockToolResult.Error( "Changing to a different reminder type entity is not supported. Create a new reminder instead." );
                }

                effectiveType = newType;
            }

            // Determine the effective entity (existing or new), validating compatibility with type
            var entityType = EntityTypeCache.Get( effectiveType.EntityTypeId ).GetEntityType();
            var personAliasEntityType = EntityTypeCache.Get<PersonAlias>();

            object effectiveEntityObj = null;
            int effectiveEntityId;

            if ( entityIdKey.IsNotNullOrWhiteSpace() )
            {
                if ( personAliasEntityType.Id == effectiveType.EntityTypeId )
                {
                    var person = personService.Get( entityIdKey, false );
                    if ( person?.PrimaryAlias != null )
                    {
                        effectiveEntityObj = person.PrimaryAlias;
                    }
                }
                else
                {
                    effectiveEntityObj = Rock.Reflection.GetIEntityForEntityType( entityType, entityIdKey, rockContext );
                }

                if ( effectiveEntityObj == null )
                {
                    return RockToolResult.Error( "The specified entity was not found." );
                }

                effectiveEntityId = ( ( IEntity ) effectiveEntityObj ).Id;
            }
            else
            {
                effectiveEntityId = reminder.EntityId;

                var existingEntityUnderSameType = Rock.Reflection.GetIEntityForEntityType( entityType, reminder.EntityId.ToString(), rockContext );
                if ( existingEntityUnderSameType == null )
                {
                    return RockToolResult.Error( "The existing entity is not compatible with the reminder type." );
                }

                effectiveEntityObj = existingEntityUnderSameType;
            }

            // Apply changes
            if ( reminderTypeIdKey.IsNotNullOrWhiteSpace() )
            {
                reminder.ReminderTypeId = effectiveType.Id;
            }

            if ( entityIdKey.IsNotNullOrWhiteSpace() )
            {
                reminder.EntityId = effectiveEntityId;
            }

            if ( note.IsNotNullOrWhiteSpace() )
            {
                reminder.Note = note;
            }

            if ( date.HasValue )
            {
                reminder.ReminderDate = date.Value.Date;
            }

            if ( isComplete.HasValue )
            {
                reminder.IsComplete = isComplete.Value;
            }

            if ( repeatEveryXDays.HasValue )
            {
                reminder.RenewPeriodDays = repeatEveryXDays.Value > 0 ? repeatEveryXDays.Value : ( int? ) null;
            }

            if ( repeatAmount.HasValue )
            {
                reminder.RenewMaxCount = repeatAmount.Value > 0 ? repeatAmount.Value : ( int? ) null;
            }

            try
            {
                rockContext.SaveChanges();
            }
            catch ( Exception ex )
            {
                _logger.LogError( ex, "Error updating reminder {ReminderIdKey}", reminderIdKey );
                return RockToolResult.Error( "There was an error updating the reminder." );
            }

            // For output, prefer the most up-to-date entity display
            var outputEntityName = ( effectiveEntityObj as IEntity )?.ToStringSafe()
                ?? "Entity";

            return RockToolResult.Success( GetReminderResult( reminder, outputEntityName ) )
                .WithHistoryContent( reminder.IdKey, reminder.IdKey )
                .WithInstructions( "The reminder has been updated. Display the note, type, status, and any repeat details." );
        }

        #endregion
    }
}

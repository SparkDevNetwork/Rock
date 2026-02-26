using System;
using System.ComponentModel;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.Configuration;
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
        /// Adds a new or updates an existing reminder.
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
        [Description( "Adds a new or updates an existing reminder." )]
        [AgentToolGuid( "AF90FA26-9A98-45EF-877C-6CF0EDE7035E" )]
        [AgentUsage( "Provide a repeatEveryXDays value with no repeatAmount to repeat indefinitely every X days. Provide both to repeat a specific number of times." )]
        [AgentToolPrerequisite( "Call LookupReminderTypes to determine available reminder types." )]
        public IAgentToolResult AddOrUpdateReminder(
            string reminderIdKey = null,
            string entityIdKey = null,
            string reminderTypeIdKey = null,
            string note = null,
            DateTime? date = null,
            bool? isComplete = null,
            int? repeatEveryXDays = null,
            int? repeatAmount = null )
        {
            var rockContext = RockApp.Current.CreateRockContext();
            var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );
            var currentPerson = AgentRequestContext.CurrentPerson;

            if ( currentPerson == null )
            {
                return Error( "You must be logged in to add a reminder." );
            }

            Reminder reminder;
            IEntity entity = null;

            if ( reminderIdKey.IsNotNullOrWhiteSpace() )
            {
                reminder = helper.GetRequiredEntity<Reminder>( reminderIdKey );

                if ( reminderTypeIdKey.IsNotNullOrWhiteSpace() )
                {
                    helper.AddError( $"{nameof( reminderTypeIdKey )} can only be specified when adding a reminder." );
                }

                if ( entityIdKey.IsNotNullOrWhiteSpace() )
                {
                    helper.AddError( $"{nameof( entityIdKey )} can only be specified when adding a reminder." );
                }
            }
            else
            {
                reminder = rockContext.Set<Reminder>().Create();
                new ReminderService( rockContext ).Add( reminder );

                reminder.PersonAliasId = currentPerson.PrimaryAliasId.Value;

                var reminderType = helper.GetRequiredEntity<ReminderType>( reminderTypeIdKey );

                if ( reminderType != null )
                {
                    reminder.ReminderTypeId = reminderType.Id;

                    if ( !reminderType.IsActive )
                    {
                        helper.AddError( "The specified reminder type is inactive." );
                    }
                }

                entity = GetReminderEntity( helper, reminderType, entityIdKey, rockContext );

                if ( entity != null )
                {
                    reminder.EntityId = entity.Id;
                }
                else
                {
                    helper.AddError( "The specified entity was not found." );
                }
            }

            if ( helper.HasErrors )
            {
                return helper.ErrorResult;
            }

            helper.UpdateProperty( reminder, r => r.IsComplete, isComplete );
            helper.UpdateProperty( reminder, r => r.ReminderDate, date );
            helper.UpdateProperty( reminder, r => r.Note, note );
            helper.UpdateProperty( reminder, r => r.RenewPeriodDays, repeatEveryXDays );
            helper.UpdateProperty( reminder, r => r.RenewMaxCount, repeatAmount );

            helper.SaveChangesIfNoErrors();

            if ( helper.HasErrors )
            {
                return helper.ErrorResult;
            }

            if ( entity == null )
            {
                // At this point, this means we are doing an update. So we need
                // to load the entity.
                entity = GetReminderEntity( helper, reminder.ReminderType, reminder.EntityId.AsIdKey(), rockContext );
            }

            // Entity could still be null because it might have been deleted.
            var result = GetReminderResult( reminder, entity.ToStringSafe() );

            return Success( result )
                .WithHistoryContent( new
                {
                    result.IdKey,
                    result.EntityName,
                } );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the entity for a reminder by using reflection.
        /// </summary>
        /// <param name="helper">The tool helper.</param>
        /// <param name="reminderType">The reminder type used to identify what kind of entity.</param>
        /// <param name="entityIdKey">The encoded entity identifier.</param>
        /// <param name="rockContext">The database context to read from.</param>
        /// <returns>An instance of <see cref="IEntity"/> or <c>null</c>.</returns>
        private IEntity GetReminderEntity( AgentToolHelper helper, ReminderType reminderType, string entityIdKey, RockContext rockContext )
        {
            var targetEntityType = EntityTypeCache.Get( reminderType.EntityTypeId, rockContext );
            var personAliasEntityType = EntityTypeCache.Get<PersonAlias>( true, rockContext );

            if ( personAliasEntityType.Id == targetEntityType.Id )
            {
                var person = helper.GetRequiredEntity<Model.Person>( entityIdKey, checkSecurity: false );

                return person.PrimaryAlias;
            }
            else
            {
                return Reflection.GetIEntityForEntityType( targetEntityType.GetEntityType(), entityIdKey, rockContext );
            }
        }

        #endregion
    }
}

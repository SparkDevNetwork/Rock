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

using Rock.AI.Agent.Annotations;
using Rock.Configuration;
using Rock.Data;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class ReminderSkill
{
    #region Tool(s)

    [Description( "Adds a new or updates an existing reminder." )]
    [AgentToolGuid( "AF90FA26-9A98-45EF-877C-6CF0EDE7035E" )]
    [AgentUsage( "Provide a repeatEveryXDays value with no repeatAmount to repeat indefinitely every X days. Provide both to repeat a specific number of times." )]
    [AgentToolPrerequisite( "Call LookupReminderTypes to determine available reminder types." )]
    public AgentToolResult AddOrUpdateReminder(
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

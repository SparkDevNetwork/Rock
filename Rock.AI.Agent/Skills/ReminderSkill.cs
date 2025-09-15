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
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.Data;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    /// <summary>
    /// Represents a skill for managing reminders in the Rock application.
    /// </summary>
    /// <remarks>
    /// This skill provides functionality to manage reminders, including adding, updating, deleting, and listing reminders.
    /// It also allows for looking up reminder types.
    /// </remarks>
    [Description( "This skill provides functionality to manage reminders." )]
    [AgentSkillGuid( "A7CDC0C6-DCA6-4E77-9295-245B18556BB1" )]
    [EntityTypeGuid( "41179AB0-702C-435D-94BA-EC6EAE22E39B" )]
    [AgentUsage( "Reminders do not care about time of day. They are always for a specific date." )]
    internal sealed class ReminderSkill : AgentSkillComponent
    {
        #region Fields

        /// <summary>
        /// The logger instance for logging messages.
        /// </summary>
        private readonly ILogger<ReminderSkill> _logger;

        /// <summary>
        /// The factory for creating RockContext instances.
        /// </summary>
        private readonly IRockContextFactory _rockContextFactory;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ReminderSkill"/> class.
        /// </summary>
        /// <param name="rockContextFactory">The factory for creating RockContext instances.</param>
        /// <param name="logger">The logger instance for logging messages.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="rockContextFactory"/> or <paramref name="logger"/> is null.</exception>
        public ReminderSkill( IRockContextFactory rockContextFactory, ILogger<ReminderSkill> logger )
        {
            _rockContextFactory = rockContextFactory ?? throw new ArgumentNullException( nameof( rockContextFactory ) );
            _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
        }

        #endregion

        #region Agent Tools

        /// <summary>
        /// Looks up all active reminder types.
        /// </summary>
        /// <returns>A <see cref="RockToolResult"/> containing the list of reminder types or an error message.</returns>
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
        /// The user must be logged in to add a reminder. If <paramref name="repeatEveryXDays"/> is provided without <paramref name="repeatAmount"/>,        /// the reminder will repeat indefinitely every X days.
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

            using ( var rockContext = _rockContextFactory.CreateRockContext() )
            {
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
        }

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

            using ( var rockContext = _rockContextFactory.CreateRockContext() )
            {
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
        }

        /// <summary>
        /// Lists reminders based on the specified filters.
        /// </summary>
        /// <param name="reminderTypeIdKeys">The keys of the reminder types to filter by (optional).</param>
        /// <param name="entityIdKey">The key of the entity to filter by (optional).</param>
        /// <param name="assignedToPersonIdKey">The key of the person assigned to the reminders (optional).</param>
        /// <param name="startDate">The start date for filtering reminders (optional).</param>
        /// <param name="endDate">The end date for filtering reminders (optional).</param>
        /// <param name="isComplete">The completion status to filter by (optional).</param>
        /// <param name="pageNumber">The page number for pagination (optional).</param>
        /// <returns>A <see cref="RockToolResult"/> containing the list of reminders or an error message.</returns>
        [AgentToolGuid( "AA2EA764-8CB6-48B1-815B-0FDCCDC742DE" )]
        public RockToolResult ListReminders(
            List<string> reminderTypeIdKeys = null,
            string entityIdKey = null,
            string assignedToPersonIdKey = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            bool? isComplete = null,
            int? pageNumber = 1 )
        {
            // Normalize and validate inputs.
            var pgNumber = ( pageNumber ?? 1 ) < 1 ? 1 : ( pageNumber ?? 1 );
            const int pageSize = 25;
            var offset = ( pgNumber - 1 ) * pageSize;
            var take = pageSize + 1; // lookahead for hasMore

            if ( startDate.HasValue && endDate.HasValue && endDate.Value <= startDate.Value )
            {
                return RockToolResult.Error( "The endDate must be after the startDate." );
            }

            using ( var rockContext = _rockContextFactory.CreateRockContext() )
            {
                var reminderService = new ReminderService( rockContext );
                var reminderTypeService = new ReminderTypeService( rockContext );
                var personService = new PersonService( rockContext );
                var personAliasService = new PersonAliasService( rockContext );

                // Base query + eager loads to avoid N+1 for type/entity-type names
                var query = reminderService.Queryable()
                    .AsNoTracking()
                    .Include( r => r.ReminderType.EntityType )
                    .Include( r => r.PersonAlias.Person );

                // Date window (exclusive end makes paging windows clean)
                if ( startDate.HasValue )
                {
                    var s = startDate.Value.Date;
                    query = query.Where( r => r.ReminderDate >= s );
                }

                if ( endDate.HasValue )
                {
                    var e = endDate.Value.Date;
                    query = query.Where( r => r.ReminderDate < e );
                }

                // Filter by 0..N reminder types
                var selectedTypes = new List<ReminderType>();
                var selectedTypeIds = new List<int>();
                var selectedTypeNames = new List<string>();

                if ( reminderTypeIdKeys != null && reminderTypeIdKeys.Any() )
                {
                    foreach ( var key in reminderTypeIdKeys.Distinct() )
                    {
                        var rt = reminderTypeService.Get( key, false );
                        if ( rt == null )
                        {
                            return RockToolResult.Error( $"An invalid reminder type key was provided: {key}" );
                        }

                        selectedTypes.Add( rt );
                        selectedTypeIds.Add( rt.Id );
                        selectedTypeNames.Add( rt.Name );
                    }

                    query = query.Where( r => selectedTypeIds.Contains( r.ReminderTypeId ) );
                }

                // Entity filter rules:
                // - If entityIdKey is supplied AND no types were selected => ambiguous (don’t know entity type)
                // - If entityIdKey is supplied AND selected types map to multiple EntityTypeIds => ambiguous
                // - Otherwise resolve the entity under that single EntityTypeId
                if ( entityIdKey.IsNotNullOrWhiteSpace() )
                {
                    if ( !selectedTypes.Any() )
                    {
                        return RockToolResult.Error( "Filtering by entity requires at least one reminder type." );
                    }

                    var distinctEntityTypeIds = selectedTypes.Select( t => t.EntityTypeId ).Distinct().ToList();
                    if ( distinctEntityTypeIds.Count != 1 )
                    {
                        return RockToolResult.Error( "Filtering by entity requires all selected reminder types to share the same entity type." );
                    }

                    var targetEntityTypeId = distinctEntityTypeIds[0];
                    var personAliasEntityType = EntityTypeCache.Get<PersonAlias>();

                    int entityId;

                    if ( targetEntityTypeId == personAliasEntityType.Id )
                    {
                        // Accept either Person IdKey or PersonAlias IdKey
                        var person = personService.Get( entityIdKey, false );
                        if ( person?.PrimaryAliasId != null )
                        {
                            entityId = person.PrimaryAliasId.Value;
                        }
                        else
                        {
                            var alias = personAliasService.Get( entityIdKey, false );
                            if ( alias == null )
                            {
                                return RockToolResult.Error( "Invalid person or person alias for entity filter." );
                            }
                            entityId = alias.Id;
                        }
                    }
                    else
                    {
                        var entityType = EntityTypeCache.Get( targetEntityTypeId ).GetEntityType();
                        var entity = Rock.Reflection.GetIEntityForEntityType( entityType, entityIdKey, rockContext );
                        if ( entity == null )
                        {
                            return RockToolResult.Error( "Invalid entity for the selected reminder type(s)." );
                        }
                        entityId = entity.Id;
                    }

                    query = query.Where( r => r.EntityId == entityId );
                }

                // Filter by assigned-to person (accept Person IdKey or PersonAlias IdKey)
                if ( assignedToPersonIdKey.IsNotNullOrWhiteSpace() )
                {
                    int? aliasId = null;

                    var person = personService.Get( assignedToPersonIdKey, false );
                    if ( person?.PrimaryAliasId != null )
                    {
                        aliasId = person.PrimaryAliasId;
                    }
                    else
                    {
                        var alias = personAliasService.Get( assignedToPersonIdKey, false );
                        if ( alias != null )
                        {
                            aliasId = alias.Id;
                        }
                    }

                    if ( !aliasId.HasValue )
                    {
                        return RockToolResult.Error( "Invalid assigned-to person." );
                    }

                    query = query.Where( r => r.PersonAliasId == aliasId.Value );
                }

                if ( isComplete.HasValue )
                {
                    query = query.Where( r => r.IsComplete == isComplete.Value );
                }

                // Deterministic order: earliest reminders first, then Id
                var ordered = query
                    .OrderBy( r => r.ReminderDate )
                    .ThenBy( r => r.Id );

                // Page directly
                var pageSlice = ordered.Skip( offset ).Take( take ).ToList();
                if ( pageSlice.Count == 0 )
                {
                    return RockToolResult.NoData();
                }

                var hasMore = pageSlice.Count > pageSize;
                if ( hasMore )
                {
                    pageSlice.RemoveAt( pageSlice.Count - 1 );
                }

                // Helper to get a friendly entity name without blowing up.
                string ResolveEntityName( Rock.Model.Reminder r )
                {
                    try
                    {
                        var etc = r.ReminderType?.EntityTypeId != null
                            ? EntityTypeCache.Get( r.ReminderType.EntityTypeId )
                            : null;

                        var clrType = etc?.GetEntityType();
                        if ( clrType == null )
                        {
                            return "Entity";
                        }

                        var entity = Rock.Reflection.GetIEntityForEntityType( clrType, r.EntityId.ToString(), rockContext );
                        return entity?.ToStringSafe() ?? "Entity";
                    }
                    catch
                    {
                        return "Entity";
                    }
                }

                // Project AFTER paging
                var items = pageSlice
                    .Select( r => GetReminderResult( r, ResolveEntityName( r ) ) )
                    .ToList();

                // Trim for history
                var historyItems = items.Select( r => new
                {
                    IdKey = r.IdKey,
                    Date = r.ReminderDate.ToShortDateString(),
                    Note = ( r.Note ?? string.Empty ).Truncate( 120 )
                } );

                // Metadata for the caller
                var meta = new Dictionary<string, object>
                {
                    { "pageNumber", pgNumber },
                    { "pageSize", pageSize },
                    { "returnedRows", items.Count },
                    { "hasMore", hasMore },
                    { "startDate", startDate?.Date },
                    { "endDate", endDate?.Date },
                    { "filters", new Dictionary<string, object>
                        {
                            { "reminderTypes", selectedTypeNames.Any() ? string.Join( ", ", selectedTypeNames ) : "Any" },
                            { "entityFiltered", entityIdKey.IsNotNullOrWhiteSpace() },
                            { "assignedTo", assignedToPersonIdKey ?? "Any" },
                            { "isComplete", isComplete?.ToString() ?? "Any" }
                        }
                    }
                };

                return RockToolResult.Success( items )
                    .WithHistoryContent( new
                    {
                        Items = historyItems,
                        PageNumber = pgNumber
                    }, "reminders-list" )
                    .WithMetadata( meta );
            }
        }

        /// <summary>
        /// Deletes a reminder.
        /// </summary>
        /// <param name="reminderIdKey">The key of the reminder to delete.</param>
        /// <returns>A <see cref="RockToolResult"/> indicating the success or failure of the operation.</returns>
        [AgentToolGuid( "7E894055-3701-4172-AF81-6D4EC6B78752" )]
        [AgentGuardrail( "This action will permanently delete the specified reminder. Ensure that this action is intentional and that you have the correct identifier before proceeding." )]
        public RockToolResult DeleteReminder( string reminderIdKey )
        {
            using ( var rockContext = _rockContextFactory.CreateRockContext() )
            {
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
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Gets the result object for a reminder.
        /// </summary>
        /// <param name="reminder">The reminder object.</param>
        /// <param name="entityName">The name of the entity associated with the reminder.</param>
        /// <returns>A <see cref="ReminderResult"/> object containing the reminder details.</returns>
        private ReminderResult GetReminderResult( Rock.Model.Reminder reminder, string entityName )
        {
            return new ReminderResult
            {
                Id = reminder.Id,
                EntityName = entityName,
                EntityType = new KeyNameResult
                {
                    Id = reminder.ReminderType.EntityTypeId,
                    Name = reminder.ReminderType.EntityType.Name
                },
                ReminderType = new KeyNameResult
                {
                    Id = reminder.ReminderTypeId,
                    Name = reminder.ReminderType.Name
                },
                IsComplete = reminder.IsComplete,
                Note = reminder.Note,
                ReminderDate = reminder.ReminderDate,
                RenewPeriodDays = reminder.RenewPeriodDays,
                RenewMaxCount = reminder.RenewMaxCount,
                RenewCurrentCount = reminder.RenewCurrentCount
            };
        }

        #endregion
    }
}
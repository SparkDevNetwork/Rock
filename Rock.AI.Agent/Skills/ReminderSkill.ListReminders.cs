using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using Rock.AI.Agent.Classes.Common;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class ReminderSkill
    {
        #region Tool(s)

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

            using var rockContext = _rockContextFactory.CreateRockContext();
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

        #endregion
    }
}

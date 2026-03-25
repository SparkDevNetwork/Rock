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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock.Model;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class ReminderSkill
{
    #region Tool(s)

    [Description( "Lists reminders that match the filters." )]
    [AgentToolGuid( "AA2EA764-8CB6-48B1-815B-0FDCCDC742DE" )]
    public IAgentToolResult ListReminders(
        List<string> reminderTypeIdKeys = null,
        string entityIdKey = null,
        string assignedToPersonIdKey = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        bool? isComplete = null,
        int pageNumber = 1 )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );

        var qry = new ReminderService( AgentRequestContext.RockContext )
            .Queryable();

        qry = helper.WhereOptionalPropertyBetween( qry, r => r.ReminderDate, startDate, endDate );
        qry = helper.WhereOptionalIdKey( qry, r => r.PersonAlias.PersonId, assignedToPersonIdKey );
        qry = helper.WhereOptionalProperty( qry, r => r.IsComplete, isComplete );

        var selectedTypes = reminderTypeIdKeys
            ?.Select( idKey => helper.GetRequiredEntity<ReminderType>( idKey, parameterExpression: nameof( reminderTypeIdKeys ) ) )
            .ToList()
            ?? [];
        var selectedTypeIds = selectedTypes.Select( rt => rt.Id ).ToList();

        if ( selectedTypeIds.Any() )
        {
            qry = qry.Where( r => selectedTypeIds.Contains( r.ReminderTypeId ) );
        }

        // Entity filter rules:
        // - If entityIdKey is supplied AND no types were selected => ambiguous (don’t know entity type)
        // - If entityIdKey is supplied AND selected types map to multiple EntityTypeIds => ambiguous
        // - Otherwise resolve the entity under that single EntityTypeId
        if ( entityIdKey.IsNotNullOrWhiteSpace() )
        {
            if ( !selectedTypes.Any() )
            {
                return Error( "Filtering by entity requires at least one reminder type." );
            }

            var distinctEntityTypeIds = selectedTypes.Select( t => t.EntityTypeId ).Distinct().ToList();
            if ( distinctEntityTypeIds.Count != 1 )
            {
                return Error( "Filtering by entity requires all selected reminder types to share the same entity type." );
            }

            var targetEntityTypeId = distinctEntityTypeIds[0];
            var personAliasEntityType = EntityTypeCache.Get<PersonAlias>();

            if ( targetEntityTypeId == personAliasEntityType.Id )
            {
                var person = helper.GetRequiredEntity<Model.Person>( entityIdKey, checkSecurity: false );

                if ( person != null )
                {
                    qry = qry.Where( r => r.EntityId == person.PrimaryAliasId.Value );
                }
            }
            else
            {
                var entityType = EntityTypeCache.Get( targetEntityTypeId ).GetEntityType();
                var entity = Rock.Reflection.GetIEntityForEntityType( entityType, entityIdKey, AgentRequestContext.RockContext );

                if ( entity != null )
                {
                    qry = qry.Where( r => r.EntityId == entity.Id );
                }
                else
                {
                    helper.AddError( "Invalid entity for the selected reminder type(s)." );
                }
            }
        }

        // Deterministic order: earliest reminders first, then Id
        var orderedQry = qry
            .OrderBy( r => r.ReminderDate )
            .ThenBy( r => r.Id );

        var page = helper.GetPaginatedItems( orderedQry, pageNumber );

        // Helper to get a friendly entity name without blowing up.
        string ResolveEntityName( Reminder r )
        {
            try
            {
                var clrType = r.ReminderType?.EntityTypeId != null
                    ? EntityTypeCache.Get( r.ReminderType.EntityTypeId, AgentRequestContext.RockContext )?.GetEntityType()
                    : null;

                if ( clrType == null )
                {
                    return "Entity";
                }

                var entity = Rock.Reflection.GetIEntityForEntityType( clrType, r.EntityId.ToString(), AgentRequestContext.RockContext );

                return entity?.ToString() ?? "Entity";
            }
            catch
            {
                return "Entity";
            }
        }

        // Project AFTER paging so we can get the entity name.
        var items = page.Items
            .Select( r => GetReminderResult( r, ResolveEntityName( r ) ) )
            .ToList();

        var projectedPage = page.WithItems( items );
        var historyPage = page.WithItems( page.Items.Select( r => new
        {
            r.IdKey,
            Date = r.ReminderDate.ToShortDateString(),
            Note = ( r.Note ?? string.Empty ).Truncate( 120 )
        } ) );

        return Success( projectedPage )
            .WithHistoryContent( historyPage );
    }

    #endregion
}

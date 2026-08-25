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
using System.Data.Entity;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Skills.EventCalendarSkill;
using Rock.Data;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Utility;

namespace Rock.AI.Agent.Skills;

internal sealed partial class EventCalendarSkill
{
    #region Tool(s)

    [Description( "Retrieves a list of event item occurrences." )]
    [AgentPurpose( "Retrieves a list of event item occurrences." )]
    [AgentToolGuid( "ceaf039f-2404-476d-aa3f-c0cd02eeac84" )]
    public AgentToolResult ListEventItemOccurrences(
        string eventCalendarIdKey = null,
        string campusIdKey = null,
        string partialName = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string cursor = null )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        var currentPerson = AgentRequestContext.CurrentPerson;
        var eventCalendarIds = GetConfiguredCalendars().Select( c => c.Id ).ToList();

        var query = new EventItemOccurrenceService( AgentRequestContext.RockContext )
            .Queryable()
            .Include( eio => eio.EventItem )
            .Where( eio => eio.EventItem.IsActive
                && eio.EventItem.EventCalendarItems.Any( eci => eventCalendarIds.Contains( eci.EventCalendarId ) ) );

        query = helper.WhereOptionalIdKey( query, eio => eio.CampusId, campusIdKey );

        if ( startDate.HasValue || endDate.HasValue )
        {
            if ( startDate.HasValue && endDate.HasValue )
            {
                query = query.Where( eio => eio.Schedule.ScheduleDates.Any( sd => sd.StartDateTime >= startDate.Value && sd.StartDateTime <= endDate.Value ) );
            }
            else if ( startDate.HasValue )
            {
                query = query.Where( eio => eio.Schedule.ScheduleDates.Any( sd => sd.StartDateTime >= startDate.Value ) );
            }
            else if ( endDate.HasValue )
            {
                query = query.Where( eio => eio.Schedule.ScheduleDates.Any( sd => sd.StartDateTime <= endDate.Value ) );
            }
        }
        else
        {
            helper.AddError( $"At least one of {nameof( startDate )} or {nameof( endDate )} must be provided." );
        }

        query = helper.WhereRequiredPropertyBetween( query, eio => eio.NextStartDateTime, startDate, endDate );

        if ( eventCalendarIdKey.IsNotNullOrWhiteSpace() )
        {
            var eventCalendarId = IdHasher.Instance.GetId( eventCalendarIdKey );

            if ( eventCalendarId.HasValue )
            {
                query = query.Where( eio => eio.EventItem.EventCalendarItems.Any( eci => eci.EventCalendarId == eventCalendarId.Value ) );
            }
            else
            {
                helper.AddError( $"The value of {nameof( eventCalendarIdKey )} is not valid." );
            }
        }

        if ( partialName.IsNotNullOrWhiteSpace() )
        {
            query = query.Where( eio => eio.EventItem.Name.Contains( partialName ) );
        }

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var paginator = new CursorPaginator<EventItemOccurrence>( currentPerson, qry => qry
            .OrderByDescending( cr => cr.NextStartDateTime.HasValue )
            .ThenBy( cr => cr.Id ) );

        var cursorPage = helper.GetCursorPaginatedItems( query, paginator, cursor );

        cursorPage.Items.LoadAttributes( AgentRequestContext.RockContext );

        var resultPage = cursorPage.WithItems( cursorPage.Items
            .Select( eio => new EventItemOccurrenceResult
            {
                Id = eio.Id,
                EventItem = new EventItemResult
                {
                    Id = eio.EventItemId,
                    Name = eio.EventItem.Name,
                },
                NextStartDateTime = eio.NextStartDateTime,
                AttributeValues = eio.GetGridAttributeValueResults( AgentRequestContext ).ToList(),
            } )
            .ToList() );

        var historyResultPage = cursorPage.WithItems( resultPage.Items
            .Select( eio => new EventItemOccurrenceResult
            {
                Id = eio.Id,
                EventItem = eio.EventItem,
                NextStartDateTime = eio.NextStartDateTime,
            } )
            .ToList() );

        return helper.GetPaginatedResult( resultPage, historyResultPage )
            .WithInstructions( "Event items can exist on multiple calendars, so there may be duplicates." );
    }

    #endregion
}

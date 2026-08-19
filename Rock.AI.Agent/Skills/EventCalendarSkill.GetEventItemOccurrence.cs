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

using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.AI.Agent.Classes.Skills.EventCalendarSkill;
using Rock.AI.Agent.Classes.Skills.EventRegistrationSkill;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class EventCalendarSkill
{
    #region Tool(s)

    [Description( "Retrieves the details of a event item occurrence." )]
    [AgentPurpose( "Retrieves the details of a event item occurrence." )]
    [AgentToolGuid( "688fa8b0-100d-4eee-b10c-a77a743fe8da" )]
    public AgentToolResult GetEventItemOccurrence( string eventItemOccurrenceIdKey )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        var calendarIds = GetConfiguredCalendars().Select( c => c.Id ).ToList();

        var eventItemOccurrence = helper.GetRequiredEntity<EventItemOccurrence>( eventItemOccurrenceIdKey, checkSecurity: true );

        if ( eventItemOccurrence != null && !calendarIds.Any( calendarId => eventItemOccurrence.EventItem.EventCalendarItems.Any( eci => eci.EventCalendarId == calendarId ) ) )
        {
            helper.AddError( "That even item occurrence is not available." );
        }

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        // Don't use the navigation property to get the linkages. It will not
        // eager load the related entities which would cause additional queries
        // to be executed for each linkage. Instead, query the linkages
        // directly and include the related entities.
        var linkages = new EventItemOccurrenceGroupMapService( AgentRequestContext.RockContext ).Queryable()
            .Include( a => a.Group )
            .Include( a => a.RegistrationInstance )
            .Where( l => l.EventItemOccurrenceId == eventItemOccurrence.Id )
            .ToList();

        // Get the linkage result objects. We intentionally do not check
        // security on the related entities. All we are exposing is the name
        // and id of the related entities. The user will still need to have
        // security to view the details of those entities.
        var linkageResults = linkages
            .Select( ri => new RegistrationInstanceLinkageResult
            {
                Group = ri.Group != null
                    ? new KeyNameResult( ri.Group.Id, ri.Group.Name )
                    : null,
                RegistrationInstance = ri.RegistrationInstance != null
                    ? new KeyNameResult( ri.RegistrationInstance.Id, ri.RegistrationInstance.Name )
                    : null,
            } )
            .ToList();

        var result = new EventItemOccurrenceResult
        {
            Id = eventItemOccurrence.Id,
            Guid = eventItemOccurrence.Guid,
            EventItem = new EventItemResult
            {
                Id = eventItemOccurrence.EventItemId,
                Name = eventItemOccurrence.EventItem.Name,
                ApprovedByPerson = PersonResult.NameOnly( eventItemOccurrence.EventItem.ApprovedByPersonAlias ),
                Audiences = eventItemOccurrence.EventItem.EventItemAudiences.Select( a => new KeyNameResult( a.DefinedValue.Id, a.DefinedValue.Value ) ).ToList(),
                Calendars = eventItemOccurrence.EventItem.EventCalendarItems.Select( a => new KeyNameResult( a.EventCalendar.Id, a.EventCalendar.Name ) ).ToList(),
                IsApproved = eventItemOccurrence.EventItem.IsApproved,
                Summary = eventItemOccurrence.EventItem.Summary,
            },
            Campus = eventItemOccurrence.CampusId.HasValue
                ? new CampusResult
                {
                    Id = eventItemOccurrence.Campus.Id,
                    Name = eventItemOccurrence.Campus.Name,
                }
                : null,
            ContactEmail = eventItemOccurrence.ContactEmail,
            ContactPerson = PersonResult.NameOnly( eventItemOccurrence.ContactPersonAlias ),
            ContactPhoneNumber = eventItemOccurrence.ContactPhone,
            LocationDescription = eventItemOccurrence.Location,
            NextStartDateTime = eventItemOccurrence.NextStartDateTime,
            ScheduleDescription = eventItemOccurrence.Schedule.Description,
            Linkages = linkageResults,
            AttributeValues = eventItemOccurrence.GetAttributeValueResults( AgentRequestContext ).ToList(),
        };

        return Success( result );
    }

    #endregion
}

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

using Rock.AI.Agent.Classes.Entity;
using Rock.Core.Geography.Classes;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class SystemUtilitySkill
{
    #region Tool(s)

    [Description( "Provides information on the campuses." )]
    [AgentToolGuid( "1FDDC83F-2911-5E86-4219-DB4A5F10BD42" )]
    public IAgentToolResult LookupCampuses()
    {
        var campusResults = RockCache.GetOrAddExisting( "rock.core.aiagent.lookupcampuses", null, () =>
        {
            return LoadCampuses();
        }, TimeSpan.FromMinutes( 3 ) ) as List<CampusResult>;

        return Success( campusResults );
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Loads the campuses from the cache and converts them to a list of CampusResult objects.
    /// </summary>
    /// <returns></returns>
    private List<CampusResult> LoadCampuses()
    {
        var isInternal = this.AgentRequestContext.AudienceType == Enums.AI.Agent.AudienceType.Internal;

        var campuses = CampusCache.All()
            .Where( c => c.IsActive == true )
            .Select( c => new CampusResult
            {
                Id = c.Id,
                Name = c.Name,
                IsActive = c.IsActive ?? false,
                Abbreviation = c.ShortCode,
                CampusType = c.CampusTypeValue?.Value ?? string.Empty,
                CampusStatus = c.CampusStatusValue?.Value ?? string.Empty,
                CampusTeamGroupId = c.TeamGroupId,
                PhoneNumber = c.PhoneNumber,
                Url = c.Url,
                Location = new LocationResult
                {
                    Street1 = c.Location.Street1,
                    Street2 = c.Location.Street2,
                    City = c.Location.City,
                    State = c.Location.State,
                    PostalCode = c.Location.PostalCode,
                    Country = c.Location.Country,
                    GeographyPoint = ( c.Location.Latitude.HasValue && c.Location.Longitude.HasValue ) ? new GeographyPoint( c.Location.Latitude.Value, c.Location.Latitude.Value ) : null
                },
                CampusSchedules = c.CampusSchedules
                    .Where( s => s.Schedule.IsActive )
                    .Select( s => new CampusScheduleResult
                    {
                        ScheduleName = s.Schedule.FriendlyScheduleText,
                        ScheduleType = s.ScheduleTypeValue.Value,
                    } )
                    .ToList(),
                AttributeValues = c.AttributeValues
                    .Where( a => a.Value.AttributeIsPublic == true || isInternal == true )
                    .Select( a => new AttributeValueResult
                    {
                        AttributeId = a.Value.AttributeId,
                        Key = a.Key,
                        Value = a.Value.ValueFormatted
                    } )
                    .ToList()
            } )
            .ToList();

        // Add the team information
        foreach ( var campus in campuses.Where( c => c.CampusTeamGroupId != null ) )
        {
            if ( campus.CampusTeamGroupId == null )
            {
                continue;
            }

            // TODO: Filter roles by public once the GroupRole property is merged. 

            // Get team members
            campus.CampusTeamMembers = new GroupMemberService( AgentRequestContext.RockContext ).Queryable()
                .Where( m => m.GroupId == campus.CampusTeamGroupId )
                .Select( m => new CampusTeamMemberResult
                {
                    Role = m.GroupRole.Name,
                    TeamMember = new PersonResult
                    {
                        Id = m.PersonId,
                        NickName = m.Person.NickName,
                        LastName = m.Person.LastName,
                        Email = m.Person.Email
                    }
                } )
                .ToList();
        }

        return campuses;
    }

    #endregion
}

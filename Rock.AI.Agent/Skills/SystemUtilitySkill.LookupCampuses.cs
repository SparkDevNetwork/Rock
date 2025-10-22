using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.Core.Geography.Classes;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class SystemUtilitySkill
    {
        #region Tool(s)

        [Description( "Provides information on the campuses." )]
        [AgentToolGuid( "1FDDC83F-2911-5E86-4219-DB4A5F10BD42" )]
        public RockToolResult LookupCampuses()
        {
            var campusResults = RockCache.GetOrAddExisting( "rock.core.aiagent.lookupcampuses", null, () =>
            {
                return LoadCampuses();
            }, TimeSpan.FromMinutes( 3 ) ) as List<CampusResult>;

            return RockToolResult.Success( campusResults );
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
                    Attributes = c.AttributeValues
                        .Where( a => a.Value.AttributeIsPublic == true || isInternal == true )
                        .Select( a => new AttributeResult
                        {
                            Id = a.Value.AttributeId,
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
}

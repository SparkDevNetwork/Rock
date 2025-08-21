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
using System.Linq;

using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.SystemUtilitySkill;
using Rock.AI.Agent.Utilities;
using Rock.Core.Geography.Classes;
using Rock.Data;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    [Description(
        "🎯 Purpose:\r\n" +
        "Provides common, non-domain-specific helper functions that can be used across multiple skills.\r\n" +
        "These include utilities for working with dates, times, and simple data conversions."
    )]
    [UserDescription( "Provides common, non-domain-specific helper functions that can be used across multiple skills." )]
    [AgentSkillGuid( "3406D2DC-6718-45A2-99D3-1DAA32BF2EFD" )]
    [EntityTypeGuid( "35CD02D0-1FF7-4256-B495-FBBFBC9A2C9C" )]
    internal sealed class SystemUtilitySkill : AgentSkillComponent
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemUtilitySkill"/> class.
        /// </summary>
        /// <param name="rockContext">Rock data context used for database access.</param>
        /// <param name="logger">Logger for diagnostics and error reporting.</param>
        public SystemUtilitySkill( RockContext rockContext, ILogger<SystemUtilitySkill> logger )
        {
            _rockContext = rockContext ?? throw new ArgumentNullException( nameof( rockContext ) );
            _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
        }

        #endregion

        #region Fields

        private readonly RockContext _rockContext;
        private readonly ILogger<SystemUtilitySkill> _logger;

        #endregion

        #region Native Functions

        /// <summary>
        /// Determines a date range from a natural language string.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        [KernelFunction( "DetermineDateRange" )]
        [Description( "🎯 Purpose:\r\n1. Determines a date range from a natural language string.\r\n\r\n\U0001f9ed Usage Guidance:\r\n1. This function is useful in cases where you need to determine a start date and end date for another\r\n   function, such as when you want to filter results by a specific date range." )]
        [UserDescription( "Determines a date range from a natural language string." )]
        [AgentFunctionGuid( "87756092-9D52-448E-82EE-556A780DF7CF" )]
        public RockToolResult DetermineDateRange(
            [Description( "A natural language string, such as 'last week', 'tomorrow', or 'March 1st to March 10th'.")]
            string query )
        {

            var dateRange = DateTimeRecognitionHelper.RecognizeDateRange( query, DateTime.Now );

            if ( dateRange == null )
            {
                return RockToolResult.Error( "A date range could not be determined from the query." )
                    .WithInstructions( $"Today is {DateTime.Now}. Using today as a reference date, infer the date range yourself." );
            }

            return RockToolResult.Success( dateRange );
        }

        [KernelFunction( "LookupCampuses" )]
        [AgentFunctionGuid( "1FDDC83F-2911-5E86-4219-DB4A5F10BD42" )]
        [UserDescription( "Provides information on the campuses." )]
        public RockToolResult LookupCampuses()
        {
            var campusResults = RockCache.GetOrAddExisting( "rock.core.aiagent.lookupcampuses", null, () =>
            {
                return LoadCampuses();
            }, TimeSpan.FromMinutes( 3 ) ) as List<CampusResult>;

            return RockToolResult.Success( campusResults );
        }

        #endregion

        #region Private Methods

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
                    CampusIdKey = c.IdKey,
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
            foreach( var campus in campuses.Where( c => c.CampusTeamGroupId != null) )
            {
                if (campus.CampusTeamGroupId == null )
                {
                    continue;
                }

                // TODO: Filter roles by public once the GroupRole property is merged. 

                // Get team members
                campus.CampusTeamMembers = new GroupMemberService( _rockContext ).Queryable()
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

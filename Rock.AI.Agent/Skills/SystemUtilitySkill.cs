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
using System.ComponentModel;
using System.Linq;

using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.SystemUtilitySkill;
using Rock.AI.Agent.Utilities;
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
        /// <param name="logger"></param>
        public SystemUtilitySkill( ILogger<SystemUtilitySkill> logger )
        {
        }

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
        public RockFunctionResult DetermineDateRange(
            [Description( "A natural language string, such as 'last week', 'tomorrow', or 'March 1st to March 10th'.")]
            string query )
        {

            var dateRange = DateTimeRecognitionHelper.RecognizeDateRange( query, DateTime.Now );

            if ( dateRange == null )
            {
                return RockFunctionResult.Error( "A date range could not be determined from the query.", instructions: $"Today is {DateTime.Now}. Using today as a reference date, infer the date range yourself." );
            }

            return RockFunctionResult.Success( dateRange );
        }

        // TODO: Finish out this function.
        public RockFunctionResult LookupCampuses()
        {
            // TODOs:  
            // 1. We need to add CampusSchedules to the CampusCache so that we can use it here.  

            var campusResults = CampusCache.All()
                .Select( c => new CampusResult
                {
                    CampusKey = c.IdKey,
                    IsActive = c.IsActive ?? false,
                    Abbreviation = c.ShortCode,
                    CampusType = c.CampusTypeValue?.Value ?? string.Empty,
                    CampusTypeKey = c.CampusTypeValue.IdKey,
                    CampusStatus = c.CampusStatusValue?.Value ?? string.Empty,
                    CampusStatusKey = c.CampusStatusValue.IdKey,
                    Location = c.Location,
                    ServiceTimes = c.ServiceTimes
                } )
                .ToList();

            return RockFunctionResult.Success( campusResults );
        }

        #endregion
    }
}

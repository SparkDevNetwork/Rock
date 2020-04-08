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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.OData;
using Rock.Badge.Component;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web.Cache;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// StreakTypeAchievementTypes REST API
    /// </summary>
    public partial class StreakTypeAchievementTypesController
    {
        /// <summary>
        /// Gets the progress for the person.
        /// </summary>
        /// <param name="personId">The person identifier. The current person is used if this is omitted.</param>
        /// <param name="includeOnlyEligible">Include only progress statements for achievement types that have no unmet prerequisites</param>
        /// <returns></returns>
        /// <exception cref="HttpResponseException"></exception>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/StreakTypeAchievementTypes/Progress" )]
        public virtual List<ProgressStatement> GetProgressForPerson( [FromUri]int personId = default, [FromUri]bool includeOnlyEligible = default )
        {
            var rockContext = Service.Context as RockContext;

            // If not specified, use the current person id
            if ( personId == default )
            {
                personId = GetPerson( rockContext )?.Id ?? default;

                if ( personId == default )
                {
                    var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.BadRequest, "The personId for the current user did not resolve" );
                    throw new HttpResponseException( errorResponse );
                }
            }

            var achievementTypeService = Service as StreakTypeAchievementTypeService;
            var progressStatements = achievementTypeService.GetProgressStatements( personId );

            if ( includeOnlyEligible )
            {
                progressStatements = progressStatements.Where( ps => !ps.UnmetPrerequisites.Any() ).ToList();
            }

            return progressStatements;
        }
    }
}
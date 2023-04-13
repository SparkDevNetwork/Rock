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

using System.Collections.Generic;
using System.Linq;

using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class InteractiveExperienceService
    {
        /// <summary>
        /// Gets the current occurrence identifiers for the experience. If any
        /// occurrences don't exist that should exist then they will be created.
        /// </summary>
        /// <param name="interactiveExperienceId">The interactive experience identifier.</param>
        /// <returns>A list of integer identifiers for the current occurrences of the experience.</returns>
        internal static IEnumerable<int> GetOrCreateAllCurrentOccurrenceIds( int interactiveExperienceId )
        {
            var experienceSchedules = InteractiveExperienceScheduleCache.All()
                .Where( ies => ies.InteractiveExperienceId == interactiveExperienceId
                    && ies.InteractiveExperience.IsActive )
                .ToList();

            var occurrenceIds = new List<int>();

            foreach ( var experienceSchedule in experienceSchedules )
            {
                if ( experienceSchedule.CampusIds.Any() )
                {
                    foreach ( var campusId in experienceSchedule.CampusIds )
                    {
                        var occurrenceId = InteractiveExperienceOccurrenceService.GetOrCreateCurrentOccurrenceId( experienceSchedule.Id, campusId );

                        if ( occurrenceId.HasValue )
                        {
                            occurrenceIds.Add( occurrenceId.Value );
                        }
                    }
                }
                else
                {
                    var occurrenceId = InteractiveExperienceOccurrenceService.GetOrCreateCurrentOccurrenceId( experienceSchedule.Id, null );

                    if ( occurrenceId.HasValue )
                    {
                        occurrenceIds.Add( occurrenceId.Value );
                    }
                }
            }

            return occurrenceIds;
        }
    }
}

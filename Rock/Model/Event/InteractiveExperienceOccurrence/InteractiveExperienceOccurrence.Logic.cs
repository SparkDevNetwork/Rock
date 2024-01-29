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
using System.Linq;

using Rock.Enums.Event;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class InteractiveExperienceOccurrence
    {
        #region Properties

        /// <summary>
        /// Gets a value indicating whether this occurrence is active right now.
        /// </summary>
        /// <value><c>true</c> if this instance occurrence is active right now; otherwise, <c>false</c>.</value>
        public bool IsOccurrenceActive => WasOccurrenceActive( RockDateTime.Now );

        #endregion

        #region Methods

        /// <summary>
        /// Determines if the occurrence schedule was active at the specified date and time.
        /// </summary>
        /// <param name="dateTime">The date time to be checked.</param>
        /// <returns><c>true</c> if the occurrence schedule was active, <c>false</c> otherwise.</returns>
        public bool WasOccurrenceActive( DateTime dateTime )
        {
            // If the schedule was not active at the specified date and time then
            // this occurrence was not active either.
            if ( !InteractiveExperienceSchedule.Schedule.WasCheckInActive( dateTime ) )
            {
                return false;
            }

            // If the specified date and time was before the schedule ended that
            // day then this occurrence was active.
            return dateTime < OccurrenceDateTime.AddMinutes( InteractiveExperienceSchedule.Schedule.DurationInMinutes );
        }

        /// <summary>
        /// Gets the campus identifier to use for a person that is joining this
        /// live experience occurrence. This is determined based on various
        /// factors and configuration settings on the experience.
        /// </summary>
        /// <param name="person">The person that is joining the experience.</param>
        /// <param name="latitude">The known latitude of the person.</param>
        /// <param name="longitude">The known longitude of the person.</param>
        /// <returns>The campus identifier or <c>null</c> if one could not be determined.</returns>
        internal int? GetIndivualCampusId( Person person, double? latitude, double? longitude )
        {
            var experience = InteractiveExperienceScheduleCache.Get( InteractiveExperienceScheduleId )?.InteractiveExperience;

            if ( experience == null )
            {
                return null;
            }

            var useGeoFence = experience.ExperienceSettings.CampusBehavior == InteractiveExperienceCampusBehavior.FilterSchedulesByCampusGeofences
                || experience.ExperienceSettings.CampusBehavior == InteractiveExperienceCampusBehavior.DetermineCampusFromGeofence;

            if ( useGeoFence )
            {
                int? campusId = null;

                // If they provided latitude and longitude then use that to
                // try and find which campus they are at.
                if ( latitude.HasValue && longitude.HasValue )
                {
                    campusId = CampusCache.All( false )
                        .Where( c => c.ContainsGeoPoint( latitude.Value, longitude.Value ) )
                        .OrderBy( c => c.Order )
                        .FirstOrDefault()
                        ?.Id;
                }

                // When we filter by campus we don't use the default campus id
                // even though it might be in the data.
                if ( experience.ExperienceSettings.CampusBehavior == InteractiveExperienceCampusBehavior.FilterSchedulesByCampusGeofences )
                {
                    return campusId;
                }

                return campusId ?? experience.ExperienceSettings.DefaultCampusId;
            }
            else if ( experience.ExperienceSettings.CampusBehavior == InteractiveExperienceCampusBehavior.UseIndividualsCampus )
            {
                return person?.PrimaryCampusId ?? experience.ExperienceSettings.DefaultCampusId;
            }

            return null;
        }

        #endregion
    }
}

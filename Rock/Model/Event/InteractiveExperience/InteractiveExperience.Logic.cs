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
using System.Data.Entity;
using System.Linq;

using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class InteractiveExperience
    {
        #region ICacheable

        /// <inheritdoc/>
        public IEntityCache GetCacheObject()
        {
            return InteractiveExperienceCache.Get( Id );
        }

        /// <inheritdoc/>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            InteractiveExperienceCache.UpdateCachedEntity( Id, entityState );
        }

        #endregion ICacheable

        #region Methods

        /// <summary>
        /// Determines whether this experience has any active occurrences on
        /// the specified date and time. This check is based on the schedules
        /// and not the existence of any <see cref="InteractiveExperienceOccurrence"/> records.
        /// </summary>
        /// <param name="dateTime">The date and time to use when checking if any occurrences were active.</param>
        /// <returns><c>true</c> if this experience has any active occurrences; otherwise, <c>false</c>.</returns>
        public bool HasActiveOccurrencesForDate( DateTime dateTime )
        {
            if ( !IsActive )
            {
                return false;
            }

            // If any of the schedules are currently active right now then
            // we should return true.
            return InteractiveExperienceSchedules.Any( ies => ies.Schedule.WasScheduleActive( dateTime ) );
        }

        #endregion
    }
}

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

using System.Data.Entity;
using System.Linq;

namespace Rock.Model
{
    public partial class GroupSyncService
    {
    }

    #region Extension Methods
    /// <summary>
    /// Extension methods used to query GroupSync.
    /// </summary>
    public static class GroupSyncServiceExtensions
    {
        /// <summary>
        /// GroupSync objects that are not archived.
        /// </summary>
        /// <param name="groupSync">The group synchronize.</param>
        /// <returns></returns>
        public static IQueryable<GroupSync> AreNotArchived( this IQueryable<GroupSync> groupSync )
        {
            return groupSync.Where( x => !x.Group.IsArchived );
        }

        /// <summary>
        /// GroupSync objects that are active.
        /// </summary>
        /// <param name="groupSync">The group synchronize.</param>
        /// <returns></returns>
        public static IQueryable<GroupSync> AreActive( this IQueryable<GroupSync> groupSync )
        {
            return groupSync.Where( x => x.Group.IsActive );
        }

        /// <summary>
        /// GroupSync objects that need to be synced.
        /// </summary>
        /// <param name="groupSync">The group synchronize.</param>
        /// <returns></returns>
        public static IQueryable<GroupSync> NeedToBeSynced( this IQueryable<GroupSync> groupSync )
        {
            return groupSync.Where( x => x.ScheduleIntervalMinutes == null
                                    || x.LastRefreshDateTime == null
                                    || DbFunctions.AddMinutes( x.LastRefreshDateTime, x.ScheduleIntervalMinutes ) < RockDateTime.Now );
        }
    }
    #endregion
}

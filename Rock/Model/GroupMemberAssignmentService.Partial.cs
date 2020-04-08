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
using System.Text;
using System.Threading.Tasks;

namespace Rock.Model
{
    /// <summary>
    /// Data access service class for <see cref="Rock.Model.GroupMemberAssignment"/> entity objects.
    /// </summary>
    public partial class GroupMemberAssignmentService
    {
        /// <summary>
        /// Adds the or update.
        /// </summary>
        /// <param name="groupMemberId">The group member identifier.</param>
        /// <param name="scheduleId">The schedule identifier.</param>
        /// <param name="locationId">The location identifier.</param>
        /// <returns></returns>
        public GroupMemberAssignment AddOrUpdate( int groupMemberId, int scheduleId, int? locationId = null )
        {
            GroupMemberAssignment groupMemberAssignment = null;

            // Check to see if a GroupMemberAssignment exists for the GroupMember and Scheudle
            groupMemberAssignment = Queryable()
                .Where( x => x.GroupMemberId == groupMemberId )
                .Where( x => x.ScheduleId == scheduleId )
                .FirstOrDefault();

            // If not then create one
            if ( groupMemberAssignment == null )
            {
                groupMemberAssignment = new GroupMemberAssignment
                {
                    GroupMemberId = groupMemberId,
                    ScheduleId = scheduleId,
                    LocationId = locationId
                };

                this.Add( groupMemberAssignment );
            }
            else
            {
                groupMemberAssignment.LocationId = locationId;
            }
            
            return groupMemberAssignment;
        }

        /// <summary>
        /// Deletes the by group member and schedule.
        /// </summary>
        /// <param name="groupMemberId">The group member identifier.</param>
        /// <param name="scheduleId">The schedule identifier.</param>
        public void DeleteByGroupMemberAndSchedule( int groupMemberId, int scheduleId )
        {
            var groupMemberAssignment = Queryable()
                .Where( x => x.GroupMemberId == groupMemberId )
                .Where( x => x.ScheduleId == scheduleId )
                .FirstOrDefault();

            if ( groupMemberAssignment != null )
            {
                this.Delete( groupMemberAssignment );
            }
        }
    }
}

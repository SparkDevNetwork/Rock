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
    /// <seealso cref="Rock.Data.Service{Rock.Model.GroupMemberAssignment}" />
    public partial class GroupMemberAssignmentService
    {
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

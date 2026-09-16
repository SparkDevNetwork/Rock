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
using System.Data.Entity;
using System.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Blocks.Engagement.SignUp
{
    /// <summary>
    /// Collection of helper methods shared by the sign-up blocks for working with sign-up
    /// projects and their opportunities (the group location schedules of a project group).
    /// </summary>
    internal static class SignUpOpportunityHelper
    {
        /// <summary>
        /// Gets the identifier of the sign-up group type, or <c>0</c> when it does not exist.
        /// </summary>
        internal static int SignUpGroupTypeId => GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_SIGNUP_GROUP.AsGuid() )?.Id ?? 0;

        /// <summary>
        /// Determines whether groups of the specified group type are sign-up projects, which is
        /// the case when the group type is the sign-up group type or directly inherits from it.
        /// </summary>
        /// <param name="groupTypeId">The identifier of the group type to check.</param>
        /// <returns><c>true</c> if groups of the group type are sign-up projects; otherwise, <c>false</c>.</returns>
        internal static bool IsSignUpGroupType( int groupTypeId )
        {
            var signUpGroupTypeId = SignUpGroupTypeId;

            if ( signUpGroupTypeId == 0 )
            {
                return false;
            }

            if ( groupTypeId == signUpGroupTypeId )
            {
                return true;
            }

            return GroupTypeCache.Get( groupTypeId )?.InheritedGroupTypeId == signUpGroupTypeId;
        }

        /// <summary>
        /// Deletes a sign-up opportunity. An opportunity is a group location schedule with
        /// possible group member assignments (and therefore, group members), so the following
        /// are deleted:
        /// <list type="number">
        /// <item>The group member assignments.</item>
        /// <item>The group members (when no other assignments remain for a given group member).</item>
        /// <item>The group location schedule and group location schedule config.</item>
        /// <item>The group location (when no more schedules are tied to it).</item>
        /// <item>The schedule (when non-named and nothing else is using it).</item>
        /// </list>
        /// Callers are responsible for verifying that the group is a sign-up project and that the
        /// current person is authorized to delete its opportunities.
        /// </summary>
        /// <param name="rockContext">The context used to load and delete the entities.</param>
        /// <param name="groupId">The identifier of the sign-up project group.</param>
        /// <param name="locationId">The identifier of the opportunity's location.</param>
        /// <param name="scheduleId">The identifier of the opportunity's schedule.</param>
        internal static void DeleteOpportunity( RockContext rockContext, int groupId, int locationId, int scheduleId )
        {
            var groupMemberAssignmentService = new GroupMemberAssignmentService( rockContext );
            var groupMemberAssignments = groupMemberAssignmentService
                .Queryable()
                .Include( gma => gma.GroupMember )
                .Where( gma =>
                    gma.GroupMember.GroupId == groupId
                    && gma.LocationId == locationId
                    && gma.ScheduleId == scheduleId )
                .ToList();

            if ( groupMemberAssignments.Any() )
            {
                // Set the group members aside so we can try to delete them next.
                var groupMembers = groupMemberAssignments
                    .Select( gma => gma.GroupMember )
                    .ToList();

                // A group member assignment is a pretty low-level entity with no child
                // entities, so a bulk delete is safe. We'll need to check CanDelete() for
                // each assignment (and abandon the bulk delete approach) if this changes in
                // the future.
                groupMemberAssignmentService.DeleteRange( groupMemberAssignments );

                // Determine which of these group members have assignments for other
                // opportunities; those group member records must remain.
                var groupMemberIds = groupMembers.Select( gm => gm.Id ).ToList();
                var deletedAssignmentIds = groupMemberAssignments.Select( gma => gma.Id ).ToList();
                var groupMemberIdsWithRemainingAssignments = new HashSet<int>(
                    groupMemberAssignmentService
                        .Queryable()
                        .AsNoTracking()
                        .Where( gma =>
                            groupMemberIds.Contains( gma.GroupMemberId )
                            && !deletedAssignmentIds.Contains( gma.Id ) )
                        .Select( gma => gma.GroupMemberId )
                        .Distinct()
                        .ToList()
                );

                var groupTypeId = new GroupService( rockContext ).GetSelect( groupId, g => g.GroupTypeId );
                var groupTypeCache = GroupTypeCache.Get( groupTypeId );
                var groupMemberService = new GroupMemberService( rockContext );

                foreach ( var groupMember in groupMembers.Where( gm => !groupMemberIdsWithRemainingAssignments.Contains( gm.Id ) ) )
                {
                    if ( groupTypeCache?.EnableGroupHistory != true && !groupMemberService.CanDelete( groupMember, out _ ) )
                    {
                        // The attendee (group member assignment) record itself will be
                        // deleted, but we cannot delete the underlying group member record.
                        continue;
                    }

                    // Delete these one-by-one, as the individual delete call will
                    // dynamically archive if necessary (whereas the bulk delete calls will
                    // not).
                    groupMemberService.Delete( groupMember );
                }
            }

            // Now go get the group location, schedule and group location schedule config.
            var groupLocationService = new GroupLocationService( rockContext );
            var groupLocation = groupLocationService
                .Queryable()
                .Include( gl => gl.Schedules )
                .Include( gl => gl.GroupLocationScheduleConfigs )
                .FirstOrDefault( gl => gl.GroupId == groupId && gl.LocationId == locationId );

            var schedulesToDelete = new List<Schedule>();

            if ( groupLocation != null )
            {
                // These are deleted last, since the schedule's identifier is referenced in
                // the group location schedule and group location schedule config tables.
                schedulesToDelete = groupLocation.Schedules
                    .Where( s => s.Id == scheduleId )
                    .ToList();

                foreach ( var schedule in schedulesToDelete )
                {
                    groupLocation.Schedules.Remove( schedule );
                }

                foreach ( var config in groupLocation.GroupLocationScheduleConfigs.Where( c => c.ScheduleId == scheduleId ).ToList() )
                {
                    groupLocation.GroupLocationScheduleConfigs.Remove( config );
                }

                // If this group location has no more schedules, delete it. Any lingering
                // group location schedule config records that somehow weren't deleted yet
                // will be removed by a cascade delete here.
                if ( !groupLocation.Schedules.Any() )
                {
                    groupLocationService.Delete( groupLocation );
                }
            }

            rockContext.WrapTransaction( () =>
            {
                // Initial save to release FK constraints tied to referenced entities we'll
                // be deleting.
                rockContext.SaveChanges();

                var scheduleService = new ScheduleService( rockContext );
                foreach ( var schedule in schedulesToDelete )
                {
                    // Remove the schedule if custom (non-named) and nothing else is using it.
                    if ( schedule.ScheduleType != ScheduleType.Named && scheduleService.CanDelete( schedule, out _ ) )
                    {
                        scheduleService.Delete( schedule );
                    }
                }

                // We cannot safely remove referenced locations (even non-named ones):
                //  1) because of the way locations are reused/shared across entities (the
                //     location picker control auto-searches/matches and saves locations).
                //  2) because of the cascade deletes many of the referencing entities have
                //     on their LocationId FK constraints (we might accidentally delete a
                //     lot of unintended stuff).

                // Follow-up save for deleted referenced entities.
                rockContext.SaveChanges();
            } );
        }
    }
}

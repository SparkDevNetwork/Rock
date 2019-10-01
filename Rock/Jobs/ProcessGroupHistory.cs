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

using Quartz;

using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// Processes Group History
    /// </summary>
    /// <seealso cref="Quartz.IJob" />
    [DisallowConcurrentExecution]
    public class ProcessGroupHistory : IJob
    {

        #region Constructor

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public ProcessGroupHistory()
        {
        }

        #endregion Constructor

        #region fields

        /// <summary>
        /// The job status messages
        /// </summary>
        private List<string> _jobStatusMessages = null;

        #endregion

        #region Methods

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            _jobStatusMessages = new List<string>();

            UpdateGroupHistorical( context );

            UpdateGroupMemberHistorical( context );

            UpdateGroupLocationHistorical( context );

            if ( _jobStatusMessages.Any() )
            {
                context.UpdateLastStatusMessage( _jobStatusMessages.AsDelimited( ", ", " and " ) );
            }
            else
            {
                context.UpdateLastStatusMessage( "No group changes detected" );
            }
        }

        /// <summary>
        /// Updates Group Historical for any groups that have data group history enabled
        /// </summary>
        /// <param name="context">The context.</param>
        public void UpdateGroupHistorical( IJobExecutionContext context )
        {
            var rockContext = new RockContext();
            var groupHistoricalService = new GroupHistoricalService( rockContext );
            var groupService = new GroupService( rockContext );

            var groupsWithHistoryEnabledQuery = groupService.Queryable().Where( a => a.GroupType.EnableGroupHistory == true ).AsNoTracking();
            var groupHistoricalsCurrentQuery = groupHistoricalService.Queryable().Where( a => a.CurrentRowIndicator == true ).AsNoTracking();

            // Mark GroupHistorical Rows as History ( CurrentRowIndicator = false, etc ) if any of the tracked field values change
            var groupHistoricalNoLongerCurrent = groupHistoricalsCurrentQuery.Join(
                    groupsWithHistoryEnabledQuery,
                    gh => gh.GroupId,
                    g => g.Id, ( gh, g ) => new
                    {
                        Group = g,
                        GroupHistorical = gh
                    } )
                    .Where( a =>
                        a.Group.Name != a.GroupHistorical.GroupName
                        || a.Group.GroupType.Name != a.GroupHistorical.GroupTypeName
                        || a.Group.CampusId != a.GroupHistorical.CampusId
                        || a.Group.ParentGroupId != a.GroupHistorical.ParentGroupId
                        || a.Group.ScheduleId != a.GroupHistorical.ScheduleId
                        || ( a.Group.ScheduleId.HasValue && ( a.Group.Schedule.ModifiedDateTime != a.GroupHistorical.ScheduleModifiedDateTime ) )
                        || a.Group.Description != a.GroupHistorical.Description
                        || a.Group.StatusValueId != a.GroupHistorical.StatusValueId
                        || a.Group.IsArchived != a.GroupHistorical.IsArchived
                        || a.Group.ArchivedDateTime != a.GroupHistorical.ArchivedDateTime
                        || a.Group.ArchivedByPersonAliasId != a.GroupHistorical.ArchivedByPersonAliasId
                        || a.Group.IsActive != a.GroupHistorical.IsActive
                        || a.Group.InactiveDateTime != a.GroupHistorical.InactiveDateTime
                    ).Select( a => a.GroupHistorical ).AsNoTracking();

            var effectiveExpireDateTime = RockDateTime.Now;

            int groupsLoggedToHistory = 0;
            int groupsSaveToHistoryCurrent = 0;

            if ( groupHistoricalNoLongerCurrent.Any() )
            {
                groupsLoggedToHistory = rockContext.BulkUpdate( groupHistoricalNoLongerCurrent, gh => new GroupHistorical
                {
                    CurrentRowIndicator = false,
                    ExpireDateTime = effectiveExpireDateTime
                } );
            }

            // Insert Groups (that have GroupType.EnableGroupHistory) that don't have a CurrentRowIndicator row yet ( or don't have a CurrentRowIndicator because it was stamped with CurrentRowIndicator=false )
            var groupsToAddToHistoricalCurrentsQuery = groupsWithHistoryEnabledQuery.Where( g => !groupHistoricalsCurrentQuery.Any( gh => gh.GroupId == g.Id ) ).AsNoTracking();

            if ( groupsToAddToHistoricalCurrentsQuery.Any() )
            {
                List<GroupHistorical> groupHistoricalCurrentsToInsert = groupsToAddToHistoricalCurrentsQuery
                    .Include( a => a.GroupType )
                    .Include( a => a.Schedule )
                    .ToList()
                    .Select( g => GroupHistorical.CreateCurrentRowFromGroup( g, effectiveExpireDateTime ) ).ToList();

                groupsSaveToHistoryCurrent = groupHistoricalCurrentsToInsert.Count();

                rockContext.BulkInsert( groupHistoricalCurrentsToInsert );
            }

            if ( groupsLoggedToHistory > 0 )
            {
                _jobStatusMessages.Add( $"Logged {groupsLoggedToHistory} {"group history snapshot".PluralizeIf( groupsLoggedToHistory != 0 )}" );
            }

            if ( groupsSaveToHistoryCurrent > 0 )
            {
                int newGroupsAddedToHistory = groupsSaveToHistoryCurrent - groupsLoggedToHistory;
                if ( newGroupsAddedToHistory > 0 )
                {
                    _jobStatusMessages.Add( $"Added {newGroupsAddedToHistory} new {"group history snapshot".PluralizeIf( newGroupsAddedToHistory != 0 )}" );
                }
            }
        }

        /// <summary>
        /// Updates GroupMemberHistorical for any group members in groups that have data group history enabled
        /// </summary>
        /// <param name="context">The context.</param>
        public void UpdateGroupMemberHistorical( IJobExecutionContext context )
        {
            var rockContext = new RockContext();
            var groupMemberHistoricalService = new GroupMemberHistoricalService( rockContext );
            var groupMemberService = new GroupMemberService( rockContext );

            var groupMembersWithHistoryEnabledQuery = groupMemberService.AsNoFilter().Where( a => a.Group.GroupType.EnableGroupHistory == true ).AsNoTracking();
            var groupMemberHistoricalsCurrentQuery = groupMemberHistoricalService.Queryable().Where( a => a.CurrentRowIndicator == true ).AsNoTracking();

            // Mark GroupMemberHistorical Rows as History ( CurrentRowIndicator = false, etc ) if any of the tracked field values change
            var groupMemberHistoricalNoLongerCurrent = groupMemberHistoricalsCurrentQuery.Join(
                    groupMembersWithHistoryEnabledQuery,
                    gmh => gmh.GroupMemberId,
                    gm => gm.Id, ( gmh, gm ) => new
                    {
                        GroupMember = gm,
                        GroupMemberHistorical = gmh
                    } )
                    .Where( a =>
                        a.GroupMember.GroupRoleId != a.GroupMemberHistorical.GroupRoleId
                        || a.GroupMember.GroupId != a.GroupMemberHistorical.GroupId
                        || a.GroupMember.GroupRole.Name != a.GroupMemberHistorical.GroupRoleName
                        || a.GroupMember.GroupRole.IsLeader != a.GroupMemberHistorical.IsLeader
                        || a.GroupMember.GroupMemberStatus != a.GroupMemberHistorical.GroupMemberStatus
                        || a.GroupMember.IsArchived != a.GroupMemberHistorical.IsArchived
                        || a.GroupMember.ArchivedDateTime != a.GroupMemberHistorical.ArchivedDateTime
                        || a.GroupMember.ArchivedByPersonAliasId != a.GroupMemberHistorical.ArchivedByPersonAliasId
                        || a.GroupMember.InactiveDateTime != a.GroupMemberHistorical.InactiveDateTime
                    ).Select( a => a.GroupMemberHistorical ).AsNoTracking();

            var effectiveExpireDateTime = RockDateTime.Now;

            int groupMembersLoggedToHistory = 0;
            int groupMembersSaveToHistoryCurrent = 0;

            if ( groupMemberHistoricalNoLongerCurrent.Any() )
            {
                groupMembersLoggedToHistory = rockContext.BulkUpdate( groupMemberHistoricalNoLongerCurrent, gmh => new GroupMemberHistorical
                {
                    CurrentRowIndicator = false,
                    ExpireDateTime = effectiveExpireDateTime
                } );
            }

            // Insert Group Members (that have a group with GroupType.EnableGroupHistory) that don't have a CurrentRowIndicator row yet ( or don't have a CurrentRowIndicator because it was stamped with CurrentRowIndicator=false )
            var groupMembersToAddToHistoricalCurrentsQuery = groupMembersWithHistoryEnabledQuery.Where( gm => !groupMemberHistoricalsCurrentQuery.Any( gmh => gmh.GroupMemberId == gm.Id ) );

            if ( groupMembersToAddToHistoricalCurrentsQuery.Any() )
            {
                List<GroupMemberHistorical> groupMemberHistoricalCurrentsToInsert = groupMembersToAddToHistoricalCurrentsQuery
                    .Include( a => a.GroupRole )
                    .ToList()
                    .Select( gm => GroupMemberHistorical.CreateCurrentRowFromGroupMember( gm, effectiveExpireDateTime ) ).ToList();

                groupMembersSaveToHistoryCurrent = groupMemberHistoricalCurrentsToInsert.Count();

                rockContext.BulkInsert( groupMemberHistoricalCurrentsToInsert );
            }

            if ( groupMembersLoggedToHistory > 0 )
            {
                _jobStatusMessages.Add( $"Logged {groupMembersLoggedToHistory} {"group member history snapshot".PluralizeIf( groupMembersLoggedToHistory != 0 )}" );
            }

            if ( groupMembersSaveToHistoryCurrent > 0 )
            {
                int newGroupMembersAddedToHistory = groupMembersSaveToHistoryCurrent - groupMembersLoggedToHistory;
                if ( newGroupMembersAddedToHistory > 0 )
                {
                    _jobStatusMessages.Add( $"Added {newGroupMembersAddedToHistory} new {"group member history snapshot".PluralizeIf( newGroupMembersAddedToHistory != 0 )}" );
                }
            }
        }

        /// <summary>
        /// Updates GroupLocationHistorical for any group locations in groups that have data group history enabled
        /// </summary>
        /// <param name="context">The context.</param>
        public void UpdateGroupLocationHistorical( IJobExecutionContext context )
        {
            var rockContext = new RockContext();
            var groupLocationHistoricalService = new GroupLocationHistoricalService( rockContext );
            var groupLocationService = new GroupLocationService( rockContext );

            var groupLocationsWithHistoryEnabledQuery = groupLocationService.Queryable().Where( a => a.Group.GroupType.EnableGroupHistory == true ).AsNoTracking();
            var groupLocationsHistoricalCurrentQuery = groupLocationHistoricalService.Queryable().Where( a => a.CurrentRowIndicator == true ).AsNoTracking();

            // Mark GroupLocationHistorical Rows as History ( CurrentRowIndicator = false, etc ) if any of the tracked field values change
            var groupLocationHistoricalNoLongerCurrentQuery = groupLocationsHistoricalCurrentQuery.Join(
                    groupLocationsWithHistoryEnabledQuery,
                    glh => glh.GroupLocationId,
                    gl => gl.Id, ( glh, gl ) => new
                    {
                        GroupLocation = gl,
                        GroupLocationHistorical = glh
                    } )
                    .Where( a =>
                        a.GroupLocation.GroupId != a.GroupLocationHistorical.GroupId
                        || ( a.GroupLocation.GroupLocationTypeValueId != a.GroupLocation.GroupLocationTypeValueId )
                        || ( a.GroupLocation.GroupLocationTypeValueId.HasValue && a.GroupLocation.GroupLocationTypeValue.Value != a.GroupLocationHistorical.GroupLocationTypeName )
                        || a.GroupLocation.LocationId != a.GroupLocationHistorical.LocationId
                        || a.GroupLocation.Location.ModifiedDateTime != a.GroupLocationHistorical.LocationModifiedDateTime
                        || ( a.GroupLocation.Schedules.Select( s => new { ScheduleId = s.Id, s.ModifiedDateTime } ).Except( a.GroupLocationHistorical.GroupLocationHistoricalSchedules.Select( hs => new { hs.ScheduleId, ModifiedDateTime = hs.ScheduleModifiedDateTime } ) ) ).Any()
                    );

            var effectiveExpireDateTime = RockDateTime.Now;
            int groupLocationsLoggedToHistory = 0;
            int groupLocationsSaveToHistoryCurrent = 0;

            if ( groupLocationHistoricalNoLongerCurrentQuery.Any() )
            {
                var groupLocationHistoricalNoLongerCurrent = groupLocationHistoricalNoLongerCurrentQuery.Select( a => a.GroupLocationHistorical ).AsNoTracking();

                groupLocationsLoggedToHistory = rockContext.BulkUpdate( groupLocationHistoricalNoLongerCurrent, glh => new GroupLocationHistorical
                {
                    CurrentRowIndicator = false,
                    ExpireDateTime = effectiveExpireDateTime
                } );
            }

            // Insert Group Locations (that have a group with GroupType.EnableGroupHistory) that don't have a CurrentRowIndicator row yet ( or don't have a CurrentRowIndicator because it was stamped with CurrentRowIndicator=false )
            var groupLocationsToAddToHistoricalCurrentsQuery = groupLocationsWithHistoryEnabledQuery.Where( gl => !groupLocationsHistoricalCurrentQuery.Any( glh => glh.GroupLocationId == gl.Id ) );

            if ( groupLocationsToAddToHistoricalCurrentsQuery.Any() )
            {
                List<GroupLocationHistorical> groupLocationHistoricalCurrentsToInsert = groupLocationsToAddToHistoricalCurrentsQuery
                    .Include( a => a.GroupLocationTypeValue )
                    .Include( a => a.Location ).ToList()
                    .Select( gl => GroupLocationHistorical.CreateCurrentRowFromGroupLocation( gl, effectiveExpireDateTime ) ).ToList();

                groupLocationsSaveToHistoryCurrent = groupLocationHistoricalCurrentsToInsert.Count();

                // get the current max GroupLocatiionHistorical.Id to help narrow down which ones were inserted
                int groupLocationHistoricalStartId = groupLocationHistoricalService.Queryable().Max( a => ( int? ) a.Id ) ?? 0;

                rockContext.BulkInsert( groupLocationHistoricalCurrentsToInsert );

                // since we used BulkInsert, we'll need to go back and get the Ids and the associated GroupLocation's Schedules for the GroupLocationHistorical records that we just inserted
                var insertedGroupLocationHistoricalIdsWithSchedules = groupLocationHistoricalService.Queryable()
                    .Where( a => a.Id > groupLocationHistoricalStartId && a.GroupLocation.Schedules.Any() ).ToList()
                    .Select( a => new { GroupLocationHistoricalId = a.Id, a.GroupLocation.Schedules } );

                List<GroupLocationHistoricalSchedule> groupLocationHistoricalScheduleCurrentsToInsert = new List<GroupLocationHistoricalSchedule>();
                foreach ( var insertedGroupLocationHistoricalIdWithSchedules in insertedGroupLocationHistoricalIdsWithSchedules )
                {
                    foreach ( Schedule schedule in insertedGroupLocationHistoricalIdWithSchedules.Schedules )
                    {
                        groupLocationHistoricalScheduleCurrentsToInsert.Add( new GroupLocationHistoricalSchedule
                        {
                            GroupLocationHistoricalId = insertedGroupLocationHistoricalIdWithSchedules.GroupLocationHistoricalId,
                            ScheduleId = schedule.Id,
                            ScheduleName = schedule.ToString(),
                            ScheduleModifiedDateTime = schedule.ModifiedDateTime
                        } );
                    }
                }

                if ( groupLocationHistoricalScheduleCurrentsToInsert.Any() )
                {
                    rockContext.BulkInsert( groupLocationHistoricalScheduleCurrentsToInsert );
                }
            }

            if ( groupLocationsLoggedToHistory > 0 )
            {
                _jobStatusMessages.Add( $"Logged {groupLocationsLoggedToHistory} {"group location history snapshot".PluralizeIf( groupLocationsLoggedToHistory != 0 )}" );
            }

            if ( groupLocationsSaveToHistoryCurrent > 0 )
            {
                int newGroupLocationsAddedToHistory = groupLocationsSaveToHistoryCurrent - groupLocationsLoggedToHistory;
                if ( newGroupLocationsAddedToHistory > 0 )
                {
                    _jobStatusMessages.Add( $"Added {newGroupLocationsAddedToHistory} new {"group location history snapshot".PluralizeIf( newGroupLocationsAddedToHistory != 0 )}" );
                }
            }
        }

        #endregion
    }
}

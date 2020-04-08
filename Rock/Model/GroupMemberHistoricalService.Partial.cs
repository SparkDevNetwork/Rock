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

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// The data access/service class for the <see cref="Rock.Model.GroupMemberHistorical"/> entity. This inherits from the Service class
    /// </summary>
    public partial class GroupMemberHistoricalService
    {
        /// <summary>
        /// Gets the group historical summary for the specified Person during the specified timeframe
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="startDateTime">The start date time.</param>
        /// <param name="stopDateTime">The stop date time.</param>
        /// <param name="groupTypeIds">The group type ids.</param>
        /// <returns></returns>
        public List<GroupHistoricalSummary> GetGroupHistoricalSummary( int personId, DateTime? startDateTime, DateTime? stopDateTime, List<int> groupTypeIds )
        {
            var rockContext = this.Context as RockContext;

            var personGroupMemberIdQuery = new GroupMemberService( rockContext ).AsNoFilter().Where( a => a.PersonId == personId ).Select( a => a.Id );

            // get all GroupMemberHistorical records for the Person
            var groupMemberHistoricalQuery = this.AsNoFilter().Where( a => personGroupMemberIdQuery.Contains( a.GroupMemberId ) );

            if ( startDateTime.HasValue )
            {
                groupMemberHistoricalQuery = groupMemberHistoricalQuery.Where( a => a.EffectiveDateTime >= startDateTime.Value );
            }

            if ( stopDateTime.HasValue )
            {
                groupMemberHistoricalQuery = groupMemberHistoricalQuery.Where( a => a.EffectiveDateTime < stopDateTime.Value );
            }

            if ( groupTypeIds?.Any() == true)
            {
                groupMemberHistoricalQuery = groupMemberHistoricalQuery.Where( a => groupTypeIds.Contains( a.Group.GroupTypeId ) );
            }
             
            return this.GetGroupHistoricalSummary( groupMemberHistoricalQuery );
        }

        /// <summary>
        /// Gets the group historical summary of the groupMemberHistoricalQuery
        /// </summary>
        /// <param name="groupMemberHistoricalQuery">The group member historical query.</param>
        /// <returns></returns>
        public List<GroupHistoricalSummary> GetGroupHistoricalSummary( IQueryable<GroupMemberHistorical> groupMemberHistoricalQuery )
        {
            var rockContext = this.Context as RockContext;

            // only fetch the groupMemberHistorical records where they were not archived and not Inactive (only fetch Active or Pending)
            // Also, IsArchive is redundant since they know the StartStop times of when they were in the group
            var groupMemberHistoricalByGroupList = groupMemberHistoricalQuery
                .Where( a => a.IsArchived == false && a.GroupMemberStatus != GroupMemberStatus.Inactive )
                .Select(a => new { a.Group, GroupMemberHistorical = a, a.EffectiveDateTime, GroupMemberDateTimeAdded = a.GroupMember.DateTimeAdded } )
                .GroupBy( a => a.Group ).ToList();

            var groupNameHistoryLookup = new GroupHistoricalService( rockContext ).Queryable()
                .Where( a => groupMemberHistoricalQuery.Any( x => x.GroupId == a.GroupId ) )
                .Select( a => new GroupNameHistory
                {
                    GroupId = a.GroupId,
                    EffectiveDateTime = a.EffectiveDateTime,
                    ExpireDateTime = a.ExpireDateTime,
                    GroupName = a.GroupName,
                } ).GroupBy( a => a.GroupId ).ToDictionary( k => k.Key, v => v.ToList() );

            var groupMemberHistoricalStartStopHistory = groupMemberHistoricalByGroupList
                .Select( a =>
                {
                    var startStopHistoryList = a.OrderBy( x => x.EffectiveDateTime )
                            .Select( x => new GroupMemberHistoricalSummary( a.Key, x.GroupMemberHistorical, x.GroupMemberDateTimeAdded ) ).ToList();

                    var groupNameHistory = groupNameHistoryLookup.GetValueOrNull( a.Key.Id ).ToList();

                    // let the first HistoryRecord know that its the first record so it can be smart about reporting StartDateTime
                    var firstStopStopHistory = startStopHistoryList.FirstOrDefault();
                    if ( firstStopStopHistory != null )
                    {
                        firstStopStopHistory.IsFirstTimeInGroup = true;
                    }

                    startStopHistoryList.ForEach( x => x._groupNameHistory = groupNameHistory );

                    var groupHistorySummary = new GroupHistoricalSummary
                    {
                        Group = a.Key,
                        StartStopHistory = startStopHistoryList
                    };

                    return groupHistorySummary;

                } ).OrderBy( a => a.Group.Id );

            return groupMemberHistoricalStartStopHistory.ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        public class GroupHistoricalSummary
        {
            /// <summary>
            /// Gets or sets the group.
            /// </summary>
            /// <value>
            /// The group.
            /// </value>
            [Newtonsoft.Json.JsonIgnore]
            public virtual Group Group { get; set; }

            /// <summary>
            /// Gets the group identifier.
            /// </summary>
            /// <value>
            /// The group identifier.
            /// </value>
            public int GroupId => Group.Id;

            /// <summary>
            /// Gets the group type identifier.
            /// </summary>
            /// <value>
            /// The group type identifier.
            /// </value>
            public int GroupTypeId => Group.GroupTypeId;

            /// <summary>
            /// The color used to visually distinguish groups on lists.
            /// </summary>
            /// <value>
            /// The color of the group type.
            /// </value>
            public string GroupTypeColor => GroupTypeCache.Get( GroupTypeId )?.GroupTypeColor;

            /// <summary>
            /// Gets the name of the group type.
            /// </summary>
            /// <value>
            /// The name of the group type.
            /// </value>
            public string GroupTypeName => GroupTypeCache.Get( GroupTypeId )?.Name;

            /// <summary>
            /// Gets or sets the start stop history.
            /// </summary>
            /// <value>
            /// The start stop history.
            /// </value>
            public List<GroupMemberHistoricalSummary> StartStopHistory { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class GroupMemberHistoricalSummary
        {
            private GroupMemberHistorical _groupMemberHistorical;
            private Group _group;
            private DateTime? _dateTimeAdded;
            internal List<GroupNameHistory> _groupNameHistory;

            /// <summary>
            /// Initializes a new instance of the <see cref="GroupMemberHistoricalSummary"/> class.
            /// </summary>
            /// <param name="group">The group.</param>
            /// <param name="groupMemberHistorical">The group member historical.</param>
            /// <param name="dateTimeAdded">The date time added.</param>
            public GroupMemberHistoricalSummary( Group group, GroupMemberHistorical groupMemberHistorical, DateTime? dateTimeAdded )
            {
                _group = group;
                _groupMemberHistorical = groupMemberHistorical;
                _dateTimeAdded = dateTimeAdded;
            }

            /// <summary>
            /// Gets the start date time.
            /// </summary>
            /// <value>
            /// The start date time.
            /// </value>
            public DateTime StartDateTime
            {
                get
                {
                    if ( this.IsFirstTimeInGroup && _dateTimeAdded.HasValue )
                    {
                        if ( _dateTimeAdded.Value < _groupMemberHistorical.EffectiveDateTime )
                        {
                            return _dateTimeAdded.Value;
                        }
                    }

                    return _groupMemberHistorical.EffectiveDateTime;
                }
            }

            /// <summary>
            /// Gets the stop date time.
            /// </summary>
            /// <value>
            /// The stop date time.
            /// </value>
            public DateTime StopDateTime => _groupMemberHistorical.ExpireDateTime;

            /// <summary>
            /// Gets the current name of the group.
            /// </summary>
            /// <value>
            /// The name of the group.
            /// </value>
            public string GroupName => _group.Name;

            private string _groupNameHistorical = null;

            /// <summary>
            /// Gets the group name as of the StartDateTime
            /// </summary>
            /// <value>
            /// The group name historical.
            /// </value>
            public string GroupNameHistorical
            {
                get
                {
                    if ( _groupNameHistorical == null )
                    {
                        _groupNameHistorical = _groupNameHistory?
                            .Where( a => _groupMemberHistorical.EffectiveDateTime < a.ExpireDateTime )
                            .OrderBy( a => a.ExpireDateTime )
                            .Select( a => a.GroupName )
                            .FirstOrDefault();

                        // if we couldn't find a GroupHistorical record, set _groupNameHistorical to _group.Name 
                        if ( _groupNameHistorical == null )
                        {
                            _groupNameHistorical = _group.Name;
                        }
                    }

                    return _groupNameHistorical;
                }
            }

            /// <summary>
            /// Gets or sets a value indicating whether this instance is first time in group.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is first time in group; otherwise, <c>false</c>.
            /// </value>
            internal bool IsFirstTimeInGroup { get; set; }

            /// <summary>
            /// Gets or sets the name of the group role.
            /// </summary>
            /// <value>
            /// The name of the group role.
            /// </value>
            public string GroupRoleName => _groupMemberHistorical.GroupRoleName;

            /// <summary>
            /// Gets or sets a value indicating whether this instance is leader.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is leader; otherwise, <c>false</c>.
            /// </value>
            public bool IsLeader => _groupMemberHistorical.IsLeader;

            /// <summary>
            /// Gets the name of the group member status.
            /// </summary>
            /// <value>
            /// The name of the group member status.
            /// </value>
            public string GroupMemberStatusName => this.GroupMemberStatus.ConvertToString();

            /// <summary>
            /// Gets or sets the group member status.
            /// </summary>
            /// <value>
            /// The group member status.
            /// </value>
            public GroupMemberStatus GroupMemberStatus => _groupMemberHistorical.GroupMemberStatus;
        }

        /// <summary>
        /// 
        /// </summary>
        public class GroupNameHistory
        {
            /// <summary>
            /// Gets or sets the group identifier.
            /// </summary>
            /// <value>
            /// The group identifier.
            /// </value>
            public int GroupId { get; set; }

            /// <summary>
            /// Gets or sets the effective date time.
            /// </summary>
            /// <value>
            /// The effective date time.
            /// </value>
            public DateTime EffectiveDateTime { get; set; }

            /// <summary>
            /// Gets or sets the expire date time.
            /// </summary>
            /// <value>
            /// The expire date time.
            /// </value>
            public DateTime ExpireDateTime { get; set; }

            /// <summary>
            /// Gets or sets the name of the group.
            /// </summary>
            /// <value>
            /// The name of the group.
            /// </value>
            public string GroupName { get; set; }
        }
    }
}
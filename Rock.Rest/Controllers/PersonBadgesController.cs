// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.Data.Entity;
using System.Net;
using System.Web.Http;
using System.Linq;

using Rock.Model;
using Rock.Rest.Filters;
using Rock.Security;
using Rock.Data;
using System.Collections.Generic;
using System.Data;
using System;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public partial class PersonBadgesController
    {
        /// <summary>
        /// Gets the attendance summary data for the 24 month attenance badge 
        /// </summary>
        /// <param name="personId">The person id.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/PersonBadges/InGroupOfType/{personId}/{groupTypeId}" )]
        public GroupOfTypeResult GetInGroupOfType(int personId, Guid groupTypeId)
        {
            GroupOfTypeResult result = new GroupOfTypeResult();
            result.PersonId = personId;
            result.PersonInGroup = false;
            result.GroupList = new List<GroupSummary>();

            // get person info
            Person person = new PersonService( (Rock.Data.RockContext)Service.Context ).Get( personId );

            if (person != null)
            {
                result.NickName = person.NickName;
                result.LastName = person.LastName;
            }

            // get group type info
            GroupType groupType = new GroupTypeService( (Rock.Data.RockContext)Service.Context ).Get( groupTypeId );

            if (groupType != null)
            {
                result.GroupTypeName = groupType.Name;
                result.GroupTypeIconCss = groupType.IconCssClass;
                result.GroupTypeId = groupType.Id;
            }

            // determine if person is in this type of group
            GroupMemberService groupMemberService = new GroupMemberService( (Rock.Data.RockContext)Service.Context );
            
            IQueryable<GroupMember> groupMembershipsQuery = groupMemberService.Queryable("Person,GroupRole,Group")
                                        .Where(t => t.Group.GroupType.Guid == groupTypeId && t.PersonId == personId && t.GroupMemberStatus == GroupMemberStatus.Active )
                                        .OrderBy(g => g.GroupRole.Order);

            foreach (GroupMember member in groupMembershipsQuery)
            {
                result.PersonInGroup = true;
                GroupSummary group = new GroupSummary();
                group.GroupName = member.Group.Name;
                group.GroupId = member.Group.Id;
                group.RoleName = member.GroupRole.Name;

                result.GroupList.Add(group);
            }

            return result;
        }

        /// <summary>
        /// Returns groups that are a specified type and geofence a given person
        /// </summary>
        /// <param name="personId">The person id.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/PersonBadges/GeofencingGroups/{personId}/{groupTypeGuid}" )]
        public List<GroupAndLeaderInfo> GetGeofencingGroups( int personId, Guid groupTypeGuid )
        {
            var rockContext = (Rock.Data.RockContext)Service.Context;
            var groupMemberService = new GroupMemberService( rockContext );

            var groups = new GroupService( rockContext ).GetGeofencingGroups( personId, groupTypeGuid ).AsNoTracking();

            var result = new List<GroupAndLeaderInfo>();
            foreach ( var group in groups.OrderBy( g => g.Name ) )
            {
                var info = new GroupAndLeaderInfo();
                info.GroupName = group.Name.Trim();
                info.LeaderNames = groupMemberService
                    .Queryable().AsNoTracking()
                    .Where( m => 
                        m.GroupId == group.Id &&
                        m.GroupRole.IsLeader )
                    .Select( m => m.Person.NickName + " " + m.Person.LastName )
                    .ToList()
                    .AsDelimited(", ");
                result.Add(info);
            }

            return result;
        }

        /// <summary>
        /// Gets the attendance summary data for the 24 month attenance badge 
        /// </summary>
        /// <param name="personId">The person id.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/PersonBadges/WeeksAttendedInDuration/{personId}/{weekCount}" )]
        public int GetWeeksAttendedInDuration(int personId, int weekCount)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("PersonId", personId);
            parameters.Add("WeekDuration", weekCount);

            var result = DbService.ExecuteScaler( "spCheckin_WeeksAttendedInDuration", System.Data.CommandType.StoredProcedure, parameters );
            if (result != null)
            {
                return (int)result;
            }

            return -1;
        }

        /// <summary>
        /// Gets the attendance summary data for the 24 month attenance badge 
        /// </summary>
        /// <param name="personId">The person id.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/PersonBadges/LastVisitOnSite/{personId}/{siteId}" )]
        public int GetLastVisitOnSite( int personId, int siteId )
        {
            PageView mostRecentPageView = new PageViewService( (Rock.Data.RockContext)Service.Context ).Queryable()
                                                .Where( p => p.PersonAlias.PersonId == personId && p.SiteId == siteId )
                                                .OrderByDescending( p => p.DateTimeViewed )
                                                .FirstOrDefault();

            if ( mostRecentPageView != null && mostRecentPageView.DateTimeViewed.HasValue)
            {
                TimeSpan duration = RockDateTime.Now - mostRecentPageView.DateTimeViewed.Value;
                return duration.Days;
            }

            return -1;
        }

        /// <summary>
        /// Gets the attendance summary data for the 24 month attenance badge 
        /// </summary>
        /// <param name="personId">The person id.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/PersonBadges/FamilyAttendance/{personId}/{monthCount}" )]
        public IQueryable<MonthlyAttendanceSummary> GetFamilyAttendance(int personId, int monthCount)
        {
            List<MonthlyAttendanceSummary> attendanceSummary = new List<MonthlyAttendanceSummary>();
            
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("PersonId", personId);
            parameters.Add("MonthCount", monthCount);

            var table = DbService.GetDataTable( "spCheckin_BadgeAttendance", System.Data.CommandType.StoredProcedure, parameters );

            if (table != null)
            {
                foreach (DataRow row in table.Rows)
                {
                    MonthlyAttendanceSummary item = new MonthlyAttendanceSummary();
                    item.AttendanceCount = row["AttendanceCount"].ToString().AsInteger();
                    item.SundaysInMonth = row["SundaysInMonth"].ToString().AsInteger();
                    item.Month = row["Month"].ToString().AsInteger();
                    item.Year = row["Year"].ToString().AsInteger();

                    attendanceSummary.Add(item);
                }
            }

            return attendanceSummary.AsQueryable();
        }

        /// <summary>
        /// Result set for group of type badge
        /// </summary>
        public class GroupOfTypeResult
        {
            /// <summary>
            /// Gets or sets the group type id of the group.
            /// </summary>
            /// <value>
            /// The group type id.
            /// </value>
            public int GroupTypeId { get; set; }

            /// <summary>
            /// Gets or sets the group type name.
            /// </summary>
            /// <value>
            /// The group type name.
            /// </value>
            public string GroupTypeName { get; set; }

            /// <summary>
            /// Gets or sets the group type icon css.
            /// </summary>
            /// <value>
            /// The group type icon css.
            /// </value>
            public string GroupTypeIconCss { get; set; }

            /// <summary>
            /// Gets or sets the person id of the individual.
            /// </summary>
            /// <value>
            /// The person id.
            /// </value>
            public int PersonId { get; set; }

            /// <summary>
            /// Gets or sets the person nick name of the individual.
            /// </summary>
            /// <value>
            /// The nick name.
            /// </value>
            public string NickName { get; set; }

            /// <summary>
            /// Gets or sets the person last name of the individual.
            /// </summary>
            /// <value>
            /// The last name.
            /// </value>
            public string LastName { get; set; }

            /// <summary>
            /// Gets or sets whether the given person is in a group of this type.
            /// </summary>
            /// <value>
            /// Whether the person is in a group of this type.
            /// </value>
            public bool PersonInGroup { get; set; }

            /// <summary>
            /// Gets or sets a list of groups the person is in.
            /// </summary>
            /// <value>List of groups that the person is in.</value>
            public List<GroupSummary> GroupList { get; set; }
        }

        /// <summary>
        /// Summary of a group for use in the group of type result
        /// </summary>
        public class GroupSummary
        {
            /// <summary>
            /// Gets or sets the group id of the group.
            /// </summary>
            /// <value>
            /// The group type id.
            /// </value>
            public int GroupId { get; set; }

            /// <summary>
            /// Gets or sets the group name.
            /// </summary>
            /// <value>
            /// The group type name.
            /// </value>
            public string GroupName { get; set; }

            /// <summary>
            /// Gets or sets the group member role name.
            /// </summary>
            /// <value>
            /// The group member role name.
            /// </value>
            public string RoleName { get; set; }
        }

        /// <summary>
        /// Group and Leader name info
        /// </summary>
        public class GroupAndLeaderInfo
        {
            /// <summary>
            /// Gets or sets the name of the group.
            /// </summary>
            /// <value>
            /// The name of the group.
            /// </value>
            public string GroupName { get; set; }

            /// <summary>
            /// Gets or sets the leader names.
            /// </summary>
            /// <value>
            /// The leader names.
            /// </value>
            public string LeaderNames { get; set; }
        }

        /// <summary>
        /// Monthly attendance summary structure
        /// </summary>
        public class MonthlyAttendanceSummary
        {
            /// <summary>
            /// Gets or sets the number of times the unit attended.
            /// </summary>
            /// <value>
            /// The number of attendances.
            /// </value>
            public int AttendanceCount { get; set; }

            /// <summary>
            /// Gets or sets the number of Sundays in the month
            /// </summary>
            /// <value>
            /// The number of Sundays.
            /// </value>
            public int SundaysInMonth { get; set; }

            /// <summary>
            /// Gets or sets the month.
            /// </summary>
            /// <value>
            /// The month of the attendance.
            /// </value>
            public int Month { get; set; }

            /// <summary>
            /// Gets or sets the year.
            /// </summary>
            /// <value>The year.</value>
            public int Year { get; set; }
        }
    }
}

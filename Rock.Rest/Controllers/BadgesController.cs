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
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;

using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Rest.Controllers
{
    /// <summary>
    ///
    /// </summary>
    [RockGuid( "de5b9d27-4a72-45af-bf78-75888cc9a554" )]
    public partial class BadgesController
    {
        /// <summary>
        /// Gets the attendance summary data for the 24 month attendance badge
        /// </summary>
        /// <param name="personId">The person id.</param>
        /// <param name="groupTypeGuid">The group type unique identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/Badges/InGroupOfType/{personId}/{groupTypeGuid}" )]
        [System.Web.Http.Route( "api/PersonBadges/InGroupOfType/{personId}/{groupTypeGuid}" )]
        [RockGuid( "bf5e550b-a516-4392-bb0d-01dd9924cb51" )]
        public GroupOfTypeResult GetInGroupOfType( int personId, Guid groupTypeGuid )
        {
            GroupOfTypeResult result = new GroupOfTypeResult();
            result.PersonId = personId;
            result.PersonInGroup = false;
            result.GroupList = new List<GroupSummary>();

            // get person info
            Person person = new PersonService( ( Rock.Data.RockContext ) Service.Context ).Get( personId );

            if ( person != null )
            {
                result.NickName = person.NickName;
                result.LastName = person.LastName;
            }

            // get group type info
            GroupType groupType = new GroupTypeService( ( Rock.Data.RockContext ) Service.Context ).Get( groupTypeGuid );

            if ( groupType != null )
            {
                result.GroupTypeName = groupType.Name;
                result.GroupTypeIconCss = groupType.IconCssClass;
                result.GroupTypeId = groupType.Id;
            }

            // determine if person is in this type of group
            GroupMemberService groupMemberService = new GroupMemberService( ( Rock.Data.RockContext ) Service.Context );

            IQueryable<GroupMember> groupMembershipsQuery = groupMemberService.Queryable( "Person,GroupRole,Group" )
                                        .Where( t => t.Group.GroupType.Guid == groupTypeGuid
                                                 && t.PersonId == personId
                                                 && t.GroupMemberStatus == GroupMemberStatus.Active
                                                 && t.Group.IsActive && !t.Group.IsArchived )
                                        .OrderBy( g => g.GroupRole.Order );

            foreach ( GroupMember member in groupMembershipsQuery )
            {
                result.PersonInGroup = true;
                GroupSummary group = new GroupSummary();
                group.GroupName = member.Group.Name;
                group.GroupId = member.Group.Id;
                group.RoleName = member.GroupRole.Name;

                result.GroupList.Add( group );
            }

            return result;
        }

        /// <summary>
        /// Gets the attendance summary data for the 24 month attendance badge
        /// </summary>
        /// <param name="personId">The person id.</param>
        /// <param name="definedValueGuid">The defined value unique identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/Badges/InGroupWithPurpose/{personId}/{definedValueGuid}" )]
        [System.Web.Http.Route( "api/PersonBadges/InGroupWithPurpose/{personId}/{definedValueGuid}" )]
        [RockGuid( "42e14515-eb74-4792-8c2d-2d59977d0670" )]
        public GroupWithPurposeResult GetGroupWithPurpose( int personId, Guid definedValueGuid )
        {
            GroupWithPurposeResult result = new GroupWithPurposeResult();
            result.PersonId = personId;
            result.PersonInGroup = false;
            result.GroupList = new List<GroupSummary>();

            // get person info
            Person person = new PersonService( ( Rock.Data.RockContext ) Service.Context ).Get( personId );

            if ( person != null )
            {
                result.NickName = person.NickName;
                result.LastName = person.LastName;
            }

            var purposeValue = DefinedValueCache.Get( definedValueGuid );
            result.Purpose = purposeValue.Value;

            // determine if person is in a group with this purpose
            GroupMemberService groupMemberService = new GroupMemberService( ( Rock.Data.RockContext ) Service.Context );

            IQueryable<GroupMember> groupMembershipsQuery = groupMemberService.Queryable( "Person,GroupRole,Group" )
                                        .Where( t => t.Group.GroupType.GroupTypePurposeValueId == purposeValue.Id
                                                 && t.PersonId == personId
                                                 && t.GroupMemberStatus == GroupMemberStatus.Active
                                                 && t.Group.IsActive )
                                        .OrderBy( g => g.GroupRole.Order );

            foreach ( GroupMember member in groupMembershipsQuery )
            {
                result.PersonInGroup = true;
                GroupSummary group = new GroupSummary();
                group.GroupName = member.Group.Name;
                group.GroupId = member.Group.Id;
                group.RoleName = member.GroupRole.Name;

                result.GroupList.Add( group );
            }

            return result;
        }

        /// <summary>
        /// Returns groups that are a specified type and geofence a given person
        /// </summary>
        /// <param name="personId">The person id.</param>
        /// <param name="groupTypeGuid">The group type unique identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/Badges/GeofencingGroups/{personId}/{groupTypeGuid}" )]
        [System.Web.Http.Route( "api/PersonBadges/GeofencingGroups/{personId}/{groupTypeGuid}" )]
        [RockGuid( "3e3a0048-33ce-48e5-92a6-d2f1537ab5f7" )]
        public List<GroupAndLeaderInfo> GetGeofencingGroups( int personId, Guid groupTypeGuid )
        {
            var rockContext = ( Rock.Data.RockContext ) Service.Context;
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
                    .AsDelimited( ", " );
                result.Add( info );
            }

            return result;
        }

        /// <summary>
        /// Gets the attendance summary data for the 24 month attendance badge
        /// </summary>
        /// <param name="personId">The person id.</param>
        /// <param name="weekCount">The week count.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/Badges/WeeksAttendedInDuration/{personId}/{weekCount}" )]
        [System.Web.Http.Route( "api/PersonBadges/WeeksAttendedInDuration/{personId}/{weekCount}" )]
        [RockGuid( "85bc0af8-aa65-454d-bd3f-6289f5e96a9e" )]
        public int GetWeeksAttendedInDuration( int personId, int weekCount )
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add( "PersonId", personId );
            parameters.Add( "WeekDuration", weekCount );

            var result = DbService.ExecuteScaler( "spCheckin_WeeksAttendedInDuration", System.Data.CommandType.StoredProcedure, parameters );
            if ( result != null )
            {
                return ( int ) result;
            }

            return -1;
        }

        /// <summary>
        /// Gets the attendance summary data for the 24 month attendance badge
        /// </summary>
        /// <param name="personId">The person id.</param>
        /// <param name="siteId">The site identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/Badges/LastVisitOnSite/{personId}/{siteId}" )]
        [System.Web.Http.Route( "api/PersonBadges/LastVisitOnSite/{personId}/{siteId}" )]
        [RockGuid( "5b2ca298-b318-44a4-a071-2597f91a73c7" )]
        public int GetLastVisitOnSite( int personId, int siteId )
        {
            int channelMediumValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_WEBSITE.AsGuid() ).Id;

            InteractionChannelService interactionChannelService = new InteractionChannelService( ( Rock.Data.RockContext ) Service.Context );
            var interactionChannelId = interactionChannelService.Queryable()
                                                .Where( a => a.ChannelTypeMediumValueId == channelMediumValueId && a.ChannelEntityId == siteId )
                                                .Select( a => ( int? ) a.Id )
                                                .FirstOrDefault();

            if ( interactionChannelId == null )
            {
                return -1;
            }

            var mostRecentPageViewInteractionDateTime = new InteractionService( ( Rock.Data.RockContext ) Service.Context ).Queryable()
                                                .Where( a => a.PersonAlias.PersonId == personId && a.InteractionComponent.InteractionChannelId == interactionChannelId )
                                                .Max( p => ( DateTime? ) p.InteractionDateTime );

            if ( mostRecentPageViewInteractionDateTime.HasValue )
            {
                TimeSpan duration = RockDateTime.Now - mostRecentPageViewInteractionDateTime.Value;
                return duration.Days;
            }

            return -1;
        }

        /// <summary>
        /// Gets the Total number of personal devices
        /// </summary>
        /// <param name="personId">The person id.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/Badges/PersonalDevicesNumber/{personId}" )]
        [System.Web.Http.Route( "api/PersonBadges/PersonalDevicesNumber/{personId}" )]
        [RockGuid( "9eb3dd74-4b8a-41d3-ac04-3e929eb89afa" )]
        public int GetPersonalDevicesNumber( int personId )
        {
            int channelMediumValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_WEBSITE.AsGuid() ).Id;

            PersonalDeviceService personalDeviceService = new PersonalDeviceService( ( Rock.Data.RockContext ) Service.Context );
            return personalDeviceService.Queryable()
                                                .Where( a => a.PersonAlias.PersonId == personId )
                                                .Count();

        }

        /// <summary>
        /// Gets the attendance summary data for the 24 month attendance badge
        /// </summary>
        /// <param name="personId">The person id.</param>
        /// <param name="monthCount">The month count.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/Badges/FamilyAttendance/{personId}/{monthCount}" )]
        [System.Web.Http.Route( "api/PersonBadges/FamilyAttendance/{personId}/{monthCount}" )]
        [RockGuid( "b4d861ed-1b7a-49f8-beff-79a8886dc1db" )]
        public IQueryable<MonthlyAttendanceSummary> GetFamilyAttendance( int personId, int monthCount )
        {
            List<MonthlyAttendanceSummary> attendanceSummary = new List<MonthlyAttendanceSummary>();

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add( "PersonId", personId );
            parameters.Add( "MonthCount", monthCount );

            var table = DbService.GetDataTable( "spCheckin_BadgeAttendance", System.Data.CommandType.StoredProcedure, parameters );

            if ( table != null )
            {
                foreach ( DataRow row in table.Rows )
                {
                    MonthlyAttendanceSummary item = new MonthlyAttendanceSummary();
                    item.AttendanceCount = row["AttendanceCount"].ToString().AsInteger();
                    item.SundaysInMonth = row["SundaysInMonth"].ToString().AsInteger();
                    item.Month = row["Month"].ToString().AsInteger();
                    item.Year = row["Year"].ToString().AsInteger();

                    attendanceSummary.Add( item );
                }
            }

            return attendanceSummary.AsQueryable();
        }

        /// <summary>
        /// Gets the the number of interactions in a given date range
        /// </summary>
        /// <param name="personId">The person id.</param>
        /// <param name="interactionChannelId">The interaction channel identifier.</param>
        /// <param name="delimitedDateRange">The delimited date range value.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/Badges/InteractionsInRange/{personId}/{interactionChannelId}/{delimitedDateRange}" )]
        [System.Web.Http.Route( "api/PersonBadges/InteractionsInRange/{personId}/{interactionChannelId}/{delimitedDateRange}" )]
        [RockGuid( "29930905-936f-46be-818c-25be683dd48c" )]
        public int InteractionsInRange( int personId, int interactionChannelId, string delimitedDateRange )
        {
            var interactionQry = new InteractionService( ( Rock.Data.RockContext ) Service.Context ).Queryable()
                                                .Where( a => a.PersonAlias.PersonId == personId && a.InteractionComponent.InteractionChannelId == interactionChannelId );

            if ( !string.IsNullOrEmpty( delimitedDateRange ) )
            {
                var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( delimitedDateRange );
                if ( dateRange.Start.HasValue )
                {
                    interactionQry = interactionQry.Where( a => a.InteractionDateTime >= dateRange.Start.Value );
                }

                if ( dateRange.End.HasValue )
                {
                    interactionQry = interactionQry.Where( a => a.InteractionDateTime <= dateRange.End.Value );
                }
            }

            return interactionQry.Count();
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
        /// Result set for group with purpose badge
        /// </summary>
        public class GroupWithPurposeResult
        {
            /// <summary>
            /// Gets or sets the purpose.
            /// </summary>
            /// <value>
            /// The purpose.
            /// </value>
            public string Purpose { get; set; }

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

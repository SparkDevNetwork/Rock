using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;

namespace church.ccv.Badges.Rest.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public partial class CCVBadgesController : Rock.Rest.ApiControllerBase
    {
        /// <summary>
        /// Returns groups that are a specified type and geofence a given person for their home campus
        /// </summary>
        /// <param name="personId">The person id.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/CCV/Badges/GeofencingCampusGroups/{personId}/{groupTypeGuid}" )]
        public List<GroupAndLeaderInfo> GetGeofencingCampusGroups( int personId, Guid groupTypeGuid )
        {
            var rockContext = new RockContext();
            var groupMemberService = new GroupMemberService( rockContext );
            int? campusId = null;
            var person = new PersonService( rockContext ).Get( personId );
            if ( person != null )
            {
                var campus = person.GetCampus();
                if ( campus != null )
                {
                    campusId = campus.Id;
                }
            }

            var groups = new GroupService( rockContext ).GetGeofencingGroups( personId, groupTypeGuid ).AsNoTracking();
            var campusGroups = groups.Where( a => a.CampusId == campusId );

            var result = new List<GroupAndLeaderInfo>();
            foreach ( var group in campusGroups.OrderBy( g => g.Name ) )
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
        /// Steps Result 
        /// </summary>
        public class StepsBarResult
        {
            public BaptismResult BaptismResult { get; set; }
            public bool IsMember { get; set; }
            public bool IsWorshipper { get; set; }
            public ConnectionResult ConnectionResult { get; set; }
            public bool isTithing { get; set; }
            public ServingResult ServingResult { get; set;}
            public CoachingResult CoachingResult { get; set; }
        }

        /// <summary>
        /// Coaching Result
        /// </summary>
        public class CoachingResult
        {
            /// <summary>
            /// Gets or sets a value indicating whether this instance is coaching.
            /// </summary>
            /// <value>
            /// <c>true</c> if this instance is coaching; otherwise, <c>false</c>.
            /// </value>
            public bool IsCoaching { get; set; }
            /// <summary>
            /// Gets or sets the groups.
            /// </summary>
            /// <value>
            /// The groups.
            /// </value>
            public List<GroupMemberSummary> Groups { get; set; }
        }

        /// <summary>
        /// Serving Result
        /// </summary>
        public class ServingResult
        {
            /// <summary>
            /// Gets or sets a value indicating whether this instance is serving.
            /// </summary>
            /// <value>
            /// <c>true</c> if this instance is serving; otherwise, <c>false</c>.
            /// </value>
            public bool IsServing { get; set; }
            /// <summary>
            /// Gets or sets the groups.
            /// </summary>
            /// <value>
            /// The groups.
            /// </value>
            List<GroupMemberSummary> Groups { get; set; }
        }

        /// <summary>
        /// Connection Result
        /// </summary>
        public class ConnectionResult
        {
            /// <summary>
            /// Gets or sets a value indicating whether this instance is leader.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is leader; otherwise, <c>false</c>.
            /// </value>
            public bool IsLeader { get; set; }
            /// <summary>
            /// Gets or sets the connection status.
            /// </summary>
            /// <value>
            /// The connection status.
            /// </value>
            public ConnectionStatus ConnectionStatus { get; set; }
            /// <summary>
            /// Gets or sets the groups.
            /// </summary>
            /// <value>
            /// The groups.
            /// </value>
            public List<GroupMemberSummary> Groups { get; set; }
        }

        /// <summary>
        /// Group Member Summary
        /// </summary>
        public class GroupMemberSummary
        {
            /// <summary>
            /// Gets or sets the group identifier.
            /// </summary>
            /// <value>
            /// The group identifier.
            /// </value>
            public int GroupId { get; set; }
            /// <summary>
            /// Gets or sets the name of the group.
            /// </summary>
            /// <value>
            /// The name of the group.
            /// </value>
            public string GroupName { get; set; }

            /// <summary>
            /// Gets or sets the role.
            /// </summary>
            /// <value>
            /// The role.
            /// </value>
            public string Role { get; set; }

            /// <summary>
            /// Gets or sets the role identifier.
            /// </summary>
            /// <value>
            /// The role identifier.
            /// </value>
            public int RoleId { get; set; }
        }

        /// <summary>
        /// Connection Status
        /// </summary>
        public enum ConnectionStatus
        {
            InGroup,
            NotInGroup,
            PendingInGroup
        }

        /// <summary>
        /// Baptism Result
        /// </summary>
        public class BaptismResult
        {
            /// <summary>
            /// Gets or sets the baptism date.
            /// </summary>
            /// <value>
            /// The baptism date.
            /// </value>
            public DateTime? BaptismDate { get; set; }
            /// <summary>
            /// Gets or sets the baptism status.
            /// </summary>
            /// <value>
            /// The baptism status.
            /// </value>
            public BaptismStatus BaptismStatus { get; set; }
            /// <summary>
            /// Gets or sets the registration group identifier.
            /// </summary>
            /// <value>
            /// The registration group identifier.
            /// </value>
            public int RegistrationGroupId { get; set; }
        }

        /// <summary>
        /// Baptism status
        /// </summary>
        public enum BaptismStatus
        {
            NotBaptised,
            Baptised,
            Registered
        }
    }
}

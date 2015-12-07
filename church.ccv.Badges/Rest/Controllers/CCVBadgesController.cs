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
    }
}

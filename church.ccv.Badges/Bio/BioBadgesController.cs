using church.ccv.Actions;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;

namespace church.ccv.Badges.Bio
{
    public partial class BioBadgesController : Rock.Rest.ApiControllerBase
    {
        /// <summary>
        /// Returns campuses with leader of a given person
        /// </summary>
        /// <param name="personId">The person id.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/CCV/Badges/CampusesWithLeader/{personId}" )]
        public List<CampusAndLeaderInfo> GetCampusesWithLeaders( int personId )
        {
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );

            var result = new List<CampusAndLeaderInfo>();
            var info = new CampusAndLeaderInfo();

            var families = personService.GetFamilies( personId );

            if ( families != null )
            {
                var campusNames = new List<string>();
                var campusLeaders = new List<string>();

                foreach ( int campusId in families
                    .Where( g => g.CampusId.HasValue )
                    .Select( g => g.CampusId )
                    .Distinct()
                    .ToList() )
                {
                    var campus = Rock.Web.Cache.CampusCache.Read( campusId );

                    campusNames.Add( campus.Name );
                    campusLeaders.Add( new PersonAliasService( rockContext ).GetPerson( (int)campus.LeaderPersonAliasId ).FullName );
                }

                info.CampusNames = campusNames.ToList().AsDelimited( ", " );
                info.LeaderNames = campusLeaders.ToList().AsDelimited( ", " );
            }

            result.Add( info );

            return result;
        }

        /// <summary>
        /// Returns leaders of specified group types of a given person
        /// </summary>
        /// <param name="personId">The person id.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/CCV/Badges/Coaches/{personId}" )]
        public List<LeaderInfo> GetCoaches( int personId )
        {
            var rockContext = new RockContext();

            bool isAdult = new PersonService( rockContext ).GetAllAdults( ).Where( p => p.Id == personId ).Count( ) > 0 ? true : false;

            List<int> groupIds = null;
            bool isCoached = false;

            if( isAdult )
            {
                Actions_Adult.Mentored.Result mentorResult;
                Actions_Adult.Mentored.IsMentored( personId, out mentorResult );

                isCoached = mentorResult.IsMentored( );
                groupIds = mentorResult.GetCombinedMentorGroups( );
            }
            else
            {
                Actions_Student.Mentored.Result mentorResult;
                Actions_Student.Mentored.IsMentored( personId, out mentorResult );

                isCoached = mentorResult.IsMentored( );
                groupIds = mentorResult.GetCombinedMentorGroups( );
            }

            // setup the coach lists, which will return as blank if this person isn't being coached
            var result = new List<LeaderInfo>();
            var info = new LeaderInfo();

            if( isCoached == true )
            {
                var groupMemberService = new GroupMemberService( rockContext );
            
                var groups = groupMemberService
                    .Queryable()
                    .Where( m => groupIds.Contains( m.Group.Id ) && m.PersonId == personId )
                    .Select( m => m.Group.Id )
                    .ToList();
                
                info.LeaderNames = groupMemberService
                    .Queryable()
                    .Where( m => groups.Contains( m.Group.Id )
                        && m.GroupMemberStatus != GroupMemberStatus.Inactive
                        && m.Group.IsActive != false
                        && m.GroupRole.IsLeader == true )
                    .Select( m => m.Person.NickName + " " + m.Person.LastName )
                    .ToList()
                    .AsDelimited( ", " );

                if ( !string.IsNullOrEmpty( info.LeaderNames ) )
                {
                    result.Add( info );
                }
            }

            return result;
        }

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
        /// Campus and Leader info
        /// </summary>
        public class CampusAndLeaderInfo
        {
            /// <summary>
            /// Gets or sets the name of the campus.
            /// </summary>
            /// <value>
            /// The name of the campus.
            /// </value>
            public string CampusNames { get; set; }

            /// <summary>
            /// Gets or sets the leader names.
            /// </summary>
            /// <value>
            /// The leader names.
            /// </value>
            public string LeaderNames { get; set; }
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
        /// Leader name info
        /// </summary>
        public class LeaderInfo
        {
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

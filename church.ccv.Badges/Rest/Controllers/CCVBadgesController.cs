using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web.Cache;

namespace church.ccv.Badges.Rest.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public partial class CCVBadgesController : Rock.Rest.ApiControllerBase
    {
        const string ATTRIBUTE_PERSON_DATE_OF_BAPTISM = "BaptismDate";
        const string ATTRIBUTE_PERSON_ERA = "CurrentlyanERA";
        const string ATTRIBUTE_PERSON_GIVING_IN_LAST_12_MONTHS = "GivingInLast12Months";
        const string ATTRIBUTE_PERSON_DATE_OF_MEMBERSHIP = "DateofMembership";

        const string ATTRIBUTE_GLOBAL_TITHE_THRESHOLD = "TitheThreshold";
        const string ATTRIBUTE_GLOBAL_COACHING_GROUPTYPE_IDS = "CoachingGroupTypeIds";
        const string ATTRIBUTE_GLOBAL_CONNECTION_GROUPTYPE_IDS = "ConnectionGroupTypeIds";
        const string ATTRIBUTE_GLOBAL_SERVING_GROUPTYPE_IDS = "ServingGroupTypeIds";
        const string ATTRIBUTE_GLOBAL_BAPTISM_GROUPTYPE_IDS = "BaptismGroupTypeIds";
        const string ATTRIBUTE_GLOBAL_MEMBERSHIP_VALUE_ID = "MembershipValueId";

        /// <summary>
        /// Returns leaders of specified group types of a given person
        /// </summary>
        /// <param name="personId">The person id.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/CCV/Badges/Coaches/{personId}/{groupTypeIds}" )]
        public List<LeaderInfo> GetCoaches( int personId, string groupTypeIds )
        {
            var rockContext = new RockContext();
            var groupMemberService = new GroupMemberService( rockContext );

            var groupTypeIdsList = groupTypeIds.Split( ',' ).AsIntegerList();

            var groups = groupMemberService
                .Queryable()
                .Where( m => groupTypeIdsList.Contains( m.Group.GroupTypeId )
                    && m.GroupMemberStatus != GroupMemberStatus.Inactive
                    && m.Group.IsActive != false
                    && m.PersonId == personId
                    && m.GroupRole.IsLeader != true )
                .Select( m => m.Group.Id )
                .ToList();

            var result = new List<LeaderInfo>();
            var info = new LeaderInfo();
            info.LeaderNames = groupMemberService
                .Queryable()
                .Where( m => groups.Contains( m.Group.Id )
                    && m.GroupMemberStatus != GroupMemberStatus.Inactive
                    && m.Group.IsActive != false
                    && m.GroupRole.IsLeader == true )
                .Select( m => m.Person.NickName + " " + m.Person.LastName )
                .ToList()
                .AsDelimited( ", " );

            if( !string.IsNullOrEmpty( info.LeaderNames ) )
            {
                result.Add( info );
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

        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/CCV/Badges/StepsBar/{personGuid}" )]
        public StepsBarResult GetStepsBar( Guid personGuid )
        {

            return GetStepsResult( personGuid );

        }

        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/CCV/Badges/StepsBarGroup/{groupGuid}" )]
        public Dictionary<int, StepsBarResult> GetStepsBarGroup( Guid groupGuid )
        {
            Dictionary<int, StepsBarResult> groupResults = new Dictionary<int, StepsBarResult>();

            using ( RockContext rockContext = new RockContext() )
            {
                var groupMembers = new GroupMemberService( rockContext ).Queryable().Where( m => m.Group.Guid == groupGuid ).Select( m => new { Guid = m.Person.Guid, Id = m.PersonId } );

                foreach ( var groupMember in groupMembers )
                {
                    groupResults.Add( groupMember.Id, GetStepsResult( groupMember.Guid ) );
                }
            }

            return groupResults;
        }

        private StepsBarResult GetStepsResult(Guid personGuid )
        {
            StepsBarResult stepsBarResult = new StepsBarResult();

            using ( RockContext rockContext = new RockContext() )
            {
                var person = new PersonService( rockContext ).Get( personGuid );
                if ( person != null )
                {
                    person.LoadAttributes();

                    // membership
                    int MEMBERSHIP_CONNECTION_VALUE_ID = DefinedValueCache.Read( GlobalAttributesCache.Read().GetValue( ATTRIBUTE_GLOBAL_MEMBERSHIP_VALUE_ID ) ).Id;
                    stepsBarResult.MembershipResult = new MembershipResult();
                    if ( person.ConnectionStatusValueId == MEMBERSHIP_CONNECTION_VALUE_ID )
                    {
                        stepsBarResult.MembershipResult.IsMember = true;
                        if ( person.AttributeValues.ContainsKey( ATTRIBUTE_PERSON_DATE_OF_MEMBERSHIP ) )
                        {
                            stepsBarResult.MembershipResult.MembershipDate = person.GetAttributeValue(ATTRIBUTE_PERSON_DATE_OF_MEMBERSHIP).AsDateTime();
                        }
                    }
                    else
                    {
                        stepsBarResult.MembershipResult.IsMember = false;
                    }

                    // baptism - baptism is driven by the baptism date person attribute
                    stepsBarResult.BaptismResult = new BaptismResult();
                    stepsBarResult.BaptismResult.BaptismDate = person.GetAttributeValue( ATTRIBUTE_PERSON_DATE_OF_BAPTISM ).AsDateTime();

                    if ( stepsBarResult.BaptismResult.BaptismDate.HasValue )
                    {
                        stepsBarResult.BaptismResult.BaptismStatus = BaptismStatus.Baptised;
                    } else
                    {
                        // check if registered for baptism
                        List<int> GROUPTYPES_BAPTISM_IDS = new List<int>();
                        try
                        {
                            GROUPTYPES_BAPTISM_IDS = GlobalAttributesCache.Read().GetValue( ATTRIBUTE_GLOBAL_BAPTISM_GROUPTYPE_IDS ).Split( ',' ).Select( int.Parse ).ToList();
                        }
                        catch
                        { 
                            // intentionally blank
                        }

                        var baptismGroups = new GroupMemberService( rockContext ).Queryable()
                                                        .Where( m => GROUPTYPES_BAPTISM_IDS.Contains(m.Group.GroupTypeId)
                                                             && m.GroupMemberStatus == GroupMemberStatus.Active
                                                             && m.PersonId == person.Id )
                                                        .Select( m => m.GroupId ).ToList();
                        if ( baptismGroups.Count > 0 ) {

                            // ensure baptisms are in the future
                            var baptismEventItems = new EventItemOccurrenceService( rockContext ).Queryable( "Schedule,Linkages" )
                                                .Where( e => e.Linkages.Any( l => l.GroupId.HasValue && baptismGroups.Contains( l.GroupId.Value ) ) )
                                                .ToList();

                            bool futureBaptismScheduled = false;
                            int? futureBaptismGroupId = null;
                            DateTime? futureBaptismDate = null;

                            foreach (var baptismEventItem in baptismEventItems)
                            {
                                if (baptismEventItem.NextStartDateTime >= RockDateTime.Now )
                                {
                                    futureBaptismScheduled = true;
                                    futureBaptismGroupId = baptismEventItem.Linkages.First().GroupId;
                                    futureBaptismDate = baptismEventItem.NextStartDateTime;
                                    break;
                                }
                            }

                            if ( futureBaptismScheduled )
                            {
                                stepsBarResult.BaptismResult.BaptismStatus = BaptismStatus.Registered;
                                stepsBarResult.BaptismResult.RegistrationGroupId = futureBaptismGroupId;
                                stepsBarResult.BaptismResult.BaptismRegistrationDate = futureBaptismDate;
                            }
                            else
                            {
                                stepsBarResult.BaptismResult.BaptismStatus = BaptismStatus.NotBaptised;
                            }
                        }
                    }

                    // is worshiper
                    stepsBarResult.IsWorshipper = person.GetAttributeValue( ATTRIBUTE_PERSON_ERA ).AsBoolean();

                    // connect - in NG group
                    stepsBarResult.ConnectionResult = new ConnectionResult();
                    stepsBarResult.ConnectionResult.Groups = new List<GroupMemberSummary>();

                    // get group list
                    List<int> GROUPTYPES_CONNECTION_IDS = new List<int>();
                    try
                    {
                        GROUPTYPES_CONNECTION_IDS = GlobalAttributesCache.Read().GetValue( ATTRIBUTE_GLOBAL_CONNECTION_GROUPTYPE_IDS ).Split( ',' ).Select( int.Parse ).ToList();
                    }
                    catch
                    {
                        // intentionally blank
                    }

                    var neighborhoodGroups = new GroupMemberService( rockContext ).Queryable()
                                                .Where( m => GROUPTYPES_CONNECTION_IDS.Contains(m.Group.GroupTypeId)
                                                     && m.GroupMemberStatus != GroupMemberStatus.Inactive
                                                     && m.Group.IsActive != false
                                                     && m.PersonId == person.Id)
                                                .Select(m => new
                                                                {
                                                                    GroupId = m.GroupId,
                                                                    GroupName = m.Group.Name,
                                                                    Role = m.GroupRole,
                                                                    Status = m.GroupMemberStatus,
                                                                    StartDate = m.CreatedDateTime
                                                                } ).ToList();

                    if (neighborhoodGroups.Count == 0 )
                    {
                        stepsBarResult.ConnectionResult.ConnectionStatus = ConnectionStatus.NotInGroup;
                    } else
                    {
                        stepsBarResult.ConnectionResult.ConnectedSince = DateTime.MaxValue;

                        foreach ( var group in neighborhoodGroups )
                        {
                            GroupMemberSummary groupMemberSummary = new GroupMemberSummary();
                            groupMemberSummary.GroupId = group.GroupId;
                            groupMemberSummary.GroupName = group.GroupName;
                            groupMemberSummary.Role = group.Role.Name;
                            groupMemberSummary.RoleId = group.Role.Id;
                            stepsBarResult.ConnectionResult.Groups.Add( groupMemberSummary );

                            if (group.StartDate < stepsBarResult.ConnectionResult.ConnectedSince )
                            {
                                stepsBarResult.ConnectionResult.ConnectedSince = group.StartDate;
                            }
                        }

                        if ( neighborhoodGroups.Any( m => m.Status == GroupMemberStatus.Active ))
                        {
                            stepsBarResult.ConnectionResult.ConnectionStatus = ConnectionStatus.InGroup;

                            if ( neighborhoodGroups.Any( m => m.Role.IsLeader ) )
                            {
                                stepsBarResult.ConnectionResult.IsLeader = true;
                            }
                        }
                        else
                        {
                            stepsBarResult.ConnectionResult.ConnectionStatus = ConnectionStatus.PendingInGroup;
                        }
                    }

                    // is tithing
                    decimal givingInLast12Months = person.GetAttributeValue( ATTRIBUTE_PERSON_GIVING_IN_LAST_12_MONTHS ).AsDecimal();
                    decimal titheThreshold = GlobalAttributesCache.Read().GetValue( ATTRIBUTE_GLOBAL_TITHE_THRESHOLD ).AsDecimal();

                    stepsBarResult.IsTithing = (givingInLast12Months >= titheThreshold);

                    // serving results
                    List<int> GROUPTYPES_SERVING_IDS = new List<int>();
                    try
                    {
                        GROUPTYPES_SERVING_IDS = GlobalAttributesCache.Read().GetValue( ATTRIBUTE_GLOBAL_SERVING_GROUPTYPE_IDS ).Split( ',' ).Select( int.Parse ).ToList();
                    }
                    catch
                    {
                        // intentionally blank
                    }

                    stepsBarResult.ServingResult = new ServingResult();
                    stepsBarResult.ServingResult.Groups = new List<GroupMemberSummary>();

                    var servingGroups = new GroupMemberService( rockContext ).Queryable()
                                                .Where( m => GROUPTYPES_SERVING_IDS.Contains(m.Group.GroupTypeId)
                                                     && m.GroupMemberStatus != GroupMemberStatus.Inactive
                                                     && m.Group.IsActive != false
                                                     && m.PersonId == person.Id )
                                                .Select( m => new
                                                {
                                                    GroupId = m.GroupId,
                                                    GroupName = m.Group.Name,
                                                    Role = m.GroupRole,
                                                    Status = m.GroupMemberStatus,
                                                    StartDate = m.CreatedDateTime
                                                } ).ToList();

                    if (servingGroups.Count == 0 )
                    {
                        stepsBarResult.ServingResult.IsServing = false;
                    } else
                    {
                        stepsBarResult.ServingResult.IsServing = true;
                        stepsBarResult.ServingResult.ServingSince = DateTime.MaxValue;

                        foreach ( var group in servingGroups )
                        {
                            GroupMemberSummary groupMemberSummary = new GroupMemberSummary();
                            groupMemberSummary.GroupId = group.GroupId;
                            groupMemberSummary.GroupName = group.GroupName;
                            groupMemberSummary.Role = group.Role.Name;
                            groupMemberSummary.RoleId = group.Role.Id;
                            stepsBarResult.ServingResult.Groups.Add( groupMemberSummary );

                            if ( group.StartDate < stepsBarResult.ServingResult.ServingSince )
                            {
                                stepsBarResult.ServingResult.ServingSince = group.StartDate;
                            }
                        }
                    }

                    // coaching
                    List<int> GROUPTYPES_COACHING_IDS = new List<int>();
                    try
                    {
                        GROUPTYPES_COACHING_IDS = GlobalAttributesCache.Read().GetValue( ATTRIBUTE_GLOBAL_COACHING_GROUPTYPE_IDS ).Split( ',' ).Select( int.Parse ).ToList();
                    }
                    catch
                    {
                        // intentionally blank
                    }

                    stepsBarResult.CoachingResult = new CoachingResult();
                    stepsBarResult.CoachingResult.Groups = new List<GroupMemberSummary>();

                    var coachingGroups = new GroupMemberService( rockContext ).Queryable()
                                                .Where( m => GROUPTYPES_COACHING_IDS.Contains(m.Group.GroupTypeId)
                                                     && m.GroupMemberStatus != GroupMemberStatus.Inactive
                                                     && m.Group.IsActive != false
                                                     && m.PersonId == person.Id 
                                                     && m.GroupRole.IsLeader == true)
                                                .Select( m => new
                                                {
                                                    GroupId = m.GroupId,
                                                    GroupName = m.Group.Name,
                                                    Role = m.GroupRole,
                                                    Status = m.GroupMemberStatus,
                                                    StartDate = m.CreatedDateTime
                                                } ).ToList();

                    if ( coachingGroups.Count == 0 )
                    {
                        stepsBarResult.CoachingResult.IsCoaching = false;
                    }
                    else
                    {
                        stepsBarResult.CoachingResult.IsCoaching = true;
                        stepsBarResult.CoachingResult.CoachingSince = DateTime.MaxValue;

                        foreach ( var group in coachingGroups )
                        {
                            GroupMemberSummary groupMemberSummary = new GroupMemberSummary();
                            groupMemberSummary.GroupId = group.GroupId;
                            groupMemberSummary.GroupName = group.GroupName;
                            groupMemberSummary.Role = group.Role.Name;
                            groupMemberSummary.RoleId = group.Role.Id;
                            stepsBarResult.CoachingResult.Groups.Add( groupMemberSummary );

                            if ( group.StartDate < stepsBarResult.CoachingResult.CoachingSince )
                            {
                                stepsBarResult.CoachingResult.CoachingSince = group.StartDate;
                            }
                        }
                    }
                }


            }

            return stepsBarResult;
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

        /// <summary>
        /// Steps Result 
        /// </summary>
        public class StepsBarResult
        {
            /// <summary>
            /// Gets or sets the baptism result.
            /// </summary>
            /// <value>
            /// The baptism result.
            /// </value>
            public BaptismResult BaptismResult { get; set; }
            /// <summary>
            /// Gets or sets the membership result.
            /// </summary>
            /// <value>
            /// The membership result.
            /// </value>
            public MembershipResult MembershipResult { get; set; }
            /// <summary>
            /// Gets or sets a value indicating whether this instance is worshipper.
            /// </summary>
            /// <value>
            /// <c>true</c> if this instance is worshipper; otherwise, <c>false</c>.
            /// </value>
            public bool IsWorshipper { get; set; }
            /// <summary>
            /// Gets or sets the connection result.
            /// </summary>
            /// <value>
            /// The connection result.
            /// </value>
            public ConnectionResult ConnectionResult { get; set; }
            /// <summary>
            /// Gets or sets a value indicating whether this instance is tithing.
            /// </summary>
            /// <value>
            /// <c>true</c> if this instance is tithing; otherwise, <c>false</c>.
            /// </value>
            public bool IsTithing { get; set; }
            /// <summary>
            /// Gets or sets the serving result.
            /// </summary>
            /// <value>
            /// The serving result.
            /// </value>
            public ServingResult ServingResult { get; set;}
            /// <summary>
            /// Gets or sets the coaching result.
            /// </summary>
            /// <value>
            /// The coaching result.
            /// </value>
            public CoachingResult CoachingResult { get; set; }
        }

        /// <summary>
        /// Membership Date
        /// </summary>
        public class MembershipResult
        {
            /// <summary>
            /// Gets or sets a value indicating whether this instance is member.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is member; otherwise, <c>false</c>.
            /// </value>
            public bool IsMember { get; set; }
            /// <summary>
            /// Gets or sets the membership date.
            /// </summary>
            /// <value>
            /// The membership date.
            /// </value>
            public DateTime? MembershipDate { get; set; }
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

            /// <summary>
            /// Gets or sets the coaching since.
            /// </summary>
            /// <value>
            /// The coaching since.
            /// </value>
            public DateTime? CoachingSince { get; set; }
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
            public List<GroupMemberSummary> Groups { get; set; }

            /// <summary>
            /// Gets or sets the serving since.
            /// </summary>
            /// <value>
            /// The serving since.
            /// </value>
            public DateTime? ServingSince { get; set; }
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

            /// <summary>
            /// Gets or sets the connected since.
            /// </summary>
            /// <value>
            /// The connected since.
            /// </value>
            public DateTime? ConnectedSince { get; set; }
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
            public int? RegistrationGroupId { get; set; }
            /// <summary>
            /// Gets or sets the baptism registration date.
            /// </summary>
            /// <value>
            /// The baptism registration date.
            /// </value>
            public DateTime? BaptismRegistrationDate { get; set; }
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

using church.ccv.Actions;
using church.ccv.Steps.Model;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace church.ccv.Badges.NextSteps
{
    public partial class NextStepsBadgesController : Rock.Rest.ApiControllerBase
    {
        // The ID for the Baptism group type. This will never change within CCV.
        const int Baptism_GroupType_Id = 58;
        
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/CCV/Badges/StepsTaken/{personGuid}" )]
        public StepsTaken GetStepsTaken( Guid personGuid )
        {
            var stepsTaken = new StepsTaken();

            using ( RockContext rockContext = new RockContext() )
            {
                int currentYear = RockDateTime.Now.Year;
                DateTime dateYearAgo = RockDateTime.Now.AddDays( -364 ); // technically we want 52 weeks 52*7 = 364

                StepTakenService stepTakenService = new StepTakenService(rockContext);

                stepsTaken.StepsThisYear = stepTakenService.Queryable()
                                                .Where( t =>
                                                     t.PersonAlias.Person.Guid == personGuid
                                                     && t.DateTaken.Year == currentYear )
                                                .Count();

                stepsTaken.StepsIn52Weeks = stepTakenService.Queryable()
                                                .Where( t =>
                                                     t.PersonAlias.Person.Guid == personGuid
                                                     && t.DateTaken >= dateYearAgo )
                                                .Count();
            }

            return stepsTaken;
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
                // make sure to only take unique IDs, as it's possible for a person to be in a group multiple times.
                var groupMembers = new GroupMemberService( rockContext ).Queryable()
                    .Where( m => m.Group.Guid == groupGuid )
                    .Select( m => new { Guid = m.Person.Guid, Id = m.PersonId } )
                    .DistinctBy( m => m.Id );

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
                    // we need to know if they're an adult or not. It affects which Actions we call, and how the UI renders.
                    bool isAdult = new PersonService( rockContext ).GetAllAdults( ).Where( p => p.Guid == personGuid ).Count( ) > 0 ? true : false;
                    stepsBarResult.IsAdult = isAdult;

                    person.LoadAttributes();
                    
                    // first, call all the actions for either an adult or student/child and store the results
                    bool isERA = false;
                    bool isGiving = false;

                    DateTime? baptismDate = null;
                    bool isBaptised = false;

                    List<int> peerLearningGroups = null;
                    bool isPeerLearning = false;

                    List<int> servingGroups = null;
                    bool isServing = false;

                    List<int> teachingGroups = null;
                    bool isTeaching = false;

                    List<int> sharedStoryIds = null;
                    bool sharedStory = false;

                    // handle adults
                    if( isAdult == true )
                    {
                        isERA = Actions_Adult.ERA.IsERA( person.Id );
                        isGiving = Actions_Adult.Give.IsGiving( person.Id );
                        isBaptised = Actions_Adult.Baptised.IsBaptised( person.Id, out baptismDate );

                        // Peer Learning
                        Actions_Adult.PeerLearning.Result peerLearningResult;
                        Actions_Adult.PeerLearning.IsPeerLearning( person.Id, out peerLearningResult );

                        isPeerLearning = peerLearningResult.IsPeerLearning;
                        peerLearningGroups = peerLearningResult.GroupIds;

                                                
                        // Serving
                        Actions_Adult.Serving.Result servingResult;
                        Actions_Adult.Serving.IsServing( person.Id, out servingResult );

                        isServing = servingResult.IsServing;
                        servingGroups = servingResult.GroupIds;


                        // Teaching
                        Actions_Adult.Teaching.Result teachingResult;
                        Actions_Adult.Teaching.IsTeaching( person.Id, out teachingResult );

                        isTeaching = teachingResult.IsTeaching;
                        teachingGroups = teachingResult.GroupIds;


                        // Shared Story
                        sharedStory = Actions_Adult.ShareStory.SharedStory( person.Id, out sharedStoryIds );
                    }
                    else
                    {
                        isERA = Actions_Student.ERA.IsERA( person.Id );
                        isGiving = Actions_Student.Give.IsGiving( person.Id );
                        isBaptised = Actions_Student.Baptised.IsBaptised( person.Id, out baptismDate );


                        // Peer Learning
                        Actions_Student.PeerLearning.Result peerLearningResult;
                        Actions_Student.PeerLearning.IsPeerLearning( person.Id, out peerLearningResult );

                        isPeerLearning = peerLearningResult.IsPeerLearning;
                        peerLearningGroups = peerLearningResult.GroupIds;

                                                
                        // Serving
                        Actions_Student.Serving.Result servingResult;
                        Actions_Student.Serving.IsServing( person.Id, out servingResult );

                        isServing = servingResult.IsServing;
                        servingGroups = servingResult.GroupIds;


                        // Teaching
                        Actions_Student.Teaching.Result teachingResult;
                        Actions_Student.Teaching.IsTeaching( person.Id, out teachingResult );

                        isTeaching = teachingResult.IsTeaching;
                        teachingGroups = teachingResult.GroupIds;

                        // Shared Story
                        sharedStory = Actions_Student.ShareStory.SharedStory( person.Id, out sharedStoryIds );
                    }

                    // Worshipping
                    stepsBarResult.IsWorshipper = isERA;

                    // Tithing
                    stepsBarResult.IsTithing = isGiving;

                    // Baptism
                    // Use the baptismDate's existence to know how to shade the icon
                    stepsBarResult.BaptismResult = new BaptismResult();
                    if( baptismDate != null )
                    {
                        stepsBarResult.BaptismResult.BaptismStatus = BaptismStatus.Baptised;

                        stepsBarResult.BaptismResult.BaptismDate = baptismDate;
                        stepsBarResult.BaptismResult.BaptismDateFormatted = baptismDate.Value.ToShortDateString();
                    }
                    else
                    {
                        // check if registered for baptism
                        var baptismGroups = new GroupMemberService( rockContext ).Queryable()
                                                        .Where( m => Baptism_GroupType_Id == m.Group.GroupTypeId
                                                             && m.GroupMemberStatus == GroupMemberStatus.Active
                                                             && m.PersonId == person.Id )
                                                        .Select( m => m.GroupId ).ToList();

                        if ( baptismGroups.Count > 0 )
                        {
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


                    // Connected
                    bool connected = isPeerLearning;

                    stepsBarResult.ConnectionResult = new ConnectionResult();
                    stepsBarResult.ConnectionResult.Groups = new List<GroupMemberSummary>();

                    // use the result to show the badge as either empty, shaded, or filled
                    if( connected == false )
                    {
                        stepsBarResult.ConnectionResult.ConnectionStatus = ConnectionStatus.NotInGroup;
                    }
                    else
                    {
                        stepsBarResult.ConnectionResult.ConnectedSince = DateTime.MaxValue;

                        // get the groupMember entities for each group the person's in
                        // create an anonymous class storing the data we'll shove to the badge
                        var groupMemberList = new GroupMemberService( rockContext ).Queryable()
                                                    .Where( m => peerLearningGroups.Contains(m.Group.Id) && m.PersonId == person.Id )
                                                    .Select(m => new
                                                    {
                                                        GroupId = m.GroupId,
                                                        GroupName = m.Group.Name,
                                                        Role = m.GroupRole,
                                                        Status = m.GroupMemberStatus,
                                                        StartDate = m.CreatedDateTime
                                                    } ).ToList();

                        foreach ( var groupMember in groupMemberList )
                        {
                            GroupMemberSummary groupMemberSummary = new GroupMemberSummary();
                            groupMemberSummary.GroupId = groupMember.GroupId;
                            groupMemberSummary.GroupName = groupMember.GroupName;
                            groupMemberSummary.Role = groupMember.Role.Name;
                            groupMemberSummary.RoleId = groupMember.Role.Id;
                            stepsBarResult.ConnectionResult.Groups.Add( groupMemberSummary );

                            if ( groupMember.StartDate < stepsBarResult.ConnectionResult.ConnectedSince )
                            {
                                stepsBarResult.ConnectionResult.ConnectedSince = groupMember.StartDate;
                            }
                        }

                        if ( groupMemberList.Any( m => m.Status == GroupMemberStatus.Active ))
                        {
                            stepsBarResult.ConnectionResult.ConnectionStatus = ConnectionStatus.InGroup;

                            if ( groupMemberList.Any( m => m.Role.IsLeader ) )
                            {
                                stepsBarResult.ConnectionResult.IsLeader = true;
                            }
                        }
                        else
                        {
                            stepsBarResult.ConnectionResult.ConnectionStatus = ConnectionStatus.PendingInGroup;
                        }
                    }
                    

                    // Serving
                    bool serving = isServing;

                    // use the result to show the badge as either empty, shaded, or filled
                    stepsBarResult.ServingResult = new ServingResult();
                    stepsBarResult.ServingResult.Groups = new List<GroupMemberSummary>();

                    if( serving == false )
                    {
                        stepsBarResult.ServingResult.ServingStatus = ServingStatus.NotServing;
                    }
                    else
                    {
                        // get the groupMember entities for each group the person's in
                        // create an anonymous class storing the data we'll shove to the badge
                        var groupMemberList = new GroupMemberService( rockContext ).Queryable()
                                                    .Where( m => servingGroups.Contains(m.Group.Id) && m.PersonId == person.Id )
                                                    .Select( m => new
                                                    {
                                                        GroupId = m.GroupId,
                                                        GroupName = m.Group.Name,
                                                        Role = m.GroupRole,
                                                        Status = m.GroupMemberStatus,
                                                        StartDate = m.CreatedDateTime
                                                    } ).ToList();

                        
                        // now see if this person should be set to 'pending' or not.
                        // they should only be pending if they aren't active in ANY of their groups.
                        int numActiveGroups = groupMemberList.Where(sg => sg.Status == GroupMemberStatus.Active).Count();

                        stepsBarResult.ServingResult.ServingStatus = numActiveGroups > 0 ? ServingStatus.Serving : ServingStatus.PendingServing;
                        stepsBarResult.ServingResult.ServingSince = DateTime.MaxValue;

                        foreach( var group in groupMemberList )
                        {
                            GroupMemberSummary groupMemberSummary = new GroupMemberSummary();
                            groupMemberSummary.GroupId = group.GroupId;
                            groupMemberSummary.GroupName = group.GroupName;
                            groupMemberSummary.Role = group.Role.Name;
                            groupMemberSummary.RoleId = group.Role.Id;
                            stepsBarResult.ServingResult.Groups.Add(groupMemberSummary);

                            if(group.StartDate < stepsBarResult.ServingResult.ServingSince)
                            {
                                stepsBarResult.ServingResult.ServingSince = group.StartDate;
                            }
                        }
                    }


                    // coaching
                    bool coaching = isTeaching;

                    stepsBarResult.CoachingResult = new CoachingResult();
                    stepsBarResult.CoachingResult.Groups = new List<GroupMemberSummary>();
                    
                    if( coaching == false )
                    {
                        stepsBarResult.CoachingResult.IsCoaching = false;
                    }
                    else
                    {
                        // get the groupMember entities for each group the person's in
                        // create an anonymous class storing the data we'll shove to the badge
                        var groupMemberList = new GroupMemberService( rockContext ).Queryable()
                                                    .Where( m => teachingGroups.Contains(m.Group.Id) && m.PersonId == person.Id )
                                                    .Select( m => new
                                                    {
                                                        GroupId = m.GroupId,
                                                        GroupName = m.Group.Name,
                                                        Role = m.GroupRole,
                                                        Status = m.GroupMemberStatus,
                                                        StartDate = m.CreatedDateTime
                                                    } ).ToList();

                        stepsBarResult.CoachingResult.IsCoaching = true;
                        stepsBarResult.CoachingResult.CoachingSince = DateTime.MaxValue;

                        foreach ( var group in groupMemberList )
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

                    // shared story
                    stepsBarResult.SharedStoryResult = new ShareStoryResult();
                    stepsBarResult.SharedStoryResult.SharedStory = sharedStory;
                    stepsBarResult.SharedStoryResult.SharedStoryIds = sharedStoryIds;
                }
            }

            return stepsBarResult;
        }
        
        /// <summary>
        /// The number of steps a person has taken.
        /// </summary>
        public class StepsTaken
        {
            /// <summary>
            /// Gets or sets the steps this year.
            /// </summary>
            /// <value>
            /// The steps this year.
            /// </value>
            public int StepsThisYear { get; set; }
            /// <summary>
            /// Gets or sets the steps in52 weeks.
            /// </summary>
            /// <value>
            /// The steps in52 weeks.
            /// </value>
            public int StepsIn52Weeks { get; set; }
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

            public ShareStoryResult SharedStoryResult { get; set; }

            public bool IsAdult { get; set; }
        }
        
        public class ShareStoryResult
        {
            public bool SharedStory { get; set; }
            public List<int> SharedStoryIds { get; set; }
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
            /// Stores the state of serving (Not, Pending, or Is)
            /// </summary>
            /// <value>
            public ServingStatus ServingStatus { get; set; }

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
        /// Connection Status
        /// </summary>
        public enum ServingStatus
        {
            NotServing,
            PendingServing,
            Serving
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
            /// Gets or sets the baptism date formatted.
            /// </summary>
            /// <value>
            /// The baptism date formatted.
            /// </value>
            public string BaptismDateFormatted { get; set; }
            
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

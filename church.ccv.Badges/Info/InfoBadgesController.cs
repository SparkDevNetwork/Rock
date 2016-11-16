using church.ccv.Actions;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace church.ccv.Badges.Bio
{
    public partial class InfoBadgesController : Rock.Rest.ApiControllerBase
    {
        // The ID for the Starting Point group type. This will never change within CCV.
        const int StartingPoint_GroupType_Id = 57;

        /// <summary>
        /// Returns the result of a person's membership
        /// </summary>
        /// <param name="personId">The person id.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/CCV/Badges/IsMember/{personId}" )]
        public IsMemberResult IsMember( int personId )
        {
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );
            
            Person person = personService.Queryable( ).Where( p => p.Id == personId ).SingleOrDefault( );
            bool isAdult = new PersonService( rockContext ).GetAllAdults( ).Where( p => p.Id == personId ).Count( ) > 0 ? true : false;

            IsMemberResult result = new IsMemberResult( );
            if ( person != null )
            {
                DateTime? membershipDate = null;
                if ( isAdult )
                {
                    Actions_Adult.Member.IsMember( person.Id, out membershipDate );
                }
                else
                {
                    Actions_Student.Member.IsMember( person.Id, out membershipDate );
                }
                
                result.MembershipDate = membershipDate;
            }
            
            return result;
        }
        
        public class IsMemberResult
        {
            public DateTime? MembershipDate { get; set; }
        }


        /// <summary>
        /// Given a groupID, returns the Starting Point status of each person in the group.
        /// </summary>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/CCV/Badges/TakenStartingPointGroup/{groupGuid}" )]
        public Dictionary<int, TakenStartingPointResult> TakenStartingPointGroup( Guid groupGuid )
        {
            Dictionary<int, TakenStartingPointResult> groupResults = new Dictionary<int, TakenStartingPointResult>();

            using ( RockContext rockContext = new RockContext() )
            {
                // make sure to only take unique IDs, as it's possible for a person to be in a group multiple times.
                var groupMembers = new GroupMemberService( rockContext ).Queryable()
                    .Where( m => m.Group.Guid == groupGuid )
                    .Select( m => new { Guid = m.Person.Guid, Id = m.PersonId } )
                    .DistinctBy( m => m.Id );

                foreach ( var groupMember in groupMembers )
                {
                    groupResults.Add( groupMember.Id, GetStartingPointResult( groupMember.Id ) );
                }
            }

            return groupResults;
        }

        /// <summary>
        /// Returns the result of a person's starting point state
        /// </summary>
        /// <param name="personId">The person id.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/CCV/Badges/TakenStartingPoint/{personId}" )]
        public TakenStartingPointResult TakenStartingPoint( int personId )
        {
            return GetStartingPointResult( personId );
        }

        private TakenStartingPointResult GetStartingPointResult( int personId )
        {
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );

            // setup the return result. We'll assume they've never taken SP, nor are they registered.
            TakenStartingPointResult result = new TakenStartingPointResult( );
            result.Status = StartingPointStatus.NotTaken;

            Person person = personService.Queryable( ).Where( p => p.Id == personId ).SingleOrDefault( );
            if ( person != null )
            {
                bool isAdult = new PersonService( rockContext ).GetAllAdults( ).Where( p => p.Id == personId ).Count( ) > 0 ? true : false;

                DateTime? startingPointDate = null;
                if ( isAdult )
                {
                    Actions_Adult.StartingPoint.TakenStartingPoint( person.Id, out startingPointDate );
                }
                else
                {
                    Actions_Student.StartingPoint.TakenStartingPoint( person.Id, out startingPointDate );
                }
                
                
                // if there's a starting point date, they've taken it. simple.
                if( startingPointDate != null )
                {
                    result.Status = StartingPointStatus.Taken;

                    result.Date = startingPointDate;
                    result.DateFormatted = startingPointDate.Value.ToShortDateString( );
                }
                else
                {
                    // they have not taken starting point. Check to see if they're in a group with a future date.
                    // That would mean they've registered for a class.

                    // get all the starting point groups they're in. Note that some of these could be in the past, if they registered and never showed up.
                    var startingPointGroups = new GroupMemberService( rockContext ).Queryable()
                                                    .Where( m => m.Group.GroupTypeId == StartingPoint_GroupType_Id
                                                            && m.GroupMemberStatus == GroupMemberStatus.Active
                                                            && m.PersonId == person.Id )
                                                    .Select( m => m.GroupId ).ToList();

                    if( startingPointGroups.Count > 0 )
                    {
                        var startingPointEventItems = new EventItemOccurrenceService( rockContext ).Queryable( "Schedule,Linkages" )
                                                .Where( e => e.Linkages.Any( l => l.GroupId.HasValue && startingPointGroups.Contains( l.GroupId.Value ) ) )
                                                .ToList();

                        bool futureStartingPointScheduled = false;
                        int? futureStartingPointGroupId = null;
                        DateTime? futureStartingPointDate = null;

                        // see if any of the starting point groups they're in are in the future.
                        foreach( EventItemOccurrence startingPointEventItem in startingPointEventItems )
                        {
                            // if so, they're registered.
                            if( startingPointEventItem.NextStartDateTime >= RockDateTime.Now )
                            {
                                futureStartingPointScheduled = true;
                                futureStartingPointGroupId = startingPointEventItem.Linkages.First().GroupId;
                                futureStartingPointDate = startingPointEventItem.NextStartDateTime;
                                break;
                            }
                        }

                        if( futureStartingPointScheduled )
                        {
                            result.RegistrationGroupId = futureStartingPointGroupId;

                            result.Date = futureStartingPointDate;
                            result.DateFormatted = futureStartingPointDate.Value.ToShortDateString( );

                            result.Status = StartingPointStatus.Registered;
                        }
                    }
                }
            }
            
            return result;
        }
        
        public class TakenStartingPointResult
        {
            public StartingPointStatus Status { get; set; }
            
            // this is either the date they took starting point, or the date they're registered to take it.
            public DateTime? Date { get; set; }
            public string DateFormatted { get; set; }

            // If registered for Starting Point, this is the group they're registered in.
            public int? RegistrationGroupId { get; set; }
        }

        public enum StartingPointStatus
        {
            NotTaken,
            Taken,
            Registered
        }
    }
}

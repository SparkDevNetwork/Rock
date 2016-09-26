using church.ccv.Actions;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using System;
using System.Linq;
using System.Web.Http;

namespace church.ccv.Badges.Bio
{
    public partial class InfoBadgesController : Rock.Rest.ApiControllerBase
    {
        /// <summary>
        /// Returns campuses with leader of a given person
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
        

        /// <summary>
        /// Campus and Leader info
        /// </summary>
        public class IsMemberResult
        {
            /// <summary>
            /// Gets or sets the name of the campus.
            /// </summary>
            /// <value>
            /// The name of the campus.
            /// </value>
            public DateTime? MembershipDate { get; set; }
        }
    }
}

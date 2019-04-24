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

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.OData;

using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public partial class PrayerRequestsController
    {
        /// <summary>
        /// Queryable GET of PrayerRequest records that Public, Active, Approved and not expired
        /// </summary>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [EnableQuery]
        [System.Web.Http.Route( "api/PrayerRequests/Public" )]
        public IQueryable<PrayerRequest> Public()
        {
            var now = RockDateTime.Now;
            return base.Get()
                .Where( p =>
                    ( p.IsActive.HasValue && p.IsActive.Value == true ) &&
                    ( p.IsPublic.HasValue && p.IsPublic.Value == true ) &&
                    ( p.IsApproved.HasValue && p.IsApproved == true ) &&
                    ( !p.ExpirationDate.HasValue || p.ExpirationDate.Value > now )
                );
        }

        /// <summary>
        /// Queryable GET of Prayer Requests for the specified top-level category
        /// Prayer Requests that are in categories that are decendents of the specified category will also be included
        /// </summary>
        /// <param name="categoryId">The category identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [EnableQuery]
        [System.Web.Http.Route( "api/PrayerRequests/GetByCategory/{categoryId}" )]
        public IQueryable<PrayerRequest> GetByCategory( int categoryId )
        {
            var rockContext = ( this.Service.Context as RockContext ) ?? new RockContext();
            var decendentsCategoriesQry = new CategoryService( rockContext ).GetAllDescendents( categoryId ).Select( a => a.Id );
            return this.Get().Where( a => a.CategoryId.HasValue ).Where( a => decendentsCategoriesQry.Contains( a.CategoryId.Value ) || ( a.CategoryId.Value == categoryId ) );
        }

        /// <summary>
        /// Increment the prayer count for a prayer request
        /// </summary>
        /// <param name="prayerId">The prayer identifier.</param>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <exception cref="System.Web.Http.OData.IEdmEntityObject"></exception>
        [Authenticate, Secured]
        [HttpPut]
        [System.Web.Http.Route( "api/PrayerRequests/Prayed/{prayerId}/{personAliasId}" )]
        public virtual void Prayed( int prayerId, int personAliasId )
        {
            SetProxyCreation( true );

            PrayerRequest prayerRequest;
            if ( !Service.TryGet( prayerId, out prayerRequest ) )
            {
                throw new HttpResponseException( HttpStatusCode.NotFound );
            }

            prayerRequest.PrayerCount = ( prayerRequest.PrayerCount ?? 0 ) + 1;

            System.Web.HttpContext.Current.Items.Add( "CurrentPerson", GetPerson() );
            Service.Context.SaveChanges();
        }


        /// <summary>
        /// Flags the specified prayer request.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <exception cref="System.Web.Http.OData.IEdmEntityObject"></exception>
        [Authenticate, Secured]
        [HttpPut]
        [System.Web.Http.Route( "api/PrayerRequests/Flag/{id}" )]
        public virtual void Flag( int id )
        {
            SetProxyCreation( true );

            PrayerRequest prayerRequest;
            if ( !Service.TryGet( id, out prayerRequest ) )
            {
                throw new HttpResponseException( HttpStatusCode.NotFound );
            }

            prayerRequest.FlagCount = ( prayerRequest.FlagCount ?? 0 ) + 1;
            if ( prayerRequest.FlagCount >= 1 )
            {
                prayerRequest.IsApproved = false;
            }

            Service.Context.SaveChanges();
        }
        
        /// <summary>
        /// Increment the prayer count for a prayer request
        /// </summary>
        /// <param name="id">The prayer identifier.</param>
        /// <exception cref="System.Web.Http.OData.IEdmEntityObject"></exception>
        [Authenticate, Secured]
        [HttpPut]
        [System.Web.Http.Route( "api/PrayerRequests/Prayed/{id}" )]
        public virtual void Prayed( int id )
        {
            SetProxyCreation( true );

            PrayerRequest prayerRequest;
            if ( !Service.TryGet( id, out prayerRequest ) )
            {
                throw new HttpResponseException( HttpStatusCode.NotFound );
            }

            prayerRequest.PrayerCount = ( prayerRequest.PrayerCount ?? 0 ) + 1;

            Service.Context.SaveChanges();
        }

        /// <summary>
        /// Get the prayer requests of every member of a certain set of groups that a person belongs to
        /// </summary>
        /// <param name="excludePerson">Exclude the logged in person prayers</param>
        /// <param name="groupTypeIds">A list of group type ids</param>
        /// <param name="personId">The id of the person to pull group prayers for</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/PrayerRequests/GetForGroupMembersOfPersonInGroupTypes/{personId}" )]
        public IQueryable<PrayerRequest> GetForGroupMembersOfPersonInGroupTypes( bool excludePerson, string groupTypeIds, int personId )
        {
            RockContext rockContext = new RockContext();
            System.DateTime now = RockDateTime.Now;

            // Turn the comma separated list of groupTypeIds into a list of strings.
            List<int> groupTypeIdsList = ( groupTypeIds ?? "" ).Split( ',' ).AsIntegerList();

            GroupMemberService groupMemberService = new GroupMemberService( rockContext );

            IQueryable<int> groupMemberPersonAliasList = groupMemberService.GetByPersonId( personId )   // Get the groups that a person is a part of
                .Where( gm =>
                    groupTypeIdsList.Contains( gm.Group.GroupTypeId ) &&    // Filter those groups by a set of passed in group types. 
                    gm.Group.IsActive == true && gm.Group.IsArchived == false   // Also make sure the groups are active and not archived.
                 )
                .SelectMany( gm => gm.Group.Members )   // Get the members of those groups
                .Where( gm => gm.GroupMemberStatus == GroupMemberStatus.Active && gm.IsArchived == false ) // Make sure that the group members are active and haven't been archived
                .Select( m => m.Person.Aliases.FirstOrDefault().Id );   // Return the person alias ids

            // Get the prayers for the people.
            PrayerRequestService prayerRequestService = new PrayerRequestService( rockContext );
            IQueryable<PrayerRequest> prayerRequestList = prayerRequestService.Queryable()
                .Where( pr =>
                    ( groupMemberPersonAliasList.Contains( (int)pr.RequestedByPersonAliasId ) ) &&
                    ( pr.IsActive.HasValue && pr.IsActive.Value == true ) &&
                    ( pr.IsPublic.HasValue && pr.IsPublic.Value == true ) &&
                    ( pr.IsApproved.HasValue && pr.IsApproved == true ) &&
                    ( !pr.ExpirationDate.HasValue || pr.ExpirationDate.Value > now )
                );

            // Filter out the current persons prayers if excludePerson is true
            if ( excludePerson ) { 
                prayerRequestList = prayerRequestList.Where( pr => pr.RequestedByPersonAlias.PersonId != personId );
            }

            //Return this as a queryable.
            return prayerRequestList;
        }
    }
}

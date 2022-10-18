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
using System.Net.Http;
using System.Web.Http;

using Microsoft.AspNet.OData;

using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;

namespace Rock.Rest.Controllers
{
    public partial class GroupMembersController
    {
        /// <summary>
        /// Overrides base Get controller method to include deceased GroupMembers
        /// </summary>
        /// <returns>A queryable collection of GroupMembers, including deceased, that match the provided query.</returns>
        [Authenticate, Secured]
        [RockEnableQuery]
        public override IQueryable<GroupMember> Get()
        {
            var queryString = Request.RequestUri.Query;
            var includeDeceased = System.Web.HttpUtility.ParseQueryString( queryString ).Get( "IncludeDeceased" );

            if ( includeDeceased.AsBoolean( false ) )
            {
                var rockContext = new Rock.Data.RockContext();
                return new GroupMemberService( rockContext ).Queryable( true );
            }
            else
            {
                return base.Get();
            }
        }

        /// <summary>
        /// Gets the group placement group members.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/GroupMembers/GetGroupPlacementGroupMembers" )]
        [HttpPost]
        [Rock.SystemGuid.RestActionGuid( "6E5F85FB-5D43-4C3A-96C0-0D94D347A6FE" )]
        public IEnumerable<GroupPlacementGroupMember> GetGroupPlacementGroupMembers( [FromBody] GetGroupPlacementGroupMembersParameters options )
        {
            var rockContext = new RockContext();
            var groupMemberService = new GroupMemberService( rockContext );
            return groupMemberService.GetGroupPlacementGroupMembers( options, this.GetPerson() );
        }

        /// <summary>
        /// Creates the known relationship.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="relatedPersonId">The related person identifier.</param>
        /// <param name="relationshipRoleId">The relationship role identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "api/GroupMembers/KnownRelationship" )]
        [Rock.SystemGuid.RestActionGuid( "018496A4-F8C3-473E-BEBB-2236497AFB1C" )]
        public System.Net.Http.HttpResponseMessage CreateKnownRelationship( int personId, int relatedPersonId, int relationshipRoleId )
        {
            SetProxyCreation( true );
            var rockContext = this.Service.Context as RockContext;
            var personService = new PersonService( rockContext );
            var person = personService.Get( personId );
            var relatedPerson = personService.Get( relatedPersonId );

            CheckCanEdit( person );
            CheckCanEdit( relatedPerson );

            System.Web.HttpContext.Current.AddOrReplaceItem( "CurrentPerson", GetPerson() );

            var groupMemberService = new GroupMemberService( rockContext );
            groupMemberService.CreateKnownRelationship( personId, relatedPersonId, relationshipRoleId );

            return ControllerContext.Request.CreateResponse( HttpStatusCode.Created );
        }

        /// <summary>
        /// Gets the known relationship.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="relationshipRoleId">The relationship role identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/GroupMembers/KnownRelationship" )]
        [Rock.SystemGuid.RestActionGuid( "51CB49B5-4338-4826-8DFC-2F902E30A01C" )]
        public IQueryable<GroupMember> GetKnownRelationship( int personId, int relationshipRoleId )
        {
            SetProxyCreation( true );
            var rockContext = this.Service.Context as RockContext;

            var groupMemberService = new GroupMemberService( rockContext );
            var groupMembers = groupMemberService.GetKnownRelationship( personId, relationshipRoleId );

            return groupMembers;
        }

        /// <summary>
        /// Deletes the known relationship.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="relatedPersonId">The related person identifier.</param>
        /// <param name="relationshipRoleId">The relationship role identifier.</param>
        [Authenticate, Secured]
        [HttpDelete]
        [System.Web.Http.Route( "api/GroupMembers/KnownRelationship" )]
        [Rock.SystemGuid.RestActionGuid( "F5FC602F-AE95-4853-9BCA-16BE0FBBCAFC" )]
        public void DeleteKnownRelationship( int personId, int relatedPersonId, int relationshipRoleId )
        {
            SetProxyCreation( true );
            var rockContext = this.Service.Context as RockContext;
            var personService = new PersonService( rockContext );
            var person = personService.Get( personId );
            var relatedPerson = personService.Get( relatedPersonId );

            CheckCanEdit( person );
            CheckCanEdit( relatedPerson );

            System.Web.HttpContext.Current.AddOrReplaceItem( "CurrentPerson", GetPerson() );

            var groupMemberService = new GroupMemberService( rockContext );
            groupMemberService.DeleteKnownRelationship( personId, relatedPersonId, relationshipRoleId );
        }
    }
}
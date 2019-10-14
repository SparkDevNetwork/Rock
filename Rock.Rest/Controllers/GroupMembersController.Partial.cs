﻿// <copyright>
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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.OData;

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
        [EnableQuery]
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
        /// Creates the known relationship.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="relatedPersonId">The related person identifier.</param>
        /// <param name="relationshipRoleId">The relationship role identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "api/GroupMembers/KnownRelationship" )]
        public System.Net.Http.HttpResponseMessage CreateKnownRelationship( int personId, int relatedPersonId, int relationshipRoleId )
        {
            SetProxyCreation( true );
            var rockContext = this.Service.Context as RockContext;
            var personService = new PersonService(rockContext);
            var person = personService.Get(personId);
            var relatedPerson = personService.Get(relatedPersonId);

            CheckCanEdit( person );
            CheckCanEdit( relatedPerson );

            System.Web.HttpContext.Current.Items.Add( "CurrentPerson", GetPerson() );

            var groupMemberService = new GroupMemberService(rockContext);
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
        public void DeleteKnownRelationship( int personId, int relatedPersonId, int relationshipRoleId )
        {
            SetProxyCreation( true );
            var rockContext = this.Service.Context as RockContext;
            var personService = new PersonService( rockContext );
            var person = personService.Get( personId );
            var relatedPerson = personService.Get( relatedPersonId );

            CheckCanEdit( person );
            CheckCanEdit( relatedPerson );

            System.Web.HttpContext.Current.Items.Add( "CurrentPerson", GetPerson() );

            var groupMemberService = new GroupMemberService( rockContext );
            groupMemberService.DeleteKnownRelationship( personId, relatedPersonId, relationshipRoleId );
        }
    }
}

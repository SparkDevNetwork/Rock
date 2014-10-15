// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.Http;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web.Cache;

namespace Rock.Rest.Controllers
{
    public partial class GroupMembersController : IHasCustomRoutes
    {

        /// <summary>
        /// Adds custom routes for this controller to the route collection
        /// </summary>
        /// <param name="routes">The route collection</param>
        public void AddRoutes( System.Web.Routing.RouteCollection routes )
        {
            routes.MapHttpRoute(
                name: "GroupMembersGetIncludeDeceased",
                routeTemplate: "api/GroupMembers/IncludeDeceased/",
                defaults: new
                {
                    controller = "GroupMembers",
                    action = "GetIncludeDeceased"
                } );
        }

        [Authenticate, Secured]
        [Queryable( AllowedQueryOptions = System.Web.Http.OData.Query.AllowedQueryOptions.All )]
        [HttpGet]
        public IQueryable<GroupMember> GetIncludeDeceased()
        {
            var rockContext = new Rock.Data.RockContext();
            var groupMemberService = new GroupMemberService( rockContext );

            return groupMemberService.Queryable( true );
        }

    }
}

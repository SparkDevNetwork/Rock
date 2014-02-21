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
using System.Net;
using System.Web.Http;

using Rock.Model;
using Rock.Rest.Filters;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// Users REST API
    /// </summary>
    public partial class UserLoginsController : IHasCustomRoutes
    {
        public void AddRoutes( System.Web.Routing.RouteCollection routes )
        {
            routes.MapHttpRoute(
                name: "UsernameAvailable",
                routeTemplate: "api/userlogins/available/{username}",
                defaults: new
                {
                    controller = "userlogins",
                    action = "available"
                } );
        }

        /// <summary>
        /// Tests if a username is available
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        [HttpGet]
        public bool Available( string username )
        {
            return new UserLoginService().GetByUserName( username ) == null;
        }
    }
}

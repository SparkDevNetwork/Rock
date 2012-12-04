//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Net;
using System.Web.Http;
using Rock.Model;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// Users REST API
    /// </summary>
    public partial class UsersController : IHasCustomRoutes
    {
        public void AddRoutes( System.Web.Routing.RouteCollection routes )
        {
            routes.MapHttpRoute(
                name: "UsernameAvailable",
                routeTemplate: "api/users/available/{username}",
                defaults: new
                {
                    controller = "users",
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
            return new UserService().GetByUserName( username ) == null;
        }
    }
}

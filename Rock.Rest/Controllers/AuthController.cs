//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Net;
using System.Web.Http;
using Rock.Model;
using Rock.Security;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class AuthController : ApiController, IHasCustomRoutes
    {
        /// <summary>
        /// Adds the routes.
        /// </summary>
        /// <param name="routes">The routes.</param>
        public void AddRoutes( System.Web.Routing.RouteCollection routes )
        {
            routes.MapHttpRoute(
                name: "AuthLogin",
                routeTemplate: "api/Auth/Login",
                defaults: new
                {
                    controller = "Auth",
                    action = "Login"
                } );
        }

        /// <summary>
        /// Logins the specified login parameters.
        /// </summary>
        /// <param name="loginParameters">The login parameters.</param>
        /// <exception cref="System.Web.Http.HttpResponseException"></exception>
        [HttpPost]
        public void Login( [FromBody]LoginParameters loginParameters )
        {
            bool valid = false;

            var userLoginService = new UserLoginService();
            var userLogin = userLoginService.GetByUserName( loginParameters.Username );
            if ( userLogin != null && userLogin.ServiceType == AuthenticationServiceType.Internal )
            {
                foreach ( var serviceEntry in AuthenticationContainer.Instance.Components )
                {
                    var component = serviceEntry.Value.Value;
                    string componentName = component.GetType().FullName;

                    if (
                        userLogin.ServiceName == componentName &&
                        component.AttributeValues.ContainsKey( "Active" ) &&
                        bool.Parse( component.AttributeValues["Active"][0].Value )
                    )
                    {
                        if ( component.Authenticate( userLogin, loginParameters.Password ) )
                        {
                            valid = true;
                            Rock.Security.Authorization.SetAuthCookie( loginParameters.Username, false, false );
                        }
                    }
                }
            }

            if ( !valid )
            {
                throw new HttpResponseException( HttpStatusCode.Unauthorized );
            }
        }
    }
}

using System.Net;
using System.Web.Http;

namespace Rock.Security.Authentication.Auth0.Rest.Controllers
{
    /// NOTE: WebApi doesn't support Controllers with the Same Name, even if they have different namespaces, so can't call this AuthController
    public class Auth0Controller : ApiController
    {
        /// <summary>
        /// Use this to Login a user and return an AuthCookie which can be used in subsequent REST calls
        /// </summary>
        /// <param name="auth0UserInfo">The auth0 user information.</param>
        /// <exception cref="HttpResponseException"></exception>
        /// <exception cref="System.Web.Http.HttpResponseException"></exception>
        [HttpPost]
        [System.Web.Http.Route( "api/Auth/Auth0" )]
        public void Auth0Login( [FromBody]Rock.Security.Authentication.Auth0.Auth0Authentication.Auth0UserInfo auth0UserInfo )
        {
            string userName = Rock.Security.Authentication.Auth0.Auth0Authentication.GetAuth0UserName( auth0UserInfo );
            if ( !string.IsNullOrWhiteSpace( userName ) )
            {
                Rock.Security.Authorization.SetAuthCookie( userName, false, false );
            }
            else
            {
                throw new HttpResponseException( HttpStatusCode.Unauthorized );
            }
        }
    }
}

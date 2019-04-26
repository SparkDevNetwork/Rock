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
using System;
using System.Linq;
using System.Security.Principal;
using System.ServiceModel.Channels;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

using Rock.Model;

namespace Rock.Rest.Filters
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Web.Http.Filters.AuthorizationFilterAttribute" />
    public class AuthenticateAttribute : AuthorizationFilterAttribute
    {
        /// <summary>
        /// Calls when a process requests authorization.
        /// </summary>
        /// <param name="actionContext">The action context, which encapsulates information for using <see cref="T:System.Web.Http.Filters.AuthorizationFilterAttribute" />.</param>
        public override void OnAuthorization( HttpActionContext actionContext )
        {
            // See if user is logged in
            var principal = System.Threading.Thread.CurrentPrincipal;
            if ( principal != null && principal.Identity != null && !String.IsNullOrWhiteSpace(principal.Identity.Name))
            {
                //var userLoginService = new UserLoginService();
                //var user = userLoginService.GetByUserName(principal.Identity.Name);
                //if ( user != null )
                //{
                    actionContext.Request.SetUserPrincipal( principal );
                    return;
                //}
            }

            // If not, see if there's a valid token
            string authToken = null;
            if (actionContext.Request.Headers.Contains("Authorization-Token"))
                authToken = actionContext.Request.Headers.GetValues( "Authorization-Token" ).FirstOrDefault();
            if ( String.IsNullOrWhiteSpace( authToken ) )
            {
                string queryString = actionContext.Request.RequestUri.Query;
                authToken = System.Web.HttpUtility.ParseQueryString(queryString).Get("apikey");
            }

            if (! String.IsNullOrWhiteSpace( authToken ) )
            {
                var userLoginService = new UserLoginService( new Rock.Data.RockContext() );
                var userLogin = userLoginService.Queryable().Where( u => u.ApiKey == authToken ).FirstOrDefault();
                if ( userLogin != null )
                {
                    var identity = new GenericIdentity( userLogin.UserName );
                    principal = new GenericPrincipal(identity, null);
                    actionContext.Request.SetUserPrincipal( principal );
                    return;
                }
            }
        }
    }
}
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
using System.Net;
using System.Net.Http;
using System.ServiceModel.Channels;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Rest.Filters
{
    /// <summary>
    /// Checks to see if the Logged-In person has authorization View (HttpMethod: GET) or Edit (all other HttpMethods) for the RestController and Controller's associated EntityType
    /// </summary>
    public class SecuredAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Occurs before the action method is invoked.
        /// </summary>
        /// <param name="actionContext">The action context.</param>
        public override void OnActionExecuting( HttpActionContext actionContext )
        {
            var principal = actionContext.Request.GetUserPrincipal();
            Person person = null;

            if ( principal != null && principal.Identity != null )
            {
                using ( var rockContext = new RockContext() )
                {
                    string userName = principal.Identity.Name;
                    UserLogin userLogin = null;
                    if ( userName.StartsWith( "rckipid=" ) )
                    {
                        var personService = new PersonService( rockContext );
                        var impersonatedPerson = personService.GetByImpersonationToken( userName.Substring( 8 ) );
                        if ( impersonatedPerson != null )
                        {
                            userLogin = impersonatedPerson.GetImpersonatedUser();
                        }
                    }
                    else
                    {
                        var userLoginService = new UserLoginService( rockContext );
                        userLogin = userLoginService.GetByUserName( userName );
                    }

                    if ( userLogin != null )
                    {
                        person = userLogin.Person;
                        var pinAuthentication = AuthenticationContainer.GetComponent( typeof( Security.Authentication.PINAuthentication ).FullName );

                        // Don't allow PIN authentications.
                        if ( userLogin.EntityTypeId != null )
                        {
                            var userLoginEntityType = EntityTypeCache.Get( userLogin.EntityTypeId.Value );
                            if ( userLoginEntityType != null && userLoginEntityType.Id == pinAuthentication?.EntityType?.Id )
                            {
                                actionContext.Response = new HttpResponseMessage( HttpStatusCode.Unauthorized );
                                return;
                            }
                        }
                    }
                }
            }

            var reflectedHttpActionDescriptor = ( ReflectedHttpActionDescriptor ) actionContext.ActionDescriptor;

            var controller = actionContext.ActionDescriptor.ControllerDescriptor;
            var controllerClassName = controller.ControllerType.FullName;
            var actionMethod = actionContext.Request.Method.Method;

            var apiId = RestControllerService.GetApiId( reflectedHttpActionDescriptor.MethodInfo, actionMethod, controller.ControllerName, out Guid? restActionGuid );
            ISecured item;
            if ( restActionGuid.HasValue )
            {
                item = RestActionCache.Get( restActionGuid.Value );
            }
            else
            {
#pragma warning disable CS0618 // Type or member is obsolete
                item = RestActionCache.Get( apiId );
#pragma warning restore CS0618 // Type or member is obsolete
            }

            if ( item == null )
            {
                // if there isn't a RestAction in the database, use the Controller as the secured item
                item = RestControllerCache.Get( controllerClassName );
                if ( item == null )
                {
                    item = new RestController();
                }
            }

            if ( actionContext.Request.Properties.Keys.Contains( "Person" ) )
            {
                person = actionContext.Request.Properties["Person"] as Person;
            }
            else
            {
                actionContext.Request.Properties.Add( "Person", person );

                /* 12/12/2019 BJW
                 *
                 * Setting this current person item was only done in put, post, and patch in the ApiController
                 * class. Set it here so that it is always set for all methods, including delete. This enhances
                 * history logging done in the pre and post save model hooks (when the pre-save event is called
                 * we can access DbContext.GetCurrentPersonAlias and log who deleted the record).
                 *
                 * Task: https://app.asana.com/0/1120115219297347/1153140643799337/f
                 */
                System.Web.HttpContext.Current.AddOrReplaceItem( "CurrentPerson", person );
            }

            string action = actionMethod.Equals( "GET", StringComparison.OrdinalIgnoreCase ) ?
                Security.Authorization.VIEW : Security.Authorization.EDIT;

            bool authorized = false;

            if ( item.IsAuthorized( action, person ) )
            {
                authorized = true;
            }
            else if ( actionContext.Request.Headers.Contains( "X-Rock-App-Id" ) && actionContext.Request.Headers.Contains( "X-Rock-Mobile-Api-Key" ) )
            {
                // Normal authorization failed, but this is a Mobile App request so check
                // if the application itself has been given permission.
                var appId = actionContext.Request.Headers.GetValues( "X-Rock-App-Id" ).First().AsIntegerOrNull();
                var mobileApiKey = actionContext.Request.Headers.GetValues( "X-Rock-Mobile-Api-Key" ).First();

                if ( appId.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var appUser = Mobile.MobileHelper.GetMobileApplicationUser( appId.Value, mobileApiKey, rockContext );

                        if ( appUser != null && item.IsAuthorized( action, appUser.Person ) )
                        {
                            authorized = true;
                        }
                    }
                }
            }

            if ( !authorized )
            {
                actionContext.Response = new HttpResponseMessage( HttpStatusCode.Unauthorized );
            }
        }
    }
}
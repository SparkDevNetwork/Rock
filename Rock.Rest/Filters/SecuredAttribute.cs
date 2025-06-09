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

#if REVIEW_NET5_0_OR_GREATER
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
#endif

using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Rest.Filters
{
    /// <summary>
    /// Checks to see if the Logged-In person has authorization View (HttpMethod: GET) or Edit (all other HttpMethods) for the RestController and Controller's associated EntityType
    /// </summary>
#if REVIEW_NET5_0_OR_GREATER
    public class SecuredAttribute : System.Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync( ActionExecutingContext actionContext, ActionExecutionDelegate next )
#else
    public class SecuredAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// The security action that will be checked when authorizing the request.
        /// If this is null or an empty string then it will be automatically
        /// determined by the HTTP verb.
        /// </summary>
        public string SecurityAction { get; }

        /// <summary>
        /// Creates a new instance of <see cref="SecuredAttribute"/> that
        /// automatically detects the security action based on the HTTP verb
        /// of the request.
        /// </summary>
        public SecuredAttribute()
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="SecuredAttribute"/> that
        /// uses the specified security action when authorizing the request.
        /// </summary>
        /// <param name="securityAction">The security action such as VIEW or EDIT.</param>
        public SecuredAttribute( string securityAction )
        {
            SecurityAction = securityAction;
        }

        /// <summary>
        /// Occurs before the action method is invoked.
        /// </summary>
        /// <param name="actionContext">The action context.</param>
        public override void OnActionExecuting( HttpActionContext actionContext )
#endif
        {
#if REVIEW_NET5_0_OR_GREATER
            var principal = actionContext.HttpContext.User;
#else
            var principal = actionContext.Request.GetUserPrincipal();
#endif
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
#if REVIEW_NET5_0_OR_GREATER
                                actionContext.Result = new Microsoft.AspNetCore.Mvc.ChallengeResult();
#else
                                actionContext.Response = new HttpResponseMessage( HttpStatusCode.Unauthorized );
#endif
                                return;
                            }
                        }
                    }
                }
            }

#if REVIEW_NET5_0_OR_GREATER
            var reflectedHttpActionDescriptor = ( ControllerActionDescriptor ) actionContext.ActionDescriptor;

            var controller = reflectedHttpActionDescriptor;
            var controllerClassName = controller.ControllerTypeInfo.FullName;
            var actionMethod = actionContext.HttpContext.Request.Method;
#else
            var reflectedHttpActionDescriptor = ( ReflectedHttpActionDescriptor ) actionContext.ActionDescriptor;

            var controller = actionContext.ActionDescriptor.ControllerDescriptor;
            var controllerClassName = controller.ControllerType.FullName;
            var actionMethod = actionContext.Request.Method.Method;
#endif

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

#if REVIEW_NET5_0_OR_GREATER
            if ( actionContext.HttpContext.Items.ContainsKey( "Person" ) )
            {
                person = actionContext.HttpContext.Items["Person"] as Person;
            }
            else
            {
                actionContext.HttpContext.Items.Add( "Person", person );

                /* 12/12/2019 BJW
                 *
                 * Setting this current person item was only done in put, post, and patch in the ApiController
                 * class. Set it here so that it is always set for all methods, including delete. This enhances
                 * history logging done in the pre and post save model hooks (when the pre-save event is called
                 * we can access DbContext.GetCurrentPersonAlias and log who deleted the record).
                 *
                 * Task: https://app.asana.com/0/1120115219297347/1153140643799337/f
                 */
                actionContext.HttpContext.Items.AddOrReplace( "CurrentPerson", person );
            }
#else
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
#endif

            string action = SecurityAction;

            if ( action.IsNullOrWhiteSpace() )
            {
                action = actionMethod.Equals( "GET", StringComparison.OrdinalIgnoreCase ) ?
                    Security.Authorization.VIEW : Security.Authorization.EDIT;
            }

            bool authorized = false;

            if ( item.IsAuthorized( action, person ) )
            {
                authorized = true;
            }
#if REVIEW_NET5_0_OR_GREATER
            else if ( actionContext.HttpContext.Request.Headers.ContainsKey( "X-Rock-App-Id" ) && actionContext.HttpContext.Request.Headers.ContainsKey( "X-Rock-Mobile-Api-Key" ) )
#else
            else if ( actionContext.Request.Headers.Contains( "X-Rock-App-Id" ) && actionContext.Request.Headers.Contains( "X-Rock-Mobile-Api-Key" ) )
#endif
            {
                // Normal authorization failed, but this is a Mobile App request so check
                // if the application itself has been given permission.
#if REVIEW_NET5_0_OR_GREATER
                var appId = actionContext.HttpContext.Request.Headers["X-Rock-App-Id"].First().AsIntegerOrNull();
                var mobileApiKey = actionContext.HttpContext.Request.Headers["X-Rock-Mobile-Api-Key"].First();
#else
                var appId = actionContext.Request.Headers.GetValues( "X-Rock-App-Id" ).First().AsIntegerOrNull();
                var mobileApiKey = actionContext.Request.Headers.GetValues( "X-Rock-Mobile-Api-Key" ).First();
#endif

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

#if REVIEW_NET5_0_OR_GREATER
            if ( !authorized )
            {
                actionContext.Result = new Microsoft.AspNetCore.Mvc.ChallengeResult();
            }
            else
            {
                await next();
            }
#else
            if ( !authorized )
            {
                actionContext.Response = new HttpResponseMessage( HttpStatusCode.Unauthorized );
            }
#endif
        }
    }
}

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
            var reflectedHttpActionDescriptor = ( ReflectedHttpActionDescriptor ) actionContext.ActionDescriptor;

            var controller = actionContext.ActionDescriptor.ControllerDescriptor;
            string controllerClassName = controller.ControllerType.FullName;
            string actionMethod = actionContext.Request.Method.Method;

            var apiId = RestControllerService.GetApiId( reflectedHttpActionDescriptor.MethodInfo, actionMethod, controller.ControllerName );
            ISecured item = RestActionCache.Get( apiId );

            if ( item == null )
            {
                // if there isn't a RestAction in the database, use the Controller as the secured item
                item = RestControllerCache.Get( controllerClassName );
                if ( item == null )
                {
                    item = new RestController();
                }
            }

            Person person = null;

            if ( actionContext.Request.Properties.Keys.Contains( "Person" ) )
            {
                person = actionContext.Request.Properties["Person"] as Person;
            }
            else
            {
                var principal = actionContext.Request.GetUserPrincipal();
                if ( principal != null && principal.Identity != null )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        string userName = principal.Identity.Name;
                        UserLogin userLogin = null;
                        if ( userName.StartsWith( "rckipid=" ) )
                        {
                            Rock.Model.PersonService personService = new Model.PersonService( rockContext );
                            Rock.Model.Person impersonatedPerson = personService.GetByImpersonationToken( userName.Substring( 8 ) );
                            if ( impersonatedPerson != null )
                            {
                                userLogin = impersonatedPerson.GetImpersonatedUser();
                            }
                        }
                        else
                        {
                            var userLoginService = new Rock.Model.UserLoginService( rockContext );
                            userLogin = userLoginService.GetByUserName( userName );
                        }

                        if ( userLogin != null )
                        {
                            person = userLogin.Person;
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
                    }
                }
            }

            string action = actionMethod.Equals( "GET", StringComparison.OrdinalIgnoreCase ) ?
                Rock.Security.Authorization.VIEW : Rock.Security.Authorization.EDIT;
            if ( !item.IsAuthorized( action, person ) )
            {
                actionContext.Response = new HttpResponseMessage( HttpStatusCode.Unauthorized );
            }
        }
    }
}
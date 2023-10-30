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
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;

using Rock.Data;
using Rock.Model;
using Rock.Net;
using Rock.Rest.Utility;
using Rock.Web.Cache;

#if WEBFORMS
using System.Web.Http.Results;

using IActionResult = System.Web.Http.IHttpActionResult;
#endif

namespace Rock.Rest
{
    /*
     * NOTE: We could have inherited from System.Web.Http.OData.ODataController, but that changes
     * the response format from vanilla REST to OData format. That breaks existing Rock Rest clients.
     *
     */

    /// <summary>
    /// Base ApiController for Rock REST endpoints
    /// Supports ODataV3 Queries and ODataRouting
    /// </summary>
    /// <seealso cref="System.Web.Http.ApiController" />
    [ODataRouting]
    public class ApiControllerBase : ApiController
    {
        /// <summary>
        /// Gets the rock request context that describes the current request
        /// being made.
        /// </summary>
        /// <value>
        /// The rock request context that describes the current request
        /// being made.
        /// </value>
        public RockRequestContext RockRequestContext { get; private set; }

        /// <inheritdoc/>
        public override async Task<HttpResponseMessage> ExecuteAsync( HttpControllerContext controllerContext, CancellationToken cancellationToken )
        {
            var responseContext = new RockMessageResponseContext();
            RockRequestContext = new RockRequestContext( new HttpRequestMessageWrapper( controllerContext.Request ), responseContext );
            RockRequestContextAccessor.RequestContext = RockRequestContext;

            var responseMessage = await base.ExecuteAsync( controllerContext, cancellationToken );

            responseContext.Update( responseMessage );

            RockRequestContextAccessor.RequestContext = null;

            return responseMessage;
        }

        /// <summary>
        /// Gets the currently logged in Person
        /// </summary>
        /// <returns></returns>
        protected virtual Rock.Model.Person GetPerson()
        {
            return GetPerson( null );
        }

        /// <summary>
        /// Gets the currently logged in Person
        /// </summary>
        /// <param name="controller">The ApiController instance that is looking up the current person.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        internal static Rock.Model.Person GetPerson( ApiController controller, RockContext rockContext )
        {
            if ( controller.Request.Properties.Keys.Contains( "Person" ) )
            {
                return controller.Request.Properties["Person"] as Person;
            }

            var principal = controller.ControllerContext.Request.GetUserPrincipal();
            if ( principal != null && principal.Identity != null )
            {
                if ( principal.Identity.Name.StartsWith( "rckipid=" ) )
                {
                    var personService = new Model.PersonService( rockContext ?? new RockContext() );
                    Rock.Model.Person impersonatedPerson = personService.GetByImpersonationToken( principal.Identity.Name.Substring( 8 ), false, null );
                    if ( impersonatedPerson != null )
                    {
                        return impersonatedPerson;
                    }
                }
                else
                {
                    var userLoginService = new Rock.Model.UserLoginService( rockContext ?? new RockContext() );
                    var userLogin = userLoginService.GetByUserName( principal.Identity.Name );

                    if ( userLogin != null )
                    {
                        var person = userLogin.Person;
                        controller.Request.Properties.Add( "Person", person );
                        return userLogin.Person;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the currently logged in Person
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        protected virtual Rock.Model.Person GetPerson( RockContext rockContext )
        {
            return GetPerson( this, rockContext );
        }

        /// <summary>
        /// Gets the primary person alias of the currently logged in person
        /// </summary>
        /// <returns></returns>
        protected virtual Rock.Model.PersonAlias GetPersonAlias()
        {
            return GetPersonAlias( null );
        }

        /// <summary>
        /// Gets the primary person alias of the currently logged in person
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        protected virtual Rock.Model.PersonAlias GetPersonAlias( RockContext rockContext )
        {
            var person = GetPerson( rockContext );
            if ( person != null )
            {
                return person.PrimaryAlias;
            }

            return null;
        }

        /// <summary>
        /// Gets the primary person alias ID of the currently logged in person
        /// </summary>
        /// <returns></returns>
        protected virtual int? GetPersonAliasId()
        {
            return GetPersonAliasId( null );
        }

        /// <summary>
        /// Gets the primary person alias ID of the currently logged in person
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        protected virtual int? GetPersonAliasId( RockContext rockContext )
        {
            var currentPersonAlias = GetPersonAlias( rockContext );
            return currentPersonAlias == null ? ( int? ) null : currentPersonAlias.Id;
        }

        /// <summary>
        /// Determines whether the current person has unrestricted view access
        /// to the current controller action.
        /// </summary>
        /// <returns><c>true</c> if the current person has unrestricted view access; otherwise, <c>false</c>.</returns>
        protected bool IsCurrentPersonUnrestrictedView()
        {
            if ( ActionContext.ActionDescriptor is ReflectedHttpActionDescriptor actionDescriptor )
            {
                var restGuid = actionDescriptor.MethodInfo.GetCustomAttribute<SystemGuid.RestActionGuidAttribute>()?.Guid;

                if ( restGuid.HasValue )
                {
                    var restAction = RestActionCache.Get( restGuid.Value );

                    return restAction?.IsAuthorized( "UnrestrictedView", RockRequestContext.CurrentPerson ) ?? false;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether the current person has unrestricted edit access
        /// to the current controller action.
        /// </summary>
        /// <returns><c>true</c> if the current person has unrestricted view access; otherwise, <c>false</c>.</returns>
        protected bool IsCurrentPersonUnrestrictedEdit()
        {
            if ( ActionContext.ActionDescriptor is ReflectedHttpActionDescriptor actionDescriptor )
            {
                var restGuid = actionDescriptor.MethodInfo.GetCustomAttribute<SystemGuid.RestActionGuidAttribute>()?.Guid;

                if ( restGuid.HasValue )
                {
                    var restAction = RestActionCache.Get( restGuid.Value );

                    return restAction?.IsAuthorized( "UnrestrictedEdit", RockRequestContext.CurrentPerson ) ?? false;
                }
            }

            return false;
        }

        #region .NET Core compatible methods

        /// <summary>
        /// Returns a response object to indicate that no content was generated.
        /// </summary>
        /// <returns>The response object.</returns>
        protected IActionResult NoContent()
        {
            return new StatusCodeResult( HttpStatusCode.NoContent, this );
        }

        /// <summary>
        /// Creates a not found response with the given error message.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>The response object.</returns>
        protected IActionResult NotFound( string errorMessage )
        {
            var error = new HttpError( errorMessage );

            return new NegotiatedContentResult<HttpError>( HttpStatusCode.NotFound, error, this );
        }

        /// <summary>
        /// Creates an unauthorized response with the given error message.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>The response object.</returns>
        protected IActionResult Unauthorized( string errorMessage )
        {
            var error = new HttpError( errorMessage );

            return new NegotiatedContentResult<HttpError>( HttpStatusCode.Unauthorized, error, this );
        }

        /// <summary>
        /// Creates an internals erver error response with the given error message.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>The response object.</returns>
        protected IActionResult InternalServerError( string errorMessage )
        {
            var error = new HttpError( errorMessage );

            return new NegotiatedContentResult<HttpError>( HttpStatusCode.InternalServerError, error, this );
        }

        #endregion
    }
}

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
using System.Net.Http;
using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;

using Microsoft.Extensions.DependencyInjection;

using Rock.Data;
using Rock.Model;
using Rock.Net;

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
        public override Task<HttpResponseMessage> ExecuteAsync( HttpControllerContext controllerContext, CancellationToken cancellationToken )
        {
            if ( controllerContext.Request.Properties["RockServiceProvider"] is IServiceProvider serviceProvider )
            {
                RockRequestContext = serviceProvider.GetRequiredService<IRockRequestContextAccessor>().RockRequestContext;
            }

            return base.ExecuteAsync( controllerContext, cancellationToken );
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
    }
}

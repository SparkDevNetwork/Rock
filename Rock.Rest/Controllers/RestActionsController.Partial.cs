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
using System.Net;
using System.Web.Http;

using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// RestActions REST API
    /// </summary>
    public partial class RestActionsController
    {
        /// <inheritdoc/>
        protected override void CheckCanEdit( RestAction entity )
        {
            base.CheckCanEdit( entity );

            if ( entity == null )
            {
                throw new HttpResponseException( HttpStatusCode.BadRequest );
            }

            RequireAdministrateOnController( entity.ControllerId );
        }

        /// <inheritdoc/>
        protected override void CheckCanEditAfterUpdate( RestAction entity )
        {
            base.CheckCanEditAfterUpdate( entity );

            RequireAdministrateOnController( entity.ControllerId );
        }

        /// <summary>
        /// Checks that the current person can administrate the REST controller that owns the action.
        /// </summary>
        /// <param name="controllerId">The REST controller identifier.</param>
        /// <exception cref="HttpResponseException"></exception>
        private void RequireAdministrateOnController( int controllerId )
        {
            /*
                10/16/2026 - MSE

                REST security for an endpoint is read from its RestAction row and the
                RestController it belongs to. Creating, moving or deleting a RestAction changes
                which rules secure that endpoint, for example moving a finance action under a
                controller that grants access to All Users. Staff hold EDIT on every controller
                through the seeded entity type rule, so require ADMINISTRATE on the current
                controller and on any new controller.

                Reason: REST security records are administrator settings.
            */
            var controller = RestControllerCache.Get( controllerId );
            if ( controller == null )
            {
                throw new HttpResponseException( HttpStatusCode.BadRequest );
            }

            if ( !controller.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, GetPerson() ) )
            {
                throw new HttpResponseException( HttpStatusCode.Unauthorized );
            }
        }
    }
}

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

using Rock.Communication.Chat.Platform.Session;
using Rock.Configuration;
using Rock.Mobile;
using Rock.Rest.Filters;
using Rock.Security;
using Rock.ViewModels.Blocks.Communication.Chat.ChatShell;
using Rock.ViewModels.Rest.Chat;

#if WEBFORMS
using HttpPostAttribute = System.Web.Http.HttpPostAttribute;
using IActionResult = System.Web.Http.IHttpActionResult;
using RouteAttribute = System.Web.Http.RouteAttribute;
#endif

namespace Rock.Rest.v2
{
    /// <summary>
    /// The routes the Rock Mobile app uses for chat. They do not depend on any page or block, so a
    /// notification tap, a background token refresh or a sign-out works with no chat page open.
    /// </summary>
    public sealed partial class ChatController
    {
        /// <summary>
        /// Runs the chat gates for the person signed in to the app, enrols them on their first
        /// open, and answers what the web's chat block is given plus the first church token.
        /// </summary>
        /// <returns>The session, the church's name and campuses, and the first token when the gates passed.</returns>
        [HttpPost]
        [Route( "mobile/session" )]
        [Authenticate]
        [ExcludeSecurityActions( Security.Authorization.EXECUTE_READ, Security.Authorization.EXECUTE_WRITE, Security.Authorization.EXECUTE_UNRESTRICTED_READ, Security.Authorization.EXECUTE_UNRESTRICTED_WRITE )]
        [ProducesResponse( HttpStatusCode.OK, Type = typeof( ChatMobileSessionBag ) )]
        [ProducesResponse( HttpStatusCode.Unauthorized )]
        [ProducesResponse( HttpStatusCode.Forbidden )]
        [SystemGuid.RestActionGuid( "2BD9EF73-7AC4-4029-835F-1E1CA14434B2" )]
        public IActionResult MobileSession()
        {
            var refusal = CheckMobileRequest();
            if ( refusal != HttpStatusCode.OK )
            {
                return StatusCode( refusal );
            }

            using ( var rockContext = RockApp.Current.CreateRockContext() )
            {
                return Ok( ChatSessionHelper.OpenMobileSession( GetPerson( rockContext ), rockContext ) );
            }
        }

        /// <summary>
        /// Signs a fresh church token for the person signed in to the app, after running every
        /// chat gate again, so a person who lost access since chat opened gets no new token.
        /// </summary>
        /// <returns>The token and its expiry, or the gate that refused it.</returns>
        [HttpPost]
        [Route( "mobile/token" )]
        [Authenticate]
        [ExcludeSecurityActions( Security.Authorization.EXECUTE_READ, Security.Authorization.EXECUTE_WRITE, Security.Authorization.EXECUTE_UNRESTRICTED_READ, Security.Authorization.EXECUTE_UNRESTRICTED_WRITE )]
        [ProducesResponse( HttpStatusCode.OK, Type = typeof( ChatChurchTokenBag ) )]
        [ProducesResponse( HttpStatusCode.Unauthorized )]
        [ProducesResponse( HttpStatusCode.Forbidden )]
        [SystemGuid.RestActionGuid( "CAE2AD74-6AA4-4DF5-A3C5-57888F8B217B" )]
        public IActionResult MobileToken()
        {
            var refusal = CheckMobileRequest();
            if ( refusal != HttpStatusCode.OK )
            {
                return StatusCode( refusal );
            }

            using ( var rockContext = RockApp.Current.CreateRockContext() )
            {
                return Ok( ChatSessionHelper.MintToken( GetPerson( rockContext ), rockContext ) );
            }
        }

        /// <summary>
        /// Reads the facts the mobile request guard decides on from this request.
        /// </summary>
        /// <returns><see cref="HttpStatusCode.OK"/> when the request may proceed, else the status to refuse it with.</returns>
        private HttpStatusCode CheckMobileRequest()
        {
            // An Origin header that is present but empty still came from a browser.
            var origin = Request.Headers.TryGetValues( "Origin", out var origins ) ? origins.FirstOrDefault() ?? string.Empty : null;
            var isMobileSite = MobileHelper.GetCurrentApplicationSite()?.SiteType == Model.SiteType.Mobile;

            return ChatSessionHelper.CheckMobileRequest(
                Request.Method.Method,
                origin,
                Request.Content?.Headers?.ContentType?.ToString(),
                isMobileSite,
                RequestContext.Principal );
        }
    }
}

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
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using Rock.Communication.Chat;
using Rock.Communication.Chat.Sync;
using Rock.Rest.Filters;
using Rock.Security;
using Rock.Transactions;

#if WEBFORMS
using HttpPostAttribute = System.Web.Http.HttpPostAttribute;
using IActionResult = System.Web.Http.IHttpActionResult;
using RouteAttribute = System.Web.Http.RouteAttribute;
using RoutePrefixAttribute = System.Web.Http.RoutePrefixAttribute;
#endif

namespace Rock.Rest.v2
{
    /// <summary>
    /// Provides API interfaces for the Chat system in Rock.
    /// </summary>
    /// <seealso cref="ApiControllerBase"/>
    [RoutePrefix( "api/v2/chat" )]
    [Rock.SystemGuid.RestControllerGuid( "56049595-5BE3-4B70-8FE5-C8E1D53976F0" )]
    public sealed class ChatController : ApiControllerBase
    {
        /// <summary>
        /// Receives webhook requests from an external chat provider.
        /// </summary>
        /// <returns>A status code indicating the result of the request.</returns>
        [HttpPost]
        [Route( "Webhook" )]
        [Secured( Security.Authorization.EXECUTE_UNRESTRICTED_WRITE )]
        [ExcludeSecurityActions( Security.Authorization.EXECUTE_READ, Security.Authorization.EXECUTE_UNRESTRICTED_READ, Security.Authorization.EXECUTE_WRITE )]
        [ProducesResponseType( HttpStatusCode.OK )]
        [SystemGuid.RestActionGuid( "7D1B2D15-E2F5-4159-AD84-AE7D9262750D" )]
        public async Task<IActionResult> Webhook()
        {
            using ( var chatHelper = new ChatHelper() )
            {
                var webhookValidationResult = await chatHelper.ValidateWebhookRequestAsync( Request );
                if ( webhookValidationResult?.IsValid == true )
                {
                    new HandleChatWebhookRequestTransaction( new ChatWebhookRequest( webhookValidationResult.RequestBody ) ).Enqueue();
                }

                // If invalid, the chat helper will have already logged the result to Rock Logs.
            }

            return Ok();
        }
    }
}

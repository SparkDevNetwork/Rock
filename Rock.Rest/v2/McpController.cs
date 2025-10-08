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
using System.Net.Http.Headers;

using Microsoft.AspNetCore.Mvc;

using Rock.Enums.AI.Agent;
using Rock.Data;
using Rock.Rest.Filters;
using Rock.Security;
using Rock.Web.Cache.Entities;
using Rock.AI.Agent.Mcp;
using Rock.AI.Agent;

using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;

#if WEBFORMS
using IActionResult = System.Web.Http.IHttpActionResult;
using RoutePrefixAttribute = System.Web.Http.RoutePrefixAttribute;
using RouteAttribute = System.Web.Http.RouteAttribute;
using HttpPostAttribute = System.Web.Http.HttpPostAttribute;
#endif

namespace Rock.Rest.v2
{
    /// <summary>
    /// Provides API endpoints for MCP AI services.
    /// </summary>
    [RoutePrefix( "api/v2/mcp" )]
    [SystemGuid.RestControllerGuid( "0a73df31-46d0-41e2-a0f6-0a762b97fd07" )]
    internal class McpController : ApiControllerBase
    {
        private readonly IMcpServer _mcpServer;

        private readonly IChatAgentBuilder _agentBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="McpController"/> class.
        /// </summary>
        /// <param name="mcpServer">The server instance used to manage AI agent actions.</param>
        /// <param name="agentBuilder">The factory that will build our agent.</param>
        public McpController( IMcpServer mcpServer, IChatAgentBuilder agentBuilder )
        {
            _mcpServer = mcpServer ?? throw new ArgumentNullException( nameof( mcpServer ) );
            _agentBuilder = agentBuilder ?? throw new ArgumentNullException( nameof( agentBuilder ) );
        }

        /// <summary>
        /// Executes an MCP request for the agent.
        /// </summary>
        /// <param name="slug">The slug of the agent for this MCP request.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the request.</param>
        /// <returns>The response data from the MCP request.</returns>
        [HttpPost]
        [Route( "{slug}" )]
        [Authenticate]
        [Secured( Security.Authorization.EXECUTE_READ )]
        [ExcludeSecurityActions( Security.Authorization.EXECUTE_WRITE, Security.Authorization.EXECUTE_UNRESTRICTED_WRITE )]
        [ProducesResponseType( HttpStatusCode.OK )]
        [ProducesResponseType( HttpStatusCode.Accepted )]
        [ProducesResponseType( HttpStatusCode.BadRequest )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( "2c6194af-095a-42fa-9288-27e8b3494231" )]
        public async Task<IActionResult> PostMcp( string slug, CancellationToken cancellationToken )
        {
            using ( var rockContext = new RockContext() )
            {
                var agentCache = AIAgentCache.All()
                    .FirstOrDefault( a => a.AgentType == AgentType.Mcp
                        && a.GetAdditionalSettings<McpAgentSettings>().Slug == slug );

                if ( agentCache == null )
                {
                    return NotFound( "The AI Agent was not found." );
                }

                var isAuthorized = IsCurrentPersonAuthorized( Security.Authorization.EXECUTE_UNRESTRICTED_READ )
                    || agentCache.IsAuthorized( Security.Authorization.VIEW, RockRequestContext.CurrentPerson );

                if ( !isAuthorized )
                {
                    return Unauthorized( "You are not authorized to view this data view." );
                }

                var agent = _agentBuilder.Build( agentCache.Id );

                using ( var contentStream = await Request.Content.ReadAsStreamAsync() )
                {
                    var mcpRequest = new McpRequest
                    {
                        Content = contentStream
                    };

                    var mcpResponse = await _mcpServer.HandleRequestAsync( agent, mcpRequest, cancellationToken );

                    if ( mcpResponse.Content == null )
                    {
                        return StatusCode( HttpStatusCode.Accepted );
                    }

                    var result = new HttpResponseMessage( HttpStatusCode.OK )
                    {
                        Content = new StreamContent( mcpResponse.Content )
                    };

                    result.Content.Headers.ContentType = new MediaTypeHeaderValue( "application/json" );

                    return ResponseMessage( result );
                }
            }
        }
    }
}

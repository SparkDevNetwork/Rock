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
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.SemanticKernel;

using Rock.AI.Agent.Mcp.Protocol;

namespace Rock.AI.Agent.Mcp
{
    /// <summary>
    /// Implementation of an MCP Server that processes requests from MCP clients
    /// and maps those requests to the appropriate functions in the ChatAgent.
    /// </summary>
    internal class McpServer : IMcpServer
    {
        /// <inheritdoc/>
        public async Task<McpResponse> HandleRequestAsync( IChatAgent agent, McpRequest request, CancellationToken cancellationToken )
        {
            if ( !( agent is ChatAgent chatAgent ) )
            {
                throw new ArgumentOutOfRangeException( nameof( agent ), $"Parameter must be of type {typeof( ChatAgent ).FullName}." );
            }

            var rpcRequest = new JsonRpcRequest( request.Content );
            var response = await HandleRequestAsync( chatAgent, rpcRequest, cancellationToken );

            if ( !rpcRequest.Id.HasValue )
            {
                return new McpResponse();
            }

            var ms = new MemoryStream();

            response.ToJson( ms );

            ms.Position = 0;

            return new McpResponse
            {
                Content = ms
            };
        }

        /// <summary>
        /// Handles the request by processing the method specified in the request.
        /// </summary>
        /// <param name="agent">The agent that will handle tool related requests.</param>
        /// <param name="request">The object that represents the request from the client.</param>
        /// <param name="cancellationToken">A token that indicates if the request should be cancelled.</param>
        /// <returns>The response to the request.</returns>
        private async Task<JsonRpcResult> HandleRequestAsync( ChatAgent agent, JsonRpcRequest request, CancellationToken cancellationToken )
        {
            if ( request.Method == "initialize" )
            {
                return ProcessInitialize( request );
            }
            else if ( request.Method == "tools/list" )
            {
                return ProcessToolsList( request, agent );
            }
            else if ( request.Method == "tools/call" )
            {
                return await ProcessToolsCallAsync( request, agent, cancellationToken );
            }
            else if ( request.Method.StartsWith( "notifications/" ) )
            {
                // Indicate no response should be sent.
                return null;
            }
            else
            {
                return request.CreateErrorResult( JsonRpcErrorCode.MethodNotFound, $"Method '{request.Method}' not found." );
            }
        }

        /// <summary>
        /// Processes an "initialize" JSON-RPC request and generates a response
        /// containing server capabilities and information.
        /// </summary>
        /// <param name="rpcRequest">The JSON-RPC request to process.</param>
        /// <returns>A <see cref="JsonRpcResult"/> containing the server's protocol version, capabilities, and server information.
        private JsonRpcResult ProcessInitialize( JsonRpcRequest rpcRequest )
        {
            var parameters = rpcRequest.GetParameters<InitializeParameters>();

            if ( parameters == null )
            {
                return rpcRequest.CreateErrorResult( JsonRpcErrorCode.InvalidParams, "Missing or invalid request parameters." );
            }

            var response = new InitializeResult
            {
                ProtocolVersion = "2025-06-18",
                Capabilities = new Dictionary<string, Capability>
                {
                    { "tools", new Capability() },
                },
                ServerInfo = new Implementation
                {
                    Name = "Rock RMS",
                    Version = VersionInfo.VersionInfo.GetRockSemanticVersionNumber(),
                },
            };

            return rpcRequest.CreateResult( response );
        }

        /// <summary>
        /// Processes a request to retrieve a list of available tools and their metadata.
        /// </summary>
        /// <param name="rpcRequest">The JSON-RPC request to process.</param>
        /// <param name="agent">The chat agent providing access to the kernel and plugins.</param>
        /// <returns>A <see cref="JsonRpcResult"/> containing a list of tools and their metadata.</returns>
        private JsonRpcResult ProcessToolsList( JsonRpcRequest rpcRequest, ChatAgent agent )
        {
            var parameters = rpcRequest.GetParameters<ListToolsParameters>();

            if ( parameters == null )
            {
                return rpcRequest.CreateErrorResult( JsonRpcErrorCode.InvalidParams, "Missing or invalid request parameters." );
            }

            var kernel = agent.Kernel;
            var tools = new List<Tool>();

            foreach ( var plugin in kernel.Plugins )
            {
                foreach ( var function in plugin )
                {
                    var tool = new Tool
                    {
                        Name = $"{plugin.Name}__{function.Name}",
                        Title = function.Name.SplitCase(),
                        Description = function.Description,
                        InputSchema = function.JsonSchema
                    };

                    tools.Add( tool );
                }
            }

            var response = new ListToolsResult
            {
                Tools = tools
            };

            return rpcRequest.CreateResult( response );
        }

        /// <summary>
        /// Processes a tools call request by invoking the specified tool function and returning the result.
        /// </summary>
        /// <param name="rpcRequest">The JSON-RPC request containing the tool call details.</param>
        /// <param name="agent">The chat agent responsible for providing the kernel for tool invocation.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="JsonRpcResult"/> containing the result of the tool function invocation.</returns>
        private async Task<JsonRpcResult> ProcessToolsCallAsync( JsonRpcRequest rpcRequest, ChatAgent agent, CancellationToken cancellationToken )
        {
            var parameters = rpcRequest.GetParameters<CallToolParameters>();

            if ( parameters == null )
            {
                return rpcRequest.CreateErrorResult( JsonRpcErrorCode.InvalidParams, "Missing or invalid request parameters." );
            }

            var functionNameComponents = parameters.Name.Split( new string[] { "__" }, 2, StringSplitOptions.RemoveEmptyEntries );

            if ( functionNameComponents.Length != 2 )
            {
                return rpcRequest.CreateErrorResult( JsonRpcErrorCode.InvalidParams, "Tool name was not valid." );
            }

            var pluginName = functionNameComponents[0];
            var functionName = functionNameComponents[1];

            var kernel = agent.Kernel;
            KernelArguments args;

            try
            {
                args = JsonSerializer.Deserialize<KernelArguments>( parameters.Arguments.GetRawText() );
            }
            catch
            {
                return rpcRequest.CreateErrorResult( JsonRpcErrorCode.InvalidParams, "Invalid tool call arguments." );
            }

            var result = await kernel.InvokeAsync<object>( pluginName, functionName, args, cancellationToken );
            var response = new CallToolResult();

            if ( result is string )
            {
                response.Content.Add( new Protocol.TextContent
                {
                    Text = result.ToString()
                } );
            }
            else if ( result != null )
            {
                response.Content.Add( new Protocol.TextContent
                {
                    Text = JsonSerializer.Serialize( result, JsonRpcRequest.JsonOptions )
                } );

                response.StructuredContent = result;
            }

            return rpcRequest.CreateResult( response );
        }
    }
}

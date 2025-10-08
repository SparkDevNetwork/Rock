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

using System.Threading;
using System.Threading.Tasks;

using Rock.Attribute;

namespace Rock.AI.Agent.Mcp
{
    /// <summary>
    /// <para>
    /// A server that handles requests from an MCP client.
    /// </para>
    /// <para>
    /// This interface should only be implemented by the core Rock framework.
    /// It is subject to adding new methods and properties without warning and
    /// could cause implementations provided by plugins to break.
    /// </para>
    /// </summary>
    [RockInternal( "18.0" )]
    internal interface IMcpServer
    {
        /// <summary>
        /// Handles a request to the MCP server. This method is responsible for
        /// parsing the request body and executing the appropriate action based
        /// on the request type.
        /// </summary>
        /// <param name="agent">The agent that will be used to process any tool calls.</param>
        /// <param name="request">The details about the request from the MCP client.</param>
        /// <param name="cancellationToken">A token that indicates if the request should be cancelled.</param>
        /// <returns>The content to return to the MCP client. If null or empty then a 202 response should be returned instead.</returns>
        Task<McpResponse> HandleRequestAsync( IChatAgent agent, McpRequest request, CancellationToken cancellationToken = default );
    }
}

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

using System.IO;

namespace Rock.AI.Agent.Mcp
{
    /// <summary>
    /// The details of a response to the MCP server.
    /// </summary>
    internal class McpResponse
    {
        /// <summary>
        /// The content to return to the MCP client. If <c>null</c> then no
        /// response should be returned.
        /// </summary>
        public Stream Content { get; set; }
    }
}

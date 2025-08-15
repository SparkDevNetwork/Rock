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

using System.Collections.Generic;

namespace Rock.AI.Agent.Mcp.Protocol
{
    /// <summary>
    /// Represents the result from an MCP server when initializing a connection.
    /// </summary>
    internal class InitializeResult
    {
        /// <summary>
        /// The capabilities of the MCP server.
        /// </summary>
        public Dictionary<string, Capability> Capabilities { get; set; }

        /// <summary>
        /// The information about the server that is responding to the request.
        /// </summary>
        public Implementation ServerInfo { get; set; }

        /// <summary>
        /// The instructions for the client to understand how to interact with
        /// the server. These are typically passed to the language model.
        /// </summary>
        public string Instructions { get; set; }

        /// <summary>
        /// The version of the Model Context Protocol that the server wants to use.
        /// </summary>
        public string ProtocolVersion { get; set; }
    }
}

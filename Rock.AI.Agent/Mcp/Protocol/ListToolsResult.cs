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
    /// The server’s response to a tools/list request from the client.
    /// </summary>
    internal class ListToolsResult
    {
        /// <summary>
        /// The list of tools available on the server.
        /// </summary>
        public List<Tool> Tools { get; set; }

        /// <summary>
        /// An opaque token representing the pagination position after the last
        /// returned result. If present, there may be more results available.
        /// </summary>
        public string NextCursor { get; set; }
    }
}

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

using System.Text.Json;

namespace Rock.AI.Agent.Mcp.Protocol
{
    /// <summary>
    /// Used by the client to invoke a tool provided by the server.
    /// </summary>
    internal class CallToolParameters
    {
        /// <summary>
        /// The name of the tool to call.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The arguments to pass to the tool function.
        /// </summary>
        public JsonElement Arguments { get; set; }
    }
}

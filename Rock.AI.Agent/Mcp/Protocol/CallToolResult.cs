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
    /// The server’s response to a tool call.
    /// </summary>
    internal class CallToolResult
    {
        /// <summary>
        /// A list of content objects that represent the unstructured result
        /// of the tool call.
        /// </summary>
        public IList<TextContent> Content { get; set; } = new List<TextContent>();

        /// <summary>
        /// An optional JSON object that represents the structured result
        /// of the tool call.
        /// </summary>
        public object StructuredContent { get; set; }
    }
}

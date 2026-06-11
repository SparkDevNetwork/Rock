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
using Rock.Enums.AI.Agent;

namespace Rock.ViewModels.Blocks.Core.McpServerList
{
    /// <summary>
    /// Represents 
    /// </summary>
    /// <remarks>This class is typically used to transfer server metadata between components or services. It
    /// encapsulates both public and private connection information, allowing consumers to distinguish between endpoints
    /// intended for different audiences.</remarks>
    public class McpServerListItemBag
    {
        /// <summary>
        /// Gets or sets the type of audience for this MCP server.
        /// </summary>
        public AudienceType AudienceType { get; set; }

        /// <summary>
        /// Gets or sets the friendly name of the MCP server.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the description of the MCP server.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the safe MCP server URL for public display.
        /// </summary>
        public string PartialUrl { get; set; }

        /// <summary>
        /// Gets or sets the private MCP server URL.
        /// </summary>
        public string FullUrl { get; set; }
    }
}

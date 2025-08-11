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

namespace Rock.AI.Agent.Mcp.Protocol
{
    /// <summary>
    /// Represents the information of a client or server.
    /// </summary>
    internal class Implementation
    {
        /// <summary>
        /// The short name that identifies the device.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A friendly name that identifies the device.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The version number of the device.
        /// </summary>
        public string Version { get; set; }
    }
}
